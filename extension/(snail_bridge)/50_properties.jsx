// The property tree of a layer, which is where everything After Effects animates lives: transform, effects and their
// parameters, masks, text, material options. Paths are segments separated by "/", each a match name (stable across
// languages), a display name (in the language of this After Effects) or a 1-based index; the tree answers with both names.
(function () {
    var ae = SnailBridge.ae;

    var ALIASES = {
        "transform": "ADBE Transform Group", "anchor": "ADBE Anchor Point", "anchor point": "ADBE Anchor Point", "position": "ADBE Position",
        "scale": "ADBE Scale", "rotation": "ADBE Rotate Z", "z rotation": "ADBE Rotate Z", "x rotation": "ADBE Rotate X", "y rotation": "ADBE Rotate Y",
        "orientation": "ADBE Orientation", "opacity": "ADBE Opacity", "effects": "ADBE Effect Parade", "masks": "ADBE Mask Parade",
        "text": "ADBE Text Properties", "source text": "ADBE Text Document", "animators": "ADBE Text Animators", "material options": "ADBE Material Options Group",
        "audio": "ADBE Audio Group", "audio levels": "ADBE Audio Levels", "time remap": "ADBE Time Remapping", "camera options": "ADBE Camera Options Group",
        "light options": "ADBE Light Options Group", "contents": "ADBE Root Vectors Group", "layer styles": "ADBE Layer Styles",
        "geometry options": "ADBE Extrsn Options Group", "point of interest": "ADBE Anchor Point", "zoom": "ADBE Camera Zoom",
        "depth of field": "ADBE Camera Depth of Field", "focus distance": "ADBE Camera Focus Distance", "aperture": "ADBE Camera Aperture",
        "intensity": "ADBE Light Intensity", "color": "ADBE Light Color", "cone angle": "ADBE Light Cone Angle", "cone feather": "ADBE Light Cone Feather 2"
    };

    function childOf(group, segment) {
        var found = null;
        try { found = group.property(segment); } catch (ignored) { found = null; }
        if (found) { return found; }
        var alias = ALIASES[String(segment).toLowerCase()];
        if (alias) {
            try { found = group.property(alias); } catch (ignored2) { found = null; }
            if (found) { return found; }
        }
        if (/^\d+$/.test(String(segment))) {
            var index = parseInt(segment, 10);
            if (index >= 1 && index <= group.numProperties) { return group.property(index); }
        }
        return null;
    }

    function resolve(layer, path) {
        if (!ae.isGiven(path) || path === "" || path === "/") { return layer; }
        var segments = String(path).split("/");
        var current = layer;
        var walked = [];
        for (var i = 0; i < segments.length; i++) {
            var segment = segments[i];
            if (segment === "") { continue; }
            if (!(current.propertyType === PropertyType.INDEXED_GROUP || current.propertyType === PropertyType.NAMED_GROUP || current === layer)) {
                throw ae.fail("NoSuchProperty", "'" + walked.join("/") + "' is a property, not a group; nothing is below it.");
            }
            var next = childOf(current, segment);
            if (!next) {
                var siblings = [];
                for (var s = 1; s <= current.numProperties; s++) { siblings.push({ name: current.property(s).name, match_name: current.property(s).matchName }); }
                throw ae.fail("NoSuchProperty", "'" + segment + "' is not under '" + (walked.length ? walked.join("/") : layer.name) + "'.", { available: siblings });
            }
            walked.push(next.matchName);
            current = next;
        }
        return current;
    }

    function pathOf(prop) {
        var segments = [];
        var current = prop;
        while (current && current.propertyDepth > 0) {
            segments.unshift(current.matchName);
            current = current.parentProperty;
        }
        return segments.join("/");
    }

    function isGroup(prop) {
        return prop.propertyType === PropertyType.INDEXED_GROUP || prop.propertyType === PropertyType.NAMED_GROUP;
    }

    function valueTypeName(prop) { return ae.enumName(PropertyValueType, prop.propertyValueType); }

    function textDocumentOut(doc) {
        var out = {
            text: doc.text,
            font: doc.font,
            font_size: doc.fontSize,
            apply_fill: doc.applyFill,
            apply_stroke: doc.applyStroke,
            justification: ae.enumName(ParagraphJustification, doc.justification),
            tracking: doc.tracking,
            box_text: doc.boxText
        };
        try { if (doc.applyFill) { out.fill_color = ae.numbers(doc.fillColor); } } catch (ignored) { }
        try { if (doc.applyStroke) { out.stroke_color = ae.numbers(doc.strokeColor); out.stroke_width = doc.strokeWidth; } } catch (ignored2) { }
        try { out.leading = doc.leading; } catch (ignored3) { }
        try { if (doc.boxText) { out.box_text_size = ae.numbers(doc.boxTextSize); } } catch (ignored4) { }
        return out;
    }

    function pointsOut(points) {
        var out = [];
        for (var i = 0; i < points.length; i++) { out.push(ae.numbers(points[i])); }
        return out;
    }

    // Mask feather points: each sits on a segment of the path at a position along it, with a radius, inner or outer, and a tension.
    function featherOut(shape) {
        var out = [];
        try {
            var radii = shape.featherRadii || [];
            for (var i = 0; i < radii.length; i++) {
                out.push({
                    segment: shape.featherSegLocs[i] + 1, position: ae.round(shape.featherRelSegLocs[i]), radius: ae.round(Math.abs(radii[i])),
                    inner: shape.featherTypes[i] === 1 || radii[i] < 0, tension: ae.round(shape.featherTensions[i]), hold: shape.featherInterps[i] === 1, corner_angle: ae.round(shape.featherRelCornerAngles[i])
                });
            }
        } catch (ignored) { }
        return out;
    }

    function featherIn(shape, points) {
        var segments = [], positions = [], radii = [], types = [], tensions = [], interps = [], angles = [];
        for (var i = 0; i < points.length; i++) {
            var point = points[i] || {};
            if (!ae.isGiven(point.segment) || !ae.isGiven(point.radius)) { throw ae.fail("BadRequest", "Each feather point needs segment (1 first) and radius; position along the segment (0 to 1), inner, tension, hold and corner_angle are optional."); }
            segments.push(point.segment - 1);
            positions.push(ae.isGiven(point.position) ? point.position : 0.5);
            radii.push(point.inner === true ? -Math.abs(point.radius) : Math.abs(point.radius));
            types.push(point.inner === true ? 1 : 0);
            tensions.push(ae.isGiven(point.tension) ? point.tension : 0);
            interps.push(point.hold === true ? 1 : 0);
            angles.push(ae.isGiven(point.corner_angle) ? point.corner_angle : 0);
        }
        shape.featherSegLocs = segments;
        shape.featherRelSegLocs = positions;
        shape.featherRadii = radii;
        shape.featherTypes = types;
        shape.featherTensions = tensions;
        shape.featherInterps = interps;
        shape.featherRelCornerAngles = angles;
    }

    function valueOut(prop, value) {
        var type = prop.propertyValueType;
        if (type === PropertyValueType.NO_VALUE || type === PropertyValueType.CUSTOM_VALUE) { return null; }
        if (type === PropertyValueType.TEXT_DOCUMENT) { return textDocumentOut(value); }
        if (type === PropertyValueType.SHAPE) {
            var shapeOut = { closed: value.closed, vertices: pointsOut(value.vertices), in_tangents: pointsOut(value.inTangents), out_tangents: pointsOut(value.outTangents) };
            var featherPoints = featherOut(value);
            if (featherPoints.length > 0) { shapeOut.feather_points = featherPoints; }
            return shapeOut;
        }
        if (type === PropertyValueType.MARKER) { return { comment: value.comment, duration: value.duration, chapter: value.chapter, url: value.url, frame_target: value.frameTarget, cue_point_name: value.cuePointName, label: value.label }; }
        if (value instanceof Array) { return ae.numbers(value); }
        if (typeof value === "number") { return ae.round(value); }
        return value;
    }

    function valueIn(prop, value) {
        var type = prop.propertyValueType;
        if (type === PropertyValueType.TEXT_DOCUMENT) {
            var doc = prop.value;
            if (typeof value === "string") { doc.text = value; return doc; }
            if (!value || typeof value !== "object") { throw ae.fail("BadRequest", "A text property takes a string, or an object with text, font, font_size, fill_color, stroke_color, apply_fill, apply_stroke, stroke_width, justification, tracking, leading."); }
            if (ae.isGiven(value.text)) { doc.text = value.text; }
            if (ae.isGiven(value.font)) { doc.font = value.font; }
            if (ae.isGiven(value.font_size)) { doc.fontSize = value.font_size; }
            if (ae.isGiven(value.apply_fill)) { doc.applyFill = value.apply_fill === true; }
            if (ae.isGiven(value.fill_color)) { doc.applyFill = true; doc.fillColor = ae.color(value.fill_color, "fill_color"); }
            if (ae.isGiven(value.apply_stroke)) { doc.applyStroke = value.apply_stroke === true; }
            if (ae.isGiven(value.stroke_color)) { doc.applyStroke = true; doc.strokeColor = ae.color(value.stroke_color, "stroke_color"); }
            if (ae.isGiven(value.stroke_width)) { doc.strokeWidth = value.stroke_width; }
            if (ae.isGiven(value.justification)) { doc.justification = ae.enumValue(ParagraphJustification, value.justification, "justification"); }
            if (ae.isGiven(value.tracking)) { doc.tracking = value.tracking; }
            if (ae.isGiven(value.leading)) { doc.leading = value.leading; }
            return doc;
        }
        if (type === PropertyValueType.SHAPE) {
            if (!value || !(value.vertices instanceof Array)) { throw ae.fail("BadRequest", "A shape takes an object with vertices [[x, y], ...], and optionally in_tangents, out_tangents and closed."); }
            var shape = new Shape();
            shape.vertices = value.vertices;
            if (value.in_tangents instanceof Array) { shape.inTangents = value.in_tangents; }
            if (value.out_tangents instanceof Array) { shape.outTangents = value.out_tangents; }
            shape.closed = value.closed !== false;
            if (value.feather_points instanceof Array) { featherIn(shape, value.feather_points); }
            return shape;
        }
        if (type === PropertyValueType.MARKER) {
            var marker = new MarkerValue(value && ae.isGiven(value.comment) ? String(value.comment) : (typeof value === "string" ? value : ""));
            if (value && typeof value === "object") {
                if (ae.isGiven(value.duration)) { marker.duration = value.duration; }
                if (ae.isGiven(value.chapter)) { marker.chapter = value.chapter; }
                if (ae.isGiven(value.url)) { marker.url = value.url; }
                if (ae.isGiven(value.label)) { marker.label = value.label; }
            }
            return marker;
        }
        if (type === PropertyValueType.COLOR && typeof value === "string") {
            var rgb = ae.color(value);
            return [rgb[0], rgb[1], rgb[2], 1];
        }
        if (type === PropertyValueType.COLOR && value instanceof Array && value.length === 3) { return [value[0], value[1], value[2], 1]; }
        if (typeof value === "boolean") { return value ? 1 : 0; }
        if (value instanceof Array && prop.value instanceof Array && value.length < prop.value.length) {
            var padded = value.slice(0);
            for (var i = value.length; i < prop.value.length; i++) { padded.push(prop.value[i]); }
            return padded;
        }
        return value;
    }

    function describe(prop, time, withValue) {
        var out = { name: prop.name, match_name: prop.matchName, path: pathOf(prop) };
        if (isGroup(prop)) {
            out.kind = prop.propertyType === PropertyType.INDEXED_GROUP ? "indexed_group" : "group";
            out.children = prop.numProperties;
            if (prop.canSetEnabled) { out.enabled = prop.enabled; }
            return out;
        }
        out.kind = "property";
        out.value_type = valueTypeName(prop);
        if (withValue) {
            try { out.value = valueOut(prop, ae.isGiven(time) ? prop.valueAtTime(time, false) : prop.value); } catch (ignored) { out.value = null; }
        }
        out.animated = prop.numKeys > 0;
        if (prop.numKeys > 0) { out.keys = prop.numKeys; }
        if (prop.canSetExpression && prop.expression) { out.expression = prop.expression; out.expression_enabled = prop.expressionEnabled; }
        if (prop.expressionError) { out.expression_error = prop.expressionError; }
        if (prop.hasMin) { out.min = prop.minValue; }
        if (prop.hasMax) { out.max = prop.maxValue; }
        if (prop.isSpatial) { out.spatial = true; }
        try { if (prop.unitsText) { out.units = prop.unitsText; } } catch (ignoredUnits) { }
        try {
            if (prop.isSeparationLeader) { out.separation = { leader: true, separated: prop.dimensionsSeparated }; }
            if (prop.isSeparationFollower) { out.separation = { follower: true, dimension: prop.separationDimension, leader: pathOf(prop.separationLeader) }; }
        } catch (ignoredSeparation) { }
        try { if (prop.isDropdownEffect) { out.dropdown = true; out.options = prop.propertyParameters; } } catch (ignoredDropdown) { }
        if (prop.dimensionsSeparated) { out.dimensions_separated = true; }
        if (prop.propertyValueType === PropertyValueType.CUSTOM_VALUE) { out.note = "a custom value After Effects does not expose to scripts"; }
        return out;
    }

    function tree(prop, depth, time, withValues) {
        var node = describe(prop, time, withValues && !isGroup(prop));
        if (isGroup(prop) && depth > 0) {
            var children = [];
            for (var i = 1; i <= prop.numProperties; i++) { children.push(tree(prop.property(i), depth - 1, time, withValues)); }
            node.properties = children;
        }
        return node;
    }

    SnailBridge.command("property_tree", { params: ["comp", "layer", "path", "depth", "values"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var root = resolve(l, params.path);
        var depth = ae.isGiven(params.depth) ? params.depth : 2;
        var withValues = params.values !== false;
        if (root === l) {
            var groups = [];
            for (var i = 1; i <= l.numProperties; i++) { groups.push(tree(l.property(i), depth - 1, comp.time, withValues)); }
            return { layer: { index: l.index, name: l.name }, time: ae.round(comp.time), properties: groups };
        }
        var node = tree(root, depth, comp.time, withValues);
        node.layer = { index: l.index, name: l.name };
        return node;
    });

    SnailBridge.command("property_get", { params: ["comp", "layer", "path", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the property, as property_tree lists it: ADBE Transform Group/ADBE Position, or Transform/Position."); }
        var prop = resolve(l, params.path);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var out = describe(prop, time, true);
        out.time = ae.round(time);
        return out;
    });

    SnailBridge.command("property_set", { params: ["comp", "layer", "path", "value", "time", "expression", "enabled"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the property, as property_tree lists it."); }
        var prop = resolve(l, params.path);
        if (isGroup(prop)) {
            if (!ae.isGiven(params.enabled)) { throw ae.fail("BadRequest", "'" + prop.name + "' is a group; it has no value, only enabled."); }
            if (!prop.canSetEnabled) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot be switched on or off."); }
            prop.enabled = params.enabled === true;
            return describe(prop, null, false);
        }
        if (params.expression !== undefined) {
            if (!prop.canSetExpression) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot take an expression."); }
            prop.expression = params.expression === null ? "" : String(params.expression);
            if (prop.expression !== "") { prop.expressionEnabled = params.enabled !== false; }
        } else if (ae.isGiven(params.enabled) && prop.canSetExpression && prop.expression) {
            prop.expressionEnabled = params.enabled === true;
        }
        if (params.value !== undefined) {
            if (prop.propertyValueType === PropertyValueType.NO_VALUE || prop.propertyValueType === PropertyValueType.CUSTOM_VALUE) {
                throw ae.fail("BadRequest", "'" + prop.name + "' has no value scripts can set.");
            }
            var value = valueIn(prop, params.value);
            if (ae.isGiven(params.time)) { prop.setValueAtTime(params.time, value); }
            else if (prop.numKeys > 0) { prop.setValueAtTime(comp.time, value); }
            else { prop.setValue(value); }
        }
        var out = describe(prop, ae.isGiven(params.time) ? params.time : comp.time, true);
        out.time = ae.round(ae.isGiven(params.time) ? params.time : comp.time);
        return out;
    });

    SnailBridge.properties = { resolve: resolve, describe: describe, valueIn: valueIn, valueOut: valueOut, pathOf: pathOf, isGroup: isGroup };
})();
