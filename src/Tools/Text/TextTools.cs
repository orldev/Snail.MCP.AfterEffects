using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Text;

/// <summary>Fonts, the text document, animators and text on a path: the text skill.</summary>
[Skill(Skills.Text, SkillDescriptions.Text)]
[McpServerToolType]
internal sealed class TextTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_text_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.Info)]
    [Sends("text_info")]
    public Task<CallToolResult> TextInfoAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextInfo, new JsonObject().WithReference("comp", comp).WithReference("layer", layer), cancellationToken);

    [McpServerTool(Name = "ae_text_style", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.Style)]
    [Sends("text_style")]
    public Task<CallToolResult> TextStyleAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Style the document at this time, as a keyframe when the source text is animated.")] double? time = null,
        [Description("First character the style applies to, 0 first; the whole text when start and end are empty.")] int? start = null,
        [Description("Character after the last one the style applies to.")] int? end = null,
        [Description(ToolDescriptions.Parameters.Paragraph)] int? paragraph = null,
        [Description(ToolDescriptions.Parameters.ParagraphEnd)] int? paragraphEnd = null,
        [Description("Style one composed line, as the text wraps, 1 first; ae_text_info lists them.")] int? line = null,
        [Description("The last composed line the style applies to, with line.")] int? lineEnd = null,
        [Description("The new text.")] string? text = null,
        [Description("Font by PostScript name, as ae_text_fonts gives it.")] string? font = null,
        [Description("Font family name; pair with fontStyle.")] string? fontFamily = null,
        [Description("Font style within the family, Regular when empty.")] string? fontStyle = null,
        [Description(ToolDescriptions.Parameters.FontAxes)] JsonNode? fontAxes = null,
        [Description("Font size in pixels.")] double? fontSize = null,
        [Description(ToolDescriptions.Parameters.Color)] JsonNode? fillColor = null,
        [Description(ToolDescriptions.Parameters.Color)] JsonNode? strokeColor = null,
        [Description("Stroke width in pixels.")] double? strokeWidth = null,
        [Description("Draw the stroke over the fill rather than under it.")] bool? strokeOverFill = null,
        [Description("Fill the characters at all.")] bool? applyFill = null,
        [Description("Stroke the characters at all.")] bool? applyStroke = null,
        [Description("LEFT_JUSTIFY, CENTER_JUSTIFY, RIGHT_JUSTIFY, FULL_JUSTIFY_LASTLINE_LEFT, FULL_JUSTIFY_LASTLINE_CENTER, FULL_JUSTIFY_LASTLINE_RIGHT or FULL_JUSTIFY_LASTLINE_FULL.")] string? justification = null,
        [Description("Tracking in thousandths of an em.")] double? tracking = null,
        [Description("Leading in pixels; switches auto leading off.")] double? leading = null,
        [Description("Let the leading follow the font size.")] bool? autoLeading = null,
        [Description("Baseline shift in pixels.")] double? baselineShift = null,
        [Description("Faux bold.")] bool? fauxBold = null,
        [Description("Faux italic.")] bool? fauxItalic = null,
        [Description("All caps.")] bool? allCaps = null,
        [Description("Small caps.")] bool? smallCaps = null,
        [Description("Superscript.")] bool? superscript = null,
        [Description("Subscript.")] bool? subscript = null,
        [Description("Horizontal scale in percent.")] double? horizontalScale = null,
        [Description("Vertical scale in percent.")] double? verticalScale = null,
        [Description("Tsume, the proportional spacing of CJK characters, in percent.")] double? tsume = null,
        [Description("Use ligatures.")] bool? ligature = null,
        [Description("First line indent in pixels.")] double? firstLineIndent = null,
        [Description("Left margin in pixels.")] double? leftMargin = null,
        [Description("Right margin in pixels.")] double? rightMargin = null,
        [Description("Space before each paragraph in pixels.")] double? spaceBefore = null,
        [Description("Space after each paragraph in pixels.")] double? spaceAfter = null,
        [Description("For box text: [width, height] of the box in pixels.")] double[]? boxSize = null,
        [Description("Manual kerning between the character and the next, in thousandths of an em.")] double? kerning = null,
        [Description("Keep the characters from breaking across lines.")] bool? noBreak = null,
        [Description("Automatic kerning: METRIC_KERN, OPTICAL_KERN or NO_AUTO_KERN.")] string? autoKern = null,
        [Description("Caps: FONT_NORMAL_CAPS, FONT_SMALL_CAPS, FONT_ALL_CAPS or FONT_ALL_SMALL_CAPS.")] string? caps = null,
        [Description("Baseline: FONT_NORMAL_BASELINE, FONT_FAUXED_SUPERSCRIPT or FONT_FAUXED_SUBSCRIPT.")] string? baselineOption = null,
        [Description("Stroke line join: LINE_JOIN_MITER, LINE_JOIN_ROUND or LINE_JOIN_BEVEL.")] string? lineJoin = null,
        [Description("In vertical text: BASELINE_WITH_STREAM, BASELINE_VERTICAL_ROTATED or BASELINE_VERTICAL_CROSS_STREAM.")] string? baselineDirection = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextStyle,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("start", start).With("end", end)
                .With("paragraph", paragraph).With("paragraph_end", paragraphEnd).With("line", line).With("line_end", lineEnd).With("text", text)
                .With("font", font).With("font_family", fontFamily).With("font_style", fontStyle).With("font_axes", fontAxes).With("font_size", fontSize).With("fill_color", fillColor)
                .With("stroke_color", strokeColor).With("stroke_width", strokeWidth).With("stroke_over_fill", strokeOverFill).With("apply_fill", applyFill)
                .With("apply_stroke", applyStroke).With("justification", justification).With("tracking", tracking).With("leading", leading).With("auto_leading", autoLeading)
                .With("baseline_shift", baselineShift).With("faux_bold", fauxBold).With("faux_italic", fauxItalic).With("all_caps", allCaps).With("small_caps", smallCaps)
                .With("superscript", superscript).With("subscript", subscript).With("horizontal_scale", horizontalScale).With("vertical_scale", verticalScale)
                .With("tsume", tsume).With("ligature", ligature).With("first_line_indent", firstLineIndent).With("left_margin", leftMargin).With("right_margin", rightMargin)
                .With("space_before", spaceBefore).With("space_after", spaceAfter).With("box_size", boxSize).With("kerning", kerning).With("no_break", noBreak)
                .With("auto_kern", autoKern).With("caps", caps).With("baseline_option", baselineOption).With("line_join", lineJoin).With("baseline_direction", baselineDirection),
            cancellationToken);

    [McpServerTool(Name = "ae_text_layout", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.Layout)]
    [Sends("text_layout")]
    public Task<CallToolResult> TextLayoutAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Change the document at this time, as a keyframe when the source text is animated.")] double? time = null,
        [Description(ToolDescriptions.Parameters.Paragraph)] int? paragraph = null,
        [Description(ToolDescriptions.Parameters.ParagraphEnd)] int? paragraphEnd = null,
        [Description("DIRECTION_LEFT_TO_RIGHT or DIRECTION_RIGHT_TO_LEFT.")] string? direction = null,
        [Description("ROMAN_LEADING_TYPE or JAPANESE_LEADING_TYPE.")] string? leadingType = null,
        [Description("LATIN_CJK_ENGINE or UNIVERSAL_TYPE_ENGINE, the latter for Arabic, Hebrew and Indic scripts.")] string? composer = null,
        [Description("Compose every line of a paragraph together for evener breaks.")] bool? everyLineComposer = null,
        [Description("DEFAULT_DIGITS, ARABIC_DIGITS, ARABIC_DIGITS_RTL, HINDI_DIGITS or FARSI_DIGITS.")] string? digitSet = null,
        [Description("Hyphenate automatically.")] bool? autoHyphenate = null,
        [Description("Hang Roman punctuation outside the margins.")] bool? hangingRoman = null,
        [Description("Indent from the start of the line in pixels.")] double? startIndent = null,
        [Description("Indent from the end of the line in pixels.")] double? endIndent = null,
        [Description("HORIZONTAL, VERTICAL_RIGHT_TO_LEFT or VERTICAL_LEFT_TO_RIGHT.")] string? lineOrientation = null,
        [Description("For box text, how the box grows with the text: NONE, HEIGHT_CURSOR, HEIGHT_PRECISE_BOUNDS or HEIGHT_BASELINE.")] string? boxAutoFit = null,
        [Description("For box text: TOP, CENTER, BOTTOM or JUSTIFY.")] string? boxVerticalAlignment = null,
        [Description("For box text, where the first baseline sits: ASCENT, CAP_HEIGHT, LEADING, X_HEIGHT, EM_BOX, LEGACY_METRIC, TYPO_ASCENT, MINIMUM_VALUE_ROMAN or MINIMUM_VALUE_ASIAN.")] string? boxFirstBaseline = null,
        [Description("For box text: the least distance of the first baseline from the top, in pixels.")] double? boxFirstBaselineMinimum = null,
        [Description("For box text: inset of the text from the box edges, in pixels.")] double? boxInset = null,
        [Description("Reset the styles to the defaults first: character, paragraph or both.")] string? reset = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextLayout,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("paragraph", paragraph).With("paragraph_end", paragraphEnd)
                .With("direction", direction).With("leading_type", leadingType).With("composer", composer).With("every_line_composer", everyLineComposer).With("digit_set", digitSet)
                .With("auto_hyphenate", autoHyphenate).With("hanging_roman", hangingRoman).With("start_indent", startIndent).With("end_indent", endIndent)
                .With("line_orientation", lineOrientation).With("box_auto_fit", boxAutoFit).With("box_vertical_alignment", boxVerticalAlignment)
                .With("box_first_baseline", boxFirstBaseline).With("box_first_baseline_minimum", boxFirstBaselineMinimum).With("box_inset", boxInset).With("reset", reset),
            cancellationToken);

    [McpServerTool(Name = "ae_text_fonts", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.Fonts)]
    [Sends("text_fonts")]
    public Task<CallToolResult> TextFontsAsync(
        [Description("Keep only fonts whose family, style or PostScript name contains this, case-insensitive.")] string? query = null,
        [Description("Most fonts to list; 50 by default.")] int limit = 50,
        [Description("Only the font with this PostScript name.")] string? postscriptName = null,
        [Description("Only the font with this id, as detail gives it.")] int? id = null,
        [Description("Only this family, with face to pick one style.")] string? family = null,
        [Description("The style within family, such as Bold.")] string? face = null,
        [Description("Only the fonts the project asks for and the machine lacks or substitutes.")] bool missing = false,
        [Description("Only variable fonts, with their axes.")] bool variable = false,
        [Description("Only fonts that write this script: a CTScript name such as CT_CYRILLIC_SCRIPT, CT_ARABIC_SCRIPT or CT_JAPANESE_SCRIPT.")] string? script = null,
        [Description("Only fonts with glyphs for every character of this text; the answer also says which scripts the text uses and the default font for each.")] string? glyphsFor = null,
        [Description("Every detail per font: id, full name, file, version, type and writing scripts.")] bool detail = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextFonts,
            new JsonObject().With("query", query).With("limit", limit).With("postscript_name", postscriptName).With("id", id).With("family", family).With("face", face)
                .With("missing", missing).With("variable", variable).With("script", script).With("glyphs_for", glyphsFor).With("detail", detail),
            cancellationToken);

    [McpServerTool(Name = "ae_add_text_animator", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Text.AddAnimator)]
    [Sends("text_animator_add")]
    public Task<CallToolResult> AddTextAnimatorAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name of the animator.")] string? name = null,
        [Description(ToolDescriptions.Parameters.AnimatorProperties)] JsonObject? properties = null,
        [Description(ToolDescriptions.Parameters.Selector)] JsonObject? selector = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextAnimatorAdd,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("name", name).With("properties", properties).With("selector", selector),
            cancellationToken);

    [McpServerTool(Name = "ae_update_text_animator", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.UpdateAnimator)]
    [Sends("text_animator_update")]
    public Task<CallToolResult> UpdateTextAnimatorAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Animator)] string animator,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("New name.")] string? name = null,
        [Description("Switch the animator on or off.")] bool? enabled = null,
        [Description(ToolDescriptions.Parameters.AnimatorProperties)] JsonObject? properties = null,
        [Description(ToolDescriptions.Parameters.Selector)] JsonObject? selector = null,
        [Description("Add the selector as a new one instead of changing the existing selector of its type.")] bool addSelector = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextAnimatorUpdate,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("animator", animator).With("name", name).With("enabled", enabled)
                .With("properties", properties).With("selector", selector).With("add_selector", addSelector),
            cancellationToken);

    [McpServerTool(Name = "ae_remove_text_animator", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.RemoveAnimator)]
    [Sends("text_animator_remove")]
    public Task<CallToolResult> RemoveTextAnimatorAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Animator)] string animator,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Remove only this property of the animator, by alias or match name.")] string? property = null,
        [Description("Remove only this selector of the animator, by index.")] int? selector = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextAnimatorRemove,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithReference("animator", animator).With("property", property).With("selector", selector),
            cancellationToken);

    [McpServerTool(Name = "ae_text_path", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Text.Path)]
    [Sends("text_path")]
    public Task<CallToolResult> TextPathAsync(
        [Description(ToolDescriptions.Parameters.TextLayer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The mask on the layer the text follows, by name or index; the word none takes the text off its path.")] string? mask = null,
        [Description("Run the text the other way along the path.")] bool? reverse = null,
        [Description("Stand the characters perpendicular to the path.")] bool? perpendicular = null,
        [Description("Spread the text between the first and last margins.")] bool? forceAlignment = null,
        [Description("Where the text starts along the path, in pixels.")] double? firstMargin = null,
        [Description("Where the text ends along the path, in pixels.")] double? lastMargin = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TextPath,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).WithMask(mask).With("reverse", reverse).With("perpendicular", perpendicular)
                .With("force_alignment", forceAlignment).With("first_margin", firstMargin).With("last_margin", lastMargin),
            cancellationToken);
}
