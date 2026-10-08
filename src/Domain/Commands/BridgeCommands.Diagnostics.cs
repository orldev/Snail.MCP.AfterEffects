namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand Ping = new("ping", "diagnostics");

    public static readonly BridgeCommand AppInfo = new("app_info", "diagnostics");

    public static readonly BridgeCommand Script = new("script", "diagnostics");
}
