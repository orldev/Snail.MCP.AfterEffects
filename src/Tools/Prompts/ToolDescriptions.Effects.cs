namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Effects
    {
        public const string Catalog =
            "The effects this After Effects can apply: display name, match name, category and version, narrowed by a query or a category. Use the " +
            "match name in ae_add_effect; display names are in the application's language.";

        public const string Add =
            "Apply an effect to a layer by match name or display name, with its parameters set in the same call by their names or indices. The answer " +
            "lists every parameter with its value, limits and path, which ae_set_keyframes and ae_set_expression take.";

        public const string Info =
            "The effects on a layer, or one effect in full: every parameter with its type, value at a time, limits, keyframes and expression. " +
            "Popup parameters take a number, 1 for the first option.";

        public const string Update =
            "Change an effect on a layer: rename, switch on or off, set parameters by name or index (as keyframes when time is given; a layer " +
            "parameter takes a layer name or index, null for none), or move it in the stack.";

        public const string Remove = "Remove an effect from a layer.";

        public const string DropdownItems =
            "Write the entries of a Dropdown Menu Control effect's menu, which expressions and templates then read by index; the only popup whose " +
            "items a script may change. ae_effect_info shows a dropdown's current items.";

        public const string FindPresets =
            "Animation presets (.ffx) installed with After Effects and in the user's presets, found by a word in their name or folder: Text, " +
            "Transitions, Backgrounds, Behaviors, Shapes and the rest. The name is what ae_apply_preset takes.";

        public const string ApplyPreset =
            "Apply an animation preset to layers, by the name ae_find_presets gives or a path to an .ffx file. Presets add effects, keyframes, " +
            "expressions or text animators; the answer says how many effects each layer gained, and ae_layer_info shows the rest.";

        public const string Styles =
            "The layer styles on a layer (drop shadow, glows, bevel, satin, overlays, stroke) with every parameter.";

        public const string SetStyle =
            "Add a layer style to a layer, or change one it has: switch it on or off and set its parameters by name (as keyframes when time is given). " +
            "A new style is added through the Layer menu in After Effects' own language, with the layer selected for a moment; when that fails the " +
            "answer says so and the style can be added from Layer > Layer Styles by hand.";

        public const string RemoveStyle = "Remove a layer style from a layer.";
    }

    public static partial class Parameters
    {
        public const string Effect = "The effect by its name on the layer, its match name, or its index in the effects, 1 first.";

        public const string EffectSettings =
            "Parameters as an object keyed by parameter name (as the interface shows it), match name or index; values as ae_set_property takes them: " +
            "numbers, [x, y] points, colours as #rrggbb or [r, g, b], true or false for checkboxes, 1-based option numbers for popups, a layer name for layer parameters.";

        public const string LayerStyle = "drop_shadow, inner_shadow, outer_glow, inner_glow, bevel_emboss, satin, color_overlay, gradient_overlay or stroke.";
    }
}
