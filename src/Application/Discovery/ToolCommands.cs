using System.Reflection;

namespace Snail.MCP.AfterEffects.Application.Discovery;

/// <summary>Which bridge commands a tool sends, declared on the tool itself; the source holds the same fact, and a contract test keeps the two equal.</summary>
/// <remarks>A tool's command is what run_program and find_tool name, so it is written once as an attribute rather than guessed from the tool's
/// name: ae_add_layer sends layer_add, and nothing but a declaration can say so without a guess.</remarks>
internal static class ToolCommands
{
    /// <summary>The commands a tool method declares with <see cref="SendsAttribute"/>, in declaration order.</summary>
    public static IReadOnlyList<string> Of(MethodInfo method) =>
        [.. method.GetCustomAttributes<SendsAttribute>().Select(sends => sends.Command)];
}

/// <summary>Names a bridge command a tool sends.</summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
internal sealed class SendsAttribute(string command) : Attribute
{
    public string Command => command;
}
