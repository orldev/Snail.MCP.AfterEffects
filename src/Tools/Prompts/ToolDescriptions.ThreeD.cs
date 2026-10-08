namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class ThreeD
    {
        public const string Info =
            "The 3D state of a composition (renderer, cameras, lights, how many 3D layers) or of one layer: its transform, point of interest, camera " +
            "lens and depth of field, light settings, material options, geometry options, and the renderers the composition can switch to.";

        public const string SetCamera =
            "Set a camera: position and point of interest (or look_at a layer), one or two node, zoom or focal length in millimetres, depth of field with " +
            "focus distance, aperture and blur level, and the iris and highlight settings; as keyframes when time is given.";

        public const string SetLight =
            "Set a light: type, position and point of interest (or look_at a layer), intensity, colour, cone angle and feather for spots, falloff with its " +
            "start and distance, and shadows with darkness and diffusion; as keyframes when time is given.";

        public const string SetMaterial =
            "Set the material options of a 3D layer (switching it to 3D if it is not): casting and accepting shadows, accepting lights, ambient, diffuse, " +
            "specular intensity and shininess, metal, and the reflection, transparency and refraction settings of renderers that have them.";

        public const string SetGeometry =
            "Set the geometry options of a 3D layer under a renderer that extrudes (Advanced 3D, Cinema 4D): extrusion depth, bevel style, direction and " +
            "depth, hole bevel depth, curvature and segments. Under the Classic 3D renderer the answer says so and names the renderers to switch to.";

        public const string SetRenderer =
            "The renderer of a composition: Classic 3D, Advanced 3D or Cinema 4D by name or a word of it; with no renderer the answer only reports the " +
            "current one and the choices. The renderer decides which material and geometry options exist.";
    }

    public static partial class Parameters
    {
        public const string PointOfInterest = "[x, y, z] the camera or light aims at, in composition pixels.";

        public const string LookAt = "A layer whose position becomes the point of interest.";

        public const string TwoNode = "Aim at the point of interest (two node) or not (one node, free rotation).";

        public const string LightFalloff = "none, smooth, or inverse_square_clamped.";

        public const string ShadowMode = "on, off, or only (cast or accept shadows without rendering the layer itself).";
    }
}
