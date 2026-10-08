using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.History;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>After Effects itself: undo and redo, its menu commands, many commands in one call, and raw ExtendScript for what no tool reaches.</summary>
[McpServerToolType]
internal sealed class ApplicationTools(IAfterEffectsBridge bridge, ServerConfig config, UndoHistory history, ToolScopes scopes, ClientSessions clients) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_undo", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Application.Undo)]
    public async Task<CallToolResult> UndoAsync(
        [Description("How many steps to undo.")] int steps = 1,
        CancellationToken cancellationToken = default) =>
        Outcome(await history.UndoAsync(steps, cancellationToken).ConfigureAwait(false));

    [McpServerTool(Name = "ae_redo", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Application.Redo)]
    public async Task<CallToolResult> RedoAsync(
        [Description("How many steps to redo.")] int steps = 1,
        CancellationToken cancellationToken = default) =>
        Outcome(await history.RedoAsync(steps, cancellationToken).ConfigureAwait(false));

    private static CallToolResult Outcome(JsonObject report) =>
        report["asked"]?.GetValue<bool>() == true
            ? ToolResponse.Success(report)
            : ToolResponse.Failure(report["error"]?.ToString() ?? Messages.UndoNotAsked, Messages.UndoNotAskedHint);

    [McpServerTool(Name = "ae_run_menu_command", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Application.MenuCommand)]
    [Sends("menu_command")]
    public Task<CallToolResult> RunMenuCommandAsync(
        [Description("The command as the menu shows it, in After Effects' own language.")] string? name = null,
        [Description("The command's numeric id, which is the same in every language.")] int? id = null,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 60,
        CancellationToken cancellationToken = default) =>
        WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.MenuCommand, new JsonObject().With("name", name).With("id", id), cancellationToken, timeout));

    [McpServerTool(Name = "run_tools", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Application.RunTools)]
    [Sends("script")]
    [Sends("sequence")]
    public Task<CallToolResult> RunToolsAsync(
        [Description(ToolDescriptions.Parameters.Steps)] JsonArray steps,
        [Description("Stop at the first step that fails; false runs every step and reports each.")] bool stopOnError = true,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 120,
        CancellationToken cancellationToken = default)
    {
        if (config.Scripting is ScriptAccess.Off && steps.Any(step => step?["command"]?.ToString() == BridgeCommands.Script.Name))
        {
            return Task.FromResult(ToolResponse.Failure(Messages.SequenceScriptDisabled, Messages.ScriptingDisabledHint));
        }

        if (scopes.Refused(steps, clients.Current) is { } denied)
        {
            return Task.FromResult(ToolResponse.Failure(ToolScopes.Refusal(denied.Command, denied.Scope, clients.Current.Name), Messages.ScopeHint));
        }

        return WithinAsync(timeoutSeconds, timeout =>
            SendAsync(BridgeCommands.Sequence, new JsonObject().With("steps", steps).With("stop_on_error", stopOnError), cancellationToken, timeout));
    }

    [McpServerTool(Name = "ae_script", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Application.Script)]
    [Sends("script")]
    public Task<CallToolResult> ScriptAsync(
        [Description("ExtendScript to run inside After Effects; the value of its last expression comes back as text.")] string code,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 60,
        CancellationToken cancellationToken = default)
    {
        if (config.Scripting is ScriptAccess.Off)
        {
            return Task.FromResult(ToolResponse.Failure(Messages.ScriptingDisabled, Messages.ScriptingDisabledHint));
        }

        return WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.Script, new JsonObject().With("code", code), cancellationToken, timeout));
    }
}
