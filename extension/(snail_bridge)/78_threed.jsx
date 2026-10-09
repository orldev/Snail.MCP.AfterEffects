// 3D: cameras and their lens, lights and their shadows, the material options of 3D layers, geometry options, and the renderer of a composition.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    var CAMERA = {
        zoom: ["ADBE Camera Zoom"], depth_of_field: ["ADBE Camera Depth of Field"], focus_distance: ["ADBE Camera Focus Distance"], aperture: ["ADBE Camera Aperture"],
        blur_level: ["ADBE Camera Blur Level"], iris_shape: ["ADBE Iris Shape"], iris_rotation: ["ADBE Iris Rotation"], iris_roundness: ["ADBE Iris Roundness"],
        iris_aspect_ratio: ["ADBE Iris Aspect Ratio"], iris_diffraction_fringe: ["ADBE Iris Diffraction Fringe"], highlight_gain: ["ADBE Iris Highlight Gain"],
        highlight_threshold: ["ADBE Iris Highlight Threshold"], highlight_saturation: ["ADBE Iris Hightlight Saturation", "ADBE Iris Highlight Saturation"]
    };

    var LIGHT = {
        intensity: ["ADBE Light Intensity"], color: ["ADBE Light Color"], cone_angle: ["ADBE Light Cone Angle"], cone_feather: ["ADBE Light Cone Feather 2"],
        falloff: ["ADBE Light Falloff Type"], falloff_start: ["ADBE Light Falloff Start"], falloff_distance: ["ADBE Light Falloff Distance"],
        casts_shadows: ["ADBE Casts Shadows"], shadow_darkness: ["ADBE Light Shadow Darkness"], shadow_diffusion: ["ADBE Light Shadow Diffusion"]
    };

    var MATERIAL = {
        casts_shadows: ["ADBE Casts Shadows"], light_transmission: ["ADBE Light Transmission"], accepts_shadows: ["ADBE Accepts Shadows"], accepts_lights: ["ADBE Accepts Lights"],
        appears_in_reflections: ["ADBE Appears in Reflections"], ambient: ["ADBE Ambient Coefficient"], diffuse: ["ADBE Diffuse Coefficient"],
        specular_intensity: ["ADBE Specular Coefficient"], specular_shininess: ["ADBE Shininess Coefficient"], metal: ["ADBE Metal Coefficient"],
        reflection_intensity: ["ADBE Reflection Coefficient"], reflection_sharpness: ["ADBE Glossiness Coefficient"], reflection_rolloff: ["ADBE Fresnel Coefficient"],
        transparency: ["ADBE Transparency Coefficient"], transparency_rolloff: ["ADBE Transp Rolloff"], index_of_refraction: ["ADBE Index of Refraction"]
    };

    var GEOMETRY = {
        bevel_style: ["ADBE Bevel Styles"], bevel_direction: ["ADBE Bevel Direction"], bevel_depth: ["ADBE Bevel Depth"], hole_bevel_depth: ["ADBE Hole Bevel Depth"],
        extrusion_depth: ["ADBE Extrsn Depth"], curvature: ["ADBE Curvature"], segments: ["ADBE Segments"]
    };

    var BOOLEANS = { depth_of_field: true, accepts_lights: true, appears_in_reflections: true };

    var WORDS = {
        falloff: ["none", "smooth", "inverse_square_clamped"],
        casts_shadows: ["off", "on", "only"],
        accepts_shadows: ["off", "on", "only"]
    };

    function groupOf(l, matchName) {
        var group = null;
        try { group = l.property(matchName); } catch (ignored) { group = null; }
        return group;
    }

    function settingOf(group, candidates) {
        for (var i = 0; i < candidates.length; i++) {
            var found = null;
            try { found = group.property(candidates[i]); } catch (ignored) { found = null; }
            if (found) { return found; }
        }
        return null;
    }

    function wordIn(name, value) {
        if (!WORDS[name]) { return value; }
        if (typeof value === "boolean") { return value ? 1 : 0; }
        if (typeof value === "number") { return value; }
        var lowered = String(value).toLowerCase();
        for (var i = 0; i < WORDS[name].length; i++) { if (WORDS[name][i] === lowered) { return i + (name === "falloff" ? 1 : 0); } }
        throw ae.fail("BadRequest", "'" + value + "' is not a " + name + ".", { accepted: WORDS[name] });
    }

    function wordOut(name, value) {
        if (!WORDS[name] || typeof value !== "number") { return value; }
        var index = name === "falloff" ? value - 1 : value;
        return WORDS[name][index] !== undefined ? WORDS[name][index] : value;
    }

    function applyOptions(group, table, settings, time, comp) {
        var changed = [];
        for (var name in settings) {
            if (!settings.hasOwnProperty(name) || !ae.isGiven(settings[name])) { continue; }
            if (!table[name]) { continue; }
            var p = settingOf(group, table[name]);
            if (!p) { throw ae.fail("NoSuchSetting", "This layer has no '" + name + "' here.", { available: optionNames(group, table) }); }
            var raw = wordIn(name, settings[name]);
            if (name === "color") { raw = ae.color(raw, "color"); raw = [raw[0], raw[1], raw[2], 1]; }
            var value = props.valueIn(p, raw);
            if (ae.isGiven(time)) { p.setValueAtTime(time, value); }
            else if (p.numKeys > 0) { p.setValueAtTime(comp.time, value); }
            else { p.setValue(value); }
            changed.push(name);
        }
        return changed;
    }

    function optionNames(group, table) {
        var names = [];
        for (var name in table) { if (table.hasOwnProperty(name) && settingOf(group, table[name])) { names.push(name); } }
        return names;
    }

    function optionsOut(group, table, time) {
        var out = {};
        for (var name in table) {
            if (!table.hasOwnProperty(name)) { continue; }
            var p = settingOf(group, table[name]);
            if (!p) { continue; }
            var value = props.valueOut(p, ae.isGiven(time) ? p.valueAtTime(time, false) : p.value);
            if (BOOLEANS[name]) { value = value === 1; }
            out[name] = wordOut(name, value);
            if (p.numKeys > 0) { out[name + "_keys"] = p.numKeys; }
            if (p.expression) { out[name + "_expression"] = p.expression; }
        }
        return out;
    }

    function pointOfInterest(l) {
        var p = l.property("ADBE Transform Group").property("ADBE Anchor Point");
        return p;
    }

    function describe(comp, l, time) {
        var out = { layer: { index: l.index, name: l.name, kind: ae.describeLayer(l).kind, three_d: l.threeDLayer === true, auto_orient: ae.enumName(AutoOrientType, l.autoOrient) } };
        out.transform = ae.describeTransform(l);
        if (l instanceof CameraLayer || l instanceof LightLayer) {
            out.point_of_interest = ae.numbers(pointOfInterest(l).value);
            out.two_node = l.autoOrient === AutoOrientType.CAMERA_OR_POINT_OF_INTEREST;
        }
        if (l instanceof CameraLayer) {
            out.camera = optionsOut(groupOf(l, "ADBE Camera Options Group"), CAMERA, time);
            out.camera.focal_length_mm = ae.round(out.camera.zoom * 36 / comp.width);
        }
        if (l instanceof LightLayer) {
            out.light = optionsOut(groupOf(l, "ADBE Light Options Group"), LIGHT, time);
            out.light.type = ae.enumName(LightType, l.lightType);
        }
        var material = groupOf(l, "ADBE Material Options Group");
        if (material && l.threeDLayer) { out.material = optionsOut(material, MATERIAL, time); }
        var geometry = groupOf(l, "ADBE Extrsn Options Group");
        if (geometry && l.threeDLayer) { out.geometry = optionsOut(geometry, GEOMETRY, time); }
        out.renderer = { current: comp.renderer, available: comp.renderers };
        return out;
    }

    function setPointOfInterest(comp, l, params, changed) {
        if (ae.isGiven(params.look_at)) {
            var target = ae.layer(comp, params.look_at);
            var position = target.property("ADBE Transform Group").property("ADBE Position").valueAtTime(ae.isGiven(params.time) ? params.time : comp.time, false);
            params.point_of_interest = position.length === 3 ? position : [position[0], position[1], 0];
        }
        if (ae.isGiven(params.two_node)) {
            l.autoOrient = params.two_node ? AutoOrientType.CAMERA_OR_POINT_OF_INTEREST : AutoOrientType.NO_AUTO_ORIENT;
            changed.push("two_node");
        }
        if (ae.isGiven(params.point_of_interest)) {
            var p = pointOfInterest(l);
            var value = props.valueIn(p, params.point_of_interest);
            if (ae.isGiven(params.time)) { p.setValueAtTime(params.time, value); } else if (p.numKeys > 0) { p.setValueAtTime(comp.time, value); } else { p.setValue(value); }
            changed.push("point_of_interest");
        }
        if (ae.isGiven(params.position)) {
            var pos = l.property("ADBE Transform Group").property("ADBE Position");
            var posValue = props.valueIn(pos, params.position);
            if (ae.isGiven(params.time)) { pos.setValueAtTime(params.time, posValue); } else if (pos.numKeys > 0) { pos.setValueAtTime(comp.time, posValue); } else { pos.setValue(posValue); }
            changed.push("position");
        }
    }

    SnailBridge.command("threed_info", { params: ["comp", "layer", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        if (!ae.isGiven(params.layer)) {
            var cameras = [];
            var lights = [];
            var solids = 0;
            for (var i = 1; i <= comp.numLayers; i++) {
                var l = comp.layer(i);
                if (l instanceof CameraLayer) { cameras.push({ index: l.index, name: l.name, active: l.enabled }); }
                else if (l instanceof LightLayer) { lights.push({ index: l.index, name: l.name, type: ae.enumName(LightType, l.lightType), enabled: l.enabled }); }
                else if (l.threeDLayer) { solids++; }
            }
            return { comp: { id: comp.id, name: comp.name }, renderer: { current: comp.renderer, available: comp.renderers }, cameras: cameras, lights: lights, three_d_layers: solids };
        }
        return describe(comp, ae.layer(comp, params.layer), params.time);
    });

    SnailBridge.command("camera_set", {
        params: ["comp", "layer", "time", "position", "point_of_interest", "look_at", "two_node", "zoom", "focal_length_mm", "depth_of_field", "focus_distance", "aperture", "blur_level",
            "iris_shape", "iris_rotation", "iris_roundness", "iris_aspect_ratio", "iris_diffraction_fringe", "highlight_gain", "highlight_threshold", "highlight_saturation"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!(l instanceof CameraLayer)) { throw ae.fail("NotACamera", "'" + l.name + "' is not a camera."); }
        var changed = [];
        setPointOfInterest(comp, l, params, changed);
        if (ae.isGiven(params.focal_length_mm)) { params.zoom = params.focal_length_mm * comp.width / 36; }
        changed = changed.concat(applyOptions(groupOf(l, "ADBE Camera Options Group"), CAMERA, params, params.time, comp));
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass position, point_of_interest, look_at, zoom or any lens setting."); }
        var out = describe(comp, l, params.time);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("light_set", {
        params: ["comp", "layer", "time", "type", "position", "point_of_interest", "look_at", "two_node", "intensity", "color", "cone_angle", "cone_feather", "falloff", "falloff_start",
            "falloff_distance", "casts_shadows", "shadow_darkness", "shadow_diffusion"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!(l instanceof LightLayer)) { throw ae.fail("NotALight", "'" + l.name + "' is not a light."); }
        var changed = [];
        if (ae.isGiven(params.type)) { l.lightType = ae.enumValue(LightType, params.type, "light type"); changed.push("type"); }
        setPointOfInterest(comp, l, params, changed);
        changed = changed.concat(applyOptions(groupOf(l, "ADBE Light Options Group"), LIGHT, params, params.time, comp));
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass type, position, intensity, color, cone or shadow settings."); }
        var out = describe(comp, l, params.time);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("material_set", {
        params: ["comp", "layer", "time", "casts_shadows", "light_transmission", "accepts_shadows", "accepts_lights", "appears_in_reflections", "ambient", "diffuse", "specular_intensity",
            "specular_shininess", "metal", "reflection_intensity", "reflection_sharpness", "reflection_rolloff", "transparency", "transparency_rolloff", "index_of_refraction"]
    }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var group = groupOf(l, "ADBE Material Options Group");
        if (!group) { throw ae.fail("BadRequest", "'" + l.name + "' has no material options; only layers with pixels do."); }
        if (!l.threeDLayer) { l.threeDLayer = true; }
        var changed = applyOptions(group, MATERIAL, params, params.time, comp);
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass a material setting."); }
        var out = describe(comp, l, params.time);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("geometry_set", { params: ["comp", "layer", "time", "bevel_style", "bevel_direction", "bevel_depth", "hole_bevel_depth", "extrusion_depth", "curvature", "segments"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var group = groupOf(l, "ADBE Extrsn Options Group");
        if (!group) { throw ae.fail("NotSupported", "'" + l.name + "' has no geometry options under the " + comp.renderer + " renderer; renderer_set switches to one that extrudes.", { renderers: comp.renderers }); }
        if (!l.threeDLayer) { l.threeDLayer = true; }
        var changed = applyOptions(group, GEOMETRY, params, params.time, comp);
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass extrusion_depth, bevel_style, bevel_depth, hole_bevel_depth, curvature or segments."); }
        var out = describe(comp, l, params.time);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("renderer_set", { params: ["comp", "renderer"] }, function (params) {
        var comp = ae.comp(params);
        if (ae.isGiven(params.renderer)) {
            var wanted = String(params.renderer).toLowerCase();
            var chosen = null;
            for (var i = 0; i < comp.renderers.length; i++) {
                var candidate = comp.renderers[i];
                if (candidate.toLowerCase() === wanted || candidate.toLowerCase().indexOf(wanted) >= 0) { chosen = candidate; }
            }
            if (!chosen) { throw ae.fail("NotFound", "'" + params.renderer + "' is not a renderer this composition can use.", { available: comp.renderers }); }
            comp.renderer = chosen;
        }
        return { comp: { id: comp.id, name: comp.name }, renderer: comp.renderer, available: comp.renderers };
    });
})();
