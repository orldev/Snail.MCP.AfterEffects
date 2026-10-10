using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Layout;

/// <summary>Measuring, converting, selecting, guides, splitting, copying and placing: the layout skill.</summary>
[Skill(Skills.Layout, SkillDescriptions.Layout)]
[McpServerToolType]
internal sealed class LayoutTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_layer_bounds", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.Bounds)]
    [Sends("layer_bounds")]
    public Task<CallToolResult> LayerBoundsAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        [Description("Measure only the pixels with content rather than the layer's full extent; false by default.")] bool extents = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerBounds, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("extents", extents), cancellationToken);

    [McpServerTool(Name = "ae_convert_point", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.ConvertPoint)]
    [Sends("point_convert")]
    public Task<CallToolResult> ConvertPointAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description("[x, y] to convert.")] double[] point,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("comp converts from the layer's pixels into the composition (the default); source converts the other way.")] string to = "comp",
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PointConvert, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("point", point).With("to", to).With("time", time), cancellationToken);

    [McpServerTool(Name = "ae_selection", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.Selection)]
    [Sends("selection")]
    public Task<CallToolResult> SelectionAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.Selection, new JsonObject().WithReference("comp", comp), cancellationToken);

    [McpServerTool(Name = "ae_guides", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.Guides)]
    [Sends("guides_list")]
    public Task<CallToolResult> GuidesAsync(
        [Description(ToolDescriptions.Parameters.GuideItem)] string? item = null,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.GuidesList, new JsonObject().WithReference("comp", comp).WithReference("item", item), cancellationToken);

    [McpServerTool(Name = "ae_set_guides", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.SetGuides)]
    [Sends("guides_set")]
    public Task<CallToolResult> SetGuidesAsync(
        [Description(ToolDescriptions.Parameters.GuideItem)] string? item = null,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Guides to add, each {orientation: horizontal or vertical, position} in the item's pixels.")] JsonArray? add = null,
        [Description("Guides to move, each {index, position}.")] JsonArray? move = null,
        [Description("Indices of guides to remove, 0 first, as ae_guides numbers them.")] int[]? remove = null,
        [Description("Remove every guide first.")] bool clear = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.GuidesSet,
            new JsonObject().WithReference("comp", comp).WithReference("item", item).With("add", add).With("move", move).With("remove", remove).With("clear", clear),
            cancellationToken);

    [McpServerTool(Name = "ae_split_layer", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.Split)]
    [Sends("layer_split")]
    public Task<CallToolResult> SplitLayerAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerSplit, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time), cancellationToken);

    [McpServerTool(Name = "ae_copy_layer_to_comp", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.CopyToComp)]
    [Sends("layer_copy_to_comp")]
    public Task<CallToolResult> CopyLayerToCompAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description("The composition the copy goes to, by name or id.")] string target,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerCopyToComp, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("target", target), cancellationToken);

    [McpServerTool(Name = "ae_place_layer_corners", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.PlaceCorners)]
    [Sends("layer_place_corners")]
    public Task<CallToolResult> PlaceLayerCornersAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Corner)] double[] topLeft,
        [Description(ToolDescriptions.Parameters.Corner)] double[] topRight,
        [Description(ToolDescriptions.Parameters.Corner)] double[] bottomLeft,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Apply the transform to the layer (true, the default) or only compute it.")] bool apply = true,
        [Description("Set the transform as keyframes at this time.")] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerPlaceCorners,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("top_left", topLeft).With("top_right", topRight).With("bottom_left", bottomLeft)
                .With("apply", apply).With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_find_layer", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Layout.FindLayer)]
    [Sends("layer_by_id")]
    public Task<CallToolResult> FindLayerAsync(
        [Description("The layer's id as ae_layer_info reports it.")] int id,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerById, new JsonObject().With("id", id), cancellationToken);
}
