namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand TextInfo = new("text_info", "text");

    public static readonly BridgeCommand TextStyle = new("text_style", "text");

    public static readonly BridgeCommand TextLayout = new("text_layout", "text");

    public static readonly BridgeCommand TextFonts = new("text_fonts", "text");

    public static readonly BridgeCommand TextAnimatorAdd = new("text_animator_add", "text");

    public static readonly BridgeCommand TextAnimatorUpdate = new("text_animator_update", "text");

    public static readonly BridgeCommand TextAnimatorRemove = new("text_animator_remove", "text");

    public static readonly BridgeCommand TextPath = new("text_path", "text");
}
