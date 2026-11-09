using Microsoft.Extensions.Time.Testing;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.History;
using Snail.MCP.AfterEffects.Application.Installation;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Application.Transfer;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Tools.Base;
using Snail.MCP.AfterEffects.Tools.Farm;
using Snail.MCP.AfterEffects.Tools.Transfer;
using Snail.Sdk.Mcp.Hosting;
using Snail.Sdk.Mcp.Hosting.Sessions;

namespace Snail.MCP.AfterEffects.Tests.Tools;

/// <summary>Every refusal the server makes itself carries a type, the word a program branches on instead of reading the sentence.</summary>
/// <remarks>The bridge's refusals always had one; the server's own — scripts switched off, a timeout out of range, a farm or a pool that is not
/// there, a job or a worker nobody has — came without, and ae_script and run_tools said nothing where run_program said ScriptingDisabled.</remarks>
public sealed class RefusalTypeTests : IDisposable
{
    private readonly string _root = TestFolder.Create();

    private ServerConfig Config(ScriptAccess scripting = ScriptAccess.Allow)
    {
        var config = new ServerConfig { DataDirectory = Path.Combine(_root, "data"), Scripting = scripting };
        ServerConfigExtensions.Normalize(config);

        return config;
    }

    private static StubBridge Bridge() => new((_, _) => BridgeReply.Ok(new JsonObject()));

    [Fact]
    public async Task ScriptsSwitchedOff_AreRefusedWithOneType_ByTheScriptToolAndByASequence()
    {
        var tools = new ApplicationTools(Bridge(), Config(ScriptAccess.Off), null!);
        var steps = new JsonArray(new JsonObject { ["command"] = "script", ["params"] = new JsonObject { ["code"] = "1" } });

        var script = await tools.ScriptAsync("1");
        var sequence = await tools.RunToolsAsync(null!, steps);

        Assert.Equal(FailureTypes.ScriptingDisabled, script.Type());
        Assert.Equal(FailureTypes.ScriptingDisabled, sequence.Type());
        Assert.Equal("ScriptingDisabled", FailureTypes.ScriptingDisabled);
    }

    [Fact]
    public async Task ATimeoutOutOfRange_IsTypedArguments()
    {
        var answer = await new ApplicationTools(Bridge(), Config(), null!).ScriptAsync("1", timeoutSeconds: 0);

        Assert.Equal(ToolFailures.Arguments, answer.Type());
    }

    [Fact]
    public async Task AnUndoWithoutAfterEffects_IsTypedUnavailable()
    {
        var config = Config();
        var history = new UndoHistory(config, new FakeAfterEffectsHost([], isRunning: false));

        var answer = await new ApplicationTools(Bridge(), config, history).UndoAsync();

        Assert.Equal(BridgeError.UnavailableType, answer.Type());
    }

    [Fact]
    public void Transfers_AreTypedByWhatIsWrongWithTheAreaOrThePath()
    {
        var tools = Transfers();

        Assert.Equal(ToolFailures.Arguments, tools.Files(area: "elsewhere").Type());
        Assert.Equal(Reached.Outside, tools.Files(path: "../up").Type());
        Assert.Equal("NotFound", tools.Download("missing.txt").Type());
    }

    [Fact]
    public async Task AFarmWithoutAerender_AndAJobNobodyHas_AreTyped()
    {
        var config = Config();
        using var farm = new RenderFarm(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0), executable: null), config, TimeProvider.System, NullLogger<RenderFarm>.Instance);
        var tools = new FarmTools(farm, new ProjectForRender(Bridge(), config), null!);

        var submitted = await tools.SubmitAsync(comp: "Main");
        var cancelled = tools.Cancel("nope");
        var retried = tools.Retry("nope");

        Assert.Equal(FailureTypes.FarmUnavailable, submitted.Type());
        Assert.Equal(FailureTypes.NoSuchJob, cancelled.Type());
        Assert.Equal(FailureTypes.FarmUnavailable, retried.Type());
    }

    [Fact]
    public async Task AnInstallWithNoScriptsFolder_IsTyped()
    {
        var config = Config();
        var installation = new BridgeInstallation(config, new FakeAfterEffectsHost([], isRunning: false), new ExtensionFreshness(config));

        var answer = await new DiagnosticsTools(null!, installation).InstallBridgeAsync();

        Assert.Equal(FailureTypes.NoScriptsFolder, answer.Type());
    }

    private TransferTools Transfers()
    {
        var config = Config();
        var farm = new RenderFarm(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0), executable: null), config, TimeProvider.System, NullLogger<RenderFarm>.Instance);
        var links = new FileLinks(new ClientToken(config), config, new FakeTimeProvider());

        return new TransferTools(new Volume(config, farm), links, new ClientCommands(links, config), config, new ClientSessions(null));
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
