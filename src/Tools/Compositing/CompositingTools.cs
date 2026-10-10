using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Compositing;

/// <summary>Precomposing, nested compositions, footage, proxies and the project's files: the compositing skill.</summary>
[Skill(Skills.Compositing, SkillDescriptions.Compositing)]
[McpServerToolType]
internal sealed class CompositingTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_precompose", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.Precompose)]
    [Sends("precompose")]
    public Task<CallToolResult> PrecomposeAsync(
        [Description("Layers to move into the new composition, each by name or index.")] string[] layers,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name of the new composition.")] string? name = null,
        [Description("Move the layers' attributes into the new composition (true, the default) or leave them on the new layer (one layer only).")] bool moveAttributes = true,
        [Description("Open the new composition in the viewer.")] bool open = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.Precompose,
            new JsonObject().WithReference("comp", comp).WithReferences("layers", layers).With("name", name).With("move_attributes", moveAttributes).With("open", open),
            cancellationToken);

    [McpServerTool(Name = "ae_comp_tree", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.Tree)]
    [Sends("comp_tree")]
    public Task<CallToolResult> CompTreeAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("How many levels of nesting to open; 4 by default.")] int depth = 4,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CompTree, new JsonObject().WithReference("comp", comp).With("depth", depth), cancellationToken);

    [McpServerTool(Name = "ae_replace_footage", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Compositing.ReplaceFootage)]
    [Sends("footage_replace")]
    public Task<CallToolResult> ReplaceFootageAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        [Description("Path of the new file; the first frame when sequence is true.")] string? file = null,
        [Description("Treat the file as the start of an image sequence.")] bool sequence = false,
        [Description("For a sequence: take the frames in alphabetical order rather than by number.")] bool forceAlphabetical = false,
        [Description("Replace with a solid of this colour instead of a file.")] JsonNode? solidColor = null,
        [Description("Replace with a placeholder instead of a file.")] bool placeholder = false,
        [Description("Name for the solid or placeholder; the item's name when empty.")] string? name = null,
        [Description("Width of the solid or placeholder; the item's when empty.")] int? width = null,
        [Description("Height of the solid or placeholder; the item's when empty.")] int? height = null,
        [Description("Duration of the placeholder in seconds.")] double? duration = null,
        [Description("Frame rate of the placeholder.")] double? frameRate = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.FootageReplace,
            new JsonObject().WithReference("item", item).With("file", file).With("sequence", sequence).With("force_alphabetical", forceAlphabetical).With("solid_color", solidColor)
                .With("placeholder", placeholder).With("name", name).With("width", width).With("height", height).With("duration", duration).With("frame_rate", frameRate),
            cancellationToken);

    [McpServerTool(Name = "ae_interpret_footage", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.Interpret)]
    [Sends("footage_interpret")]
    public Task<CallToolResult> InterpretFootageAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        [Description(ToolDescriptions.Parameters.Alpha)] string? alpha = null,
        [Description("Invert the alpha channel.")] bool? invertAlpha = null,
        [Description("The colour the alpha was premultiplied with, as #rrggbb or [r, g, b].")] JsonNode? premulColor = null,
        [Description("Let After Effects guess the alpha interpretation.")] bool guessAlpha = false,
        [Description("Frame rate to conform the footage to.")] double? frameRate = null,
        [Description(ToolDescriptions.Parameters.Fields)] string? fields = null,
        [Description("Separate fields at high quality.")] bool? highQualityFields = null,
        [Description("Pulldown phase to remove, as After Effects names them (OFF, WSSWW, SSWWW, SWWWS, WWWSS, WWSSW, or the advanced phases).")] string? removePulldown = null,
        [Description("Let After Effects guess the 3:2 pulldown.")] bool guessPulldown = false,
        [Description("How many times the footage loops.")] int? loop = null,
        [Description("Pixel aspect ratio, 1 for square pixels.")] double? pixelAspect = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.FootageInterpret,
            new JsonObject().WithReference("item", item).With("alpha", alpha).With("invert_alpha", invertAlpha).With("premul_color", premulColor).With("guess_alpha", guessAlpha)
                .With("frame_rate", frameRate).With("fields", fields).With("high_quality_fields", highQualityFields).With("remove_pulldown", removePulldown)
                .With("guess_pulldown", guessPulldown).With("loop", loop).With("pixel_aspect", pixelAspect),
            cancellationToken);

    [McpServerTool(Name = "ae_set_proxy", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Compositing.Proxy)]
    [Sends("proxy_set")]
    public Task<CallToolResult> SetProxyAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        [Description("Path of the proxy file; the first frame when sequence is true.")] string? file = null,
        [Description("Treat the file as the start of an image sequence.")] bool sequence = false,
        [Description("Remove the proxy.")] bool none = false,
        [Description("Use the proxy (true) or the full item (false); the proxy stays set either way.")] bool? use = null,
        [Description("A solid of this colour as the proxy.")] JsonNode? solidColor = null,
        [Description("A placeholder as the proxy.")] bool placeholder = false,
        [Description("Width of the solid or placeholder proxy.")] int? width = null,
        [Description("Height of the solid or placeholder proxy.")] int? height = null,
        [Description("Frame rate of the placeholder proxy.")] double? frameRate = null,
        [Description("Duration of the placeholder proxy in seconds.")] double? duration = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProxySet,
            new JsonObject().WithReference("item", item).With("file", file).With("sequence", sequence).With("none", none).With("use", use).With("solid_color", solidColor)
                .With("placeholder", placeholder).With("width", width).With("height", height).With("frame_rate", frameRate).With("duration", duration),
            cancellationToken);

    [McpServerTool(Name = "ae_replace_layer_source", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.ReplaceLayerSource)]
    [Sends("layer_source_replace")]
    public Task<CallToolResult> ReplaceLayerSourceAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description("The footage item or composition to put on the layer, by name or id.")] string item,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Rewrite expressions that name the old source so they keep working; true by default.")] bool fixExpressions = true,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerSourceReplace,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("item", item).With("fix_expressions", fixExpressions),
            cancellationToken);

    [McpServerTool(Name = "ae_project_footage", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.ProjectFootage)]
    [Sends("project_footage")]
    public Task<CallToolResult> ProjectFootageAsync(
        [Description("Only footage whose file is missing.")] bool onlyMissing = false,
        [Description("Only footage backed by a file, leaving out solids and placeholders.")] bool onlyFiles = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectFootage, new JsonObject().With("only_missing", onlyMissing).With("only_files", onlyFiles), cancellationToken);

    [McpServerTool(Name = "ae_project_cleanup", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Compositing.Cleanup)]
    [Sends("project_cleanup")]
    public Task<CallToolResult> ProjectCleanupAsync(
        [Description("Remove footage no composition uses.")] bool removeUnused = false,
        [Description("Merge footage items that point to the same file.")] bool consolidate = false,
        [Description("Keep only these items (by name or id) and what they use; everything else goes.")] string[]? reduceTo = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectCleanup,
            new JsonObject().With("remove_unused", removeUnused).With("consolidate", consolidate).WithReferences("reduce_to", reduceTo),
            cancellationToken);

    [McpServerTool(Name = "ae_collect_files", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Compositing.CollectFiles)]
    [Sends("collect_files")]
    public Task<CallToolResult> CollectFilesAsync(
        [Description("Folder to collect into; created when missing.")] string folder,
        [Description("Name of the collected project file without .aep; the project's own name when empty.")] string? name = null,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 300,
        CancellationToken cancellationToken = default) =>
        WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.CollectFiles, new JsonObject().With("folder", folder).With("name", name), cancellationToken, timeout));
}
