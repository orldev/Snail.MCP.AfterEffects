using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>The project: what is open, opening, saving and closing, its items and footage coming in.</summary>
[McpServerToolType]
internal sealed class ProjectTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_project_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Project.Info)]
    [Sends("project_info")]
    public Task<CallToolResult> ProjectInfoAsync(CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectInfo, [], cancellationToken);

    [McpServerTool(Name = "ae_project_new", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Project.New)]
    [Sends("project_new")]
    public Task<CallToolResult> ProjectNewAsync(
        [Description(ToolDescriptions.Parameters.Discard)] bool discard = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectNew, new JsonObject().With("discard", discard), cancellationToken);

    [McpServerTool(Name = "ae_project_open", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Project.Open)]
    [Sends("project_open")]
    public Task<CallToolResult> ProjectOpenAsync(
        [Description("Path of the .aep, .aepx or .aet file on After Effects' machine.")] string path,
        [Description(ToolDescriptions.Parameters.Discard)] bool discard = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectOpen, new JsonObject().With("path", path).With("discard", discard), cancellationToken, ToolLimits.Timeout(120));

    [McpServerTool(Name = "ae_project_save", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Project.Save)]
    [Sends("project_save")]
    public Task<CallToolResult> ProjectSaveAsync(
        [Description("Where to save, for a project saved for the first time or under a new name; empty saves in place.")] string? path = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectSave, new JsonObject().With("path", path), cancellationToken, ToolLimits.Timeout(120));

    [McpServerTool(Name = "ae_project_close", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Project.Close)]
    [Sends("project_close")]
    public Task<CallToolResult> ProjectCloseAsync(
        [Description(ToolDescriptions.Parameters.Discard)] bool discard = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ProjectClose, new JsonObject().With("discard", discard), cancellationToken);

    [McpServerTool(Name = "ae_list_items", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Project.ListItems)]
    [Sends("items_list")]
    public Task<CallToolResult> ListItemsAsync(
        [Description("Keep only items of this kind: comp, footage or folder.")] string? kind = null,
        [Description("Keep only items directly inside this folder, by name or id.")] string? folder = null,
        [Description("Keep only items whose name contains this text, case-insensitive.")] string? nameContains = null,
        [Description("How many items to return at most; the total is reported regardless.")] int limit = 200,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ItemsList,
            new JsonObject().With("kind", kind).WithReference("folder", folder).With("name_contains", nameContains).With("limit", limit),
            cancellationToken);

    [McpServerTool(Name = "ae_item_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Project.ItemInfo)]
    [Sends("item_info")]
    public Task<CallToolResult> ItemInfoAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ItemInfo, new JsonObject().WithReference("item", item), cancellationToken);

    [McpServerTool(Name = "ae_item_update", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Project.ItemUpdate)]
    [Sends("item_update")]
    public Task<CallToolResult> ItemUpdateAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        [Description("The new name.")] string? name = null,
        [Description("The comment shown in the project panel.")] string? comment = null,
        [Description(ToolDescriptions.Parameters.Label)] int? label = null,
        [Description("Folder to move the item into, by name or id.")] string? folder = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ItemUpdate,
            new JsonObject().WithReference("item", item).With("name", name).With("comment", comment).With("label", label).WithReference("folder", folder),
            cancellationToken);

    [McpServerTool(Name = "ae_item_remove", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Project.ItemRemove)]
    [Sends("item_remove")]
    public Task<CallToolResult> ItemRemoveAsync(
        [Description(ToolDescriptions.Parameters.Item)] string item,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ItemRemove, new JsonObject().WithReference("item", item), cancellationToken);

    [McpServerTool(Name = "ae_folder_create", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Project.FolderCreate)]
    [Sends("folder_create")]
    public Task<CallToolResult> FolderCreateAsync(
        [Description("Name of the new folder.")] string name,
        [Description("Parent folder, by name or id; the project root otherwise.")] string? folder = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.FolderCreate, new JsonObject().With("name", name).WithReference("folder", folder), cancellationToken);

    [McpServerTool(Name = "ae_import_file", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Project.ImportFile)]
    [Sends("import_file")]
    public Task<CallToolResult> ImportFileAsync(
        [Description("Path on After Effects' machine: footage, image, sound, a sequence's first frame, .psd/.ai, or another .aep.")] string path,
        [Description("footage (default), comp (layered file as a composition), comp_cropped (layers cropped to content) or project (merge).")] string? @as = null,
        [Description("Import the file as the first frame of a numbered sequence.")] bool sequence = false,
        [Description("Folder to put the item into, by name or id.")] string? folder = null,
        [Description("Name for the item; the file's name otherwise.")] string? name = null,
        [Description("Sequence: order frames alphabetically; on unless a range is given.")] bool forceAlphabetical = true,
        [Description("Sequence: first frame number to import.")] int? rangeStart = null,
        [Description("Sequence: last frame number to import.")] int? rangeEnd = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.ImportFile,
            new JsonObject().With("path", path).With("as", @as).With("sequence", sequence).WithReference("folder", folder).With("name", name)
                .With("force_alphabetical", forceAlphabetical).With("range_start", rangeStart).With("range_end", rangeEnd),
            cancellationToken, ToolLimits.Timeout(120));
}
