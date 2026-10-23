using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Adapters;

/// <summary>What the link does with what comes back: a line that is not a reply, a reply nobody waits for, a reply that does not say whether it
/// worked, and a connection that goes away mid-request. Every one of these happened to a bridge inside After Effects, which writes its lines on
/// the application's main thread between the application's own work.</summary>
public sealed class BridgeLinkTests
{
    /// <summary>A line that is not JSON is logged and stepped over: After Effects has written a warning onto the socket before now, and one
    /// such line must not cost the request that follows it.</summary>
    [Fact]
    public async Task ALineThatIsNotAReply_IsSteppedOver()
    {
        await using var stack = await ListeningServer.StartAsync();
        FakeBridge? bridge = null;
        bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, async request =>
        {
            await bridge!.SayAsync("this is not JSON at all");
            await bridge!.SayAsync(string.Empty);

            return FakeBridge.Ok(request, new JsonObject { ["ticks"] = 9 });
        });
        await using var connected = bridge;
        await stack.UntilConnectedAsync();

        var reply = await stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(10));

        Assert.True(reply.IsOk);
        Assert.Equal(9, reply.Result!["ticks"]!.GetValue<int>());
    }

    /// <summary>A reply that does not say whether the command worked is malformed, and the answer carries the line so the reader can see what
    /// the bridge actually sent.</summary>
    [Fact]
    public async Task AReplyThatDoesNotSayWhetherItWorked_IsMalformed()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value,
            FakeBridge.Always(request => new JsonObject { ["id"] = request["id"]!.DeepClone(), ["result"] = new JsonObject() }));
        await stack.UntilConnectedAsync();

        var reply = await stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(10));

        Assert.Equal(BridgeError.MalformedType, reply.Error!.Type);
    }

    /// <summary>A failure with no message still says something: the type the bridge gave, and a sentence saying the message was missing.</summary>
    [Fact]
    public async Task AFailureWithoutAMessage_IsAnsweredWithItsTypeAndASentence()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value,
            FakeBridge.Always(request => new JsonObject { ["id"] = request["id"]!.DeepClone(), ["ok"] = false, ["error"] = new JsonObject { ["type"] = "Busy" } }));
        await stack.UntilConnectedAsync();

        var named = await stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(10));

        Assert.Equal("Busy", named.Error!.Type);
        Assert.Contains("without a message", named.Error.Message, StringComparison.Ordinal);
    }

    /// <summary>A failure with nothing in it at all is still a failure, named Error rather than passed off as a success.</summary>
    [Fact]
    public async Task AFailureWithNoErrorAtAll_IsStillAFailure()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value,
            FakeBridge.Always(request => new JsonObject { ["id"] = request["id"]!.DeepClone(), ["ok"] = false }));
        await stack.UntilConnectedAsync();

        var reply = await stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(10));

        Assert.False(reply.IsOk);
        Assert.Equal("Error", reply.Error!.Type);
    }

    /// <summary>A command that worked and had nothing to say comes back as a success with no result, which a tool answers with on its own.</summary>
    [Fact]
    public async Task ASuccessWithNothingToSay_ComesBackAsASuccess()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value,
            FakeBridge.Always(request => new JsonObject { ["id"] = request["id"]!.DeepClone(), ["ok"] = true }));
        await stack.UntilConnectedAsync();

        var reply = await stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(10));

        Assert.True(reply.IsOk);
        Assert.Null(reply.Result);
    }

    /// <summary>A number that is not a number cannot be written as JSON, and the request is refused here rather than sent half-written: the
    /// bridge reads one line per request, and a line that ends in the middle would cost every request behind it.</summary>
    [Fact]
    public async Task ParametersThatCannotBeWrittenAsJson_AreRefusedBeforeAnythingIsSent()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())));
        await stack.UntilConnectedAsync();

        var reply = await stack.Links.SendAsync(BridgeCommands.LayerTransform, new JsonObject { ["opacity"] = double.NaN }, TimeSpan.FromSeconds(10));

        Assert.False(reply.IsOk);
        Assert.Empty(bridge.Requests);
    }

    /// <summary>A bridge that goes away while a command is in flight leaves the caller with an answer rather than a wait: After Effects quits
    /// mid-render often enough that this is the ordinary case, not the strange one.</summary>
    [Fact]
    public async Task ABridgeThatGoesAwayMidRequest_EndsTheWait()
    {
        await using var stack = await ListeningServer.StartAsync();
        var gone = new TaskCompletionSource();
        var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(_ =>
        {
            gone.TrySetResult();

            return null;
        }));
        await stack.UntilConnectedAsync();

        var asking = stack.Links.SendAsync(BridgeCommands.Ping, [], TimeSpan.FromSeconds(30));
        await gone.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await bridge.DisposeAsync();

        var reply = await asking.WaitAsync(TimeSpan.FromSeconds(15));

        Assert.False(reply.IsOk);
        Assert.True(reply.Error!.Type is BridgeError.DroppedType or BridgeError.UnavailableType);
    }
}
