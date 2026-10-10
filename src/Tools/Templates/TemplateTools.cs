using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Templates;

/// <summary>Essential Graphics templates, their instances and .mogrt files: the templates skill.</summary>
[Skill(Skills.Templates, SkillDescriptions.Templates)]
[McpServerToolType]
internal sealed class TemplateTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_template_info", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Templates.Info)]
    [Sends("template_info")]
    public Task<CallToolResult> TemplateInfoAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TemplateInfo, new JsonObject().WithReference("comp", comp), cancellationToken);

    [McpServerTool(Name = "ae_update_template", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Templates.Update)]
    [Sends("template_update")]
    public Task<CallToolResult> UpdateTemplateAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("The template name, shown in Premiere Pro and the Essential Graphics panel.")] string? name = null,
        [Description("Open the composition in the Essential Graphics panel.")] bool openPanel = false,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TemplateUpdate, new JsonObject().WithReference("comp", comp).With("name", name).With("open_panel", openPanel), cancellationToken);

    [McpServerTool(Name = "ae_add_template_property", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = false)]
    [Description(ToolDescriptions.Templates.AddProperty)]
    [Sends("template_add_property")]
    public Task<CallToolResult> AddTemplatePropertyAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Name of the controller; the property's own name when empty.")] string? name = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TemplateAddProperty,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("name", name),
            cancellationToken);

    [McpServerTool(Name = "ae_fill_template", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Templates.Fill)]
    [Sends("template_fill")]
    public Task<CallToolResult> FillTemplateAsync(
        [Description("The nested composition layer that is the template instance, by name or index.")] string layer,
        [Description(ToolDescriptions.Parameters.TemplateValues)] JsonObject values,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the values as keyframes at this time.")] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TemplateFill,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("values", values).With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_substitute_template", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Templates.Substitute)]
    [Sends("template_substitute")]
    public Task<CallToolResult> SubstituteTemplateAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Layer names to the new text of each text layer.")] JsonObject? text = null,
        [Description("Layer names to the new colour of each, as #rrggbb or [r, g, b].")] JsonObject? colors = null,
        [Description("Layer names to the footage item or composition (by name or id) that replaces each layer's source.")] JsonObject? footage = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.TemplateSubstitute,
            new JsonObject().WithReference("comp", comp).With("text", text).With("colors", colors).With("footage", footage),
            cancellationToken);

    [McpServerTool(Name = "ae_export_template", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Templates.Export)]
    [Sends("template_export")]
    public Task<CallToolResult> ExportTemplateAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("A .mogrt file path, or a folder to take the template's name; the Essential Graphics library when empty.")] string? path = null,
        [Description("Replace a file that is already there.")] bool overwrite = false,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 120,
        CancellationToken cancellationToken = default) =>
        WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.TemplateExport,
            new JsonObject().WithReference("comp", comp).With("path", path).With("overwrite", overwrite), cancellationToken, timeout));

    [McpServerTool(Name = "ae_import_template", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Templates.Import)]
    [Sends("template_import")]
    public Task<CallToolResult> ImportTemplateAsync(
        [Description("Path of the .mogrt file.")] string path,
        [Description("Project folder to put it in, by name or id; the root when empty.")] string? folder = null,
        [Description("Open the imported composition in the viewer.")] bool open = false,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 120,
        CancellationToken cancellationToken = default) =>
        WithinAsync(timeoutSeconds, timeout => SendAsync(BridgeCommands.TemplateImport,
            new JsonObject().With("path", path).WithReference("folder", folder).With("open", open), cancellationToken, timeout));
}
