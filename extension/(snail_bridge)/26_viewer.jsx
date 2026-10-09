// Viewer: the panel the user looks at, its views and their options.
// View.startPlayback and View.saveBlittedImageToPng exist but reject every argument type (After Effects 26.5), so they stay out.
(function () {
    var ae = SnailBridge.ae;

    function viewer() {
        var active = app.activeViewer;
        if (!active) { throw ae.fail("NoViewer", "No viewer is open; comp_open opens a composition in one."); }
        return active;
    }

    function enumOut(enumeration, value) { try { return ae.enumName(enumeration, value); } catch (ignored) { return String(value); } }

    function optionsOut(options) {
        var out = {};
        try { out.channels = enumOut(ChannelType, options.channels); } catch (ignored1) { }
        try { out.checkerboards = options.checkerboards; } catch (ignored2) { }
        try { out.exposure = ae.round(options.exposure); } catch (ignored3) { }
        try { out.fast_preview = enumOut(FastPreviewType, options.fastPreview); } catch (ignored4) { }
        try { out.guides_locked = options.guidesLocked; out.guides_snap = options.guidesSnap; out.guides_visibility = options.guidesVisibility; } catch (ignored5) { }
        try { out.rulers = options.rulers; } catch (ignored6) { }
        try { out.zoom = ae.round(options.zoom); } catch (ignored7) { }
        return out;
    }

    function viewerOut(v) {
        var views = [];
        for (var i = 0; i < v.views.length; i++) { views.push({ index: i + 1, active: v.views[i].active === true, options: optionsOut(v.views[i].options) }); }
        return { type: enumOut(ViewerType, v.type), active: v.active === true, maximized: v.maximized === true, active_view: v.activeViewIndex + 1, views: views };
    }

    function viewAt(v, index) {
        var which = ae.isGiven(index) ? index - 1 : v.activeViewIndex;
        if (which < 0 || which >= v.views.length) { throw ae.fail("NotFound", "The viewer has no view " + index + "; it has " + v.views.length + "."); }
        return v.views[which];
    }

    SnailBridge.command("viewer_info", { params: [], read_only: true }, function () {
        return viewerOut(viewer());
    });

    SnailBridge.command("viewer_set", { params: ["view", "channels", "checkerboards", "exposure", "fast_preview", "guides_locked", "guides_snap", "guides_visibility", "rulers", "zoom", "maximized", "activate"] }, function (params) {
        var v = viewer();
        var view = viewAt(v, params.view);
        var options = view.options;
        var changed = [];
        if (ae.isGiven(params.channels)) { options.channels = ae.enumValue(ChannelType, params.channels, "channel"); changed.push("channels"); }
        if (ae.isGiven(params.checkerboards)) { options.checkerboards = params.checkerboards === true; changed.push("checkerboards"); }
        if (ae.isGiven(params.exposure)) { options.exposure = params.exposure; changed.push("exposure"); }
        if (ae.isGiven(params.fast_preview)) { options.fastPreview = ae.enumValue(FastPreviewType, params.fast_preview, "fast preview"); changed.push("fast_preview"); }
        if (ae.isGiven(params.guides_locked)) { options.guidesLocked = params.guides_locked === true; changed.push("guides_locked"); }
        if (ae.isGiven(params.guides_snap)) { options.guidesSnap = params.guides_snap === true; changed.push("guides_snap"); }
        if (ae.isGiven(params.guides_visibility)) { options.guidesVisibility = params.guides_visibility === true; changed.push("guides_visibility"); }
        if (ae.isGiven(params.rulers)) { options.rulers = params.rulers === true; changed.push("rulers"); }
        if (ae.isGiven(params.zoom)) { options.zoom = params.zoom; changed.push("zoom"); }
        if (ae.isGiven(params.maximized)) { v.maximized = params.maximized === true; changed.push("maximized"); }
        if (params.activate === true) { view.setActive(); v.setActive(); changed.push("activate"); }
        if (changed.length === 0) { throw ae.fail("BadRequest", "Nothing to change: pass a view option, maximized or activate."); }
        var out = viewerOut(v);
        out.changed = changed;
        return out;
    });


})();
