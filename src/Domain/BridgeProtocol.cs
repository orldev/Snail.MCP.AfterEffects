namespace Snail.MCP.AfterEffects.Domain;

/// <summary>What this server expects of the bridge beyond the command catalog: the protocol number and the abilities it relies on.</summary>
/// <remarks>A digest says the two sides differ; it cannot say what the difference costs. The protocol number and the abilities say that:
/// a bridge behind on either is behind, one ahead is not, because it answers everything this server knows.</remarks>
internal static class BridgeProtocol
{
    public const int Version = 1;

    public static readonly IReadOnlyList<string> RequiredAbilities = ["hello_token", "params_gate", "undo_group", "expired"];

    /// <summary>The abilities a hello lacks of those this server relies on.</summary>
    public static IReadOnlyList<string> Missing(BridgeHello hello) =>
        [.. RequiredAbilities.Where(ability => !hello.Abilities.Contains(ability, StringComparer.Ordinal))];
}
