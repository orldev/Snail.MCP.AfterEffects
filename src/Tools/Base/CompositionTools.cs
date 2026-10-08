using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>Compositions: creating, settings, duplicating and opening one in the viewer.</summary>
[McpServerToolType]
internal sealed class CompositionTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_create_comp", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Compositions.Create)]
    [Sends("comp_create")]
    public Task<CallToolResult> CreateCompAsync(
        [Description("Name of the composition.")] string name = "Comp",
        [Description("Width in pixels.")] int width = 1920,
        [Description("Height in pixels.")] int height = 1080,
        [Description("Duration in seconds.")] double duration = 10,
        [Description("Frames per second.")] double frameRate = 30,
        [Description("Pixel aspect ratio; 1 is square.")] double pixelAspect = 1,
        [Description(ToolDescriptions.Parameters.Color)] string? background = null,
        [Description("Folder to create it in, by name or id.")] string? folder = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CompCreate,
            new JsonObject().With("name", name).With("width", width).With("height", height).With("duration", duration).With("frame_rate", frameRate)
                .With("pixel_aspect", pixelAspect).With("background", background).WithReference("folder", folder),
            cancellationToken);

    [McpServerTool(Name = "ae_update_comp", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositions.Update)]
    [Sends("comp_update")]
    public Task<CallToolResult> UpdateCompAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The new name.")] string? name = null,
        [Description("Width in pixels.")] int? width = null,
        [Description("Height in pixels.")] int? height = null,
        [Description("Duration in seconds.")] double? duration = null,
        [Description("Frames per second.")] double? frameRate = null,
        [Description("Pixel aspect ratio; 1 is square.")] double? pixelAspect = null,
        [Description(ToolDescriptions.Parameters.Color)] string? background = null,
        [Description("The current time of the composition, in seconds.")] double? time = null,
        [Description("Start of the work area, in seconds.")] double? workAreaStart = null,
        [Description("Length of the work area, in seconds.")] double? workAreaDuration = null,
        [Description("Switch motion blur on or off for the composition.")] bool? motionBlur = null,
        [Description("Shutter angle in degrees, 0 to 720.")] double? shutterAngle = null,
        [Description("Hide the layers marked shy.")] bool? hideShy = null,
        [Description("Folder to move it into, by name or id.")] string? folder = null,
        [Description("Shutter phase in degrees.")] double? shutterPhase = null,
        [Description("Motion blur samples per frame.")] int? motionBlurSamples = null,
        [Description("Motion blur adaptive sample limit.")] int? motionBlurAdaptiveLimit = null,
        [Description("Preserve nested frame rate.")] bool? preserveNestedFrameRate = null,
        [Description("Preserve nested resolution.")] bool? preserveNestedResolution = null,
        [Description("Draft 3D in the viewer.")] bool? draft3d = null,
        [Description("Frame blending for layers that ask for it.")] bool? frameBlending = null,
        [Description("Resolution divisors [x, y]; [2, 2] is half.")] int[]? resolutionFactor = null,
        [Description("First frame number of the timeline.")] int? displayStartFrame = null,
        [Description("Start time of the timeline in seconds.")] double? displayStartTime = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CompUpdate,
            new JsonObject().WithReference("comp", comp).With("name", name).With("width", width).With("height", height).With("duration", duration)
                .With("frame_rate", frameRate).With("pixel_aspect", pixelAspect).With("background", background).With("time", time)
                .With("work_area_start", workAreaStart).With("work_area_duration", workAreaDuration).With("motion_blur", motionBlur)
                .With("shutter_angle", shutterAngle).With("hide_shy", hideShy).WithReference("folder", folder).With("shutter_phase", shutterPhase)
                .With("motion_blur_samples", motionBlurSamples).With("motion_blur_adaptive_limit", motionBlurAdaptiveLimit).With("preserve_nested_frame_rate", preserveNestedFrameRate)
                .With("preserve_nested_resolution", preserveNestedResolution).With("draft3d", draft3d).With("frame_blending", frameBlending).With("resolution_factor", resolutionFactor)
                .With("display_start_frame", displayStartFrame).With("display_start_time", displayStartTime),
            cancellationToken);

    [McpServerTool(Name = "ae_duplicate_comp", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Compositions.Duplicate)]
    [Sends("comp_duplicate")]
    public Task<CallToolResult> DuplicateCompAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name for the copy.")] string? name = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CompDuplicate, new JsonObject().WithReference("comp", comp).With("name", name), cancellationToken);

    [McpServerTool(Name = "ae_open_comp", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositions.Open)]
    [Sends("comp_open")]
    public Task<CallToolResult> OpenCompAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CompOpen, new JsonObject().WithReference("comp", comp).With("time", time), cancellationToken);
}
