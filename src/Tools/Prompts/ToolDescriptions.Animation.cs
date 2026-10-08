namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Animation
    {
        public const string ListKeyframes =
            "Every keyframe of a property: time, value, interpolation on each side, ease (speed and influence), and for spatial properties the " +
            "tangents and roving. Read this before easing or editing keys; the indices it gives are what the other keyframe tools take.";

        public const string SetKeyframes =
            "Set keyframes on a property in one call: a list of {time, value} pairs, each optionally with its own easing, plus a preset or " +
            "interpolation, influence and speed that apply to all of them. Existing keys at other times stay unless replace is true. A key at a " +
            "time that already has one replaces it. Values take the same forms as ae_set_property.";

        public const string RemoveKeyframes =
            "Remove keyframes of a property: the ones at the indices or times given, or all of them when neither is. A property with no keys " +
            "left keeps its last value.";

        public const string EaseKeyframes =
            "Change the interpolation and ease of existing keyframes: a preset as the Animation menu offers them, or interpolation per side with " +
            "influence (percent) and speed, continuous and auto bezier tangents, roving for spatial keys. Applies to the keys at the indices or " +
            "times given, or all of them.";

        public const string SetExpression =
            "Put an expression on a property and check it compiles: a broken expression is reported with After Effects' own error message and " +
            "not left on the property, so the answer can be trusted. An empty expression removes the one there; check_only tries the expression " +
            "and restores whatever was there before. The answer carries the value the expression yields at the current time.";

        public const string SeparateDimensions =
            "Separate the dimensions of a position or another multi-dimensional property, so each axis has keyframes of its own, or join them " +
            "again; the answer names the per-axis properties (the followers) the keyframe tools then take by path.";

        public const string ListMarkers =
            "The markers of a composition, or of a layer when one is named: time, comment, duration, label, chapter, URL, cue point name and, " +
            "for composition markers, whether the region is protected.";

        public const string SetMarker =
            "Add a marker on the composition, or on a layer when one is named, at a time with a comment and optional duration, label, chapter, " +
            "URL, frame target and cue point name. A marker already at that time is replaced.";

        public const string RemoveMarkers =
            "Remove markers of the composition, or of a layer when one is named: those at the indices or times given, or all of them.";

        public const string TimeRemap =
            "Time remapping of a footage or nested composition layer: switch it on, which puts keys at the layer's start and end, and set the " +
            "keys that map composition time to source time (both in seconds) for freeze frames, slow motion and reversal. Ease the keys with " +
            "ae_ease_keyframes on the path ADBE Time Remapping; enabled false switches it off.";
    }

    public static partial class Parameters
    {
        public const string KeyIndices = "Keyframe indices as ae_list_keyframes numbers them, 1 first; the keys at times, or all keys, when empty.";

        public const string KeyTimes = "Times in seconds of the keyframes meant, each within half a frame of a key; the keys at indices, or all keys, when empty.";

        public const string EasingPreset = "easy_ease (both sides), ease_in, ease_out, linear, or hold; the explicit settings below override a preset side by side.";

        public const string InInterpolation = "Interpolation into the key: LINEAR, BEZIER or HOLD.";

        public const string OutInterpolation = "Interpolation out of the key: LINEAR, BEZIER or HOLD.";

        public const string InfluenceIn = "Ease influence into the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.";

        public const string InfluenceOut = "Ease influence out of the key, 0.1 to 100 percent; 33.33 is Easy Ease. Setting it makes the side bezier.";

        public const string SpeedIn = "Speed into the key in units per second; 0 comes to a rest, as Easy Ease does.";

        public const string SpeedOut = "Speed out of the key in units per second; 0 starts from rest, as Easy Ease does.";

        public const string Continuous = "Keep the tangents continuous through the key.";

        public const string AutoBezier = "Let After Effects pick the tangents (auto bezier).";

        public const string Roving = "For spatial properties: let the key rove in time to keep the speed even along the path.";

        public const string Keys =
            "Keyframes as a list of objects with time (seconds) and value (as ae_set_property takes it); any may also carry its own preset, " +
            "in_interpolation, out_interpolation, influence_in, influence_out, speed_in, speed_out, in_tangent, out_tangent or label.";

        public const string KeyLabel = "Keyframe label colour as a number 0 to 16 as After Effects numbers them; 0 is none.";

        public const string MarkerLayer = "A layer by name or index for layer markers; the composition's own markers when empty.";
    }
}
