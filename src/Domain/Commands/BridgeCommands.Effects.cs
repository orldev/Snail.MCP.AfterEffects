namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand EffectsCatalog = new("effects_catalog", "effects");

    public static readonly BridgeCommand EffectAdd = new("effect_add", "effects");

    public static readonly BridgeCommand EffectInfo = new("effect_info", "effects");

    public static readonly BridgeCommand EffectUpdate = new("effect_update", "effects");

    public static readonly BridgeCommand EffectRemove = new("effect_remove", "effects");

    public static readonly BridgeCommand PresetsFind = new("presets_find", "effects");

    public static readonly BridgeCommand PresetApply = new("preset_apply", "effects");

    public static readonly BridgeCommand LayerStyles = new("layer_styles", "effects");

    public static readonly BridgeCommand LayerStyleSet = new("layer_style_set", "effects");

    public static readonly BridgeCommand LayerStyleRemove = new("layer_style_remove", "effects");

    public static readonly BridgeCommand DropdownItems = new("dropdown_items", "effects");
}
