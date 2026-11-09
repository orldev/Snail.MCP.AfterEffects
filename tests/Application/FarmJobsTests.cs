using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Configuration;
using Snail.Sdk.Mcp.Hosting.Jobs;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>A render of the farm followed as a job of the server: its frames as progress, the farm's account as the result, the farm's failure as the job's.</summary>
public sealed class FarmJobsTests : IAsyncDisposable
{
    private readonly string _root = TestFolder.Create();
    private RenderFarm? _farm;

    private static readonly string[] Frames =
    [
        "PROGRESS: Adding specified comp to Render Queue",
        "PROGRESS:  0:00:00:00 (1): 0 seconds",
        "PROGRESS:  0:00:00:01 (2): 0 seconds",
        "PROGRESS:  0:00:00:02 (3): 0 seconds",
        "PROGRESS:  Total Time Elapsed: 6 seconds",
    ];

    [Fact]
    public async Task Submit_FollowsTheRender_AndEndsWithTheFarmsAccount()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess(Frames, 0, TimeSpan.FromMilliseconds(30)));
        var jobs = await JobsAsync(aerender);

        var job = jobs.Submit(new JobRequest("/tmp/p.aep", "Main", "/tmp/out/Main", StartFrame: 0, EndFrame: 2, OutputTemplate: "PNG Sequence"));
        var report = await job.Completion.WaitAsync(TimeSpan.FromSeconds(15));

        Assert.StartsWith("job://aftereffects/render/", job.Uri, StringComparison.Ordinal);
        Assert.Equal(JobState.Succeeded, report.State);
        Assert.Equal(1, report.Progress);
        Assert.Equal(JobStates.Done, report.Result!.Value.GetProperty("state").GetString());
        Assert.Equal(3, report.Result.Value.GetProperty("progress").GetProperty("frames_rendered").GetInt32());
    }

    [Fact]
    public async Task Submit_OfARenderThatFails_EndsTheJobFailed_WithTheReason()
    {
        var aerender = new FakeAerender(_ => new FakeAerender.FakeRenderProcess(["aerender ERROR: After Effects error: file not found"], 1));
        var jobs = await JobsAsync(aerender);

        var job = jobs.Submit(new JobRequest("/tmp/p.aep", "Main", "/tmp/out/Main", Retries: 0));
        var report = await job.Completion.WaitAsync(TimeSpan.FromSeconds(15));

        Assert.Equal(JobState.Failed, report.State);
        Assert.Contains("file not found", report.Message, StringComparison.Ordinal);
        Assert.Equal(FailureTypes.RenderFailed, report.Type);
    }

    private async Task<FarmJobs> JobsAsync(FakeAerender aerender)
    {
        var config = new ServerConfig { DataDirectory = _root, FarmParallel = 1 };
        _farm = new RenderFarm(aerender, config, TimeProvider.System, NullLogger<RenderFarm>.Instance);
        await _farm.StartAsync(CancellationToken.None);

        return new FarmJobs(_farm, new Jobs("aftereffects"), TimeProvider.System) { Interval = TimeSpan.FromMilliseconds(50) };
    }

    public async ValueTask DisposeAsync()
    {
        if (_farm is not null)
        {
            await _farm.StopAsync(CancellationToken.None);
            _farm.Dispose();
        }

        Directory.Delete(_root, recursive: true);
    }
}
