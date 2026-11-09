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
}
