using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Sessions;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>Which kind of work a call is, so a client's key can be asked about it before After Effects is.</summary>
/// <remarks>Read from what the work already declares, the schema for the command a tool sends and the annotations for a tool that sends none,
/// rather than from a list kept beside them: a new tool would be missing from a list, and a tool missing from a list is a tool nothing guards.
/// Scripts and menu commands come first because both reach past every schema; rendering next, because it spends the machine. run_tools and
/// run_program are carriers: what they do is whatever is inside them, so each command they carry is asked about on its own.</remarks>
internal sealed class ToolScopes(CommandSemantics semantics, ToolIndex index)
{
    private static readonly string[] Scripting = ["script", "menu_command", "run_menu_command", "system_call"];

    private static readonly string[] Rendering = ["render", "render_start", "render_in_media_encoder", "render_in_ame", "farm_submit", "farm_retry", "watch_folder", "lease", "stop_worker"];

    private static readonly string[] Carriers = ["run_tools", "run_program"];

    /// <summary>Tools that destroy only when asked to: an upload replaces files with overwrite and otherwise only adds them.</summary>
    private static readonly string[] DestructiveOnlyWhenAsked = ["upload"];

    /// <summary>The scope a call of this tool needs, when the tool is the whole of the work.</summary>
    public string For(McpServerTool tool)
    {
        var name = tool.ProtocolTool.Name;
        var short_ = name.StartsWith(ToolIndex.Prefix, StringComparison.Ordinal) ? name[ToolIndex.Prefix.Length..] : name;

        if (Reaching(short_) is { } reaching)
        {
            return reaching;
        }

        if (Carriers.Contains(short_, StringComparer.Ordinal))
        {
            return Scopes.Read;
        }

        if (DestructiveOnlyWhenAsked.Contains(short_, StringComparer.Ordinal))
        {
            return Scopes.Write;
        }

        if (index.Named(name)?.Command is { } command && Known(command) is { } known)
        {
            return known;
        }

        if (tool.ProtocolTool.Annotations?.DestructiveHint is true)
        {
            return Scopes.Delete;
        }

        return tool.ProtocolTool.Annotations?.ReadOnlyHint is true ? Scopes.Read : Scopes.Write;
    }

    /// <summary>The scope one bridge command needs, for a tool that carries commands rather than being one.</summary>
    public string For(string command) => Known(command) ?? Scopes.Write;

    /// <summary>The first command in this list the client's key does not open, with what it would have needed; null when it opens them all.</summary>
    /// <remarks>Every step is asked about before any of them is sent: a sequence half refused is a composition half built.</remarks>
    public (string Command, string Scope)? Refused(JsonArray? commands, ClientSession client)
    {
        foreach (var step in commands?.OfType<JsonObject>() ?? [])
        {
            if (step["command"]?.GetValue<string>() is not { } command)
            {
                continue;
            }

            if (For(command) is { } scope && !client.May(scope))
            {
                return (command, scope);
            }
        }

        return null;
    }

    /// <summary>What to tell a client whose key does not open this kind of work.</summary>
    public static string Refusal(string what, string scope, string client) =>
        $"'{what}' is {scope} work, and the key '{client}' presented does not open it; ask the operator for a key with the {scope} scope.";

    private static string? Reaching(string command) =>
        Scripting.Contains(command, StringComparer.Ordinal) ? Scopes.Script
            : Rendering.Contains(command, StringComparer.Ordinal) ? Scopes.Farm
            : null;

    private string? Known(string command) =>
        Reaching(command)
        ?? (!semantics.IsKnown ? null
            : semantics.IsDestructive(command) ? Scopes.Delete
            : semantics.IsReadOnly(command) ? Scopes.Read
            : null);
}
