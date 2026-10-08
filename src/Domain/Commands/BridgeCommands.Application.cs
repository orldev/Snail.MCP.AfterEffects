namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand Sequence = new("sequence", "sequence");

    public static readonly BridgeCommand MenuCommand = new("menu_command", "application");
}
