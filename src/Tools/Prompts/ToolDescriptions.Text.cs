namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Text
    {
        public const string Info =
            "Everything about a text layer: the document with its text, font, size, colours, character and paragraph settings, whether it is point or box " +
            "text, its paragraphs and composed lines as character ranges, the source text keyframes, every animator with its properties and selectors " +
            "(with the paths the keyframe tools take), and the path options when the text runs along a mask.";

        public const string Style =
            "Style a text layer: the text itself, the font (by PostScript name, or by family and style as ae_text_fonts lists them), size, fill and " +
            "stroke, justification, tracking, kerning, leading, baseline shift, faux bold and italic, caps, horizontal and vertical scale, paragraph " +
            "margins and indents, box size, and the axes of a variable font. With start and end, paragraph or line only those characters change. " +
            "The answer is the document as it stands afterwards; ae_text_layout has the paragraph direction, composer and box settings.";

        public const string Layout =
            "Paragraph and box settings of a text layer: direction, leading type, composer engine, digit set, hyphenation, hanging punctuation, start " +
            "and end indents, line orientation, and for box text the auto-fit policy, vertical alignment, first baseline and inset; or reset the " +
            "character or paragraph styles. With paragraph only that paragraph changes.";

        public const string Fonts =
            "The fonts installed on this machine as After Effects sees them: family, style and PostScript name, with a query to narrow the list, " +
            "or only one font by PostScript name, family or id, only the missing ones, only variable fonts with their axes, only fonts for a " +
            "writing script or with glyphs for a text. Use the names it gives in ae_text_style; a font the project asks for but the machine lacks " +
            "is marked substituted.";

        public const string AddAnimator =
            "Add a text animator to a text layer: the properties it animates (opacity, position, scale, tracking, fill colour and the rest, with " +
            "their values) and a selector that says which characters, words or lines it reaches. Keyframe the selector's start, end or offset " +
            "with ae_set_keyframes on the paths the answer gives, and the characters animate one after another.";

        public const string UpdateAnimator =
            "Change a text animator: rename, switch on or off, add or set properties, change its selector or add another one. Values on the " +
            "animator's properties are set as they stand; keyframe them with the animation tools on the paths ae_text_info gives.";

        public const string RemoveAnimator =
            "Remove a text animator, or only one of its properties or selectors when one is named.";

        public const string Path =
            "Run the text of a layer along one of its masks, or take it off the path: which mask, reversed, perpendicular to the path, forced " +
            "alignment, and the first and last margins. Draw the mask on the text layer first with ae_set_property on Masks.";
    }

    public static partial class Parameters
    {
        public const string TextLayer = "The text layer by name or index.";

        public const string AnimatorProperties =
            "The properties the animator carries and their values, as an object: anchor_point, position, scale, skew, skew_axis, rotation, x_rotation, y_rotation, " +
            "opacity, fill_color, fill_hue, fill_saturation, fill_brightness, fill_opacity, stroke_color, stroke_hue, stroke_saturation, stroke_brightness, " +
            "stroke_opacity, stroke_width, tracking, tracking_type, line_anchor, line_spacing, character_offset, character_value, blur; axis_<tag> such as " +
            "axis_wght animates an axis of the layer's variable font; a match name works too, and null adds the property at its default.";

        public const string Selector =
            "The selector as an object: type (range, the default; wiggly; expression), name, index to address an existing one; for range: start, end, offset " +
            "(percent, or characters when units is index), units (percent, index), based_on (characters, characters_excluding_spaces, words, lines), mode " +
            "(add, subtract, intersect, min, max, difference), amount, shape (square, ramp_up, ramp_down, triangle, round, smooth), smoothness, ease_high, " +
            "ease_low, randomize, seed; for wiggly: max, min, frequency, correlation, temporal_phase, spatial_phase, lock_dimensions, seed; for expression: amount " +
            "as the expression text.";

        public const string Animator = "The animator by its name or its index in the layer's Animators, 1 first.";

        public const string Paragraph = "Change one paragraph, 1 first; ae_text_info lists them.";

        public const string ParagraphEnd = "The last paragraph the change applies to, with paragraph.";

        public const string FontAxes =
            "For a variable font: the axis values as an object by tag, such as {\"wght\": 700, \"wdth\": 90}; axes left out keep the font's defaults. " +
            "ae_text_fonts with variable lists the axes and their ranges.";
    }
}
