namespace Snail.MCP.AfterEffects.Domain;

/// <summary>The closed lists After Effects names things by, as the tools spell them; the descriptions name every entry and guard tests keep them equal.</summary>
internal static class LayerKinds
{
    public static readonly IReadOnlyList<string> All = ["solid", "null", "adjustment", "text", "shape", "camera", "light", "item"];
}

internal static class ItemKinds
{
    public static readonly IReadOnlyList<string> All = ["comp", "footage", "folder"];
}

internal static class BlendingModes
{
    public static readonly IReadOnlyList<string> All =
    [
        "NORMAL", "DISSOLVE", "DANCING_DISSOLVE", "DARKEN", "MULTIPLY", "COLOR_BURN", "CLASSIC_COLOR_BURN", "LINEAR_BURN", "DARKER_COLOR", "ADD",
        "LIGHTEN", "SCREEN", "COLOR_DODGE", "CLASSIC_COLOR_DODGE", "LINEAR_DODGE", "LIGHTER_COLOR", "OVERLAY", "SOFT_LIGHT", "HARD_LIGHT",
        "LINEAR_LIGHT", "VIVID_LIGHT", "PIN_LIGHT", "HARD_MIX", "DIFFERENCE", "CLASSIC_DIFFERENCE", "EXCLUSION", "SUBTRACT", "DIVIDE", "HUE",
        "SATURATION", "COLOR", "LUMINOSITY", "STENCIL_ALPHA", "STENCIL_LUMA", "SILHOUETTE_ALPHA", "SILHOUETTE_LUMA", "ALPHA_ADD", "LUMINESCENT_PREMUL",
    ];
}

internal static class LightTypes
{
    public static readonly IReadOnlyList<string> All = ["PARALLEL", "SPOT", "POINT", "AMBIENT"];
}

internal static class TrackMatteTypes
{
    public static readonly IReadOnlyList<string> All = ["ALPHA", "ALPHA_INVERTED", "LUMA", "LUMA_INVERTED", "NONE"];
}

internal static class EasingPresets
{
    public static readonly IReadOnlyList<string> All = ["easy_ease", "ease_in", "ease_out", "linear", "hold"];
}

internal static class Interpolations
{
    public static readonly IReadOnlyList<string> All = ["LINEAR", "BEZIER", "HOLD"];
}

internal static class TextAnimatorProperties
{
    public static readonly IReadOnlyList<string> All =
    [
        "anchor_point", "position", "scale", "skew", "skew_axis", "rotation", "x_rotation", "y_rotation", "opacity", "fill_color", "fill_hue", "fill_saturation",
        "fill_brightness", "fill_opacity", "stroke_color", "stroke_hue", "stroke_saturation", "stroke_brightness", "stroke_opacity", "stroke_width", "tracking",
        "tracking_type", "line_anchor", "line_spacing", "character_offset", "character_value", "blur",
    ];
}

internal static class TextSelectorSettings
{
    public static readonly IReadOnlyList<string> All =
    [
        "type", "index", "name", "start", "end", "offset", "units", "based_on", "mode", "amount", "shape", "smoothness", "ease_high", "ease_low", "randomize", "seed",
        "max", "min", "frequency", "correlation", "temporal_phase", "spatial_phase", "lock_dimensions",
    ];
}

internal static class ShapeKinds
{
    public static readonly IReadOnlyList<string> All =
    [
        "group", "rect", "ellipse", "star", "path", "fill", "stroke", "gradient_fill", "gradient_stroke", "trim", "repeater", "merge", "offset", "round_corners",
        "pucker_bloat", "twist", "wiggle_paths", "wiggle_transform", "zigzag",
    ];
}

internal static class MaskModes
{
    public static readonly IReadOnlyList<string> All = ["NONE", "ADD", "SUBTRACT", "INTERSECT", "LIGHTEN", "DARKEN", "DIFFERENCE"];
}

internal static class LayerStyles
{
    public static readonly IReadOnlyList<string> All = ["drop_shadow", "inner_shadow", "outer_glow", "inner_glow", "bevel_emboss", "satin", "color_overlay", "gradient_overlay", "stroke"];
}

internal static class LightFalloffs
{
    public static readonly IReadOnlyList<string> All = ["none", "smooth", "inverse_square_clamped"];
}

internal static class ShadowModes
{
    public static readonly IReadOnlyList<string> All = ["on", "off", "only"];
}

internal static class AlphaModes
{
    public static readonly IReadOnlyList<string> All = ["IGNORE", "STRAIGHT", "PREMULTIPLIED"];
}

internal static class FieldSeparations
{
    public static readonly IReadOnlyList<string> All = ["OFF", "UPPER_FIELD_FIRST", "LOWER_FIELD_FIRST"];
}

internal static class PostRenderActions
{
    public static readonly IReadOnlyList<string> All = ["NONE", "IMPORT", "IMPORT_AND_REPLACE_USAGE", "SET_PROXY"];
}

internal static class TimeDisplays
{
    public static readonly IReadOnlyList<string> All = ["TIMECODE", "FRAMES"];
}

internal static class GpuAccelerations
{
    public static readonly IReadOnlyList<string> All = ["METAL", "CUDA", "OPENCL", "DIRECTX", "VULKAN", "SOFTWARE"];
}

internal static class PurgeTargets
{
    public static readonly IReadOnlyList<string> All = ["ALL_CACHES", "ALL_MEMORY_CACHES", "IMAGE_CACHES", "SNAPSHOT_CACHES", "UNDO_CACHES"];
}

internal static class ViewerChannels
{
    public static readonly IReadOnlyList<string> All =
    [
        "CHANNEL_RGB", "CHANNEL_RGB_STRAIGHT", "CHANNEL_RED", "CHANNEL_GREEN", "CHANNEL_BLUE", "CHANNEL_ALPHA", "CHANNEL_ALPHA_BOUNDARY", "CHANNEL_ALPHA_OVERLAY",
        "CHANNEL_RED_COLORIZE", "CHANNEL_GREEN_COLORIZE", "CHANNEL_BLUE_COLORIZE",
    ];
}
