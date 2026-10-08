using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>Layers of a composition: listing, adding every kind, their switches and timing, their transform, order and selection.</summary>
[McpServerToolType]
internal sealed class LayerTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_list_layers", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.List)]
    [Sends("layers_list")]
    public Task<CallToolResult> ListLayersAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.LayerKind)] string? kind = null,
        [Description("Only layers whose name contains this, case-insensitive.")] string? nameContains = null,
        [Description("How many layers to return at most; the total is reported regardless.")] int limit = 200,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayersList,
            new JsonObject().WithReference("comp", comp).With("kind", kind).With("name_contains", nameContains).With("limit", limit),
            cancellationToken);

    [McpServerTool(Name = "ae_layer_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Info)]
    [Sends("layer_info")]
    public Task<CallToolResult> LayerInfoAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerInfo, new JsonObject().WithReference("comp", comp).WithReference("layer", layer), cancellationToken);

    [McpServerTool(Name = "ae_add_layer", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Add)]
    [Sends("layer_add")]
    public Task<CallToolResult> AddLayerAsync(
        [Description(ToolDescriptions.Parameters.LayerKind)] string kind = "solid",
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name of the new layer.")] string? name = null,
        [Description("For kind item: the footage or composition, by name or id.")] string? item = null,
        [Description("For kind text: the text.")] string? text = null,
        [Description("For kind text: vertical text.")] bool vertical = false,
        [Description(ToolDescriptions.Parameters.Color)] string? color = null,
        [Description("For a solid: its width; for text: the width of a text box.")] int? width = null,
        [Description("For a solid: its height; for text: the height of a text box.")] int? height = null,
        [Description("Length of the layer in seconds; the composition's otherwise.")] double? duration = null,
        [Description("[x, y] or [x, y, z] in composition pixels; the centre otherwise.")] double[]? position = null,
        [Description(ToolDescriptions.Parameters.LightType)] string? lightType = null,
        [Description("When the layer starts, in composition seconds.")] double? startTime = null,
        [Description("In point in composition seconds.")] double? inPoint = null,
        [Description("Out point in composition seconds.")] double? outPoint = null,
        [Description("Index for the layer, 1 the top; the top otherwise.")] int? at = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerAdd,
            new JsonObject().WithReference("comp", comp).With("kind", kind).With("name", name).WithReference("item", item).With("text", text).With("vertical", vertical)
                .With("color", color).With("width", width).With("height", height).With("duration", duration).With("position", position)
                .With("light_type", lightType).With("start_time", startTime).With("in_point", inPoint).With("out_point", outPoint).With("at", at),
            cancellationToken);

    [McpServerTool(Name = "ae_update_layer", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Update)]
    [Sends("layer_update")]
    public Task<CallToolResult> UpdateLayerAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The new name.")] string? name = null,
        [Description("Video switch: whether the layer renders.")] bool? enabled = null,
        [Description("Solo the layer.")] bool? solo = null,
        [Description("Mark the layer shy.")] bool? shy = null,
        [Description("Lock the layer.")] bool? locked = null,
        [Description("In point in composition seconds.")] double? inPoint = null,
        [Description("Out point in composition seconds.")] double? outPoint = null,
        [Description("When the layer starts, in composition seconds.")] double? startTime = null,
        [Description("Time stretch in percent; 100 is normal speed, -100 reversed.")] double? stretch = null,
        [Description("Parent by name or index; empty removes it.")] string? parent = null,
        [Description("When parenting, keep the transform values rather than the place on screen.")] bool? parentJump = null,
        [Description(ToolDescriptions.Parameters.BlendingMode)] string? blendingMode = null,
        [Description("Make the layer 3D or flat.")] bool? threeD = null,
        [Description("Motion blur switch of the layer.")] bool? motionBlur = null,
        [Description("Make a solid an adjustment layer or back.")] bool? adjustment = null,
        [Description("Guide layer: visible while working, left out of renders.")] bool? guide = null,
        [Description(ToolDescriptions.Parameters.Label)] int? label = null,
        [Description("Comment shown in the timeline.")] string? comment = null,
        [Description("Rendering quality: BEST, DRAFT or WIREFRAME.")] string? quality = null,
        [Description("Audio switch of a layer that has sound.")] bool? audioEnabled = null,
        [Description("Collapse transformations (continuous rasterisation for vectors).")] bool? collapse = null,
        [Description("Switch time remapping on, which adds the Time Remap property.")] bool? timeRemap = null,
        [Description("Track matte layer by name or index; pair with trackMatteType.")] string? trackMatte = null,
        [Description(ToolDescriptions.Parameters.TrackMatteType)] string? trackMatteType = null,
        [Description("NO_AUTO_ORIENT, ALONG_PATH, CAMERA_OR_POINT_OF_INTEREST or CHARACTERS_TOWARD_CAMERA.")] string? autoOrient = null,
        [Description("Preserve underlying transparency.")] bool? preserveTransparency = null,
        [Description("Effects switch of the layer.")] bool? effectsActive = null,
        [Description("NO_FRAME_BLEND, FRAME_MIX or PIXEL_MOTION.")] string? frameBlending = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerUpdate,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("name", name).With("enabled", enabled).With("solo", solo)
                .With("shy", shy).With("locked", locked).With("in_point", inPoint).With("out_point", outPoint).With("start_time", startTime)
                .With("stretch", stretch).WithParent(parent).With("parent_jump", parentJump).With("blending_mode", blendingMode).With("three_d", threeD)
                .With("motion_blur", motionBlur).With("adjustment", adjustment).With("guide", guide).With("label", label).With("comment", comment)
                .With("quality", quality).With("audio_enabled", audioEnabled).With("collapse", collapse).With("time_remap", timeRemap)
                .WithTrackMatte(trackMatte, trackMatteType).With("auto_orient", autoOrient).With("preserve_transparency", preserveTransparency)
                .With("effects_active", effectsActive).With("frame_blending", frameBlending),
            cancellationToken);

    [McpServerTool(Name = "ae_transform_layer", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Transform)]
    [Sends("layer_transform")]
    public Task<CallToolResult> TransformLayerAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Anchor point [x, y] or [x, y, z] in layer pixels.")] double[]? anchor = null,
        [Description("Position [x, y] or [x, y, z] in composition pixels.")] double[]? position = null,
        [Description("Scale [x, y] or [x, y, z] in percent; 100 keeps the size.")] double[]? scale = null,
        [Description("Rotation in degrees; the Z rotation of a 3D layer.")] double? rotation = null,
        [Description("Opacity in percent, 0 to 100.")] double? opacity = null,
        [Description("3D layers: orientation [x, y, z] in degrees.")] double[]? orientation = null,
        [Description("3D layers: X rotation in degrees.")] double? xRotation = null,
        [Description("3D layers: Y rotation in degrees.")] double? yRotation = null,
        [Description("Keyed at this time when the property has keyframes; the composition's current time when empty.")] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerTransform,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("anchor", anchor).With("position", position).With("scale", scale)
                .With("rotation", rotation).With("opacity", opacity).With("orientation", orientation).With("x_rotation", xRotation).With("y_rotation", yRotation)
                .With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_layers", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Remove)]
    [Sends("layer_remove")]
    public Task<CallToolResult> RemoveLayersAsync(
        [Description("Layers to delete, each by name or index.")] string[] layers,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerRemove, new JsonObject().WithReference("comp", comp).WithReferences("layers", layers), cancellationToken);

    [McpServerTool(Name = "ae_duplicate_layer", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Duplicate)]
    [Sends("layer_duplicate")]
    public Task<CallToolResult> DuplicateLayerAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name for the copy.")] string? name = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerDuplicate, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("name", name), cancellationToken);

    [McpServerTool(Name = "ae_move_layer", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Move)]
    [Sends("layer_move")]
    public Task<CallToolResult> MoveLayerAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Where to: an index (1 is the top), top or bottom.")] string? to = null,
        [Description("Put it right above this layer, by name or index.")] string? before = null,
        [Description("Put it right below this layer, by name or index.")] string? after = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerMove,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("to", to).WithReference("before", before).WithReference("after", after),
            cancellationToken);

    [McpServerTool(Name = "ae_select_layers", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layers.Select)]
    [Sends("layers_select")]
    public Task<CallToolResult> SelectLayersAsync(
        [Description("Layers to select, each by name or index.")] string[]? layers = null,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("replace (default), add (keep the selection), none (deselect all) or all.")] string mode = "replace",
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayersSelect, new JsonObject().WithReference("comp", comp).WithReferences("layers", layers).With("mode", mode), cancellationToken);
}
