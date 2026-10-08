namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand ProjectInfo = new("project_info", "project");

    public static readonly BridgeCommand ProjectNew = new("project_new", "project");

    public static readonly BridgeCommand ProjectOpen = new("project_open", "project");

    public static readonly BridgeCommand ProjectSave = new("project_save", "project");

    public static readonly BridgeCommand ProjectClose = new("project_close", "project");

    public static readonly BridgeCommand ItemsList = new("items_list", "project");

    public static readonly BridgeCommand ItemInfo = new("item_info", "project");

    public static readonly BridgeCommand ItemUpdate = new("item_update", "project");

    public static readonly BridgeCommand ItemRemove = new("item_remove", "project");

    public static readonly BridgeCommand FolderCreate = new("folder_create", "project");

    public static readonly BridgeCommand ImportFile = new("import_file", "project");
}
