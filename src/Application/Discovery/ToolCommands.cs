using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace Snail.MCP.AfterEffects.Application.Discovery;

/// <summary>Which bridge commands a tool sends, declared on the tool itself; the source holds the same fact, and a contract test keeps the two equal.</summary>
/// <remarks>A tool's command is what run_program and find_tool name, so it is written once as an attribute rather than guessed from the tool's
/// name: ae_add_layer sends layer_add, and nothing but a declaration can say so without a guess.</remarks>
internal static class ToolCommands
{
    /// <summary>The prefix every tool of this server that drives After Effects carries.</summary>
    public const string Prefix = "ae_";

    private static readonly IReadOnlyDictionary<string, string> Commands = Read(typeof(ToolCommands).Assembly);

    /// <summary>The commands a tool method declares with <see cref="SendsAttribute"/>, in declaration order.</summary>
    public static IReadOnlyList<string> Of(MethodInfo method) =>
        [.. method.GetCustomAttributes<SendsAttribute>().Select(sends => sends.Command)];

    /// <summary>The one command a tool sends, by the tool's name: what find_tool answers as its command and a program calls; null for a tool
    /// that sends none, or several (run_tools).</summary>
    public static string? CommandOf(string tool) => Commands.GetValueOrDefault(tool);

    /// <summary>A parameter of a tool as its command takes it, which find_tool answers for a program to send: the snake_case name the
    /// bridge reads, and none for the time a tool waits, which a program's own deadline decides.</summary>
    /// <remarks>Asked by a dotted path of schema names — <c>lightType</c>, or <c>output.fileFormat</c> for a field of an object
    /// parameter — and answered with the path it keeps in the command, every segment the bridge's spelling: a field stays inside its
    /// object, which the bridge receives whole. Every tool parameter of this server is a value, an array or free JSON today, so the
    /// fields are a record's to come, and spelling only the first segment would send that record's fields in camelCase. Only the
    /// tool's own <c>timeoutSeconds</c> is dropped: a field of that name inside an object is the bridge's to read.</remarks>
    public static string? ParameterOf(string tool, string path) =>
        path == TimeoutParameter ? null : string.Join('.', path.Split('.').Select(WireName));

    /// <summary>The parameter of a tool that says how long it waits for After Effects; no command takes it.</summary>
    public const string TimeoutParameter = "timeoutSeconds";

    /// <summary>A parameter as the bridge spells it: a tool's camelCase name is the command's snake_case one, which is what a program writes.</summary>
    public static string WireName(string name) => JsonNamingPolicy.SnakeCaseLower.ConvertName(name);

    private static Dictionary<string, string> Read(Assembly assembly)
    {
        var methods = assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var sending = methods
            .Select(method => (Name: method.GetCustomAttribute<McpServerToolAttribute>()?.Name, Commands: Of(method)))
            .Where(tool => tool.Name is not null && tool.Commands.Count == 1);

        return sending.ToDictionary(tool => tool.Name!, tool => tool.Commands[0], StringComparer.Ordinal);
    }
}

/// <summary>Names a bridge command a tool sends.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class SendsAttribute(string command) : Attribute
{
    public string Command => command;
}
