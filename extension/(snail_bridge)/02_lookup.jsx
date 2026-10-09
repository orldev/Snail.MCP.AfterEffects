// How commands find things in After Effects and how they describe them: items by id or name, compositions by name or
// the active one, layers by index or name; one description per kind, so every command answers in the same shape.
SnailBridge.ae = (function () {
    function fail(type, message, details) { return SnailBridge.fail(type, message, details); }

    function kindOf(item) {
        if (item instanceof CompItem) { return "comp"; }
        if (item instanceof FolderItem) { return "folder"; }
        if (item instanceof FootageItem) { return "footage"; }
        return "item";
    }

    function isGiven(value) { return value !== undefined && value !== null; }

    // Text and shape layers do not answer instanceof AVLayer here, although they are; a layer that is not a camera or a light has pixels.
    function hasPixels(l) { return l instanceof AVLayer || l instanceof TextLayer || l instanceof ShapeLayer || (!(l instanceof CameraLayer) && !(l instanceof LightLayer)); }

    function item(ref, what) {
        var project = app.project;
        if (typeof ref === "number") {
            var byId = project.itemByID(ref);
            if (!byId) { throw fail("NotFound", "No " + (what || "item") + " has the id " + ref + "."); }
            return byId;
        }
        if (typeof ref !== "string" || ref === "") { throw fail("BadRequest", "Name the " + (what || "item") + " by its name or its id."); }
        var matches = [];
        for (var i = 1; i <= project.numItems; i++) {
            if (project.item(i).name === ref) { matches.push(project.item(i)); }
        }
        if (matches.length === 0) { throw fail("NotFound", "No " + (what || "item") + " is named '" + ref + "'."); }
        if (matches.length > 1) {
            var ids = [];
            for (var m = 0; m < matches.length; m++) { ids.push({ id: matches[m].id, kind: kindOf(matches[m]) }); }
            throw fail("Ambiguous", matches.length + " items are named '" + ref + "'; pass the id instead.", { candidates: ids });
        }
        return matches[0];
    }

    var lastComp = null;

    function stillInProject(composition) {
        if (!composition) { return false; }
        try { return app.project.itemByID(composition.id) === composition; }
        catch (ignored) { return false; }
    }

    function onlyComp() {
        var found = null;
        for (var i = 1; i <= app.project.numItems; i++) {
            if (app.project.item(i) instanceof CompItem) {
                if (found !== null) { return null; }
                found = app.project.item(i);
            }
        }
        return found;
    }

    // The composition a call means when it names none: the active one, else the one the bridge worked on last, else the only one there is.
    function comp(params, key) {
        var ref = params[key || "comp"];
        var found;
        if (!isGiven(ref)) {
            var active = app.project.activeItem;
            if (active instanceof CompItem) { found = active; }
            else if (stillInProject(lastComp)) { found = lastComp; }
            else { found = onlyComp(); }
            if (!found) {
                var comps = [];
                for (var c = 1; c <= app.project.numItems; c++) { if (app.project.item(c) instanceof CompItem) { comps.push({ id: app.project.item(c).id, name: app.project.item(c).name }); } }
                throw fail("NoComposition", "No composition is named, none is active and the bridge has not worked on one yet; pass comp as a name or an id.",
                    { active: active ? { name: active.name, kind: kindOf(active) } : null, comps: comps });
            }
        } else {
            found = item(ref, "composition");
            if (!(found instanceof CompItem)) { throw fail("NotAComposition", "'" + found.name + "' is a " + kindOf(found) + ", not a composition."); }
        }
        lastComp = found;
        return found;
    }

    function remember(composition) { lastComp = composition; }

    function folder(ref) {
        if (!isGiven(ref)) { return app.project.rootFolder; }
        var found = item(ref, "folder");
        if (!(found instanceof FolderItem)) { throw fail("NotAFolder", "'" + found.name + "' is a " + kindOf(found) + ", not a folder."); }
        return found;
    }

    function layer(composition, ref) {
        if (typeof ref === "number") {
            if (ref < 1 || ref > composition.numLayers) { throw fail("NotFound", "'" + composition.name + "' has no layer " + ref + "; it has " + composition.numLayers + "."); }
            return composition.layer(ref);
        }
        if (typeof ref !== "string" || ref === "") { throw fail("BadRequest", "Name the layer by its name or its index."); }
        var found = composition.layer(ref);
        if (!found) { throw fail("NotFound", "'" + composition.name + "' has no layer named '" + ref + "'."); }
        return found;
    }

    function layers(composition, refs) {
        var picked = [];
        for (var i = 0; i < refs.length; i++) { picked.push(layer(composition, refs[i])); }
        return picked;
    }

    function round(value) { return Math.round(value * 10000) / 10000; }

    function numbers(values) {
        var out = [];
        for (var i = 0; i < values.length; i++) { out.push(round(values[i])); }
        return out;
    }

    function color(value, what) {
        if (typeof value === "string") {
            var hex = value.replace(/^#/, "");
            if (!/^[0-9a-fA-F]{6}$/.test(hex)) { throw fail("BadRequest", (what || "color") + " must be [r, g, b] with components 0 to 1, or #rrggbb."); }
            return [parseInt(hex.substring(0, 2), 16) / 255, parseInt(hex.substring(2, 4), 16) / 255, parseInt(hex.substring(4, 6), 16) / 255];
        }
        if (!(value instanceof Array) || value.length < 3) { throw fail("BadRequest", (what || "color") + " must be [r, g, b] with components 0 to 1, or #rrggbb."); }
        return [value[0], value[1], value[2]];
    }

    // Enumerations by name: BlendingMode.ADD is asked for as "ADD", and answered the same way.
    function isEnumKey(key) { return /^[A-Z][A-Za-z0-9_]*$/.test(key); }

    function enumValue(enumeration, name, what) {
        var wanted = String(name).toUpperCase();
        var known = [];
        for (var k in enumeration) {
            if (!enumeration.hasOwnProperty(k) || !isEnumKey(k)) { continue; }
            if (k.toUpperCase() === wanted) { return enumeration[k]; }
            known.push(k);
        }
        throw fail("BadRequest", "'" + name + "' is not a " + what + ".", { accepted: known });
    }

    function enumName(enumeration, value) {
        for (var k in enumeration) {
            if (enumeration.hasOwnProperty(k) && isEnumKey(k) && enumeration[k] === value) { return k; }
        }
        return String(value);
    }

    function file(path) {
        if (typeof path !== "string" || path === "") { throw fail("BadRequest", "A file path is needed."); }
        if (path.charAt(0) === "~") { path = Folder("~").fsName + path.substring(1); }
        return new File(path);
    }

    function describeItem(it) {
        var kind = kindOf(it);
        var out = {
            id: it.id,
            name: it.name,
            kind: kind,
            folder: it.parentFolder && it.parentFolder !== app.project.rootFolder ? it.parentFolder.id : null,
            label: it.label,
            comment: it.comment || null,
            selected: it.selected
        };
        if (kind === "comp") {
            out.width = it.width;
            out.height = it.height;
            out.pixel_aspect = it.pixelAspect;
            out.duration = round(it.duration);
            out.frame_rate = round(it.frameRate);
            out.frame_duration = round(it.frameDuration);
            out.background = numbers(it.bgColor);
            out.layers = it.numLayers;
            out.time = round(it.time);
            out.work_area = { start: round(it.workAreaStart), duration: round(it.workAreaDuration) };
            out.motion_blur = it.motionBlur;
            out.shutter_angle = it.shutterAngle;
            out.shutter_phase = it.shutterPhase;
            out.motion_blur_samples = it.motionBlurSamplesPerFrame;
            out.motion_blur_adaptive_limit = it.motionBlurAdaptiveSampleLimit;
            try { out.drop_frame = it.dropFrame; } catch (ignored6) { }
            try { out.active_camera = it.activeCamera ? { index: it.activeCamera.index, name: it.activeCamera.name } : null; } catch (ignored7) { }
            out.preserve_nested_frame_rate = it.preserveNestedFrameRate;
            out.preserve_nested_resolution = it.preserveNestedResolution;
            out.draft3d = it.draft3d;
            out.frame_blending = it.frameBlending;
            out.resolution_factor = it.resolutionFactor;
            out.display_start_frame = it.displayStartFrame;
            out.hide_shy = it.hideShyLayers;
            out.renderer = it.renderer;
        } else if (kind === "footage") {
            var source = it.mainSource;
            if (source instanceof SolidSource) { out.source = "solid"; }
            else if (source instanceof PlaceholderSource) { out.source = "placeholder"; }
            else { out.source = "file"; }
            out.file = it.file ? it.file.fsName : null;
            out.width = it.width;
            out.height = it.height;
            out.duration = round(it.duration);
            out.frame_rate = round(it.frameRate);
            out.has_video = it.hasVideo;
            out.has_audio = it.hasAudio;
            out.missing = it.footageMissing;
            if (source instanceof SolidSource) { out.color = numbers(source.color); }
            if (source instanceof FileSource) { out.is_still = source.isStill; }
        } else if (kind === "folder") {
            out.items = it.numItems;
        }
        return out;
    }

    function layerKind(l) {
        if (l instanceof CameraLayer) { return "camera"; }
        if (l instanceof LightLayer) { return "light"; }
        if (l instanceof TextLayer) { return "text"; }
        if (l instanceof ShapeLayer) { return "shape"; }
        if (l.nullLayer) { return "null"; }
        if (l.adjustmentLayer) { return "adjustment"; }
        if (l.source instanceof CompItem) { return "comp"; }
        if (l.source && l.source.mainSource instanceof SolidSource) { return "solid"; }
        return "footage";
    }

    var TRANSFORM = [
        ["anchor", "ADBE Anchor Point"], ["position", "ADBE Position"], ["scale", "ADBE Scale"], ["rotation", "ADBE Rotate Z"],
        ["opacity", "ADBE Opacity"], ["orientation", "ADBE Orientation"], ["x_rotation", "ADBE Rotate X"], ["y_rotation", "ADBE Rotate Y"]
    ];

    function describeTransform(l) {
        var transform = l.property("ADBE Transform Group");
        if (!transform) { return null; }
        var out = {};
        var animated = [];
        for (var i = 0; i < TRANSFORM.length; i++) {
            var prop = transform.property(TRANSFORM[i][1]);
            if (!prop || (!l.threeDLayer && i >= 5)) { continue; }
            var value = prop.value;
            out[TRANSFORM[i][0]] = value instanceof Array ? numbers(value) : round(value);
            if (prop.numKeys > 0) { animated.push(TRANSFORM[i][0]); }
            if (prop.expressionEnabled && prop.expression) { animated.push(TRANSFORM[i][0] + " (expression)"); }
        }
        if (animated.length > 0) { out.animated = animated; }
        return out;
    }

    function describeLayer(l) {
        var out = {
            index: l.index,
            id: l.id,
            name: l.name,
            kind: layerKind(l),
            enabled: l.enabled,
            solo: l.solo,
            shy: l.shy,
            locked: l.locked,
            selected: l.selected,
            in_point: round(l.inPoint),
            out_point: round(l.outPoint),
            start_time: round(l.startTime),
            stretch: round(l.stretch),
            parent: l.parent ? { index: l.parent.index, name: l.parent.name } : null,
            label: l.label,
            comment: l.comment || null,
            has_video: l.hasVideo,
            has_audio: l.hasAudio,
            three_d: l.threeDLayer === true,
            guide: l.guideLayer,
            motion_blur: l.motionBlur,
            adjustment: l.adjustmentLayer,
            source: l.source ? { id: l.source.id, name: l.source.name, kind: kindOf(l.source) } : null
        };
        if (hasPixels(l)) {
            out.blending_mode = enumName(BlendingMode, l.blendingMode);
            out.quality = enumName(LayerQuality, l.quality);
            out.collapse = l.collapseTransformation;
            out.audio_enabled = l.hasAudio ? l.audioEnabled : undefined;
            out.time_remap = l.timeRemapEnabled;
            out.frame_blending = enumName(FrameBlendingType, l.frameBlendingType);
            out.track_matte = enumName(TrackMatteType, l.trackMatteType);
            if (l.trackMatteType !== TrackMatteType.NO_TRACK_MATTE && typeof l.trackMatteLayer !== "undefined" && l.trackMatteLayer) {
                out.track_matte_layer = { index: l.trackMatteLayer.index, name: l.trackMatteLayer.name };
            }
        }
        out.transform = describeTransform(l);
        if (l instanceof LightLayer) { out.light_type = enumName(LightType, l.lightType); }
        if (l instanceof TextLayer) {
            var text = l.property("ADBE Text Properties").property("ADBE Text Document").value;
            out.text = text.text;
        }
        if (l.effect && l.effect.numProperties > 0) {
            var effects = [];
            for (var e = 1; e <= l.effect.numProperties; e++) { effects.push({ name: l.effect.property(e).name, match_name: l.effect.property(e).matchName, enabled: l.effect.property(e).enabled }); }
            out.effects = effects;
        }
        return out;
    }

    return {
        fail: fail,
        kindOf: kindOf,
        isGiven: isGiven,
        hasPixels: hasPixels,
        item: item,
        comp: comp,
        remember: remember,
        folder: folder,
        layer: layer,
        layers: layers,
        color: color,
        file: file,
        round: round,
        numbers: numbers,
        enumValue: enumValue,
        enumName: enumName,
        transformMatchNames: TRANSFORM,
        describeItem: describeItem,
        describeTransform: describeTransform,
        describeLayer: describeLayer
    };
})();
