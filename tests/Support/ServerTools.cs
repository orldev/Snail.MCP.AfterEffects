using System.Reflection;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.Sdk.Mcp.Hosting.Discovery;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The tools a client of this server is offered, read from the assembly the way the SDK registers them.</summary>
internal static class ServerTools
{
    /// <summary>The tools Snail.Sdk adds for this server: find_tool, run_program, and the three that load and unload skills.</summary>
    public static readonly IReadOnlyList<string> OfTheSdk = ["find_tool", "run_program", "ae_skills", "ae_enable_skill", "ae_disable_skill"];

    /// <summary>The tools listed to a session that has loaded no skill: every tool of a class outside a skill, and Snail.Sdk's.</summary>
    /// <remarks>Tools are discovered by the SDK through attributes, so a misconfigured attribute is invisible to reflection-only guards.</remarks>
    public static HashSet<string> AlwaysOn()
    {
        var classes = ClientRegistrationExtensions.ToolTypes(typeof(ToolDescriptions).Assembly).Where(type => type.GetCustomAttribute<SkillAttribute>() is null);
        var methods = classes.SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly));
        var names = methods.Select(method => method.GetCustomAttribute<McpServerToolAttribute>()?.Name).OfType<string>();

        return [.. names, .. OfTheSdk];
    }

    /// <summary>The skills the tool classes declare, each once.</summary>
    public static IReadOnlyList<string> Skills()
    {
        var declared = ClientRegistrationExtensions.ToolTypes(typeof(ToolDescriptions).Assembly).Select(type => type.GetCustomAttribute<SkillAttribute>()?.Name);

        return [.. declared.OfType<string>().Distinct(StringComparer.Ordinal)];
    }
}
