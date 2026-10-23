using System.Formats.Tar;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class VolumeTests : IDisposable
{
    private readonly string _root = TestFolder.Create();
    private readonly Volume _volume;

    public VolumeTests()
    {
        var config = new ServerConfig { DataDirectory = _root, Transport = Transport.Http };
        ServerConfigExtensions.Normalize(config);
        var clients = new ClientSessions([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);
        var farm = new RenderFarm(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0), executable: null), config, clients, TimeProvider.System, NullLogger<RenderFarm>.Instance);
        _volume = new Volume(config, farm);
    }

    /// <summary>A file sent in parts gathers under a partial name and takes its own only when the last part's digest matches.</summary>
    [Fact]
    public async Task WritePart_GathersParts_AndKeepsTheFileOnlyWhenTheDigestMatches()
    {
        var bytes = Encoding.UTF8.GetBytes("hello, after effects");
        var digest = Convert.ToHexStringLower(SHA256.HashData(bytes));

        var first = await _volume.WritePartAsync(VolumeAreas.Files, "shots/a.txt", new MemoryStream(bytes[..5]), new UploadPart(0, false, null), false, CancellationToken.None);
        var listedWhilePartial = _volume.List(VolumeAreas.Files, "shots");
        var last = await _volume.WritePartAsync(VolumeAreas.Files, "shots/a.txt", new MemoryStream(bytes[5..]), new UploadPart(5, true, digest), false, CancellationToken.None);
        var again = await _volume.WriteAsync(VolumeAreas.Files, "shots/a.txt", new MemoryStream(bytes), false, CancellationToken.None);

        Assert.True(first.IsOk);
        Assert.Equal(5, first.Received);
        Assert.Empty(listedWhilePartial.Entries);
        Assert.True(last.IsOk);
        Assert.Equal(digest, Assert.Single(last.Files).Sha256);
        Assert.Equal("hello, after effects", await File.ReadAllTextAsync(Path.Combine(_volume.RootOf(VolumeAreas.Files), "shots", "a.txt")));
        Assert.Equal("Exists", again.Error!.Type);
    }

    [Fact]
    public async Task WritePart_WithAWrongDigest_DropsTheFile()
    {
        var written = await _volume.WritePartAsync(VolumeAreas.Files, "b.txt", new MemoryStream([1, 2, 3]), new UploadPart(0, true, new string('0', 64)), false, CancellationToken.None);

        Assert.Equal("Corrupt", written.Error!.Type);
        Assert.False(File.Exists(Path.Combine(_volume.RootOf(VolumeAreas.Files), "b.txt")));
        Assert.False(File.Exists(Path.Combine(_volume.RootOf(VolumeAreas.Files), "b.txt.part")));
    }

    /// <summary>A tar unpacks into its folder, and an entry that would climb out stops the unpacking.</summary>
    [Fact]
    public async Task UnpackTar_LandsTheFiles_AndRefusesAnEntryThatClimbsOut()
    {
        var good = await TarOf(("one.txt", "1"), ("sub/two.txt", "22"));
        var bad = await TarOf(("../escape.txt", "x"));

        var unpacked = await _volume.UnpackTarAsync(VolumeAreas.Files, "incoming", good, false, CancellationToken.None);
        var refused = await _volume.UnpackTarAsync(VolumeAreas.Files, "incoming", bad, false, CancellationToken.None);
        var found = _volume.Find(VolumeAreas.Files, "incoming");

        Assert.True(unpacked.IsOk);
        Assert.Equal(2, unpacked.Files.Count);
        Assert.Equal("BadRequest", refused.Error!.Type);
        Assert.True(found.Item!.IsFolder);
        Assert.Equal(["one.txt", "sub/two.txt"], found.Item.Files.Select(file => file.Path).Order(StringComparer.Ordinal));
        Assert.False(File.Exists(Path.Combine(_volume.RootOf(VolumeAreas.Files), "escape.txt")));
    }

    [Fact]
    public async Task Delete_FreesAFileOrAFolder_AndRefusesTheAreaItself()
    {
        await _volume.WriteAsync(VolumeAreas.Files, "gone/c.txt", new MemoryStream(new byte[10]), false, CancellationToken.None);

        var folder = _volume.Delete(VolumeAreas.Files, "gone");
        var area = _volume.Delete(VolumeAreas.Files, "");
        var missing = _volume.Delete(VolumeAreas.Files, "nothing");

        Assert.Equal(10, folder.Bytes);
        Assert.Equal("BadRequest", area.Error!.Type);
        Assert.Equal("NotFound", missing.Error!.Type);
    }

    [Theory]
    [InlineData("a/b.txt", true)]
    [InlineData("", true)]
    [InlineData("../x", false)]
    [InlineData("a/../../x", false)]
    [InlineData("/etc/passwd", false)]
    [InlineData("C:/windows", false)]
    public void AreaPaths_ConfineWhatStaysInside(string path, bool confined) => Assert.Equal(confined, AreaPaths.IsConfined(path));

    private static async Task<Stream> TarOf(params (string Name, string Text)[] files)
    {
        var stream = new MemoryStream();

        await using (var writer = new TarWriter(stream, TarEntryFormat.Pax, leaveOpen: true))
        {
            foreach (var (name, text) in files)
            {
                var entry = new PaxTarEntry(TarEntryType.RegularFile, name) { DataStream = new MemoryStream(Encoding.UTF8.GetBytes(text)) };
                await writer.WriteEntryAsync(entry);
            }
        }

        stream.Position = 0;

        return stream;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
