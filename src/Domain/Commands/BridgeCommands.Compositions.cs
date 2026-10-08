namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand CompCreate = new("comp_create", "compositions");

    public static readonly BridgeCommand CompUpdate = new("comp_update", "compositions");

    public static readonly BridgeCommand CompDuplicate = new("comp_duplicate", "compositions");

    public static readonly BridgeCommand CompOpen = new("comp_open", "compositions");
}
