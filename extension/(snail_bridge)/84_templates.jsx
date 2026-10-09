// Templates: Essential Graphics controllers of a composition, instances of a template filled through their essential properties,
// substitution of text, colours and footage in a template composition, and .mogrt files out and in.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    function controllersOut(comp) {
        var out = [];
        var count = 0;
        try { count = comp.motionGraphicsTemplateControllerCount; } catch (ignored) { count = 0; }
        for (var i = 0; i < count; i++) {
            try { out.push({ index: i + 1, name: comp.getMotionGraphicsTemplateControllerName(i) }); } catch (ignored2) { }
        }
        return out;
    }

    function essentialOut(l) {
        var group = null;
        try { group = l.essentialProperty; } catch (ignored) { group = null; }
        if (!group || group.numProperties === 0) { return null; }
        var out = [];
        for (var i = 1; i <= group.numProperties; i++) {
            var p = group.property(i);
            var entry = { index: i, name: p.name, match_name: p.matchName, path: props.pathOf(p) };
            if (props.isGroup(p)) { entry.kind = "group"; }
            else {
                entry.value_type = ae.enumName(PropertyValueType, p.propertyValueType);
                try { entry.value = props.valueOut(p, p.value); } catch (ignored2) { entry.value = null; }
                if (p.numKeys > 0) { entry.keys = p.numKeys; }
                try { if (p.canSetAlternateSource) { entry.media = true; entry.alternate_source = p.alternateSource ? { id: p.alternateSource.id, name: p.alternateSource.name } : null; } } catch (ignored3) { }
                try { if (p.essentialPropertySource) { entry.source = props.pathOf(p.essentialPropertySource); } } catch (ignored4) { }
            }
            out.push(entry);
        }
        return out;
    }

    function instancesOut(comp) {
        var out = [];
        for (var i = 1; i <= comp.numLayers; i++) {
            var l = comp.layer(i);
            if (!(l.source instanceof CompItem)) { continue; }
            var essential = essentialOut(l);
            if (essential) { out.push({ layer: { index: l.index, name: l.name }, template: l.source.name, properties: essential }); }
        }
        return out;
    }

    function infoOut(comp) {
        var out = { comp: { id: comp.id, name: comp.name }, template_name: comp.motionGraphicsTemplateName || null, controllers: controllersOut(comp), instances: instancesOut(comp) };
        return out;
    }

    SnailBridge.command("template_info", { params: ["comp"], read_only: true }, function (params) {
        return infoOut(ae.comp(params));
    });

    SnailBridge.command("template_update", { params: ["comp", "name", "open_panel"] }, function (params) {
        var comp = ae.comp(params);
        var changed = [];
        if (ae.isGiven(params.name)) { comp.motionGraphicsTemplateName = String(params.name); changed.push("name"); }
        if (params.open_panel === true) { comp.openInEssentialGraphics(); changed.push("open_panel"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass name or open_panel."); }
        var out = infoOut(comp);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("template_add_property", { params: ["comp", "layer", "path", "name"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the property to expose, as property_tree lists it."); }
        var prop = props.resolve(l, params.path);
        if (props.isGroup(prop)) { throw ae.fail("BadRequest", "'" + prop.name + "' is a group; expose a property inside it, or an effect's parameter."); }
        if (!prop.canAddToMotionGraphicsTemplate(comp)) { throw ae.fail("BadRequest", "'" + prop.name + "' cannot be a template controller; After Effects allows numbers, colours, checkboxes, popups, text, 2D points and media.", { path: props.pathOf(prop) }); }
        var added;
        if (ae.isGiven(params.name)) { added = prop.addToMotionGraphicsTemplateAs(comp, String(params.name)); } else { added = prop.addToMotionGraphicsTemplate(comp); }
        if (added === false) { throw ae.fail("BadRequest", "After Effects did not add '" + prop.name + "' to the template; it may be there already."); }
        var out = infoOut(comp);
        out.added = { layer: { index: l.index, name: l.name }, path: props.pathOf(prop), name: ae.isGiven(params.name) ? String(params.name) : prop.name };
        return out;
    });

    function essentialNamed(l, name) {
        var group = null;
        try { group = l.essentialProperty; } catch (ignored) { group = null; }
        if (!group || group.numProperties === 0) { throw ae.fail("BadRequest", "'" + l.name + "' has no essential properties; it is not an instance of a template."); }
        var lowered = String(name).toLowerCase();
        if (/^\d+$/.test(String(name))) {
            var index = parseInt(name, 10);
            if (index >= 1 && index <= group.numProperties) { return group.property(index); }
        }
        for (var i = 1; i <= group.numProperties; i++) {
            if (group.property(i).name.toLowerCase() === lowered || group.property(i).matchName === name) { return group.property(i); }
        }
        var names = [];
        for (var n = 1; n <= group.numProperties; n++) { names.push(group.property(n).name); }
        throw ae.fail("NotFound", "'" + l.name + "' has no essential property '" + name + "'.", { available: names });
    }

    SnailBridge.command("template_fill", { params: ["comp", "layer", "values", "time"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!params.values || typeof params.values !== "object") { throw ae.fail("BadRequest", "values is an object of controller names to values."); }
        var changed = [];
        for (var name in params.values) {
            if (!params.values.hasOwnProperty(name) || params.values[name] === undefined) { continue; }
            var p = essentialNamed(l, name);
            if (props.isGroup(p)) { throw ae.fail("BadRequest", "'" + p.name + "' is a group and takes no value."); }
            var media = false;
            try { media = p.canSetAlternateSource === true; } catch (ignoredMedia) { media = false; }
            if (media) {
                var item = params.values[name] === null ? null : ae.item(params.values[name]);
                p.setAlternateSource(item);
                changed.push(p.name);
                continue;
            }
            var value = props.valueIn(p, params.values[name]);
            if (ae.isGiven(params.time)) { p.setValueAtTime(params.time, value); }
            else if (p.numKeys > 0) { p.setValueAtTime(comp.time, value); }
            else { p.setValue(value); }
            changed.push(p.name);
        }
        if (changed.length === 0) { throw ae.fail("BadRequest", "values named nothing to set."); }
        return { layer: { index: l.index, name: l.name }, changed: changed, properties: essentialOut(l) };
    });

    function firstFill(l) {
        var contents = l.property("ADBE Root Vectors Group");
        function search(group) {
            for (var i = 1; i <= group.numProperties; i++) {
                var item = group.property(i);
                if (item.matchName === "ADBE Vector Graphic - Fill") { return item.property("ADBE Vector Fill Color"); }
                if (item.matchName === "ADBE Vector Group") {
                    var deeper = search(item.property("ADBE Vectors Group"));
                    if (deeper) { return deeper; }
                }
            }
            return null;
        }
        return contents ? search(contents) : null;
    }

    function recolour(comp, l, color) {
        var rgb = ae.color(color, "color");
        if (l instanceof ShapeLayer) {
            var fill = firstFill(l);
            if (!fill) { throw ae.fail("BadRequest", "Shape layer '" + l.name + "' has no fill to recolour."); }
            fill.setValue([rgb[0], rgb[1], rgb[2], 1]);
            return "shape fill";
        }
        if (l instanceof TextLayer) {
            var source = l.property("ADBE Text Properties").property("ADBE Text Document");
            var doc = source.value;
            doc.applyFill = true;
            doc.fillColor = rgb;
            source.setValue(doc);
            return "text fill";
        }
        if (l.source && l.source.mainSource instanceof SolidSource) {
            l.source.mainSource.color = rgb;
            return "solid";
        }
        var fillEffect = null;
        if (l.effect) { for (var e = 1; e <= l.effect.numProperties; e++) { if (l.effect.property(e).matchName === "ADBE Fill") { fillEffect = l.effect.property(e); } } }
        if (fillEffect) { fillEffect.property("ADBE Fill-0002").setValue([rgb[0], rgb[1], rgb[2], 1]); return "fill effect"; }
        throw ae.fail("BadRequest", "'" + l.name + "' has nothing to recolour: not a solid, text, shape, or a layer with a Fill effect.");
    }

    SnailBridge.command("template_substitute", { params: ["comp", "text", "colors", "footage"] }, function (params) {
        var comp = ae.comp(params);
        var changed = [];
        var name;
        if (params.text && typeof params.text === "object") {
            for (name in params.text) {
                if (!params.text.hasOwnProperty(name)) { continue; }
                var textLayer = ae.layer(comp, name);
                if (!(textLayer instanceof TextLayer)) { throw ae.fail("NotAText", "'" + textLayer.name + "' is not a text layer."); }
                var source = textLayer.property("ADBE Text Properties").property("ADBE Text Document");
                var doc = source.value;
                doc.text = String(params.text[name]);
                if (source.numKeys > 0) { source.setValueAtTime(comp.time, doc); } else { source.setValue(doc); }
                changed.push({ layer: textLayer.name, text: doc.text });
            }
        }
        if (params.colors && typeof params.colors === "object") {
            for (name in params.colors) {
                if (!params.colors.hasOwnProperty(name)) { continue; }
                var colourLayer = ae.layer(comp, name);
                changed.push({ layer: colourLayer.name, color: recolour(comp, colourLayer, params.colors[name]) });
            }
        }
        if (params.footage && typeof params.footage === "object") {
            for (name in params.footage) {
                if (!params.footage.hasOwnProperty(name)) { continue; }
                var footageLayer = ae.layer(comp, name);
                var item = ae.item(params.footage[name]);
                if (!(item instanceof FootageItem) && !(item instanceof CompItem)) { throw ae.fail("BadRequest", "'" + item.name + "' is a " + ae.kindOf(item) + "; a layer's source is footage or a composition."); }
                footageLayer.replaceSource(item, true);
                changed.push({ layer: footageLayer.name, source: item.name });
            }
        }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to substitute: pass text, colors or footage keyed by layer name."); }
        return { comp: { id: comp.id, name: comp.name }, changed: changed };
    });

    SnailBridge.command("template_export", { params: ["comp", "path", "overwrite"], open_world: true }, function (params) {
        var comp = ae.comp(params);
        if (!comp.motionGraphicsTemplateName) { comp.motionGraphicsTemplateName = comp.name; }
        if (controllersOut(comp).length === 0) { throw ae.fail("BadRequest", "'" + comp.name + "' has no template controllers; template_add_property exposes properties first."); }
        var ok;
        var targetPath = null;
        var compId = comp.id;
        var compName = comp.name;
        var templateName = comp.motionGraphicsTemplateName;
        if (ae.isGiven(params.path)) {
            // After Effects takes a folder and names the file after the template; a file path names the template after its stem.
            var given = ae.file(params.path);
            var folder = new Folder(given.fsName);
            if (!folder.exists && /\.mogrt$/i.test(given.name)) {
                templateName = decodeURI(given.name).replace(/\.mogrt$/i, "");
                comp.motionGraphicsTemplateName = templateName;
                folder = given.parent;
            }
            if (!folder.exists && !folder.create()) { throw ae.fail("ExportFailed", "Could not create " + folder.fsName + "."); }
            targetPath = folder.fsName + "/" + templateName + ".mogrt";
            if (new File(targetPath).exists && params.overwrite !== true) { throw ae.fail("BadRequest", targetPath + " exists; pass overwrite true to replace it."); }
            ok = comp.exportAsMotionGraphicsTemplate(params.overwrite === true, folder.fsName);
        } else {
            ok = comp.exportAsMotionGraphicsTemplate(params.overwrite === true);
        }
        // The export can leave the composition reference stale, so the answer is built from values read before it and from the file itself.
        var written = targetPath ? new File(targetPath) : null;
        if (ok === false && !(written && written.exists)) { throw ae.fail("ExportFailed", "After Effects did not export the template; a dialog may have asked about fonts or unsupported properties.", { file: targetPath }); }
        var out = { comp: { id: compId, name: compName }, template_name: templateName, file: targetPath, exported: true, library: !targetPath };
        if (written && written.exists) { out.bytes = written.length; }
        return out;
    });

    SnailBridge.command("template_import", { params: ["path", "folder", "open"], open_world: true }, function (params) {
        var file = ae.file(params.path);
        if (!file.exists) { throw ae.fail("NotFound", "No file at " + file.fsName + "."); }
        if (!/\.mogrt$/i.test(file.name)) { throw ae.fail("BadRequest", "template_import takes a .mogrt file; import_file brings in footage and projects."); }
        var options = new ImportOptions(file);
        var item;
        try { item = app.project.importFile(options); }
        catch (error) { throw ae.fail("NotSupported", "This After Effects did not import the .mogrt through scripting: " + error.message + ". Import it from the Essential Graphics panel instead.", { file: file.fsName }); }
        if (ae.isGiven(params.folder)) { item.parentFolder = ae.folder(params.folder); }
        var comp = item instanceof CompItem ? item : null;
        if (!comp && item instanceof FolderItem) {
            for (var i = 1; i <= item.numItems && !comp; i++) { if (item.item(i) instanceof CompItem) { comp = item.item(i); } }
        }
        if (comp && params.open === true) { comp.openInViewer(); }
        var out = { item: ae.describeItem(item) };
        if (comp) { out.template = infoOut(comp); }
        return out;
    });
})();
