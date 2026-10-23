using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Access;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Adapters.Programs;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;

namespace Snail.MCP.AfterEffects.Tests.Adapters;

public class JavaScriptProgramsTests
{
    private static readonly ToolScopes Scopes = new(new CommandSemantics(), new ToolIndex(typeof(ToolScopes).Assembly));

    private static readonly ClientSessions Clients = new([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);

    [Fact]
    public async Task Program_CallsCommands_AndReturnsWhatItComputes()
    {
        var bridge = new StubBridge((command, parameters) => BridgeReply.Ok(new JsonObject { ["index"] = IndexFrom(parameters) }));

        var run = await Programs(bridge).RunAsync("""
            const made = [];
            for (let i = 0; i < input.count; i++) { made.push(ae.layer_add({ kind: "null", name: "n" + i }).index); }
            log("made", made.length);
            return { made };
            """, new JsonObject { ["count"] = 3 }, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Null(run.Error);
        Assert.Equal(3, run.Result!["made"]!.AsArray().Count);
        Assert.Equal(3, bridge.Sent.Count);
        Assert.Equal("n2", bridge.Sent[2].Parameters["name"]!.ToString());
        Assert.Equal(["made 3"], run.Log);
    }

    private static int IndexFrom(JsonObject parameters) => int.Parse(parameters["name"]!.ToString()[1..]) + 1;

    /// <summary>A program that called a command by the name of its tool is told the names that come closest: the words of a tool and of its
    /// command are in opposite order, and six of the eighteen failed calls in a fortnight of logs were that guess, answered by nothing but the
    /// miss itself.</summary>
    [Theory]
    [InlineData("create_comp", "comp_create")]
    [InlineData("remove_layers", "layer_remove")]
    [InlineData("list_layers", "layers_list")]
    [InlineData("layer_list", "layers_list")]
    public async Task AProgram_AskingForACommandTheCatalogHasNot_IsToldTheNearestNames(string asked, string nearest)
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await Programs(bridge).RunAsync($"return ae.{asked}({{}});", null, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Equal("NoSuchCommand", run.Error!.Type);
        Assert.Contains(asked, run.Error.Message, StringComparison.Ordinal);
        Assert.Contains(nearest, run.Error.Message, StringComparison.Ordinal);
        Assert.Empty(bridge.Sent);
    }

    /// <summary>A name that shares no word with any command is answered with that alone, rather than with the three names that happen to sort first.</summary>
    [Fact]
    public async Task AProgram_AskingForSomethingUnlikeAnyCommand_IsToldOnlyThat()
    {
        var run = await Programs(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject())))
            .RunAsync("return ae.frobnicate({});", null, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Equal("NoSuchCommand", run.Error!.Type);
        Assert.DoesNotContain("nearest", run.Error.Message, StringComparison.Ordinal);
    }

    /// <summary>The guard leaves the catalog as it was: a command is still a function to call, and a name may be read without being called.</summary>
    [Fact]
    public async Task TheGuardedCatalog_StillAnswersForTheCommandsItHolds()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject { ["ticks"] = 7 }));

        var run = await Programs(bridge).RunAsync("""
            const kinds = [typeof ae.ping, typeof ae.nothing_like_this];

            return { kinds, ticks: ae.ping({}).ticks };
            """, null, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Null(run.Error);
        Assert.Equal(["function", "function"], run.Result!["kinds"]!.AsArray().Select(kind => kind!.ToString()));
        Assert.Equal(7, run.Result["ticks"]!.GetValue<int>());
    }

    /// <summary>A failed command is an exception the program can catch, with the bridge's own type; uncaught, it ends the program with that type.</summary>
    [Fact]
    public async Task FailedCommand_ThrowsIntoTheProgram_WithTheBridgesType()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Failed(new BridgeError("NotFound", "no such layer")));

        var caught = await Programs(bridge).RunAsync("try { ae.layer_info({ layer: 'x' }); } catch (e) { return e.type + ':' + e.message; }", null, TimeSpan.FromSeconds(10), CancellationToken.None);
        var uncaught = await Programs(bridge).RunAsync("ae.layer_info({ layer: 'x' }); return 1;", null, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Equal("NotFound:no such layer", caught.Result!.ToString());
        Assert.Equal("NotFound", uncaught.Error!.Type);
        Assert.Single(uncaught.Calls);
    }

    [Fact]
    public async Task Program_StopsAfterTheCallLimit_AndTheCallsSoFarStand()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await Programs(bridge).RunAsync("for (let i = 0; i < 1000; i++) { ae.ping({}); } return 'done';", null, TimeSpan.FromSeconds(10), CancellationToken.None);

        Assert.Equal("TooManyCalls", run.Error!.Type);
        Assert.Equal(JavaScriptPrograms.MostCalls, run.Calls.Count);
    }

    [Fact]
    public async Task Program_ThatRunsTooLong_IsStopped()
    {
        var run = await Programs(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()))).RunAsync("while (true) {}", null, TimeSpan.FromMilliseconds(300), CancellationToken.None);

        Assert.Equal(BridgeError.TimeoutType, run.Error!.Type);
    }

    [Fact]
    public async Task Program_CannotReachTheHost()
    {
        var run = await Programs(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()))).RunAsync("return typeof System + '/' + typeof require + '/' + typeof fetch;", null, TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.Equal("undefined/undefined/undefined", run.Result!.ToString());
    }

    /// <summary>A server with scripts switched off must not run them by way of one line of JavaScript.</summary>
    [Fact]
    public async Task Program_WithScriptsOff_IsRefusedTheScriptCommand()
    {
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var run = await Programs(bridge, ScriptAccess.Off).RunAsync("return ae.script({ code: '1' });", null, TimeSpan.FromSeconds(5), CancellationToken.None);

        Assert.Equal("ScriptingDisabled", run.Error!.Type);
        Assert.Empty(bridge.Sent);
    }

    private static JavaScriptPrograms Programs(StubBridge bridge, ScriptAccess scripting = ScriptAccess.Allow) =>
        new(bridge, new ServerConfig { Scripting = scripting }, Scopes, Clients);
}
