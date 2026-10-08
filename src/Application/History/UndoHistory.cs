using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.History;

/// <summary>Undo and redo in After Effects, run as a script of their own through the host rather than over the bridge.</summary>
/// <remarks>After Effects' Undo command started from the bridge's scheduled task never returns: it opens a nested event loop that waits for
/// the task to end, and the task is waiting for it. Started as a top-level script — through AppleScript on macOS, AfterFX -r on Windows —
/// the same command takes milliseconds. The script writes nothing back, so the answer says the command was asked for, and ae_project_info
/// or ae_list_layers show what it did.</remarks>
internal sealed class UndoHistory(ServerConfig config, IAfterEffectsHost host)
{
    public const int UndoCommandId = 16;

    public const int RedoCommandId = 17;

    public Task<JsonObject> UndoAsync(int steps, CancellationToken cancellationToken) => RunAsync("undo", UndoCommandId, steps, cancellationToken);

    public Task<JsonObject> RedoAsync(int steps, CancellationToken cancellationToken) => RunAsync("redo", RedoCommandId, steps, cancellationToken);

    private async Task<JsonObject> RunAsync(string what, int commandId, int steps, CancellationToken cancellationToken)
    {
        var count = Math.Clamp(steps, 1, 100);

        if (!host.IsRunning)
        {
            return new JsonObject { ["asked"] = false, ["error"] = "After Effects is not running." };
        }

        var script = Path.Combine(config.DataDirectory, "state", $"{what}.jsx");
        Directory.CreateDirectory(Path.GetDirectoryName(script)!);
        await File.WriteAllTextAsync(script, $"for (var i = 0; i < {count}; i++) {{ app.executeCommand({commandId}); }}\n", cancellationToken).ConfigureAwait(false);

        var run = await host.RunScriptAsync(script, cancellationToken).ConfigureAwait(false);

        return new JsonObject { ["asked"] = run.Succeeded, ["steps"] = count, ["detail"] = run.Detail };
    }
}
