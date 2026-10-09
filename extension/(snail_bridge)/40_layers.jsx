// Layers of a composition: listing, adding every kind, their switches and timing, their transform, order and selection.
(function () {
    var ae = SnailBridge.ae;

    function brief(l) {
        return {
            index: l.index,
            name: l.name,
            kind: SnailBridge.ae.describeLayer(l).kind,
            enabled: l.enabled,
            selected: l.selected,
            in_point: ae.round(l.inPoint),
            out_point: ae.round(l.outPoint),
            parent: l.parent ? l.parent.index : null,
            source: l.source ? l.source.name : null
        };
    }

    SnailBridge.command("layers_list", { params: ["comp", "kind", "name_contains", "limit"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var needle = ae.isGiven(params.name_contains) ? String(params.name_contains).toLowerCase() : null;
        var limit = ae.isGiven(params.limit) ? params.limit : 200;
        var found = [];
        var total = 0;
        for (var i = 1; i <= comp.numLayers; i++) {
            var l = comp.layer(i);
            var described = brief(l);
            if (ae.isGiven(params.kind) && described.kind !== params.kind) { continue; }
            if (needle !== null && l.name.toLowerCase().indexOf(needle) < 0) { continue; }
            total++;
            if (found.length < limit) { found.push(described); }
        }
        return { comp: { id: comp.id, name: comp.name }, total: total, layers: found, truncated: total > found.length };
    });

    SnailBridge.command("layer_info", { params: ["comp", "layer"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var out = ae.describeLayer(l);
        var masks = l.property("ADBE Mask Parade");
        out.masks = masks ? masks.numProperties : 0;
        return out;
    });

    function placeAt(comp, l, at) {
        if (!ae.isGiven(at)) { return; }
        if (at >= comp.numLayers) { l.moveToEnd(); return; }
        if (at <= 1) { l.moveToBeginning(); return; }
        l.moveBefore(comp.layer(at + 1));
    }

    SnailBridge.command("layer_add", {
        params: ["comp", "kind", "name", "item", "text", "vertical", "color", "width", "height", "duration", "position", "light_type", "start_time", "in_point", "out_point", "at"]
    }, function (params) {
        var comp = ae.comp(params);
        var kind = ae.isGiven(params.kind) ? params.kind : (ae.isGiven(params.item) ? "item" : "solid");
        var duration = ae.isGiven(params.duration) ? params.duration : comp.duration;
        var centre = [comp.width / 2, comp.height / 2];
        var l;
        if (kind === "solid" || kind === "adjustment") {
            var color = kind === "adjustment" ? [1, 1, 1] : (ae.isGiven(params.color) ? ae.color(params.color) : [0.5, 0.5, 0.5]);
            l = comp.layers.addSolid(color, ae.isGiven(params.name) ? params.name : (kind === "adjustment" ? "Adjustment" : "Solid"),
                ae.isGiven(params.width) ? params.width : comp.width, ae.isGiven(params.height) ? params.height : comp.height, comp.pixelAspect, duration);
            if (kind === "adjustment") { l.adjustmentLayer = true; }
        } else if (kind === "null") {
            l = comp.layers.addNull(duration);
        } else if (kind === "text") {
            var text = ae.isGiven(params.text) ? params.text : "";
            var boxed = ae.isGiven(params.width) && ae.isGiven(params.height);
            if (params.vertical === true) { l = boxed ? comp.layers.addVerticalBoxText([params.width, params.height], text) : comp.layers.addVerticalText(text); }
            else { l = boxed ? comp.layers.addBoxText([params.width, params.height], text) : comp.layers.addText(text); }
        } else if (kind === "shape") {
            l = comp.layers.addShape();
        } else if (kind === "camera") {
            l = comp.layers.addCamera(ae.isGiven(params.name) ? params.name : "Camera", centre);
        } else if (kind === "light") {
            l = comp.layers.addLight(ae.isGiven(params.name) ? params.name : "Light", centre);
            if (ae.isGiven(params.light_type)) { l.lightType = ae.enumValue(LightType, params.light_type, "light type"); }
        } else if (kind === "item" || kind === "footage" || kind === "comp") {
            if (!ae.isGiven(params.item)) { throw ae.fail("BadRequest", "A footage or composition layer needs item: the project item to put on the layer."); }
            var source = ae.item(params.item);
            if (!(source instanceof FootageItem) && !(source instanceof CompItem)) { throw ae.fail("BadRequest", "'" + source.name + "' is a " + ae.kindOf(source) + " and cannot be a layer."); }
            if (source === comp) { throw ae.fail("BadRequest", "A composition cannot be placed inside itself."); }
            l = ae.isGiven(params.duration) ? comp.layers.add(source, params.duration) : comp.layers.add(source);
        } else {
            throw ae.fail("BadRequest", "kind must be solid, null, adjustment, text, shape, camera, light, or item for footage and compositions.", { accepted: ["solid", "null", "adjustment", "text", "shape", "camera", "light", "item"] });
        }
        if (ae.isGiven(params.name) && kind !== "camera" && kind !== "light" && !(kind === "solid" || kind === "adjustment")) { l.name = params.name; }
        if (ae.isGiven(params.position) && l.property("ADBE Transform Group").property("ADBE Position")) {
            l.property("ADBE Transform Group").property("ADBE Position").setValue(params.position);
        }
        if (ae.isGiven(params.start_time)) { l.startTime = params.start_time; }
        if (ae.isGiven(params.in_point)) { l.inPoint = params.in_point; }
        if (ae.isGiven(params.out_point)) { l.outPoint = params.out_point; }
        placeAt(comp, l, params.at);
        return ae.describeLayer(l);
    });

    SnailBridge.command("layer_update", {
        params: ["comp", "layer", "name", "enabled", "solo", "shy", "locked", "in_point", "out_point", "start_time", "stretch", "parent", "blending_mode",
            "three_d", "motion_blur", "adjustment", "guide", "label", "comment", "quality", "audio_enabled", "collapse", "time_remap", "track_matte", "track_matte_type",
            "auto_orient", "preserve_transparency", "effects_active", "frame_blending", "parent_jump"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (ae.isGiven(params.name)) { l.name = params.name; }
        if (ae.isGiven(params.enabled)) { l.enabled = params.enabled === true; }
        if (ae.isGiven(params.solo)) { l.solo = params.solo === true; }
        if (ae.isGiven(params.shy)) { l.shy = params.shy === true; }
        if (ae.isGiven(params.locked)) { l.locked = params.locked === true; }
        if (ae.isGiven(params.start_time)) { l.startTime = params.start_time; }
        if (ae.isGiven(params.in_point)) { l.inPoint = params.in_point; }
        if (ae.isGiven(params.out_point)) { l.outPoint = params.out_point; }
        if (ae.isGiven(params.stretch)) { l.stretch = params.stretch; }
        if (params.parent !== undefined) {
            var parent = params.parent === null || params.parent === "" ? null : ae.layer(comp, params.parent);
            if (params.parent_jump === true && parent) { l.setParentWithJump(parent); } else { l.parent = parent; }
        }
        if (ae.isGiven(params.blending_mode)) { l.blendingMode = ae.enumValue(BlendingMode, params.blending_mode, "blending mode"); }
        if (ae.isGiven(params.three_d)) { l.threeDLayer = params.three_d === true; }
        if (ae.isGiven(params.motion_blur)) { l.motionBlur = params.motion_blur === true; }
        if (ae.isGiven(params.adjustment)) { l.adjustmentLayer = params.adjustment === true; }
        if (ae.isGiven(params.guide)) { l.guideLayer = params.guide === true; }
        if (ae.isGiven(params.label)) { l.label = params.label; }
        if (ae.isGiven(params.comment)) { l.comment = params.comment; }
        if (ae.isGiven(params.quality)) { l.quality = ae.enumValue(LayerQuality, params.quality, "layer quality"); }
        if (ae.isGiven(params.audio_enabled) && l.hasAudio) { l.audioEnabled = params.audio_enabled === true; }
        if (ae.isGiven(params.collapse)) { l.collapseTransformation = params.collapse === true; }
        if (ae.isGiven(params.time_remap)) { l.timeRemapEnabled = params.time_remap === true; }
        if (ae.isGiven(params.auto_orient)) { l.autoOrient = ae.enumValue(AutoOrientType, params.auto_orient, "auto-orient mode"); }
        if (ae.isGiven(params.preserve_transparency)) { l.preserveTransparency = params.preserve_transparency === true; }
        if (ae.isGiven(params.effects_active)) { l.effectsActive = params.effects_active === true; }
        if (ae.isGiven(params.frame_blending)) { l.frameBlendingType = ae.enumValue(FrameBlendingType, params.frame_blending, "frame blending type"); }
        if (ae.isGiven(params.track_matte) || ae.isGiven(params.track_matte_type)) { setTrackMatte(comp, l, { layer: params.track_matte, type: params.track_matte_type }); }
        return ae.describeLayer(l);
    });

    function setTrackMatte(comp, l, matte) {
        var type = ae.isGiven(matte.type) ? String(matte.type).toUpperCase() : "ALPHA";
        if (type === "NONE") {
            if (typeof l.removeTrackMatte === "function") { l.removeTrackMatte(); } else { l.trackMatteType = TrackMatteType.NO_TRACK_MATTE; }
            return;
        }
        var kind = ae.enumValue(TrackMatteType, type, "track matte type");
        if (typeof l.setTrackMatte === "function") {
            if (!ae.isGiven(matte.layer)) { throw ae.fail("BadRequest", "track_matte needs layer: the layer that serves as the matte."); }
            l.setTrackMatte(ae.layer(comp, matte.layer), kind);
        } else {
            l.trackMatteType = kind;
        }
    }

    function setTransform(l, key, matchName, value, time) {
        var prop = l.property("ADBE Transform Group").property(matchName);
        if (!prop) { throw ae.fail("BadRequest", "'" + l.name + "' has no " + key + "."); }
        if (value instanceof Array && prop.value instanceof Array && value.length < prop.value.length) {
            var padded = value.slice(0);
            for (var i = value.length; i < prop.value.length; i++) { padded.push(prop.value[i]); }
            value = padded;
        }
        if (prop.numKeys > 0) { prop.setValueAtTime(time, value); } else { prop.setValue(value); }
    }

    SnailBridge.command("layer_transform", {
        params: ["comp", "layer", "anchor", "position", "scale", "rotation", "opacity", "orientation", "x_rotation", "y_rotation", "time"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var names = ae.transformMatchNames;
        for (var i = 0; i < names.length; i++) {
            if (ae.isGiven(params[names[i][0]])) { setTransform(l, names[i][0], names[i][1], params[names[i][0]], time); }
        }
        return { index: l.index, name: l.name, time: ae.round(time), transform: ae.describeTransform(l) };
    });

    SnailBridge.command("layer_remove", { params: ["comp", "layers"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        if (!(params.layers instanceof Array) || params.layers.length === 0) { throw ae.fail("BadRequest", "layers must name at least one layer, by name or index."); }
        var picked = ae.layers(comp, params.layers);
        var removed = [];
        for (var i = 0; i < picked.length; i++) { removed.push({ index: picked[i].index, name: picked[i].name }); }
        picked.sort(function (a, b) { return b.index - a.index; });
        for (var j = 0; j < picked.length; j++) { picked[j].remove(); }
        return { removed: removed, layers: comp.numLayers };
    });

    SnailBridge.command("layer_duplicate", { params: ["comp", "layer", "name"] }, function (params) {
        var comp = ae.comp(params);
        var copy = ae.layer(comp, params.layer).duplicate();
        if (ae.isGiven(params.name)) { copy.name = params.name; }
        return ae.describeLayer(copy);
    });

    SnailBridge.command("layer_move", { params: ["comp", "layer", "to", "before", "after"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (ae.isGiven(params.before)) { l.moveBefore(ae.layer(comp, params.before)); }
        else if (ae.isGiven(params.after)) { l.moveAfter(ae.layer(comp, params.after)); }
        else if (params.to === "top") { l.moveToBeginning(); }
        else if (params.to === "bottom") { l.moveToEnd(); }
        else if (typeof params.to === "number") {
            if (params.to <= 1) { l.moveToBeginning(); }
            else if (params.to >= comp.numLayers) { l.moveToEnd(); }
            else if (params.to > l.index) { l.moveAfter(comp.layer(params.to)); }
            else { l.moveBefore(comp.layer(params.to)); }
        } else {
            throw ae.fail("BadRequest", "Say where: to (an index, top or bottom), before or after another layer.");
        }
        return { index: l.index, name: l.name };
    });

    SnailBridge.command("layers_select", { params: ["comp", "layers", "mode"] }, function (params) {
        var comp = ae.comp(params);
        var mode = ae.isGiven(params.mode) ? params.mode : "replace";
        if (mode === "replace" || mode === "none" || mode === "all") {
            for (var i = 1; i <= comp.numLayers; i++) { comp.layer(i).selected = mode === "all"; }
        }
        if (mode !== "none" && mode !== "all") {
            var picked = ae.layers(comp, params.layers instanceof Array ? params.layers : []);
            for (var p = 0; p < picked.length; p++) { picked[p].selected = true; }
        }
        var selected = [];
        for (var s = 1; s <= comp.numLayers; s++) { if (comp.layer(s).selected) { selected.push({ index: s, name: comp.layer(s).name }); } }
        return { selected: selected };
    });
})();
