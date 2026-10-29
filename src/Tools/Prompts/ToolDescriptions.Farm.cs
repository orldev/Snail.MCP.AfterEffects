namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Farm
    {
        public const string Submit =
            "Queue a render of a composition on the farm: aerender renders the saved project in its own engine, so After Effects stays free and " +
            "the job runs on after this call returns. The open project is saved first unless save is false; a project path renders that file " +
            "instead. Frames, templates and settings are aerender's own. Answers when the render ends within `wait` seconds, with the farm's " +
            "account of it; a longer render answers with the job running — read its resource (job://aftereffects/render/<id>) for frames " +
            "rendered, the last frame and the log, or wait on it.";

        public const string Jobs =
            "The farm's jobs, newest first: state (queued, running, done, failed, cancelled), frames rendered against the total " +
            "when the span is known, the last frame and timecode, attempts and retries left, the error if any, and the tail of the log. " +
            "One render's progress is its resource, job://aftereffects/render/<id>.";

        public const string Cancel = "Stop a queued or running job; a running render is killed and its partial output stays on disk.";

        public const string Retry = "Queue a finished, failed or cancelled job again as a new job with the same request.";

        public const string Cleanup = "Forget finished jobs older than a number of minutes, and delete their logs; 0 forgets every finished job.";
    }

    public static partial class Parameters
    {
        public const string JobId = "The farm's job id, as ae_farm_jobs lists it and the result of ae_farm_submit carries it.";

        public const string Wait = "Seconds to wait for the render to end before answering with it still running; 60 by default, 0 answers at once with the job to read or wait on.";

        public const string AerenderSettings = "As aerender takes them, an object of names to values; the After Effects guide to render and output settings names them.";
    }
}
