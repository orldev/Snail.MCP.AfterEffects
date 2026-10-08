namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand KeyframesList = new("keyframes_list", "animation");

    public static readonly BridgeCommand KeyframesSet = new("keyframes_set", "animation");

    public static readonly BridgeCommand KeyframesRemove = new("keyframes_remove", "animation");

    public static readonly BridgeCommand KeyframesEase = new("keyframes_ease", "animation");

    public static readonly BridgeCommand ExpressionSet = new("expression_set", "animation");

    public static readonly BridgeCommand MarkersList = new("markers_list", "animation");

    public static readonly BridgeCommand MarkerSet = new("marker_set", "animation");

    public static readonly BridgeCommand MarkerRemove = new("marker_remove", "animation");

    public static readonly BridgeCommand TimeRemap = new("time_remap", "animation");

    public static readonly BridgeCommand PropertySeparate = new("property_separate", "animation");
}
