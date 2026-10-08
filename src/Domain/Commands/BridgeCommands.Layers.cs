namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand LayersList = new("layers_list", "layers");

    public static readonly BridgeCommand LayerInfo = new("layer_info", "layers");

    public static readonly BridgeCommand LayerAdd = new("layer_add", "layers");

    public static readonly BridgeCommand LayerUpdate = new("layer_update", "layers");

    public static readonly BridgeCommand LayerTransform = new("layer_transform", "layers");

    public static readonly BridgeCommand LayerRemove = new("layer_remove", "layers");

    public static readonly BridgeCommand LayerDuplicate = new("layer_duplicate", "layers");

    public static readonly BridgeCommand LayerMove = new("layer_move", "layers");

    public static readonly BridgeCommand LayersSelect = new("layers_select", "layers");
}
