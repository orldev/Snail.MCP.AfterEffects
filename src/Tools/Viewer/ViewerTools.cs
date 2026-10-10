using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Viewer;

/// <summary>The viewer panel and its views: the viewer skill.</summary>
[Skill(Skills.Viewer, SkillDescriptions.Viewer)]
[McpServerToolType]
internal sealed class ViewerTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_viewer", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Viewer.Info)]
    [Sends("viewer_info")]
    public Task<CallToolResult> ViewerAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ViewerInfo, [], cancellationToken);

    [McpServerTool(Name = "ae_set_viewer", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Viewer.Set)]
    [Sends("viewer_set")]
    public Task<CallToolResult> SetViewerAsync(
        [Description(ToolDescriptions.Parameters.View)] int? view = null,
        [Description(ToolDescriptions.Parameters.Channel)] string? channels = null,
        [Description("Show a checkerboard behind transparency.")] bool? checkerboards = null,
        [Description("Exposure in stops.")] double? exposure = null,
        [Description("FP_OFF, FP_ADAPTIVE_RESOLUTION, FP_DRAFT, FP_FAST_DRAFT or FP_WIREFRAME.")] string? fastPreview = null,
        [Description("Lock the guides.")] bool? guidesLocked = null,
        [Description("Snap to the guides.")] bool? guidesSnap = null,
        [Description("Show the guides.")] bool? guidesVisibility = null,
        [Description("Show the rulers.")] bool? rulers = null,
        [Description("Zoom factor, 1 for 100 percent.")] double? zoom = null,
        [Description("Maximize the viewer panel, or restore it.")] bool? maximized = null,
        [Description("Make this view and its viewer active.")] bool activate = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ViewerSet,
            new JsonObject().With("view", view).With("channels", channels).With("checkerboards", checkerboards).With("exposure", exposure).With("fast_preview", fastPreview)
                .With("guides_locked", guidesLocked).With("guides_snap", guidesSnap).With("guides_visibility", guidesVisibility).With("rulers", rulers).With("zoom", zoom)
                .With("maximized", maximized).With("activate", activate),
            cancellationToken);
}
