namespace Snail.MCP.AfterEffects.Domain.Commands;

internal static partial class BridgeCommands
{
    public static readonly BridgeCommand TemplateInfo = new("template_info", "templates");

    public static readonly BridgeCommand TemplateUpdate = new("template_update", "templates");

    public static readonly BridgeCommand TemplateAddProperty = new("template_add_property", "templates");

    public static readonly BridgeCommand TemplateFill = new("template_fill", "templates");

    public static readonly BridgeCommand TemplateSubstitute = new("template_substitute", "templates");

    public static readonly BridgeCommand TemplateExport = new("template_export", "templates");

    public static readonly BridgeCommand TemplateImport = new("template_import", "templates");
}
