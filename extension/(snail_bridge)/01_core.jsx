// The bridge: finds the servers announced under the data directory, dials each of them, and on every tick of
// After Effects' scheduler reads their requests, runs them on the main thread and writes the replies.
// Everything runs inside try/catch, because an uncaught exception here is a modal dialog that stops After Effects.
// Assigned to $.global on purpose: a file the loader evaluates from inside its own function would otherwise keep its
// top-level variables to that function, and no later script would see the bridge.
$.global.SnailBridge = (function () {
    var VERSION = "0.1.0";
    var PROTOCOL = 1;
    var ABILITIES = ["hello_token", "params_gate", "undo_group", "deadline", "expired"];
    var TICK_MS = 50;
    var SCAN_EVERY_TICKS = 20;
    var MOST_REQUESTS_PER_TICK = 50;
    var RETRY_REFUSED_AFTER_TICKS = 1200;
    var MOST_LOG_BYTES = 1048576;
    var WATCH_MS = 5000;
    var SILENT_FOR_MS = 15000;
    var ALWAYS_ALLOWED_KEYS = ["agent"];

    var registry = {};
    var names = [];
    var deferred = [];
    var state = {
        links: {},
        ticks: 0,
        startedAt: new Date().getTime(),
        lastTickAt: 0,
        handled: 0,
        failed: 0,
        executing: null,
        taskId: 0,
        watchId: 0,
        revivals: 0,
        digest: null,
        instance: null,
        lastCommandAt: 0
    };

    // The name this After Effects answers to among several: the environment names a worker the server launched, anything else is the desk.
    function workerName() {
        var named = "";
        try { named = $.global.SNAIL_BRIDGE_WORKER ? String($.global.SNAIL_BRIDGE_WORKER) : String(app.getenv("SNAIL_BRIDGE_WORKER") || ""); } catch (ignored) { named = ""; }
        named = named.replace(/^\s+|\s+$/g, "");
        return named === "" ? "desk" : named;
    }

    function instanceId() {
        if (state.instance === null) {
            var hex = "";
            for (var i = 0; i < 8; i++) { hex += "0123456789abcdef".charAt(Math.floor(Math.random() * 16)); }
            state.instance = hex;
        }
        return state.instance;
    }

    function now() { return new Date().getTime(); }

    function dataRoot() {
        if ($.global.SNAIL_BRIDGE_DATA_ROOT) { return String($.global.SNAIL_BRIDGE_DATA_ROOT); }
        try { var fromEnvironment = app.getenv("SNAIL_BRIDGE_DATA_ROOT"); if (fromEnvironment) { return String(fromEnvironment); } } catch (ignored) { }
        // The server's folder under its s3l name; the snail name for a server from before the rename.
        var home = Folder("~").fsName;
        return new Folder(home + "/.s3l-mcp-aftereffects").exists ? home + "/.s3l-mcp-aftereffects" : home + "/.snail-mcp-aftereffects";
    }

    // Minutes a worker may go without a command before it quits on its own; 0, and the desk always, never quit.
    function idleMinutes() {
        if (workerName() === "desk") { return 0; }
        var text = "";
        try { text = String(app.getenv("SNAIL_BRIDGE_IDLE_MINUTES") || ""); } catch (ignored) { text = ""; }
        var minutes = parseInt(text, 10);
        return isNaN(minutes) || minutes < 0 ? 0 : minutes;
    }

    function retireIfIdle() {
        var minutes = idleMinutes();
        if (minutes === 0 || state.executing) { return; }
        var since = state.lastCommandAt || state.startedAt;
        if (now() - since < minutes * 60000) { return; }
        log("worker " + workerName() + " idle for " + minutes + " min; quitting");
        try { if (app.project) { app.project.close(CloseOptions.DO_NOT_SAVE_CHANGES); } } catch (ignoredClose) { }
        app.quit();
    }

    function stateFolder() { return new Folder(dataRoot() + "/state"); }

    function serversFolder() { return new Folder(dataRoot() + "/state/servers"); }

    function readText(file) {
        if (!file.exists) { return null; }
        file.encoding = "UTF-8";
        if (!file.open("r")) { return null; }
        var text = file.read();
        file.close();
        return text;
    }

    function token() {
        var text = readText(new File(dataRoot() + "/state/token"));
        return text === null ? "" : text.replace(/^\s+|\s+$/g, "");
    }

    function log(text) {
        try {
            var folder = stateFolder();
            if (!folder.exists) { folder.create(); }
            var file = new File(folder.fsName + "/bridge.log");
            file.encoding = "UTF-8";
            file.lineFeed = "Unix";
            var mode = file.exists && file.length > MOST_LOG_BYTES ? "w" : "a";
            if (file.open(mode)) {
                file.writeln(new Date().toISOString ? new Date().toISOString() : String(new Date()), " ", text);
                file.close();
            }
        } catch (ignored) {
        }
    }

    function contains(list, item) {
        for (var i = 0; i < list.length; i++) { if (list[i] === item) { return true; } }
        return false;
    }

    function keysOf(object) {
        var keys = [];
        for (var key in object) { if (Object.prototype.hasOwnProperty.call(object, key)) { keys.push(key); } }
        return keys;
    }

    function fnv1a(seed, text) {
        var hash = seed;
        for (var i = 0; i < text.length; i++) {
            hash ^= text.charCodeAt(i);
            hash = (hash + ((hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24))) >>> 0;
        }
        return hash;
    }

    // One number for the installed build: the module files in name order, each as its name and its text with
    // line endings normalised, so the server can say whether what After Effects runs is what it ships.
    function digest() {
        if (state.digest !== null) { return state.digest; }
        var files = $.global.SNAIL_BRIDGE_FILES || [];
        var hash = 2166136261;
        for (var i = 0; i < files.length; i++) {
            var text = readText(files[i]);
            hash = fnv1a(hash, files[i].name + "\n");
            hash = fnv1a(hash, text === null ? "" : text.replace(/\r\n/g, "\n"));
        }
        var hex = hash.toString(16);
        while (hex.length < 8) { hex = "0" + hex; }
        state.digest = hex;
        return hex;
    }

    function scriptingAllowed() {
        try { return app.preferences.getPrefAsLong("Main Pref Section", "Pref_SCRIPTING_FILE_NETWORK_SECURITY") === 1; }
        catch (ignored) { return false; }
    }

    // A failure the handler meant: travels as data with the type it names, unlike an exception After Effects threw.
    function fail(type, message, details) {
        return { snailType: type, message: message, details: details || null };
    }

    function command(name, spec, handler) {
        registry[name] = {
            name: name,
            params: spec.params || [],
            readOnly: spec.read_only === true,
            destructive: spec.destructive === true,
            undoGroup: spec.undo_group !== false,
            handler: handler
        };
        names.push(name);
    }

    function unknownKeys(params, allowed) {
        var unknown = [];
        var keys = keysOf(params);
        for (var i = 0; i < keys.length; i++) {
            if (!contains(allowed, keys[i]) && !contains(ALWAYS_ALLOWED_KEYS, keys[i])) { unknown.push(keys[i]); }
        }
        return unknown;
    }

    function errorOf(e) {
        if (e && e.snailType) {
            return { type: e.snailType, message: e.message, details: e.details };
        }
        var error = { type: (e && e.name) || "Error", message: String(e && e.message !== undefined ? e.message : e) };
        if (e && e.line) { error.line = e.line; }
        if (e && e.fileName) { error.file = String(e.fileName).replace(/^.*[\/\\]/, ""); }
        return error;
    }

    // Runs one command the way every entry point must: unknown command and unknown keys are refused before anything runs.
    function invoke(name, params, request) {
        var spec = registry[name];
        if (!spec) {
            throw fail("UnknownCommand", "The bridge has no command named '" + name + "'.", { commands: names.length, bridge: VERSION });
        }
        params = params || {};
        var unknown = unknownKeys(params, spec.params);
        if (unknown.length > 0) {
            throw fail("BadRequest", "'" + name + "' does not read: " + unknown.join(", ") + ".", { unknown: unknown, accepted: spec.params });
        }
        return spec.handler(params, request || {});
    }

    function handle(request) {
        var reply = { id: request.id };
        var spec = registry[request.command];
        if (request.deadline_at && now() > request.deadline_at) {
            reply.ok = false;
            reply.error = { type: "Expired", message: "'" + request.command + "' reached the bridge after its caller stopped waiting; it did not run." };
            return reply;
        }
        var startedAt = now();
        var grouped = !spec || spec.undoGroup;
        state.executing = request.command;
        state.lastCommandAt = startedAt;
        if (grouped) { app.beginUndoGroup("Snail " + request.command); }
        // A dialog a command raises would hold the main thread until a person closes it; suppressed, After Effects takes the default answer.
        try { app.beginSuppressDialogs(); } catch (ignoredBegin) { }
        try {
            reply.ok = true;
            reply.result = invoke(request.command, request.params, request);
            state.handled++;
        } catch (e) {
            reply.ok = false;
            reply.error = errorOf(e);
            state.failed++;
        }
        try { app.endSuppressDialogs(false); } catch (ignoredEnd) { }
        if (grouped) { app.endUndoGroup(); }
        state.executing = null;
        reply.ms = now() - startedAt;
        return reply;
    }

    function hello() {
        return {
            hello: {
                token: token(),
                bridge: VERSION,
                protocol: PROTOCOL,
                abilities: ABILITIES,
                digest: digest(),
                commands: names.length,
                app: app.version,
                build: app.buildName,
                os: $.os,
                scripting_allowed: scriptingAllowed(),
                data_root: dataRoot(),
                worker: workerName(),
                instance: instanceId()
            }
        };
    }

    function announcedServers() {
        var folder = serversFolder();
        if (!folder.exists) { return []; }
        var files = folder.getFiles("*.json");
        var servers = [];
        for (var i = 0; i < files.length; i++) {
            try {
                var text = readText(files[i]);
                if (text === null || text === "") { continue; }
                var info = JSON.parse(text);
                if (info && info.port) { servers.push({ port: info.port, host: info.host ? String(info.host) : "127.0.0.1", pid: info.pid, name: String(files[i].name) }); }
            } catch (ignored) {
            }
        }
        var own = ownServerPid();
        if (own) {
            var mine = [];
            for (var o = 0; o < servers.length; o++) { if (String(servers[o].pid) === own) { mine.push(servers[o]); } }
            return mine;
        }
        return servers;
    }

    // A worker the server launched dials that server alone; the desk dials every server announced.
    function ownServerPid() {
        try { var pid = app.getenv("SNAIL_BRIDGE_SERVER_PID"); return pid ? String(pid) : ""; } catch (ignored) { return ""; }
    }

    function connect(link) {
        var socket = new Socket();
        socket.timeout = 0;
        if (!socket.open(link.host + ":" + link.port, "UTF-8")) {
            link.failures++;
            return false;
        }
        link.socket = socket;
        link.welcomed = false;
        link.failures = 0;
        socket.writeln(JSON.stringify(hello()));
        log("connected to server on port " + link.port);
        return true;
    }

    function drop(link, why) {
        if (link.socket) {
            try { link.socket.close(); } catch (ignored) { }
        }
        link.socket = null;
        link.welcomed = false;
        log("link to port " + link.port + " closed: " + why);
    }

    function scan() {
        var servers = announcedServers();
        var seen = {};
        for (var i = 0; i < servers.length; i++) {
            var key = String(servers[i].port);
            seen[key] = true;
            if (!state.links[key]) {
                state.links[key] = { port: servers[i].port, host: servers[i].host, pid: servers[i].pid, socket: null, welcomed: false, failures: 0, retryAtTick: 0, server: null };
            }
        }
        var keys = keysOf(state.links);
        for (var j = 0; j < keys.length; j++) {
            if (!seen[keys[j]]) {
                drop(state.links[keys[j]], "the server withdrew its announcement");
                delete state.links[keys[j]];
            }
        }
    }

    function control(link, message) {
        if (message.welcome) {
            link.welcomed = true;
            link.server = message.welcome;
            log("welcomed by " + (message.welcome.name || "server") + " " + (message.welcome.version || "") + " on port " + link.port);
            return true;
        }
        if (message.refused) {
            log("refused by the server on port " + link.port + ": " + (message.refused.reason || "no reason given"));
            drop(link, "refused");
            link.retryAtTick = state.ticks + RETRY_REFUSED_AFTER_TICKS;
            return true;
        }
        return false;
    }

    function serve(link) {
        var socket = link.socket;
        var line = socket.readln();
        var served = 0;
        while (line !== "" && served < MOST_REQUESTS_PER_TICK) {
            served++;
            var message;
            try { message = JSON.parse(line); }
            catch (e) {
                socket.writeln(JSON.stringify({ ok: false, error: { type: "BadRequest", message: "The line is not JSON." } }));
                line = socket.readln();
                continue;
            }
            if (!control(link, message)) {
                socket.writeln(JSON.stringify(handle(message)));
            }
            if (!link.socket) { return; }
            line = socket.readln();
        }
    }

    // Work a command leaves for a later tick, such as removing a helper it made once After Effects has finished writing a file.
    function later(ticksFromNow, work) {
        deferred.push({ at: state.ticks + ticksFromNow, work: work });
    }

    function runDeferred() {
        var kept = [];
        for (var i = 0; i < deferred.length; i++) {
            if (deferred[i].at > state.ticks) { kept.push(deferred[i]); continue; }
            try { deferred[i].work(); }
            catch (e) { log("deferred work failed: " + (e && e.message)); }
        }
        deferred = kept;
    }

    function tick() {
        var at = now();
        state.lastTickAt = at;
        state.ticks++;
        if (state.ticks % SCAN_EVERY_TICKS === 1) { scan(); retireIfIdle(); }
        if (deferred.length > 0) { runDeferred(); }
        var keys = keysOf(state.links);
        for (var i = 0; i < keys.length; i++) {
            var link = state.links[keys[i]];
            if (link.socket && !link.socket.connected) { drop(link, "the connection ended"); }
            if (!link.socket) {
                if (state.ticks >= link.retryAtTick && state.ticks % SCAN_EVERY_TICKS === 1) { connect(link); }
                continue;
            }
            serve(link);
        }
    }

    function linksReport() {
        var report = [];
        var keys = keysOf(state.links);
        for (var i = 0; i < keys.length; i++) {
            var link = state.links[keys[i]];
            report.push({ port: link.port, pid: link.pid, up: !!(link.socket && link.socket.connected), welcomed: link.welcomed, server: link.server });
        }
        return report;
    }

    function start() {
        stop();
        beat();
        watch();
        log("bridge " + VERSION + " started, task " + state.taskId + ", " + names.length + " commands, digest " + digest());
    }

    // The scheduled task that reads the links. After Effects drops a task whose function throws its way out, so the body catches
    // everything; the watchdog below is for the throws it cannot catch and for a task After Effects cancelled on its own.
    function beat() {
        $.global.__snailBridgeTick = function () {
            try { tick(); }
            catch (e) { log("tick failed: " + (e && e.message) + " at line " + (e && e.line)); }
        };
        state.lastTickAt = now();
        state.taskId = app.scheduleTask("__snailBridgeTick()", TICK_MS, true);
        $.global.__snailBridgeTaskId = state.taskId;
    }

    // A second task, rare and doing nothing that can fail, which puts the first one back when it has gone quiet. Without it a single
    // command that took the tick down left the bridge dead until somebody installed it again, with After Effects looking fine.
    function watch() {
        $.global.__snailBridgeWatch = function () {
            try {
                if (now() - state.lastTickAt < SILENT_FOR_MS) { return; }
                state.revivals++;
                log("the tick went quiet for " + Math.round((now() - state.lastTickAt) / 1000) + " s; starting it again (" + state.revivals + ")");
                beat();
            } catch (ignored) { }
        };
        state.watchId = app.scheduleTask("__snailBridgeWatch()", WATCH_MS, true);
        $.global.__snailBridgeWatchId = state.watchId;
    }

    function stop() {
        var previous = $.global.__snailBridgeTaskId;
        if (previous) {
            try { app.cancelTask(previous); } catch (ignored) { }
            $.global.__snailBridgeTaskId = 0;
        }
        var watching = $.global.__snailBridgeWatchId;
        if (watching) {
            try { app.cancelTask(watching); } catch (ignoredWatch) { }
            $.global.__snailBridgeWatchId = 0;
        }
        var keys = keysOf(state.links);
        for (var i = 0; i < keys.length; i++) { drop(state.links[keys[i]], "the bridge stopped"); }
    }

    return {
        VERSION: VERSION,
        workerName: workerName,
        PROTOCOL: PROTOCOL,
        ABILITIES: ABILITIES,
        command: command,
        invoke: invoke,
        errorOf: errorOf,
        later: later,
        fail: fail,
        start: start,
        stop: stop,
        state: state,
        names: names,
        registry: registry,
        digest: digest,
        dataRoot: dataRoot,
        scriptingAllowed: scriptingAllowed,
        links: linksReport,
        log: log
    };
})();
