// The project: what is open, opening, saving and closing, the items in it and footage coming in.
(function () {
    var ae = SnailBridge.ae;

    function projectSummary() {
        var project = app.project;
        var comps = 0, footage = 0, folders = 0;
        for (var i = 1; i <= project.numItems; i++) {
            var kind = ae.kindOf(project.item(i));
            if (kind === "comp") { comps++; } else if (kind === "footage") { footage++; } else if (kind === "folder") { folders++; }
        }
        var active = project.activeItem;
        var selection = [];
        for (var s = 0; s < project.selection.length; s++) { selection.push({ id: project.selection[s].id, name: project.selection[s].name }); }
        return {
            file: project.file ? project.file.fsName : null,
            name: project.file ? project.file.displayName : null,
            dirty: project.dirty === true,
            items: project.numItems,
            comps: comps,
            footage: footage,
            folders: folders,
            active_item: active ? { id: active.id, name: active.name, kind: ae.kindOf(active) } : null,
            selection: selection,
            bits_per_channel: project.bitsPerChannel,
            working_space: project.workingSpace,
            render_queue_items: project.renderQueue.numItems
        };
    }

    function dirtyGuard(params, verb) {
        if (app.project && app.project.dirty && params.discard !== true) {
            throw ae.fail("ProjectDirty", "The open project has unsaved changes; save it with project_save, or pass discard: true to " + verb + " without saving.");
        }
    }

    SnailBridge.command("project_info", { params: [], read_only: true }, function () {
        return projectSummary();
    });

    SnailBridge.command("project_new", { params: ["discard"], destructive: true }, function (params) {
        dirtyGuard(params, "start a new one");
        if (app.project) { app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES); }
        app.newProject();
        return projectSummary();
    });

    SnailBridge.command("project_open", { params: ["path", "discard"], destructive: true }, function (params) {
        var file = ae.file(params.path);
        if (!file.exists) { throw ae.fail("NotFound", "No file at " + file.fsName + "."); }
        dirtyGuard(params, "open another");
        if (app.project) { app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES); }
        app.beginSuppressDialogs();
        try { if (/\.aet$/i.test(file.name)) { app.openTemplate(file); } else { app.open(file); } }
        finally { app.endSuppressDialogs(false); }
        var summary = projectSummary();
        if (/\.aet$/i.test(file.name)) { summary.from_template = file.fsName; }
        return summary;
    });

    SnailBridge.command("project_save", { params: ["path"] }, function (params) {
        var project = app.project;
        if (ae.isGiven(params.path)) {
            var file = ae.file(params.path);
            if (!file.parent.exists) { file.parent.create(); }
            project.save(file);
        } else {
            if (!project.file) { throw ae.fail("NoFile", "The project has never been saved; pass path to say where."); }
            project.save();
        }
        return { file: project.file.fsName, dirty: project.dirty === true };
    });

    SnailBridge.command("project_close", { params: ["discard"], destructive: true }, function (params) {
        dirtyGuard(params, "close it");
        app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES);
        app.newProject();
        return { closed: true };
    });

    SnailBridge.command("items_list", { params: ["kind", "folder", "name_contains", "limit"], read_only: true }, function (params) {
        var project = app.project;
        var inFolder = ae.isGiven(params.folder) ? ae.folder(params.folder) : null;
        var needle = ae.isGiven(params.name_contains) ? String(params.name_contains).toLowerCase() : null;
        var limit = ae.isGiven(params.limit) ? params.limit : 200;
        var found = [];
        var total = 0;
        for (var i = 1; i <= project.numItems; i++) {
            var it = project.item(i);
            if (ae.isGiven(params.kind) && ae.kindOf(it) !== params.kind) { continue; }
            if (inFolder !== null && it.parentFolder !== inFolder) { continue; }
            if (needle !== null && it.name.toLowerCase().indexOf(needle) < 0) { continue; }
            total++;
            if (found.length < limit) { found.push(ae.describeItem(it)); }
        }
        return { total: total, items: found, truncated: total > found.length };
    });

    SnailBridge.command("item_info", { params: ["item"], read_only: true }, function (params) {
        var it = ae.item(params.item);
        var out = ae.describeItem(it);
        if (it instanceof CompItem) {
            var layers = [];
            for (var i = 1; i <= it.numLayers; i++) { layers.push({ index: i, name: it.layer(i).name, enabled: it.layer(i).enabled }); }
            out.layer_list = layers;
        }
        if (it instanceof FolderItem) {
            var children = [];
            for (var c = 1; c <= it.numItems; c++) { children.push({ id: it.item(c).id, name: it.item(c).name, kind: ae.kindOf(it.item(c)) }); }
            out.children = children;
        }
        var usedIn = [];
        if (it instanceof FootageItem || it instanceof CompItem) {
            for (var u = 1; u <= app.project.numItems; u++) {
                var host = app.project.item(u);
                if (!(host instanceof CompItem) || host === it) { continue; }
                for (var l = 1; l <= host.numLayers; l++) {
                    if (host.layer(l).source === it) { usedIn.push({ comp: host.id, name: host.name, layer: l }); break; }
                }
            }
        }
        out.used_in = usedIn;
        return out;
    });

    SnailBridge.command("item_update", { params: ["item", "name", "comment", "label", "folder"] }, function (params) {
        var it = ae.item(params.item);
        if (ae.isGiven(params.name)) { it.name = params.name; }
        if (ae.isGiven(params.comment)) { it.comment = params.comment; }
        if (ae.isGiven(params.label)) { it.label = params.label; }
        if (ae.isGiven(params.folder)) { it.parentFolder = ae.folder(params.folder); }
        return ae.describeItem(it);
    });

    SnailBridge.command("item_remove", { params: ["item"], destructive: true }, function (params) {
        var it = ae.item(params.item);
        var gone = { id: it.id, name: it.name, kind: ae.kindOf(it) };
        it.remove();
        return gone;
    });

    SnailBridge.command("folder_create", { params: ["name", "folder"] }, function (params) {
        if (!ae.isGiven(params.name)) { throw ae.fail("BadRequest", "A folder needs a name."); }
        var created = app.project.items.addFolder(params.name);
        if (ae.isGiven(params.folder)) { created.parentFolder = ae.folder(params.folder); }
        return ae.describeItem(created);
    });

    SnailBridge.command("import_file", { params: ["path", "as", "sequence", "folder", "name", "force_alphabetical", "range_start", "range_end"], open_world: true }, function (params) {
        var file = ae.file(params.path);
        if (!file.exists) { throw ae.fail("NotFound", "No file at " + file.fsName + "."); }
        var options = new ImportOptions(file);
        var as = ae.isGiven(params.as) ? params.as : "footage";
        var types = { footage: ImportAsType.FOOTAGE, comp: ImportAsType.COMP, comp_cropped: ImportAsType.COMP_CROPPED_LAYERS, project: ImportAsType.PROJECT };
        if (!types.hasOwnProperty(as)) { throw ae.fail("BadRequest", "as must be footage, comp, comp_cropped or project."); }
        if (!options.canImportAs(types[as])) { throw ae.fail("CannotImport", file.displayName + " cannot be imported as " + as + "."); }
        options.importAs = types[as];
        var numbered = null;
        try { numbered = options.isFileNameNumbered(); } catch (ignored) { numbered = null; }
        if (params.sequence === true) {
            options.sequence = true;
            var ranged = ae.isGiven(params.range_start) || ae.isGiven(params.range_end);
            options.forceAlphabetical = params.force_alphabetical !== false && !ranged;
            if (ranged) {
                if (numbered === false) { throw ae.fail("BadRequest", "range_start and range_end need numbered file names; " + file.displayName + " carries no number."); }
                try {
                    if (ae.isGiven(params.range_start)) { options.rangeStart = params.range_start; }
                    if (ae.isGiven(params.range_end)) { options.rangeEnd = params.range_end; }
                } catch (error) { throw ae.fail("NotSupported", "This After Effects cannot limit the frames of a sequence on import (it came with 24.0): " + error.message); }
            }
        }
        app.beginSuppressDialogs();
        var imported;
        try { imported = app.project.importFile(options); }
        finally { app.endSuppressDialogs(false); }
        if (!imported) { throw ae.fail("CannotImport", "After Effects imported nothing from " + file.displayName + "."); }
        if (ae.isGiven(params.name)) { imported.name = params.name; }
        if (ae.isGiven(params.folder)) { imported.parentFolder = ae.folder(params.folder); }
        var described = ae.describeItem(imported);
        if (numbered !== null) { described.numbered = numbered; }
        return described;
    });
})();
