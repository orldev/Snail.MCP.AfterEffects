// Layout: measuring layers, converting points between a layer and its composition, the selection, guides, splitting and copying
// layers, placing a layer by three corners, and finding a layer by its id.
(function () {
    var ae = SnailBridge.ae;

    function rectOut(rect) { return { left: ae.round(rect.left), top: ae.round(rect.top), width: ae.round(rect.width), height: ae.round(rect.height) }; }

    // Converts with the composition at the given time, since the conversion reads the layer at the current time only.
    function atTime(comp, time, work) {
        if (!ae.isGiven(time)) { return work(); }
        var was = comp.time;
        comp.time = time;
        try { return work(); }
        finally { comp.time = was; }
    }

    function toComp(l, point) { return ae.numbers(l.sourcePointToComp([point[0], point[1]])); }

    SnailBridge.command("layer_bounds", { params: ["comp", "layer", "time", "extents"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.hasPixels(l)) { throw ae.fail("BadRequest", "'" + l.name + "' has no pixels to measure."); }
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var rect = l.sourceRectAtTime(time, params.extents === true);
        var out = { layer: { index: l.index, name: l.name }, time: ae.round(time), source: rectOut(rect) };
        if (!l.threeDLayer) {
            atTime(comp, params.time, function () {
                out.comp = {
                    top_left: toComp(l, [rect.left, rect.top]),
                    top_right: toComp(l, [rect.left + rect.width, rect.top]),
                    bottom_left: toComp(l, [rect.left, rect.top + rect.height]),
                    bottom_right: toComp(l, [rect.left + rect.width, rect.top + rect.height]),
                    center: toComp(l, [rect.left + rect.width / 2, rect.top + rect.height / 2])
                };
                var xs = [out.comp.top_left[0], out.comp.top_right[0], out.comp.bottom_left[0], out.comp.bottom_right[0]];
                var ys = [out.comp.top_left[1], out.comp.top_right[1], out.comp.bottom_left[1], out.comp.bottom_right[1]];
                var minX = Math.min.apply(null, xs), maxX = Math.max.apply(null, xs), minY = Math.min.apply(null, ys), maxY = Math.max.apply(null, ys);
                out.comp.box = { left: ae.round(minX), top: ae.round(minY), width: ae.round(maxX - minX), height: ae.round(maxY - minY) };
                return null;
            });
        }
        return out;
    });

    SnailBridge.command("point_convert", { params: ["comp", "layer", "point", "to", "time"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!(params.point instanceof Array) || params.point.length < 2) { throw ae.fail("BadRequest", "point is [x, y]."); }
        var to = ae.isGiven(params.to) ? String(params.to) : "comp";
        if (to !== "comp" && to !== "source") { throw ae.fail("BadRequest", "to is comp (from the layer's pixels into the composition) or source (the other way).", { accepted: ["comp", "source"] }); }
        var converted = atTime(comp, params.time, function () {
            return to === "comp" ? l.sourcePointToComp([params.point[0], params.point[1]]) : l.compPointToSource([params.point[0], params.point[1]]);
        });
        return { layer: { index: l.index, name: l.name }, from: to === "comp" ? "source" : "comp", to: to, point: ae.numbers(converted) };
    });

    SnailBridge.command("selection", { params: ["comp"], read_only: true }, function (params) {
        var comp = ae.comp(params);
        var layers = [];
        for (var i = 0; i < comp.selectedLayers.length; i++) { layers.push({ index: comp.selectedLayers[i].index, name: comp.selectedLayers[i].name }); }
        var properties = [];
        for (var p = 0; p < comp.selectedProperties.length; p++) {
            var prop = comp.selectedProperties[p];
            var owner = prop;
            while (owner && owner.propertyDepth > 0) { owner = owner.parentProperty; }
            var segments = [];
            var walk = prop;
            while (walk && walk.propertyDepth > 0) { segments.unshift(walk.matchName); walk = walk.parentProperty; }
            properties.push({ layer: owner ? { index: owner.index, name: owner.name } : null, name: prop.name, match_name: prop.matchName, path: segments.join("/"), is_group: prop.propertyType !== PropertyType.PROPERTY });
        }
        return { comp: { id: comp.id, name: comp.name }, layers: layers, properties: properties, time: ae.round(comp.time) };
    });

    function guideItem(params) {
        var it = ae.isGiven(params.item) ? ae.item(params.item) : ae.comp(params);
        if (!(it instanceof CompItem) && !(it instanceof FootageItem)) { throw ae.fail("BadRequest", "'" + it.name + "' is a " + ae.kindOf(it) + "; guides live on compositions and footage."); }
        return it;
    }

    function orientationIn(value) {
        var lowered = String(value).toLowerCase();
        if (lowered === "horizontal" || lowered === "0") { return 0; }
        if (lowered === "vertical" || lowered === "1") { return 1; }
        throw ae.fail("BadRequest", "'" + value + "' is not a guide orientation.", { accepted: ["horizontal", "vertical"] });
    }

    // addGuide takes 0 for horizontal and 1 for vertical; a guide reads back as 14212 and 14213 in the same order.
    function orientationOut(code) { return code === 0 || code === 14212 ? "horizontal" : "vertical"; }

    function guidesOut(it) {
        var out = [];
        var guides = it.guides || [];
        for (var i = 0; i < guides.length; i++) {
            out.push({ index: i, orientation: orientationOut(guides[i].orientationType), position: ae.round(guides[i].position) });
        }
        return { item: { id: it.id, name: it.name, kind: ae.kindOf(it) }, guides: out };
    }

    SnailBridge.command("guides_list", { params: ["comp", "item"], read_only: true }, function (params) {
        return guidesOut(guideItem(params));
    });

    SnailBridge.command("guides_set", { params: ["comp", "item", "add", "move", "remove", "clear"], destructive: true }, function (params) {
        var it = guideItem(params);
        var changed = [];
        var i;
        if (params.clear === true) {
            for (i = (it.guides || []).length - 1; i >= 0; i--) { it.removeGuide(i); }
            changed.push("clear");
        }
        if (params.remove instanceof Array) {
            var indices = params.remove.slice(0).sort(function (a, b) { return b - a; });
            for (i = 0; i < indices.length; i++) {
                if (indices[i] < 0 || indices[i] >= it.guides.length) { throw ae.fail("NotFound", "No guide " + indices[i] + "; there are " + it.guides.length + ", numbered from 0."); }
                it.removeGuide(indices[i]);
            }
            changed.push("remove " + indices.length);
        }
        if (params.move instanceof Array) {
            for (i = 0; i < params.move.length; i++) {
                var step = params.move[i];
                if (!step || typeof step.index !== "number" || typeof step.position !== "number") { throw ae.fail("BadRequest", "move takes {index, position} pairs."); }
                it.setGuide(step.position, step.index);
            }
            changed.push("move " + params.move.length);
        }
        if (params.add instanceof Array) {
            for (i = 0; i < params.add.length; i++) {
                var guide = params.add[i];
                if (!guide || typeof guide.position !== "number") { throw ae.fail("BadRequest", "add takes {orientation, position} pairs; position in pixels of the item."); }
                it.addGuide(orientationIn(ae.isGiven(guide.orientation) ? guide.orientation : "horizontal"), guide.position);
            }
            changed.push("add " + params.add.length);
        }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass add, move, remove or clear."); }
        var out = guidesOut(it);
        out.changed = changed;
        return out;
    });

    SnailBridge.command("layer_split", { params: ["comp", "layer", "time"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        if (time <= l.inPoint || time >= l.outPoint) { throw ae.fail("BadRequest", "time must fall inside the layer, between " + ae.round(l.inPoint) + " and " + ae.round(l.outPoint) + " s."); }
        var index = l.index;
        var name = l.name;
        var created = l.splitLayer(time);
        var second = created || comp.layer(index);
        var first = comp.layer(second.index + 1);
        return { time: ae.round(time), before: { index: first.index, name: first.name, in_point: ae.round(first.inPoint), out_point: ae.round(first.outPoint) }, after: { index: second.index, name: second.name, in_point: ae.round(second.inPoint), out_point: ae.round(second.outPoint) }, was: { index: index, name: name } };
    });

    SnailBridge.command("layer_copy_to_comp", { params: ["comp", "layer", "target"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.isGiven(params.target)) { throw ae.fail("BadRequest", "target names the composition the copy goes to."); }
        var target = ae.item(params.target, "composition");
        if (!(target instanceof CompItem)) { throw ae.fail("NotAComposition", "'" + target.name + "' is not a composition."); }
        var before = target.numLayers;
        l.copyToComp(target);
        var copy = target.numLayers > before ? target.layer(1) : null;
        return { from: { comp: { id: comp.id, name: comp.name }, layer: { index: l.index, name: l.name } }, to: { comp: { id: target.id, name: target.name }, layer: copy ? ae.describeLayer(copy) : null } };
    });

    var PLACED = [["anchorPoint", "ADBE Anchor Point"], ["position", "ADBE Position"], ["scale", "ADBE Scale"], ["rotation", "ADBE Rotate Z"], ["orientation", "ADBE Orientation"], ["xRotation", "ADBE Rotate X"], ["yRotation", "ADBE Rotate Y"], ["zRotation", "ADBE Rotate Z"]];

    SnailBridge.command("layer_place_corners", { params: ["comp", "layer", "top_left", "top_right", "bottom_left", "apply", "time"] }, function (params) {
        var comp = ae.comp(params);
        var l = ae.layer(comp, params.layer);
        if (!ae.hasPixels(l)) { throw ae.fail("BadRequest", "'" + l.name + "' has no pixels to place."); }
        var corners = ["top_left", "top_right", "bottom_left"];
        for (var c = 0; c < corners.length; c++) {
            if (!(params[corners[c]] instanceof Array) || params[corners[c]].length < 2) { throw ae.fail("BadRequest", corners[c] + " is [x, y] or [x, y, z] in composition pixels."); }
        }
        function point3(p) { return [p[0], p[1], p.length > 2 ? p[2] : 0]; }
        var transform = atTime(comp, params.time, function () { return l.calculateTransformFromPoints(point3(params.top_left), point3(params.top_right), point3(params.bottom_left)); });
        var out = { layer: { index: l.index, name: l.name }, transform: {} };
        var group = l.property("ADBE Transform Group");
        for (var i = 0; i < PLACED.length; i++) {
            var value = transform[PLACED[i][0]];
            if (value === undefined) { continue; }
            out.transform[PLACED[i][0]] = value instanceof Array ? ae.numbers(value) : ae.round(value);
            if (params.apply === false) { continue; }
            var prop = group.property(PLACED[i][1]);
            if (!prop) { continue; }
            try {
                if (ae.isGiven(params.time)) { prop.setValueAtTime(params.time, value); } else if (prop.numKeys > 0) { prop.setValueAtTime(comp.time, value); } else { prop.setValue(value); }
            } catch (ignored) { }
        }
        out.applied = params.apply !== false;
        return out;
    });

    SnailBridge.command("layer_by_id", { params: ["id"], read_only: true }, function (params) {
        if (typeof params.id !== "number") { throw ae.fail("BadRequest", "id is the layer's number, as layer_info reports it."); }
        var l = app.project.layerByID(params.id);
        if (!l) { throw ae.fail("NotFound", "No layer in the project has the id " + params.id + "."); }
        var comp = l.containingComp;
        return { comp: { id: comp.id, name: comp.name }, layer: ae.describeLayer(l) };
    });
})();
