using Snail.MCP.AfterEffects.Application.History;
using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class UndoHistoryTests : IDisposable
{
    private readonly string _root = TestFolder.Create();

    /// <summary>Undo goes to After Effects as a script of its own, with the command id that is the same in every language, once per step.</summary>
    [Fact]
    public async Task Undo_HandsAfterEffectsAScript_WithOneCommandPerStep()
    {
        var host = new FakeAfterEffectsHost([], isRunning: true);

        var report = await History(host).UndoAsync(3, CancellationToken.None);

        Assert.True(report["asked"]!.GetValue<bool>());
        Assert.Equal(3, report["steps"]!.GetValue<int>());
        Assert.Contains($"i < 3", Assert.Single(host.ScriptsRun), StringComparison.Ordinal);
        Assert.Contains($"app.executeCommand({UndoHistory.UndoCommandId})", host.ScriptsRun[0], StringComparison.Ordinal);
    }

    [Fact]
    public async Task Redo_UsesTheRedoCommand()
    {
        var host = new FakeAfterEffectsHost([], isRunning: true);

        await History(host).RedoAsync(1, CancellationToken.None);

        Assert.Contains($"app.executeCommand({UndoHistory.RedoCommandId})", Assert.Single(host.ScriptsRun), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Undo_WithoutARunningAfterEffects_SaysSo()
    {
        var report = await History(new FakeAfterEffectsHost([], isRunning: false)).UndoAsync(1, CancellationToken.None);

        Assert.False(report["asked"]!.GetValue<bool>());
    }

    private UndoHistory History(IAfterEffectsHost host)
    {
        var config = new ServerConfig { DataDirectory = _root };
        ServerConfigExtensions.Normalize(config);

        return new UndoHistory(config, host);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
