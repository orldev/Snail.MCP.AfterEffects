using System.Formats.Tar;
using System.IO.Compression;
using System.Security.Cryptography;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Storage;

/// <summary>A file under a path of an area, relative to that path, with its size and its modification time in Unix seconds.</summary>
internal sealed record VolumeFile(string Path, long Size, long Modified);

/// <summary>What a path of an area holds: one file, or a folder and every file under it.</summary>
internal sealed record VolumeItem(bool IsFolder, IReadOnlyList<VolumeFile> Files, long Bytes, bool IsTruncated);

/// <summary>A path of an area looked up: what it holds, or why it could not be read.</summary>
internal sealed record VolumeLookup(VolumeItem? Item, BridgeError? Error);

/// <summary>One row of a folder listing: a file or a folder, its size, and why it cannot be deleted now when it cannot.</summary>
internal sealed record VolumeEntry(string Name, bool IsFolder, long Bytes, int Files, DateTimeOffset Modified, string? State = null, string? Detail = null, string? InUse = null);

/// <summary>A folder of an area listed, or why it could not be.</summary>
internal sealed record VolumeListing(string Directory, IReadOnlyList<VolumeEntry> Entries, bool IsTruncated, BridgeError? Error);

/// <summary>Where the bytes of one request land in a file sent in several: at an offset, and whether this request ends the file, checked
/// against the digest the client computed of the whole.</summary>
internal sealed record UploadPart(long Offset, bool IsLast, string? Sha256);

/// <summary>One file that landed, with the digest both ends agreed on.</summary>
internal sealed record Transferred(string From, string To, long Size, string Sha256);

/// <summary>Files written into an area, or what was received so far of one sent in parts, and the error that stopped the writing when one did.</summary>
internal sealed record VolumeWrite(IReadOnlyList<Transferred> Files, BridgeError? Error, long Received = 0)
{
    public bool IsOk => Error is null;
}

/// <summary>What a deletion freed, or why it was refused.</summary>
internal sealed record VolumeDelete(long Bytes, BridgeError? Error)
{
    public bool IsOk => Error is null;
}

/// <summary>This server's storage as the HTTP server and the pages reach it: the areas measured, listed, read, written and deleted on this machine's disk.</summary>
/// <remarks>The server runs beside After Effects, so the areas are folders here and bytes move straight between the request and the disk. A
/// file sent in parts gathers under a partial name and takes its own only once its digest matches, so an interrupted upload never leaves a
/// truncated file behind.</remarks>
internal sealed class Volume(ServerConfig config, RenderFarm farm)
{
    /// <summary>Files one listing or one archive carries; past that a folder is answered in part and refused as one archive.</summary>
    public const int MostFiles = 20_000;

    private const string PartialSuffix = ".part";

    /// <summary>The folder an area is, on this machine.</summary>
    public string RootOf(string area) => area switch
    {
        VolumeAreas.Files => config.ProjectDirectory ?? Path.Combine(config.DataDirectory, "files"),
        VolumeAreas.Captures => Path.Combine(config.ProjectDirectory ?? config.DataDirectory, "captures"),
        VolumeAreas.Renders => Path.Combine(config.ProjectDirectory ?? config.DataDirectory, "renders"),
        VolumeAreas.Jobs => farm.LogDirectory,
        _ => throw new ArgumentOutOfRangeException(nameof(area), area, "no such area"),
    };

    /// <summary>Every area with its size, and the disk the data directory is on.</summary>
    public JsonObject Measure()
    {
        var areas = new JsonArray();

        foreach (var area in VolumeAreas.All)
        {
            var root = RootOf(area);
            var files = Directory.Exists(root) ? Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Select(file => new FileInfo(file).Length).ToList() : [];

            areas.Add(new JsonObject { ["area"] = area, ["directory"] = root, ["bytes"] = files.Sum(), ["files"] = files.Count });
        }

        var disk = new JsonObject();

        try
        {
            var drive = new DriveInfo(Path.GetPathRoot(Path.GetFullPath(config.DataDirectory)) ?? config.DataDirectory);
            disk["free_bytes"] = drive.AvailableFreeSpace;
            disk["total_bytes"] = drive.TotalSize;
        }
        catch (Exception failure) when (failure is IOException or UnauthorizedAccessException or ArgumentException)
        {
        }

        return new JsonObject { ["areas"] = areas, ["disk"] = disk };
    }

    /// <summary>The entries of a folder, folders first; in the jobs area each log carries the state of its job.</summary>
    public VolumeListing List(string area, string path)
    {
        var relative = AreaPaths.Normal(path);
        var folder = Full(area, relative);

        if (!Directory.Exists(folder))
        {
            return new VolumeListing(folder, [], false, Directory.Exists(RootOf(area)) || relative.Length > 0 ? NotFound(area, relative) : null);
        }

        var entries = new List<VolumeEntry>();
        var truncated = false;

        foreach (var child in new DirectoryInfo(folder).EnumerateFileSystemInfos().OrderByDescending(info => info is DirectoryInfo).ThenBy(info => info.Name, StringComparer.OrdinalIgnoreCase))
        {
            if (entries.Count >= 2000)
            {
                truncated = true;

                break;
            }

            if (child.Name.EndsWith(PartialSuffix, StringComparison.Ordinal) || child.Name.StartsWith(".snail-", StringComparison.Ordinal))
            {
                continue;
            }

            entries.Add(child is DirectoryInfo directory ? FolderEntry(directory) : FileEntry(area, (FileInfo)child));
        }

        return new VolumeListing(folder, entries, truncated, null);
    }

    /// <summary>What a path holds: a file, or a folder with every file under it.</summary>
    public VolumeLookup Find(string area, string path)
    {
        var relative = AreaPaths.Normal(path);
        var full = Full(area, relative);

        if (File.Exists(full))
        {
            var info = new FileInfo(full);

            return new VolumeLookup(new VolumeItem(false, [new VolumeFile(info.Name, info.Length, Seconds(info.LastWriteTimeUtc))], info.Length, false), null);
        }

        if (!Directory.Exists(full))
        {
            return new VolumeLookup(null, NotFound(area, relative));
        }

        var files = new List<VolumeFile>();
        var truncated = false;
        long bytes = 0;

        foreach (var file in Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            if (files.Count >= MostFiles)
            {
                truncated = true;

                break;
            }

            var info = new FileInfo(file);

            if (info.Name.EndsWith(PartialSuffix, StringComparison.Ordinal))
            {
                continue;
            }

            files.Add(new VolumeFile(Path.GetRelativePath(full, file).Replace(Path.DirectorySeparatorChar, '/'), info.Length, Seconds(info.LastWriteTimeUtc)));
            bytes += info.Length;
        }

        return new VolumeLookup(new VolumeItem(true, files, bytes, truncated), null);
    }

    public Stream OpenRead(string area, string path) =>
        new FileStream(Full(area, AreaPaths.Normal(path)), FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true);

    /// <summary>A folder as a tar of every file under it, written as it is read.</summary>
    public async Task WriteTarAsync(string area, string path, VolumeItem folder, Stream destination, CancellationToken cancellationToken)
    {
        var root = Full(area, AreaPaths.Normal(path));
        await using var tar = new TarWriter(destination, TarEntryFormat.Pax, leaveOpen: true);

        foreach (var file in folder.Files)
        {
            await tar.WriteEntryAsync(Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar)), file.Path, cancellationToken).ConfigureAwait(false);
        }
    }

    /// <summary>A folder as a zip of every file under it, for a browser that opens archives with a double click.</summary>
    public async Task WriteZipAsync(string area, string path, VolumeItem folder, Stream destination, CancellationToken cancellationToken)
    {
        var root = Full(area, AreaPaths.Normal(path));
        using var archive = new ZipArchive(destination, ZipArchiveMode.Create, leaveOpen: true);

        foreach (var file in folder.Files)
        {
            await using var content = new FileStream(Path.Combine(root, file.Path.Replace('/', Path.DirectorySeparatorChar)), FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true);
            await using var target = archive.CreateEntry(file.Path, CompressionLevel.Fastest).Open();

            await content.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
        }
    }

    public Task<VolumeWrite> WriteAsync(string area, string path, Stream source, bool overwrite, CancellationToken cancellationToken) =>
        WritePartAsync(area, path, source, new UploadPart(0, IsLast: true, Sha256: null), overwrite, cancellationToken);

    /// <summary>One request of a file sent in several: its bytes continue the partial file at the part's offset, and the last part gives the
    /// file its name once the digest matches, the client's when it sent one and the one computed here otherwise.</summary>
    public async Task<VolumeWrite> WritePartAsync(string area, string path, Stream source, UploadPart part, bool overwrite, CancellationToken cancellationToken)
    {
        var relative = AreaPaths.Normal(path);

        if (relative.Length == 0 || !AreaPaths.IsConfined(relative))
        {
            return new VolumeWrite([], new BridgeError("BadRequest", $"'{relative}' is not a file path inside the {area} area"));
        }

        var target = Full(area, relative);

        if (Directory.Exists(target))
        {
            return new VolumeWrite([], new BridgeError("Exists", $"'{relative}' is a folder"));
        }

        if (File.Exists(target) && !overwrite)
        {
            return new VolumeWrite([], new BridgeError("Exists", $"'{relative}' already exists; pass overwrite to replace it", new JsonObject { ["path"] = relative }));
        }

        Directory.CreateDirectory(Path.GetDirectoryName(target)!);
        var partial = $"{target}{PartialSuffix}";

        if (part.Offset == 0 && File.Exists(partial))
        {
            File.Delete(partial);
        }

        if (part.Offset > 0 && (!File.Exists(partial) || new FileInfo(partial).Length != part.Offset))
        {
            var received = File.Exists(partial) ? new FileInfo(partial).Length : 0;

            return new VolumeWrite([], new BridgeError("BadRequest", $"'{relative}' has {received} bytes so far, not {part.Offset}; send the parts in order from the start", new JsonObject { ["received"] = received }));
        }

        long written;

        await using (var output = new FileStream(partial, FileMode.Append, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
        {
            await source.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
            written = output.Length;
        }

        if (!part.IsLast)
        {
            return new VolumeWrite([], null, written);
        }

        var digest = await DigestAsync(partial, cancellationToken).ConfigureAwait(false);

        if (part.Sha256 is { } expected && !string.Equals(expected, digest, StringComparison.OrdinalIgnoreCase))
        {
            File.Delete(partial);

            return new VolumeWrite([], new BridgeError("Corrupt", $"'{relative}' arrived different from what was sent", new JsonObject { ["expected"] = expected, ["received"] = digest }));
        }

        File.Move(partial, target, overwrite: true);

        return new VolumeWrite([new Transferred(relative, target, written, digest)], null, written);
    }

    /// <summary>A tar unpacked into a folder of an area; entries that climb out are refused, files that exist are kept unless overwriting.</summary>
    /// <remarks>An archive that ends in the middle of a file leaves nothing of it: a tar reader hands over what it has without a word, so an
    /// entry shorter than its header promises is refused and its half of a file removed, rather than published under the name of a whole one.
    /// The partial of a paged write is a different thing and is kept on purpose, for the next part to continue.</remarks>
    public async Task<VolumeWrite> UnpackTarAsync(string area, string path, Stream tar, bool overwrite, CancellationToken cancellationToken)
    {
        var relative = AreaPaths.Normal(path);
        var root = Full(area, relative);
        var landed = new List<Transferred>();

        Directory.CreateDirectory(root);
        await using var reader = new TarReader(tar, leaveOpen: true);

        while (await reader.GetNextEntryAsync(cancellationToken: cancellationToken).ConfigureAwait(false) is { } entry)
        {
            var name = AreaPaths.Normal(entry.Name.StartsWith("./", StringComparison.Ordinal) ? entry.Name[2..] : entry.Name);

            if (name.Length == 0 || entry.EntryType is TarEntryType.Directory)
            {
                continue;
            }

            if (!AreaPaths.IsConfined(name))
            {
                return new VolumeWrite(landed, new BridgeError("BadRequest", $"the archive holds '{entry.Name}', which would leave the folder"));
            }

            if (entry.EntryType is not (TarEntryType.RegularFile or TarEntryType.V7RegularFile) || entry.DataStream is null)
            {
                continue;
            }

            var target = Path.Combine(root, name.Replace('/', Path.DirectorySeparatorChar));

            if (File.Exists(target) && !overwrite)
            {
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(target)!);
            var partial = $"{target}{PartialSuffix}";

            long written;

            try
            {
                await using (var output = new FileStream(partial, FileMode.Create, FileAccess.Write, FileShare.None, 1 << 16, useAsync: true))
                {
                    await entry.DataStream.CopyToAsync(output, cancellationToken).ConfigureAwait(false);
                    written = output.Length;
                }
            }
            catch
            {
                Forget(partial);

                throw;
            }

            if (written != entry.Length)
            {
                Forget(partial);

                return new VolumeWrite(landed, new BridgeError("BadRequest", $"the archive ends inside '{entry.Name}', which arrived as {written} of {entry.Length} bytes"));
            }

            var digest = await DigestAsync(partial, cancellationToken).ConfigureAwait(false);
            File.Move(partial, target, overwrite: true);

            landed.Add(new Transferred(entry.Name, target, written, digest));
        }

        return new VolumeWrite(landed, null, landed.Sum(file => file.Size));
    }

    /// <summary>A tar that arrived in parts into a staging file, unpacked into its folder and then deleted, whether the unpacking succeeded or not.</summary>
    public async Task<VolumeWrite> UnpackStagedTarAsync(string area, string path, string staged, bool overwrite, CancellationToken cancellationToken)
    {
        var stagedFile = Full(area, AreaPaths.Normal(staged));

        try
        {
            await using var tar = new FileStream(stagedFile, FileMode.Open, FileAccess.Read, FileShare.None, 1 << 16, useAsync: true);

            return await UnpackTarAsync(area, path, tar, overwrite, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (File.Exists(stagedFile))
            {
                File.Delete(stagedFile);
            }
        }
    }

    /// <summary>Where a tar sent in parts gathers before it is unpacked: beside its folder, named after the folder and whoever sends it.</summary>
    public static string StagingPath(string relative, string? who)
    {
        var sender = new string([.. (who ?? "anyone").Where(char.IsLetterOrDigit)]);
        var folder = AreaPaths.Normal(relative);
        var tag = Convert.ToHexStringLower(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(folder)))[..12];

        return $".snail-upload-{sender}-{tag}.tar";
    }

    /// <summary>Removes a file or a folder of an area; the area itself and the log of a running job are refused.</summary>
    public VolumeDelete Delete(string area, string path)
    {
        var relative = AreaPaths.Normal(path);

        if (relative.Length == 0 || !AreaPaths.IsConfined(relative))
        {
            return new VolumeDelete(0, new BridgeError("BadRequest", $"name a file or folder inside the {area} area to delete, not the area itself"));
        }

        if (area == VolumeAreas.Jobs && JobOf(relative) is { IsActive: true } job)
        {
            return new VolumeDelete(0, new BridgeError("InUse", $"job {job.Id} is {job.State}; cancel it before deleting its log"));
        }

        var full = Full(area, relative);

        if (File.Exists(full))
        {
            var bytes = new FileInfo(full).Length;
            File.Delete(full);

            return new VolumeDelete(bytes, null);
        }

        if (!Directory.Exists(full))
        {
            return new VolumeDelete(0, NotFound(area, relative));
        }

        var freed = Directory.EnumerateFiles(full, "*", SearchOption.AllDirectories).Sum(file => new FileInfo(file).Length);
        Directory.Delete(full, recursive: true);

        return new VolumeDelete(freed, null);
    }

    /// <summary>Removes a file the caller no longer wants, and says nothing when it is already gone or held.</summary>
    private static void Forget(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    /// <summary>The full path of something in an area, refused when it would leave it.</summary>
    /// <remarks>The separator is part of the comparison: without it a folder whose name merely begins with the area's — <c>files-of-someone-else</c>
    /// beside <c>files</c> — would pass for one inside it. The callers check the relative path as well; this is what holds when one of them forgets.</remarks>
    private string Full(string area, string relative)
    {
        var root = Path.TrimEndingDirectorySeparator(Path.GetFullPath(RootOf(area)));
        var full = Path.GetFullPath(Path.Combine(root, relative.Replace('/', Path.DirectorySeparatorChar)));
        var inside = full.Length == root.Length
            ? string.Equals(full, root, StringComparison.Ordinal)
            : full.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.Ordinal);

        return inside ? full : throw new UnauthorizedAccessException($"'{relative}' leaves the {area} area");
    }

    private VolumeEntry FolderEntry(DirectoryInfo directory)
    {
        var files = directory.EnumerateFiles("*", SearchOption.AllDirectories).Take(MostFiles).ToList();

        return new VolumeEntry(directory.Name, true, files.Sum(file => file.Length), files.Count, directory.LastWriteTimeUtc);
    }

    private VolumeEntry FileEntry(string area, FileInfo file)
    {
        if (area != VolumeAreas.Jobs || JobOf(file.Name) is not { } job)
        {
            return new VolumeEntry(file.Name, false, file.Length, 1, file.LastWriteTimeUtc);
        }

        var progress = job.Describe(0)["progress"]!;

        return new VolumeEntry(file.Name, false, file.Length, 1, file.LastWriteTimeUtc, job.State,
            $"{progress["frames_rendered"]} of {progress["frames_total"]?.ToString() ?? "?"} frames", job.IsActive ? job.State : null);
    }

    private RenderJob? JobOf(string name) => farm.Find(Path.GetFileNameWithoutExtension(name));

    private static BridgeError NotFound(string area, string relative) =>
        new("NotFound", $"nothing at '{relative}' in the {area} area", new JsonObject { ["area"] = area, ["path"] = relative });

    private static long Seconds(DateTime moment) => new DateTimeOffset(moment, TimeSpan.Zero).ToUnixTimeSeconds();

    private static async Task<string> DigestAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 1 << 16, useAsync: true);

        return Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, cancellationToken).ConfigureAwait(false));
    }
}
