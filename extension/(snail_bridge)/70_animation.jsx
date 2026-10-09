// Animation: keyframes, interpolation and easing, expressions checked for errors, markers, time remapping.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    var EASE_INFLUENCE = 33.3333;
    var PRESETS = ["easy_ease", "ease_in", "ease_out", "linear", "hold"];

    function animatable(comp, params) {
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the property, as property_tree lists it."); }
        var prop = props.resolve(l, params.path);
        if (props.isGroup(prop)) { throw ae.fail("BadRequest", "'" + prop.name + "' is a group; keyframes live on the properties inside it."); }
        if (!prop.canVaryOverTime) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot be animated."); }
        return { layer: l, prop: prop };
    }

    function easeOut(eases) {
        var out = [];
        for (var i = 0; i < eases.length; i++) { out.push({ speed: ae.round(eases[i].speed), influence: ae.round(eases[i].influence) }); }
        return out;
    }

    function keyOut(prop, index) {
        var key = {
            index: index,
            time: ae.round(prop.keyTime(index)),
            value: props.valueOut(prop, prop.keyValue(index)),
            in_interpolation: ae.enumName(KeyframeInterpolationType, prop.keyInInterpolationType(index)),
            out_interpolation: ae.enumName(KeyframeInterpolationType, prop.keyOutInterpolationType(index)),
            selected: prop.keySelected(index)
        };
        try { key.in_ease = easeOut(prop.keyInTemporalEase(index)); key.out_ease = easeOut(prop.keyOutTemporalEase(index)); } catch (ignored) { }
        try { key.continuous = prop.keyTemporalContinuous(index); key.auto_bezier = prop.keyTemporalAutoBezier(index); } catch (ignored2) { }
        if (prop.isSpatial) {
            try { key.roving = prop.keyRoving(index); } catch (ignored3) { }
            try { key.in_spatial_tangent = ae.numbers(prop.keyInSpatialTangent(index)); key.out_spatial_tangent = ae.numbers(prop.keyOutSpatialTangent(index)); } catch (ignored4) { }
            try { key.spatial_continuous = prop.keySpatialContinuous(index); key.spatial_auto_bezier = prop.keySpatialAutoBezier(index); } catch (ignored5) { }
        }
        try { key.label = prop.keyLabel(index); } catch (ignored6) { }
        return key;
    }

    function keysOut(l, prop) {
        var keys = [];
        for (var i = 1; i <= prop.numKeys; i++) { keys.push(keyOut(prop, i)); }
        var out = { layer: { index: l.index, name: l.name }, property: prop.name, path: props.pathOf(prop), value_type: ae.enumName(PropertyValueType, prop.propertyValueType), keys: keys };
        if (prop.isSpatial) { out.spatial = true; }
        if (prop.expression) { out.expression = prop.expression; out.expression_enabled = prop.expressionEnabled; }
        return out;
    }

    function halfFrame(comp) { return comp.frameDuration / 2; }

    // Which keys a call means: the indices it lists, the keys at the times it lists, or all of them.
    function pick(comp, prop, params) {
        var picked = [];
        var i;
        if (params.indices instanceof Array) {
            for (i = 0; i < params.indices.length; i++) {
                var index = params.indices[i];
                if (typeof index !== "number" || index < 1 || index > prop.numKeys) { throw ae.fail("NotFound", "'" + prop.name + "' has no keyframe " + index + "; it has " + prop.numKeys + "."); }
                picked.push(index);
            }
            return picked;
        }
        if (params.times instanceof Array) {
            for (i = 0; i < params.times.length; i++) {
                var time = params.times[i];
                if (prop.numKeys === 0) { throw ae.fail("NotFound", "'" + prop.name + "' has no keyframes."); }
                var nearest = prop.nearestKeyIndex(time);
                if (Math.abs(prop.keyTime(nearest) - time) > halfFrame(comp)) { throw ae.fail("NotFound", "'" + prop.name + "' has no keyframe at " + time + " s; the nearest is at " + ae.round(prop.keyTime(nearest)) + " s."); }
                picked.push(nearest);
            }
            return picked;
        }
        for (i = 1; i <= prop.numKeys; i++) { picked.push(i); }
        return picked;
    }

    function removeKeys(prop, indices) {
        var sorted = indices.slice(0).sort(function (a, b) { return b - a; });
        var last = null;
        for (var i = 0; i < sorted.length; i++) {
            if (sorted[i] === last) { continue; }
            prop.removeKey(sorted[i]);
            last = sorted[i];
        }
    }

    function eases(speed, influence, count) {
        var out = [];
        for (var i = 0; i < count; i++) { out.push(new KeyframeEase(speed, influence)); }
        return out;
    }

    function withEaseCount(prop, index, apply) {
        var counts = [1, 2, 3, 4];
        try { counts.unshift(prop.keyInTemporalEase(index).length); } catch (ignored) { }
        var lastError = null;
        for (var i = 0; i < counts.length; i++) {
            try { apply(counts[i]); return; }
            catch (error) { lastError = error; }
        }
        throw lastError;
    }

    function interpolation(name, what) {
        return ae.enumValue(KeyframeInterpolationType, name, what);
    }

    // One side of a key: its interpolation and, when bezier, its ease; the other side is read back so it is left as it was.
    function shape(prop, index, spec) {
        var inType = ae.isGiven(spec.in_interpolation) ? interpolation(spec.in_interpolation, "interpolation") : prop.keyInInterpolationType(index);
        var outType = ae.isGiven(spec.out_interpolation) ? interpolation(spec.out_interpolation, "interpolation") : prop.keyOutInterpolationType(index);
        var easeIn = ae.isGiven(spec.influence_in) || ae.isGiven(spec.speed_in);
        var easeOutGiven = ae.isGiven(spec.influence_out) || ae.isGiven(spec.speed_out);
        if (easeIn && inType === KeyframeInterpolationType.LINEAR) { inType = KeyframeInterpolationType.BEZIER; }
        if (easeOutGiven && outType === KeyframeInterpolationType.LINEAR) { outType = KeyframeInterpolationType.BEZIER; }
        prop.setInterpolationTypeAtKey(index, inType, outType);
        if (easeIn || easeOutGiven) {
            withEaseCount(prop, index, function (count) {
                var currentIn = prop.keyInTemporalEase(index);
                var currentOut = prop.keyOutTemporalEase(index);
                var nextIn = easeIn ? eases(ae.isGiven(spec.speed_in) ? spec.speed_in : 0, ae.isGiven(spec.influence_in) ? spec.influence_in : EASE_INFLUENCE, count) : currentIn;
                var nextOut = easeOutGiven ? eases(ae.isGiven(spec.speed_out) ? spec.speed_out : 0, ae.isGiven(spec.influence_out) ? spec.influence_out : EASE_INFLUENCE, count) : currentOut;
                prop.setTemporalEaseAtKey(index, nextIn, nextOut);
            });
        }
        if (ae.isGiven(spec.continuous)) { prop.setTemporalContinuousAtKey(index, spec.continuous === true); }
        if (ae.isGiven(spec.auto_bezier)) { prop.setTemporalAutoBezierAtKey(index, spec.auto_bezier === true); }
        if (ae.isGiven(spec.roving) && prop.isSpatial) { prop.setRovingAtKey(index, spec.roving === true); }
        if (prop.isSpatial && (spec.in_tangent instanceof Array || spec.out_tangent instanceof Array)) {
            var inTangent = spec.in_tangent instanceof Array ? spec.in_tangent : prop.keyInSpatialTangent(index);
            var outTangent = spec.out_tangent instanceof Array ? spec.out_tangent : prop.keyOutSpatialTangent(index);
            prop.setSpatialTangentsAtKey(index, inTangent, outTangent);
        }
        if (ae.isGiven(spec.spatial_continuous) && prop.isSpatial) { prop.setSpatialContinuousAtKey(index, spec.spatial_continuous === true); }
        if (ae.isGiven(spec.spatial_auto_bezier) && prop.isSpatial) { prop.setSpatialAutoBezierAtKey(index, spec.spatial_auto_bezier === true); }
        if (ae.isGiven(spec.label)) { prop.setLabelAtKey(index, spec.label); }
    }

    // The presets the Animation menu offers, spelled as the keyframe assistant names them.
    function presetSpec(name) {
        if (name === "easy_ease") { return { in_interpolation: "BEZIER", out_interpolation: "BEZIER", influence_in: EASE_INFLUENCE, influence_out: EASE_INFLUENCE, speed_in: 0, speed_out: 0 }; }
        if (name === "ease_in") { return { in_interpolation: "BEZIER", influence_in: EASE_INFLUENCE, speed_in: 0 }; }
        if (name === "ease_out") { return { out_interpolation: "BEZIER", influence_out: EASE_INFLUENCE, speed_out: 0 }; }
        if (name === "linear") { return { in_interpolation: "LINEAR", out_interpolation: "LINEAR" }; }
        if (name === "hold") { return { out_interpolation: "HOLD" }; }
        throw ae.fail("BadRequest", "'" + name + "' is not an easing preset.", { accepted: PRESETS });
    }

    function merge(base, over) {
        var out = {};
        var k;
        for (k in base) { if (base.hasOwnProperty(k)) { out[k] = base[k]; } }
        for (k in over) { if (over.hasOwnProperty(k) && ae.isGiven(over[k])) { out[k] = over[k]; } }
        return out;
    }

    function specOf(params) {
        var explicit = {
            in_interpolation: params.in_interpolation, out_interpolation: params.out_interpolation,
            influence_in: params.influence_in, influence_out: params.influence_out, speed_in: params.speed_in, speed_out: params.speed_out,
            continuous: params.continuous, auto_bezier: params.auto_bezier, roving: params.roving,
            in_tangent: params.in_tangent, out_tangent: params.out_tangent, spatial_continuous: params.spatial_continuous, spatial_auto_bezier: params.spatial_auto_bezier,
            label: params.label
        };
        if (ae.isGiven(params.preset)) { return merge(presetSpec(String(params.preset)), explicit); }
        return explicit;
    }

    function hasShape(spec) {
        for (var k in spec) { if (spec.hasOwnProperty(k) && ae.isGiven(spec[k])) { return true; } }
        return false;
    }

    SnailBridge.command("keyframes_list", { params: ["comp", "layer", "path"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var target = animatable(comp, params);
        return keysOut(target.layer, target.prop);
    });

    SnailBridge.command("keyframes_set", {
        params: ["comp", "layer", "path", "keys", "replace", "preset", "in_interpolation", "out_interpolation", "influence_in", "influence_out", "speed_in", "speed_out", "continuous", "auto_bezier", "roving", "in_tangent", "out_tangent", "spatial_continuous", "spatial_auto_bezier", "label"]
    }, function (params) {
        var comp = ae.comp(params);
        var target = animatable(comp, params);
        var prop = target.prop;
        if (!(params.keys instanceof Array) || params.keys.length === 0) { throw ae.fail("BadRequest", "keys lists the keyframes to set, each as {time, value}."); }
        if (params.replace === true) { removeKeys(prop, pick(comp, prop, {})); }
        var common = specOf(params);
        var set = [];
        var times = [];
        var values = [];
        for (var i = 0; i < params.keys.length; i++) {
            var key = params.keys[i];
            if (!key || typeof key.time !== "number" || key.value === undefined) { throw ae.fail("BadRequest", "Keyframe " + (i + 1) + " needs time in seconds and a value."); }
            times.push(key.time);
            values.push(props.valueIn(prop, key.value));
        }
        // Many keys of plain values go in one call; a text document or a shape goes one at a time, as the batch call takes numbers only.
        var plain = times.length > 1 && typeof values[0] !== "object" || (values[0] instanceof Array);
        if (plain) { prop.setValuesAtTimes(times, values); }
        else { for (var v = 0; v < times.length; v++) { prop.setValueAtTime(times[v], values[v]); } }
        for (var n = 0; n < times.length; n++) { set.push(prop.nearestKeyIndex(times[n])); }
        for (var s = 0; s < set.length; s++) {
            var own = merge(common, params.keys[s]);
            delete own.time;
            delete own.value;
            if (hasShape(own)) { shape(prop, set[s], own); }
        }
        var out = keysOut(target.layer, prop);
        out.set = set.length;
        return out;
    });

    SnailBridge.command("keyframes_remove", { params: ["comp", "layer", "path", "indices", "times"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var target = animatable(comp, params);
        var picked = pick(comp, target.prop, params);
        removeKeys(target.prop, picked);
        var out = keysOut(target.layer, target.prop);
        out.removed = picked.length;
        return out;
    });

    SnailBridge.command("keyframes_ease", {
        params: ["comp", "layer", "path", "indices", "times", "preset", "in_interpolation", "out_interpolation", "influence_in", "influence_out", "speed_in", "speed_out", "continuous", "auto_bezier", "roving", "in_tangent", "out_tangent", "spatial_continuous", "spatial_auto_bezier", "label"]
    }, function (params) {
        var comp = ae.comp(params);
        var target = animatable(comp, params);
        var prop = target.prop;
        if (prop.numKeys === 0) { throw ae.fail("NoKeyframes", "'" + prop.name + "' has no keyframes to ease; set some first."); }
        var spec = specOf(params);
        if (!hasShape(spec)) { throw ae.fail("BadRequest", "Say how to ease: a preset, or interpolation, influence and speed for each side.", { presets: PRESETS }); }
        var picked = pick(comp, prop, params);
        for (var i = 0; i < picked.length; i++) { shape(prop, picked[i], spec); }
        var out = keysOut(target.layer, prop);
        out.eased = picked.length;
        return out;
    });

    SnailBridge.command("expression_set", { params: ["comp", "layer", "path", "expression", "enabled", "check_only"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the property, as property_tree lists it."); }
        var prop = props.resolve(l, params.path);
        if (props.isGroup(prop) || !prop.canSetExpression) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot take an expression."); }
        var previous = { expression: prop.expression, enabled: prop.expressionEnabled };
        var expression = ae.isGiven(params.expression) ? String(params.expression) : "";
        prop.expression = expression;
        if (expression !== "") { prop.expressionEnabled = params.enabled !== false; }
        var error = prop.expressionError || "";
        var out = { layer: { index: l.index, name: l.name }, property: prop.name, path: props.pathOf(prop), expression: expression, enabled: expression !== "" && prop.expressionEnabled, valid: error === "" };
        if (error === "") {
            try { out.value = props.valueOut(prop, prop.valueAtTime(comp.time, false)); } catch (ignored) { }
        }
        if (params.check_only === true || error !== "") {
            prop.expression = previous.expression;
            if (previous.expression !== "") { prop.expressionEnabled = previous.enabled; }
            out.applied = false;
        } else {
            out.applied = true;
        }
        if (error !== "") { throw ae.fail("ExpressionError", error, { expression: expression, property: out.path }); }
        return out;
    });

    function markerProperty(comp, params) {
        if (ae.isGiven(params.layer)) {
            var l = ae.layer(comp, params.layer);
            return { owner: { layer: { index: l.index, name: l.name } }, prop: l.property("ADBE Marker") };
        }
        return { owner: { comp: { id: comp.id, name: comp.name } }, prop: comp.markerProperty };
    }

    function markerOut(prop, index) {
        var marker = prop.keyValue(index);
        var out = { index: index, time: ae.round(prop.keyTime(index)), comment: marker.comment, duration: ae.round(marker.duration), label: marker.label, chapter: marker.chapter, url: marker.url, frame_target: marker.frameTarget, cue_point_name: marker.cuePointName };
        try { out.protected_region = marker.protectedRegion; } catch (ignored) { }
        try { out.event_cue_point = marker.eventCuePoint === true; } catch (ignored2) { }
        try {
            var parameters = marker.getParameters();
            var any = false;
            for (var key in parameters) { if (parameters.hasOwnProperty(key)) { any = true; } }
            if (any) { out.parameters = parameters; }
        } catch (ignored3) { }
        return out;
    }

    function markersOut(found) {
        var markers = [];
        for (var i = 1; i <= found.prop.numKeys; i++) { markers.push(markerOut(found.prop, i)); }
        var out = found.owner;
        out.markers = markers;
        return out;
    }

    SnailBridge.command("markers_list", { params: ["comp", "layer"], read_only: true }, function (params) {
        return markersOut(markerProperty(ae.comp(params), params));
    });

    SnailBridge.command("marker_set", { params: ["comp", "layer", "time", "comment", "duration", "label", "chapter", "url", "frame_target", "cue_point_name", "protected_region", "event_cue_point", "parameters"] }, function (params) {
        var comp = ae.comp(params);
        var found = markerProperty(comp, params);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var marker = new MarkerValue(ae.isGiven(params.comment) ? String(params.comment) : "");
        if (ae.isGiven(params.duration)) { marker.duration = params.duration; }
        if (ae.isGiven(params.label)) { marker.label = params.label; }
        if (ae.isGiven(params.chapter)) { marker.chapter = String(params.chapter); }
        if (ae.isGiven(params.url)) { marker.url = String(params.url); }
        if (ae.isGiven(params.frame_target)) { marker.frameTarget = String(params.frame_target); }
        if (ae.isGiven(params.cue_point_name)) { marker.cuePointName = String(params.cue_point_name); }
        if (ae.isGiven(params.protected_region) && !ae.isGiven(params.layer)) { marker.protectedRegion = params.protected_region === true; }
        if (ae.isGiven(params.event_cue_point)) { marker.eventCuePoint = params.event_cue_point === true; }
        if (params.parameters && typeof params.parameters === "object") {
            var parameters = {};
            for (var key in params.parameters) { if (params.parameters.hasOwnProperty(key)) { parameters[key] = String(params.parameters[key]); } }
            marker.setParameters(parameters);
        }
        found.prop.setValueAtTime(time, marker);
        var out = markersOut(found);
        out.set = markerOut(found.prop, found.prop.nearestKeyIndex(time));
        return out;
    });

    SnailBridge.command("marker_remove", { params: ["comp", "layer", "indices", "times"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var found = markerProperty(comp, params);
        var picked = pick(comp, found.prop, params);
        removeKeys(found.prop, picked);
        var out = markersOut(found);
        out.removed = picked.length;
        return out;
    });

    SnailBridge.command("time_remap", { params: ["comp", "layer", "enabled", "keys", "replace"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.hasPixels(l) || !l.canSetTimeRemapEnabled) { throw ae.fail("BadRequest", "'" + l.name + "' cannot be time remapped; only footage and nested compositions can."); }
        if (params.enabled === false) {
            l.timeRemapEnabled = false;
            return { layer: { index: l.index, name: l.name }, enabled: false };
        }
        l.timeRemapEnabled = true;
        var prop = l.property("ADBE Time Remapping");
        if (params.keys instanceof Array) {
            if (params.replace === true) { removeKeys(prop, pick(comp, prop, {})); }
            for (var i = 0; i < params.keys.length; i++) {
                var key = params.keys[i];
                if (!key || typeof key.time !== "number" || typeof key.value !== "number") { throw ae.fail("BadRequest", "Keyframe " + (i + 1) + " needs time and value, both in seconds: the composition time and the source time it shows."); }
                prop.setValueAtTime(key.time, key.value);
            }
        }
        var out = keysOut(l, prop);
        out.enabled = true;
        out.source_duration = l.source ? ae.round(l.source.duration) : null;
        return out;
    });

    // Separating the dimensions of a position gives each axis keys of its own; the followers are the properties to key afterwards.
    SnailBridge.command("property_separate", { params: ["comp", "layer", "path", "separate"] }, function (params) {
        var comp = ae.comp(params);
        var target = animatable(comp, params);
        var prop = target.prop;
        if (!prop.isSeparationLeader && !prop.isSeparationFollower) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot separate its dimensions; position and the like can."); }
        if (prop.isSeparationFollower) { prop = prop.separationLeader; }
        if (ae.isGiven(params.separate)) { prop.dimensionsSeparated = params.separate === true; }
        var out = { layer: { index: target.layer.index, name: target.layer.name }, property: prop.name, path: props.pathOf(prop), separated: prop.dimensionsSeparated, followers: [] };
        if (prop.dimensionsSeparated) {
            var dims = prop.value instanceof Array ? prop.value.length : 1;
            for (var d = 0; d < dims; d++) {
                try { var follower = prop.getSeparationFollower(d); out.followers.push({ dimension: d, name: follower.name, path: props.pathOf(follower), keys: follower.numKeys }); } catch (ignored) { }
            }
        }
        return out;
    });
})();
