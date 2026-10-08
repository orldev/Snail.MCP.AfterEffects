namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Skills
    {
        public const string List = "The skills this server can load — groups of tools for animation, text, shapes, effects, 3D, compositing, rendering — with which are loaded and the tools each brings.";

        public const string Enable = "Load a skill: its tools join the tool list at once, and the client is told the list changed. Skills stay loaded until ae_disable_skill, unless the server unloads idle ones.";

        public const string Disable = "Unload a skill, taking its tools out of the list.";
    }

    public static class Discovery
    {
        public const string FindTool =
            "Find the tool for a task across every skill, loaded or not, by saying what you want in English words. Each match carries the tool's " +
            "command and parameters as a program sends them (snake_case; the tool takes the same names in camelCase), which is enough to call it " +
            "through run_program without loading anything; pass enable to load the skills " +
            "the matches live in so the tools can be called by name. Ask here before concluding that something has no tool.";
    }

    public static class Diagnostics
    {
        public const string Diagnose =
            "Health of this server and of the link into After Effects: the server version, whether After Effects has connected, the port the " +
            "bridge dials, where the token comes from, the project folder and the data directory. Call it first when a tool cannot reach After Effects.";

        public const string InstallBridge =
            "Install the bridge this server ships into After Effects' user scripts folder (Startup runs it on every launch) and, when After Effects " +
            "is running, start it there at once; After Effects then dials this server within a few seconds and ae_diagnose reports it. Safe to repeat: " +
            "it replaces an older build. Needs 'Allow Scripts to Write Files and Access Network' switched on in Preferences → Scripting & Expressions.";
    }
}
