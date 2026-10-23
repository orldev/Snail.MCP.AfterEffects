using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Tools.Base;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class ToolIndexTests
{
    private static readonly ToolIndex Index = new(typeof(DiscoveryTools).Assembly);

    /// <summary>Plain words find the tool: the name is matched by its words, so "add a layer" reaches ae_add_layer ahead of the rest.</summary>
    [Fact]
    public void Search_FindsATool_ByWhatItDoes()
    {
        var matches = Index.Search("add a layer to the composition", 3);

        Assert.Equal("ae_add_layer", matches[0].Tool.Name);
    }

    /// <summary>A match carries the command and parameters a program needs, so finding a tool is enough to call it.</summary>
    [Fact]
    public void AMatch_CarriesTheCommandAndParameters()
    {
        var match = Assert.Single(Index.Search("add a layer", 1));

        Assert.Equal("layer_add", match.Tool.Command);
        Assert.Contains(match.Tool.Parameters, parameter => parameter.Name == "kind" && parameter.Type == "string");
    }

    [Fact]
    public void Search_WithNothingToMatch_IsEmpty()
    {
        Assert.Empty(Index.Search("the of a", 5));
        Assert.Empty(Index.Search("quaternion slerp gimbal", 5));
    }

    /// <summary>Every bridge-sending tool declares its command, and only the server's own tools send none.</summary>
    [Fact]
    public void EveryTool_IsIndexed_WithItsCommandWhenItHasOne()
    {
        var withoutCommand = Index.Tools.Where(tool => tool.Command is null).Select(tool => tool.Name).Order(StringComparer.Ordinal).ToList();

        Assert.Equal(
            ["ae_diagnose", "ae_disable_skill", "ae_download", "ae_enable_skill", "ae_farm_cancel", "ae_farm_cleanup", "ae_farm_jobs", "ae_farm_retry", "ae_farm_submit", "ae_files", "ae_install_bridge", "ae_lease", "ae_redo", "ae_skills", "ae_stop_worker", "ae_undo", "ae_upload", "ae_workers", "find_tool", "run_program", "run_tools"],
            withoutCommand);
    }
}
