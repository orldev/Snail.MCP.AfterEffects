using Snail.MCP.AfterEffects.Adapters.Programs;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Tools;
using Snail.Sdk.Mcp.Programs;

namespace Snail.MCP.AfterEffects.Tests.Adapters;

/// <summary>The bridge's commands as a program calls them, run by Snail.Sdk's runner with this server's limits: what reaches After Effects,
/// what comes back, and what this server refuses before anything is sent.</summary>
public class BridgeProgramCommandsTests
{
    private static readonly ProgramRunner Runner = new(new ProgramOptions
    {
        Global = "ae",
        MaxCalls = ProgramLimits.MostCalls,
        Timeout = TimeSpan.FromSeconds(ProgramLimits.DefaultTimeoutSeconds),
    });

    [Fact]
    public async Task Program_CallsCommands_AndReturnsWhatItComputes()
    {
        var bridge = new StubBridge((command, parameters) => BridgeReply.Ok(new JsonObject { ["index"] = IndexFrom(parameters) }));

        var run = await RunAsync(bridge, """
            const made = [];
            for (let i = 0; i < input.count; i++) { made.push(ae.layer_add({ kind: "null", name: "n" + i }).index); }
            log("made", made.length);
            return { made };
            """, new JsonObject { ["count"] = 3 });

        Assert.Null(run.Failure);
        Assert.Equal(3, run.Result!["made"]!.AsArray().Count);
        Assert.Equal(3, bridge.Sent.Count);
        Assert.Equal("n2", bridge.Sent[2].Parameters["name"]!.ToString());
        Assert.Equal(["made 3"], run.Log);
    }

    private static int IndexFrom(JsonObject parameters) => int.Parse(parameters["name"]!.ToString()[1..]) + 1;

    /// <summary>A program that called a command by the name of its tool is told the names that come closest: the words of a tool and of its
    /// command are in opposite order, and six of the eighteen failed calls in a fortnight of logs were that guess.</summary>
    [Theory]
    [InlineData("create_comp", "comp_create")]
    [InlineData("remove_layers", "layer_remove")]
    [InlineData("list_layers", "layers_list")]
    [InlineData("layer_list", "layers_list")]
    public async Task AProgram_AskingForACommandTheCatalogHasNot_IsToldTheNearestNames(string asked, string nearest)
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await RunAsync(bridge, $"return ae.{asked}({{}});");

        Assert.Equal(ProgramFailures.NoSuchCommand, run.Failure!.Type);
        Assert.Contains(asked, run.Failure.Error, StringComparison.Ordinal);
        Assert.Contains(nearest, run.Failure.Error, StringComparison.Ordinal);
        Assert.Empty(bridge.Sent);
    }

    /// <summary>A failed command is an exception the program can catch, with the bridge's own type; uncaught, it ends the program with that type.</summary>
    [Fact]
    public async Task FailedCommand_ThrowsIntoTheProgram_WithTheBridgesType()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Failed(new BridgeError("NotFound", "no such layer")));

        var caught = await RunAsync(bridge, "try { ae.layer_info({ layer: 'x' }); } catch (e) { return e.type + ':' + e.error; }");
        var uncaught = await RunAsync(bridge, "ae.layer_info({ layer: 'x' }); return 1;");

        Assert.Equal("NotFound:no such layer", caught.Result!.ToString());
        Assert.Equal("NotFound", uncaught.Failure!.Type);
        Assert.Single(uncaught.Calls);
    }

    /// <summary>The bridge's hint for its type rides with the failure, so the program — and the answer of one that stops — says what to do.</summary>
    [Fact]
    public async Task FailedCommand_CarriesTheHintOfItsType()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Failed(new BridgeError("NotFound", "no such layer")));

        var run = await RunAsync(bridge, "try { ae.layer_info({ layer: 'x' }); } catch (e) { return e.hint; }");

        Assert.Contains("ae_list_items", run.Result!.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Program_StopsAfterTheCallLimit_AndTheCallsSoFarStand()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await RunAsync(bridge, "for (let i = 0; i < 1000; i++) { ae.ping({}); } return 'done';");

        Assert.Equal(ProgramFailures.TooManyCalls, run.Failure!.Type);
        Assert.Equal(ProgramLimits.MostCalls, run.Calls.Count);
    }

    /// <summary>A server with scripts switched off must not run them by way of one line of JavaScript.</summary>
    [Fact]
    public async Task Program_WithScriptsOff_IsRefusedTheScriptCommand()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await RunAsync(bridge, "return ae.script({ code: '1' });", scripting: ScriptAccess.Off);

        Assert.Equal(FailureTypes.ScriptingDisabled, run.Failure!.Type);
        Assert.Empty(bridge.Sent);
    }

    /// <summary>Nor by way of a sequence that carries the script command among its steps, which run_tools refuses for the same reason.</summary>
    [Fact]
    public async Task Program_WithScriptsOff_IsRefusedASequenceThatCarriesAScript()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await RunAsync(bridge, """
            return ae.sequence({ steps: [{ command: "ping", params: {} }, { command: "script", params: { code: "1" } }] });
            """, scripting: ScriptAccess.Off);

        Assert.Equal(FailureTypes.ScriptingDisabled, run.Failure!.Type);
        Assert.Empty(bridge.Sent);
    }

    /// <summary>The gate is asked for every command under the command's own name, and a refused one never reaches After Effects.</summary>
    [Fact]
    public async Task Program_RefusedACommandByTheGate_SendsNothingOfIt()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await Runner.RunAsync(Commands(bridge), "ae.ping({}); return ae.layer_remove({ layer: 1 });", refusal: tool => tool == "layer_remove" ? "'layer_remove' is delete work" : null);

        Assert.Equal(ProgramFailures.NotGranted, run.Failure!.Type);
        Assert.Equal(["ping"], bridge.Sent.Select(sent => sent.Command.Name));
    }

    /// <summary>A command waits for After Effects as long as its program has left, not the longest a program may ever run.</summary>
    [Fact]
    public async Task Program_GivesACommand_TheTimeItHasLeft()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await RunAsync(bridge, "return ae.ping({});");

        Assert.Null(run.Failure);
        var waited = Assert.Single(bridge.Waited);
        Assert.InRange(waited!.Value, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void EveryCommandOfTheBridge_IsOneAProgramMayCall()
    {
        var commands = Commands(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject())));

        Assert.Equal(BridgeCommands.All.Select(command => command.Name), commands.Names);
        Assert.All(commands.Names, name => Assert.Null(commands.ToolOf(name)));
    }

    private static Task<ProgramRun> RunAsync(StubBridge bridge, string code, JsonObject? input = null, ScriptAccess scripting = ScriptAccess.Allow) =>
        Runner.RunAsync(Commands(bridge, scripting), code, input, TimeSpan.FromSeconds(10));

    private static BridgeProgramCommands Commands(StubBridge bridge, ScriptAccess scripting = ScriptAccess.Allow) =>
        new(bridge, new ServerConfig { Scripting = scripting });
}
