// Shapes: the contents of a shape layer (groups, rectangles, ellipses, stars, paths, fills, strokes, trim paths, repeaters and the
// other operators) and the masks of any layer, each settable by plain names that stand for the match names underneath.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    var KINDS = {
        group: "ADBE Vector Group", rect: "ADBE Vector Shape - Rect", ellipse: "ADBE Vector Shape - Ellipse", star: "ADBE Vector Shape - Star", path: "ADBE Vector Shape - Group",
        fill: "ADBE Vector Graphic - Fill", stroke: "ADBE Vector Graphic - Stroke", gradient_fill: "ADBE Vector Graphic - G-Fill", gradient_stroke: "ADBE Vector Graphic - G-Stroke",
        trim: "ADBE Vector Filter - Trim", repeater: "ADBE Vector Filter - Repeater", merge: "ADBE Vector Filter - Merge", offset: "ADBE Vector Filter - Offset",
        round_corners: "ADBE Vector Filter - RC", pucker_bloat: "ADBE Vector Filter - PB", twist: "ADBE Vector Filter - Twist", wiggle_paths: "ADBE Vector Filter - Roughen",
        wiggle_transform: "ADBE Vector Filter - Wiggler", zigzag: "ADBE Vector Filter - Zigzag"
    };

    // A plain name stands for whichever of these match names the item carries; the first found wins, so "size" is the rectangle's
    // size inside a rectangle and the zig zag's inside a zig zag.
    var SETTINGS = {
        size: ["ADBE Vector Rect Size", "ADBE Vector Ellipse Size", "ADBE Vector Roughen Size", "ADBE Vector Zigzag Size"],
        position: ["ADBE Vector Rect Position", "ADBE Vector Ellipse Position", "ADBE Vector Star Position", "ADBE Vector Position", "ADBE Vector Repeater Position"],
        roundness: ["ADBE Vector Rect Roundness"], direction: ["ADBE Vector Shape Direction"],
        type: ["ADBE Vector Star Type", "ADBE Vector Trim Type", "ADBE Vector Grad Type"], points: ["ADBE Vector Star Points", "ADBE Vector Roughen Points", "ADBE Vector Zigzag Points"],
        rotation: ["ADBE Vector Star Rotation", "ADBE Vector Rotation", "ADBE Vector Repeater Rotation"], inner_radius: ["ADBE Vector Star Inner Radius"], outer_radius: ["ADBE Vector Star Outer Radius"],
        inner_roundness: ["ADBE Vector Star Inner Roundess"], outer_roundness: ["ADBE Vector Star Outer Roundess"],
        path: ["ADBE Vector Shape"],
        color: ["ADBE Vector Fill Color", "ADBE Vector Stroke Color"], opacity: ["ADBE Vector Fill Opacity", "ADBE Vector Stroke Opacity", "ADBE Vector Group Opacity"],
        fill_rule: ["ADBE Vector Fill Rule"], blend_mode: ["ADBE Vector Blend Mode"], composite: ["ADBE Vector Composite Order"],
        width: ["ADBE Vector Stroke Width"], line_cap: ["ADBE Vector Stroke Line Cap"], line_join: ["ADBE Vector Stroke Line Join", "ADBE Vector Offset Line Join"],
        miter_limit: ["ADBE Vector Stroke Miter Limit", "ADBE Vector Offset Miter Limit"], dash_offset: ["ADBE Vector Stroke Offset"],
        taper_start_length: ["ADBE Vector Taper Start Length"], taper_end_length: ["ADBE Vector Taper End Length"], taper_start_width: ["ADBE Vector Taper Start Width"],
        taper_end_width: ["ADBE Vector Taper End Width"], taper_start_ease: ["ADBE Vector Taper Start Ease"], taper_end_ease: ["ADBE Vector Taper End Ease"],
        gradient_start: ["ADBE Vector Grad Start Pt"], gradient_end: ["ADBE Vector Grad End Pt"], highlight_length: ["ADBE Vector Grad HiLite Length"], highlight_angle: ["ADBE Vector Grad HiLite Angle"],
        start: ["ADBE Vector Trim Start"], end: ["ADBE Vector Trim End"], offset: ["ADBE Vector Trim Offset", "ADBE Vector Repeater Offset"],
        copies: ["ADBE Vector Repeater Copies"], order: ["ADBE Vector Repeater Order"], anchor: ["ADBE Vector Anchor", "ADBE Vector Repeater Anchor Point"],
        scale: ["ADBE Vector Scale", "ADBE Vector Repeater Scale"], skew: ["ADBE Vector Skew"], skew_axis: ["ADBE Vector Skew Axis"],
        start_opacity: ["ADBE Vector Repeater Start Opacity"], end_opacity: ["ADBE Vector Repeater End Opacity"],
        mode: ["ADBE Vector Merge Type"], amount: ["ADBE Vector Offset Amount", "ADBE Vector PuckerBloat Amount"], radius: ["ADBE Vector RoundCorner Radius"],
        angle: ["ADBE Vector Twist Angle"], center: ["ADBE Vector Twist Center"], detail: ["ADBE Vector Roughen Detail"], ridges: ["ADBE Vector Zigzag Detail"],
        frequency: ["ADBE Vector Temporal Freq"], correlation: ["ADBE Vector Correlation"], temporal_phase: ["ADBE Vector Temporal Phase"], spatial_phase: ["ADBE Vector Spatial Phase"],
        seed: ["ADBE Vector Random Seed"]
    };

    var COLOR_SETTINGS = { color: true };

    function shapeLayer(comp, params) {
        var l = ae.layer(comp, params.layer);
        if (!(l instanceof ShapeLayer)) { throw ae.fail("NotAShape", "'" + l.name + "' is not a shape layer."); }
        return l;
    }

    function contents(l) { return l.property("ADBE Root Vectors Group"); }

    function kindOf(item) {
        for (var kind in KINDS) { if (KINDS.hasOwnProperty(kind) && KINDS[kind] === item.matchName) { return kind; } }
        return item.matchName;
    }

    function isGroupItem(item) { return item.matchName === KINDS.group; }

    function innerContents(group) { return group.property("ADBE Vectors Group"); }

    function findWithin(item, matchName) {
        var direct = null;
        try { direct = item.property(matchName); } catch (ignored) { direct = null; }
        if (direct) { return direct; }
        if (!props.isGroup(item)) { return null; }
        for (var i = 1; i <= item.numProperties; i++) {
            var child = item.property(i);
            if (child.matchName === "ADBE Vectors Group") { continue; }
            if (props.isGroup(child)) {
                var deeper = findWithin(child, matchName);
                if (deeper) { return deeper; }
            }
        }
        return null;
    }

    function settingOf(item, name) {
        var candidates = SETTINGS[name] || [name];
        for (var i = 0; i < candidates.length; i++) {
            var found = findWithin(item, candidates[i]);
            if (found) { return found; }
        }
        return null;
    }

    function knownSettings(item) {
        var names = [];
        for (var name in SETTINGS) { if (SETTINGS.hasOwnProperty(name) && settingOf(item, name)) { names.push(name); } }
        return names;
    }

    function setDashes(item, dashes) {
        var group = findWithin(item, "ADBE Vector Stroke Dashes");
        if (!group) { throw ae.fail("BadRequest", "Only a stroke takes dashes."); }
        for (var existing = group.numProperties; existing >= 1; existing--) {
            if (group.property(existing).matchName !== "ADBE Vector Stroke Offset") { group.property(existing).remove(); }
        }
        for (var i = 0; i < dashes.length; i++) {
            var slot = Math.floor(i / 2) + 1;
            var matchName = (i % 2 === 0 ? "ADBE Vector Stroke Dash " : "ADBE Vector Stroke Gap ") + slot;
            if (!group.canAddProperty(matchName)) { throw ae.fail("BadRequest", "A stroke takes at most 3 dash and gap pairs."); }
            group.addProperty(matchName).setValue(dashes[i]);
        }
    }

    function applySettings(item, settings, time) {
        var changed = [];
        for (var name in settings) {
            if (!settings.hasOwnProperty(name) || settings[name] === undefined) { continue; }
            if (name === "dashes") {
                if (!(settings[name] instanceof Array)) { throw ae.fail("BadRequest", "dashes is a list of lengths: dash, gap, dash, gap."); }
                setDashes(item, settings[name]);
                changed.push(name);
                continue;
            }
            var target = settingOf(item, name);
            if (!target) { throw ae.fail("NoSuchSetting", "'" + kindOf(item) + " " + item.name + "' has no setting '" + name + "'.", { available: knownSettings(item) }); }
            if (props.isGroup(target)) {
                if (!ae.isGiven(settings[name]) || !target.canSetEnabled) { throw ae.fail("BadRequest", "'" + name + "' is a group; it has no value."); }
                target.enabled = settings[name] === true;
                changed.push(name);
                continue;
            }
            var raw = settings[name];
            if (COLOR_SETTINGS[name] && typeof raw === "string") { raw = ae.color(raw, name); }
            if (COLOR_SETTINGS[name] && raw instanceof Array && raw.length === 3) { raw = [raw[0], raw[1], raw[2], 1]; }
            var value = props.valueIn(target, raw);
            if (ae.isGiven(time)) { target.setValueAtTime(time, value); }
            else if (target.numKeys > 0) { target.setValueAtTime(target.propertyGroup(target.propertyDepth).containingComp.time, value); }
            else { target.setValue(value); }
            changed.push(name);
        }
        return changed;
    }

    function valuesOut(item) {
        var out = {};
        for (var name in SETTINGS) {
            if (!SETTINGS.hasOwnProperty(name)) { continue; }
            var p = settingOf(item, name);
            if (!p || props.isGroup(p) || p.propertyValueType === PropertyValueType.NO_VALUE || p.propertyValueType === PropertyValueType.CUSTOM_VALUE) { continue; }
            if (name === "path") { out[name] = { vertices: p.value.vertices.length, closed: p.value.closed }; continue; }
            out[name] = props.valueOut(p, p.value);
            if (p.numKeys > 0) { out[name + "_keys"] = p.numKeys; }
            if (p.expression) { out[name + "_expression"] = p.expression; }
        }
        var dashes = findWithin(item, "ADBE Vector Stroke Dashes");
        if (dashes && dashes.numProperties > 1) {
            var list = [];
            for (var d = 1; d <= dashes.numProperties; d++) { if (dashes.property(d).matchName !== "ADBE Vector Stroke Offset") { list.push(ae.round(dashes.property(d).value)); } }
            out.dashes = list;
        }
        return out;
    }

    function itemOut(item, depth) {
        var out = { index: item.propertyIndex, name: item.name, kind: kindOf(item), enabled: item.enabled, path: props.pathOf(item) };
        if (isGroupItem(item)) {
            out.values = valuesOut(item.property("ADBE Vector Transform Group"));
            if (depth > 0) {
                var inner = innerContents(item);
                out.contents = [];
                for (var i = 1; i <= inner.numProperties; i++) { out.contents.push(itemOut(inner.property(i), depth - 1)); }
            } else {
                out.items = innerContents(item).numProperties;
            }
        } else {
            out.values = valuesOut(item);
        }
        return out;
    }

    function containerOf(l, path) {
        if (!ae.isGiven(path) || path === "" || path === "/") { return contents(l); }
        var item = props.resolve(l, String(path));
        if (item.matchName === "ADBE Root Vectors Group" || item.matchName === "ADBE Vectors Group") { return item; }
        if (isGroupItem(item)) { return innerContents(item); }
        throw ae.fail("BadRequest", "'" + item.name + "' is a " + kindOf(item) + ", not a group; shapes go into groups or the layer's contents.");
    }

    function itemAt(l, path) {
        if (!ae.isGiven(path)) { throw ae.fail("BadRequest", "path names the item, as shape_contents lists it."); }
        var item = props.resolve(l, String(path));
        if (item === l || item.matchName === "ADBE Root Vectors Group") { throw ae.fail("BadRequest", "path must name an item inside the contents."); }
        return item;
    }

    function addItem(container, kind, name) {
        var matchName = KINDS[kind] || kind;
        if (!container.canAddProperty(matchName)) {
            var accepted = [];
            for (var k in KINDS) { if (KINDS.hasOwnProperty(k)) { accepted.push(k); } }
            throw ae.fail("BadRequest", "'" + kind + "' is not something a shape layer's contents can hold.", { accepted: accepted });
        }
        var item = container.addProperty(matchName);
        if (ae.isGiven(name)) { item.name = String(name); }
        return item;
    }

    SnailBridge.command("shape_contents", { params: ["comp", "layer", "path", "depth"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = shapeLayer(comp, params);
        var depth = ae.isGiven(params.depth) ? params.depth : 3;
        var container = containerOf(l, params.path);
        var items = [];
        for (var i = 1; i <= container.numProperties; i++) { items.push(itemOut(container.property(i), depth)); }
        return { layer: { index: l.index, name: l.name }, path: props.pathOf(container), items: items };
    });

    SnailBridge.command("shape_add", { params: ["comp", "layer", "kind", "into", "name", "settings", "fill", "stroke", "stroke_width", "at"] }, function (params) {
        var comp = ae.comp(params);
        var l = shapeLayer(comp, params);
        if (!ae.isGiven(params.kind)) { throw ae.fail("BadRequest", "kind says what to add: group, rect, ellipse, star, path, fill, stroke, trim, repeater and the rest."); }
        var kind = String(params.kind);
        var container = containerOf(l, params.into);
        var group = null;
        var wantsPaint = ae.isGiven(params.fill) || ae.isGiven(params.stroke);
        if (kind !== "group" && wantsPaint && container.matchName === "ADBE Root Vectors Group") {
            group = addItem(container, "group", params.name);
            container = innerContents(group);
        }
        var item = addItem(container, kind, group ? null : params.name);
        if (params.settings && typeof params.settings === "object") { applySettings(item, params.settings, null); }
        var itemIndex = item.propertyIndex;
        if (ae.isGiven(params.fill)) { applySettings(addItem(container, "fill", null), { color: params.fill }, null); }
        if (ae.isGiven(params.stroke)) {
            var stroke = addItem(container, "stroke", null);
            applySettings(stroke, { color: params.stroke }, null);
            if (ae.isGiven(params.stroke_width)) { applySettings(stroke, { width: params.stroke_width }, null); }
        }
        // A reference to an item goes stale once siblings are added beside it, so the item is read back by its index.
        item = container.property(itemIndex);
        if (ae.isGiven(params.at) && params.at >= 1 && params.at <= container.numProperties) { item.moveTo(params.at); item = container.property(params.at); }
        var out = { layer: { index: l.index, name: l.name }, item: itemOut(item, 1) };
        if (group) { out.group = itemOut(contents(l).property(group.propertyIndex), 1); }
        return out;
    });

    SnailBridge.command("shape_update", { params: ["comp", "layer", "path", "name", "enabled", "settings", "time", "at"] }, function (params) {
        var comp = ae.comp(params);
        var l = shapeLayer(comp, params);
        var item = itemAt(l, params.path);
        var changed = [];
        if (ae.isGiven(params.name)) { item.name = String(params.name); changed.push("name"); }
        if (ae.isGiven(params.enabled)) { item.enabled = params.enabled === true; changed.push("enabled"); }
        if (params.settings && typeof params.settings === "object") {
            var target = isGroupItem(item) ? item.property("ADBE Vector Transform Group") : item;
            changed = changed.concat(applySettings(target, params.settings, params.time));
        }
        if (ae.isGiven(params.at)) { item.moveTo(params.at); changed.push("at"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass name, enabled, settings or at."); }
        return { layer: { index: l.index, name: l.name }, changed: changed, item: itemOut(item, 1) };
    });

    SnailBridge.command("shape_remove", { params: ["comp", "layer", "path"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var l = shapeLayer(comp, params);
        var item = itemAt(l, params.path);
        var removed = { name: item.name, kind: kindOf(item) };
        var parent = item.parentProperty;
        item.remove();
        return { layer: { index: l.index, name: l.name }, removed: removed, remaining: parent.numProperties };
    });

    var ELLIPSE_K = 0.5522847498;

    function rectangle(bounds) {
        var x = bounds[0], y = bounds[1], w = bounds[2], h = bounds[3];
        return { vertices: [[x, y], [x + w, y], [x + w, y + h], [x, y + h]], in_tangents: [[0, 0], [0, 0], [0, 0], [0, 0]], out_tangents: [[0, 0], [0, 0], [0, 0], [0, 0]], closed: true };
    }

    function ellipse(bounds) {
        var rx = bounds[2] / 2, ry = bounds[3] / 2, cx = bounds[0] + rx, cy = bounds[1] + ry, kx = ELLIPSE_K * rx, ky = ELLIPSE_K * ry;
        return {
            vertices: [[cx, cy - ry], [cx + rx, cy], [cx, cy + ry], [cx - rx, cy]],
            in_tangents: [[-kx, 0], [0, -ky], [kx, 0], [0, ky]],
            out_tangents: [[kx, 0], [0, ky], [-kx, 0], [0, -ky]],
            closed: true
        };
    }

    function maskShapeOf(params) {
        if (ae.isGiven(params.rectangle)) { return rectangle(params.rectangle); }
        if (ae.isGiven(params.ellipse)) { return ellipse(params.ellipse); }
        if (ae.isGiven(params.vertices)) { return { vertices: params.vertices, in_tangents: params.in_tangents, out_tangents: params.out_tangents, closed: params.closed }; }
        return null;
    }

    // The shape to write: the new one when given, else the current one when only its feather points change.
    function maskShapeToWrite(mask, params, comp) {
        var shapeValue = maskShapeOf(params);
        if (!shapeValue && ae.isGiven(params.feather_points)) {
            var shapeProp = mask.property("ADBE Mask Shape");
            shapeValue = props.valueOut(shapeProp, ae.isGiven(params.time) ? shapeProp.valueAtTime(params.time, false) : shapeProp.valueAtTime(comp.time, false));
        }
        if (shapeValue && ae.isGiven(params.feather_points)) { shapeValue.feather_points = params.feather_points; }
        return shapeValue;
    }

    function masks(l) { return l.property("ADBE Mask Parade"); }

    function maskOf(l, ref) {
        var parade = masks(l);
        if (typeof ref === "number") {
            if (ref < 1 || ref > parade.numProperties) { throw ae.fail("NotFound", "'" + l.name + "' has no mask " + ref + "; it has " + parade.numProperties + "."); }
            return parade.property(ref);
        }
        if (typeof ref !== "string" || ref === "") { throw ae.fail("BadRequest", "Name the mask by its name or its index."); }
        for (var i = 1; i <= parade.numProperties; i++) { if (parade.property(i).name === ref) { return parade.property(i); } }
        throw ae.fail("NotFound", "'" + l.name + "' has no mask named '" + ref + "'.");
    }

    function maskOut(mask, time) {
        var shape = mask.property("ADBE Mask Shape");
        var value = ae.isGiven(time) ? shape.valueAtTime(time, false) : shape.value;
        var out = {
            index: mask.propertyIndex, name: mask.name, path: props.pathOf(mask), mode: ae.enumName(MaskMode, mask.maskMode), inverted: mask.inverted,
            locked: mask.locked, roto_bezier: mask.rotoBezier, color: ae.numbers(mask.color), vertices: value.vertices.length, closed: value.closed,
            feather: ae.numbers(mask.property("ADBE Mask Feather").value), opacity: mask.property("ADBE Mask Opacity").value, expansion: mask.property("ADBE Mask Offset").value,
            shape_keys: shape.numKeys
        };
        try { out.motion_blur = ae.enumName(MaskMotionBlur, mask.maskMotionBlur); } catch (ignored) { }
        try { out.feather_falloff = ae.enumName(MaskFeatherFalloff, mask.maskFeatherFalloff); } catch (ignored2) { }
        var featherPoints = props.valueOut(shape, value).feather_points;
        if (featherPoints) { out.feather_points = featherPoints; }
        return out;
    }

    function applyMask(mask, params, comp) {
        var changed = [];
        var shapeValue = maskShapeToWrite(mask, params, comp);
        if (shapeValue) {
            var shapeProp = mask.property("ADBE Mask Shape");
            var shape = props.valueIn(shapeProp, shapeValue);
            if (ae.isGiven(params.time)) { shapeProp.setValueAtTime(params.time, shape); }
            else if (shapeProp.numKeys > 0) { shapeProp.setValueAtTime(comp.time, shape); }
            else { shapeProp.setValue(shape); }
            changed.push("shape");
        }
        if (ae.isGiven(params.name)) { mask.name = String(params.name); changed.push("name"); }
        if (ae.isGiven(params.mode)) { mask.maskMode = ae.enumValue(MaskMode, params.mode, "mask mode"); changed.push("mode"); }
        if (ae.isGiven(params.inverted)) { mask.inverted = params.inverted === true; changed.push("inverted"); }
        if (ae.isGiven(params.locked)) { mask.locked = params.locked === true; changed.push("locked"); }
        if (ae.isGiven(params.roto_bezier)) { mask.rotoBezier = params.roto_bezier === true; changed.push("roto_bezier"); }
        if (ae.isGiven(params.color)) { mask.color = ae.color(params.color, "color"); changed.push("color"); }
        if (ae.isGiven(params.feather_falloff)) { mask.maskFeatherFalloff = ae.enumValue(MaskFeatherFalloff, params.feather_falloff, "feather falloff"); changed.push("feather_falloff"); }
        if (ae.isGiven(params.motion_blur)) { mask.maskMotionBlur = ae.enumValue(MaskMotionBlur, params.motion_blur, "mask motion blur"); changed.push("motion_blur"); }
        var numeric = [["feather", "ADBE Mask Feather"], ["opacity", "ADBE Mask Opacity"], ["expansion", "ADBE Mask Offset"]];
        for (var i = 0; i < numeric.length; i++) {
            if (!ae.isGiven(params[numeric[i][0]])) { continue; }
            var p = mask.property(numeric[i][1]);
            var raw = params[numeric[i][0]];
            if (numeric[i][0] === "feather" && typeof raw === "number") { raw = [raw, raw]; }
            if (ae.isGiven(params.time)) { p.setValueAtTime(params.time, raw); } else if (p.numKeys > 0) { p.setValueAtTime(comp.time, raw); } else { p.setValue(raw); }
            changed.push(numeric[i][0]);
        }
        return changed;
    }

    SnailBridge.command("masks_list", { params: ["comp", "layer", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var parade = masks(l);
        if (!parade) { throw ae.fail("BadRequest", "'" + l.name + "' cannot carry masks."); }
        var out = [];
        for (var i = 1; i <= parade.numProperties; i++) { out.push(maskOut(parade.property(i), params.time)); }
        return { layer: { index: l.index, name: l.name }, masks: out };
    });

    SnailBridge.command("mask_add", {
        params: ["comp", "layer", "name", "vertices", "in_tangents", "out_tangents", "closed", "rectangle", "ellipse", "mode", "inverted", "locked", "roto_bezier", "color", "feather", "opacity", "expansion", "time",
            "feather_points", "feather_falloff", "motion_blur"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var parade = masks(l);
        if (!parade || !parade.canAddProperty("ADBE Mask Atom")) { throw ae.fail("BadRequest", "'" + l.name + "' cannot carry masks."); }
        if (!maskShapeOf(params)) { throw ae.fail("BadRequest", "Give the mask a shape: rectangle or ellipse as [x, y, width, height], or vertices with tangents."); }
        var mask = parade.addProperty("ADBE Mask Atom");
        applyMask(mask, params, comp);
        return { layer: { index: l.index, name: l.name }, mask: maskOut(mask, params.time) };
    });

    SnailBridge.command("mask_update", {
        params: ["comp", "layer", "name", "vertices", "in_tangents", "out_tangents", "closed", "rectangle", "ellipse", "mode", "inverted", "locked", "roto_bezier", "color", "feather", "opacity", "expansion", "time", "mask",
            "feather_points", "feather_falloff", "motion_blur"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var mask = maskOf(l, params.mask);
        var changed = applyMask(mask, params, comp);
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass a shape, name, mode, inverted, feather, feather points or falloff, opacity, expansion, color or locked."); }
        return { layer: { index: l.index, name: l.name }, changed: changed, mask: maskOut(mask, params.time) };
    });

    SnailBridge.command("mask_remove", { params: ["comp", "layer", "mask"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var mask = maskOf(l, params.mask);
        var removed = mask.name;
        mask.remove();
        return { layer: { index: l.index, name: l.name }, removed: removed, remaining: masks(l).numProperties };
    });
})();
