namespace Snail.MCP.AfterEffects.Tools.Prompts;

internal static partial class ToolDescriptions
{
    public static class Settings
    {
        public const string Project =
            "The project settings as Project Settings shows them, read and set in one call: time display (timecode or frames, the start frame), " +
            "colour depth, linear blending, working space and gamma, colour management and OCIO config, LUT interpolation, the expression engine, " +
            "GPU acceleration (with what the machine offers), transparency grid thumbnails, the active tool. With colorProfiles the answer lists " +
            "the colour profiles a working space can be.";

        public const string ReplaceExpressions =
            "Replace text in every expression of the project, as After Effects does when it renames something: a layer name, a property name, a " +
            "comp name that expressions refer to.";

        public const string Fonts =
            "The fonts the project uses, each with the layers that use it, and the fonts that are missing or substituted on this machine.";

        public const string ReplaceFont = "Replace one font with another across the project, by PostScript names as ae_text_fonts gives them.";

        public const string Placeholder =
            "Add a placeholder footage item of a size, frame rate and duration, to lay out a composition before the footage exists; ae_replace_footage " +
            "swaps the file in later.";

        public const string Purge = "Purge After Effects' caches: all, memory, images, snapshots or undo, as the Edit > Purge menu offers.";

        public const string Performance =
            "Memory and multi-frame rendering of After Effects: the image cache and total memory percentages, and multi-frame rendering on or off with " +
            "a CPU ceiling; the preferences the Performance pane holds.";

        public const string SystemInfo =
            "The machine After Effects runs on and the application itself: machine and user name, operating system and version, After Effects' " +
            "version, build and language, memory in use, whether it is a render engine or a beta, the rendering and crash-saving flags, where " +
            "its preferences live, and environment variables by name.";

        public const string SetApplication =
            "Application-wide switches: rendering of previews off while scripts work, an error on missing footage frames, saving the project on a " +
            "crash, writing the preferences on quit.";

        public const string ReadSwatches = "Read an Adobe swatch file (.ase or .aco) through After Effects: each swatch's name, colour space and values.";

        public const string SystemCall =
            "Run a shell command on After Effects' machine through After Effects and return its output; it waits for the command to finish. " +
            "Script-scope work: a key without that scope is refused.";

        public const string Preference =
            "Read one After Effects preference by section and key, as the preferences file spells them (ae_system_info says where it is), " +
            "read as a string, long, float or bool.";

        public const string SetPreference =
            "Write, remove, reload or save to disk After Effects' preferences by section and key; a value takes effect when the preference " +
            "is read next, some only after a restart. Wrong keys create new entries, so read with ae_preference first.";

        public const string ScriptSetting =
            "Read a setting scripts keep between sessions in After Effects, by section and key: the key-value store of app.settings.";

        public const string SaveScriptSetting = "Keep a text under a section and key in After Effects' script settings, where it survives restarts.";

        public const string Quit =
            "Quit After Effects: with unsaved changes it saves first or drops them as save says, and refuses when save is not given. The bridge " +
            "returns once it is running again.";
    }

    public static partial class Parameters
    {
        public const string TimeDisplay = "TIMECODE or FRAMES.";

        public const string GpuAccel = "METAL, CUDA, OPENCL, DIRECTX, VULKAN or SOFTWARE; the answer lists what this machine offers.";

        public const string PurgeTarget = "ALL_CACHES, ALL_MEMORY_CACHES, IMAGE_CACHES, SNAPSHOT_CACHES or UNDO_CACHES.";

        public const string PreferenceSection = "The section in the preferences file, such as \"Main Pref Section v2\".";

        public const string PreferenceKey = "The key within the section, such as \"Pref_SCRIPTING_FILE_NETWORK_SECURITY\".";

        public const string PreferenceKind = "How the value is read or written: string, long, float or bool.";

        public const string PreferenceFile =
            "Which preferences file: MACHINE_INDEPENDENT (the default), MACHINE_SPECIFIC, MACHINE_INDEPENDENT_RENDER, MACHINE_INDEPENDENT_OUTPUT, " +
            "MACHINE_INDEPENDENT_COMPOSITION, MACHINE_SPECIFIC_TEXT, MACHINE_SPECIFIC_PAINT, MACHINE_SPECIFIC_EFFECTS, " +
            "MACHINE_SPECIFIC_EXPRESSION_SNIPPETS or MACHINE_SPECIFIC_SCRIPT_SNIPPETS.";
    }
}
