using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Effects;

/// <summary>Effects, animation presets and layer styles: the effects skill.</summary>
[Skill(Skills.Effects, SkillDescriptions.Effects)]
[McpServerToolType]
internal sealed class EffectTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_effects_catalog", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Catalog)]
    [Sends("effects_catalog")]
    public Task<CallToolResult> EffectsCatalogAsync(
        [Description("Keep only effects whose name or match name contains this, case-insensitive.")] string? query = null,
        [Description("Keep only effects of this category, as the answer lists them.")] string? category = null,
        [Description("Most effects to list; 60 by default.")] int limit = 60,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.EffectsCatalog, new JsonObject().With("query", query).With("category", category).With("limit", limit), cancellationToken);

    [McpServerTool(Name = "ae_add_effect", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Add)]
    [Sends("effect_add")]
    public Task<CallToolResult> AddEffectAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description("The effect to apply, by match name (ADBE Gaussian Blur 2) or display name as ae_effects_catalog lists them.")] string effect,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name for the effect on the layer; the effect's own name when empty.")] string? name = null,
        [Description(ToolDescriptions.Parameters.EffectSettings)] JsonObject? settings = null,
        [Description("Position in the effect stack, 1 first; at the end when empty.")] int? at = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.EffectAdd,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("effect", effect).With("name", name).With("settings", settings).With("at", at),
            cancellationToken);

    [McpServerTool(Name = "ae_effect_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Info)]
    [Sends("effect_info")]
    public Task<CallToolResult> EffectInfoAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The effect to describe in full; all effects in brief when empty.")] string? effect = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.EffectInfo,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("effect", effect).With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_update_effect", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Update)]
    [Sends("effect_update")]
    public Task<CallToolResult> UpdateEffectAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Effect)] string effect,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("New name.")] string? name = null,
        [Description("Switch the effect on or off.")] bool? enabled = null,
        [Description(ToolDescriptions.Parameters.EffectSettings)] JsonObject? settings = null,
        [Description("Set the parameters as keyframes at this time.")] double? time = null,
        [Description("Move the effect to this position in the stack, 1 first.")] int? at = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.EffectUpdate,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("effect", effect).With("name", name).With("enabled", enabled)
                .With("settings", settings).With("time", time).With("at", at),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_effect", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Remove)]
    [Sends("effect_remove")]
    public Task<CallToolResult> RemoveEffectAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Effect)] string effect,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.EffectRemove, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("effect", effect), cancellationToken);

    [McpServerTool(Name = "ae_set_dropdown_items", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.DropdownItems)]
    [Sends("dropdown_items")]
    public Task<CallToolResult> SetDropdownItemsAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description("Path of the menu property of a Dropdown Menu Control effect, as ae_effect_info lists it.")] string path,
        [Description("The entries of the menu, in order.")] string[] items,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.DropdownItems, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("items", items), cancellationToken);

    [McpServerTool(Name = "ae_find_presets", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.FindPresets)]
    [Sends("presets_find")]
    public Task<CallToolResult> FindPresetsAsync(
        [Description("A word of the preset's name or folder, case-insensitive; everything when empty.")] string? query = null,
        [Description("Most presets to list; 60 by default.")] int limit = 60,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PresetsFind, new JsonObject().With("query", query).With("limit", limit), cancellationToken);

    [McpServerTool(Name = "ae_apply_preset", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Effects.ApplyPreset)]
    [Sends("preset_apply")]
    public Task<CallToolResult> ApplyPresetAsync(
        [Description("Layers to apply the preset to, each by name or index.")] string[] layers,
        [Description("The preset by name as ae_find_presets gives it, or a path to an .ffx file.")] string preset,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PresetApply, new JsonObject().WithReference("comp", comp).WithReferences("layers", layers).With("preset", preset), cancellationToken);

    [McpServerTool(Name = "ae_layer_styles", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.Styles)]
    [Sends("layer_styles")]
    public Task<CallToolResult> LayerStylesAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerStyles, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time), cancellationToken);

    [McpServerTool(Name = "ae_set_layer_style", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.SetStyle)]
    [Sends("layer_style_set")]
    public Task<CallToolResult> SetLayerStyleAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.LayerStyle)] string style,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Switch the style on or off.")] bool? enabled = null,
        [Description(ToolDescriptions.Parameters.EffectSettings)] JsonObject? settings = null,
        [Description("Set the parameters as keyframes at this time.")] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerStyleSet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("style", style).With("enabled", enabled).With("settings", settings).With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_layer_style", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Effects.RemoveStyle)]
    [Sends("layer_style_remove")]
    public Task<CallToolResult> RemoveLayerStyleAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.LayerStyle)] string style,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LayerStyleRemove, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("style", style), cancellationToken);
}
