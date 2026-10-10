using System.ComponentModel;
using System.Reflection;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Skills;

namespace Snail.MCP.AfterEffects.Application.Discovery;

/// <summary>One parameter of a tool, as the answer to a search states it.</summary>
internal sealed record ToolParameter(string Name, string Type, string? Description, bool IsRequired);

/// <summary>A tool as discovery sees it: the name the model would call, what the catalog says it does, the skill it arrives with, the command a
/// program sends for it, and the parameters either takes.</summary>
/// <remarks>The parameters travel with a search so that finding a tool is enough to call it: through run_program by its command name, without
/// loading the skill and so without changing the tool list, which a client would have to read again. For a tool that sends a command they are
/// spelled as the command takes them, in snake_case; the tool itself takes the same names in camelCase.</remarks>
internal sealed record ToolEntry(string Name, string Description, string? Skill, string? Command = null, IReadOnlyList<ToolParameter>? Parameters = null)
{
    public IReadOnlyList<ToolParameter> Parameters { get; } = Parameters ?? [];

    /// <summary>The words of the name; a search that matches here means far more than one that matches the prose.</summary>
    public IReadOnlySet<string> Words { get; } = Vocabulary.Of(Name);
}

/// <summary>A tool a search found, with how well it answered.</summary>
internal sealed record ToolMatch(ToolEntry Tool, double Score);

/// <summary>Every tool the server can offer, skills included, searchable by what the caller is trying to do.</summary>
/// <remarks>The tool list a client holds shows only the loaded skills, so a model that has not opened a skill cannot see the tool it needs and
/// reaches for ae_script instead. This index is read from the same attributes the server registers tools from, so it answers for the whole
/// catalog whatever is loaded, and cannot drift from it.</remarks>
internal sealed class ToolIndex(Assembly assembly)
{
    public const string Prefix = "ae_";

    private readonly IReadOnlyList<ToolEntry> _tools = Read(assembly);

    public IReadOnlyList<ToolEntry> Tools => _tools;

    /// <summary>The tools that best answer an intent in plain words, most relevant first; empty when nothing matches.</summary>
    public IReadOnlyList<ToolMatch> Search(string intent, int limit)
    {
        var words = Vocabulary.Of(intent);

        if (words.Count == 0)
        {
            return [];
        }

        var matching = _tools
            .Select(tool => new ToolMatch(tool, Score(tool, words)))
            .Where(match => match.Score > 0);
        var ranked = matching
            .OrderByDescending(match => match.Score)
            .ThenBy(match => match.Tool.Name, StringComparer.Ordinal);

        return [.. ranked.Take(limit)];
    }

    public ToolEntry? Named(string name) =>
        _tools.FirstOrDefault(tool => string.Equals(tool.Name, name, StringComparison.Ordinal));

    /// <summary>A word in the name is worth four times one in the prose: tools here are named after what they do.</summary>
    private static double Score(ToolEntry tool, IReadOnlySet<string> words)
    {
        var score = 0.0;

        foreach (var word in words)
        {
            if (tool.Words.Contains(word))
            {
                score += 4;
            }
            else if (tool.Words.Any(part => Shares(part, word)))
            {
                score += 2.5;
            }
            else if (tool.Description.Contains(word, StringComparison.OrdinalIgnoreCase))
            {
                score += 1;
            }
        }

        return Math.Round(score / words.Count, 2);
    }

    /// <summary>The command a program sends for this tool, read from the tool's source: the tools that send none are the server's own.</summary>
    private static string? CommandOf(MethodInfo method) =>
        ToolCommands.Of(method) is { Count: 1 } commands ? commands[0] : null;

    private static bool IsAnArgument(ParameterInfo parameter) =>
        parameter.ParameterType != typeof(CancellationToken) && parameter.ParameterType.Name != "McpServer";

    /// <summary>The wait a tool keeps on the server side; a program sends the command without it.</summary>
    private static bool IsAWait(ParameterInfo parameter) => parameter.Name == "timeoutSeconds";

    /// <summary>A parameter as the bridge spells it: a tool's camelCase name is the command's snake_case one, which is what a program writes.</summary>
    public static string WireName(string name)
    {
        var builder = new System.Text.StringBuilder(name.Length + 4);

        foreach (var letter in name)
        {
            if (char.IsUpper(letter))
            {
                builder.Append('_').Append(char.ToLowerInvariant(letter));
            }
            else
            {
                builder.Append(letter);
            }
        }

        return builder.ToString();
    }

    private static IReadOnlyList<ToolParameter> ParametersOf(MethodInfo method, string? command) =>
        [.. method.GetParameters()
            .Where(IsAnArgument)
            .Where(parameter => command is null || !IsAWait(parameter))
            .Select(parameter => Describe(parameter, command is null ? parameter.Name ?? string.Empty : WireName(parameter.Name ?? string.Empty)))];

    private static ToolParameter Describe(ParameterInfo parameter, string name) => new(
        name,
        WireType(parameter.ParameterType),
        parameter.GetCustomAttribute<DescriptionAttribute>()?.Description,
        !parameter.HasDefaultValue);

    /// <summary>The type as the wire carries it, which is what a program has to write.</summary>
    private static string WireType(Type type)
    {
        var bare = Nullable.GetUnderlyingType(type) ?? type;

        if (bare == typeof(string) || bare.IsEnum)
        {
            return "string";
        }

        if (bare == typeof(bool))
        {
            return "boolean";
        }

        if (bare == typeof(int) || bare == typeof(long) || bare == typeof(double) || bare == typeof(float))
        {
            return "number";
        }

        return bare.IsArray || bare == typeof(JsonArray) ? "array" : "object";
    }

    /// <summary>One word is a form of the other: "layer" answers "layers", "render" answers "rendering".</summary>
    private static bool Shares(string part, string word) =>
        part.Length >= 4 && word.Length >= 4
        && (part.StartsWith(word, StringComparison.OrdinalIgnoreCase) || word.StartsWith(part, StringComparison.OrdinalIgnoreCase));

    private static IReadOnlyList<ToolEntry> Read(Assembly source) =>
        [.. source.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .SelectMany(type => type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(method => (Method: method, Tool: method.GetCustomAttribute<McpServerToolAttribute>(), Text: method.GetCustomAttribute<DescriptionAttribute>(), Skill: type.GetCustomAttribute<SkillAttribute>()))
                .Where(found => found.Tool?.Name is not null)
                .Select(found => new ToolEntry(
                    found.Tool!.Name!,
                    found.Text?.Description ?? string.Empty,
                    found.Skill?.Name,
                    CommandOf(found.Method),
                    ParametersOf(found.Method, CommandOf(found.Method)))))
            .OrderBy(tool => tool.Name, StringComparer.Ordinal)];
}
