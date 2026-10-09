// What the bridge says about itself and about After Effects, and the raw script for what no command covers.
SnailBridge.command("ping", { params: [], read_only: true }, function () {
    var state = SnailBridge.state;
    return {
        app: app.version,
        build: app.buildName,
        os: $.os,
        bridge: SnailBridge.VERSION,
        protocol: SnailBridge.PROTOCOL,
        abilities: SnailBridge.ABILITIES,
        digest: SnailBridge.digest(),
        commands: SnailBridge.names.length,
        ticks: state.ticks,
        uptime_ms: new Date().getTime() - state.startedAt,
        handled: state.handled,
        failed: state.failed,
        revivals: state.revivals,
        scripting_allowed: SnailBridge.scriptingAllowed(),
        data_root: SnailBridge.dataRoot(),
        links: SnailBridge.links()
    };
});

SnailBridge.command("app_info", { params: [], read_only: true }, function () {
    var project = app.project;
    return {
        version: app.version,
        build: app.buildName,
        language: app.isoLanguage,
        is_render_engine: app.isRenderEngine,
        memory_in_use: app.memoryInUse,
        project: project ? {
            file: project.file ? project.file.fsName : null,
            items: project.numItems,
            dirty: project.dirty === true,
            active_item: project.activeItem ? project.activeItem.name : null
        } : null
    };
});

SnailBridge.command("script", { params: ["code"], destructive: true }, function (params) {
    if (typeof params.code !== "string" || params.code === "") {
        throw SnailBridge.fail("BadRequest", "'script' needs code: the ExtendScript to run.");
    }
    var value = eval(params.code);
    return { value: value === undefined ? null : String(value) };
});
