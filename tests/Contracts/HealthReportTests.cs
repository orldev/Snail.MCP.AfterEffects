using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Announcements;
using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The report ae_diagnose answers with, read as the contract it is: an agent and the operator's page both find their way around it by
/// the names of its fields, so the names are asserted here rather than left to whoever edits the builder next.</summary>
/// <remarks>Written after a mutation run: of the forty-seven mutants that survived in ServerHealth, twenty-four were a field's name replaced by
/// another string, which no test noticed. A renamed field breaks every client that reads the report, and the build said nothing.</remarks>
public sealed class HealthReportTests
{
    [Fact]
    public async Task TheReport_CarriesTheFieldsItsReadersLookFor()
    {
        var report = await Health(connected: true).ReportAsync(CancellationToken.None);

        Assert.Equal(["afterEffects", "bridge", "dataDirectory", "extension", "idleTimeoutMinutes", "project", "server", "transport"], Fields(report));
        Assert.Equal(["name", "version"], Fields(report["server"]!));
        Assert.Equal(["agent", "announcements", "client", "connected", "port", "requestTimeoutSeconds", "scopes", "token", "worker"], Fields(report["bridge"]!));
        Assert.Equal(["bundled", "installed", "protocol", "status"], Fields(report["extension"]!));
        Assert.Equal(["kind"], Fields(report["transport"]!));
    }

    /// <summary>An After Effects that answers is reported reachable, with what its own ping said kept as it came.</summary>
    [Fact]
    public async Task AnAfterEffectsThatAnswers_IsReportedReachable()
    {
        var report = await Health(connected: true).ReportAsync(CancellationToken.None);
        var afterEffects = report["afterEffects"]!;

        Assert.True(afterEffects["reachable"]!.GetValue<bool>());
        Assert.Equal(7, afterEffects["ticks"]!.GetValue<int>());
        Assert.Equal("stdio", report["transport"]!["kind"]!.GetValue<string>());
    }

    /// <summary>An After Effects that does not answer is reported with the reason and what to do about it, which is the whole point of asking.</summary>
    [Fact]
    public async Task AnAfterEffectsThatIsNotThere_IsReportedWithTheReasonAndAHint()
    {
        var report = await Health(connected: false).ReportAsync(CancellationToken.None);
        var afterEffects = report["afterEffects"]!;

        Assert.False(afterEffects["reachable"]!.GetValue<bool>());
        Assert.False(string.IsNullOrWhiteSpace(afterEffects["error"]!.GetValue<string>()));
        Assert.False(string.IsNullOrWhiteSpace(afterEffects["hint"]!.GetValue<string>()));
        Assert.Equal("unknown", report["extension"]!["status"]!.GetValue<string>());
    }

    /// <summary>Scripts that may not write files or reach the network is the one thing a person has to switch on by hand, so the report says it
    /// in a note of its own rather than leaving the reader to notice a false.</summary>
    [Fact]
    public async Task AnAfterEffectsWithScriptsHeldBack_SaysSoInANote()
    {
        var report = await Health(connected: true, scriptingAllowed: false).ReportAsync(CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(report["afterEffects"]!["note"]!.GetValue<string>()));
    }

    /// <summary>What a load balancer asks is short on purpose: that the server runs, and whether After Effects answers right now.</summary>
    [Fact]
    public async Task Liveness_SaysTheServerRuns_AndWhetherAfterEffectsAnswers()
    {
        var up = await Health(connected: true).LivenessAsync(CancellationToken.None);
        var down = await Health(connected: false).LivenessAsync(CancellationToken.None);

        Assert.Equal(["afterEffects", "status"], Fields(up));
        Assert.Equal("ok", up["status"]!.GetValue<string>());
        Assert.Equal("up", up["afterEffects"]!.GetValue<string>());
        Assert.Equal("down", down["afterEffects"]!.GetValue<string>());
    }

    private static IReadOnlyList<string> Fields(JsonNode report) =>
        [.. report.AsObject().Select(field => field.Key).Order(StringComparer.Ordinal)];

    private static ServerHealth Health(bool connected, bool scriptingAllowed = true)
    {
        var config = new ServerConfig { DataDirectory = TestFolder.Create() };
        ServerConfigExtensions.Normalize(config);
        var ping = new JsonObject { ["ticks"] = 7, ["scripting_allowed"] = scriptingAllowed };
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(ping.DeepClone())) { IsConnected = connected };

        return new ServerHealth(
            config,
            bridge,
            new BridgeToken(config.Bridge, NullLogger<BridgeToken>.Instance),
            new ServerAnnouncement(config, NullLogger<ServerAnnouncement>.Instance),
            new ExtensionFreshness(config));
    }
}
