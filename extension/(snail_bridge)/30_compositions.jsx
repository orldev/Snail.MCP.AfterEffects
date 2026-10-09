// Compositions: creating, settings, duplicating and opening one in the viewer.
(function () {
    var ae = SnailBridge.ae;

    function applySettings(comp, params) {
        if (ae.isGiven(params.name)) { comp.name = params.name; }
        if (ae.isGiven(params.width)) { comp.width = params.width; }
        if (ae.isGiven(params.height)) { comp.height = params.height; }
        if (ae.isGiven(params.duration)) { comp.duration = params.duration; }
        if (ae.isGiven(params.frame_rate)) { comp.frameRate = params.frame_rate; }
        if (ae.isGiven(params.pixel_aspect)) { comp.pixelAspect = params.pixel_aspect; }
        if (ae.isGiven(params.background)) { comp.bgColor = ae.color(params.background, "background"); }
        if (ae.isGiven(params.time)) { comp.time = params.time; }
        if (ae.isGiven(params.work_area_start)) { comp.workAreaStart = params.work_area_start; }
        if (ae.isGiven(params.work_area_duration)) { comp.workAreaDuration = params.work_area_duration; }
        if (ae.isGiven(params.motion_blur)) { comp.motionBlur = params.motion_blur === true; }
        if (ae.isGiven(params.shutter_angle)) { comp.shutterAngle = params.shutter_angle; }
        if (ae.isGiven(params.hide_shy)) { comp.hideShyLayers = params.hide_shy === true; }
        if (ae.isGiven(params.folder)) { comp.parentFolder = ae.folder(params.folder); }
    }

    SnailBridge.command("comp_create", { params: ["name", "width", "height", "duration", "frame_rate", "pixel_aspect", "background", "folder"] }, function (params) {
        var name = ae.isGiven(params.name) ? params.name : "Comp";
        var width = ae.isGiven(params.width) ? params.width : 1920;
        var height = ae.isGiven(params.height) ? params.height : 1080;
        var duration = ae.isGiven(params.duration) ? params.duration : 10;
        var frameRate = ae.isGiven(params.frame_rate) ? params.frame_rate : 30;
        var pixelAspect = ae.isGiven(params.pixel_aspect) ? params.pixel_aspect : 1;
        var comp = app.project.items.addComp(name, width, height, pixelAspect, duration, frameRate);
        applySettings(comp, { background: params.background, folder: params.folder });
        comp.openInViewer();
        ae.remember(comp);
        return ae.describeItem(comp);
    });

    SnailBridge.command("comp_update", {
        params: ["comp", "name", "width", "height", "duration", "frame_rate", "pixel_aspect", "background", "time", "work_area_start", "work_area_duration", "motion_blur", "shutter_angle", "hide_shy", "folder",
            "shutter_phase", "motion_blur_samples", "motion_blur_adaptive_limit", "preserve_nested_frame_rate", "preserve_nested_resolution", "draft3d", "frame_blending", "resolution_factor",
            "display_start_frame", "display_start_time"]
    }, function (params) {
        var comp = ae.comp(params);
        applySettings(comp, params);
        if (ae.isGiven(params.shutter_phase)) { comp.shutterPhase = params.shutter_phase; }
        if (ae.isGiven(params.motion_blur_samples)) { comp.motionBlurSamplesPerFrame = params.motion_blur_samples; }
        if (ae.isGiven(params.motion_blur_adaptive_limit)) { comp.motionBlurAdaptiveSampleLimit = params.motion_blur_adaptive_limit; }
        if (ae.isGiven(params.preserve_nested_frame_rate)) { comp.preserveNestedFrameRate = params.preserve_nested_frame_rate === true; }
        if (ae.isGiven(params.preserve_nested_resolution)) { comp.preserveNestedResolution = params.preserve_nested_resolution === true; }
        if (ae.isGiven(params.draft3d)) { comp.draft3d = params.draft3d === true; }
        if (ae.isGiven(params.frame_blending)) { comp.frameBlending = params.frame_blending === true; }
        if (params.resolution_factor instanceof Array && params.resolution_factor.length === 2) { comp.resolutionFactor = params.resolution_factor; }
        if (ae.isGiven(params.display_start_frame)) { comp.displayStartFrame = params.display_start_frame; }
        if (ae.isGiven(params.display_start_time)) { comp.displayStartTime = params.display_start_time; }
        return ae.describeItem(comp);
    });

    SnailBridge.command("comp_duplicate", { params: ["comp", "name"] }, function (params) {
        var copy = ae.comp(params).duplicate();
        if (ae.isGiven(params.name)) { copy.name = params.name; }
        ae.remember(copy);
        return ae.describeItem(copy);
    });

    SnailBridge.command("comp_open", { params: ["comp", "time"] }, function (params) {
        var comp = ae.comp(params);
        comp.openInViewer();
        if (ae.isGiven(params.time)) { comp.time = params.time; }
        return ae.describeItem(comp);
    });
})();
