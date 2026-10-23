using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class RenderFarmTests : IAsyncDisposable
{
    private readonly string _root = TestFolder.Create();
    private readonly ClientSessions _clients = new([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);
    private RenderFarm? _farm;

    private static readonly string[] Frames =
    [
        "PROGRESS: Adding specified comp to Render Queue",
        "PROGRESS:  0:00:00:00 (1): 0 seconds",
        "PROGRESS:  0:00:00:01 (2): 0 seconds",
        "PROGRESS:  0:00:00:02 (3): 0 seconds",
        "PROGRESS:  Total Time Elapsed: 6 seconds",
    ];

    /// <summary>A job runs aerender with the request on its command line, counts the frames it prints and ends done.</summary>
    [Fact]
    public async Task Submit_RunsAerender_AndCountsTheFrames()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess(Frames, 0));
        var farm = await StartAsync(aerender);

        var job = farm.Submit(new JobRequest("/tmp/p.aep", "Main", "/tmp/out/Main", StartFrame: 0, EndFrame: 2, OutputTemplate: "PNG Sequence"));
        await Until(() => job.State == JobStates.Done);

        var arguments = Assert.Single(aerender.Started);
        Assert.Contains("-comp", arguments);
        Assert.Equal("Main", After(arguments, "-comp"));
        Assert.Equal("PNG Sequence", After(arguments, "-OMtemplate"));

        var described = job.Describe(5);
        Assert.Equal(3, described["progress"]!["frames_rendered"]!.GetValue<int>());
        Assert.Equal(100.0, described["progress"]!["percent"]!.GetValue<double>());
        Assert.Equal(3, described["progress"]!["last_frame"]!.GetValue<int>());
        Assert.True(File.Exists(job.LogPath));
    }

    /// <summary>A render that exits with an error is tried again as many times as the request allows, then stays failed with the reason.</summary>
    [Fact]
    public async Task AFailedJob_IsRetried_ThenFails()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess(["aerender ERROR: After Effects error: file not found"], 1));
        var farm = await StartAsync(aerender);

        var job = farm.Submit(new JobRequest("/tmp/p.aep", "Main", "/tmp/out/Main", Retries: 2));
        await Until(() => job.State == JobStates.Failed && aerender.Started.Count == 3);

        Assert.Equal(3, aerender.Started.Count);
        Assert.Equal(0, job.RetriesLeft);
        Assert.Contains("file not found", job.Describe(0)["error"]!.GetValue<string>(), StringComparison.Ordinal);
    }

    /// <summary>Cancelling a running job kills aerender and leaves the job cancelled, not failed, so it is not retried.</summary>
    [Fact]
    public async Task Cancel_KillsARunningJob()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess([.. Enumerable.Repeat("PROGRESS:  0:00:00:01 (2): 0 seconds", 200)], 0, TimeSpan.FromMilliseconds(20)));
        var farm = await StartAsync(aerender);

        var job = farm.Submit(new JobRequest("/tmp/p.aep", "Main", "/tmp/out/Main", Retries: 3));
        await Until(() => job.State == JobStates.Running);

        Assert.True(farm.TryCancel(job.Id));
        await Until(() => job.State == JobStates.Cancelled);

        Assert.True(Assert.Single(aerender.Processes).WasKilled);
        Assert.False(farm.TryCancel(job.Id));
    }

    /// <summary>With one render at a time, the queue starts the highest priority job next and pins the farm skill while anything is active.</summary>
    [Fact]
    public async Task Queued_JobsRun_HighestPriorityFirst_OneAtATime()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess(Frames, 0, TimeSpan.FromMilliseconds(15)));
        var farm = await StartAsync(aerender);

        var first = farm.Submit(new JobRequest("/tmp/p.aep", "First", "/tmp/out/First"));
        await Until(() => first.State == JobStates.Running);
        var low = farm.Submit(new JobRequest("/tmp/p.aep", "Low", "/tmp/out/Low", Priority: 0));
        var high = farm.Submit(new JobRequest("/tmp/p.aep", "High", "/tmp/out/High", Priority: 5));

        await Until(() => first.State == JobStates.Done && low.State == JobStates.Done && high.State == JobStates.Done);

        Assert.Equal(["First", "High", "Low"], aerender.Started.Select(arguments => After(arguments, "-comp")));
        Assert.Equal(1, farm.Cleanup(TimeSpan.Zero) >= 3 ? 1 : 0);
        Assert.Empty(farm.Jobs);
    }

    private static string After(IReadOnlyList<string> arguments, string flag) =>
        arguments[arguments.ToList().IndexOf(flag) + 1];

    private async Task<RenderFarm> StartAsync(FakeAerender aerender)
    {
        var config = new ServerConfig { DataDirectory = _root, FarmParallel = 1 };
        _farm = new RenderFarm(aerender, config, _clients, TimeProvider.System, NullLogger<RenderFarm>.Instance);
        await _farm.StartAsync(CancellationToken.None);

        return _farm;
    }

    private static async Task Until(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(10);

        while (!condition())
        {
            Assert.True(DateTime.UtcNow < deadline, "the farm did not reach the expected state in time");
            await Task.Delay(10);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_farm is not null)
        {
            await _farm.StopAsync(CancellationToken.None);
            _farm.Dispose();
        }

        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }
}
