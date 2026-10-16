using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Application.Workers;

/// <summary>The After Effects instances this server lends out: the ones it launches, named w1, w2 and so on, and the ones somebody else
/// started under a name, which it adopts when their bridge connects.</summary>
/// <remarks>A worker is ready once its bridge connects under its name; a client leases it and every command of that client goes there until
/// it releases it. The pool keeps at least the configured minimum of its own running, never launches more than the maximum, and quits a
/// free one that sat idle longer than the configured minutes, which the worker's own bridge also does on its side. An adopted worker is
/// leased and stopped like any other, but nobody here relaunches it or quits it for idleness: whoever started it, a service or a
/// container, owns its life. The desk, the After Effects a person started, is never part of the pool.</remarks>
internal sealed class WorkerPool(IWorkerLauncher launcher, IAfterEffectsBridge bridge, ServerConfig config, ClientSessions clients, TimeProvider clock, ILogger<WorkerPool> logger)
    : BackgroundService
{
    /// <summary>How often the pool looks at its workers.</summary>
    public static readonly TimeSpan Every = TimeSpan.FromSeconds(2);

    /// <summary>How long a gone worker stays in the list, so a client learns why it is gone.</summary>
    public static readonly TimeSpan GoneKept = TimeSpan.FromMinutes(5);

    /// <summary>How long a worker that lost its bridge while its process lives is given to reconnect.</summary>
    public static readonly TimeSpan ReconnectWithin = TimeSpan.FromSeconds(60);

    private readonly List<Worker> _workers = [];
    private readonly Lock _gate = new();

    public bool IsAvailable => launcher.Executable is not null && config.Workers.Max > 0;

    public string? Executable => launcher.Executable;

    public IReadOnlyList<Worker> Workers
    {
        get
        {
            lock (_gate)
            {
                return [.. _workers];
            }
        }
    }

    public Worker? Find(string name)
    {
        lock (_gate)
        {
            return _workers.FirstOrDefault(worker => worker.IsAlive && string.Equals(worker.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>Launches one more After Effects under the given name, or the next free one; null when the pool is full or launching is off.</summary>
    public Worker? Launch(string? name = null)
    {
        lock (_gate)
        {
            Sweep();

            if (!IsAvailable || _workers.Count(worker => worker.IsAlive && worker.Origin == WorkerOrigins.Launched) >= config.Workers.Max)
            {
                return null;
            }

            var chosen = name ?? NextName();

            if (Find(chosen) is not null)
            {
                throw new InvalidOperationException($"A worker named '{chosen}' is already running.");
            }

            var environment = new Dictionary<string, string>
            {
                ["SNAIL_BRIDGE_WORKER"] = chosen,
                ["SNAIL_BRIDGE_IDLE_MINUTES"] = config.Workers.IdleMinutes.ToString(System.Globalization.CultureInfo.InvariantCulture),
                ["SNAIL_BRIDGE_DATA_ROOT"] = config.DataDirectory,
                ["SNAIL_BRIDGE_SERVER_PID"] = Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture),
            };
            var process = launcher.Launch(environment);
            var worker = new Worker(chosen, process, clock.GetUtcNow());
            _workers.Add(worker);
            Log.WorkerLaunched(logger, chosen, process.Pid);

            return worker;
        }
    }

    /// <summary>Binds a client to a worker: the one named, the one it already holds, a free one, or a freshly launched one when allowed;
    /// waits for a starting worker's bridge.</summary>
    public async Task<Lease> LeaseAsync(ClientSession client, string? name, bool launch, CancellationToken cancellationToken)
    {
        Reconcile();

        if (name is not null && Find(name) is { LeasedBy: { } holder } taken && !string.Equals(holder, client.Name, StringComparison.Ordinal))
        {
            return new Lease(taken, Messages.WorkerHeld(taken.Name, holder));
        }

        var worker = Pick(client, name, launch);

        if (worker is null)
        {
            return new Lease(null, name is null || launch ? Messages.PoolFull : Messages.UnknownWorker(name));
        }

        var patience = clock.GetUtcNow() + TimeSpan.FromSeconds(config.Workers.StartupSeconds);

        while (worker.State is WorkerStates.Starting && clock.GetUtcNow() < patience)
        {
            await Task.Delay(500, cancellationToken).ConfigureAwait(false);
            Reconcile();
        }

        lock (_gate)
        {
            if (worker.State is not (WorkerStates.Ready or WorkerStates.Leased))
            {
                Unbind(client);

                return new Lease(worker, Messages.WorkerNotReady(worker.Name, worker.State));
            }

            worker.State = WorkerStates.Leased;
            worker.LastUsed = clock.GetUtcNow();
        }

        return new Lease(worker, null);
    }

    /// <summary>Gives the client's worker back to the pool; the worker it held, or null when it held none.</summary>
    public Worker? Release(ClientSession client)
    {
        lock (_gate)
        {
            var held = client.Worker is { } name ? Find(name) : null;
            Unbind(client);

            return held;
        }
    }

    /// <summary>Asks a worker to quit, dropping its project unsaved, and frees whoever held it; kills it when it does not go.</summary>
    public async Task<bool> TryStopAsync(string name, CancellationToken cancellationToken)
    {
        var worker = Find(name);

        if (worker is null)
        {
            return false;
        }

        lock (_gate)
        {
            foreach (var session in clients.All.Where(session => string.Equals(session.Worker, name, StringComparison.OrdinalIgnoreCase)))
            {
                session.Bind(null);
            }

            worker.LeasedBy = null;
            worker.State = WorkerStates.Stopping;
            worker.Note = "asked to quit";
        }

        var reply = await bridge.SendToAsync(name, BridgeCommands.AppQuit, new JsonObject { ["save"] = false }, TimeSpan.FromSeconds(15), cancellationToken).ConfigureAwait(false);

        if (!reply.IsOk && worker.Process is { } process)
        {
            Log.WorkerKilled(logger, name, reply.Error?.Message);
            process.Kill();
        }
        else if (!reply.IsOk)
        {
            lock (_gate)
            {
                Gone(worker, $"did not take the quit: {reply.Error?.Message}");
            }
        }

        return true;
    }

    /// <summary>Copies a project file into the worker's own folder under the data directory and opens the copy there, dropping whatever it had.</summary>
    public Task<BridgeReply> OpenCopyAsync(string worker, string project, CancellationToken cancellationToken)
    {
        var source = Path.GetFullPath(project);
        var folder = Path.Combine(config.DataDirectory, "workers", worker);
        Directory.CreateDirectory(folder);
        var copy = Path.Combine(folder, Path.GetFileName(source));
        File.Copy(source, copy, overwrite: true);

        return bridge.SendToAsync(worker, BridgeCommands.ProjectOpen, new JsonObject { ["path"] = copy, ["discard"] = true }, null, cancellationToken);
    }

    /// <summary>Looks at every worker once: marks the connected ready, the exited gone, launches up to the minimum and quits the idle beyond it.</summary>
    public void Reconcile()
    {
        var now = clock.GetUtcNow();
        List<string> toStop = [];

        lock (_gate)
        {
            Adopt(now);

            foreach (var worker in _workers.Where(worker => worker.IsAlive))
            {
                var connected = bridge.IsConnectedTo(worker.Name);

                if (connected)
                {
                    worker.LastSeen = now;

                    if (worker.State is WorkerStates.Starting or WorkerStates.Ready or WorkerStates.Leased)
                    {
                        worker.State = worker.LeasedBy is null ? WorkerStates.Ready : WorkerStates.Leased;
                    }

                    continue;
                }

                if (worker.Process is { HasExited: true })
                {
                    Gone(worker, worker.State is WorkerStates.Stopping ? "quit" : "the process exited");
                    continue;
                }

                var since = worker.LastSeen ?? worker.Started;
                var allowed = worker.LastSeen is null ? TimeSpan.FromSeconds(config.Workers.StartupSeconds) : ReconnectWithin;

                if (worker.Process is null && worker.State is WorkerStates.Stopping && now - since > Every)
                {
                    Gone(worker, "quit");
                    continue;
                }

                if (worker.State is not WorkerStates.Stopping && now - since > allowed)
                {
                    worker.Process?.Kill();
                    Gone(worker, worker.LastSeen is null ? "its bridge never connected" : worker.Process is null ? "its bridge went away" : "its bridge did not come back");
                }
            }

            Sweep();
            var alive = _workers.Count(worker => worker.IsAlive && worker.Origin == WorkerOrigins.Launched);

            if (config.Workers.IdleMinutes > 0)
            {
                var idle = TimeSpan.FromMinutes(config.Workers.IdleMinutes);
                toStop = [.. _workers.Where(worker => worker.IsFree && worker.Origin == WorkerOrigins.Launched && alive > config.Workers.Min && now - worker.LastUsed > idle).Select(worker => worker.Name)];
                alive -= toStop.Count;
            }

            while (IsAvailable && alive < Math.Min(config.Workers.Min, config.Workers.Max))
            {
                Launch();
                alive++;
            }
        }

        foreach (var name in toStop)
        {
            Log.WorkerIdle(logger, name);
            _ = TryStopAsync(name, CancellationToken.None);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            using var timer = new PeriodicTimer(Every);

            while (await timer.WaitForNextTickAsync(stoppingToken).ConfigureAwait(false))
            {
                try
                {
                    Reconcile();
                }
                catch (Exception failure) when (failure is InvalidOperationException or IOException)
                {
                    Log.WorkersNotReconciled(logger, failure.Message);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    /// <summary>The worker this client gets, taken for it before the lock is let go.</summary>
    /// <remarks>Choosing and taking are one step on purpose: a worker is held from the moment it is chosen, through the wait for its bridge,
    /// so that two clients asking at the same moment are never handed the same After Effects.</remarks>
    private Worker? Pick(ClientSession client, string? name, bool launch)
    {
        lock (_gate)
        {
            var chosen = Chosen(client, name, launch);

            if (chosen is null)
            {
                return null;
            }

            Unbind(client);
            client.Bind(chosen.Name);
            chosen.LeasedBy = client.Name;
            chosen.LastUsed = clock.GetUtcNow();

            if (chosen.State is WorkerStates.Ready)
            {
                chosen.State = WorkerStates.Leased;
            }

            return chosen;
        }
    }

    /// <summary>The worker a lease should get: the one named, the one this client already holds, a free one, one already on its way, or a
    /// freshly launched one when the pool has room.</summary>
    private Worker? Chosen(ClientSession client, string? name, bool launch)
    {
        if (name is not null)
        {
            return Find(name) ?? (launch ? Launch(name) : null);
        }

        if (client.Worker is { } held && Find(held) is { } own)
        {
            return own;
        }

        return _workers.FirstOrDefault(worker => worker.IsFree)
            ?? _workers.FirstOrDefault(worker => worker.State is WorkerStates.Starting && worker.LeasedBy is null)
            ?? (launch ? Launch() : null);
    }

    /// <summary>Takes in every connected bridge that names itself and is not the desk and not already a worker here.</summary>
    private void Adopt(DateTimeOffset now)
    {
        foreach (var hello in bridge.Connected)
        {
            if (string.Equals(hello.Worker, BridgeHello.Desk, StringComparison.OrdinalIgnoreCase) || Find(hello.Worker) is not null)
            {
                continue;
            }

            var adopted = new Worker(hello.Worker, null, now) { State = WorkerStates.Ready, LastSeen = now };
            _workers.Add(adopted);
            Log.WorkerAdopted(logger, hello.Worker);
        }
    }

    private void Unbind(ClientSession client)
    {
        if (client.Worker is { } name && Find(name) is { } held && string.Equals(held.LeasedBy, client.Name, StringComparison.Ordinal))
        {
            held.LeasedBy = null;
            held.State = held.State is WorkerStates.Leased ? WorkerStates.Ready : held.State;
            held.LastUsed = clock.GetUtcNow();
        }

        client.Bind(null);
    }

    private void Gone(Worker worker, string why)
    {
        worker.State = WorkerStates.Gone;
        worker.Note = why;
        worker.LeasedBy = null;
        worker.LastUsed = clock.GetUtcNow();
        worker.Process?.Dispose();
        Log.WorkerGone(logger, worker.Name, why);

        foreach (var session in clients.All.Where(session => string.Equals(session.Worker, worker.Name, StringComparison.OrdinalIgnoreCase)))
        {
            session.Bind(null);
        }
    }

    private void Sweep()
    {
        var now = clock.GetUtcNow();
        _workers.RemoveAll(worker => !worker.IsAlive && now - worker.LastUsed > GoneKept);
    }

    private string NextName()
    {
        for (var i = 1; ; i++)
        {
            var candidate = $"w{i}";

            if (Find(candidate) is null)
            {
                return candidate;
            }
        }
    }
}
