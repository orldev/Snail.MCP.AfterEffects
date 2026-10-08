namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand PropertyTree = new("property_tree", "properties");

    public static readonly BridgeCommand PropertyGet = new("property_get", "properties");

    public static readonly BridgeCommand PropertySet = new("property_set", "properties");
}
