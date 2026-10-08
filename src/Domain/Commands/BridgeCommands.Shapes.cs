namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand ShapeContents = new("shape_contents", "shapes");

    public static readonly BridgeCommand ShapeAdd = new("shape_add", "shapes");

    public static readonly BridgeCommand ShapeUpdate = new("shape_update", "shapes");

    public static readonly BridgeCommand ShapeRemove = new("shape_remove", "shapes");

    public static readonly BridgeCommand MasksList = new("masks_list", "shapes");

    public static readonly BridgeCommand MaskAdd = new("mask_add", "shapes");

    public static readonly BridgeCommand MaskUpdate = new("mask_update", "shapes");

    public static readonly BridgeCommand MaskRemove = new("mask_remove", "shapes");
}
