// Settings: the project's time display, colour management, expression engine and GPU; the fonts it uses; placeholders; caches and memory.
(function () {
    var ae = SnailBridge.ae;

    function enumOut(enumeration, value) { try { return ae.enumName(enumeration, value); } catch (ignored) { return String(value); } }

    function readable(target, name) { try { return target[name]; } catch (ignored) { return undefined; } }

    function projectOut(project) {
        var out = {
            file: project.file ? project.file.fsName : null,
            dirty: project.dirty === true,
            time_display: enumOut(TimeDisplayType, project.timeDisplayType),
            display_start_frame: project.displayStartFrame,
            frames_count: enumOut(FramesCountType, project.framesCountType),
            frames_use_feet_frames: project.framesUseFeetFrames,
            feet_frames_film: enumOut(FeetFramesFilmType, project.feetFramesFilmType),
            footage_timecode_start: enumOut(FootageTimecodeDisplayStartType, project.footageTimecodeDisplayStartType),
            bits_per_channel: project.bitsPerChannel,
            linear_blending: project.linearBlending,
            working_space: project.workingSpace,
            working_gamma: readable(project, "workingGamma"),
            linearize_working_space: readable(project, "linearizeWorkingSpace"),
            compensate_for_scene_referred_profiles: readable(project, "compensateForSceneReferredProfiles"),
            color_management_system: readable(project, "colorManagementSystem"),
            ocio_configuration_file: readable(project, "ocioConfigurationFile"),
            lut_interpolation_method: readable(project, "lutInterpolationMethod"),
            expression_engine: project.expressionEngine,
            gpu_accel: enumOut(GpuAccelType, project.gpuAccelType),
            transparency_grid_thumbnails: project.transparencyGridThumbnails,
            tool: enumOut(ToolType, project.toolType),
            revision: readable(project, "revision")
        };
        try { out.available_gpu_accel = []; var kinds = app.availableGPUAccelTypes; for (var i = 0; i < kinds.length; i++) { out.available_gpu_accel.push(enumOut(GpuAccelType, kinds[i])); } } catch (ignored) { }
        return out;
    }

        SnailBridge.command("project_settings", {
        params: ["time_display", "display_start_frame", "frames_count", "frames_use_feet_frames", "feet_frames_film", "footage_timecode_start", "bits_per_channel", "linear_blending",
        "working_space", "working_gamma", "linearize_working_space", "compensate_for_scene_referred_profiles", "color_management_system", "ocio_configuration_file", "lut_interpolation_method",
        "expression_engine", "gpu_accel", "transparency_grid_thumbnails", "tool", "color_profiles", "default_import_folder"]
    }, function (params) {
        var project = app.project;
        var changed = [];
        function set(name, apply) { if (ae.isGiven(params[name])) { apply(params[name]); changed.push(name); } }
        set("time_display", function (v) { project.timeDisplayType = ae.enumValue(TimeDisplayType, v, "time display"); });
        set("display_start_frame", function (v) { project.displayStartFrame = v; });
        set("frames_count", function (v) { project.framesCountType = ae.enumValue(FramesCountType, v, "frames count"); });
        set("frames_use_feet_frames", function (v) { project.framesUseFeetFrames = v === true; });
        set("feet_frames_film", function (v) { project.feetFramesFilmType = ae.enumValue(FeetFramesFilmType, v, "film type"); });
        set("footage_timecode_start", function (v) { project.footageTimecodeDisplayStartType = ae.enumValue(FootageTimecodeDisplayStartType, v, "footage timecode start"); });
        set("bits_per_channel", function (v) { if (v !== 8 && v !== 16 && v !== 32) { throw ae.fail("BadRequest", "bits_per_channel is 8, 16 or 32."); } project.bitsPerChannel = v; });
        set("linear_blending", function (v) { project.linearBlending = v === true; });
        set("working_space", function (v) { project.workingSpace = String(v); });
        set("working_gamma", function (v) { project.workingGamma = v; });
        set("linearize_working_space", function (v) { project.linearizeWorkingSpace = v === true; });
        set("compensate_for_scene_referred_profiles", function (v) { project.compensateForSceneReferredProfiles = v === true; });
        set("color_management_system", function (v) { project.colorManagementSystem = v; });
        set("ocio_configuration_file", function (v) { project.ocioConfigurationFile = v === "" ? "" : ae.file(String(v)).fsName; });
        set("lut_interpolation_method", function (v) { project.lutInterpolationMethod = v; });
        set("expression_engine", function (v) { project.expressionEngine = String(v); });
        set("gpu_accel", function (v) { project.gpuAccelType = ae.enumValue(GpuAccelType, v, "GPU acceleration"); });
        set("transparency_grid_thumbnails", function (v) { project.transparencyGridThumbnails = v === true; });
        set("tool", function (v) { project.toolType = ae.enumValue(ToolType, v, "tool"); });
        set("default_import_folder", function (v) { var folder = new Folder(String(v).charAt(0) === "~" ? Folder("~").fsName + String(v).substring(1) : String(v)); if (!folder.exists) { throw ae.fail("NotFound", "No folder at " + folder.fsName + "."); } project.setDefaultImportFolder(folder); });
        var out = projectOut(project);
        out.changed = changed;
        if (params.color_profiles === true) { out.color_profiles = project.listColorProfiles(); }
        return out;
    });

    SnailBridge.command("expressions_replace", { params: ["find", "replace"] }, function (params) {
        if (!ae.isGiven(params.find) || String(params.find) === "") { throw ae.fail("BadRequest", "find is the text to replace in every expression of the project."); }
        app.project.autoFixExpressions(String(params.find), ae.isGiven(params.replace) ? String(params.replace) : "");
        return { find: String(params.find), replace: ae.isGiven(params.replace) ? String(params.replace) : "", done: true };
    });

    function fontOut(font) {
        var out = { postscript_name: font.postScriptName, family: font.familyName, style: font.styleName };
        try { out.substituted = font.isSubstitute === true; } catch (ignored) { }
        try { out.from_adobe_fonts = font.isFromAdobeFonts === true; } catch (ignored2) { }
        return out;
    }

    SnailBridge.command("project_fonts", { params: [], read_only: true }, function () {
        var used = [];
        var usage = app.project.usedFonts || [];
        for (var i = 0; i < usage.length; i++) {
            var entry = { font: fontOut(usage[i].font), used_by: [] };
            var where = usage[i].usedAt || [];
            for (var w = 0; w < where.length; w++) {
                var spot = where[w];
                try { entry.used_by.push({ layer: { id: spot.layerID }, times: spot.times ? spot.times.length : undefined }); } catch (ignored) { }
            }
            used.push(entry);
        }
        var missing = [];
        try { var subs = app.fonts.missingOrSubstitutedFonts; for (var m = 0; m < subs.length; m++) { missing.push(fontOut(subs[m])); } } catch (ignored2) { }
        return { fonts: used, missing_or_substituted: missing };
    });

    function fontNamed(postScript) {
        var found = app.fonts.getFontsByPostScriptName(String(postScript));
        if (!found || found.length === 0) { throw ae.fail("NotFound", "No font has the PostScript name '" + postScript + "'; text_fonts lists the installed ones.", { font: postScript }); }
        return found[0];
    }

    SnailBridge.command("font_replace", { params: ["from", "to", "reload_missing"] }, function (params) {
        if (!ae.isGiven(params.from) || !ae.isGiven(params.to)) { throw ae.fail("BadRequest", "from and to are PostScript names; project_fonts and text_fonts list them."); }
        var ok = app.project.replaceFont(fontNamed(params.from), fontNamed(params.to), params.reload_missing === true);
        return { from: String(params.from), to: String(params.to), replaced: ok !== false };
    });

    SnailBridge.command("import_placeholder", { params: ["name", "width", "height", "frame_rate", "duration", "folder"] }, function (params) {
        var item = app.project.importPlaceholder(
            ae.isGiven(params.name) ? String(params.name) : "Placeholder",
            ae.isGiven(params.width) ? params.width : 1920,
            ae.isGiven(params.height) ? params.height : 1080,
            ae.isGiven(params.frame_rate) ? params.frame_rate : 30,
            ae.isGiven(params.duration) ? params.duration : 10);
        if (ae.isGiven(params.folder)) { item.parentFolder = ae.folder(params.folder); }
        return ae.describeItem(item);
    });

    SnailBridge.command("purge", { params: ["target"], destructive: true }, function (params) {
        var target = ae.isGiven(params.target) ? ae.enumValue(PurgeTarget, params.target, "purge target") : PurgeTarget.ALL_CACHES;
        app.purge(target);
        return { purged: ae.enumName(PurgeTarget, target) };
    });

    SnailBridge.command("app_performance", { params: ["image_cache_percent", "max_memory_percent", "multi_frame_rendering", "max_cpu_percent"] }, function (params) {
        var changed = [];
        if (ae.isGiven(params.image_cache_percent) || ae.isGiven(params.max_memory_percent)) {
            if (!ae.isGiven(params.image_cache_percent) || !ae.isGiven(params.max_memory_percent)) { throw ae.fail("BadRequest", "image_cache_percent and max_memory_percent go together."); }
            app.setMemoryUsageLimits(params.image_cache_percent, params.max_memory_percent);
            changed.push("memory");
        }
        if (ae.isGiven(params.multi_frame_rendering)) {
            app.setMultiFrameRenderingConfig(params.multi_frame_rendering === true, ae.isGiven(params.max_cpu_percent) ? params.max_cpu_percent : 100);
            changed.push("multi_frame_rendering");
        }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Pass image_cache_percent with max_memory_percent, or multi_frame_rendering with max_cpu_percent."); }
        return { changed: changed, memory_in_use: readable(app, "memoryInUse") };
    });
})();
