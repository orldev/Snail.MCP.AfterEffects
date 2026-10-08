namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Capture
    {
        public const string Frame =
            "A frame of a composition as a picture, scaled to width and laid over the composition's background colour (After Effects renders the " +
            "background as transparency), returned as image content next to the path of the PNG. Look at it before judging layout, colour or timing.";
    }

    public static class Properties
    {
        public const string Tree =
            "The property tree of a layer, which is where everything After Effects animates lives: Transform, Effects and their parameters, Masks, " +
            "Text, Material Options, Audio. Each node carries its display name, its match name — stable across languages, use it in paths — its " +
            "type, value, and whether it is animated or driven by an expression. Start here before reading or setting a property.";

        public const string Get =
            "One property by path: its value at a time, type, limits, keyframe count, expression and expression error. Any property of any effect is " +
            "reachable this way; ae_property_tree lists the paths.";

        public const string Set =
            "Set any property of a layer by path: transform values, effect parameters, mask settings, text (a string or an object with font, size and " +
            "colours), shapes, markers, or switch a group such as an effect on and off. With time the value becomes a keyframe; an expression can be " +
            "put on or taken off at the same time. The answer is the property as it stands afterwards.";
    }
}
