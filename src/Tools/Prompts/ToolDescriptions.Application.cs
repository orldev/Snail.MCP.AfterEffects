namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Application
    {
        public const string Undo =
            "Undo the last steps in After Effects; every tool call is one step, run_tools is one step for its whole list. The answer says the undo was " +
            "asked for; ae_list_layers or ae_project_info show the result.";

        public const string Redo = "Redo steps undone with ae_undo; the answer says it was asked for.";

        public const string MenuCommand =
            "Run any command of After Effects' menus, by its name in After Effects' language or by its numeric id, on the current " +
            "selection: ae_select_layers chooses the layers first. For what no tool covers, such as pre-composing or fitting a layer to the composition. " +
            "Never Undo or Redo this way: use ae_undo and ae_redo, which run them safely.";

        public const string RunTools =
            "Run a list of bridge commands in one call and one undo step: each step names a command — the tool name without ae_, parameters in " +
            "snake_case as the bridge reads them, e.g. {\"command\": \"layer_add\", \"params\": {\"kind\": \"text\", \"text\": \"Hi\"}} — and gets its " +
            "own result or error. A build of twenty layers costs one round trip instead of twenty.";

        public const string Program =
            "Run a small JavaScript program here on the server that calls the bridge commands itself — ae.layer_add({...}), ae.property_set({...}) " +
            "— with loops and the results in hand, so a build that decides as it goes costs one call and one reply. Only what the program " +
            "returns and logs comes back. The program reaches the commands only: no files, no network, no After Effects API beyond them.";

        public const string Script =
            "Run ExtendScript inside After Effects for what no tool reaches. A script skips the tools' validation and comes back as text only; " +
            "prefer the tools and run_tools, and use this for the API they do not cover. Do not call app.executeCommand for Undo or Redo in a script: " +
            "it stalls the bridge; ae_undo and ae_redo exist for that.";
    }
}
