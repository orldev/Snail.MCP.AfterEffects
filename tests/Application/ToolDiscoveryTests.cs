using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Extensions;
using Snail.Sdk.Mcp.Hosting.Discovery;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>find_tool's words for this server, ranked by Snail.Sdk as find_tool ranks them, over the tool classes the server registers.</summary>
public sealed class ToolDiscoveryTests
{
    private static readonly DiscoveryOptions Options = new()
    {
        Prefix = ToolCommands.Prefix,
        Noise = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { TransportRegistrationExtensions.ShortName },
        CommandOf = ToolCommands.CommandOf,
    };

    private static readonly IReadOnlyList<ToolEntry> Catalog =
        [.. ClientRegistrationExtensions.ToolTypes(typeof(ToolCommands).Assembly).SelectMany(type => ToolCatalog.Of(type))];

    /// <summary>Plain words find the tool: the name is matched by its words, so "add a layer" reaches ae_add_layer ahead of the rest.</summary>
    [Fact]
    public void Search_FindsATool_ByWhatItDoes()
    {
        var matches = ToolCatalog.Find(Catalog, "add a layer to the composition", 3, Options);

        Assert.Equal("ae_add_layer", matches[0].Name);
    }

    /// <summary>A tool of a skill is found whether the skill is loaded or not, and says which skill brings it.</summary>
    [Fact]
    public void Search_FindsTheToolsOfASkill()
    {
        var matches = ToolCatalog.Find(Catalog, "ease the keyframes", 5, Options);

        Assert.Contains(matches, match => match.Skill == Skills.Animation);
    }

    [Fact]
    public void Search_WithNothingButGlue_IsEmpty()
    {
        Assert.Empty(ToolCatalog.Find(Catalog, "the of a", 5, Options));
    }

    /// <summary>Words no tool uses find nothing, so find_tool answers isMatched false and lists the tools by name instead.</summary>
    [Fact]
    public void Search_WithWordsNoToolUses_IsEmpty()
    {
        Assert.Empty(ToolCatalog.Find(Catalog, "quaternion slerp gimbal", 5, Options));
    }

    /// <summary>find_tool names a tool's parameters as its command takes them, so a program written from the answer sends what the bridge
    /// reads: light_type, not lightType, and no timeoutSeconds, which a program's own deadline decides.</summary>
    [Theory]
    [InlineData("lightType", "light_type")]
    [InlineData("layer", "layer")]
    [InlineData("timeoutSeconds", null)]
    public void ACommandsParameter_IsNamedAsTheBridgeReadsIt(string parameter, string? named)
    {
        Assert.Equal(named, ToolCommands.ParameterOf("ae_add_light", parameter));
    }

    /// <summary>find_tool asks for the field of an object parameter by its dotted path: every segment is spelled as the bridge reads it and
    /// the field stays inside its object, which the bridge receives whole; only the tool's own timeoutSeconds is dropped, never a field
    /// that happens to share the name.</summary>
    [Theory]
    [InlineData("output.fileFormat", "output.file_format")]
    [InlineData("renderSettings.frameRate", "render_settings.frame_rate")]
    [InlineData("job.timeoutSeconds", "job.timeout_seconds")]
    [InlineData("focus.distance", "focus.distance")]
    public void AFieldOfAnObjectParameter_StaysNestedInTheBridgesSpelling(string path, string named)
    {
        Assert.Equal(named, ToolCommands.ParameterOf("ae_add_light", path));
    }

    /// <summary>Every bridge-sending tool names its command, which find_tool answers beside it and a program calls; only the server's own
    /// tools send none.</summary>
    [Fact]
    public void EveryTool_NamesItsCommand_WhenItHasOne()
    {
        var withoutCommand = Catalog.Where(tool => ToolCommands.CommandOf(tool.Name) is null).Select(tool => tool.Name).Order(StringComparer.Ordinal).ToList();

        Assert.Equal(
            ["ae_diagnose", "ae_download", "ae_farm_cancel", "ae_farm_cleanup", "ae_farm_jobs", "ae_farm_retry", "ae_farm_submit", "ae_files", "ae_install_bridge", "ae_lease", "ae_redo", "ae_stop_worker", "ae_undo", "ae_upload", "ae_workers", "run_tools"],
            withoutCommand);
        Assert.Equal("layer_add", ToolCommands.CommandOf("ae_add_layer"));
    }
}
