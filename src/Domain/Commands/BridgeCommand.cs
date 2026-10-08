using System.Reflection;

namespace Snail.MCP.AfterEffects.Domain.Commands;

/// <summary>A command the bridge understands: its wire name and the bridge module that implements it.</summary>
internal sealed record BridgeCommand(string Name, string Module);

/// <summary>Every command the bridge registers, one partial file per bridge module; the list is read off the fields, so a command declared once is catalogued once.</summary>
/// <remarks>The contract tests keep each partial equal to the registrations of the module it names, so a command added on one side and
/// forgotten on the other fails a test rather than a call at runtime. The list is built lazily: static initialisers of partial files run
/// in no defined order, and read eagerly the fields of a later file would still be null.</remarks>
internal static partial class BridgeCommands
{
    private static readonly Lazy<IReadOnlyList<BridgeCommand>> Catalog = new(() =>
    [
        .. typeof(BridgeCommands)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(BridgeCommand))
            .Select(field => (BridgeCommand)field.GetValue(null)!),
    ]);

    public static IReadOnlyList<BridgeCommand> All => Catalog.Value;

    /// <summary>The commands one bridge module implements.</summary>
    public static IReadOnlyList<BridgeCommand> Of(string module) =>
        [.. All.Where(command => string.Equals(command.Module, module, StringComparison.Ordinal))];

    /// <summary>The commands whose names are closest to the one asked for, the closest first; empty when none of them shares a word with it.</summary>
    /// <remarks>Shared words rather than edit distance, because that is the shape of the misses in the logs: the tools read ae_create_comp and
    /// ae_remove_layers while the commands they send read comp_create and layer_remove, the same words the other way round, and a guess is
    /// usually a plural where the catalogue is singular. Both are compared with the trailing s dropped for that reason.</remarks>
    public static IReadOnlyList<string> Nearest(string name, int most)
    {
        var wanted = Words(name);
        var scored = new List<(string Name, int Shared)>();

        foreach (var command in All)
        {
            var shared = Words(command.Name).Count(word => wanted.Contains(word, StringComparer.Ordinal));

            if (shared > 0)
            {
                scored.Add((command.Name, shared));
            }
        }

        var closest = scored
            .OrderByDescending(pair => pair.Shared)
            .ThenBy(pair => pair.Name, StringComparer.Ordinal)
            .Take(most);

        return [.. closest.Select(pair => pair.Name)];
    }

    private static IReadOnlyList<string> Words(string name) =>
        [.. name.Split('_').Select(word => word.TrimEnd('s'))];
}
