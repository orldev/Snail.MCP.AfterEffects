namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Workers
    {
        public const string List =
            "The pool of After Effects workers: each one's name, state (starting, ready, leased, stopping, gone), origin (launched by this server, " +
            "or adopted when a named instance somebody else started connected), pid, uptime, who holds it and how long it sat idle; with the " +
            "pool's limits and which worker this client is bound to. The desk is not a worker.";

        public const string Lease =
            "Take an After Effects of your own: binds this client to a free worker, to the one named, or to a fresh one the server launches when " +
            "the pool has room, waiting for it to come up; from then on every command of this client goes there. With project, a copy of that " +
            "file is opened in it. release gives the worker back and commands return to the desk.";

        public const string Stop = "Quit a worker, dropping its project unsaved, and free whoever held it; the pool launches another when it is below its minimum.";
    }

    public static partial class Parameters
    {
        public const string WorkerName = "The worker by name, such as w1, as ae_workers lists them.";
    }
}
