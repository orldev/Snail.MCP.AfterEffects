using System.ComponentModel;
using ModelContextProtocol.Server;

namespace Snail.MCP.AfterEffects.Tools.Prompts;

/// <summary>Recipes a studio follows, offered as MCP prompts so a client can start a task with the right order of tools already in front of the model.</summary>
[McpServerPromptType]
internal sealed class PipelinePrompts
{
    [McpServerPrompt(Name = PromptTexts.TitleCardName)]
    [Description(PromptTexts.TitleCardDescription)]
    public string TitleCard(
        [Description("The title text.")] string title,
        [Description("Width in pixels.")] string width = "1920",
        [Description("Height in pixels.")] string height = "1080",
        [Description("Frame rate.")] string fps = "25",
        [Description("Duration in seconds.")] string duration = "5",
        [Description("Delivery: H.264 for review, PNG frames, ProRes master.")] string delivery = "H.264 for review") =>
        PromptTexts.TitleCard
            .Replace("{title}", title, StringComparison.Ordinal).Replace("{width}", width, StringComparison.Ordinal).Replace("{height}", height, StringComparison.Ordinal)
            .Replace("{fps}", fps, StringComparison.Ordinal).Replace("{duration}", duration, StringComparison.Ordinal).Replace("{delivery}", delivery, StringComparison.Ordinal);

    [McpServerPrompt(Name = PromptTexts.TemplateFillName)]
    [Description(PromptTexts.TemplateFillDescription)]
    public string TemplateFill(
        [Description("The template: a .mogrt path or the name of the composition in the project.")] string template,
        [Description("The values to fill, in words: title, subtitle, colours, logo.")] string values,
        [Description("Delivery: H.264 for review, PNG frames, ProRes master, or a new .mogrt.")] string delivery = "H.264 for review") =>
        PromptTexts.TemplateFill
            .Replace("{template}", template, StringComparison.Ordinal).Replace("{values}", values, StringComparison.Ordinal).Replace("{delivery}", delivery, StringComparison.Ordinal);

    [McpServerPrompt(Name = PromptTexts.DeliveryName)]
    [Description(PromptTexts.DeliveryDescription)]
    public string Delivery(
        [Description("The composition by name.")] string comp,
        [Description("Absolute folder for the output.")] string directory,
        [Description("Delivery: H.264 for review, ProRes master, PNG frames.")] string delivery = "H.264 for review") =>
        PromptTexts.Delivery
            .Replace("{comp}", comp, StringComparison.Ordinal).Replace("{directory}", directory, StringComparison.Ordinal).Replace("{delivery}", delivery, StringComparison.Ordinal);
}
