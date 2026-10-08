namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand RenderQueue = new("render_queue", "render");

    public static readonly BridgeCommand RenderTemplates = new("render_templates", "render");

    public static readonly BridgeCommand RenderAdd = new("render_add", "render");

    public static readonly BridgeCommand RenderUpdate = new("render_update", "render");

    public static readonly BridgeCommand RenderRemove = new("render_remove", "render");

    public static readonly BridgeCommand RenderStart = new("render_start", "render");

    public static readonly BridgeCommand RenderInAme = new("render_in_ame", "render");

    public static readonly BridgeCommand RenderControl = new("render_control", "render");

    public static readonly BridgeCommand RenderTemplateSave = new("render_template_save", "render");

    public static readonly BridgeCommand WatchFolder = new("watch_folder", "render");
}
