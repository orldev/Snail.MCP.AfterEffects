namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Shapes
    {
        public const string Contents =
            "The contents of a shape layer as a tree: groups with their transform, the shapes (rectangle, ellipse, star, path), the paint (fill, " +
            "stroke, gradients) and the operators (trim paths, repeater, merge, offset, round corners, pucker and bloat, twist, wiggle, zig zag), each " +
            "with its settings by plain name and the path the other tools take.";

        public const string Add =
            "Add an item to a shape layer: a group, a rectangle, ellipse, star or path, a fill or stroke, or an operator, into the layer's contents " +
            "or a group, with its settings. A shape added at the top level with fill or stroke gets its own group with the paint, which is how " +
            "After Effects draws it; the answer carries the paths to animate it with the keyframe tools.";

        public const string Update =
            "Change an item of a shape layer by path: rename, switch on or off, set any of its settings (a group's transform when it is a group), " +
            "or move it to another position in its group. With time a setting becomes a keyframe.";

        public const string Remove = "Remove an item of a shape layer by path, with everything inside it.";

        public const string ListMasks =
            "The masks of a layer: name, mode, inverted, feather, opacity, expansion, colour, how many vertices the path has and whether it is " +
            "animated. Any layer with pixels can carry masks; text runs along one with ae_text_path.";

        public const string AddMask =
            "Add a mask to a layer: a rectangle or an ellipse from [x, y, width, height] in layer pixels, or a path from vertices with tangents; with " +
            "mode, inverted, feather, opacity and expansion. The answer names the mask for later changes.";

        public const string UpdateMask =
            "Change a mask: a new shape (rectangle, ellipse or vertices, as a keyframe when time is given), name, mode, inverted, feather, opacity, " +
            "expansion, colour, locked, roto bezier.";

        public const string RemoveMask = "Remove a mask from a layer.";
    }

    public static partial class Parameters
    {
        public const string FeatherPoints =
            "Variable-width feather points along the mask path, as an array of objects: segment (1 first), position along it (0 to 1, 0.5 by default), " +
            "radius in pixels, inner (true feathers inward), tension (0 to 1), hold, corner_angle; an empty array removes them all.";

        public const string ShapeLayer = "The shape layer by name or index.";

        public const string ShapeKind =
            "group, rect, ellipse, star, path, fill, stroke, gradient_fill, gradient_stroke, trim, repeater, merge, offset, round_corners, pucker_bloat, twist, " +
            "wiggle_paths, wiggle_transform, or zigzag.";

        public const string ShapeSettings =
            "Settings as an object of plain names: size, position, roundness for rect and ellipse; points, inner_radius, outer_radius, rotation, type (1 star, " +
            "2 polygon) for star; path as {vertices, in_tangents, out_tangents, closed} for path; color, opacity, fill_rule for fill; color, width, opacity, line_cap, " +
            "line_join, miter_limit, dashes [dash, gap, ...], dash_offset for stroke; start, end, offset, type (1 simultaneously, 2 individually) for trim; " +
            "copies, offset, anchor, position, scale, rotation, start_opacity, end_opacity for repeater; amount, radius, angle, center, size, detail, ridges, " +
            "points, frequency, seed for the other operators; anchor, position, scale, skew, skew_axis, rotation, opacity for a group. A match name works too.";

        public const string ShapePath = "The item by path as ae_shape_contents gives it.";

        public const string Mask = "The mask by name or index, 1 first.";

        public const string MaskVertices = "Vertices of the path as [[x, y], ...] in layer pixels.";

        public const string MaskRectangle = "A rectangle as [x, y, width, height] in layer pixels.";

        public const string MaskEllipse = "An ellipse inside [x, y, width, height] in layer pixels.";

        public const string MaskMode = "NONE, ADD, SUBTRACT, INTERSECT, LIGHTEN, DARKEN or DIFFERENCE.";
    }
}
