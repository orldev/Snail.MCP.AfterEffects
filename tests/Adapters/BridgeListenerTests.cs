using Snail.MCP.AfterEffects.Application.Announcements;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Adapters;

public class BridgeListenerTests
{
    [Fact]
    public async Task Bridge_WithTheRightToken_IsWelcomed_AndItsAnswersComeBack()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["pong"] = true })));

        await stack.UntilConnectedAsync();
        var reply = await stack.Links.SendAsync(BridgeCommands.Ping);

        Assert.Equal("s3l-mcp-aftereffects", bridge.WelcomedBy);
        Assert.True(reply.IsOk);
        Assert.True(reply.Result!["pong"]!.GetValue<bool>());
        Assert.Equal("26.5x89", stack.Links.Hello!.App);
    }

    /// <summary>Anything on the machine can connect to the port; only the token in the user's own data directory makes it the bridge.</summary>
    [Fact]
    public async Task Bridge_WithAWrongToken_IsRefused_AndOpensNoLink()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, "not-the-token", FakeBridge.Always(request => null));

        await Task.Delay(100);

        Assert.Equal("token", bridge.Refusal);
        Assert.Null(bridge.WelcomedBy);
        Assert.False(stack.Links.IsConnected);
    }

    [Fact]
    public async Task Connection_ThatIsNotABridge_IsRefused()
    {
        await using var stack = await ListeningServer.StartAsync();
        using var stranger = new System.Net.Sockets.TcpClient();
        await stranger.ConnectAsync("127.0.0.1", stack.Port);
        await stranger.GetStream().WriteAsync("GET / HTTP/1.1\n"u8.ToArray());

        using var reader = new StreamReader(stranger.GetStream());
        var answer = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Contains("refused", answer, StringComparison.Ordinal);
        Assert.False(stack.Links.IsConnected);
    }

    /// <summary>A reply that arrives after its caller gave up is dropped by its id; the next request gets its own answer, never the stale one.</summary>
    [Fact]
    public async Task Reply_AfterTheTimeout_IsDropped_AndTheNextCallGetsItsOwnAnswer()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, async request =>
        {
            if (request["id"]!.ToString() == "1") await Task.Delay(400);

            return FakeBridge.Ok(request, new JsonObject { ["answered"] = request["id"]!.ToString() });
        });

        await stack.UntilConnectedAsync();
        var late = await stack.Links.SendAsync(BridgeCommands.Ping, timeout: TimeSpan.FromMilliseconds(100));
        var next = await stack.Links.SendAsync(BridgeCommands.AppInfo);

        Assert.Equal(BridgeError.TimeoutType, late.Error!.Type);
        Assert.True(next.IsOk);
        Assert.Equal("2", next.Result!["answered"]!.ToString());
    }

    [Fact]
    public async Task Bridge_ThatDisconnects_LeavesNoLink_AndCallsAreUnavailable()
    {
        await using var stack = await ListeningServer.StartAsync();
        var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())));
        await stack.UntilConnectedAsync();

        await bridge.DisposeAsync();
        await stack.UntilDisconnectedAsync();
        var reply = await stack.Links.SendAsync(BridgeCommands.Ping);

        Assert.Equal(BridgeError.UnavailableType, reply.Error!.Type);
    }

    /// <summary>The bridge finds the server through its announcement, so the file names the port while the listener runs and is gone once it stops.</summary>
    [Fact]
    public async Task Listener_AnnouncesItsPort_WhileItRuns_AndWithdrawsItWhenItStops()
    {
        var stack = await ListeningServer.StartAsync();
        var file = Path.Combine(stack.DataDirectory, "state", ServerAnnouncement.FolderName, ServerAnnouncement.FileName(Environment.ProcessId));
        var announced = JsonNode.Parse(File.ReadAllText(file))!;

        Assert.Equal(stack.Port, announced["port"]!.GetValue<int>());
        Assert.True(File.Exists(stack.Config.Bridge.TokenFile));

        await stack.Listener.StopAsync(CancellationToken.None);

        Assert.False(File.Exists(file));

        await stack.DisposeAsync();
    }

    /// <summary>A bridge that reconnects, or a second After Effects, is the one the user is looking at: commands go to the newest live link.</summary>
    [Fact]
    public async Task TheNewestLiveBridge_IsTheOneCommandsGoTo()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var first = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "first" })));
        await stack.UntilConnectedAsync();
        await using var second = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "second" })), app: "27.0x1");
        await ListeningServer.Until(() => stack.Links.Hello?.App == "27.0x1", TimeSpan.FromSeconds(5));

        var reply = await stack.Links.SendAsync(BridgeCommands.Ping);

        Assert.Equal("second", reply.Result!["who"]!.ToString());
    }

    /// <summary>Bridges name themselves: an unbound client reaches the desk even when a worker connected later, a name picks that worker, and a
    /// bridge reconnecting under a held name replaces the older link.</summary>
    [Fact]
    public async Task Bridges_AreToldApartByWorkerName_AndTheDeskIsTheDefault()
    {
        await using var stack = await ListeningServer.StartAsync();
        await using var desk = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "desk" })));
        await stack.UntilConnectedAsync();
        await using var worker = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "w1" })), worker: "w1");
        await ListeningServer.Until(() => stack.Links.Connected.Count == 2, TimeSpan.FromSeconds(5));

        Assert.Equal("desk", (await stack.Links.SendAsync(BridgeCommands.Ping)).Result!["who"]!.ToString());
        Assert.Equal("w1", stack.Links.Named("w1")!.Hello.Worker);
        Assert.Null(stack.Links.Named("w2"));

        await using var again = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "w1 again" })), worker: "w1");
        await ListeningServer.Until(() => stack.Links.Named("w1")?.Hello.Instance == again.Instance, TimeSpan.FromSeconds(5));

        Assert.Equal(2, stack.Links.Connected.Count);
        Assert.Equal(["desk", "w1"], [.. stack.Links.Connected.Select(hello => hello.Worker).Order()]);
    }

    /// <summary>The agent name rides in every request's params, which is what the bridge's journal will show.</summary>
    [Fact]
    public async Task Requests_CarryTheAgent_AndTheDeadline()
    {
        await using var stack = await ListeningServer.StartAsync(agent: "titles");
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())));
        await stack.UntilConnectedAsync();

        await stack.Links.SendAsync(BridgeCommands.Ping, timeout: TimeSpan.FromSeconds(12));
        var request = bridge.Requests.Single();

        Assert.Equal("titles", request["params"]!["agent"]!.ToString());
        Assert.Equal(12, request["deadline_s"]!.GetValue<double>());
        Assert.True(request["deadline_at"]!.GetValue<long>() > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
}
