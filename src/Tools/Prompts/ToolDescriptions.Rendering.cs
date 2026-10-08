namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Rendering
    {
        public const string Queue =
            "The render queue: every item with its composition, status, span, render flag and output modules (file, format and settings, " +
            "post-render action), whether After Effects is rendering, and whether Media Encoder can take the queue.";

        public const string Templates =
            "The render settings templates (Best Settings, Draft Settings and the user's own) and output module templates (Lossless, H.264, PNG " +
            "Sequence and the rest) this After Effects has; their names are what ae_add_render takes. With an empty queue a composition is " +
            "needed to read them.";

        public const string Add =
            "Put a composition in the render queue: render settings and output module templates by name, the output file (or a folder, which " +
            "takes the composition's name and the template's extension), the span as start and end or the work area, skip frames, a post-render " +
            "action, and any render or output settings by their names. The item is queued and waits for ae_render.";

        public const string Update =
            "Change an item of the render queue that has not rendered yet: templates, output file, span, skip frames, render flag, post-render " +
            "action, render or output settings.";

        public const string Remove = "Remove items from the render queue: one by index, all of them, or those with a status such as DONE.";

        public const string Render =
            "Render the queued items (or only the ones named) inside After Effects. By default the tool waits: After Effects is busy until the render " +
            "ends, so give a timeout the render fits in. With wait false the render starts and the call returns; ae_render_queue shows the progress " +
            "and ae_render_control stops or pauses it. The answer is the queue with each output file and its size. The farm renders through aerender " +
            "while After Effects stays free.";

        public const string Control =
            "Stop, pause or resume a render that ae_render started without waiting, switch the finished notification, or bring the Render Queue to " +
            "the front. After Effects answers the bridge only between frames it yields on, so a render in the application usually ends before " +
            "stop or pause reach it; a render that must be cancellable goes through the farm (ae_farm_submit, ae_farm_cancel).";

        public const string SaveTemplate =
            "Save a queue item's render settings or output module as a named template, so later ae_add_render calls can name it instead of " +
            "listing every setting.";

        public const string WatchFolder =
            "Turn this After Effects into a watch folder render node, as File > Watch Folder does, or stop or pause it. While it watches, the " +
            "application renders what arrives in the folder and answers little else; ae_render_queue and ae_diagnose still work.";

        public const string InAme =
            "Send the render queue to Adobe Media Encoder, which renders in its own process and leaves After Effects free; with render_immediately " +
            "it starts at once. Needs Media Encoder of the same version installed.";
    }

    public static partial class Parameters
    {
        public const string RenderItem = "Index of the item in the render queue, 1 first, as ae_render_queue lists them.";

        public const string PostRenderAction = "NONE, IMPORT, IMPORT_AND_REPLACE_USAGE or SET_PROXY.";

        public const string RenderSettings =
            "Render settings by name as the Render Settings dialog shows them, such as Quality, Resolution, Motion Blur, Frame Rate; ae_render_queue " +
            "with settings lists the settable ones.";

        public const string OutputSettings =
            "Output module settings by name, such as Format, Channels, Depth, Color, Video Output, Audio Output; ae_render_queue with settings lists the settable ones.";
    }
}
