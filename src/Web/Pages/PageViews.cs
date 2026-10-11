using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Web.Pages;

/// <summary>What the pages show, read out of the server's own reports.</summary>
internal static class PageViews
{
    public static IReadOnlyList<Section> Sections { get; } =
    [
        new("Status", "/"),
        new("Files", $"/{VolumeAreas.Files}"),
        new("Captures", $"/{VolumeAreas.Captures}"),
        new("Renders", $"/{VolumeAreas.Renders}"),
        new("Jobs", $"/{VolumeAreas.Jobs}"),
    ];

    private static readonly HashSet<string> Images = new(StringComparer.OrdinalIgnoreCase) { ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp", ".svg" };

    private static readonly HashSet<string> Texts = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".log", ".json", ".jsx", ".js", ".md", ".csv", ".xml", ".yaml", ".yml", ".ini", ".cfg", ".srt", ".aepx",
    };

    public const long LargestTextPreview = 256 * 1024;

    public static string TitleOf(string area) => Sections.First(section => section.Href == $"/{area}").Label;

    public static string Page(string area, string path) => path.Length == 0 ? $"/{area}" : $"/{area}/{Escape(path)}";

    public static string Raw(string area, string path) => $"{FileLinks.RawPrefix}/{area}/{Escape(path)}";

    public static string Join(string folder, string name) => folder.Length == 0 ? name : $"{folder}/{name}";

    private static string Escape(string path) => string.Join('/', path.Split('/').Select(Uri.EscapeDataString));

    public static bool IsImage(string name) => Images.Contains(Path.GetExtension(name));

    public static bool IsText(string name) => Texts.Contains(Path.GetExtension(name));

    public static IReadOnlyList<Crumb> Crumbs(string area, string path)
    {
        var crumbs = new List<Crumb> { new(area, Page(area, string.Empty)) };
        var walked = string.Empty;

        foreach (var segment in path.Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            walked = Join(walked, segment);
            crumbs.Add(new Crumb(segment, Page(area, walked)));
        }

        return crumbs;
    }

    public static BrowseView Browse(string area, string path, VolumeListing listing, Flash? flash)
    {
        var entries = listing.Entries.Select(entry => Entry(area, path, entry)).ToList();

        return new BrowseView
        {
            Area = area,
            Title = TitleOf(area),
            Path = path,
            Crumbs = Crumbs(area, path),
            Entries = entries,
            IsTruncated = listing.IsTruncated,
            ZipHref = entries.Count == 0 ? null : $"{Raw(area, path)}?format={FileLinks.ZipFormat}",
            Flash = flash,
            CanPrune = path.Length == 0 && area == VolumeAreas.Jobs && entries.Any(entry => entry.State is not null && !entry.IsRunning),
        };
    }

    public static BrowseView Unavailable(string area, string path, BridgeError error, Flash? flash) => new()
    {
        Area = area,
        Title = TitleOf(area),
        Path = path,
        Crumbs = Crumbs(area, path),
        Flash = flash,
        Problem = error.Message,
    };

    public static FileView File(string area, string path, long bytes, string? text) => new()
    {
        Area = area,
        Title = TitleOf(area),
        Name = path.Split('/')[^1],
        Folder = path.Contains('/') ? path[..path.LastIndexOf('/')] : string.Empty,
        Crumbs = Crumbs(area, path),
        Bytes = bytes,
        RawHref = Raw(area, path),
        DownloadHref = $"{Raw(area, path)}?download=true",
        IsImage = IsImage(path),
        Text = text,
    };

    public static Flash? FlashOf(int? deleted, long? freed, string? cancelled, string? problem) =>
        problem is not null ? new Flash(problem, IsError: true)
        : deleted is { } count ? new Flash(Messages.DeletedFromThePage(count, Readable.Size(freed ?? 0)), IsError: false)
        : cancelled is not null ? new Flash(Messages.CancelledFromThePage(cancelled), IsError: false)
        : null;

    public static IReadOnlyList<StatusGroup> Status(JsonObject report, JsonObject storage, IReadOnlyList<RenderJob> jobs, RenderFarm farm, ServerConfig config, TimeSpan running)
    {
        var afterEffects = report["afterEffects"] as JsonObject ?? [];
        var extension = report["extension"] as JsonObject ?? [];
        var bridge = report["bridge"] as JsonObject ?? [];
        var isReachable = afterEffects["reachable"]?.GetValue<bool>() == true;

        IReadOnlyList<StatusGroup> groups =
        [
            new StatusGroup("After Effects",
            [
                isReachable
                    ? new StatusRow("After Effects", $"answers · {afterEffects["app"]?.ToString() ?? afterEffects["version"]?.ToString() ?? "connected"}")
                    : new StatusRow("After Effects", $"does not answer · {afterEffects["error"]?.ToString() ?? "no bridge connected"}", IsProblem: true),
                ExtensionRow(extension),
                new StatusRow("Scripting", afterEffects["scripting_allowed"]?.GetValue<bool>() == false ? "scripts may not write files or reach the network" : "allowed", IsProblem: afterEffects["scripting_allowed"]?.GetValue<bool>() == false),
                new StatusRow("Bridge port", bridge["port"]?.ToString() ?? "none"),
            ]),
            new StatusGroup("Farm",
            [
                new StatusRow("aerender", farm.Executable ?? "not found beside any After Effects", IsProblem: farm.Executable is null),
                new StatusRow("Render jobs", JobCounts(jobs)),
                new StatusRow("At once", config.FarmParallel.ToString()),
            ]),
            new StatusGroup("Storage", [.. StorageRows(storage)]),
            new StatusGroup("Server",
            [
                new StatusRow("Version", report["server"]?["version"]?.GetValue<string>() ?? string.Empty),
                new StatusRow("Address", config.Http.PublicUrl ?? config.Http.Url),
                new StatusRow("Running for", Readable.Duration(running)),
                new StatusRow("File links", $"valid for {config.Http.LinkMinutes} min"),
            ]),
        ];

        return groups;
    }

    private static EntryView Entry(string area, string folder, VolumeEntry entry)
    {
        var path = Join(folder, entry.Name);

        return new EntryView
        {
            Name = entry.Name,
            Href = Page(area, path),
            DownloadHref = entry.IsFolder ? $"{Raw(area, path)}?format={FileLinks.ZipFormat}" : $"{Raw(area, path)}?download=true",
            IsFolder = entry.IsFolder,
            Bytes = entry.Bytes,
            Modified = entry.Modified,
            State = entry.State,
            Detail = entry.Detail,
            InUse = entry.InUse,
            JobId = area == VolumeAreas.Jobs ? Path.GetFileNameWithoutExtension(entry.Name) : null,
        };
    }

    private static StatusRow ExtensionRow(JsonObject extension) => extension["status"]?.GetValue<string>() switch
    {
        "current" => new StatusRow("Bridge", $"the build this server ships · {extension["installed"]?["digest"]}"),
        "differs" => new StatusRow("Bridge", $"differs from this server's · installed {extension["installed"]?["digest"]}, shipped {extension["bundled"]?["digest"]}", IsProblem: true),
        "older" => new StatusRow("Bridge", $"older than this server · installed {extension["installed"]?["digest"]}", IsProblem: true),
        _ => new StatusRow("Bridge", "unknown until After Effects connects", IsProblem: true),
    };

    private static string JobCounts(IReadOnlyList<RenderJob> jobs)
    {
        var states = jobs
            .GroupBy(job => job.State, StringComparer.Ordinal)
            .OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => $"{group.Count()} {group.Key}")
            .ToList();

        return states.Count == 0 ? "none" : string.Join(" · ", states);
    }

    private static IEnumerable<StatusRow> StorageRows(JsonObject storage)
    {
        foreach (var area in (storage["areas"] as JsonArray ?? []).OfType<JsonObject>())
        {
            yield return new StatusRow(area["area"]?.GetValue<string>() ?? string.Empty, $"{Readable.Size(area["bytes"]?.GetValue<long>() ?? 0)} · {area["files"]} files · {area["directory"]}");
        }

        var free = storage["disk"]?["free_bytes"]?.GetValue<long>() ?? 0;
        var total = storage["disk"]?["total_bytes"]?.GetValue<long>() ?? 0;

        if (total > 0)
        {
            yield return new StatusRow("Disk", $"{Readable.Size(free)} free of {Readable.Size(total)}", IsProblem: free < total / 20);
        }
    }
}
