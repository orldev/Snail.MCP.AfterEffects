namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand Precompose = new("precompose", "compositing");

    public static readonly BridgeCommand CompTree = new("comp_tree", "compositing");

    public static readonly BridgeCommand FootageReplace = new("footage_replace", "compositing");

    public static readonly BridgeCommand FootageInterpret = new("footage_interpret", "compositing");

    public static readonly BridgeCommand ProxySet = new("proxy_set", "compositing");

    public static readonly BridgeCommand LayerSourceReplace = new("layer_source_replace", "compositing");

    public static readonly BridgeCommand ProjectFootage = new("project_footage", "compositing");

    public static readonly BridgeCommand ProjectCleanup = new("project_cleanup", "compositing");

    public static readonly BridgeCommand CollectFiles = new("collect_files", "compositing");
}
