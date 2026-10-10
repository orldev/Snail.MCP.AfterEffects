using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Shapes;

/// <summary>Shape layer contents and masks: the shapes skill.</summary>
[Skill(Skills.Shapes, SkillDescriptions.Shapes)]
[McpServerToolType]
internal sealed class ShapeTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_shape_contents", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.Contents)]
    [Sends("shape_contents")]
    public Task<CallToolResult> ShapeContentsAsync(
        [Description(ToolDescriptions.Parameters.ShapeLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Group to start from, by path; the whole contents when empty.")] string? path = null,
        [Description("How many levels of groups to open; 3 by default.")] int depth = 3,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ShapeContents,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("depth", depth),
            cancellationToken);

    [McpServerTool(Name = "ae_add_shape", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.Add)]
    [Sends("shape_add")]
    public Task<CallToolResult> AddShapeAsync(
        [Description(ToolDescriptions.Parameters.ShapeLayer)] string layer,
        [Description(ToolDescriptions.Parameters.ShapeKind)] string kind,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The group to add into, by path; the layer's contents when empty.")] string? into = null,
        [Description("Name of the new item, or of the group made for it.")] string? name = null,
        [Description(ToolDescriptions.Parameters.ShapeSettings)] JsonObject? settings = null,
        [Description("Add a fill of this colour beside the shape.")] JsonNode? fill = null,
        [Description("Add a stroke of this colour beside the shape.")] JsonNode? stroke = null,
        [Description("Width of that stroke in pixels.")] double? strokeWidth = null,
        [Description("Position in the group, 1 first; at the end when empty. Paint and operators act on what is above them.")] int? at = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ShapeAdd,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("kind", kind).With("into", into).With("name", name).With("settings", settings)
                .With("fill", fill).With("stroke", stroke).With("stroke_width", strokeWidth).With("at", at),
            cancellationToken);

    [McpServerTool(Name = "ae_update_shape", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.Update)]
    [Sends("shape_update")]
    public Task<CallToolResult> UpdateShapeAsync(
        [Description(ToolDescriptions.Parameters.ShapeLayer)] string layer,
        [Description(ToolDescriptions.Parameters.ShapePath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("New name.")] string? name = null,
        [Description("Switch the item on or off.")] bool? enabled = null,
        [Description(ToolDescriptions.Parameters.ShapeSettings)] JsonObject? settings = null,
        [Description("Set the settings as keyframes at this time.")] double? time = null,
        [Description("Move the item to this position in its group, 1 first.")] int? at = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ShapeUpdate,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("name", name).With("enabled", enabled)
                .With("settings", settings).With("time", time).With("at", at),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_shape", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.Remove)]
    [Sends("shape_remove")]
    public Task<CallToolResult> RemoveShapeAsync(
        [Description(ToolDescriptions.Parameters.ShapeLayer)] string layer,
        [Description(ToolDescriptions.Parameters.ShapePath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ShapeRemove, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path), cancellationToken);

    [McpServerTool(Name = "ae_list_masks", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.ListMasks)]
    [Sends("masks_list")]
    public Task<CallToolResult> ListMasksAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MasksList, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time), cancellationToken);

    [McpServerTool(Name = "ae_add_mask", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.AddMask)]
    [Sends("mask_add")]
    public Task<CallToolResult> AddMaskAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name of the mask.")] string? name = null,
        [Description(ToolDescriptions.Parameters.MaskRectangle)] double[]? rectangle = null,
        [Description(ToolDescriptions.Parameters.MaskEllipse)] double[]? ellipse = null,
        [Description(ToolDescriptions.Parameters.MaskVertices)] JsonArray? vertices = null,
        [Description("In tangents, one [x, y] per vertex relative to it; straight when empty.")] JsonArray? inTangents = null,
        [Description("Out tangents, one [x, y] per vertex relative to it; straight when empty.")] JsonArray? outTangents = null,
        [Description("Close the path; true by default.")] bool? closed = null,
        [Description(ToolDescriptions.Parameters.MaskMode)] string? mode = null,
        [Description("Invert the mask.")] bool? inverted = null,
        [Description("Feather in pixels, one number or [x, y].")] JsonNode? feather = null,
        [Description("Mask opacity in percent.")] double? opacity = null,
        [Description("Mask expansion in pixels; negative contracts.")] double? expansion = null,
        [Description(ToolDescriptions.Parameters.Color)] JsonNode? color = null,
        [Description(ToolDescriptions.Parameters.FeatherPoints)] JsonArray? featherPoints = null,
        [Description("How the feather falls off: FFO_SMOOTH or FFO_LINEAR.")] string? featherFalloff = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MaskAdd,
            MaskArguments(comp, layer, name, rectangle, ellipse, vertices, inTangents, outTangents, closed, mode, inverted, feather, opacity, expansion, color)
                .With("feather_points", featherPoints).With("feather_falloff", featherFalloff),
            cancellationToken);

    [McpServerTool(Name = "ae_update_mask", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.UpdateMask)]
    [Sends("mask_update")]
    public Task<CallToolResult> UpdateMaskAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Mask)] string mask,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("New name.")] string? name = null,
        [Description(ToolDescriptions.Parameters.MaskRectangle)] double[]? rectangle = null,
        [Description(ToolDescriptions.Parameters.MaskEllipse)] double[]? ellipse = null,
        [Description(ToolDescriptions.Parameters.MaskVertices)] JsonArray? vertices = null,
        [Description("In tangents, one [x, y] per vertex relative to it.")] JsonArray? inTangents = null,
        [Description("Out tangents, one [x, y] per vertex relative to it.")] JsonArray? outTangents = null,
        [Description("Close the path.")] bool? closed = null,
        [Description(ToolDescriptions.Parameters.MaskMode)] string? mode = null,
        [Description("Invert the mask.")] bool? inverted = null,
        [Description("Feather in pixels, one number or [x, y].")] JsonNode? feather = null,
        [Description("Mask opacity in percent.")] double? opacity = null,
        [Description("Mask expansion in pixels; negative contracts.")] double? expansion = null,
        [Description(ToolDescriptions.Parameters.Color)] JsonNode? color = null,
        [Description("Lock the mask against edits in the interface.")] bool? locked = null,
        [Description("Smooth the path as a roto bezier.")] bool? rotoBezier = null,
        [Description("Set the shape and the numbers as keyframes at this time.")] double? time = null,
        [Description(ToolDescriptions.Parameters.FeatherPoints)] JsonArray? featherPoints = null,
        [Description("How the feather falls off: FFO_SMOOTH or FFO_LINEAR.")] string? featherFalloff = null,
        [Description("Motion blur of the mask: SAME_AS_LAYER, ON or OFF.")] string? motionBlur = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MaskUpdate,
            MaskArguments(comp, layer, name, rectangle, ellipse, vertices, inTangents, outTangents, closed, mode, inverted, feather, opacity, expansion, color)
                .WithReference("mask", mask).With("locked", locked).With("roto_bezier", rotoBezier).With("time", time)
                .With("feather_points", featherPoints).With("feather_falloff", featherFalloff).With("motion_blur", motionBlur),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_mask", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Shapes.RemoveMask)]
    [Sends("mask_remove")]
    public Task<CallToolResult> RemoveMaskAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Mask)] string mask,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MaskRemove, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("mask", mask), cancellationToken);

    private static JsonObject MaskArguments(
        string? comp, string layer, string? name, double[]? rectangle, double[]? ellipse, JsonArray? vertices, JsonArray? inTangents, JsonArray? outTangents,
        bool? closed, string? mode, bool? inverted, JsonNode? feather, double? opacity, double? expansion, JsonNode? color) =>
        new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("name", name).With("rectangle", rectangle).With("ellipse", ellipse)
            .With("vertices", vertices).With("in_tangents", inTangents).With("out_tangents", outTangents).With("closed", closed).With("mode", mode)
            .With("inverted", inverted).With("feather", feather).With("opacity", opacity).With("expansion", expansion).With("color", color);
}
