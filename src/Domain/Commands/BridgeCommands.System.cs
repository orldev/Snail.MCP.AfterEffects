namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand SystemInfo = new("system_info", "system");

    public static readonly BridgeCommand SystemCall = new("system_call", "system");

    public static readonly BridgeCommand PreferenceGet = new("preference_get", "system");

    public static readonly BridgeCommand PreferenceSet = new("preference_set", "system");

    public static readonly BridgeCommand ScriptSettingGet = new("script_setting_get", "system");

    public static readonly BridgeCommand ScriptSettingSave = new("script_setting_save", "system");

    public static readonly BridgeCommand AppSet = new("app_set", "system");

    public static readonly BridgeCommand SwatchRead = new("swatch_read", "system");

    public static readonly BridgeCommand AppQuit = new("app_quit", "system");
}
