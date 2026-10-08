namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand LayerBounds = new("layer_bounds", "layout");

    public static readonly BridgeCommand PointConvert = new("point_convert", "layout");

    public static readonly BridgeCommand Selection = new("selection", "layout");

    public static readonly BridgeCommand GuidesList = new("guides_list", "layout");

    public static readonly BridgeCommand GuidesSet = new("guides_set", "layout");

    public static readonly BridgeCommand LayerSplit = new("layer_split", "layout");

    public static readonly BridgeCommand LayerCopyToComp = new("layer_copy_to_comp", "layout");

    public static readonly BridgeCommand LayerPlaceCorners = new("layer_place_corners", "layout");

    public static readonly BridgeCommand LayerById = new("layer_by_id", "layout");
}
