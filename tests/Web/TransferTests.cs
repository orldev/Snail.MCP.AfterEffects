using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Protocol;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Tools.Transfer;

namespace Snail.MCP.AfterEffects.Tests.Web;

/// <summary>The files of the areas as a client sees them: the listing, and the commands a client on another machine is told
/// to run to put a file there or fetch one back.</summary>
/// <remarks>The server is the HTTP one, built in this process, because the commands only exist over HTTP: beside After
/// Effects the client and the server share a disk and the tools say so instead.</remarks>
public sealed class TransferTests
{
    private const string Token = "transfer-operator-token";

    /// <summary>What is in an area, with a signed link per entry when one is asked for; a path leading out of the area and
    /// an area that does not exist are both refused.</summary>
    [Fact]
    public async Task Files_ListsAnArea_AndRefusesWhatIsNotOne()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var tools = Tools(server);
        var volume = server.Services.GetRequiredService<Volume>();
        Directory.CreateDirectory(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes"));
        await File.WriteAllTextAsync(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes", "hello.txt"), "hello");

        var listed = Data(tools.Files(withLinks: true));

        Assert.Equal(VolumeAreas.Files, listed["area"]!.ToString());
        Assert.Contains("notes", listed.ToJsonString(), StringComparison.Ordinal);

        Assert.True(Failed(tools.Files(area: "nowhere")));
        Assert.True(Failed(tools.Files(path: "../..")));
    }

    /// <summary>An upload answers with the command the client runs and where the file will land; a path that climbs out of
    /// the area is refused before any of that.</summary>
    [Fact]
    public async Task Upload_TellsTheClientWhatToRun()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var tools = Tools(server);

        var told = Data(tools.Upload("/tmp/plate.mov", "footage/plate.mov"));

        Assert.Contains("curl", told["run"]!.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Contains("plate.mov", told["lands"]!.ToString(), StringComparison.Ordinal);

        Assert.True(Failed(tools.Upload("/tmp/plate.mov", "../plate.mov")));
    }

    /// <summary>A download names the file it brings and the command that brings it; a file nobody wrote is not offered.</summary>
    [Fact]
    public async Task Download_TellsTheClientWhatToRun_ForAFileThatIsThere()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var tools = Tools(server);
        var volume = server.Services.GetRequiredService<Volume>();
        Directory.CreateDirectory(volume.RootOf(VolumeAreas.Files));
        await File.WriteAllTextAsync(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes.txt"), "hello");

        var told = Data(tools.Download("notes.txt"));

        Assert.Contains("curl", told["run"]!.ToString(), StringComparison.OrdinalIgnoreCase);

        Assert.True(Failed(tools.Download("nothing.txt")));
        Assert.True(Failed(tools.Download("notes.txt", area: "nowhere")));
    }

    private static JsonObject Data(CallToolResult result) => result.Data();

    private static bool Failed(CallToolResult result) => result.Failed();

    private static TransferTools Tools(HttpServerInProcess server) =>
        ActivatorUtilities.CreateInstance<TransferTools>(server.Services);
}
