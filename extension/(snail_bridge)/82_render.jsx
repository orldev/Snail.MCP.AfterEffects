// Rendering: the render queue, its templates, output modules, rendering in place and handing the queue to Media Encoder.
(function () {
    var ae = SnailBridge.ae;

    function queue() { return app.project.renderQueue; }

    function rqItem(ref) {
        var rq = queue();
        if (typeof ref !== "number" || ref < 1 || ref > rq.numItems) { throw ae.fail("NotFound", "The render queue has no item " + ref + "; it has " + rq.numItems + ".", { items: rq.numItems }); }
        return rq.item(ref);
    }

    function settingsOf(target) {
        var out = {};
        try {
            var settings = target.getSettings(GetSettingsFormat.STRING_SETTABLE);
            for (var key in settings) { if (settings.hasOwnProperty(key)) { out[key] = settings[key]; } }
        } catch (ignored) { }
        return out;
    }

    function fileInfo(file) {
        if (!file) { return null; }
        var out = { path: file.fsName, exists: file.exists };
        if (file.exists) { out.bytes = file.length; }
        return out;
    }

    function moduleOut(om, index) {
        var out = { index: index, name: om.name, file: fileInfo(om.file), post_render_action: ae.enumName(PostRenderAction, om.postRenderAction), include_source_xmp: om.includeSourceXMP };
        out.settings = settingsOf(om);
        return out;
    }

    function itemOut(item, index, withSettings) {
        var out = {
            index: index, comp: { id: item.comp.id, name: item.comp.name }, status: ae.enumName(RQItemStatus, item.status), render: item.render,
            start: ae.round(item.timeSpanStart), duration: ae.round(item.timeSpanDuration), skip_frames: item.skipFrames, elapsed_seconds: ae.round(item.elapsedSeconds || 0),
            log: ae.enumName(LogType, item.logType), output_modules: []
        };
        for (var i = 1; i <= item.numOutputModules; i++) { out.output_modules.push(moduleOut(item.outputModule(i), i)); }
        if (withSettings) { out.settings = settingsOf(item); }
        return out;
    }

    function queueOut(withSettings) {
        var rq = queue();
        var items = [];
        for (var i = 1; i <= rq.numItems; i++) { items.push(itemOut(rq.item(i), i, withSettings)); }
        var out = { items: items, rendering: rq.rendering, can_queue_in_ame: rq.canQueueInAME };
        try { out.notify_when_done = rq.queueNotify === true; } catch (ignored3) { }
        try { if (typeof rq.firstError !== "function" && rq.firstError) { out.first_error = String(rq.firstError); } } catch (ignored) { }
        try { if (typeof rq.lastError !== "function" && rq.lastError) { out.last_error = String(rq.lastError); } } catch (ignored2) { }
        return out;
    }

    SnailBridge.command("render_queue", { params: ["settings"], read_only: true }, function (params) {
        return queueOut(params.settings === true);
    });

    function withSample(params, work) {
        var rq = queue();
        if (rq.numItems > 0) { return work(rq.item(1), false); }
        var comp = ae.comp(params);
        var temporary = rq.items.add(comp);
        try { return work(temporary, true); }
        finally { temporary.remove(); }
    }

    SnailBridge.command("render_templates", { params: ["comp"], read_only: true }, function (params) {
        return withSample(params, function (item) {
            return { render_settings: item.templates, output_modules: item.outputModule(1).templates };
        });
    });

    // Template names are in the application's language, so a name matches exactly, else as the one template that contains the text.
    function applyTemplate(target, name, what) {
        var known = target.templates;
        var wanted = String(name).toLowerCase();
        var containing = [];
        for (var i = 0; i < known.length; i++) {
            if (known[i].toLowerCase() === wanted) { target.applyTemplate(known[i]); return known[i]; }
            if (known[i].toLowerCase().indexOf(wanted) >= 0) { containing.push(known[i]); }
        }
        if (containing.length === 1) { target.applyTemplate(containing[0]); return containing[0]; }
        if (containing.length > 1) { throw ae.fail("Ambiguous", containing.length + " " + what + " templates contain '" + name + "'; name one of them.", { candidates: containing }); }
        throw ae.fail("NotFound", "No " + what + " template is named '" + name + "'.", { available: known });
    }

    // A path that is a folder, or ends in a slash, takes the composition's name and the extension the template gave.
    function outputFile(om, comp, path) {
        var target = ae.file(path);
        var folder = new Folder(target.fsName);
        if (folder.exists || /[\/\\]$/.test(path)) {
            var current = om.file ? om.file.name : comp.name + ".mov";
            var tail = current.replace(/^[^\[._]*/, "");
            if (tail === "" || tail.charAt(0) !== "." && tail.charAt(0) !== "_" && tail.charAt(0) !== "[") { tail = ".mov"; }
            if (!folder.exists) { folder.create(); }
            target = new File(folder.fsName + "/" + comp.name + tail);
        } else if (!target.parent.exists) {
            target.parent.create();
        }
        return target;
    }

    function applySettings(target, settings, what) {
        if (!settings || typeof settings !== "object") { return []; }
        var changed = [];
        var batch = {};
        for (var key in settings) { if (settings.hasOwnProperty(key) && settings[key] !== undefined) { batch[key] = settings[key]; changed.push(key); } }
        if (changed.length === 0) { return changed; }
        try { target.setSettings(batch); }
        catch (error) { throw ae.fail("BadRequest", "After Effects refused the " + what + " settings: " + error.message, { settable: settingsOf(target) }); }
        return changed;
    }

    function configure(item, params, changed) {
        if (ae.isGiven(params.render_template)) { changed.push("render_template: " + applyTemplate(item, params.render_template, "render settings")); }
        var om = item.outputModule(ae.isGiven(params.output_module) ? params.output_module : 1);
        if (ae.isGiven(params.output_template)) { changed.push("output_template: " + applyTemplate(om, params.output_template, "output module")); }
        if (ae.isGiven(params.output_path)) { om.file = outputFile(om, item.comp, String(params.output_path)); changed.push("output_path"); }
        if (ae.isGiven(params.post_render_action)) { om.postRenderAction = ae.enumValue(PostRenderAction, params.post_render_action, "post-render action"); changed.push("post_render_action"); }
        if (ae.isGiven(params.start) || ae.isGiven(params.end)) {
            var start = ae.isGiven(params.start) ? params.start : item.timeSpanStart;
            var end = ae.isGiven(params.end) ? params.end : item.timeSpanStart + item.timeSpanDuration;
            if (end <= start) { throw ae.fail("BadRequest", "end must come after start."); }
            item.timeSpanStart = start;
            item.timeSpanDuration = end - start;
            changed.push("span");
        }
        if (params.work_area === true) { item.timeSpanStart = item.comp.workAreaStart; item.timeSpanDuration = item.comp.workAreaDuration; changed.push("span: work area"); }
        if (ae.isGiven(params.skip_frames)) { item.skipFrames = params.skip_frames; changed.push("skip_frames"); }
        if (ae.isGiven(params.render)) { item.render = params.render === true; changed.push("render"); }
        var renderChanged = applySettings(item, params.render_settings, "render");
        for (var r = 0; r < renderChanged.length; r++) { changed.push("render_settings." + renderChanged[r]); }
        var outputChanged = applySettings(om, params.output_settings, "output module");
        for (var o = 0; o < outputChanged.length; o++) { changed.push("output_settings." + outputChanged[o]); }
    }

    var CONFIGURE = ["render_template", "output_template", "output_path", "output_module", "post_render_action", "start", "end", "work_area", "skip_frames", "render", "render_settings", "output_settings"];

    SnailBridge.command("render_add", {
        params: ["comp", "render_template", "output_template", "output_path", "output_module", "post_render_action", "start", "end", "work_area", "skip_frames", "render", "render_settings", "output_settings"],
        open_world: true
    }, function (params) {
        var comp = ae.comp(params);
        var item = queue().items.add(comp);
        var changed = [];
        try { configure(item, params, changed); }
        catch (error) { item.remove(); throw error; }
        var index = queue().numItems;
        return { changed: changed, item: itemOut(item, index, true) };
    });

    SnailBridge.command("render_update", {
        params: ["item", "render_template", "output_template", "output_path", "output_module", "post_render_action", "start", "end", "work_area", "skip_frames", "render", "render_settings", "output_settings"],
        open_world: true
    }, function (params) {
        var item = rqItem(params.item);
        if (item.status === RQItemStatus.RENDERING || item.status === RQItemStatus.DONE) { throw ae.fail("BadRequest", "Item " + params.item + " is " + ae.enumName(RQItemStatus, item.status) + " and cannot be changed; duplicate the composition's entry with render_add."); }
        var changed = [];
        configure(item, params, changed);
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change.", { accepted: CONFIGURE }); }
        return { changed: changed, item: itemOut(item, params.item, true) };
    });

    SnailBridge.command("render_remove", { params: ["item", "all", "status"], destructive: true }, function (params) {
        var rq = queue();
        var removed = 0;
        if (ae.isGiven(params.item)) { rqItem(params.item).remove(); removed = 1; }
        else if (params.all === true || ae.isGiven(params.status)) {
            var wanted = ae.isGiven(params.status) ? ae.enumValue(RQItemStatus, params.status, "render queue status") : null;
            for (var i = rq.numItems; i >= 1; i--) {
                if (wanted === null || rq.item(i).status === wanted) { rq.item(i).remove(); removed++; }
            }
        } else {
            throw ae.fail("BadRequest", "Say which items to remove: item, all, or status such as DONE.");
        }
        return { removed: removed, remaining: rq.numItems };
    });

    SnailBridge.command("render_start", { params: ["items", "wait"], open_world: true }, function (params) {
        var rq = queue();
        if (rq.rendering) { throw ae.fail("Busy", "The render queue is already rendering."); }
        if (rq.numItems === 0) { throw ae.fail("BadRequest", "The render queue is empty; render_add puts a composition in it."); }
        var remembered = [];
        var chosen = {};
        if (params.items instanceof Array && params.items.length > 0) {
            for (var c = 0; c < params.items.length; c++) { rqItem(params.items[c]); chosen[params.items[c]] = true; }
            for (var i = 1; i <= rq.numItems; i++) {
                remembered.push(rq.item(i).render);
                if (rq.item(i).status === RQItemStatus.QUEUED || rq.item(i).status === RQItemStatus.UNQUEUED || rq.item(i).status === RQItemStatus.NEEDS_OUTPUT) { rq.item(i).render = chosen[i] === true; }
            }
        }
        var queued = 0;
        for (var q = 1; q <= rq.numItems; q++) { if (rq.item(q).render && rq.item(q).status === RQItemStatus.QUEUED) { queued++; } }
        if (queued === 0) { throw ae.fail("BadRequest", "No item is queued to render; an item needs an output path and render true.", { queue: queueOut(false).items }); }
        var started = new Date().getTime();
        if (params.wait === false) {
            // renderAsync hands the queue to After Effects and returns; the flags of the other items stay switched until the render ends.
            rq.renderAsync();
            var queued = queueOut(false);
            queued.started = true;
            queued.waited = false;
            return queued;
        }
        try { rq.render(); }
        finally {
            for (var r = 0; r < remembered.length; r++) { if (rq.item(r + 1).status !== RQItemStatus.DONE) { rq.item(r + 1).render = remembered[r]; } }
        }
        var out = queueOut(false);
        out.seconds = ae.round((new Date().getTime() - started) / 1000);
        out.waited = true;
        return out;
    });

    SnailBridge.command("render_in_ame", { params: ["render_immediately"], open_world: true }, function (params) {
        var rq = queue();
        if (!rq.canQueueInAME) { throw ae.fail("NotSupported", "Media Encoder is not available to this After Effects, or nothing in the queue can be sent to it."); }
        rq.queueInAME(params.render_immediately === true);
        return { sent: true, render_immediately: params.render_immediately === true, remaining: rq.numItems };
    });

    SnailBridge.command("render_control", { params: ["action", "notify", "show"], destructive: true }, function (params) {
        var rq = queue();
        var action = ae.isGiven(params.action) ? String(params.action).toLowerCase() : "";
        if (ae.isGiven(params.notify)) { rq.queueNotify = params.notify === true; }
        if (params.show === true) { rq.showWindow(true); }
        if ((action === "stop" || action === "pause") && rq.rendering !== true) {
            throw ae.fail("NotRendering", "Nothing is rendering; " + action + " applies to a render that ae_render started with wait false.");
        }
        if (action === "stop") { rq.stopRendering(); }
        else if (action === "pause") { rq.pauseRendering(true); }
        else if (action === "resume") { rq.pauseRendering(false); }
        else if (action !== "" || (!ae.isGiven(params.notify) && params.show !== true)) { throw ae.fail("BadRequest", "action is stop, pause or resume; notify and show work without one.", { accepted: ["stop", "pause", "resume"] }); }
        var out = queueOut(false);
        out.action = action;
        return out;
    });

    SnailBridge.command("render_template_save", { params: ["item", "output_module", "name", "kind"], open_world: true }, function (params) {
        var item = rqItem(ae.isGiven(params.item) ? params.item : 1);
        if (!ae.isGiven(params.name)) { throw ae.fail("BadRequest", "name is what the template will be called."); }
        if (item.status === RQItemStatus.DONE || item.status === RQItemStatus.RENDERING || item.status === RQItemStatus.USER_STOPPED || item.status === RQItemStatus.ERR_STOPPED) {
            throw ae.fail("BadRequest", "Item " + item.comp.name + " has rendered; After Effects saves templates from queued items only. render_add the composition again and save from that item.");
        }
        var kind = ae.isGiven(params.kind) ? String(params.kind).toLowerCase() : "output";
        if (kind === "render") { item.saveAsTemplate(String(params.name)); return { saved: String(params.name), kind: "render", templates: item.templates }; }
        if (kind !== "output") { throw ae.fail("BadRequest", "kind is render (render settings) or output (output module).", { accepted: ["render", "output"] }); }
        var om = item.outputModule(ae.isGiven(params.output_module) ? params.output_module : 1);
        om.saveAsTemplate(String(params.name));
        return { saved: String(params.name), kind: "output", templates: om.templates };
    });

    SnailBridge.command("watch_folder", { params: ["path", "stop", "pause"], open_world: true }, function (params) {
        var changed = [];
        if (params.stop === true) { app.endWatchFolder(); changed.push("stop"); }
        if (ae.isGiven(params.path)) {
            var folder = new Folder(ae.file(String(params.path)).fsName);
            if (!folder.exists) { throw ae.fail("NotFound", "No folder at " + folder.fsName + "."); }
            app.watchFolder(folder);
            changed.push("watch");
        }
        if (ae.isGiven(params.pause)) { app.pauseWatchFolder(params.pause === true); changed.push(params.pause ? "pause" : "resume"); }
        return { changed: changed, watching: app.isWatchFolder === true };
    });
})();
