namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand ViewerInfo = new("viewer_info", "viewer");

    public static readonly BridgeCommand ViewerSet = new("viewer_set", "viewer");
}
