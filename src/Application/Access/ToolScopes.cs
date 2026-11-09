using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>Which kind of work a call is, so a client's key can be asked about it before After Effects is.</summary>
/// <remarks>Read from what the work already declares, the schema for the command a tool sends and the annotations for a tool that sends none,
/// rather than from a list kept beside them: a new tool would be missing from a list, and a tool missing from a list is a tool nothing guards.
/// Scripts and menu commands come first because both reach past every schema; rendering next, because it spends the machine. run_tools and
/// run_program are carriers: what they do is whatever is inside them, so each command they carry is asked about on its own. find_tool and the
/// tools that load and unload skills change nothing but the caller's own tool list, so they are reading whatever their annotations say.</remarks>
internal sealed class ToolScopes(CommandSemantics semantics)
{
    private static readonly string[] Scripting = ["script", "menu_command", "run_menu_command", "system_call"];

    private static readonly string[] Rendering = ["render", "render_start", "render_in_media_encoder", "render_in_ame", "farm_submit", "farm_retry", "watch_folder", "lease", "stop_worker"];

    private static readonly string[] Carriers = ["run_tools", "run_program"];

    /// <summary>Snail.Sdk's tools that find tools and load or unload skills: not read-only by their annotations, since they change the list
    /// a session is shown, but no work in After Effects.</summary>
    private static readonly string[] Discovery = ["find_tool", "skills", "enable_skill", "disable_skill"];

    /// <summary>Tools that destroy only when asked to: an upload replaces files with overwrite and otherwise only adds them.</summary>
    private static readonly string[] DestructiveOnlyWhenAsked = ["upload"];

    /// <summary>The scope a call of this tool needs, when the tool is the whole of the work.</summary>
    public string For(McpServerTool tool)
    {
        var name = tool.ProtocolTool.Name;
        var short_ = name.StartsWith(ToolCommands.Prefix, StringComparison.Ordinal) ? name[ToolCommands.Prefix.Length..] : name;

        if (Reaching(short_) is { } reaching)
        {
            return reaching;
        }

        if (Carriers.Contains(short_, StringComparer.Ordinal) || Discovery.Contains(short_, StringComparer.Ordinal))
        {
            return Scopes.Read;
        }

        if (DestructiveOnlyWhenAsked.Contains(short_, StringComparer.Ordinal))
        {
            return Scopes.Write;
        }

        if (ToolCommands.CommandOf(name) is { } command && Known(command) is { } known)
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
