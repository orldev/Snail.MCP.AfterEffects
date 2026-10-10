using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.ThreeD;

/// <summary>Cameras, lights, materials, geometry and the renderer: the 3D skill.</summary>
[Skill(Skills.ThreeD, SkillDescriptions.ThreeD)]
[McpServerToolType]
internal sealed class ThreeDTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_3d_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.Info)]
    [Sends("threed_info")]
    public Task<CallToolResult> InfoAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("One layer to describe in full; the composition's 3D overview when empty.")] string? layer = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ThreeDInfo, new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time), cancellationToken);

    [McpServerTool(Name = "ae_set_camera", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.SetCamera)]
    [Sends("camera_set")]
    public Task<CallToolResult> SetCameraAsync(
        [Description("The camera layer by name or index.")] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the values as keyframes at this time.")] double? time = null,
        [Description("[x, y, z] position of the camera in composition pixels.")] double[]? position = null,
        [Description(ToolDescriptions.Parameters.PointOfInterest)] double[]? pointOfInterest = null,
        [Description(ToolDescriptions.Parameters.LookAt)] string? lookAt = null,
        [Description(ToolDescriptions.Parameters.TwoNode)] bool? twoNode = null,
        [Description("Zoom in pixels: the distance from the camera to the plane that fills the frame.")] double? zoom = null,
        [Description("Focal length in millimetres on a 36 mm film back; sets zoom from the composition width.")] double? focalLengthMm = null,
        [Description("Switch depth of field on or off.")] bool? depthOfField = null,
        [Description("Focus distance in pixels.")] double? focusDistance = null,
        [Description("Aperture in pixels; larger blurs more.")] double? aperture = null,
        [Description("Blur level in percent.")] double? blurLevel = null,
        [Description("Iris shape as an option number, 1 fast rectangle, then triangle, square, pentagon, hexagon, heptagon, octagon, nonagon, decagon.")] int? irisShape = null,
        [Description("Iris rotation in degrees.")] double? irisRotation = null,
        [Description("Iris roundness in percent.")] double? irisRoundness = null,
        [Description("Iris aspect ratio.")] double? irisAspectRatio = null,
        [Description("Iris diffraction fringe.")] double? irisDiffractionFringe = null,
        [Description("Highlight gain.")] double? highlightGain = null,
        [Description("Highlight threshold.")] double? highlightThreshold = null,
        [Description("Highlight saturation.")] double? highlightSaturation = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.CameraSet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("position", position).With("point_of_interest", pointOfInterest)
                .WithReference("look_at", lookAt).With("two_node", twoNode).With("zoom", zoom).With("focal_length_mm", focalLengthMm).With("depth_of_field", depthOfField)
                .With("focus_distance", focusDistance).With("aperture", aperture).With("blur_level", blurLevel).With("iris_shape", irisShape).With("iris_rotation", irisRotation)
                .With("iris_roundness", irisRoundness).With("iris_aspect_ratio", irisAspectRatio).With("iris_diffraction_fringe", irisDiffractionFringe)
                .With("highlight_gain", highlightGain).With("highlight_threshold", highlightThreshold).With("highlight_saturation", highlightSaturation),
            cancellationToken);

    [McpServerTool(Name = "ae_set_light", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.SetLight)]
    [Sends("light_set")]
    public Task<CallToolResult> SetLightAsync(
        [Description("The light layer by name or index.")] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the values as keyframes at this time.")] double? time = null,
        [Description("PARALLEL, SPOT, POINT or AMBIENT.")] string? type = null,
        [Description("[x, y, z] position of the light in composition pixels.")] double[]? position = null,
        [Description(ToolDescriptions.Parameters.PointOfInterest)] double[]? pointOfInterest = null,
        [Description(ToolDescriptions.Parameters.LookAt)] string? lookAt = null,
        [Description(ToolDescriptions.Parameters.TwoNode)] bool? twoNode = null,
        [Description("Intensity in percent.")] double? intensity = null,
        [Description(ToolDescriptions.Parameters.Color)] JsonNode? color = null,
        [Description("Cone angle in degrees, for spot lights.")] double? coneAngle = null,
        [Description("Cone feather in percent, for spot lights.")] double? coneFeather = null,
        [Description(ToolDescriptions.Parameters.LightFalloff)] string? falloff = null,
        [Description("Where the falloff starts, in pixels.")] double? falloffStart = null,
        [Description("Over how many pixels the light falls off.")] double? falloffDistance = null,
        [Description("Cast shadows from this light.")] bool? castsShadows = null,
        [Description("Shadow darkness in percent.")] double? shadowDarkness = null,
        [Description("Shadow diffusion in pixels.")] double? shadowDiffusion = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.LightSet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("type", type).With("position", position)
                .With("point_of_interest", pointOfInterest).WithReference("look_at", lookAt).With("two_node", twoNode).With("intensity", intensity).With("color", color)
                .With("cone_angle", coneAngle).With("cone_feather", coneFeather).With("falloff", falloff).With("falloff_start", falloffStart).With("falloff_distance", falloffDistance)
                .With("casts_shadows", castsShadows).With("shadow_darkness", shadowDarkness).With("shadow_diffusion", shadowDiffusion),
            cancellationToken);

    [McpServerTool(Name = "ae_set_material", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.SetMaterial)]
    [Sends("material_set")]
    public Task<CallToolResult> SetMaterialAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the values as keyframes at this time.")] double? time = null,
        [Description(ToolDescriptions.Parameters.ShadowMode)] string? castsShadows = null,
        [Description("Light transmission in percent: how much light passes through to cast coloured shadows.")] double? lightTransmission = null,
        [Description(ToolDescriptions.Parameters.ShadowMode)] string? acceptsShadows = null,
        [Description("Let lights affect the layer.")] bool? acceptsLights = null,
        [Description("Show the layer in reflections, where the renderer has them.")] bool? appearsInReflections = null,
        [Description("Ambient in percent.")] double? ambient = null,
        [Description("Diffuse in percent.")] double? diffuse = null,
        [Description("Specular intensity in percent.")] double? specularIntensity = null,
        [Description("Specular shininess in percent.")] double? specularShininess = null,
        [Description("Metal in percent: how much the specular takes the layer's colour.")] double? metal = null,
        [Description("Reflection intensity in percent.")] double? reflectionIntensity = null,
        [Description("Reflection sharpness in percent.")] double? reflectionSharpness = null,
        [Description("Reflection rolloff in percent.")] double? reflectionRolloff = null,
        [Description("Transparency in percent.")] double? transparency = null,
        [Description("Transparency rolloff in percent.")] double? transparencyRolloff = null,
        [Description("Index of refraction.")] double? indexOfRefraction = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.MaterialSet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("casts_shadows", castsShadows)
                .With("light_transmission", lightTransmission).With("accepts_shadows", acceptsShadows).With("accepts_lights", acceptsLights)
                .With("appears_in_reflections", appearsInReflections).With("ambient", ambient).With("diffuse", diffuse).With("specular_intensity", specularIntensity)
                .With("specular_shininess", specularShininess).With("metal", metal).With("reflection_intensity", reflectionIntensity)
                .With("reflection_sharpness", reflectionSharpness).With("reflection_rolloff", reflectionRolloff).With("transparency", transparency)
                .With("transparency_rolloff", transparencyRolloff).With("index_of_refraction", indexOfRefraction),
            cancellationToken);

    [McpServerTool(Name = "ae_set_geometry", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.SetGeometry)]
    [Sends("geometry_set")]
    public Task<CallToolResult> SetGeometryAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the values as keyframes at this time.")] double? time = null,
        [Description("Bevel style as an option number: 1 none, 2 angular, 3 concave, 4 convex.")] int? bevelStyle = null,
        [Description("Bevel direction as an option number, where the renderer has it.")] int? bevelDirection = null,
        [Description("Bevel depth in pixels.")] double? bevelDepth = null,
        [Description("Hole bevel depth in percent.")] double? holeBevelDepth = null,
        [Description("Extrusion depth in pixels.")] double? extrusionDepth = null,
        [Description("Curvature in percent, for curved layers.")] double? curvature = null,
        [Description("Segments of a curved layer.")] int? segments = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.GeometrySet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("time", time).With("bevel_style", bevelStyle).With("bevel_direction", bevelDirection)
                .With("bevel_depth", bevelDepth).With("hole_bevel_depth", holeBevelDepth).With("extrusion_depth", extrusionDepth).With("curvature", curvature).With("segments", segments),
            cancellationToken);

    [McpServerTool(Name = "ae_set_renderer", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.ThreeD.SetRenderer)]
    [Sends("renderer_set")]
    public Task<CallToolResult> SetRendererAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The renderer by its name or match name (ADBE Advanced 3d, ADBE Ernst for Cinema 4D, ADBE Standard 3d for Classic 3D), or a word of it; empty only reports.")] string? renderer = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.RendererSet, new JsonObject().WithReference("comp", comp).With("renderer", renderer), cancellationToken);
}
