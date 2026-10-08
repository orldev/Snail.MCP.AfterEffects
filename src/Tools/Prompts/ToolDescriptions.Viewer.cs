namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Viewer
    {
        public const string Info =
            "The active viewer: composition, layer or footage viewer, whether it is maximized, its views with the active one, and each view's " +
            "channels, checkerboards, exposure, fast preview mode, rulers, guides and zoom.";

        public const string Set =
            "Change a view of the active viewer: the channel shown, transparency checkerboards, exposure in stops, fast preview mode, rulers, " +
            "guides visibility, snapping and locking, zoom; maximize the viewer or make the view active.";

    }

    public static partial class Parameters
    {
        public const string View = "Which view of the viewer, 1 first; the active view when empty.";

        public const string Channel =
            "CHANNEL_RGB, CHANNEL_RGB_STRAIGHT, CHANNEL_RED, CHANNEL_GREEN, CHANNEL_BLUE, CHANNEL_ALPHA, CHANNEL_ALPHA_BOUNDARY, CHANNEL_ALPHA_OVERLAY, " +
            "CHANNEL_RED_COLORIZE, CHANNEL_GREEN_COLORIZE or CHANNEL_BLUE_COLORIZE.";
    }
}
