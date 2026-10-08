namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>Tool description catalog — what the model reads before calling; a contract with the agent guarded by <c>ToolCatalogConventionTests</c>. One partial file per area of After Effects.</summary>
internal static partial class ToolDescriptions
{
    /// <summary>Parameter descriptions shared by several tools.</summary>
    public static partial class Parameters
    {
        public const string Item = "The item by its name as the project panel shows it, or by its id when names repeat.";

        public const string Comp = "The composition by name or id; when empty, the active one, else the one worked on last, else the only one in the project.";

        public const string Layer = "The layer by its name or its index in the composition, 1 at the top.";

        public const string Discard = "Throw away the open project's unsaved changes instead of refusing.";

        public const string SkillName = "Skill name as ae_skills lists it.";

        public const string Label = "Label colour as a number 0 to 16 as After Effects numbers them; 0 is none.";

        /// <summary>States <c>ToolLimits.MaxTimeoutSeconds</c>; a guard test keeps the number equal.</summary>
        public const string TimeoutSeconds = "Seconds to wait for After Effects, 1 to 600.";

        public const string Time = "Time in seconds within the composition; the composition's current time when empty.";

        public const string Color = "[r, g, b] with components 0 to 1, or #rrggbb.";

        public const string PropertyPath =
            "Path of the property, segments separated by /: match names as ae_property_tree lists them (ADBE Transform Group/ADBE Position), display " +
            "names in After Effects' language, the English names of the common ones (Transform/Position, Effects/<effect>/<parameter>, Masks/1/Mask Path, " +
            "Text/Source Text), or 1-based indices.";

        /// <summary>States <c>JavaScriptPrograms.MostCalls</c>; a guard test keeps the number equal.</summary>
        public const string ProgramCode =
            "JavaScript. Call commands as ae.<command>({...}) — the tool name without ae_, parameters in snake_case — read input, return a value, " +
            "log(...) to add a line. A failed command throws {type, message, details}. Up to 200 commands, no files, no network.";

        public const string Steps =
            "The steps as a JSON array of {\"command\": <bridge command>, \"params\": {...}}; commands and parameters are the tools' in snake_case " +
            "without the ae_ prefix (layer_add, property_set, comp_create).";

        public const string PropertyValue =
            "The new value in the property's shape: a number; [x, y] or [x, y, z]; a colour as [r, g, b, a] 0 to 1 or #rrggbb; true/false for a checkbox; " +
            "a string or an object {text, font, font_size, fill_color, stroke_color, stroke_width, justification, tracking, leading} for text; " +
            "{vertices, in_tangents, out_tangents, closed} for a mask or shape path; {comment, duration} for a marker.";

        /// <summary>Names every entry of <c>LayerKinds.All</c>; a guard test keeps them equal.</summary>
        public const string LayerKind = "solid, null, adjustment, text, shape, camera, light, or item (footage or a composition from the project).";

        /// <summary>Names every entry of <c>LightTypes.All</c>; a guard test keeps them equal.</summary>
        public const string LightType = "For kind light: PARALLEL, SPOT, POINT or AMBIENT.";

        /// <summary>Names every entry of <c>BlendingModes.All</c>; a guard test keeps them equal.</summary>
        public const string BlendingMode =
            "NORMAL, DISSOLVE, DANCING_DISSOLVE, DARKEN, MULTIPLY, COLOR_BURN, CLASSIC_COLOR_BURN, LINEAR_BURN, DARKER_COLOR, ADD, LIGHTEN, SCREEN, " +
            "COLOR_DODGE, CLASSIC_COLOR_DODGE, LINEAR_DODGE, LIGHTER_COLOR, OVERLAY, SOFT_LIGHT, HARD_LIGHT, LINEAR_LIGHT, VIVID_LIGHT, PIN_LIGHT, HARD_MIX, " +
            "DIFFERENCE, CLASSIC_DIFFERENCE, EXCLUSION, SUBTRACT, DIVIDE, HUE, SATURATION, COLOR, LUMINOSITY, STENCIL_ALPHA, STENCIL_LUMA, SILHOUETTE_ALPHA, " +
            "SILHOUETTE_LUMA, ALPHA_ADD or LUMINESCENT_PREMUL.";

        /// <summary>Names every entry of <c>TrackMatteTypes.All</c>; a guard test keeps them equal.</summary>
        public const string TrackMatteType = "ALPHA (the default when a matte layer is named), ALPHA_INVERTED, LUMA, LUMA_INVERTED, or NONE to remove the matte.";
    }
}
