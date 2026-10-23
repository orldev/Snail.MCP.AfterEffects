using System.Formats.Tar;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Application.Workers;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Chaos;

/// <summary>The crash tests of the audit, kept as regression: each one broke something when it was written, and now holds the fix in
/// place. What each one proved is in its own summary.</summary>
public sealed class ChaosTests : IDisposable
{
    private readonly string _root = TestFolder.Create();

    /// <summary>Two clients leasing at the same moment must not be handed the same After Effects: a worker is the one thing a client is
    /// promised to have to itself, and two agents in one application overwrite each other's project. Before the fix both were handed w1,
    /// because choosing a worker and taking it were two critical sections with a wait between them.</summary>
    [Fact]
    public async Task Lease_AskedByTwoClientsAtOnce_HandsTheWorkerToOneOfThem()
    {
        await using var stack = await ListeningServer.StartAsync();
        var launcher = new FakeLauncher();
        var (pool, clients) = Pool(stack, launcher, max: 1);
        var first = clients.Open(new McpServerOptions(), "titles");
        var second = clients.Open(new McpServerOptions(), "credits");

        var leasing = Task.WhenAll(
            pool.LeaseAsync(first, null, launch: true, CancellationToken.None),
            pool.LeaseAsync(second, null, launch: true, CancellationToken.None));

        await ListeningServer.Until(() => launcher.Launched.Count == 1, TimeSpan.FromSeconds(5));
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, FakeBridge.Always(request => FakeBridge.Ok(request, new JsonObject())), worker: "w1");
        var leased = await leasing.WaitAsync(TimeSpan.FromSeconds(15));

        var held = new[] { first.Worker, second.Worker }.Count(worker => worker is not null);

        Assert.Single(pool.Workers);
        Assert.Equal(1, held);
        Assert.Equal(1, leased.Count(lease => lease.Worker?.State == WorkerStates.Leased));
    }

    /// <summary>The last line of defence of the areas holds on its own: a path that climbs into a folder whose name merely begins with the
    /// area's is outside the area, whatever the caller checked before. Before the fix the comparison left the separator out, so
    /// <c>files-of-someone-else</c> passed for a folder inside <c>files</c>.</summary>
    [Fact]
    public void Volume_AskedForAFolderBesideTheArea_RefusesIt()
    {
        var volume = Storage();
        var beside = Path.Combine(_root, "files-of-someone-else");
        Directory.CreateDirectory(beside);
        File.WriteAllText(Path.Combine(beside, "secret.txt"), "not yours");

        Assert.Throws<UnauthorizedAccessException>(() => volume.List(VolumeAreas.Files, "../files-of-someone-else"));
        Assert.Throws<UnauthorizedAccessException>(() => volume.List(VolumeAreas.Files, "../captures"));
    }

    /// <summary>An archive that ends in the middle of a file leaves nothing behind: a tar reader hands over the bytes it has without a word,
    /// and before the fix the half of a file was moved to its final name and served to the next reader as if it were whole.</summary>
    [Fact]
    public async Task UnpackTar_FromAnArchiveThatEndsMidFile_PublishesNothing()
    {
        var volume = Storage();
        var whole = new MemoryStream();

        await using (var writer = new TarWriter(whole, TarEntryFormat.Pax, leaveOpen: true))
        {
            var entry = new PaxTarEntry(TarEntryType.RegularFile, "plate.mov") { DataStream = new MemoryStream(new byte[256 * 1024]) };

            await writer.WriteEntryAsync(entry);
        }

        var truncated = new MemoryStream(whole.ToArray()[..(64 * 1024)]);
        var unpacked = await volume.UnpackTarAsync(VolumeAreas.Files, string.Empty, truncated, overwrite: false, CancellationToken.None);

        Assert.False(unpacked.IsOk);
        Assert.Empty(unpacked.Files);

        var leftovers = Directory.Exists(volume.RootOf(VolumeAreas.Files))
            ? Directory.GetFiles(volume.RootOf(VolumeAreas.Files), "*", SearchOption.AllDirectories)
            : [];

        Assert.True(leftovers.Length == 0, string.Join(", ", leftovers));
    }

    /// <summary>Many commands in flight over one link keep their own answers: the replies come back in another order than the requests went
    /// out, and every caller must still get the one it asked for. This one passed the day it was written; it stays to keep that true.</summary>
    [Fact]
    public async Task Link_WithManyCommandsInFlight_GivesEveryCallerItsOwnAnswer()
    {
        await using var stack = await ListeningServer.StartAsync();
        var slower = new Random(7);
        await using var bridge = await FakeBridge.ConnectAsync(stack.Port, stack.Token.Value, async request =>
        {
            await Task.Delay(slower.Next(1, 25));

            return FakeBridge.Ok(request, new JsonObject { ["echo"] = request["params"]!["agent_note"]!.ToString() });
        });
        await stack.UntilConnectedAsync();

        var asked = await Task.WhenAll(Enumerable.Range(0, 60).Select(async number =>
        {
            var reply = await stack.Links.SendAsync(BridgeCommands.Ping, new JsonObject { ["agent_note"] = number.ToString() }, TimeSpan.FromSeconds(30));

            return (Number: number.ToString(), Answer: reply.Result?["echo"]?.ToString());
        }));

        Assert.All(asked, pair => Assert.Equal(pair.Number, pair.Answer));
    }

    private Volume Storage()
    {
        var config = new ServerConfig { DataDirectory = _root, Transport = Transport.Http };
        ServerConfigExtensions.Normalize(config);
        var clients = new ClientSessions([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);
        var farm = new RenderFarm(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0), executable: null), config, clients, TimeProvider.System, NullLogger<RenderFarm>.Instance);

        return new Volume(config, farm);
    }

    private static (WorkerPool Pool, ClientSessions Clients) Pool(ListeningServer stack, FakeLauncher launcher, int max)
    {
        stack.Config.Workers = new WorkersOptions { Max = max, Min = 0, IdleMinutes = 0, StartupSeconds = 30 };
        var clients = new ClientSessions([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);
        var pool = new WorkerPool(launcher, stack.Links, stack.Config, clients, TimeProvider.System, NullLogger<WorkerPool>.Instance);

        return (pool, clients);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_root, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
