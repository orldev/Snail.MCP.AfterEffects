using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Domain;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class SkillCatalogTests
{
    private readonly FakeTimeProvider _clock = new();
    private readonly McpServerPrimitiveCollection<McpServerTool> _tools = [];
    private readonly SkillCatalog _catalog;

    public SkillCatalogTests()
    {
        var probe = new Skill("probe", "A skill for this test.", [typeof(ProbeTools)]);

        _catalog = new SkillCatalog([probe], _tools, new ServiceCollection().BuildServiceProvider(), _clock);
    }

    /// <summary>Enabling a skill puts its tools in the collection the client reads; disabling takes them out again.</summary>
    [Fact]
    public void Enable_ThenDisable_MovesTheToolsInAndOutOfTheList()
    {
        Assert.False(Assert.Single(_catalog.Status()).IsEnabled);

        var enabled = _catalog.Enable("PROBE");

        Assert.True(enabled!.IsEnabled);
        Assert.Equal(["ae_probe_one", "ae_probe_two"], enabled.Tools);
        Assert.True(_tools.TryGetPrimitive("ae_probe_one", out _));

        Assert.False(_catalog.Disable("probe")!.IsEnabled);
        Assert.Empty(_tools);
    }

    [Fact]
    public void AnUnknownSkill_IsNull()
    {
        Assert.Null(_catalog.Enable("nope"));
        Assert.Null(_catalog.Disable("nope"));
    }

    /// <summary>A skill nobody used for the idle time is unloaded; a call to one of its tools, or a pin, keeps it.</summary>
    [Fact]
    public void ExpireIdle_UnloadsUntouchedSkills_AndKeepsUsedOrPinnedOnes()
    {
        _catalog.Enable("probe");
        _clock.Advance(TimeSpan.FromMinutes(5));
        _catalog.Touch("ae_probe_two");
        _clock.Advance(TimeSpan.FromMinutes(6));

        Assert.Empty(_catalog.ExpireIdle(TimeSpan.FromMinutes(10)));

        _catalog.Pin("probe");
        _clock.Advance(TimeSpan.FromMinutes(20));

        Assert.Empty(_catalog.ExpireIdle(TimeSpan.FromMinutes(10)));

        _catalog.Unpin("probe");

        Assert.Equal(["probe"], _catalog.ExpireIdle(TimeSpan.FromMinutes(10)));
        Assert.Empty(_tools);
    }

    /// <summary>Discovery reads skills off the attribute, grouping the classes that share a name.</summary>
    [Fact]
    public void Discover_GroupsToolClasses_BySkillName()
    {
        var skills = SkillCatalog.Discover(typeof(SkillCatalogTests).Assembly);

        var probe = Assert.Single(skills, skill => skill.Name == "probe");
        Assert.Contains(typeof(ProbeTools), probe.ToolTypes);
    }

    [Skill("probe", "A skill for this test.")]
    [McpServerToolType]
    public sealed class ProbeTools
    {
        [McpServerTool(Name = "ae_probe_one")]
        [Description("One.")]
        public CallToolResult One() => new();

        [McpServerTool(Name = "ae_probe_two")]
        [Description("Two.")]
        public CallToolResult Two() => new();
    }
}
