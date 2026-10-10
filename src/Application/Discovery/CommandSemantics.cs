using System.Text.Json;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Discovery;

/// <summary>What each bridge command means, read at run time from the schema both sides share.</summary>
/// <remarks>The guard tests read <c>extension/commands.json</c> to hold the tools' annotations to it; this reads the same file for the decisions
/// made while the server runs: a tool's annotation answers for a tool, and run_tools and run_program carry commands they are handed, so only the
/// schema says what the work they are about to do is. A file that cannot be read leaves every command unknown rather than stopping the server.</remarks>
internal sealed class CommandSemantics(string? extensionDirectory = null)
{
    private readonly IReadOnlyDictionary<string, JsonObject> _commands = Read(Path.Combine(extensionDirectory ?? ServerPaths.BundledExtensionDirectory, "commands.json"));

    /// <summary>Whether the schema was found at all; without it every answer here is a guess.</summary>
    public bool IsKnown => _commands.Count > 0;

    public bool IsReadOnly(string command) => IsFlagSet(command, "read_only");

    public bool IsDestructive(string command) => IsFlagSet(command, "destructive");

    public bool IsOpenWorld(string command) => IsFlagSet(command, "open_world");

    private bool IsFlagSet(string command, string name) =>
        _commands.TryGetValue(command, out var meaning) && meaning[name] is JsonValue value && value.TryGetValue<bool>(out var set) && set;

    private static IReadOnlyDictionary<string, JsonObject> Read(string file)
    {
        try
        {
            return (JsonNode.Parse(File.ReadAllText(file))?["commands"] as JsonObject ?? [])
                .Where(command => command.Value is JsonObject)
                .ToDictionary(command => command.Key, command => (JsonObject)command.Value!, StringComparer.Ordinal);
        }
        catch (Exception failure) when (failure is IOException or JsonException or UnauthorizedAccessException)
        {
            return new Dictionary<string, JsonObject>(StringComparer.Ordinal);
        }
    }
}
