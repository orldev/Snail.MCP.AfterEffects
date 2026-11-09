using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.Sdk.Mcp.Hosting.Sessions;
using Snail.Sdk.Mcp.Programs;

namespace Snail.MCP.AfterEffects.Adapters.Programs;

/// <summary>The bridge's commands as a program calls them: <c>ae.layer_add({ kind: "text" })</c> for every command the bridge registers.</summary>
/// <remarks>
/// <para>
/// Snail.Sdk runs the program and asks the session's gate before every command; no command is named after a tool here, so the gate is
/// asked the command's own name, which <see cref="Application.Access.ScopeGate"/> judges as the command it is. What is left here is what
/// only this server knows: the script command is put to the server's scripting setting first — without this, a server with scripts
/// switched off ran them by way of one line of JavaScript — and a sequence is a list of commands, each held to the same two rules before
/// any of them is sent, as run_tools holds its steps.
/// </para>
/// <para>
/// A command waits for After Effects as long as its program has left (<see cref="CommandCall.Remaining"/>), and the bridge inside After
/// Effects is told the same deadline: a command that is not told the time left would give up after the bridge's default of seconds, and
/// one told the longest a program may run kept After Effects at work after its program had stopped.
/// </para>
/// </remarks>
internal sealed class BridgeProgramCommands(IAfterEffectsBridge bridge, ServerConfig config) : IProgramCommands
{
    private static readonly Dictionary<string, BridgeCommand> Catalog = BridgeCommands.All.ToDictionary(command => command.Name, StringComparer.Ordinal);

    public IReadOnlyList<string> Names { get; } = [.. BridgeCommands.All.Select(command => command.Name)];

    /// <summary>No command stands for a tool: the gate judges each by its own name, as the command it is, whichever tool may also send it.</summary>
    public string? ToolOf(string command) => null;

    /// <summary>A command of a program's run, refused by the session of its request and waiting no longer than the program has left.</summary>
    public async Task<Answer<JsonNode?>> CallAsync(CommandCall call, CancellationToken cancellationToken)
    {
        Func<string, Answer<JsonNode?>?>? refusal = call.Request is { } request ? step => ToolForwarding.Refused<JsonNode?>(request, step) : null;

        if (Refused(call.Command, call.Arguments, refusal) is { } refused)
        {
            return refused;
        }

        var reply = await bridge.SendAsync(Catalog[call.Command], call.Arguments, call.Remaining, cancellationToken).ConfigureAwait(false);

        return reply.IsOk
            ? Answer.Ok<JsonNode?>(reply.Result ?? new JsonObject())
            : Answer.Failed(reply.Error!.Message, Messages.HintFor(reply.Error), reply.Error.Type, reply.Error.Details?.DeepClone()).As<JsonNode?>();
    }

    /// <summary>Why the command may not be sent, or null when it may: the script command while scripts are off, and for a sequence the first
    /// step that is either that or a command the session may not send.</summary>
    private Answer<JsonNode?>? Refused(string command, JsonObject arguments, Func<string, Answer<JsonNode?>?>? refusal)
    {
        if (IsScript(command))
        {
            return Scripting(Messages.ProgramScriptDisabled);
        }

        if (command != BridgeCommands.Sequence.Name)
        {
            return null;
        }

        foreach (var step in arguments["steps"] as JsonArray ?? [])
        {
            if (step?["command"]?.ToString() is not { } named)
            {
                continue;
            }

            if (IsScript(named))
            {
                return Scripting(Messages.SequenceScriptDisabled);
            }

            if (refusal?.Invoke(named) is { } refused)
            {
                return refused;
            }
        }

        return null;
    }

    private bool IsScript(string command) => command == BridgeCommands.Script.Name && config.Scripting is ScriptAccess.Off;

    private static Answer<JsonNode?> Scripting(string error) => Answer.Failed(error, Messages.ScriptingDisabledHint, FailureTypes.ScriptingDisabled);
}
