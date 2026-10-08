using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>The property tree of a layer: reading any property, setting any value, and expressions.</summary>
[McpServerToolType]
internal sealed class PropertyTools(IAfterEffectsBridge bridge) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_property_tree", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Properties.Tree)]
    [Sends("property_tree")]
    public Task<CallToolResult> PropertyTreeAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Group to start from, as a path; the whole layer when empty.")] string? path = null,
        [Description("How many levels below the start to list; 2 shows the groups and what is directly in them.")] int depth = 2,
        [Description("Include the current value of every property listed; false lists names and types only.")] bool values = true,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PropertyTree,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("depth", depth).With("values", values),
            cancellationToken);

    [McpServerTool(Name = "ae_get_property", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Properties.Get)]
    [Sends("property_get")]
    public Task<CallToolResult> GetPropertyAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PropertyGet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("time", time),
            cancellationToken);

    [McpServerTool(Name = "ae_set_property", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Properties.Set)]
    [Sends("property_set")]
    public Task<CallToolResult> SetPropertyAsync(
        [Description(ToolDescriptions.Parameters.Layer)] string layer,
        [Description(ToolDescriptions.Parameters.PropertyPath)] string path,
        [Description(ToolDescriptions.Parameters.PropertyValue)] JsonNode? value = null,
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description("Set the value as a keyframe at this time; without it an unanimated property changes everywhere and an animated one is keyed at the current time.")] double? time = null,
        [Description("An expression to put on the property; an empty string removes it.")] string? expression = null,
        [Description("For a group such as an effect or a mask: switch it on or off; for a property with an expression: enable or disable the expression.")] bool? enabled = null,
        CancellationToken cancellationToken = default) =>
        SendAsync(BridgeCommands.PropertySet,
            new JsonObject().WithReference("comp", comp).WithReference("layer", layer).With("path", path).With("value", value).With("time", time)
                .With("expression", expression).With("enabled", enabled),
            cancellationToken);
}
