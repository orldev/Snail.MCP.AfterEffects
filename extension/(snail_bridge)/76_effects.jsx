// Effects: the catalog After Effects can apply, effects on a layer with their parameters, animation presets (.ffx) and layer styles.
(function () {
    var ae = SnailBridge.ae;
    var props = SnailBridge.properties;

    var STYLES = {
        drop_shadow: "dropShadow/enabled", inner_shadow: "innerShadow/enabled", outer_glow: "outerGlow/enabled", inner_glow: "innerGlow/enabled",
        bevel_emboss: "bevelEmboss/enabled", satin: "chromeFX/enabled", color_overlay: "solidFill/enabled", gradient_overlay: "gradientFill/enabled", stroke: "frameFX/enabled"
    };

    // The menu labels of each style in every language After Effects ships, from its dictionaries: the only way a script adds a style is the Layer menu.
    var STYLE_LABELS = {
        drop_shadow: ["Drop Shadow", "\u0422\u0435\u043d\u044c", "Schlagschatten", "Ombre port\u00e9e", "Sombra paralela", "Ombra esterna", "\u30c9\u30ed\u30c3\u30d7\u30b7\u30e3\u30c9\u30a6", "\uadf8\ub9bc\uc790 \ud6a8\uacfc", "Sombra projetada", "\u6295\u5f71"],
        inner_shadow: ["Inner Shadow", "\u0412\u043d\u0443\u0442\u0440\u0435\u043d\u043d\u044f\u044f \u0442\u0435\u043d\u044c", "Schatten nach innen", "Ombre interne", "Sombra interior", "Ombra interna", "\u30b7\u30e3\u30c9\u30a6 (\u5185\u5074)", "\ub0b4\ubd80 \uadf8\ub9bc\uc790", "Sombra interna", "\u5185\u9634\u5f71"],
        outer_glow: ["Outer Glow", "\u0412\u043d\u0435\u0448\u043d\u0435\u0435 \u0441\u0432\u0435\u0447\u0435\u043d\u0438\u0435", "Schein nach au\u00dfen", "Luminescence externe", "Resplandor exterior", "Bagliore esterno", "\u5149\u5f69 (\u5916\u5074)", "\uc678\ubd80 \uad11\uc120", "Brilho externo", "\u5916\u53d1\u5149"],
        inner_glow: ["Inner Glow", "\u0412\u043d\u0443\u0442\u0440\u0435\u043d\u043d\u0435\u0435 \u0441\u0432\u0435\u0447\u0435\u043d\u0438\u0435", "Schein nach innen", "Luminescence interne", "Resplandor interior", "Bagliore interno", "\u5149\u5f69 (\u5185\u5074)", "\ub0b4\ubd80 \uad11\uc120", "Brilho interno", "\u5185\u53d1\u5149"],
        bevel_emboss: ["Bevel and Emboss", "\u0421\u043a\u043e\u0441 \u0438 \u0442\u0438\u0441\u043d\u0435\u043d\u0438\u0435", "Abgeflachte Kante und Relief", "Biseautage et estampage", "Inglete y relieve", "Smusso ed effetto rilievo", "\u30d9\u30d9\u30eb\u3068\u30a8\u30f3\u30dc\u30b9", "\uacbd\uc0ac\uc640 \uc5e0\ubcf4\uc2a4", "Chanfro e entalhe", "\u659c\u9762\u548c\u6d6e\u96d5"],
        satin: ["Satin", "\u0413\u043b\u044f\u043d\u0435\u0446", "Glanz", "Satin", "Satinado", "Finitura lucida", "\u30b5\u30c6\u30f3", "\uc0c8\ud2f4", "Cetim", "\u5149\u6cfd"],
        color_overlay: ["Color Overlay", "\u041d\u0430\u043b\u043e\u0436\u0435\u043d\u0438\u0435 \u0446\u0432\u0435\u0442\u0430", "Farb\u00fcberlagerung", "Incrustation couleur", "Superposici\u00f3n de color", "Sovrapposizione colore", "\u30ab\u30e9\u30fc\u30aa\u30fc\u30d0\u30fc\u30ec\u30a4", "\uc0c9\uc0c1 \uc624\ubc84\ub808\uc774", "Sobreposi\u00e7\u00e3o de cores", "\u989c\u8272\u53e0\u52a0"],
        gradient_overlay: ["Gradient Overlay", "\u041d\u0430\u043b\u043e\u0436\u0435\u043d\u0438\u0435 \u0433\u0440\u0430\u0434\u0438\u0435\u043d\u0442\u0430", "Verlaufs\u00fcberlagerung", "Incrustation de d\u00e9grad\u00e9", "Superposici\u00f3n de degradado", "Sovrapposizione sfumatura", "\u30b0\u30e9\u30c7\u30fc\u30b7\u30e7\u30f3\u30aa\u30fc\u30d0\u30fc\u30ec\u30a4", "\uadf8\ub77c\ub514\uc5b8\ud2b8 \uc624\ubc84\ub808\uc774", "Sobreposi\u00e7\u00e3o de gradientes", "\u6e10\u53d8\u53e0\u52a0"],
        stroke: ["Stroke", "\u041e\u0431\u0432\u043e\u0434\u043a\u0430", "Kontur", "Contour", "Trazo", "Tratto", "\u5883\u754c\u7dda", "\uc120", "Tra\u00e7ado", "\u63cf\u8fb9"]
    };

    function effects(l) {
        var group = l.property("ADBE Effect Parade");
        if (!group) { throw ae.fail("BadRequest", "'" + l.name + "' cannot carry effects."); }
        return group;
    }

    function catalogEntry(e) { return { name: e.displayName, match_name: e.matchName, category: e.category, version: e.version }; }

    function catalogMatch(name) {
        var lowered = String(name).toLowerCase();
        var byName = null;
        for (var i = 0; i < app.effects.length; i++) {
            var e = app.effects[i];
            if (e.matchName === name) { return e; }
            if (e.displayName.toLowerCase() === lowered) { byName = e; }
        }
        return byName;
    }

    function parameterOf(effect, key) {
        var found = null;
        if (typeof key === "number") {
            if (key >= 1 && key <= effect.numProperties) { return effect.property(key); }
            return null;
        }
        try { found = effect.property(String(key)); } catch (ignored) { found = null; }
        if (found) { return found; }
        var lowered = String(key).toLowerCase();
        for (var i = 1; i <= effect.numProperties; i++) {
            var p = effect.property(i);
            if (p.name.toLowerCase() === lowered || p.matchName.toLowerCase() === lowered) { return p; }
        }
        return null;
    }

    function parameterNames(effect) {
        var names = [];
        for (var i = 1; i <= effect.numProperties; i++) {
            var p = effect.property(i);
            if (p.propertyValueType !== PropertyValueType.NO_VALUE || props.isGroup(p)) { names.push({ index: i, name: p.name, match_name: p.matchName }); }
        }
        return names;
    }

    function setValue(p, raw, time, comp) {
        var value = props.valueIn(p, raw);
        if (ae.isGiven(time)) { p.setValueAtTime(time, value); }
        else if (p.numKeys > 0) { p.setValueAtTime(comp.time, value); }
        else { p.setValue(value); }
    }

    function applySettings(effect, settings, time, comp) {
        var changed = [];
        for (var key in settings) {
            if (!settings.hasOwnProperty(key) || settings[key] === undefined) { continue; }
            var p = parameterOf(effect, /^\d+$/.test(key) ? parseInt(key, 10) : key);
            if (!p) { throw ae.fail("NoSuchSetting", "'" + effect.name + "' has no parameter '" + key + "'.", { available: parameterNames(effect) }); }
            if (props.isGroup(p)) {
                if (!p.canSetEnabled) { throw ae.fail("BadRequest", "'" + p.name + "' is a group; it has no value."); }
                p.enabled = settings[key] === true;
            } else if (p.propertyValueType === PropertyValueType.LAYER_INDEX) {
                var ref = settings[key];
                p.setValue(ref === null ? 0 : ae.layer(comp, ref).index);
            } else if (p.propertyValueType === PropertyValueType.NO_VALUE || p.propertyValueType === PropertyValueType.CUSTOM_VALUE) {
                throw ae.fail("BadRequest", "'" + p.name + "' has no value scripts can set.");
            } else {
                setValue(p, settings[key], time, comp);
            }
            changed.push(p.name);
        }
        return changed;
    }

    function parametersOut(effect, time, withValues) {
        var out = [];
        for (var i = 1; i <= effect.numProperties; i++) {
            var p = effect.property(i);
            var entry = props.describe(p, time, withValues);
            entry.index = i;
            if (p.propertyValueType === PropertyValueType.LAYER_INDEX && withValues) {
                var index = p.value;
                entry.value = index > 0 ? { index: index, name: effect.propertyGroup(effect.propertyDepth).containingComp.layer(index).name } : null;
            }
            out.push(entry);
        }
        return out;
    }

    function effectOut(effect, time, withValues) {
        return { index: effect.propertyIndex, name: effect.name, match_name: effect.matchName, enabled: effect.enabled, path: props.pathOf(effect), parameters: parametersOut(effect, time, withValues) };
    }

    function effectOf(l, ref) {
        var group = effects(l);
        if (typeof ref === "number") {
            if (ref < 1 || ref > group.numProperties) { throw ae.fail("NotFound", "'" + l.name + "' has no effect " + ref + "; it has " + group.numProperties + "."); }
            return group.property(ref);
        }
        if (typeof ref !== "string" || ref === "") { throw ae.fail("BadRequest", "Name the effect by its name on the layer or its index."); }
        var byMatch = null;
        for (var i = 1; i <= group.numProperties; i++) {
            if (group.property(i).name === ref) { return group.property(i); }
            if (group.property(i).matchName === ref && !byMatch) { byMatch = group.property(i); }
        }
        if (byMatch) { return byMatch; }
        var names = [];
        for (var n = 1; n <= group.numProperties; n++) { names.push({ index: n, name: group.property(n).name, match_name: group.property(n).matchName }); }
        throw ae.fail("NotFound", "'" + l.name + "' has no effect '" + ref + "'.", { effects: names });
    }

    SnailBridge.command("effects_catalog", { params: ["query", "category", "limit"], read_only: true }, function (params) {
        var needle = ae.isGiven(params.query) ? String(params.query).toLowerCase() : "";
        var category = ae.isGiven(params.category) ? String(params.category).toLowerCase() : "";
        var limit = ae.isGiven(params.limit) ? params.limit : 60;
        var out = [];
        var total = 0;
        var categories = {};
        for (var i = 0; i < app.effects.length; i++) {
            var e = app.effects[i];
            categories[e.category] = true;
            if (needle && e.displayName.toLowerCase().indexOf(needle) < 0 && e.matchName.toLowerCase().indexOf(needle) < 0) { continue; }
            if (category && e.category.toLowerCase() !== category) { continue; }
            total++;
            if (out.length < limit) { out.push(catalogEntry(e)); }
        }
        var names = [];
        for (var c in categories) { if (categories.hasOwnProperty(c)) { names.push(c); } }
        return { effects: out, matched: total, truncated: total > out.length, categories: names.sort() };
    });

    SnailBridge.command("effect_add", { params: ["comp", "layer", "effect", "name", "settings", "at"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.effect)) { throw ae.fail("BadRequest", "effect names what to add, by match name or display name as effects_catalog lists them."); }
        var entry = catalogMatch(params.effect);
        if (!entry) { throw ae.fail("NotFound", "No effect is called '" + params.effect + "'; effects_catalog finds the name.", { query: params.effect }); }
        var group = effects(l);
        if (!group.canAddProperty(entry.matchName)) { throw ae.fail("BadRequest", "'" + entry.displayName + "' cannot be applied to '" + l.name + "'."); }
        var effect = group.addProperty(entry.matchName);
        if (ae.isGiven(params.name)) { effect.name = String(params.name); }
        var changed = [];
        if (params.settings && typeof params.settings === "object") { changed = applySettings(effect, params.settings, null, comp); }
        if (ae.isGiven(params.at) && params.at >= 1 && params.at <= group.numProperties) { effect.moveTo(params.at); }
        return { layer: { index: l.index, name: l.name }, changed: changed, effect: effectOut(effect, comp.time, true) };
    });

    SnailBridge.command("effect_info", { params: ["comp", "layer", "effect", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.effect)) {
            var group = effects(l);
            var list = [];
            for (var i = 1; i <= group.numProperties; i++) { list.push({ index: i, name: group.property(i).name, match_name: group.property(i).matchName, enabled: group.property(i).enabled }); }
            return { layer: { index: l.index, name: l.name }, effects: list };
        }
        var effect = effectOf(l, params.effect);
        return { layer: { index: l.index, name: l.name }, effect: effectOut(effect, ae.isGiven(params.time) ? params.time : comp.time, true) };
    });

    SnailBridge.command("effect_update", { params: ["comp", "layer", "effect", "name", "enabled", "settings", "time", "at"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var effect = effectOf(l, params.effect);
        var changed = [];
        if (ae.isGiven(params.name)) { effect.name = String(params.name); changed.push("name"); }
        if (ae.isGiven(params.enabled)) { effect.enabled = params.enabled === true; changed.push("enabled"); }
        if (params.settings && typeof params.settings === "object") { changed = changed.concat(applySettings(effect, params.settings, params.time, comp)); }
        if (ae.isGiven(params.at)) { effect.moveTo(params.at); changed.push("at"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass name, enabled, settings or at."); }
        return { layer: { index: l.index, name: l.name }, changed: changed, effect: effectOut(effect, ae.isGiven(params.time) ? params.time : comp.time, true) };
    });

    SnailBridge.command("effect_remove", { params: ["comp", "layer", "effect"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var effect = effectOf(l, params.effect);
        var removed = { name: effect.name, match_name: effect.matchName };
        effect.remove();
        return { layer: { index: l.index, name: l.name }, removed: removed, remaining: effects(l).numProperties };
    });

    var presetCache = null;

    function presetRoots() {
        var roots = [];
        try { roots.push(new Folder(Folder.appPackage.parent.fsName + "/Presets")); } catch (ignored) { }
        try { roots.push(new Folder(Folder.myDocuments.fsName + "/Adobe/After Effects " + app.version.split(".")[0] + "/User Presets")); } catch (ignored2) { }
        try { roots.push(new Folder(Folder.myDocuments.fsName + "/Adobe/After Effects " + (2000 + parseInt(app.version.split(".")[0], 10) - 0) + "/User Presets")); } catch (ignored3) { }
        return roots;
    }

    function collectPresets(folder, into, root) {
        var entries = folder.getFiles();
        for (var i = 0; i < entries.length; i++) {
            var entry = entries[i];
            if (entry instanceof Folder) { collectPresets(entry, into, root); continue; }
            if (/\.ffx$/i.test(entry.name)) {
                into.push({ name: decodeURI(entry.name).replace(/\.ffx$/i, ""), path: entry.fsName, folder: decodeURI(entry.parent.fsName.substring(root.fsName.length + 1)) });
            }
        }
    }

    function presets() {
        if (presetCache) { return presetCache; }
        var found = [];
        var roots = presetRoots();
        for (var i = 0; i < roots.length; i++) { if (roots[i].exists) { collectPresets(roots[i], found, roots[i]); } }
        presetCache = found;
        return found;
    }

    function presetFile(ref) {
        if (/\.ffx$/i.test(ref) && ae.file(ref).exists) { return ae.file(ref); }
        var lowered = String(ref).toLowerCase();
        var all = presets();
        var exact = [];
        for (var i = 0; i < all.length; i++) { if (all[i].name.toLowerCase() === lowered) { exact.push(all[i]); } }
        if (exact.length === 1) { return new File(exact[0].path); }
        if (exact.length > 1) { throw ae.fail("Ambiguous", exact.length + " presets are named '" + ref + "'; pass the path.", { candidates: exact }); }
        throw ae.fail("NotFound", "No animation preset is named '" + ref + "' and no such file exists; presets_find lists them.", { query: ref });
    }

    SnailBridge.command("presets_find", { params: ["query", "limit"], read_only: true }, function (params) {
        var needle = ae.isGiven(params.query) ? String(params.query).toLowerCase() : "";
        var limit = ae.isGiven(params.limit) ? params.limit : 60;
        var all = presets();
        var out = [];
        var total = 0;
        for (var i = 0; i < all.length; i++) {
            if (needle && all[i].name.toLowerCase().indexOf(needle) < 0 && all[i].folder.toLowerCase().indexOf(needle) < 0) { continue; }
            total++;
            if (out.length < limit) { out.push(all[i]); }
        }
        return { presets: out, matched: total, truncated: total > out.length };
    });

    SnailBridge.command("preset_apply", { params: ["comp", "layers", "preset"], open_world: true }, function (params) {
        var comp = ae.comp(params);
        if (!(params.layers instanceof Array) || params.layers.length === 0) { throw ae.fail("BadRequest", "layers lists the layers to apply the preset to."); }
        if (!ae.isGiven(params.preset)) { throw ae.fail("BadRequest", "preset names the animation preset, by name or as a path to an .ffx file."); }
        var file = presetFile(String(params.preset));
        var targets = ae.layers(comp, params.layers);
        var out = [];
        for (var i = 0; i < targets.length; i++) {
            var l = targets[i];
            var before = l.property("ADBE Effect Parade") ? l.property("ADBE Effect Parade").numProperties : 0;
            l.applyPreset(file);
            var after = l.property("ADBE Effect Parade") ? l.property("ADBE Effect Parade").numProperties : 0;
            out.push({ index: l.index, name: l.name, effects_added: after - before });
        }
        return { preset: file.fsName, layers: out };
    });

    function styles(l) {
        var group = l.property("ADBE Layer Styles");
        if (!group) { throw ae.fail("BadRequest", "'" + l.name + "' cannot carry layer styles."); }
        return group;
    }

    function styleKey(ref) {
        var lowered = String(ref).toLowerCase().replace(/[\s-]+/g, "_");
        if (STYLES[lowered]) { return lowered; }
        for (var key in STYLES) { if (STYLES.hasOwnProperty(key) && STYLES[key] === ref) { return key; } }
        var accepted = [];
        for (var k in STYLES) { if (STYLES.hasOwnProperty(k)) { accepted.push(k); } }
        throw ae.fail("BadRequest", "'" + ref + "' is not a layer style.", { accepted: accepted });
    }

    // The style's group when the layer carries it; the group exists hidden on every layer until the style is added.
    function styleOf(group, key) {
        var found = null;
        try { found = group.property(STYLES[key]); } catch (ignored) { found = null; }
        return found && found.canSetEnabled ? found : null;
    }

    function effectsCount(l) { var parade = l.property("ADBE Effect Parade"); return parade ? parade.numProperties : 0; }

    function dropEffectsAfter(l, count) {
        var parade = l.property("ADBE Effect Parade");
        while (parade && parade.numProperties > count) { parade.property(parade.numProperties).remove(); }
    }

    // Adds a style through the Layer menu with only this layer selected: the label is tried in every language, and a label that also
    // names an effect (Drop Shadow does) may add the effect first, which is removed before the next try. The selection is restored.
    function addStyleByMenu(comp, l, group, key) {
        var selected = [];
        for (var i = 0; i < comp.selectedLayers.length; i++) { selected.push(comp.selectedLayers[i]); }
        for (var s = 0; s < selected.length; s++) { selected[s].selected = false; }
        l.selected = true;
        var tried = [];
        try {
            var candidates = STYLE_LABELS[key] || [];
            for (var c = 0; c < candidates.length; c++) {
                var id = 0;
                try { id = app.findMenuCommandId(candidates[c]); } catch (ignored) { id = 0; }
                if (!id) { continue; }
                tried.push(candidates[c]);
                for (var attempt = 0; attempt < 2; attempt++) {
                    var effectsBefore = effectsCount(l);
                    app.executeCommand(id);
                    var style = styleOf(group, key);
                    if (style) { return style; }
                    dropEffectsAfter(l, effectsBefore);
                }
            }
        } finally {
            l.selected = false;
            for (var r = 0; r < selected.length; r++) { try { selected[r].selected = true; } catch (ignored2) { } }
        }
        throw ae.fail("NotSupported", "After Effects did not add the " + key + " layer style from its menu" + (tried.length ? " (tried " + tried.join(", ") + ")" : "") + "; add it from Layer > Layer Styles, then set it here.", { tried: tried });
    }

    function styleOut(style, key, time) {
        return { style: key, name: style.name, match_name: style.matchName, enabled: style.enabled, path: props.pathOf(style), parameters: parametersOut(style, time, true) };
    }

    SnailBridge.command("layer_styles", { params: ["comp", "layer", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var group = styles(l);
        var out = [];
        for (var key in STYLES) {
            if (!STYLES.hasOwnProperty(key)) { continue; }
            var style = styleOf(group, key);
            if (style) { out.push(styleOut(style, key, ae.isGiven(params.time) ? params.time : comp.time)); }
        }
        return { layer: { index: l.index, name: l.name }, styles: out, enabled: group.enabled };
    });

    SnailBridge.command("layer_style_set", { params: ["comp", "layer", "style", "enabled", "settings", "time"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var group = styles(l);
        var key = styleKey(params.style);
        var style = styleOf(group, key);
        var added = false;
        if (!style) {
            style = group.canAddProperty(STYLES[key]) ? group.addProperty(STYLES[key]) : addStyleByMenu(comp, l, group, key);
            added = true;
        }
        if (ae.isGiven(params.enabled)) { style.enabled = params.enabled === true; }
        var changed = [];
        if (params.settings && typeof params.settings === "object") { changed = applySettings(style, params.settings, params.time, comp); }
        return { layer: { index: l.index, name: l.name }, added: added, changed: changed, style: styleOut(style, key, ae.isGiven(params.time) ? params.time : comp.time) };
    });

    SnailBridge.command("layer_style_remove", { params: ["comp", "layer", "style"], destructive: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var key = styleKey(params.style);
        var style = styleOf(styles(l), key);
        if (!style) { throw ae.fail("NotFound", "'" + l.name + "' has no " + key + " layer style."); }
        style.remove();
        return { layer: { index: l.index, name: l.name }, removed: key };
    });

    // The items of a Dropdown Menu Control effect, which is the one popup whose entries a script may write.
    SnailBridge.command("dropdown_items", { params: ["comp", "layer", "path", "items"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path names the dropdown property, as effect_info lists it."); }
        var prop = SnailBridge.properties.resolve(l, params.path);
        var isDropdown = false;
        try { isDropdown = prop.isDropdownEffect === true; } catch (ignored) { isDropdown = false; }
        if (!isDropdown) { throw ae.fail("BadRequest", "'" + prop.name + "' is not the menu of a Dropdown Menu Control effect; other popups keep the items their effect defines."); }
        if (!(params.items instanceof Array) || params.items.length === 0) { throw ae.fail("BadRequest", "items lists the entries of the menu, in order."); }
        var items = [];
        for (var i = 0; i < params.items.length; i++) { items.push(String(params.items[i])); }
        // Writing the items rebuilds the effect under a new match name and its default name, so it is found again by index and renamed back.
        var effect = prop.parentProperty;
        var effectIndex = effect.propertyIndex;
        var effectName = effect.name;
        var paramIndex = prop.propertyIndex;
        prop.setPropertyParameters(items);
        var rebuilt = l.effect(effectIndex);
        rebuilt.name = effectName;
        var fresh = rebuilt.property(paramIndex);
        return { layer: { index: l.index, name: l.name }, effect: { index: effectIndex, name: rebuilt.name, match_name: rebuilt.matchName }, path: SnailBridge.properties.pathOf(fresh), items: fresh.propertyParameters, value: fresh.value };
    });
})();
