namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand ProjectSettings = new("project_settings", "settings");

    public static readonly BridgeCommand ExpressionsReplace = new("expressions_replace", "settings");

    public static readonly BridgeCommand ProjectFonts = new("project_fonts", "settings");

    public static readonly BridgeCommand FontReplace = new("font_replace", "settings");

    public static readonly BridgeCommand ImportPlaceholder = new("import_placeholder", "settings");

    public static readonly BridgeCommand Purge = new("purge", "settings");

    public static readonly BridgeCommand AppPerformance = new("app_performance", "settings");
}
