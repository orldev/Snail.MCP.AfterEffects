using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Settings;

/// <summary>Project and application settings, fonts, placeholders, caches and memory: the settings skill.</summary>
[Skill(Skills.Settings, SkillDescriptions.Settings)]
[McpServerToolType]
internal sealed class SettingsTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_project_settings", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Project)]
    [Sends("project_settings")]
    public Task<CallToolResult> ProjectSettingsAsync(
        [Description(ToolDescriptions.Parameters.TimeDisplay)] string? timeDisplay = null,
        [Description("The frame numbers start at 0 or 1.")] int? displayStartFrame = null,
        [Description("FC_START_0, FC_START_1 or FC_TIMECODE_CONVERSION.")] string? framesCount = null,
        [Description("Count frames in feet and frames.")] bool? framesUseFeetFrames = null,
        [Description("MM16 or MM35, for feet and frames.")] string? feetFramesFilm = null,
        [Description("FTCS_START_0 or FTCS_USE_SOURCE_MEDIA: where footage timecode starts.")] string? footageTimecodeStart = null,
        [Description("Colour depth: 8, 16 or 32 bits per channel.")] int? bitsPerChannel = null,
        [Description("Blend colours in linear light.")] bool? linearBlending = null,
        [Description("The working colour space by profile name, as the colour profiles list spells it; empty for none.")] string? workingSpace = null,
        [Description("Working gamma, 2.2 or 2.4, where the colour management system has it.")] double? workingGamma = null,
        [Description("Linearize the working space.")] bool? linearizeWorkingSpace = null,
        [Description("Compensate for scene-referred profiles.")] bool? compensateForSceneReferredProfiles = null,
        [Description("Colour management system as After Effects numbers them, where the version has the setting.")] int? colorManagementSystem = null,
        [Description("Path of an OCIO configuration file; an empty string returns to the built-in one.")] string? ocioConfigurationFile = null,
        [Description("LUT interpolation method as After Effects numbers them.")] int? lutInterpolationMethod = null,
        [Description("javascript-1.0 or extendscript.")] string? expressionEngine = null,
        [Description(ToolDescriptions.Parameters.GpuAccel)] string? gpuAccel = null,
        [Description("Show thumbnails over a transparency grid.")] bool? transparencyGridThumbnails = null,
        [Description("The active tool, as ToolType names it (Tool_Arrow, Tool_Pen, Tool_TextH and so on).")] string? tool = null,
        [Description("List the colour profiles the working space can be set to.")] bool colorProfiles = false,
        [Description("Folder the Import dialog opens in, on After Effects' machine.")] string? defaultImportFolder = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectSettings,
            new JsonObject().With("time_display", timeDisplay).With("display_start_frame", displayStartFrame).With("frames_count", framesCount)
                .With("frames_use_feet_frames", framesUseFeetFrames).With("feet_frames_film", feetFramesFilm).With("footage_timecode_start", footageTimecodeStart)
                .With("bits_per_channel", bitsPerChannel).With("linear_blending", linearBlending).With("working_space", workingSpace).With("working_gamma", workingGamma)
                .With("linearize_working_space", linearizeWorkingSpace).With("compensate_for_scene_referred_profiles", compensateForSceneReferredProfiles)
                .With("color_management_system", colorManagementSystem).With("ocio_configuration_file", ocioConfigurationFile).With("lut_interpolation_method", lutInterpolationMethod)
                .With("expression_engine", expressionEngine).With("gpu_accel", gpuAccel).With("transparency_grid_thumbnails", transparencyGridThumbnails).With("tool", tool)
                .With("color_profiles", colorProfiles).With("default_import_folder", defaultImportFolder),
            cancellationToken);

    [McpServerTool(Name = "ae_replace_in_expressions", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.ReplaceExpressions)]
    [Sends("expressions_replace")]
    public Task<CallToolResult> ReplaceInExpressionsAsync(
        [Description("Text to find in the expressions.")] string find,
        [Description("Text to put in its place; empty removes the text.")] string replace = "",
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ExpressionsReplace, new JsonObject().With("find", find).With("replace", replace), cancellationToken);

    [McpServerTool(Name = "ae_project_fonts", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Fonts)]
    [Sends("project_fonts")]
    public Task<CallToolResult> ProjectFontsAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectFonts, [], cancellationToken);

    [McpServerTool(Name = "ae_replace_font", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.ReplaceFont)]
    [Sends("font_replace")]
    public Task<CallToolResult> ReplaceFontAsync(
        [Description("PostScript name of the font to replace.")] string from,
        [Description("PostScript name of the font to use instead.")] string to,
        [Description("Also reload footage whose fonts were missing.")] bool reloadMissing = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.FontReplace, new JsonObject().With("from", from).With("to", to).With("reload_missing", reloadMissing), cancellationToken);

    [McpServerTool(Name = "ae_import_placeholder", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Placeholder)]
    [Sends("import_placeholder")]
    public Task<CallToolResult> ImportPlaceholderAsync(
        [Description("Name of the placeholder.")] string? name = null,
        [Description("Width in pixels; 1920 by default.")] int? width = null,
        [Description("Height in pixels; 1080 by default.")] int? height = null,
        [Description("Frame rate; 30 by default.")] double? frameRate = null,
        [Description("Duration in seconds; 10 by default.")] double? duration = null,
        [Description("Project folder to put it in, by name or id.")] string? folder = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ImportPlaceholder,
            new JsonObject().With("name", name).With("width", width).With("height", height).With("frame_rate", frameRate).With("duration", duration).WithReference("folder", folder),
            cancellationToken);

    [McpServerTool(Name = "ae_purge", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Purge)]
    [Sends("purge")]
    public Task<CallToolResult> PurgeAsync(
        [Description(ToolDescriptions.Parameters.PurgeTarget)] string target = "ALL_CACHES",
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.Purge, new JsonObject().With("target", target), cancellationToken);

    [McpServerTool(Name = "ae_performance", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Performance)]
    [Sends("app_performance")]
    public Task<CallToolResult> PerformanceAsync(
        [Description("Percent of memory for the image cache; goes with maxMemoryPercent.")] int? imageCachePercent = null,
        [Description("Percent of memory After Effects may use in all.")] int? maxMemoryPercent = null,
        [Description("Multi-frame rendering on or off.")] bool? multiFrameRendering = null,
        [Description("CPU percent multi-frame rendering may take, 1 to 100.")] int? maxCpuPercent = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.AppPerformance,
            new JsonObject().With("image_cache_percent", imageCachePercent).With("max_memory_percent", maxMemoryPercent).With("multi_frame_rendering", multiFrameRendering).With("max_cpu_percent", maxCpuPercent),
            cancellationToken);

    [McpServerTool(Name = "ae_system_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.SystemInfo)]
    [Sends("system_info")]
    public Task<CallToolResult> SystemInfoAsync(
        [Description("Environment variables to read on After Effects' machine, by name.")] string[]? environment = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.SystemInfo, new JsonObject().With("environment", environment), cancellationToken);

    [McpServerTool(Name = "ae_set_application", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.SetApplication)]
    [Sends("app_set")]
    public Task<CallToolResult> SetApplicationAsync(
        [Description("Stop After Effects rendering previews while scripts run; speeds long batches.")] bool? disableRendering = null,
        [Description("Stop and report when a frame of footage is missing, rather than render on.")] bool? reportErrorOnMissingFrame = null,
        [Description("Save the project when After Effects crashes.")] bool? saveProjectOnCrash = null,
        [Description("Write the preferences when After Effects quits.")] bool? savePreferencesOnQuit = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.AppSet,
            new JsonObject().With("disable_rendering", disableRendering).With("report_error_on_missing_frame", reportErrorOnMissingFrame)
                .With("save_project_on_crash", saveProjectOnCrash).With("save_preferences_on_quit", savePreferencesOnQuit),
            cancellationToken);

    [McpServerTool(Name = "ae_read_swatches", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Settings.ReadSwatches)]
    [Sends("swatch_read")]
    public Task<CallToolResult> ReadSwatchesAsync(
        [Description("Path of the .ase or .aco swatch file on After Effects' machine.")] string path,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.SwatchRead, new JsonObject().With("path", path), cancellationToken);

    [McpServerTool(Name = "ae_system_call", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Settings.SystemCall)]
    [Sends("system_call")]
    public Task<CallToolResult> SystemCallAsync(
        [Description("The shell line to run.")] string command,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.SystemCall, new JsonObject().With("command", command), cancellationToken);

    [McpServerTool(Name = "ae_preference", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Preference)]
    [Sends("preference_get")]
    public Task<CallToolResult> PreferenceAsync(
        [Description(ToolDescriptions.Parameters.PreferenceSection)] string section,
        [Description(ToolDescriptions.Parameters.PreferenceKey)] string key,
        [Description(ToolDescriptions.Parameters.PreferenceKind)] string kind = "string",
        [Description(ToolDescriptions.Parameters.PreferenceFile)] string? file = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PreferenceGet, new JsonObject().With("section", section).With("key", key).With("kind", kind).With("file", file), cancellationToken);

    [McpServerTool(Name = "ae_set_preference", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.SetPreference)]
    [Sends("preference_set")]
    public Task<CallToolResult> SetPreferenceAsync(
        [Description(ToolDescriptions.Parameters.PreferenceSection)] string? section = null,
        [Description(ToolDescriptions.Parameters.PreferenceKey)] string? key = null,
        [Description("The value to store, read as kind says.")] JsonNode? value = null,
        [Description(ToolDescriptions.Parameters.PreferenceKind)] string kind = "string",
        [Description(ToolDescriptions.Parameters.PreferenceFile)] string? file = null,
        [Description("Remove the preference instead of setting it.")] bool remove = false,
        [Description("Write the preferences file to disk afterwards.")] bool saveToDisk = false,
        [Description("Re-read the preferences file first, dropping unsaved changes.")] bool reload = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PreferenceSet,
            new JsonObject().With("section", section).With("key", key).With("value", value).With("kind", kind).With("file", file).With("remove", remove).With("save_to_disk", saveToDisk).With("reload", reload),
            cancellationToken);

    [McpServerTool(Name = "ae_script_setting", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.ScriptSetting)]
    [Sends("script_setting_get")]
    public Task<CallToolResult> ScriptSettingAsync(
        [Description("The section, usually the script's or tool's name.")] string section,
        [Description("The key within the section.")] string key,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ScriptSettingGet, new JsonObject().With("section", section).With("key", key), cancellationToken);

    [McpServerTool(Name = "ae_save_script_setting", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.SaveScriptSetting)]
    [Sends("script_setting_save")]
    public Task<CallToolResult> SaveScriptSettingAsync(
        [Description("The section, usually the script's or tool's name.")] string section,
        [Description("The key within the section.")] string key,
        [Description("The text to keep.")] string value,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ScriptSettingSave, new JsonObject().With("section", section).With("key", key).With("value", value), cancellationToken);

    [McpServerTool(Name = "ae_quit", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Settings.Quit)]
    [Sends("app_quit")]
    public Task<CallToolResult> QuitAsync(
        [Description("With unsaved changes: true saves the project first, false drops the changes; empty refuses to quit.")] bool? save = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.AppQuit, new JsonObject().With("save", save), cancellationToken);
}
