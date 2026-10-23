using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Time.Testing;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Workers;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class WorkerPoolTests
{
    private readonly FakeTimeProvider _clock = new(new DateTimeOffset(2026, 10, 8, 12, 0, 0, TimeSpan.Zero));

    /// <summary>A lease launches a worker named for the bridge inside it, and once that bridge connects the client's commands go there.</summary>
    [Fact]
    public async Task Lease_LaunchesAWorker_AndBindsTheClient_OnceItsBridgeConnects()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, clients) = Pool(stack, launcher, max: 2);
        var client = clients.Open(new McpServerOptions(), "titles");
        var leasing = pool.LeaseAsync(client, null, launch: true, CancellationToken.None);
        await ListeningServer.Until(() => launcher.Launched.Count == 1, TimeSpan.FromSeconds(5));

        Assert.Equal("w1", launcher.Launched[0]["SNAIL_BRIDGE_WORKER"]);
        Assert.Equal(stack.DataDirectory, launcher.Launched[0]["SNAIL_BRIDGE_DATA_ROOT"]);
        Assert.Equal(Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture), launcher.Launched[0]["SNAIL_BRIDGE_SERVER_PID"]);

        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "w1" })), worker: "w1");
        var leased = (await leasing.WaitAsync(TimeSpan.FromSeconds(10))).Worker;

        Assert.Equal(WorkerStates.Leased, leased!.State);
        Assert.Equal("w1", client.Worker);
        Assert.Equal("titles", leased.LeasedBy);

        using (clients.Serve(client))
        {
            Assert.Equal("w1", (await stack.Links.SendAsync(BridgeCommands.Ping)).Result!["who"]!.ToString());
        }

        Assert.Same(leased, pool.Release(client));
        Assert.Null(client.Worker);
        Assert.Equal(WorkerStates.Ready, leased.State);
    }

    /// <summary>The pool never passes its maximum, a worker whose process ends is gone and its client is unbound, and the minimum is relaunched.</summary>
    [Fact]
    public async Task Pool_KeepsToItsLimits_AndRelaunchesTheMinimum()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, clients) = Pool(stack, launcher, max: 1, min: 1);

        pool.Reconcile();
        Assert.Single(launcher.Launched);
        Assert.Null(pool.Launch());

        var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())), worker: "w1");
        await ListeningServer.Until(() => stack.Links.IsConnectedTo("w1"), TimeSpan.FromSeconds(5));
        var client = clients.Open(new McpServerOptions(), "titles");
        var leased = (await pool.LeaseAsync(client, "w1", launch: false, CancellationToken.None)).Worker;

        Assert.Equal(WorkerStates.Leased, leased!.State);

        await bridge.DisposeAsync();
        await ListeningServer.Until(() => !stack.Links.IsConnectedTo("w1"), TimeSpan.FromSeconds(5));
        launcher.Processes[0].Exit();
        pool.Reconcile();

        Assert.Equal(WorkerStates.Gone, leased.State);
        Assert.Null(client.Worker);
        Assert.Equal(2, launcher.Launched.Count);
        Assert.Equal("w1", launcher.Launched[1]["SNAIL_BRIDGE_WORKER"]);
    }

    /// <summary>A worker whose bridge never comes is killed after the startup allowance, and a free worker past the idle allowance is asked to quit.</summary>
    [Fact]
    public async Task Pool_KillsAWorkerThatNeverConnects_AndQuitsOneThatSitsIdle()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, _) = Pool(stack, launcher, max: 2, idleMinutes: 1, startupSeconds: 30);
        var never = pool.Launch("w9")!;

        _clock.Advance(TimeSpan.FromSeconds(31));
        pool.Reconcile();

        Assert.Equal(WorkerStates.Gone, never.State);
        Assert.True(launcher.Processes[0].WasKilled);

        var idle = pool.Launch("w1")!;
        var quits = 0;
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request =>
        {
            if (request["command"]?.ToString() == "app_quit")
            {
                quits++;
            }

            return FakeBridge.Ok(request, new JsonObject());
        }), worker: "w1");
        await ListeningServer.Until(() => stack.Links.IsConnectedTo("w1"), TimeSpan.FromSeconds(5));
        pool.Reconcile();

        Assert.Equal(WorkerStates.Ready, idle.State);

        _clock.Advance(TimeSpan.FromMinutes(2));
        pool.Reconcile();
        await ListeningServer.Until(() => quits == 1, TimeSpan.FromSeconds(5));

        Assert.Equal(WorkerStates.Stopping, idle.State);
    }

    /// <summary>A named bridge nobody here launched is adopted: leasable and stoppable, never relaunched, gone when its bridge stays away.</summary>
    [Fact]
    public async Task Pool_AdoptsANamedBridgeItDidNotLaunch_AndLetsItGoWhenItLeaves()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, clients) = Pool(stack, launcher, max: 0, min: 0);
        var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject { ["who"] = "w7" })), worker: "w7");
        await ListeningServer.Until(() => stack.Links.IsConnectedTo("w7"), TimeSpan.FromSeconds(5));
        pool.Reconcile();
        var adopted = Assert.Single(pool.Workers);

        Assert.Equal("w7", adopted.Name);
        Assert.Equal(WorkerOrigins.Adopted, adopted.Origin);
        Assert.Equal(WorkerStates.Ready, adopted.State);
        Assert.Null(adopted.Pid);

        var client = clients.Open(new McpServerOptions(), "titles");
        var leased = (await pool.LeaseAsync(client, "w7", launch: false, CancellationToken.None)).Worker;

        Assert.Same(adopted, leased);
        Assert.Equal("w7", client.Worker);

        await bridge.DisposeAsync();
        await ListeningServer.Until(() => !stack.Links.IsConnectedTo("w7"), TimeSpan.FromSeconds(5));
        _clock.Advance(WorkerPool.ReconnectWithin + TimeSpan.FromSeconds(1));
        pool.Reconcile();

        Assert.Equal(WorkerStates.Gone, adopted.State);
        Assert.Null(client.Worker);
        Assert.Empty(launcher.Launched);
    }

    /// <summary>A worker belongs to the client holding it: naming it in a lease does not take it away, and the refusal says who has it. The
    /// holder itself may lease it again, which is how a client that reconnects finds its own After Effects.</summary>
    [Fact]
    public async Task Lease_ByName_DoesNotTakeAWorkerAnotherClientHolds()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, clients) = Pool(stack, launcher, max: 1);
        var holder = clients.Open(new McpServerOptions(), "titles");
        var other = clients.Open(new McpServerOptions(), "credits");
        pool.Launch("w1");
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())), worker: "w1");
        await ListeningServer.Until(() => stack.Links.IsConnectedTo("w1"), TimeSpan.FromSeconds(5));

        var held = await pool.LeaseAsync(holder, "w1", launch: false, CancellationToken.None);

        Assert.True(held.IsOk);

        var refused = await pool.LeaseAsync(other, "w1", launch: false, CancellationToken.None);

        Assert.False(refused.IsOk);
        Assert.Contains("titles", refused.Refusal!, StringComparison.Ordinal);
        Assert.Null(other.Worker);
        Assert.Equal("titles", pool.Find("w1")!.LeasedBy);

        var again = await pool.LeaseAsync(holder, "w1", launch: false, CancellationToken.None);

        Assert.True(again.IsOk);
        Assert.Equal("w1", holder.Worker);
    }

    /// <summary>A name nobody launched is said to be unknown, rather than reported as a full pool.</summary>
    [Fact]
    public async Task Lease_ByAnUnknownName_WithoutLaunching_SaysSo()
    {
        await using var stack = await ListeningServer.StartAsync();
        var (pool, clients) = Pool(stack, new FakeLauncher(), max: 1);
        var client = clients.Open(new McpServerOptions(), "titles");

        var refused = await pool.LeaseAsync(client, "w9", launch: false, CancellationToken.None);

        Assert.False(refused.IsOk);
        Assert.Contains("w9", refused.Refusal!, StringComparison.Ordinal);
        Assert.Null(refused.Worker);
    }

    private (WorkerPool Pool, ClientSessions Clients) Pool(ListeningServer stack, FakeLauncher launcher, int max, int min = 0, int idleMinutes = 0, int startupSeconds = 60)
    {
        stack.Config.Workers = new WorkersOptions { Max = max, Min = min, IdleMinutes = idleMinutes, StartupSeconds = startupSeconds };
        var services = new ServiceCollection().BuildServiceProvider();
        var clients = new ClientSessions([], [], null, services, _clock);
        var pool = new WorkerPool(launcher, stack.Links, stack.Config, clients, _clock, NullLogger<WorkerPool>.Instance);

        return (pool, clients);
    }
}
