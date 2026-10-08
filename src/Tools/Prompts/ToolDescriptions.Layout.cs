namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Layout
    {
        public const string Bounds =
            "The pixel bounds of a layer at a time: the rectangle in the layer's own space (what sourceRectAtTime measures, text and shapes " +
            "included) and, for a 2D layer, its four corners, centre and bounding box in composition pixels after transform. Use it to align " +
            "and fit layers instead of guessing sizes.";

        public const string ConvertPoint =
            "Convert a point between a layer's own pixels and the composition's, at a time: where a corner of a text lands on screen, or which " +
            "pixel of a footage sits under a composition point.";

        public const string Selection =
            "What the user has selected in a composition: the layers, and the properties with the layer and path of each; the composition's " +
            "current time comes along.";

        public const string Guides = "The guides of a composition or a footage item: index, orientation and position in its pixels.";

        public const string SetGuides =
            "Change the guides of a composition or footage item in one call: add guides by orientation and position, move or remove them by " +
            "index, or clear them all. The answer lists the guides afterwards.";

        public const string Split =
            "Split a layer at a time into two, as Edit > Split Layer does: the part before keeps the name and goes below, the part after sits above. " +
            "The answer carries both with their in and out points.";

        public const string CopyToComp =
            "Copy a layer with everything on it into another composition, at the top of its stack; the original stays.";

        public const string PlaceCorners =
            "Place a 2D or 3D layer so that its top-left, top-right and bottom-left corners land on three points of the composition, as " +
            "calculateTransformFromPoints solves it; the transform is applied unless apply is false, and returned either way.";

        public const string FindLayer =
            "Find a layer anywhere in the project by its id, which ae_layer_info and ae_list_layers report, and answer with its composition and " +
            "its full description; ids survive renames and reordering.";
    }

    public static partial class Parameters
    {
        public const string GuideItem = "The composition or footage item by name or id; the active composition when empty.";

        public const string Corner = "[x, y] or [x, y, z] in composition pixels.";
    }
}
