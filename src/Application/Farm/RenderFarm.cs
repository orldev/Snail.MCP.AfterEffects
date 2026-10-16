using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Farm;

/// <summary>Background renders through aerender: a queue by priority, a bounded number running at once, retries, cancellation, and logs.</summary>
/// <remarks>aerender renders a saved project in its own After Effects engine, so the open application stays free and a job outlives any tool
/// call. While a job is active the farm skill is pinned, so the tools to poll and cancel it do not unload under the model.</remarks>
internal sealed class RenderFarm(IAerender aerender, ServerConfig config, ClientSessions clients, TimeProvider clock, ILogger<RenderFarm> logger) : BackgroundService
{
    private readonly List<RenderJob> _jobs = [];
    private readonly Dictionary<string, CancellationTokenSource> _running = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();
    private readonly SemaphoreSlim _wake = new(0);
    private CancellationToken _stopping;

    public bool IsAvailable => aerender.Executable is not null;

    public string? Executable => aerender.Executable;

    public string LogDirectory => Path.Combine(config.DataDirectory, "farm");

    public IReadOnlyList<RenderJob> Jobs
    {
        get
        {
            lock (_gate)
            {
                return [.. _jobs];
            }
        }
    }

    public RenderJob? Find(string id)
    {
        lock (_gate)
        {
            return _jobs.FirstOrDefault(job => string.Equals(job.Id, id, StringComparison.OrdinalIgnoreCase));
        }
    }

    public RenderJob Submit(JobRequest request)
    {
        Directory.CreateDirectory(LogDirectory);
        var id = Guid.NewGuid().ToString("N")[..8];
        var job = new RenderJob(id, request, clock.GetUtcNow(), Path.Combine(LogDirectory, $"{id}.log"), clients.Current.Skills);

        lock (_gate)
        {
            _jobs.Add(job);
        }

        PinWhileBusy();
        Wake();

        return job;
    }

    /// <summary>Stops a job: a queued one leaves the queue, a running one is killed; false when it is unknown or already over.</summary>
    public bool TryCancel(string id)
    {
        if (Find(id) is not { } job || !job.IsActive)
        {
            return false;
        }

        lock (_gate)
        {
            if (_running.TryGetValue(job.Id, out var running))
            {
                running.Cancel();

                return true;
            }
        }

        job.Cancel(clock.GetUtcNow());
        PinWhileBusy();

        return true;
    }

    /// <summary>Queues a finished job again under a new id, same request.</summary>
    public RenderJob? Retry(string id) =>
        Find(id) is { IsActive: false } job ? Submit(job.Request) : null;

    /// <summary>Forgets finished jobs older than the age, and their logs; returns how many.</summary>
    public int Cleanup(TimeSpan olderThan)
    {
        var cutoff = clock.GetUtcNow() - olderThan;
        List<RenderJob> gone;

        lock (_gate)
        {
            gone = [.. _jobs.Where(job => !job.IsActive && (job.Finished ?? job.Submitted) <= cutoff)];
            _jobs.RemoveAll(gone.Contains);
        }

        foreach (var job in gone.Where(job => File.Exists(job.LogPath)))
        {
            File.Delete(job.LogPath);
        }

        return gone.Count;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _stopping = stoppingToken;

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await _wake.WaitAsync(stoppingToken).ConfigureAwait(false);
                StartReady();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>Starts queued jobs, highest priority and oldest first, while there is room.</summary>
    private void StartReady()
    {
        while (true)
        {
            RenderJob? next;

            lock (_gate)
            {
                if (_running.Count >= Math.Max(1, config.FarmParallel))
                {
                    return;
                }

                next = _jobs
                    .Where(job => job.State == JobStates.Queued)
                    .OrderByDescending(job => job.Request.Priority)
                    .ThenBy(job => job.Submitted)
                    .FirstOrDefault();

                if (next is null)
                {
                    return;
                }

                var cancellation = CancellationTokenSource.CreateLinkedTokenSource(_stopping);
                _running[next.Id] = cancellation;
                _ = RunAsync(next, cancellation);
            }
        }
    }

    private async Task RunAsync(RenderJob job, CancellationTokenSource cancellation)
    {
        var cancelled = false;

        try
        {
            using var process = aerender.Start(job.Request.Arguments());
            job.Start(clock.GetUtcNow(), process.Pid);
            await using var log = new StreamWriter(job.LogPath, append: true);

            try
            {
                await foreach (var line in process.ReadLinesAsync(cancellation.Token).ConfigureAwait(false))
                {
                    job.Observe(line);
                    await log.WriteLineAsync(line).ConfigureAwait(false);
                }

                job.Finish(clock.GetUtcNow(), await process.WaitForExitAsync(cancellation.Token).ConfigureAwait(false));
            }
            catch (OperationCanceledException)
            {
                process.Kill();
                job.Cancel(clock.GetUtcNow());
                cancelled = true;
            }
        }
        catch (Exception failure) when (failure is InvalidOperationException or IOException or System.ComponentModel.Win32Exception)
        {
            job.Fail(clock.GetUtcNow(), failure.Message);
        }
        finally
        {
            lock (_gate)
            {
                _running.Remove(job.Id);
            }

            cancellation.Dispose();

            if (!cancelled && job.State == JobStates.Failed && job.RetriesLeft > 0)
            {
                Log.RenderJobRetrying(logger, job.Id, job.RetriesLeft);
                job.Requeue();
            }
            else
            {
                Log.RenderJobEnded(logger, job.Id, job.State);
            }

            PinWhileBusy();
            Wake();
        }
    }

    /// <summary>Keeps the farm skill loaded for every client with an active job, and lets it go for the rest.</summary>
    private void PinWhileBusy()
    {
        List<(SkillCatalog Owner, bool Busy)> owners;

        lock (_gate)
        {
            owners = [.. _jobs
                .Select(job => job.Owner)
                .OfType<SkillCatalog>()
                .Distinct()
                .Select(owner => (owner, _jobs.Any(job => job.Owner == owner && job.IsActive)))];
        }

        foreach (var (owner, busy) in owners)
        {
            if (busy)
            {
                owner.Pin(Domain.Skills.Farm);
            }
            else
            {
                owner.Unpin(Domain.Skills.Farm);
            }
        }
    }

    /// <summary>Stops the loop and lets go of the signal that wakes it.</summary>
    public override void Dispose()
    {
        base.Dispose();
        _wake.Dispose();
    }

    /// <summary>Wakes the loop to start what is ready.</summary>
    /// <remarks>A job that ends after the farm was disposed, on its way out with the host, has no loop left to wake.</remarks>
    private void Wake()
    {
        try
        {
            _wake.Release();
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
