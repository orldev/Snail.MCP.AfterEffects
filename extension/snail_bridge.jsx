// Snail bridge loader. After Effects runs this file from a Scripts/Startup folder; the bridge itself lives in the
// (snail_bridge) folder beside Startup, which the parentheses keep out of the File > Scripts menu. The modules are
// loaded in name order, so their numeric prefixes are their load order. A module that fails to load is written to
// ~/.s3l-mcp-aftereffects/state/loader.log (~/.snail-mcp-aftereffects/state/loader.log while only the folder of a server from before the
// rename exists) instead of raising a dialog, because a dialog at start-up stops After Effects.
(function () {
    function note(text) {
        try {
            var home = Folder("~").fsName;
            var root = new Folder(home + "/.s3l-mcp-aftereffects").exists ? home + "/.s3l-mcp-aftereffects" : home + "/.snail-mcp-aftereffects";
            var folder = new Folder(root + "/state");
            if (!folder.exists) { folder.create(); }
            var file = new File(folder.fsName + "/loader.log");
            file.encoding = "UTF-8";
            file.lineFeed = "Unix";
            if (file.open("a")) { file.writeln(String(new Date()) + " " + text); file.close(); }
        } catch (ignored) {
        }
    }
    var here = new File($.fileName).parent;
    var candidates = [new Folder(here.fsName + "/(snail_bridge)"), new Folder(here.parent.fsName + "/(snail_bridge)")];
    var modules = null;
    for (var i = 0; i < candidates.length; i++) {
        if (candidates[i].exists) { modules = candidates[i]; break; }
    }
    if (modules === null) {
        note("no (snail_bridge) folder beside " + here.fsName);
        return;
    }
    var found = modules.getFiles("*.jsx");
    var byName = {};
    var names = [];
    for (var n = 0; n < found.length; n++) {
        byName[found[n].name] = found[n];
        names.push(found[n].name);
    }
    names.sort();
    var files = [];
    for (var m = 0; m < names.length; m++) { files.push(byName[names[m]]); }
    $.global.SNAIL_BRIDGE_FILES = files;
    for (var j = 0; j < files.length; j++) {
        try {
            $.evalFile(files[j]);
        } catch (e) {
            note("module " + files[j].name + " failed to load: " + e.message + " (line " + e.line + ")");
            return;
        }
    }
    try {
        SnailBridge.start();
    } catch (e) {
        note("the bridge did not start: " + e.message + " (line " + e.line + ")");
    }
})();
