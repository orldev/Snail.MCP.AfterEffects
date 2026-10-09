// Pictures out of After Effects: one frame of a composition as a PNG the server shows the model.
(function () {
    var ae = SnailBridge.ae;

    var TICKS_BEFORE_CLEANUP = 60;

    // saveFrameToPng writes the file after the script has returned, on After Effects' own time, so the command cannot wait for it:
    // it answers at once with the path, the server watches the file grow, and a helper composition is removed some ticks later.
    SnailBridge.command("capture_frame", { params: ["comp", "time", "path", "width", "transparent"], open_world: true }, function (params) {
        var comp = ae.comp(params);
        if (!ae.isGiven(params.path)) { throw ae.fail("BadRequest", "path says where the PNG goes."); }
        var time = ae.isGiven(params.time) ? params.time : comp.time;
        var file = ae.file(params.path);
        if (!file.parent.exists) { file.parent.create(); }
        if (file.exists) { file.remove(); }
        var scale = ae.isGiven(params.width) && params.width > 0 ? params.width / comp.width : 1;
        var width = Math.round(comp.width * scale);
        var height = Math.round(comp.height * scale);
        // The frame is rendered through a helper composition: scaled when a width is asked for, and over a solid of the composition's
        // background colour unless a transparent picture is wanted, since After Effects renders the background as transparency.
        var helper = app.project.items.addComp("(snail capture)", width, height, comp.pixelAspect, comp.duration, comp.frameRate);
        helper.bgColor = comp.bgColor;
        var nested = helper.layers.add(comp);
        if (scale !== 1) { nested.property("ADBE Transform Group").property("ADBE Scale").setValue([scale * 100, scale * 100, 100]); }
        if (params.transparent !== true) {
            var backdrop = helper.layers.addSolid(comp.bgColor, "(snail backdrop)", width, height, comp.pixelAspect, comp.duration);
            backdrop.moveToEnd();
        }
        SnailBridge.later(TICKS_BEFORE_CLEANUP, function () {
            app.beginUndoGroup("Snail capture cleanup");
            try {
                if (app.project.itemByID(helper.id) === helper) {
                    for (var i = helper.numLayers; i >= 1; i--) { if (helper.layer(i).source && helper.layer(i).source.name === "(snail backdrop)") { var solid = helper.layer(i).source; helper.layer(i).remove(); solid.remove(); } }
                    helper.remove();
                }
            } finally { app.endUndoGroup(); }
        });
        helper.saveFrameToPng(time, file);
        return { path: file.fsName, width: width, height: height, time: ae.round(time), pending: true, transparent: params.transparent === true, comp: { id: comp.id, name: comp.name } };
    });
})();
