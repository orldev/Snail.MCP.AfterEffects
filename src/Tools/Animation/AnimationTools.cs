using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Animation;

/// <summary>Keyframes, easing, expressions, markers and time remapping: the animation skill.</summary>
[Skill(Skills.Animation, SkillDescriptions.Animation)]
[McpServerToolType]
internal sealed class AnimationTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_list_keyframes", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.ListKeyframes)]
    [Sends("keyframes_list")]
    public Task<CallToolResult> ListKeyframesAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.KeyframesList, Target(comp, layer, path), cancellationToken);

    [McpServerTool(Name = "ae_set_keyframes", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.SetKeyframes)]
    [Sends("keyframes_set")]
    public Task<CallToolResult> SetKeyframesAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Keys)] JsonArray keys,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Remove the property's existing keyframes first.")] bool replace = false,
        [Description(ToolDescriptions.Parameters.EasingPreset)] string? preset = null,
        [Description(ToolDescriptions.Parameters.InInterpolation)] string? inInterpolation = null,
        [Description(ToolDescriptions.Parameters.OutInterpolation)] string? outInterpolation = null,
        [Description(ToolDescriptions.Parameters.InfluenceIn)] double? influenceIn = null,
        [Description(ToolDescriptions.Parameters.InfluenceOut)] double? influenceOut = null,
        [Description(ToolDescriptions.Parameters.SpeedIn)] double? speedIn = null,
        [Description(ToolDescriptions.Parameters.SpeedOut)] double? speedOut = null,
        [Description(ToolDescriptions.Parameters.KeyLabel)] int? label = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.KeyframesSet,
            Eased(Target(comp, layer, path).With("keys", keys).With("replace", replace), preset, inInterpolation, outInterpolation, influenceIn, influenceOut, speedIn, speedOut)
                .With("label", label),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_keyframes", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.RemoveKeyframes)]
    [Sends("keyframes_remove")]
    public Task<CallToolResult> RemoveKeyframesAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.KeyIndices)] int[]? indices = null,
        [Description(ToolDescriptions.Parameters.KeyTimes)] double[]? times = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.KeyframesRemove, Target(comp, layer, path).With("indices", indices).With("times", times), cancellationToken);

    [McpServerTool(Name = "ae_ease_keyframes", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.EaseKeyframes)]
    [Sends("keyframes_ease")]
    public Task<CallToolResult> EaseKeyframesAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.KeyIndices)] int[]? indices = null,
        [Description(ToolDescriptions.Parameters.KeyTimes)] double[]? times = null,
        [Description(ToolDescriptions.Parameters.EasingPreset)] string? preset = null,
        [Description(ToolDescriptions.Parameters.InInterpolation)] string? inInterpolation = null,
        [Description(ToolDescriptions.Parameters.OutInterpolation)] string? outInterpolation = null,
        [Description(ToolDescriptions.Parameters.InfluenceIn)] double? influenceIn = null,
        [Description(ToolDescriptions.Parameters.InfluenceOut)] double? influenceOut = null,
        [Description(ToolDescriptions.Parameters.SpeedIn)] double? speedIn = null,
        [Description(ToolDescriptions.Parameters.SpeedOut)] double? speedOut = null,
        [Description(ToolDescriptions.Parameters.Continuous)] bool? continuous = null,
        [Description(ToolDescriptions.Parameters.AutoBezier)] bool? autoBezier = null,
        [Description(ToolDescriptions.Parameters.Roving)] bool? roving = null,
        [Description("For spatial properties: the tangent into the key as [x, y] or [x, y, z] relative to it.")] double[]? inTangent = null,
        [Description("For spatial properties: the tangent out of the key.")] double[]? outTangent = null,
        [Description("For spatial properties: keep the path tangents continuous through the key.")] bool? spatialContinuous = null,
        [Description("For spatial properties: let After Effects pick the path tangents.")] bool? spatialAutoBezier = null,
        [Description(ToolDescriptions.Parameters.KeyLabel)] int? label = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.KeyframesEase,
            Eased(Target(comp, layer, path).With("indices", indices).With("times", times), preset, inInterpolation, outInterpolation, influenceIn, influenceOut, speedIn, speedOut)
                .With("continuous", continuous).With("auto_bezier", autoBezier).With("roving", roving).With("in_tangent", inTangent).With("out_tangent", outTangent)
                .With("spatial_continuous", spatialContinuous).With("spatial_auto_bezier", spatialAutoBezier).With("label", label),
            cancellationToken);

    [McpServerTool(Name = "ae_separate_dimensions", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.SeparateDimensions)]
    [Sends("property_separate")]
    public Task<CallToolResult> SeparateDimensionsAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Separate the dimensions (true) or join them again (false); empty only reports.")] bool? separate = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PropertySeparate, Target(comp, layer, path).With("separate", separate), cancellationToken);

    [McpServerTool(Name = "ae_set_expression", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.SetExpression)]
    [Sends("expression_set")]
    public Task<CallToolResult> SetExpressionAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description("The expression, in After Effects' JavaScript; empty removes the expression on the property.")] string expression,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Leave the expression switched on; false puts it on the property disabled.")] bool enabled = true,
        [Description("Only check that the expression compiles and what it yields, restoring the property afterwards.")] bool checkOnly = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ExpressionSet,
            Target(comp, layer, path).With("expression", expression).With("enabled", enabled).With("check_only", checkOnly),
            cancellationToken);

    [McpServerTool(Name = "ae_list_markers", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.ListMarkers)]
    [Sends("markers_list")]
    public Task<CallToolResult> ListMarkersAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.MarkerLayer)] string? layer = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MarkersList, new JsonObject().WithReference("comp", comp).WithReference("layer", layer), cancellationToken);

    [McpServerTool(Name = "ae_set_marker", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.SetMarker)]
    [Sends("marker_set")]
    public Task<CallToolResult> SetMarkerAsync(
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        [Description("The marker's comment, shown on it in the timeline.")] string? comment = null,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.MarkerLayer)] string? layer = null,
        [Description("Duration of the marker in seconds; 0 for a point marker.")] double? duration = null,
        [Description(ToolDescriptions.Parameters.Label)] int? label = null,
        [Description("Chapter name, for formats that carry chapters.")] string? chapter = null,
        [Description("Web link the marker carries.")] string? url = null,
        [Description("Frame target of the web link.")] string? frameTarget = null,
        [Description("Cue point name, for Flash and streaming cue points.")] string? cuePointName = null,
        [Description("For composition markers: protect the region the marker spans from being retimed.")] bool? protectedRegion = null,
        [Description("Make the cue point an event rather than navigation.")] bool? eventCuePoint = null,
        [Description("Key and value pairs the marker carries, as an object of strings; for cue points and scripts that read them.")] JsonObject? parameters = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MarkerSet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("comment", comment).With("duration", duration)
                .With("label", label).With("chapter", chapter).With("url", url).With("frame_target", frameTarget).With("cue_point_name", cuePointName)
                .With("protected_region", protectedRegion).With("event_cue_point", eventCuePoint).With("parameters", parameters),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_markers", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.RemoveMarkers)]
    [Sends("marker_remove")]
    public Task<CallToolResult> RemoveMarkersAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.MarkerLayer)] string? layer = null,
        [Description("Marker indices as ae_list_markers numbers them, 1 first; the markers at times, or all markers, when empty.")] int[]? indices = null,
        [Description("Times in seconds of the markers meant; the markers at indices, or all markers, when empty.")] double[]? times = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MarkerRemove,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("indices", indices).With("times", times),
            cancellationToken);

    [McpServerTool(Name = "ae_time_remap", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Animation.TimeRemap)]
    [Sends("time_remap")]
    public Task<CallToolResult> TimeRemapAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Switch time remapping on (the default) or off.")] bool enabled = true,
        [Description("Keyframes as {time, value}: the composition time and the source time it shows, both in seconds.")] JsonArray? keys = null,
        [Description("Remove the existing time remap keys before setting the new ones.")] bool replace = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TimeRemap,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("enabled", enabled).With("keys", keys).With("replace", replace),
            cancellationToken);

    private static JsonObject Target(string? comp, string layer, string path) =>
        new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path);

    private static JsonObject Eased(
        JsonObject arguments, string? preset, string? inInterpolation, string? outInterpolation, double? influenceIn, double? influenceOut, double? speedIn, double? speedOut) =>
        arguments.With("preset", preset).With("in_interpolation", inInterpolation).With("out_interpolation", outInterpolation)
            .With("influence_in", influenceIn).With("influence_out", influenceOut).With("speed_in", speedIn).With("speed_out", speedOut);
}
