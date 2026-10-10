using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Rendering;

/// <summary>The render queue, templates, rendering in place and Media Encoder: the rendering skill.</summary>
[Skill(Skills.Rendering, SkillDescriptions.Rendering)]
[McpServerToolType]
internal sealed class RenderTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_render_queue", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Rendering.Queue)]
    [Sends("render_queue")]
    public Task<CallToolResult> RenderQueueAsync(
        [Description("Include every settable render and output setting of each item.")] bool settings = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderQueue, new JsonObject().With("settings", settings), cancellationToken);

    [McpServerTool(Name = "ae_render_templates", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Rendering.Templates)]
    [Sends("render_templates")]
    public Task<CallToolResult> RenderTemplatesAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderTemplates, new JsonObject().WithReference("comp", comp), cancellationToken);

    [McpServerTool(Name = "ae_add_render", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.Add)]
    [Sends("render_add")]
    public Task<CallToolResult> AddRenderAsync(
        [Description("Where to write the output: a file path, or a folder to take the composition's name.")] string outputPath,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Render settings template by name, as ae_render_templates lists them.")] string? renderTemplate = null,
        [Description("Output module template by name, as ae_render_templates lists them.")] string? outputTemplate = null,
        [Description("Which output module of the item the path and template apply to, 1 first.")] int? outputModule = null,
        [Description(ToolDescriptions.Parameters.PostRenderAction)] string? postRenderAction = null,
        [Description("Start of the span to render, in seconds.")] double? start = null,
        [Description("End of the span to render, in seconds.")] double? end = null,
        [Description("Render the composition's work area instead of a span.")] bool workArea = false,
        [Description("Render every nth frame only; 0 renders all.")] int? skipFrames = null,
        [Description(ToolDescriptions.Parameters.RenderSettings)] JsonObject? renderSettings = null,
        [Description(ToolDescriptions.Parameters.OutputSettings)] JsonObject? outputSettings = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderAdd,
            new JsonObject().WithReference("comp", comp).With("render_template", renderTemplate).With("output_template", outputTemplate).With("output_path", outputPath)
                .With("output_module", outputModule).With("post_render_action", postRenderAction).With("start", start).With("end", end).With("work_area", workArea)
                .With("skip_frames", skipFrames).With("render_settings", renderSettings).With("output_settings", outputSettings),
            cancellationToken);

    [McpServerTool(Name = "ae_update_render", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.Update)]
    [Sends("render_update")]
    public Task<CallToolResult> UpdateRenderAsync(
        [Description(ToolDescriptions.Parameters.RenderItem)] int item,
        [Description("Render settings template by name.")] string? renderTemplate = null,
        [Description("Output module template by name.")] string? outputTemplate = null,
        [Description("Where to write the output: a file path, or a folder to take the composition's name.")] string? outputPath = null,
        [Description("Which output module of the item the path and template apply to, 1 first.")] int? outputModule = null,
        [Description(ToolDescriptions.Parameters.PostRenderAction)] string? postRenderAction = null,
        [Description("Start of the span to render, in seconds.")] double? start = null,
        [Description("End of the span to render, in seconds.")] double? end = null,
        [Description("Render the composition's work area instead of a span.")] bool workArea = false,
        [Description("Render every nth frame only; 0 renders all.")] int? skipFrames = null,
        [Description("Queue the item (true) or leave it unqueued (false).")] bool? render = null,
        [Description(ToolDescriptions.Parameters.RenderSettings)] JsonObject? renderSettings = null,
        [Description(ToolDescriptions.Parameters.OutputSettings)] JsonObject? outputSettings = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderUpdate,
            new JsonObject().With("item", item).With("render_template", renderTemplate).With("output_template", outputTemplate).With("output_path", outputPath)
                .With("output_module", outputModule).With("post_render_action", postRenderAction).With("start", start).With("end", end).With("work_area", workArea)
                .With("skip_frames", skipFrames).With("render", render).With("render_settings", renderSettings).With("output_settings", outputSettings),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_render", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Rendering.Remove)]
    [Sends("render_remove")]
    public Task<CallToolResult> RemoveRenderAsync(
        [Description(ToolDescriptions.Parameters.RenderItem)] int? item = null,
        [Description("Remove every item.")] bool all = false,
        [Description("Remove the items with this status: DONE, UNQUEUED, QUEUED, NEEDS_OUTPUT, USER_STOPPED, ERR_STOPPED, WILL_CONTINUE.")] string? status = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderRemove, new JsonObject().With("item", item).With("all", all).With("status", status), cancellationToken);

    [McpServerTool(Name = "ae_render", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.Render)]
    [Sends("render_start")]
    public Task<CallToolResult> RenderAsync(
        [Description("Indices of the items to render; every queued item when empty.")] int[]? items = null,
        [Description("Wait for the render (true, the default), or start it and return at once; ae_render_queue then shows the progress and ae_render_control stops or pauses it.")] bool wait = true,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 300,
        CancellationToken cancellationToken = default) =>
        WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.RenderStart, new JsonObject().With("items", items).With("wait", wait), cancellationToken, timeout));

    [McpServerTool(Name = "ae_render_control", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Rendering.Control)]
    [Sends("render_control")]
    public Task<CallToolResult> RenderControlAsync(
        [Description("stop, pause or resume the render that ae_render started without waiting; empty to only set notify or show.")] string? action = null,
        [Description("Have After Effects notify when the queue finishes.")] bool? notify = null,
        [Description("Bring the Render Queue panel to the front.")] bool show = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderControl, new JsonObject().With("action", action).With("notify", notify).With("show", show), cancellationToken);

    [McpServerTool(Name = "ae_save_render_template", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.SaveTemplate)]
    [Sends("render_template_save")]
    public Task<CallToolResult> SaveRenderTemplateAsync(
        [Description("Name of the new template.")] string name,
        [Description("render saves the item's render settings; output (the default) saves its output module.")] string kind = "output",
        [Description(ToolDescriptions.Parameters.RenderItem)] int item = 1,
        [Description("Which output module of the item, 1 first.")] int? outputModule = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderTemplateSave, new JsonObject().With("name", name).With("kind", kind).With("item", item).With("output_module", outputModule), cancellationToken);

    [McpServerTool(Name = "ae_watch_folder", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.WatchFolder)]
    [Sends("watch_folder")]
    public Task<CallToolResult> WatchFolderAsync(
        [Description("Folder to watch for render control files.")] string? path = null,
        [Description("Stop watching.")] bool stop = false,
        [Description("Pause (true) or resume (false) the watch folder.")] bool? pause = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.WatchFolder, new JsonObject().With("path", path).With("stop", stop).With("pause", pause), cancellationToken);

    [McpServerTool(Name = "ae_render_in_media_encoder", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Rendering.InAme)]
    [Sends("render_in_ame")]
    public Task<CallToolResult> RenderInMediaEncoderAsync(
        [Description("Start rendering in Media Encoder at once rather than only queueing.")] bool renderImmediately = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RenderInAme, new JsonObject().With("render_immediately", renderImmediately), cancellationToken);
}
