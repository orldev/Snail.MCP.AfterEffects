// Compositing: precomposing, the tree of nested compositions, replacing and interpreting footage, proxies, the project's files.
(function () {
    var ae = SnailBridge.ae;

    function footageItem(ref) {
        var it = ae.item(ref, "footage");
        if (!(it instanceof FootageItem)) { throw ae.fail("NotFootage", "'" + it.name + "' is a " + ae.kindOf(it) + ", not footage."); }
        return it;
    }

    function interpretationOut(it) {
        var source = it.mainSource;
        var out = { id: it.id, name: it.name, file: it.file ? it.file.fsName : null, missing: it.footageMissing, width: it.width, height: it.height, pixel_aspect: it.pixelAspect, duration: ae.round(it.duration), frame_rate: ae.round(it.frameRate), has_alpha: source.hasAlpha, is_still: source.isStill };
        if (source.hasAlpha) { out.alpha = ae.enumName(AlphaMode, source.alphaMode); out.invert_alpha = source.invertAlpha; if (source.alphaMode === AlphaMode.PREMULTIPLIED) { out.premul_color = ae.numbers(source.premulColor); } }
        if (!source.isStill) {
            out.native_frame_rate = ae.round(source.nativeFrameRate);
            out.conform_frame_rate = ae.round(source.conformFrameRate);
            out.fields = ae.enumName(FieldSeparationType, source.fieldSeparationType);
            out.high_quality_fields = source.highQualityFieldSeparation;
            out.remove_pulldown = ae.enumName(PulldownPhase, source.removePulldown);
        }
        out.loop = source.loop;
        if (source instanceof FileSource) { out.source = "file"; } else if (source instanceof SolidSource) { out.source = "solid"; } else { out.source = "placeholder"; }
        out.use_proxy = it.useProxy;
        if (it.proxySource) { out.proxy = { file: it.proxySource.file ? it.proxySource.file.fsName : null, width: it.proxySource.width, height: it.proxySource.height }; }
        return out;
    }

    SnailBridge.command("precompose", { params: ["comp", "layers", "name", "move_attributes", "open"] }, function (params) {
        var comp = ae.comp(params);
        if (!(params.layers instanceof Array) || params.layers.length === 0) { throw ae.fail("BadRequest", "layers lists the layers to precompose."); }
        var picked = ae.layers(comp, params.layers);
        var indices = [];
        for (var i = 0; i < picked.length; i++) { indices.push(picked[i].index); }
        var moveAttributes = params.move_attributes !== false;
        if (!moveAttributes && indices.length > 1) { throw ae.fail("BadRequest", "Leaving the attributes behind works for one layer only; precompose several layers with move_attributes true."); }
        var name = ae.isGiven(params.name) ? String(params.name) : (picked.length === 1 ? picked[0].name + " Comp" : "Pre-comp");
        var created = comp.layers.precompose(indices, name, moveAttributes);
        var layer = null;
        for (var l = 1; l <= comp.numLayers; l++) { if (comp.layer(l).source === created) { layer = comp.layer(l); } }
        if (params.open === true) { created.openInViewer(); }
        return { comp: ae.describeItem(created), layer: layer ? { index: layer.index, name: layer.name } : null, in_comp: { id: comp.id, name: comp.name } };
    });

    function treeOf(comp, depth, seen) {
        var node = { id: comp.id, name: comp.name, width: comp.width, height: comp.height, duration: ae.round(comp.duration), frame_rate: ae.round(comp.frameRate), layers: comp.numLayers, nested: [] };
        if (seen[comp.id]) { node.repeated = true; return node; }
        seen[comp.id] = true;
        if (depth > 0) {
            for (var i = 1; i <= comp.numLayers; i++) {
                var l = comp.layer(i);
                if (l.source instanceof CompItem) {
                    var child = treeOf(l.source, depth - 1, seen);
                    child.layer = { index: l.index, name: l.name, enabled: l.enabled, collapse: l.collapseTransformation, time_remap: l.timeRemapEnabled };
                    node.nested.push(child);
                }
            }
        }
        delete seen[comp.id];
        return node;
    }

    SnailBridge.command("comp_tree", { params: ["comp", "depth"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var depth = ae.isGiven(params.depth) ? params.depth : 4;
        var tree = treeOf(comp, depth, {});
        var usedIn = [];
        for (var i = 0; i < comp.usedIn.length; i++) { usedIn.push({ id: comp.usedIn[i].id, name: comp.usedIn[i].name }); }
        tree.used_in = usedIn;
        return tree;
    });

    SnailBridge.command("footage_replace", { params: ["item", "file", "sequence", "force_alphabetical", "solid_color", "name", "width", "height", "duration", "frame_rate", "placeholder"], open_world: true }, function (params) {
        var it = footageItem(params.item);
        var before = it.name;
        if (ae.isGiven(params.file)) {
            var file = ae.file(params.file);
            if (!file.exists) { throw ae.fail("NotFound", "No file at " + file.fsName + "."); }
            if (params.sequence === true) { it.replaceWithSequence(file, params.force_alphabetical === true); } else { it.replace(file); }
        } else if (ae.isGiven(params.solid_color)) {
            var color = ae.color(params.solid_color, "solid_color");
            it.replaceWithSolid(color, ae.isGiven(params.name) ? String(params.name) : it.name, ae.isGiven(params.width) ? params.width : it.width, ae.isGiven(params.height) ? params.height : it.height, it.pixelAspect);
        } else if (params.placeholder === true) {
            it.replaceWithPlaceholder(ae.isGiven(params.name) ? String(params.name) : it.name, ae.isGiven(params.width) ? params.width : it.width, ae.isGiven(params.height) ? params.height : it.height,
                ae.isGiven(params.frame_rate) ? params.frame_rate : (it.frameRate || 30), ae.isGiven(params.duration) ? params.duration : (it.duration || 1));
        } else {
            throw ae.fail("BadRequest", "Say what to replace the footage with: file (and sequence), solid_color, or placeholder.");
        }
        var out = interpretationOut(it);
        out.was = before;
        return out;
    });

    SnailBridge.command("footage_interpret", { params: ["item", "alpha", "invert_alpha", "premul_color", "guess_alpha", "frame_rate", "fields", "high_quality_fields", "remove_pulldown", "guess_pulldown", "loop", "pixel_aspect"] }, function (params) {
        var it = footageItem(params.item);
        var source = it.mainSource;
        var changed = [];
        if (params.guess_alpha === true) { source.guessAlphaMode(); changed.push("alpha"); }
        if (ae.isGiven(params.alpha)) {
            if (!source.hasAlpha) { throw ae.fail("BadRequest", "'" + it.name + "' has no alpha channel to interpret."); }
            source.alphaMode = ae.enumValue(AlphaMode, params.alpha, "alpha mode");
            changed.push("alpha");
        }
        if (ae.isGiven(params.invert_alpha)) { source.invertAlpha = params.invert_alpha === true; changed.push("invert_alpha"); }
        if (ae.isGiven(params.premul_color)) { source.premulColor = ae.color(params.premul_color, "premul_color"); changed.push("premul_color"); }
        if (ae.isGiven(params.frame_rate)) {
            if (source.isStill) { throw ae.fail("BadRequest", "'" + it.name + "' is a still; it has no frame rate to conform."); }
            source.conformFrameRate = params.frame_rate;
            changed.push("frame_rate");
        }
        if (ae.isGiven(params.fields)) { source.fieldSeparationType = ae.enumValue(FieldSeparationType, params.fields, "field separation"); changed.push("fields"); }
        if (ae.isGiven(params.high_quality_fields)) { source.highQualityFieldSeparation = params.high_quality_fields === true; changed.push("high_quality_fields"); }
        if (params.guess_pulldown === true) { source.guessPulldown(PulldownMethod.PULLDOWN_3_2); changed.push("remove_pulldown"); }
        if (ae.isGiven(params.remove_pulldown)) { source.removePulldown = ae.enumValue(PulldownPhase, params.remove_pulldown, "pulldown phase"); changed.push("remove_pulldown"); }
        if (ae.isGiven(params.loop)) { source.loop = params.loop; changed.push("loop"); }
        if (ae.isGiven(params.pixel_aspect)) { it.pixelAspect = params.pixel_aspect; changed.push("pixel_aspect"); }
        var out = interpretationOut(it);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("proxy_set", { params: ["item", "file", "sequence", "none", "use", "solid_color", "width", "height", "placeholder", "frame_rate", "duration"], open_world: true }, function (params) {
        var it = ae.item(params.item);
        if (!(it instanceof FootageItem) && !(it instanceof CompItem)) { throw ae.fail("BadRequest", "'" + it.name + "' is a " + ae.kindOf(it) + "; only footage and compositions take proxies."); }
        var changed = [];
        if (params.none === true) { it.setProxyToNone(); changed.push("none"); }
        else if (ae.isGiven(params.file)) {
            var file = ae.file(params.file);
            if (!file.exists) { throw ae.fail("NotFound", "No file at " + file.fsName + "."); }
            if (params.sequence === true) { it.setProxyWithSequence(file, false); } else { it.setProxy(file); }
            changed.push("proxy");
        } else if (ae.isGiven(params.solid_color)) {
            it.setProxyWithSolid(ae.color(params.solid_color, "solid_color"), it.name + " proxy", ae.isGiven(params.width) ? params.width : it.width, ae.isGiven(params.height) ? params.height : it.height, it.pixelAspect);
            changed.push("proxy");
        } else if (params.placeholder === true) {
            it.setProxyWithPlaceholder(it.name + " proxy", ae.isGiven(params.width) ? params.width : it.width, ae.isGiven(params.height) ? params.height : it.height,
                ae.isGiven(params.frame_rate) ? params.frame_rate : (it.frameRate || 30), ae.isGiven(params.duration) ? params.duration : (it.duration || 1));
            changed.push("proxy");
        }
        if (ae.isGiven(params.use)) {
            if (params.use === true && !it.proxySource) { throw ae.fail("BadRequest", "'" + it.name + "' has no proxy to use; set one first."); }
            it.useProxy = params.use === true;
            changed.push("use");
        }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Say what to do: file, solid_color or placeholder for a proxy, none to remove it, use to switch it."); }
        var out = { id: it.id, name: it.name, use_proxy: it.useProxy, changed: changed };
        if (it.proxySource) { out.proxy = { file: it.proxySource.file ? it.proxySource.file.fsName : null, width: it.proxySource.width, height: it.proxySource.height }; }
        return out;
    });

    SnailBridge.command("layer_source_replace", { params: ["comp", "layer", "item", "fix_expressions"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.hasPixels(l) || !l.source) { throw ae.fail("BadRequest", "'" + l.name + "' has no source to replace."); }
        var it = ae.item(params.item);
        if (!(it instanceof FootageItem) && !(it instanceof CompItem)) { throw ae.fail("BadRequest", "'" + it.name + "' is a " + ae.kindOf(it) + "; a layer's source is footage or a composition."); }
        var was = { id: l.source.id, name: l.source.name };
        l.replaceSource(it, params.fix_expressions !== false);
        return { layer: ae.describeLayer(l), was: was };
    });

    SnailBridge.command("project_footage", { params: ["only_missing", "only_files"], read_only: true }, function (params) {
        var out = [];
        for (var i = 1; i <= app.project.numItems; i++) {
            var it = app.project.item(i);
            if (!(it instanceof FootageItem)) { continue; }
            var isFile = it.mainSource instanceof FileSource;
            if (params.only_files === true && !isFile) { continue; }
            if (params.only_missing === true && !it.footageMissing) { continue; }
            var entry = { id: it.id, name: it.name, file: it.file ? it.file.fsName : null, missing: it.footageMissing, used_in: it.usedIn.length };
            if (!isFile) { entry.source = it.mainSource instanceof SolidSource ? "solid" : "placeholder"; }
            out.push(entry);
        }
        return { footage: out, count: out.length };
    });

    SnailBridge.command("project_cleanup", { params: ["remove_unused", "consolidate", "reduce_to"], destructive: true }, function (params) {
        var out = {};
        if (params.consolidate === true) { out.consolidated = app.project.consolidateFootage(); }
        if (params.remove_unused === true) { out.removed_unused = app.project.removeUnusedFootage(); }
        if (params.reduce_to instanceof Array && params.reduce_to.length > 0) {
            var keep = [];
            for (var i = 0; i < params.reduce_to.length; i++) { keep.push(ae.item(params.reduce_to[i])); }
            out.reduced = app.project.reduceProject(keep);
        }
        if (!ae.isGiven(out.consolidated) && !ae.isGiven(out.removed_unused) && !ae.isGiven(out.reduced)) { throw ae.fail("BadRequest", "Say what to clean: consolidate, remove_unused, or reduce_to a list of items."); }
        out.items = app.project.numItems;
        return out;
    });

    function uniqueTarget(folder, name) {
        var target = new File(folder.fsName + "/" + name);
        var stem = name.replace(/(\.[^.]+)$/, "");
        var ext = name.length > stem.length ? name.substring(stem.length) : "";
        var n = 1;
        while (target.exists) { n++; target = new File(folder.fsName + "/" + stem + " " + n + ext); }
        return target;
    }

    SnailBridge.command("collect_files", { params: ["folder", "name"], open_world: true }, function (params) {
        if (!ae.isGiven(params.folder)) { throw ae.fail("BadRequest", "folder is where the project and its footage are collected."); }
        var root = new Folder(ae.file(params.folder).fsName);
        if (!root.exists && !root.create()) { throw ae.fail("CollectFailed", "Could not create " + root.fsName + "."); }
        var footageFolder = new Folder(root.fsName + "/(Footage)");
        if (!footageFolder.exists && !footageFolder.create()) { throw ae.fail("CollectFailed", "Could not create " + footageFolder.fsName + "."); }
        var copied = [];
        var missing = [];
        var sequences = {};
        for (var i = 1; i <= app.project.numItems; i++) {
            var it = app.project.item(i);
            if (!(it instanceof FootageItem) || !(it.mainSource instanceof FileSource) || !it.file) { continue; }
            if (it.footageMissing) { missing.push({ id: it.id, name: it.name, file: it.file.fsName }); continue; }
            var sourceFolder = it.file.parent;
            var isSequence = !it.mainSource.isStill && /\d+\.[a-z0-9]+$/i.test(it.file.name) && it.frameRate > 0 && !/\.(mov|mp4|m4v|avi|mxf|mkv|webm|wav|mp3|aif|aiff|gif|r3d|braw)$/i.test(it.file.name);
            if (isSequence) {
                var key = sourceFolder.fsName;
                var targetFolder = sequences[key];
                if (!targetFolder) {
                    targetFolder = new Folder(uniqueTarget(footageFolder, sourceFolder.name).fsName);
                    targetFolder.create();
                    var frames = sourceFolder.getFiles();
                    for (var f = 0; f < frames.length; f++) { if (frames[f] instanceof File) { frames[f].copy(targetFolder.fsName + "/" + frames[f].name); } }
                    sequences[key] = targetFolder;
                }
                it.replaceWithSequence(new File(targetFolder.fsName + "/" + it.file.name), false);
                copied.push({ id: it.id, name: it.name, to: targetFolder.fsName, sequence: true });
            } else {
                var target = uniqueTarget(footageFolder, it.file.name);
                if (!it.file.copy(target.fsName)) { throw ae.fail("CollectFailed", "Could not copy " + it.file.fsName + " to " + target.fsName + "."); }
                it.replace(target);
                copied.push({ id: it.id, name: it.name, to: target.fsName });
            }
        }
        var projectName = ae.isGiven(params.name) ? String(params.name) : (app.project.file ? app.project.file.name.replace(/\.aep$/i, "") : "Untitled");
        var projectFile = new File(root.fsName + "/" + projectName + ".aep");
        app.project.save(projectFile);
        return { project: projectFile.fsName, footage_folder: footageFolder.fsName, copied: copied, missing: missing };
    });
})();
