// After Effects itself: any command of its menus. Undo and redo are not here on purpose: app.executeCommand(16) run from the
// bridge's scheduled task never returns, so the server runs them through a script of their own instead.
(function () {
    var ae = SnailBridge.ae;

    SnailBridge.command("menu_command", { params: ["name", "id"], destructive: true }, function (params) {
        var id = ae.isGiven(params.id) ? params.id : 0;
        if (!id && ae.isGiven(params.name)) { id = app.findMenuCommandId(params.name); }
        if (!id) { throw ae.fail("NoSuchMenuCommand", "No menu command is named '" + params.name + "' in this After Effects' language; pass its id instead."); }
        app.executeCommand(id);
        return { id: id, name: ae.isGiven(params.name) ? params.name : null };
    });
})();
