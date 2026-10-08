namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand ThreeDInfo = new("threed_info", "threed");

    public static readonly BridgeCommand CameraSet = new("camera_set", "threed");

    public static readonly BridgeCommand LightSet = new("light_set", "threed");

    public static readonly BridgeCommand MaterialSet = new("material_set", "threed");

    public static readonly BridgeCommand GeometrySet = new("geometry_set", "threed");

    public static readonly BridgeCommand RendererSet = new("renderer_set", "threed");
}
