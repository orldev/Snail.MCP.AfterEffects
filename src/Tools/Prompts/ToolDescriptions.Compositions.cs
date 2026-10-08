namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Compositions
    {
        public const string Create =
            "Create a composition with a size, duration and frame rate; 1920×1080 at 30 fps for 10 seconds unless told otherwise. It opens in the viewer, " +
            "and later calls that name no composition work on it.";

        public const string Update =
            "Change a composition's settings: name, size, duration, frame rate, pixel aspect, background colour, current time, work area, motion blur " +
            "and shutter angle, shy layers, or its folder. Only what is passed changes.";

        public const string Duplicate = "Duplicate a composition, optionally under a new name; the layers keep pointing at the same footage and nested compositions.";

        public const string Open =
            "Open a composition in the viewer so the user sees it, and set its current time; later calls that name no composition work on it.";
    }

    public static class Layers
    {
        public const string List =
            "The layers of a composition from top to bottom with index, kind, switches, in and out points, parent and source; filter by kind or name. " +
            "The index is what later calls take, and it changes when layers are added, removed or moved.";

        public const string Info =
            "Everything about one layer: kind, switches, timing, parent, blending mode, transform values with which of them are animated, effects, " +
            "mask count, and the text of a text layer.";

        public const string Add =
            "Add a layer to a composition: a solid, null or adjustment layer, a text layer (a box when width and height are given), an empty shape layer, " +
            "a camera or light, or a footage item or nested composition from the project. New layers go to the top unless at says otherwise; the " +
            "answer carries the index later calls use.";

        public const string Update =
            "Change a layer's switches and timing: name, video, solo, shy, lock, in and out points, start time, stretch, parent, blending mode, 3D, " +
            "motion blur, adjustment, guide, label, comment, quality, audio, collapse, time remapping, track matte, auto-orient, preserve " +
            "transparency, effects and frame blending. Only what is passed changes.";

        public const string Transform =
            "Set a layer's anchor point, position, scale, rotation, opacity and, for 3D layers, orientation and X/Y rotation. A property that already " +
            "has keyframes gets a keyframe at time instead of a value everywhere; the answer shows the transform and which of it is animated.";

        public const string Remove = "Delete one or more layers from a composition; the footage they used stays in the project.";

        public const string Duplicate = "Duplicate a layer right above the original, with its effects, masks and keyframes, optionally under a new name.";

        public const string Move = "Move a layer to an index, to the top or bottom, or right above or below another layer.";

        public const string Select = "Select layers in the timeline, which is what menu commands and the user's next action act on.";
    }
}
