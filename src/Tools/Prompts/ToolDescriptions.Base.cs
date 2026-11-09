namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
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
