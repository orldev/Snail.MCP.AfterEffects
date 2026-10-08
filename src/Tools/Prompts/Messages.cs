namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>Human-facing texts: errors and hints in tool responses, seen by the model after the call; tool descriptions live in <see cref="ToolDescriptions"/>.</summary>
internal static class Messages
{
    public const string WorkerGoneHint = "ae_diagnose lists the bridges alive under bridge.connected; bind this client to another worker, or release the binding to use the desk.";

    public const string BridgeUnavailableHint =
        "Start After Effects with the Snail bridge installed in its Scripts/Startup folder and the preference " +
        "'Allow Scripts to Write Files and Access Network' switched on; the bridge dials this server within a few seconds of starting, " +
        "and ae_diagnose shows it once it has.";

    public const string TimeoutHint =
        "After Effects is busy on its main thread — a render, an import or a modal dialog — and the bridge runs commands only between " +
        "those. Finish or dismiss it in After Effects, or pass a longer timeoutSeconds for work that takes long.";

    public const string DroppedHint = "The bridge closed the connection; it reconnects on its own within a few seconds.";

    public const string MalformedHint = "Reinstall the bridge shipped with this server; ae_diagnose says whether the installed build is the bundled one.";

    public const string ExpiredHint = "The command reached After Effects after the wait ended and did not run; call again with a longer timeoutSeconds.";

    public const string StaleBridgeHint =
        "The bridge installed in After Effects is older than this server; reinstall it and check ae_diagnose, whose extension block names what the two sides disagree on.";

    public const string BadRequestHint = "Read details.accepted for the parameters the command reads, and send only those.";

    public const string BadValueHint = "The error names the parameter and what it takes; details carry the values to choose from when there are any.";

    public const string ScriptingNotAllowedNote =
        "After Effects refuses scripts network access and file writes until Preferences → Scripting & Expressions → " +
        "'Allow Scripts to Write Files and Access Network' is switched on; the bridge cannot switch it on itself.";

    public const string NoScriptsHome = "No user scripts folder of After Effects exists on this machine, so there is nowhere to install the bridge.";

    public const string NoScriptsHomeHint = "Start After Effects once: it creates its preferences folder, with the Scripts folder the bridge goes into, on first launch.";

    public const string StartAfterEffectsNote = "After Effects is not running; the bridge starts with it and dials this server a few seconds after launch.";

    public const string BridgeStartedNote = "After Effects accepted the loader; the bridge dials this server within a few seconds, and ae_diagnose shows it connected.";

    public static readonly IReadOnlyList<string> InstallSteps =
    [
        "In After Effects open Preferences → Scripting & Expressions and switch on 'Allow Scripts to Write Files and Access Network', once.",
        "Leave After Effects running, or start it: the bridge in its Startup folder dials this server on its own.",
        "Call ae_diagnose: afterEffects.reachable turns true once the bridge is connected.",
    ];

    public const string TimeoutOutOfRange = "timeoutSeconds is outside the 1 to 600 range this server accepts.";

    public const string TimeoutRangeHint = "Pass a value from 1 to 600.";

    public const string NotFoundHint = "ae_list_items shows the items with their ids; ae_item_info shows a composition's layers.";

    public const string AmbiguousHint = "Several items share the name; details.candidates lists them with ids, pass the id.";

    public const string ProjectDirtyHint = "Save with ae_project_save first, or pass discard: true when the changes may be lost.";

    public const string NoSuchPropertyHint = "details.available lists what is there; ae_property_tree shows the whole tree with match names, which work in every language.";

    public static string CaptureNotWritten(string path) => $"After Effects did not finish writing {path} within the wait.";

    public const string CaptureFailedHint = "After Effects wrote no picture; ae_diagnose says whether it is busy, and a composition with no layers or a time outside it gives an empty frame.";

    public const string UndoNotAsked = "After Effects could not be asked to undo.";

    public const string UndoNotAskedHint = "Undo runs as a script handed to After Effects from outside; ae_diagnose and ae_install_bridge say whether After Effects is running and reachable.";

    public const string NoSuchMenuCommandHint = "Menu names are in the language After Effects runs in; pass the command's numeric id, which is the same everywhere.";

    public static string UnknownSkill(string name) => $"There is no skill named '{name}'.";

    public static string KnownSkills(IEnumerable<string> names) => names.Any() ? $"ae_skills lists them: {string.Join(", ", names)}." : "This build ships no skills yet; every tool is always on.";

    public static string NoToolFor(string intent) => $"No tool of this server matches '{intent}'.";

    /// <summary>What a program asked the catalogue for and the commands that come closest to it.</summary>
    public static string NoSuchCommand(string name, IReadOnlyList<string> nearest) =>
        nearest.Count == 0
            ? $"No command of the catalogue is named '{name}'."
            : $"No command of the catalogue is named '{name}'; the nearest are {string.Join(", ", nearest)}.";

    public const string NoSuchCommandHint =
        "A program calls commands, not tools, and the words of the two are in opposite order: comp_create for ae_create_comp, layer_remove " +
        "for ae_remove_layers, layers_list for ae_list_layers. find_tool answers with each tool's command and its parameters, so look the " +
        "name up there rather than guessing it again.";

    public const string NoFileHint =
        "The open project has no file on disk yet, so there is nowhere to read or write beside it. Save it with ae_project_save, passing path " +
        "the first time, and call again.";

    public const string CannotImportHint =
        "After Effects will not read that file as that kind of footage: ae_import_file with as left out lets it decide, and a sequence needs the " +
        "folder of frames rather than one frame. A codec After Effects does not have is the other reason, and no parameter mends it.";

    public const string NoToolForHint =
        "Ask in English words for what the tool would do, not for an identifier: \"ease the keyframes\" rather than \"setTemporalEaseAtKey\". " +
        "ae_skills lists the areas the catalogue covers; ae_set_property reaches any property and ae_script any API.";

    public const string ExpressionErrorHint =
        "After Effects rejected the expression; it was not put on the property. Fix it from the message and call ae_set_expression again, " +
        "with check_only to try it first. Property names in expressions are in the application's language; match names are not accepted there.";

    public const string NoSuchSettingHint = "The answer lists the settings this item has; a match name from ae_property_tree works too.";

    public const string NotSupportedHint = "This After Effects is older than the feature; ae_diagnose tells its version, and ae_script may still reach the older API.";

    public const string CollectFailedHint = "Check that the folder is writable and the disk has room; what was copied before the failure stays in the folder.";

    public const string BusyHint = "After Effects is rendering; ae_render_queue shows the state. Wait for it, or stop it in the Render Queue panel.";

    public const string WorkersUnavailable = "No After Effects to launch: none is installed where the server looks, or the pool's maximum is 0.";

    public const string WorkersUnavailableHint = "Set Workers:Executable to the After Effects binary and Workers:Max above 0; ae_diagnose shows what the server found.";

    public const string PoolFull = "Every worker is taken and the pool is at its maximum.";

    public const string PoolFullHint = "ae_workers shows who holds them; release one, raise Workers:Max, or work on the desk.";

    public static string WorkerNotReady(string worker, string state) => $"The worker '{worker}' is {state} and cannot be leased.";

    public static string UnknownWorker(string worker) => $"No worker is named '{worker}'.";

    public static string WorkerHeld(string worker, string holder) => $"The worker '{worker}' is held by '{holder}'.";

    public const string WorkerHeldHint = "Lease without naming one and the pool gives you a free worker, or ae_stop_worker frees that one for everybody.";

    public const string UnknownWorkerHint = "ae_workers lists the workers alive; ae_lease with launch starts one under a new name.";

    public const string FarmUnavailable = "aerender was not found beside any installed After Effects, so the farm cannot render.";

    public const string FarmUnavailableHint = "ae_render renders inside After Effects instead; ae_diagnose shows which After Effects this machine has.";

    public static string UnknownJob(string id) => $"No render job has the id '{id}'.";

    public const string UnknownJobHint = "ae_farm_jobs lists the jobs this server knows; jobs are forgotten when the server restarts or after ae_farm_cleanup.";

    public static string JobNotActive(string id, string state) => $"Job '{id}' is {state}; only queued and running jobs can be cancelled.";

    public const string ExportFailedHint = "Look at After Effects: a dialog about fonts or unsupported properties may be waiting; the Essential Graphics panel shows what the template allows.";

    public const string HttpTokenMissing =
        "The HTTP server needs a token clients present: set SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN, or SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN_FILE to a file holding one, " +
        "or mint client keys with the 'clients add' command and give the server any token so it starts.";

    public const string ScopeHint = "Keys are minted with 'clients add <name> --scope read,write,delete,script,farm' on the server's machine; ae_diagnose shows which client this key names.";

    public static string NoSuchArea(string area) => $"There is no area '{area}'; the areas are {string.Join(", ", Application.Storage.VolumeAreas.All)}.";

    public static string PathOutsideArea(string path, string area) => $"'{path}' must stay inside the {area} area: relative, with forward slashes and no '..'.";

    public const string SameMachineUpload = "The server runs on this machine, so there is nothing to transfer: copy or save the file at this path, or import it from where it is with ae_import_file.";

    public const string SameMachineDownload = "The server runs on this machine, so the file is already here at this path.";

    public const string RunOnTheClient = "Run this command in the client's shell; the link inside it works until it expires and for this path only.";

    public static string DeletedFromThePage(int count, string size) => count == 0 ? "Nothing was deleted." : $"Deleted {count} {(count == 1 ? "entry" : "entries")}, {size} freed.";

    public static string CancelledFromThePage(string job) => $"Asked job {job} to stop.";

    public const string NothingSelected = "Nothing was selected to delete.";

    public const string NotOneName = "A name to delete is one entry of the folder shown, without slashes.";

    public const string FolderOutsideTheArea = "The folder named leaves its area; nothing was done.";

    public const string NotRendering = "ae_render_queue shows whether a render is running; start one with ae_render and wait false.";

    public const string NoViewerHint = "ae_open_comp opens a composition in a viewer; a layer or footage viewer opens from the interface.";

    public const string NoKeyframesHint = "ae_set_keyframes puts keys on the property first; ae_list_keyframes shows what is there.";

    public const string ProgramStoppedHint =
        "What the program sent before it stopped stands: read the commands list in the reply, then send a shorter program that carries on from there.";

    public const string ScriptingDisabled = "ae_script is switched off by configuration on this server.";

    public const string SequenceScriptDisabled = "A run_tools list may not contain the script command while ae_script is switched off.";

    public const string ScriptingDisabledHint =
        "Use the dedicated tools, or send many of them at once with run_tools; an administrator enables scripts with SNAIL_MCP_AFTEREFFECTS_SCRIPTING=allow.";

    /// <summary>The hint that fits an error of the bridge or of the link; null when there is nothing useful to add.</summary>
    public static string? HintFor(Domain.BridgeError error) => error.Type switch
    {
        Domain.BridgeError.UnavailableType => BridgeUnavailableHint,
        Domain.BridgeError.WorkerGoneType => WorkerGoneHint,
        Domain.BridgeError.TimeoutType => TimeoutHint,
        Domain.BridgeError.DroppedType => DroppedHint,
        Domain.BridgeError.MalformedType => MalformedHint,
        Domain.BridgeError.ExpiredType => ExpiredHint,
        "UnknownCommand" => StaleBridgeHint,
        "NoSuchCommand" => NoSuchCommandHint,
        "BadRequest" => error.Details is JsonObject details && details.ContainsKey("accepted") ? BadRequestHint : BadValueHint,
        "ScriptingDisabled" => ScriptingDisabledHint,
        "NotFound" or "NoComposition" or "NotAComposition" or "NotAFolder" or "NotAText" or "NotAShape" or "NotACamera" or "NotALight" or "NotFootage" => NotFoundHint,
        "CollectFailed" => CollectFailedHint,
        "Busy" => BusyHint,
        "NoViewer" => NoViewerHint,
        "NotRendering" => NotRendering,
        "Forbidden" => ScopeHint,
        "ExportFailed" => ExportFailedHint,
        "NoSuchSetting" => NoSuchSettingHint,
        "NotSupported" => NotSupportedHint,
        "Ambiguous" => AmbiguousHint,
        "ProjectDirty" => ProjectDirtyHint,
        "NoSuchProperty" => NoSuchPropertyHint,
        "NoSuchMenuCommand" => NoSuchMenuCommandHint,
        "CaptureFailed" => CaptureFailedHint,
        "ExpressionError" => ExpressionErrorHint,
        "NoKeyframes" => NoKeyframesHint,
        "NoFile" => NoFileHint,
        "CannotImport" => CannotImportHint,
        _ => null,
    };
}
