// Text: the document behind a text layer with every attribute scripts reach, paragraphs and lines, variable fonts, fonts and scripts, animators with their selectors, text on a path.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    var STYLE_ATTRIBUTES = [
        ["font", "font"], ["font_size", "fontSize"], ["apply_fill", "applyFill"], ["apply_stroke", "applyStroke"], ["stroke_width", "strokeWidth"],
        ["stroke_over_fill", "strokeOverFill"], ["tracking", "tracking"], ["leading", "leading"], ["auto_leading", "autoLeading"], ["baseline_shift", "baselineShift"],
        ["faux_bold", "fauxBold"], ["faux_italic", "fauxItalic"], ["all_caps", "allCaps"], ["small_caps", "smallCaps"], ["superscript", "superscript"],
        ["subscript", "subscript"], ["horizontal_scale", "horizontalScale"], ["vertical_scale", "verticalScale"], ["tsume", "tsume"], ["ligature", "ligature"],
        ["first_line_indent", "firstLineIndent"], ["left_margin", "leftMargin"], ["right_margin", "rightMargin"], ["space_before", "spaceBefore"], ["space_after", "spaceAfter"],
        ["kerning", "kerning"], ["no_break", "noBreak"]
    ];

    var STYLE_ENUMS = [
        ["auto_kern", "autoKernType", "AutoKernType", "kerning mode"], ["caps", "fontCapsOption", "FontCapsOption", "caps option"],
        ["baseline_option", "fontBaselineOption", "FontBaselineOption", "baseline option"], ["line_join", "lineJoinType", "LineJoinType", "line join"],
        ["baseline_direction", "baselineDirection", "BaselineDirection", "baseline direction"]
    ];

    var PARAGRAPH_ATTRIBUTES = [
        ["auto_hyphenate", "autoHyphenate"], ["every_line_composer", "everyLineComposer"], ["hanging_roman", "hangingRoman"], ["start_indent", "startIndent"],
        ["end_indent", "endIndent"]
    ];

    var PARAGRAPH_ENUMS = [
        ["direction", "direction", "ParagraphDirection", "paragraph direction"], ["leading_type", "leadingType", "LeadingType", "leading type"],
        ["composer", "composerEngine", "ComposerEngine", "composer engine"], ["digit_set", "digitSet", "DigitSet", "digit set"]
    ];

    // Settings of the document as a whole: a paragraph cannot carry them.
    var DOCUMENT_ATTRIBUTES = [["box_inset", "boxInsetSpacing"], ["box_first_baseline_minimum", "boxFirstBaselineAlignmentMinimum"]];

    var DOCUMENT_ENUMS = [
        ["line_orientation", "lineOrientation", "LineOrientation", "line orientation"], ["box_auto_fit", "boxAutoFitPolicy", "BoxAutoFitPolicy", "box auto-fit policy"],
        ["box_vertical_alignment", "boxVerticalAlignment", "BoxVerticalAlignment", "box vertical alignment"],
        ["box_first_baseline", "boxFirstBaselineAlignment", "BoxFirstBaselineAlignment", "first baseline alignment"]
    ];

    function enumerationNamed(name) {
        try { return eval(name); } catch (ignored) { return null; }
    }

    var ANIMATOR_PROPERTIES = {
        anchor_point: "ADBE Text Anchor Point 3D", position: "ADBE Text Position 3D", scale: "ADBE Text Scale 3D", skew: "ADBE Text Skew", skew_axis: "ADBE Text Skew Axis",
        rotation: "ADBE Text Rotation", x_rotation: "ADBE Text Rotation X", y_rotation: "ADBE Text Rotation Y", opacity: "ADBE Text Opacity",
        fill_color: "ADBE Text Fill Color", fill_hue: "ADBE Text Fill Hue", fill_saturation: "ADBE Text Fill Saturation", fill_brightness: "ADBE Text Fill Brightness",
        fill_opacity: "ADBE Text Fill Opacity", stroke_color: "ADBE Text Stroke Color", stroke_hue: "ADBE Text Stroke Hue", stroke_saturation: "ADBE Text Stroke Saturation",
        stroke_brightness: "ADBE Text Stroke Brightness", stroke_opacity: "ADBE Text Stroke Opacity", stroke_width: "ADBE Text Stroke Width",
        tracking: "ADBE Text Tracking Amount", tracking_type: "ADBE Text Tracking Type", line_anchor: "ADBE Text Line Anchor", line_spacing: "ADBE Text Line Spacing",
        character_offset: "ADBE Text Character Offset", character_value: "ADBE Text Character Value", blur: "ADBE Text Blur"
    };

    var SELECTOR_TYPES = { range: "ADBE Text Selector", wiggly: "ADBE Text Wiggly Selector", expression: "ADBE Text Expressible Selector" };

    var SELECTOR_SETTINGS = {
        start: ["ADBE Text Percent Start", "ADBE Text Index Start"], end: ["ADBE Text Percent End", "ADBE Text Index End"], offset: ["ADBE Text Percent Offset", "ADBE Text Index Offset"],
        units: ["ADBE Text Range Units"], based_on: ["ADBE Text Range Type2"], mode: ["ADBE Text Selector Mode"], amount: ["ADBE Text Selector Max Amount", "ADBE Text Expressible Amount"],
        shape: ["ADBE Text Range Shape"], smoothness: ["ADBE Text Selector Smoothness"], ease_high: ["ADBE Text Levels Max Ease"], ease_low: ["ADBE Text Levels Min Ease"],
        randomize: ["ADBE Text Randomize Order"], seed: ["ADBE Text Random Seed", "ADBE Text Wiggly Random Seed"], max: ["ADBE Text Wiggly Max Amount"], min: ["ADBE Text Wiggly Min Amount"],
        frequency: ["ADBE Text Temporal Freq"], correlation: ["ADBE Text Character Correlation"], temporal_phase: ["ADBE Text Temporal Phase"], spatial_phase: ["ADBE Text Spatial Phase"],
        lock_dimensions: ["ADBE Text Wiggly Lock Dim"]
    };

    var SELECTOR_WORDS = {
        units: ["percent", "index"],
        based_on: ["characters", "characters_excluding_spaces", "words", "lines"],
        mode: ["add", "subtract", "intersect", "min", "max", "difference"],
        shape: ["square", "ramp_up", "ramp_down", "triangle", "round", "smooth"]
    };

    function textLayer(comp, params) {
        var l = ae.layer(comp, params.layer);
        if (!(l instanceof TextLayer)) { throw ae.fail("NotAText", "'" + l.name + "' is not a text layer."); }
        return l;
    }

    function sourceText(l) { return l.property("ADBE Text Properties").property("ADBE Text Document"); }

    function readAttribute(target, name) {
        try { return target[name]; } catch (ignored) { return undefined; }
    }

    function documentOut(doc) {
        var out = { text: doc.text, box_text: doc.boxText, justification: ae.enumName(ParagraphJustification, doc.justification) };
        for (var i = 0; i < STYLE_ATTRIBUTES.length; i++) {
            var value = readAttribute(doc, STYLE_ATTRIBUTES[i][1]);
            if (value !== undefined) { out[STYLE_ATTRIBUTES[i][0]] = value; }
        }
        var attributes = PARAGRAPH_ATTRIBUTES.concat(DOCUMENT_ATTRIBUTES);
        for (var a = 0; a < attributes.length; a++) {
            var layoutValue = readAttribute(doc, attributes[a][1]);
            if (layoutValue !== undefined) { out[attributes[a][0]] = layoutValue; }
        }
        var enums = STYLE_ENUMS.concat(PARAGRAPH_ENUMS, DOCUMENT_ENUMS);
        for (var e = 0; e < enums.length; e++) {
            var enumeration = enumerationNamed(enums[e][2]);
            var raw = readAttribute(doc, enums[e][1]);
            if (enumeration && raw !== undefined) { out[enums[e][0]] = ae.enumName(enumeration, raw); }
        }
        if (out.apply_fill) { out.fill_color = ae.numbers(doc.fillColor); }
        if (out.apply_stroke) { out.stroke_color = ae.numbers(doc.strokeColor); }
        if (doc.boxText) {
            try { out.box_size = ae.numbers(doc.boxTextSize); } catch (ignored) { }
            try { out.box_position = ae.numbers(doc.boxTextPos); } catch (ignored2) { }
            try { out.box_overflow = doc.boxOverflow === true; } catch (ignored4) { }
        }
        try { out.paragraphs = rangesOut(doc.paragraphCount, function (i) { return doc.paragraphRange(i); }); } catch (ignored5) { }
        try { out.lines = rangesOut(doc.composedLineCount, function (i) { return doc.composedLineRange(i); }); } catch (ignored6) { }
        try { if (doc.fontLocation) { out.font_location = doc.fontLocation; } } catch (ignored7) { }
        var fontObject = readAttribute(doc, "fontObject");
        if (fontObject) { out.font_family = fontObject.familyName; out.font_style = fontObject.styleName; out.font_substituted = fontObject.isSubstitute === true; }
        try { out.lines = doc.baselineLocs ? doc.baselineLocs.length / 4 : undefined; } catch (ignored3) { }
        return out;
    }

    function rangesOut(count, rangeAt) {
        var out = [];
        for (var i = 0; i < count && i < 200; i++) {
            var range = rangeAt(i);
            out.push({ index: i + 1, start: range.characterStart, end: range.characterEnd });
        }
        return out;
    }

    function fontsApi() {
        var fonts = null;
        try { fonts = app.fonts; } catch (ignored) { fonts = null; }
        if (!fonts) { throw ae.fail("NotSupported", "This After Effects has no fonts API (it came with 24.0); set font as a PostScript name instead."); }
        return fonts;
    }

    function fontOut(font) {
        var out = { postscript_name: font.postScriptName, family: font.familyName, style: font.styleName };
        if (font.isSubstitute) { out.substituted = true; }
        try { out.technology = ae.enumName(CTFontTechnology, font.technology); } catch (ignored) { }
        try { out.variable = font.hasDesignAxes === true; } catch (ignored2) { }
        try { if (font.isFromAdobeFonts) { out.adobe_fonts = true; } } catch (ignored3) { }
        if (out.variable) {
            try { out.axes = axesOut(font); } catch (ignored4) { }
            try { out.design_vector = ae.numbers(font.designVector); } catch (ignored5) { }
        }
        return out;
    }

    function fontDetailOut(font) {
        var out = fontOut(font);
        try { out.id = font.fontID; } catch (ignored) { }
        try { out.full_name = font.fullName; } catch (ignored2) { }
        try { out.location = font.location; } catch (ignored3) { }
        try { out.version = font.version; } catch (ignored4) { }
        try { out.type = ae.enumName(CTFontType, font.type); } catch (ignored5) { }
        try {
            var scripts = font.writingScripts;
            out.writing_scripts = [];
            for (var i = 0; i < scripts.length; i++) { out.writing_scripts.push(ae.enumName(CTScript, scripts[i])); }
        } catch (ignored6) { }
        return out;
    }

    function axesOut(font) {
        var axes = font.designAxesData;
        var out = [];
        for (var i = 0; i < axes.length; i++) { out.push({ tag: axes[i].tag, name: axes[i].name, min: ae.round(axes[i].min), max: ae.round(axes[i].max), "default": ae.round(axes[i]["default"]) }); }
        return out;
    }

    // The instance of a variable font at the given axis values, the font's defaults for the axes not named.
    function fontInstance(font, axes) {
        if (!font || font.hasDesignAxes !== true) { throw ae.fail("BadRequest", "font_axes needs a variable font; ae_text_fonts with variable lists them.", { font: font ? font.postScriptName : null }); }
        var data = font.designAxesData;
        var vector = [];
        var unknown = [];
        for (var tag in axes) { if (axes.hasOwnProperty(tag)) { var found = false; for (var k = 0; k < data.length; k++) { if (data[k].tag === tag) { found = true; } } if (!found) { unknown.push(tag); } } }
        if (unknown.length > 0) { throw ae.fail("BadRequest", "'" + font.postScriptName + "' has no axis " + unknown.join(", ") + ".", { axes: axesOut(font) }); }
        for (var i = 0; i < data.length; i++) {
            var value = axes.hasOwnProperty(data[i].tag) ? Number(axes[data[i].tag]) : data[i]["default"];
            if (isNaN(value) || value < data[i].min - 0.01 || value > data[i].max + 0.01) { throw ae.fail("BadRequest", "Axis " + data[i].tag + " takes " + ae.round(data[i].min) + " to " + ae.round(data[i].max) + ".", { axes: axesOut(font) }); }
            vector.push(Math.min(Math.max(value, data[i].min), data[i].max));
        }
        var name;
        try { name = font.postScriptNameForDesignVector(vector); } catch (error) { throw ae.fail("NotSupported", "After Effects could not name an instance of '" + font.postScriptName + "' at these axes: " + error.message, { axes: axesOut(font) }); }
        var instances = fontsApi().getFontsByPostScriptName(name);
        if (!instances || instances.length === 0) { throw ae.fail("NotSupported", "After Effects names the instance '" + name + "' but does not provide it.", { postscript_name: name }); }
        return instances[0];
    }

    function matches(font, needle) {
        if (!needle) { return true; }
        var hay = (font.familyName + " " + font.styleName + " " + font.postScriptName).toLowerCase();
        return hay.indexOf(needle) >= 0;
    }

    function fontNamed(family, style) {
        var found = fontsApi().getFontsByFamilyNameAndStyleName(family, style || "Regular");
        if (!found || found.length === 0) { throw ae.fail("NotFound", "No installed font is '" + family + "' " + (style || "Regular") + "; text_fonts lists what there is.", { family: family, style: style || "Regular" }); }
        return found[0];
    }

    // The part of the document a style applies to: a character range when start or end is given, else the whole document.
    function styleTarget(doc, params) {
        var span = spanOf(doc, params);
        if (!ae.isGiven(span.start) && !ae.isGiven(span.end)) { return doc; }
        if (typeof doc.characterRange !== "function") { throw ae.fail("NotSupported", "This After Effects cannot style a range of characters (it came with 24.3); style the whole text."); }
        var length = doc.text.length;
        var start = ae.isGiven(span.start) ? span.start : 0;
        var end = ae.isGiven(span.end) ? span.end : length;
        if (start < 0 || end > length || start >= end) { throw ae.fail("BadRequest", "start and end index the characters, 0 to " + length + ", start before end."); }
        return doc.characterRange(start, end);
    }

    // The characters a paragraph or composed line covers, or the start and end given as they are.
    function spanOf(doc, params) {
        if (ae.isGiven(params.paragraph)) { return unitSpan(doc, params.paragraph, params.paragraph_end, doc.paragraphCount, "paragraph", function (i) { return doc.paragraphRange(i); }); }
        if (ae.isGiven(params.line)) { return unitSpan(doc, params.line, params.line_end, doc.composedLineCount, "line", function (i) { return doc.composedLineRange(i); }); }
        return { start: params.start, end: params.end };
    }

    function unitSpan(doc, first, last, count, what, rangeAt) {
        var to = ae.isGiven(last) ? last : first;
        if (first < 1 || to > count || to < first) { throw ae.fail("BadRequest", "The text has " + count + " " + what + "s, 1 first; " + what + " and " + what + "_end address them in order.", { count: count }); }
        return { start: rangeAt(first - 1).characterStart, end: rangeAt(to - 1).characterEnd };
    }

    function applyEnums(target, params, table) {
        var touched = [];
        for (var i = 0; i < table.length; i++) {
            if (!ae.isGiven(params[table[i][0]])) { continue; }
            var enumeration = enumerationNamed(table[i][2]);
            if (!enumeration) { throw ae.fail("NotSupported", "This After Effects has no " + table[i][3] + " setting."); }
            target[table[i][1]] = ae.enumValue(enumeration, params[table[i][0]], table[i][3]);
            touched.push(table[i][0]);
        }
        return touched;
    }

    function applyAttributes(target, params, table) {
        var touched = [];
        for (var i = 0; i < table.length; i++) {
            if (!ae.isGiven(params[table[i][0]])) { continue; }
            target[table[i][1]] = params[table[i][0]];
            touched.push(table[i][0]);
        }
        return touched;
    }

    function applyStyle(target, params) {
        var touched = [];
        if (ae.isGiven(params.text)) { target.text = String(params.text); touched.push("text"); }
        if (ae.isGiven(params.font_family)) {
            var font = fontNamed(params.font_family, params.font_style);
            if (!("fontObject" in target)) { target.font = font.postScriptName; } else { target.fontObject = font; }
            touched.push("font");
        }
        if (params.font_axes && typeof params.font_axes === "object") {
            var base = ae.isGiven(params.font_family) ? fontNamed(params.font_family, params.font_style) : readAttribute(target, "fontObject");
            target.fontObject = fontInstance(base, params.font_axes);
            touched.push("font_axes");
        }
        touched = touched.concat(applyAttributes(target, params, STYLE_ATTRIBUTES), applyEnums(target, params, STYLE_ENUMS));
        if (ae.isGiven(params.fill_color)) { target.applyFill = true; target.fillColor = ae.color(params.fill_color, "fill_color"); touched.push("fill_color"); }
        if (ae.isGiven(params.stroke_color)) { target.applyStroke = true; target.strokeColor = ae.color(params.stroke_color, "stroke_color"); touched.push("stroke_color"); }
        if (ae.isGiven(params.justification)) { target.justification = ae.enumValue(ParagraphJustification, params.justification, "justification"); touched.push("justification"); }
        if (ae.isGiven(params.box_size)) {
            if (!target.boxText) { throw ae.fail("BadRequest", "box_size applies to box text; this layer is point text."); }
            target.boxTextSize = params.box_size;
            touched.push("box_size");
        }
        return touched;
    }

    function animators(l) { return l.property("ADBE Text Properties").property("ADBE Text Animators"); }

    function animatorOf(l, ref) {
        var group = animators(l);
        if (typeof ref === "number") {
            if (ref < 1 || ref > group.numProperties) { throw ae.fail("NotFound", "'" + l.name + "' has no animator " + ref + "; it has " + group.numProperties + "."); }
            return group.property(ref);
        }
        if (typeof ref !== "string" || ref === "") { throw ae.fail("BadRequest", "Name the animator by its name or its index."); }
        for (var i = 1; i <= group.numProperties; i++) { if (group.property(i).name === ref) { return group.property(i); } }
        throw ae.fail("NotFound", "'" + l.name + "' has no animator named '" + ref + "'.");
    }

    function selectorTypeOf(selector) {
        for (var key in SELECTOR_TYPES) { if (SELECTOR_TYPES.hasOwnProperty(key) && SELECTOR_TYPES[key] === selector.matchName) { return key; } }
        return selector.matchName;
    }

    function settingsOut(group, out) {
        for (var i = 1; i <= group.numProperties; i++) {
            var p = group.property(i);
            if (props.isGroup(p)) { settingsOut(p, out); continue; }
            if (p.propertyValueType === PropertyValueType.NO_VALUE) { continue; }
            var key = null;
            for (var name in SELECTOR_SETTINGS) {
                if (!SELECTOR_SETTINGS.hasOwnProperty(name)) { continue; }
                for (var m = 0; m < SELECTOR_SETTINGS[name].length; m++) { if (SELECTOR_SETTINGS[name][m] === p.matchName) { key = name; } }
            }
            if (!key) { continue; }
            var value = props.valueOut(p, p.value);
            if (SELECTOR_WORDS[key] && typeof value === "number" && SELECTOR_WORDS[key][value - 1]) { value = SELECTOR_WORDS[key][value - 1]; }
            out[key] = value;
            if (p.numKeys > 0) { out[key + "_keys"] = p.numKeys; }
            if (p.expression) { out[key + "_expression"] = p.expression; }
        }
        return out;
    }

    function selectorOut(selector, index) {
        var out = { index: index, name: selector.name, type: selectorTypeOf(selector), enabled: selector.enabled, path: props.pathOf(selector) };
        return settingsOut(selector, out);
    }

    function animatorOut(animator, index) {
        var out = { index: index, name: animator.name, enabled: animator.enabled, path: props.pathOf(animator), properties: [], selectors: [] };
        var values = animator.property("ADBE Text Animator Properties");
        for (var i = 1; i <= values.numProperties; i++) {
            var p = values.property(i);
            var entry = { name: p.name, match_name: p.matchName, path: props.pathOf(p), value: props.valueOut(p, p.value) };
            for (var alias in ANIMATOR_PROPERTIES) { if (ANIMATOR_PROPERTIES.hasOwnProperty(alias) && ANIMATOR_PROPERTIES[alias] === p.matchName) { entry.alias = alias; } }
            if (p.numKeys > 0) { entry.keys = p.numKeys; }
            if (p.expression) { entry.expression = p.expression; }
            out.properties.push(entry);
        }
        var selectors = animator.property("ADBE Text Selectors");
        for (var s = 1; s <= selectors.numProperties; s++) { out.selectors.push(selectorOut(selectors.property(s), s)); }
        return out;
    }

    function animatorsOut(l) {
        var group = animators(l);
        var out = [];
        for (var i = 1; i <= group.numProperties; i++) { out.push(animatorOut(group.property(i), i)); }
        return out;
    }

    // The animator property for one axis of the layer's variable font, added when the animator has none yet.
    function axisProperty(group, tag) {
        if (typeof group.addVariableFontAxis !== "function") { throw ae.fail("NotSupported", "This After Effects cannot animate variable font axes (it came with 24.1)."); }
        try { return group.addVariableFontAxis(tag); }
        catch (error) { throw ae.fail("BadRequest", "No axis '" + tag + "' on the layer's font: " + error.message + " Set a variable font with ae_text_style font_axes first.", { tag: tag }); }
    }

    function addAnimatorProperties(animator, values) {
        var group = animator.property("ADBE Text Animator Properties");
        for (var alias in values) {
            if (!values.hasOwnProperty(alias)) { continue; }
            var matchName = ANIMATOR_PROPERTIES[alias] || alias;
            var p = null;
            if (alias.indexOf("axis_") === 0) { p = axisProperty(group, alias.substring(5)); }
            else if (group.canAddProperty(matchName)) { p = group.addProperty(matchName); }
            else { try { p = group.property(matchName); } catch (ignored) { p = null; } }
            if (!p) {
                var accepted = [];
                for (var known in ANIMATOR_PROPERTIES) { if (ANIMATOR_PROPERTIES.hasOwnProperty(known)) { accepted.push(known); } }
                throw ae.fail("BadRequest", "'" + alias + "' is not a property a text animator can carry.", { accepted: accepted });
            }
            if (values[alias] !== null) { p.setValue(props.valueIn(p, values[alias])); }
        }
    }

    function selectorSetting(selector, name, value) {
        var candidates = SELECTOR_SETTINGS[name];
        if (!candidates) {
            var accepted = [];
            for (var known in SELECTOR_SETTINGS) { if (SELECTOR_SETTINGS.hasOwnProperty(known)) { accepted.push(known); } }
            throw ae.fail("BadRequest", "'" + name + "' is not a selector setting.", { accepted: accepted });
        }
        var p = null;
        for (var i = 0; i < candidates.length && !p; i++) {
            try { p = selector.property(candidates[i]); } catch (ignored) { p = null; }
            if (!p) {
                var advanced = null;
                try { advanced = selector.property("ADBE Text Range Advanced"); } catch (ignored2) { advanced = null; }
                if (advanced) { try { p = advanced.property(candidates[i]); } catch (ignored3) { p = null; } }
            }
        }
        if (!p) { throw ae.fail("BadRequest", "This selector has no '" + name + "' setting; a " + selectorTypeOf(selector) + " selector does not carry it."); }
        if (SELECTOR_WORDS[name] && typeof value === "string") {
            var index = -1;
            for (var w = 0; w < SELECTOR_WORDS[name].length; w++) { if (SELECTOR_WORDS[name][w] === String(value).toLowerCase()) { index = w + 1; } }
            if (index < 0) { throw ae.fail("BadRequest", "'" + value + "' is not a " + name + ".", { accepted: SELECTOR_WORDS[name] }); }
            value = index;
        }
        if (name === "amount" && selector.matchName === SELECTOR_TYPES.expression && typeof value === "string") { p.expression = value; return; }
        p.setValue(props.valueIn(p, value));
    }

    function applySelector(animator, spec, create) {
        var selectors = animator.property("ADBE Text Selectors");
        var selector = null;
        var type = ae.isGiven(spec.type) ? String(spec.type).toLowerCase() : "range";
        if (!SELECTOR_TYPES[type]) { throw ae.fail("BadRequest", "'" + spec.type + "' is not a selector type.", { accepted: ["range", "wiggly", "expression"] }); }
        if (ae.isGiven(spec.index)) {
            if (spec.index < 1 || spec.index > selectors.numProperties) { throw ae.fail("NotFound", "The animator has no selector " + spec.index + "; it has " + selectors.numProperties + "."); }
            selector = selectors.property(spec.index);
        } else if (!create) {
            for (var i = 1; i <= selectors.numProperties && !selector; i++) { if (selectors.property(i).matchName === SELECTOR_TYPES[type]) { selector = selectors.property(i); } }
        }
        if (!selector) { selector = selectors.addProperty(SELECTOR_TYPES[type]); }
        if (ae.isGiven(spec.name)) { selector.name = String(spec.name); }
        if (ae.isGiven(spec.units)) { selectorSetting(selector, "units", spec.units); }
        for (var key in spec) {
            if (!spec.hasOwnProperty(key) || key === "type" || key === "index" || key === "name" || key === "units" || !ae.isGiven(spec[key])) { continue; }
            selectorSetting(selector, key, spec[key]);
        }
        return selector;
    }

    SnailBridge.command("text_info", { params: ["comp", "layer"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var source = sourceText(l);
        var out = { layer: { index: l.index, name: l.name }, document: documentOut(source.value), source_text_keys: source.numKeys, animators: animatorsOut(l) };
        if (source.expression) { out.source_text_expression = source.expression; }
        var pathOptions = l.property("ADBE Text Properties").property("ADBE Text Path Options");
        if (pathOptions) {
            var mask = pathOptions.property("ADBE Text Path").value;
            out.path = { mask: mask > 0 ? mask : null };
            if (mask > 0) {
                out.path.reverse = pathOptions.property("ADBE Text Reverse Path").value === 1;
                out.path.perpendicular = pathOptions.property("ADBE Text Perpendicular To Path").value === 1;
                out.path.force_alignment = pathOptions.property("ADBE Text Force Align Path").value === 1;
                out.path.first_margin = pathOptions.property("ADBE Text First Margin").value;
                out.path.last_margin = pathOptions.property("ADBE Text Last Margin").value;
            }
        }
        return out;
    });

    SnailBridge.command("text_style", {
        params: ["comp", "layer", "time", "start", "end", "paragraph", "paragraph_end", "line", "line_end", "text", "font", "font_family", "font_style", "font_axes", "font_size",
            "fill_color", "stroke_color", "stroke_width", "stroke_over_fill", "apply_fill", "apply_stroke", "justification", "tracking", "leading", "auto_leading", "baseline_shift",
            "faux_bold", "faux_italic", "all_caps", "small_caps", "superscript", "subscript", "horizontal_scale", "vertical_scale", "tsume", "ligature", "first_line_indent",
            "left_margin", "right_margin", "space_before", "space_after", "box_size", "kerning", "no_break", "auto_kern", "caps", "baseline_option", "line_join", "baseline_direction"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var source = sourceText(l);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var doc = source.numKeys > 0 ? source.valueAtTime(time, false) : source.value;
        var touched = applyStyle(styleTarget(doc, params), params);
        if (touched.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass text or any style attribute."); }
        if (ae.isGiven(params.time) || source.numKeys > 0) { source.setValueAtTime(time, doc); } else { source.setValue(doc); }
        return { layer: { index: l.index, name: l.name }, changed: touched, document: documentOut(source.numKeys > 0 ? source.valueAtTime(time, false) : source.value) };
    });

    SnailBridge.command("text_layout", {
        params: ["comp", "layer", "time", "paragraph", "paragraph_end", "direction", "leading_type", "composer", "every_line_composer", "digit_set", "auto_hyphenate", "hanging_roman",
            "start_indent", "end_indent", "line_orientation", "box_auto_fit", "box_vertical_alignment", "box_first_baseline", "box_first_baseline_minimum", "box_inset", "reset"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var source = sourceText(l);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var doc = source.numKeys > 0 ? source.valueAtTime(time, false) : source.value;
        var target = styleTarget(doc, params);
        var touched = [];
        if (ae.isGiven(params.reset)) {
            var reset = String(params.reset).toLowerCase();
            if (reset !== "character" && reset !== "paragraph" && reset !== "both") { throw ae.fail("BadRequest", "reset is character, paragraph or both.", { accepted: ["character", "paragraph", "both"] }); }
            if (reset !== "paragraph") { target.resetCharStyle(); }
            if (reset !== "character") { target.resetParagraphStyle(); }
            touched.push("reset");
        }
        touched = touched.concat(applyAttributes(target, params, PARAGRAPH_ATTRIBUTES), applyEnums(target, params, PARAGRAPH_ENUMS),
            applyAttributes(doc, params, DOCUMENT_ATTRIBUTES), applyEnums(doc, params, DOCUMENT_ENUMS));
        if (touched.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass a paragraph, box or composer setting, or reset."); }
        if (ae.isGiven(params.time) || source.numKeys > 0) { source.setValueAtTime(time, doc); } else { source.setValue(doc); }
        return { layer: { index: l.index, name: l.name }, changed: touched, document: documentOut(source.numKeys > 0 ? source.valueAtTime(time, false) : source.value) };
    });

    function scriptRunsOut(fonts, text) {
        var runs = fonts.getCTScriptForString(String(text), CTScript.CT_ROMAN_SCRIPT);
        var out = [];
        var offset = 0;
        for (var i = 0; i < runs.length; i++) {
            var entry = { script: ae.enumName(CTScript, runs[i].ctScript), start: offset, chars: runs[i].chars };
            try { entry.default_font = fonts.getDefaultFontForCTScript(runs[i].ctScript).postScriptName; } catch (ignored) { }
            offset += runs[i].chars;
            out.push(entry);
        }
        return out;
    }

    SnailBridge.command("text_fonts", { params: ["query", "limit", "postscript_name", "id", "family", "face", "missing", "variable", "script", "glyphs_for", "detail"], read_only: true }, function (params) {
        var fonts = fontsApi();
        var needle = ae.isGiven(params.query) ? String(params.query).toLowerCase() : "";
        var limit = ae.isGiven(params.limit) ? params.limit : 50;
        var detail = params.detail === true;
        var pick = detail ? fontDetailOut : fontOut;
        var script = ae.isGiven(params.script) ? ae.enumValue(CTScript, params.script, "writing script") : null;
        var answer = {};
        if (ae.isGiven(params.id)) {
            var byId = fonts.getFontByID(params.id);
            if (!byId) { throw ae.fail("NotFound", "No font has id " + params.id + "."); }
            return { fonts: [fontDetailOut(byId)], matched: 1, truncated: false };
        }
        var candidates = [];
        if (ae.isGiven(params.postscript_name)) { candidates = fonts.getFontsByPostScriptName(String(params.postscript_name)); }
        else if (ae.isGiven(params.family)) { candidates = fonts.getFontsByFamilyNameAndStyleName(String(params.family), ae.isGiven(params.face) ? String(params.face) : ""); }
        else if (params.missing === true) { candidates = fonts.missingOrSubstitutedFonts; }
        else if (params.variable === true) { candidates = fonts.fontsWithDefaultDesignAxes; }
        else { var families = fonts.allFonts; for (var f = 0; f < families.length; f++) { for (var s = 0; s < families[f].length; s++) { candidates.push(families[f][s]); } } }
        if (ae.isGiven(params.glyphs_for)) { answer.scripts = scriptRunsOut(fonts, params.glyphs_for); }
        var out = [];
        var total = 0;
        for (var c = 0; c < candidates.length; c++) {
            var font = candidates[c];
            if (!matches(font, needle)) { continue; }
            if (params.variable === true && font.hasDesignAxes !== true) { continue; }
            if (script !== null && !hasScript(font, script)) { continue; }
            if (ae.isGiven(params.glyphs_for) && !font.hasGlyphsFor(String(params.glyphs_for))) { continue; }
            total++;
            if (out.length < limit) { out.push(pick(font)); }
        }
        answer.fonts = out;
        answer.matched = total;
        answer.truncated = total > out.length;
        return answer;
    });

    function hasScript(font, script) {
        try { var scripts = font.writingScripts; for (var i = 0; i < scripts.length; i++) { if (scripts[i] === script) { return true; } } } catch (ignored) { }
        return false;
    }

    SnailBridge.command("text_animator_add", { params: ["comp", "layer", "name", "properties", "selector"] }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var animator = animators(l).addProperty("ADBE Text Animator");
        try {
            if (ae.isGiven(params.name)) { animator.name = String(params.name); }
            if (params.properties && typeof params.properties === "object") { addAnimatorProperties(animator, params.properties); }
            if (params.selector && typeof params.selector === "object") { applySelector(animator, params.selector, false); }
        } catch (error) {
            try { animator.remove(); } catch (ignored) { }
            throw error;
        }
        return { layer: { index: l.index, name: l.name }, animator: animatorOut(animator, animator.propertyIndex) };
    });

    SnailBridge.command("text_animator_update", { params: ["comp", "layer", "animator", "name", "enabled", "properties", "selector", "add_selector"] }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var animator = animatorOf(l, params.animator);
        if (ae.isGiven(params.name)) { animator.name = String(params.name); }
        if (ae.isGiven(params.enabled)) { animator.enabled = params.enabled === true; }
        if (params.properties && typeof params.properties === "object") { addAnimatorProperties(animator, params.properties); }
        if (params.selector && typeof params.selector === "object") { applySelector(animator, params.selector, params.add_selector === true); }
        return { layer: { index: l.index, name: l.name }, animator: animatorOut(animator, animator.propertyIndex) };
    });

    SnailBridge.command("text_animator_remove", { params: ["comp", "layer", "animator", "property", "selector"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var animator = animatorOf(l, params.animator);
        var removed;
        if (ae.isGiven(params.property)) {
            var values = animator.property("ADBE Text Animator Properties");
            var matchName = ANIMATOR_PROPERTIES[params.property] || params.property;
            var p = null;
            try { p = values.property(matchName); } catch (ignored) { p = null; }
            if (!p) { throw ae.fail("NotFound", "The animator carries no '" + params.property + "'."); }
            removed = { property: p.name };
            p.remove();
        } else if (ae.isGiven(params.selector)) {
            var selectors = animator.property("ADBE Text Selectors");
            if (params.selector < 1 || params.selector > selectors.numProperties) { throw ae.fail("NotFound", "The animator has no selector " + params.selector + "; it has " + selectors.numProperties + "."); }
            removed = { selector: selectors.property(params.selector).name };
            selectors.property(params.selector).remove();
        } else {
            removed = { animator: animator.name };
            animator.remove();
            return { layer: { index: l.index, name: l.name }, removed: removed, animators: animatorsOut(l) };
        }
        return { layer: { index: l.index, name: l.name }, removed: removed, animator: animatorOut(animator, animator.propertyIndex) };
    });

    SnailBridge.command("text_path", { params: ["comp", "layer", "mask", "reverse", "perpendicular", "force_alignment", "first_margin", "last_margin"] }, function (params) {
        var comp = ae.comp(params);
        var l = textLayer(comp, params);
        var options = l.property("ADBE Text Properties").property("ADBE Text Path Options");
        if (params.mask !== undefined) {
            var index = 0;
            if (params.mask !== null) {
                var masks = l.property("ADBE Mask Parade");
                if (typeof params.mask === "number") {
                    if (params.mask < 1 || params.mask > masks.numProperties) { throw ae.fail("NotFound", "'" + l.name + "' has no mask " + params.mask + "; it has " + masks.numProperties + "."); }
                    index = params.mask;
                } else {
                    for (var m = 1; m <= masks.numProperties; m++) { if (masks.property(m).name === params.mask) { index = m; } }
                    if (index === 0) { throw ae.fail("NotFound", "'" + l.name + "' has no mask named '" + params.mask + "'."); }
                }
            }
            options.property("ADBE Text Path").setValue(index);
        }
        if (options.property("ADBE Text Path").value > 0) {
            if (ae.isGiven(params.reverse)) { options.property("ADBE Text Reverse Path").setValue(params.reverse ? 1 : 0); }
            if (ae.isGiven(params.perpendicular)) { options.property("ADBE Text Perpendicular To Path").setValue(params.perpendicular ? 1 : 0); }
            if (ae.isGiven(params.force_alignment)) { options.property("ADBE Text Force Align Path").setValue(params.force_alignment ? 1 : 0); }
            if (ae.isGiven(params.first_margin)) { options.property("ADBE Text First Margin").setValue(params.first_margin); }
            if (ae.isGiven(params.last_margin)) { options.property("ADBE Text Last Margin").setValue(params.last_margin); }
        }
        var mask = options.property("ADBE Text Path").value;
        var out = { layer: { index: l.index, name: l.name }, mask: mask > 0 ? mask : null };
        if (mask > 0) {
            out.reverse = options.property("ADBE Text Reverse Path").value === 1;
            out.perpendicular = options.property("ADBE Text Perpendicular To Path").value === 1;
            out.force_alignment = options.property("ADBE Text Force Align Path").value === 1;
            out.first_margin = options.property("ADBE Text First Margin").value;
            out.last_margin = options.property("ADBE Text Last Margin").value;
        }
        return out;
    });
})();
