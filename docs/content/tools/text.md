# Text

Part of the `text` skill: these tools appear once it is loaded — by `ae_enable_skill` with `text`, or by `find_tool` when a search returns one of them.

## <code>ae_<wbr>text_<wbr>info</code>

Everything about a text layer: the document with its text, font, size, colours, character and paragraph settings, whether it is point or box text, its paragraphs and composed lines as character ranges, the source text keyframes, every animator with its properties and selectors (with the paths the keyframe tools take), and the path options when the text runs along a mask.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>text_<wbr>style</code>

Style a text layer: the text itself, the font (by PostScript name, or by family and style as ae_text_fonts lists them), size, fill and stroke, justification, tracking, kerning, leading, baseline shift, faux bold and italic, caps, horizontal and vertical scale, paragraph margins and indents, box size, and the axes of a variable font. With start and end, paragraph or line only those characters change. The answer is the document as it stands afterwards; ae_text_layout has the paragraph direction, composer and box settings.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Style the document at this time, as a keyframe when the source text is animated. |
| `start` | `integer?` | — | `null` | First character the style applies to, 0 first; the whole text when start and end are empty. |
| `end` | `integer?` | — | `null` | Character after the last one the style applies to. |
| `paragraph` | `integer?` | — | `null` | Change one paragraph, 1 first; ae_text_info lists them. |
| `paragraphEnd` | `integer?` | — | `null` | The last paragraph the change applies to, with paragraph. |
| `line` | `integer?` | — | `null` | Style one composed line, as the text wraps, 1 first; ae_text_info lists them. |
| `lineEnd` | `integer?` | — | `null` | The last composed line the style applies to, with line. |
| `text` | `string?` | — | `null` | The new text. |
| `font` | `string?` | — | `null` | Font by PostScript name, as ae_text_fonts gives it. |
| `fontFamily` | `string?` | — | `null` | Font family name; pair with fontStyle. |
| `fontStyle` | `string?` | — | `null` | Font style within the family, Regular when empty. |
| `fontAxes` | `any` | — | `null` | For a variable font: the axis values as an object by tag, such as {"wght": 700, "wdth": 90}; axes left out keep the font's defaults. ae_text_fonts with variable lists the axes and their ranges. |
| `fontSize` | `number?` | — | `null` | Font size in pixels. |
| `fillColor` | `any` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `strokeColor` | `any` | — | `null` | [r, g, b] with components 0 to 1, or #rrggbb. |
| `strokeWidth` | `number?` | — | `null` | Stroke width in pixels. |
| `strokeOverFill` | `boolean?` | — | `null` | Draw the stroke over the fill rather than under it. |
| `applyFill` | `boolean?` | — | `null` | Fill the characters at all. |
| `applyStroke` | `boolean?` | — | `null` | Stroke the characters at all. |
| `justification` | `string?` | — | `null` | LEFT_JUSTIFY, CENTER_JUSTIFY, RIGHT_JUSTIFY, FULL_JUSTIFY_LASTLINE_LEFT, FULL_JUSTIFY_LASTLINE_CENTER, FULL_JUSTIFY_LASTLINE_RIGHT or FULL_JUSTIFY_LASTLINE_FULL. |
| `tracking` | `number?` | — | `null` | Tracking in thousandths of an em. |
| `leading` | `number?` | — | `null` | Leading in pixels; switches auto leading off. |
| `autoLeading` | `boolean?` | — | `null` | Let the leading follow the font size. |
| `baselineShift` | `number?` | — | `null` | Baseline shift in pixels. |
| `fauxBold` | `boolean?` | — | `null` | Faux bold. |
| `fauxItalic` | `boolean?` | — | `null` | Faux italic. |
| `allCaps` | `boolean?` | — | `null` | All caps. |
| `smallCaps` | `boolean?` | — | `null` | Small caps. |
| `superscript` | `boolean?` | — | `null` | Superscript. |
| `subscript` | `boolean?` | — | `null` | Subscript. |
| `horizontalScale` | `number?` | — | `null` | Horizontal scale in percent. |
| `verticalScale` | `number?` | — | `null` | Vertical scale in percent. |
| `tsume` | `number?` | — | `null` | Tsume, the proportional spacing of CJK characters, in percent. |
| `ligature` | `boolean?` | — | `null` | Use ligatures. |
| `firstLineIndent` | `number?` | — | `null` | First line indent in pixels. |
| `leftMargin` | `number?` | — | `null` | Left margin in pixels. |
| `rightMargin` | `number?` | — | `null` | Right margin in pixels. |
| `spaceBefore` | `number?` | — | `null` | Space before each paragraph in pixels. |
| `spaceAfter` | `number?` | — | `null` | Space after each paragraph in pixels. |
| `boxSize` | `array?` | — | `null` | For box text: [width, height] of the box in pixels. |
| `kerning` | `number?` | — | `null` | Manual kerning between the character and the next, in thousandths of an em. |
| `noBreak` | `boolean?` | — | `null` | Keep the characters from breaking across lines. |
| `autoKern` | `string?` | — | `null` | Automatic kerning: METRIC_KERN, OPTICAL_KERN or NO_AUTO_KERN. |
| `caps` | `string?` | — | `null` | Caps: FONT_NORMAL_CAPS, FONT_SMALL_CAPS, FONT_ALL_CAPS or FONT_ALL_SMALL_CAPS. |
| `baselineOption` | `string?` | — | `null` | Baseline: FONT_NORMAL_BASELINE, FONT_FAUXED_SUPERSCRIPT or FONT_FAUXED_SUBSCRIPT. |
| `lineJoin` | `string?` | — | `null` | Stroke line join: LINE_JOIN_MITER, LINE_JOIN_ROUND or LINE_JOIN_BEVEL. |
| `baselineDirection` | `string?` | — | `null` | In vertical text: BASELINE_WITH_STREAM, BASELINE_VERTICAL_ROTATED or BASELINE_VERTICAL_CROSS_STREAM. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Style the document at this time, as a keyframe when the source text is animated.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "start": {
      "description": "First character the style applies to, 0 first; the whole text when start and end are empty.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "end": {
      "description": "Character after the last one the style applies to.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "paragraph": {
      "description": "Change one paragraph, 1 first; ae_text_info lists them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "paragraphEnd": {
      "description": "The last paragraph the change applies to, with paragraph.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "line": {
      "description": "Style one composed line, as the text wraps, 1 first; ae_text_info lists them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "lineEnd": {
      "description": "The last composed line the style applies to, with line.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "text": {
      "description": "The new text.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "font": {
      "description": "Font by PostScript name, as ae_text_fonts gives it.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "fontFamily": {
      "description": "Font family name; pair with fontStyle.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "fontStyle": {
      "description": "Font style within the family, Regular when empty.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "fontAxes": {
      "description": "For a variable font: the axis values as an object by tag, such as {\u0022wght\u0022: 700, \u0022wdth\u0022: 90}; axes left out keep the font\u0027s defaults. ae_text_fonts with variable lists the axes and their ranges.",
      "default": null
    },
    "fontSize": {
      "description": "Font size in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "fillColor": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "default": null
    },
    "strokeColor": {
      "description": "[r, g, b] with components 0 to 1, or #rrggbb.",
      "default": null
    },
    "strokeWidth": {
      "description": "Stroke width in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "strokeOverFill": {
      "description": "Draw the stroke over the fill rather than under it.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "applyFill": {
      "description": "Fill the characters at all.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "applyStroke": {
      "description": "Stroke the characters at all.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "justification": {
      "description": "LEFT_JUSTIFY, CENTER_JUSTIFY, RIGHT_JUSTIFY, FULL_JUSTIFY_LASTLINE_LEFT, FULL_JUSTIFY_LASTLINE_CENTER, FULL_JUSTIFY_LASTLINE_RIGHT or FULL_JUSTIFY_LASTLINE_FULL.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "tracking": {
      "description": "Tracking in thousandths of an em.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "leading": {
      "description": "Leading in pixels; switches auto leading off.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "autoLeading": {
      "description": "Let the leading follow the font size.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "baselineShift": {
      "description": "Baseline shift in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "fauxBold": {
      "description": "Faux bold.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "fauxItalic": {
      "description": "Faux italic.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "allCaps": {
      "description": "All caps.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "smallCaps": {
      "description": "Small caps.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "superscript": {
      "description": "Superscript.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "subscript": {
      "description": "Subscript.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "horizontalScale": {
      "description": "Horizontal scale in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "verticalScale": {
      "description": "Vertical scale in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "tsume": {
      "description": "Tsume, the proportional spacing of CJK characters, in percent.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "ligature": {
      "description": "Use ligatures.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "firstLineIndent": {
      "description": "First line indent in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "leftMargin": {
      "description": "Left margin in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "rightMargin": {
      "description": "Right margin in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "spaceBefore": {
      "description": "Space before each paragraph in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "spaceAfter": {
      "description": "Space after each paragraph in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "boxSize": {
      "description": "For box text: [width, height] of the box in pixels.",
      "type": [
        "array",
        "null"
      ],
      "items": {
        "type": "number"
      },
      "default": null
    },
    "kerning": {
      "description": "Manual kerning between the character and the next, in thousandths of an em.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "noBreak": {
      "description": "Keep the characters from breaking across lines.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "autoKern": {
      "description": "Automatic kerning: METRIC_KERN, OPTICAL_KERN or NO_AUTO_KERN.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "caps": {
      "description": "Caps: FONT_NORMAL_CAPS, FONT_SMALL_CAPS, FONT_ALL_CAPS or FONT_ALL_SMALL_CAPS.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "baselineOption": {
      "description": "Baseline: FONT_NORMAL_BASELINE, FONT_FAUXED_SUPERSCRIPT or FONT_FAUXED_SUBSCRIPT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "lineJoin": {
      "description": "Stroke line join: LINE_JOIN_MITER, LINE_JOIN_ROUND or LINE_JOIN_BEVEL.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "baselineDirection": {
      "description": "In vertical text: BASELINE_WITH_STREAM, BASELINE_VERTICAL_ROTATED or BASELINE_VERTICAL_CROSS_STREAM.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>text_<wbr>layout</code>

Paragraph and box settings of a text layer: direction, leading type, composer engine, digit set, hyphenation, hanging punctuation, start and end indents, line orientation, and for box text the auto-fit policy, vertical alignment, first baseline and inset; or reset the character or paragraph styles. With paragraph only that paragraph changes.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `time` | `number?` | — | `null` | Change the document at this time, as a keyframe when the source text is animated. |
| `paragraph` | `integer?` | — | `null` | Change one paragraph, 1 first; ae_text_info lists them. |
| `paragraphEnd` | `integer?` | — | `null` | The last paragraph the change applies to, with paragraph. |
| `direction` | `string?` | — | `null` | DIRECTION_LEFT_TO_RIGHT or DIRECTION_RIGHT_TO_LEFT. |
| `leadingType` | `string?` | — | `null` | ROMAN_LEADING_TYPE or JAPANESE_LEADING_TYPE. |
| `composer` | `string?` | — | `null` | LATIN_CJK_ENGINE or UNIVERSAL_TYPE_ENGINE, the latter for Arabic, Hebrew and Indic scripts. |
| `everyLineComposer` | `boolean?` | — | `null` | Compose every line of a paragraph together for evener breaks. |
| `digitSet` | `string?` | — | `null` | DEFAULT_DIGITS, ARABIC_DIGITS, ARABIC_DIGITS_RTL, HINDI_DIGITS or FARSI_DIGITS. |
| `autoHyphenate` | `boolean?` | — | `null` | Hyphenate automatically. |
| `hangingRoman` | `boolean?` | — | `null` | Hang Roman punctuation outside the margins. |
| `startIndent` | `number?` | — | `null` | Indent from the start of the line in pixels. |
| `endIndent` | `number?` | — | `null` | Indent from the end of the line in pixels. |
| `lineOrientation` | `string?` | — | `null` | HORIZONTAL, VERTICAL_RIGHT_TO_LEFT or VERTICAL_LEFT_TO_RIGHT. |
| `boxAutoFit` | `string?` | — | `null` | For box text, how the box grows with the text: NONE, HEIGHT_CURSOR, HEIGHT_PRECISE_BOUNDS or HEIGHT_BASELINE. |
| `boxVerticalAlignment` | `string?` | — | `null` | For box text: TOP, CENTER, BOTTOM or JUSTIFY. |
| `boxFirstBaseline` | `string?` | — | `null` | For box text, where the first baseline sits: ASCENT, CAP_HEIGHT, LEADING, X_HEIGHT, EM_BOX, LEGACY_METRIC, TYPO_ASCENT, MINIMUM_VALUE_ROMAN or MINIMUM_VALUE_ASIAN. |
| `boxFirstBaselineMinimum` | `number?` | — | `null` | For box text: the least distance of the first baseline from the top, in pixels. |
| `boxInset` | `number?` | — | `null` | For box text: inset of the text from the box edges, in pixels. |
| `reset` | `string?` | — | `null` | Reset the styles to the defaults first: character, paragraph or both. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "time": {
      "description": "Change the document at this time, as a keyframe when the source text is animated.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "paragraph": {
      "description": "Change one paragraph, 1 first; ae_text_info lists them.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "paragraphEnd": {
      "description": "The last paragraph the change applies to, with paragraph.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "direction": {
      "description": "DIRECTION_LEFT_TO_RIGHT or DIRECTION_RIGHT_TO_LEFT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "leadingType": {
      "description": "ROMAN_LEADING_TYPE or JAPANESE_LEADING_TYPE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "composer": {
      "description": "LATIN_CJK_ENGINE or UNIVERSAL_TYPE_ENGINE, the latter for Arabic, Hebrew and Indic scripts.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "everyLineComposer": {
      "description": "Compose every line of a paragraph together for evener breaks.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "digitSet": {
      "description": "DEFAULT_DIGITS, ARABIC_DIGITS, ARABIC_DIGITS_RTL, HINDI_DIGITS or FARSI_DIGITS.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "autoHyphenate": {
      "description": "Hyphenate automatically.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "hangingRoman": {
      "description": "Hang Roman punctuation outside the margins.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "startIndent": {
      "description": "Indent from the start of the line in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "endIndent": {
      "description": "Indent from the end of the line in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "lineOrientation": {
      "description": "HORIZONTAL, VERTICAL_RIGHT_TO_LEFT or VERTICAL_LEFT_TO_RIGHT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "boxAutoFit": {
      "description": "For box text, how the box grows with the text: NONE, HEIGHT_CURSOR, HEIGHT_PRECISE_BOUNDS or HEIGHT_BASELINE.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "boxVerticalAlignment": {
      "description": "For box text: TOP, CENTER, BOTTOM or JUSTIFY.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "boxFirstBaseline": {
      "description": "For box text, where the first baseline sits: ASCENT, CAP_HEIGHT, LEADING, X_HEIGHT, EM_BOX, LEGACY_METRIC, TYPO_ASCENT, MINIMUM_VALUE_ROMAN or MINIMUM_VALUE_ASIAN.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "boxFirstBaselineMinimum": {
      "description": "For box text: the least distance of the first baseline from the top, in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "boxInset": {
      "description": "For box text: inset of the text from the box edges, in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "reset": {
      "description": "Reset the styles to the defaults first: character, paragraph or both.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>text_<wbr>fonts</code>

The fonts installed on this machine as After Effects sees them: family, style and PostScript name, with a query to narrow the list, or only one font by PostScript name, family or id, only the missing ones, only variable fonts with their axes, only fonts for a writing script or with glyphs for a text. Use the names it gives in ae_text_style; a font the project asks for but the machine lacks is marked substituted.

*read-only · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `query` | `string?` | — | `null` | Keep only fonts whose family, style or PostScript name contains this, case-insensitive. |
| `limit` | `integer` | — | `50` | Most fonts to list; 50 by default. |
| `postscriptName` | `string?` | — | `null` | Only the font with this PostScript name. |
| `id` | `integer?` | — | `null` | Only the font with this id, as detail gives it. |
| `family` | `string?` | — | `null` | Only this family, with face to pick one style. |
| `face` | `string?` | — | `null` | The style within family, such as Bold. |
| `missing` | `boolean` | — | `false` | Only the fonts the project asks for and the machine lacks or substitutes. |
| `variable` | `boolean` | — | `false` | Only variable fonts, with their axes. |
| `script` | `string?` | — | `null` | Only fonts that write this script: a CTScript name such as CT_CYRILLIC_SCRIPT, CT_ARABIC_SCRIPT or CT_JAPANESE_SCRIPT. |
| `glyphsFor` | `string?` | — | `null` | Only fonts with glyphs for every character of this text; the answer also says which scripts the text uses and the default font for each. |
| `detail` | `boolean` | — | `false` | Every detail per font: id, full name, file, version, type and writing scripts. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "query": {
      "description": "Keep only fonts whose family, style or PostScript name contains this, case-insensitive.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "limit": {
      "description": "Most fonts to list; 50 by default.",
      "type": "integer",
      "default": 50
    },
    "postscriptName": {
      "description": "Only the font with this PostScript name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "id": {
      "description": "Only the font with this id, as detail gives it.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    },
    "family": {
      "description": "Only this family, with face to pick one style.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "face": {
      "description": "The style within family, such as Bold.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "missing": {
      "description": "Only the fonts the project asks for and the machine lacks or substitutes.",
      "type": "boolean",
      "default": false
    },
    "variable": {
      "description": "Only variable fonts, with their axes.",
      "type": "boolean",
      "default": false
    },
    "script": {
      "description": "Only fonts that write this script: a CTScript name such as CT_CYRILLIC_SCRIPT, CT_ARABIC_SCRIPT or CT_JAPANESE_SCRIPT.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "glyphsFor": {
      "description": "Only fonts with glyphs for every character of this text; the answer also says which scripts the text uses and the default font for each.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "detail": {
      "description": "Every detail per font: id, full name, file, version, type and writing scripts.",
      "type": "boolean",
      "default": false
    }
  }
}
```

</details>

## <code>ae_<wbr>add_<wbr>text_<wbr>animator</code>

Add a text animator to a text layer: the properties it animates (opacity, position, scale, tracking, fill colour and the rest, with their values) and a selector that says which characters, words or lines it reaches. Keyframe the selector's start, end or offset with ae_set_keyframes on the paths the answer gives, and the characters animate one after another.

*only adds · every call adds · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | Name of the animator. |
| `properties` | `object?` | — | `null` | The properties the animator carries and their values, as an object: anchor_point, position, scale, skew, skew_axis, rotation, x_rotation, y_rotation, opacity, fill_color, fill_hue, fill_saturation, fill_brightness, fill_opacity, stroke_color, stroke_hue, stroke_saturation, stroke_brightness, stroke_opacity, stroke_width, tracking, tracking_type, line_anchor, line_spacing, character_offset, character_value, blur; axis_&lt;tag&gt; such as axis_wght animates an axis of the layer's variable font; a match name works too, and null adds the property at its default. |
| `selector` | `object?` | — | `null` | The selector as an object: type (range, the default; wiggly; expression), name, index to address an existing one; for range: start, end, offset (percent, or characters when units is index), units (percent, index), based_on (characters, characters_excluding_spaces, words, lines), mode (add, subtract, intersect, min, max, difference), amount, shape (square, ramp_up, ramp_down, triangle, round, smooth), smoothness, ease_high, ease_low, randomize, seed; for wiggly: max, min, frequency, correlation, temporal_phase, spatial_phase, lock_dimensions, seed; for expression: amount as the expression text. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "name": {
      "description": "Name of the animator.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "properties": {
      "description": "The properties the animator carries and their values, as an object: anchor_point, position, scale, skew, skew_axis, rotation, x_rotation, y_rotation, opacity, fill_color, fill_hue, fill_saturation, fill_brightness, fill_opacity, stroke_color, stroke_hue, stroke_saturation, stroke_brightness, stroke_opacity, stroke_width, tracking, tracking_type, line_anchor, line_spacing, character_offset, character_value, blur; axis_\u003Ctag\u003E such as axis_wght animates an axis of the layer\u0027s variable font; a match name works too, and null adds the property at its default.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "selector": {
      "description": "The selector as an object: type (range, the default; wiggly; expression), name, index to address an existing one; for range: start, end, offset (percent, or characters when units is index), units (percent, index), based_on (characters, characters_excluding_spaces, words, lines), mode (add, subtract, intersect, min, max, difference), amount, shape (square, ramp_up, ramp_down, triangle, round, smooth), smoothness, ease_high, ease_low, randomize, seed; for wiggly: max, min, frequency, correlation, temporal_phase, spatial_phase, lock_dimensions, seed; for expression: amount as the expression text.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

## <code>ae_<wbr>update_<wbr>text_<wbr>animator</code>

Change a text animator: rename, switch on or off, add or set properties, change its selector or add another one. Values on the animator's properties are set as they stand; keyframe them with the animation tools on the paths ae_text_info gives.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `animator` | `string` | yes | — | The animator by its name or its index in the layer's Animators, 1 first. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `name` | `string?` | — | `null` | New name. |
| `enabled` | `boolean?` | — | `null` | Switch the animator on or off. |
| `properties` | `object?` | — | `null` | The properties the animator carries and their values, as an object: anchor_point, position, scale, skew, skew_axis, rotation, x_rotation, y_rotation, opacity, fill_color, fill_hue, fill_saturation, fill_brightness, fill_opacity, stroke_color, stroke_hue, stroke_saturation, stroke_brightness, stroke_opacity, stroke_width, tracking, tracking_type, line_anchor, line_spacing, character_offset, character_value, blur; axis_&lt;tag&gt; such as axis_wght animates an axis of the layer's variable font; a match name works too, and null adds the property at its default. |
| `selector` | `object?` | — | `null` | The selector as an object: type (range, the default; wiggly; expression), name, index to address an existing one; for range: start, end, offset (percent, or characters when units is index), units (percent, index), based_on (characters, characters_excluding_spaces, words, lines), mode (add, subtract, intersect, min, max, difference), amount, shape (square, ramp_up, ramp_down, triangle, round, smooth), smoothness, ease_high, ease_low, randomize, seed; for wiggly: max, min, frequency, correlation, temporal_phase, spatial_phase, lock_dimensions, seed; for expression: amount as the expression text. |
| `addSelector` | `boolean` | — | `false` | Add the selector as a new one instead of changing the existing selector of its type. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "animator": {
      "description": "The animator by its name or its index in the layer\u0027s Animators, 1 first.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "name": {
      "description": "New name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "enabled": {
      "description": "Switch the animator on or off.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "properties": {
      "description": "The properties the animator carries and their values, as an object: anchor_point, position, scale, skew, skew_axis, rotation, x_rotation, y_rotation, opacity, fill_color, fill_hue, fill_saturation, fill_brightness, fill_opacity, stroke_color, stroke_hue, stroke_saturation, stroke_brightness, stroke_opacity, stroke_width, tracking, tracking_type, line_anchor, line_spacing, character_offset, character_value, blur; axis_\u003Ctag\u003E such as axis_wght animates an axis of the layer\u0027s variable font; a match name works too, and null adds the property at its default.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "selector": {
      "description": "The selector as an object: type (range, the default; wiggly; expression), name, index to address an existing one; for range: start, end, offset (percent, or characters when units is index), units (percent, index), based_on (characters, characters_excluding_spaces, words, lines), mode (add, subtract, intersect, min, max, difference), amount, shape (square, ramp_up, ramp_down, triangle, round, smooth), smoothness, ease_high, ease_low, randomize, seed; for wiggly: max, min, frequency, correlation, temporal_phase, spatial_phase, lock_dimensions, seed; for expression: amount as the expression text.",
      "type": [
        "object",
        "null"
      ],
      "default": null
    },
    "addSelector": {
      "description": "Add the selector as a new one instead of changing the existing selector of its type.",
      "type": "boolean",
      "default": false
    }
  },
  "required": [
    "layer",
    "animator"
  ]
}
```

</details>

## <code>ae_<wbr>remove_<wbr>text_<wbr>animator</code>

Remove a text animator, or only one of its properties or selectors when one is named.

*may overwrite or delete · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `animator` | `string` | yes | — | The animator by its name or its index in the layer's Animators, 1 first. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `property` | `string?` | — | `null` | Remove only this property of the animator, by alias or match name. |
| `selector` | `integer?` | — | `null` | Remove only this selector of the animator, by index. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "animator": {
      "description": "The animator by its name or its index in the layer\u0027s Animators, 1 first.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "property": {
      "description": "Remove only this property of the animator, by alias or match name.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "selector": {
      "description": "Remove only this selector of the animator, by index.",
      "type": [
        "integer",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer",
    "animator"
  ]
}
```

</details>

## <code>ae_<wbr>text_<wbr>path</code>

Run the text of a layer along one of its masks, or take it off the path: which mask, reversed, perpendicular to the path, forced alignment, and the first and last margins. Draw the mask on the text layer first with ae_set_property on Masks.

*only adds · repeat-safe · stays inside After Effects.*

| Parameter | Type | Required | Default | Description |
| --- | --- | --- | --- | --- |
| `layer` | `string` | yes | — | The text layer by name or index. |
| `comp` | `string?` | — | `null` | The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project. |
| `mask` | `string?` | — | `null` | The mask on the layer the text follows, by name or index; the word none takes the text off its path. |
| `reverse` | `boolean?` | — | `null` | Run the text the other way along the path. |
| `perpendicular` | `boolean?` | — | `null` | Stand the characters perpendicular to the path. |
| `forceAlignment` | `boolean?` | — | `null` | Spread the text between the first and last margins. |
| `firstMargin` | `number?` | — | `null` | Where the text starts along the path, in pixels. |
| `lastMargin` | `number?` | — | `null` | Where the text ends along the path, in pixels. |

<details><summary>JSON Schema</summary>

```json
{
  "type": "object",
  "properties": {
    "layer": {
      "description": "The text layer by name or index.",
      "type": "string"
    },
    "comp": {
      "description": "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "mask": {
      "description": "The mask on the layer the text follows, by name or index; the word none takes the text off its path.",
      "type": [
        "string",
        "null"
      ],
      "default": null
    },
    "reverse": {
      "description": "Run the text the other way along the path.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "perpendicular": {
      "description": "Stand the characters perpendicular to the path.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "forceAlignment": {
      "description": "Spread the text between the first and last margins.",
      "type": [
        "boolean",
        "null"
      ],
      "default": null
    },
    "firstMargin": {
      "description": "Where the text starts along the path, in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    },
    "lastMargin": {
      "description": "Where the text ends along the path, in pixels.",
      "type": [
        "number",
        "null"
      ],
      "default": null
    }
  },
  "required": [
    "layer"
  ]
}
```

</details>

