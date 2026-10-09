// The application and the machine: who and where After Effects is, its preferences and script settings by key, a shell command, and leaving.
(function () {
    var ae = SnailBridge.ae;

    var KINDS = { "string": ["getPrefAsString", "savePrefAsString"], "long": ["getPrefAsLong", "savePrefAsLong"], "float": ["getPrefAsFloat", "savePrefAsFloat"], "bool": ["getPrefAsBool", "savePrefAsBool"] };

    function prefType(name) {
        if (!ae.isGiven(name)) { return PREFType.PREF_Type_MACHINE_INDEPENDENT; }
        var wanted = String(name).toUpperCase();
        if (wanted.indexOf("PREF_TYPE_") !== 0) { wanted = "PREF_Type_" + wanted; } else { wanted = "PREF_Type_" + wanted.substring(10); }
        for (var key in PREFType) { if (PREFType.hasOwnProperty(key) && key.toUpperCase() === wanted.toUpperCase()) { return PREFType[key]; } }
        throw ae.fail("BadRequest", "'" + name + "' is not a preference file.", { accepted: ["MACHINE_INDEPENDENT", "MACHINE_SPECIFIC", "MACHINE_INDEPENDENT_RENDER", "MACHINE_INDEPENDENT_OUTPUT", "MACHINE_INDEPENDENT_COMPOSITION", "MACHINE_SPECIFIC_TEXT", "MACHINE_SPECIFIC_PAINT", "MACHINE_SPECIFIC_EFFECTS", "MACHINE_SPECIFIC_EXPRESSION_SNIPPETS", "MACHINE_SPECIFIC_SCRIPT_SNIPPETS"] });
    }

    function kindOf(params) {
        var kind = ae.isGiven(params.kind) ? String(params.kind).toLowerCase() : "string";
        if (!KINDS.hasOwnProperty(kind)) { throw ae.fail("BadRequest", "kind is string, long, float or bool.", { accepted: ["string", "long", "float", "bool"] }); }
        return kind;
    }

    function sectionAndKey(params) {
        if (!ae.isGiven(params.section) || !ae.isGiven(params.key)) { throw ae.fail("BadRequest", "section and key name the preference, as the Preferences file spells them."); }
        return [String(params.section), String(params.key)];
    }

    SnailBridge.command("system_info", { params: ["environment"], read_only: true }, function (params) {
        var out = { machine: system.machineName, os: system.osName, os_version: system.osVersion, user: system.userName, version: app.version, build: app.buildName };
        try { out.app_name = app.appName; out.build_number = app.buildNumber; } catch (ignored0) { }
        try { out.language = ae.enumName(Language, app.language); out.iso_language = app.isoLanguage; } catch (ignored) { }
        try { out.memory_in_use = app.memoryInUse; } catch (ignored2) { }
        try { out.render_engine = app.isRenderEngine === true; } catch (ignored3) { }
        try { out.watch_folder = app.isWatchFolder === true; } catch (ignored4) { }
        try { out.preferences_folder = Folder.userData.fsName; } catch (ignored5) { }
        try { out.beta = app.isBeta === true; out.professional = app.isProfessionalVersion === true; out.ui_suppressed = app.isUISuppressed === true; } catch (ignored6) { }
        try { out.tooltips = app.areToolTipsEnabled === true; } catch (ignored7) { }
        try { out.rendering_disabled = app.disableRendering === true; out.report_error_on_missing_frame = app.reportErrorOnMissingFrame === true; out.save_project_on_crash = app.saveProjectOnCrash === true; } catch (ignored8) { }
        if (ae.isGiven(params.environment)) {
            var names = params.environment instanceof Array ? params.environment : [params.environment];
            out.environment = {};
            for (var i = 0; i < names.length; i++) { out.environment[String(names[i])] = app.getenv(String(names[i])); }
        }
        return out;
    });

    SnailBridge.command("app_set", { params: ["disable_rendering", "report_error_on_missing_frame", "save_project_on_crash", "save_preferences_on_quit"] }, function (params) {
        var changed = [];
        if (ae.isGiven(params.disable_rendering)) { app.disableRendering = params.disable_rendering === true; changed.push("disable_rendering"); }
        if (ae.isGiven(params.report_error_on_missing_frame)) { app.reportErrorOnMissingFrame = params.report_error_on_missing_frame === true; changed.push("report_error_on_missing_frame"); }
        if (ae.isGiven(params.save_project_on_crash)) { app.saveProjectOnCrash = params.save_project_on_crash === true; changed.push("save_project_on_crash"); }
        if (ae.isGiven(params.save_preferences_on_quit)) { app.setSavePreferencesOnQuit(params.save_preferences_on_quit === true); changed.push("save_preferences_on_quit"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Pass disable_rendering, report_error_on_missing_frame, save_project_on_crash or save_preferences_on_quit."); }
        return { changed: changed, rendering_disabled: app.disableRendering === true, report_error_on_missing_frame: app.reportErrorOnMissingFrame === true, save_project_on_crash: app.saveProjectOnCrash === true };
    });

    SnailBridge.command("swatch_read", { params: ["path"], read_only: true }, function (params) {
        var file = ae.file(params.path);
        if (!file.exists) { throw ae.fail("NotFound", "No swatch file at " + file.fsName + "."); }
        var parsed = app.parseSwatchFile(file);
        var out = { path: file.fsName, swatches: [] };
        var list = parsed && parsed.values ? parsed.values : [];
        for (var i = 0; i < list.length; i++) {
            var swatch = list[i];
            var entry = { name: swatch.name, type: swatch.type, color_space: swatch.colorSpace };
            try { entry.values = ae.numbers(swatch.values); } catch (ignored) { entry.values = swatch.values; }
            out.swatches.push(entry);
        }
        if (parsed && ae.isGiven(parsed.majorVersion)) { out.version = parsed.majorVersion + "." + parsed.minorVersion; }
        return out;
    });

    SnailBridge.command("system_call", { params: ["command"], destructive: true, open_world: true }, function (params) {
        if (!ae.isGiven(params.command) || String(params.command) === "") { throw ae.fail("BadRequest", "command is the shell line to run on After Effects' machine."); }
        var output = system.callSystem(String(params.command));
        return { command: String(params.command), output: output };
    });

    SnailBridge.command("preference_get", { params: ["section", "key", "kind", "file"], read_only: true }, function (params) {
        var where = sectionAndKey(params);
        var type = prefType(params.file);
        var kind = kindOf(params);
        var out = { section: where[0], key: where[1], kind: kind, exists: app.preferences.havePref(where[0], where[1], type) };
        if (out.exists) { out.value = app.preferences[KINDS[kind][0]](where[0], where[1], type); }
        return out;
    });

    SnailBridge.command("preference_set", { params: ["section", "key", "value", "kind", "file", "remove", "save_to_disk", "reload"], destructive: true }, function (params) {
        var changed = [];
        if (params.reload === true) { app.preferences.reload(); changed.push("reload"); }
        if (ae.isGiven(params.section) || ae.isGiven(params.key)) {
            var where = sectionAndKey(params);
            var type = prefType(params.file);
            if (params.remove === true) { app.preferences.deletePref(where[0], where[1], type); changed.push("remove"); }
            else if (ae.isGiven(params.value)) {
                var kind = kindOf(params);
                var value = params.value;
                if (kind === "string") { value = String(value); }
                else if (kind === "bool") { value = value === true || value === "true" || value === 1; }
                else { value = Number(value); if (isNaN(value)) { throw ae.fail("BadRequest", "value must be a number for kind " + kind + "."); } }
                app.preferences[KINDS[kind][1]](where[0], where[1], value, type);
                changed.push("value");
            }
        }
        if (params.save_to_disk === true) { app.preferences.saveToDisk(); changed.push("save_to_disk"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Pass section and key with value or remove, or reload or save_to_disk."); }
        return { changed: changed };
    });

    SnailBridge.command("script_setting_get", { params: ["section", "key"], read_only: true }, function (params) {
        var where = sectionAndKey(params);
        var out = { section: where[0], key: where[1], exists: app.settings.haveSetting(where[0], where[1]) };
        if (out.exists) { out.value = app.settings.getSetting(where[0], where[1]); }
        return out;
    });

    SnailBridge.command("script_setting_save", { params: ["section", "key", "value"] }, function (params) {
        var where = sectionAndKey(params);
        if (!ae.isGiven(params.value)) { throw ae.fail("BadRequest", "value is the text to keep under the key."); }
        app.settings.saveSetting(where[0], where[1], String(params.value));
        return { section: where[0], key: where[1], value: app.settings.getSetting(where[0], where[1]) };
    });

    SnailBridge.command("app_quit", { params: ["save"], destructive: true }, function (params) {
        var dirty = app.project && app.project.dirty === true;
        if (dirty && params.save === true) {
            if (!app.project.file) { throw ae.fail("ProjectDirty", "The project was never saved; ae_project_save with a path first, then quit."); }
            app.project.save();
        } else if (dirty && params.save !== false) {
            throw ae.fail("ProjectDirty", "The project has unsaved changes; pass save true to save them or false to drop them.");
        }
        SnailBridge.later(1, function () {
            try { if (app.project) { app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES); } } catch (ignored) { }
            app.quit();
        });
        return { quitting: true, saved: dirty && params.save === true };
    });
})();
