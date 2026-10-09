using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Adapters.Bridge;

/// <summary>The bridges connected to this server, one per After Effects, and the one commands go to: the worker the calling client is bound to,
/// else the desk, else the newest welcomed that is still alive.</summary>
/// <remarks>Every bridge names itself in its hello: the desk is the After Effects a person works in, a worker is one the server launched. A
/// bridge that reconnects under a name already held replaces the older link, since a restarted bridge reconnects before its old socket is
/// noticed dead. A client bound to a worker that is gone gets an unavailable answer rather than someone else's After Effects.</remarks>
internal sealed class BridgeLinks(BridgeOptions options, ClientSessions? clients = null) : IAfterEffectsBridge, IAsyncDisposable
{
    private readonly List<BridgeLink> _links = [];
    private readonly Lock _gate = new();

    public bool IsConnected => Current is not null;

    public BridgeHello? Hello => Current?.Hello;

    public IReadOnlyList<BridgeHello> Connected
    {
        get
        {
            lock (_gate)
            {
                _links.RemoveAll(link => !link.IsAlive);

                return [.. _links.Select(link => link.Hello)];
            }
        }
    }

    /// <summary>The link commands go to right now: the calling client's worker when it is bound to one, else the desk, else the newest; null
    /// while none fits.</summary>
    public BridgeLink? Current => Named(clients?.Current.Worker);

    /// <summary>The live link of a worker by name; with no name, the desk or the newest live link.</summary>
    public BridgeLink? Named(string? worker)
    {
        lock (_gate)
        {
            _links.RemoveAll(link => !link.IsAlive);

            if (worker is not null)
            {
                return _links.LastOrDefault(link => string.Equals(link.Hello.Worker, worker, StringComparison.OrdinalIgnoreCase));
            }

            return _links.LastOrDefault(link => string.Equals(link.Hello.Worker, BridgeHello.Desk, StringComparison.OrdinalIgnoreCase))
                ?? (_links.Count == 0 ? null : _links[^1]);
        }
    }

    public void Admit(BridgeLink link)
    {
        List<BridgeLink> replaced;

        lock (_gate)
        {
            replaced = [.. _links.Where(held => string.Equals(held.Hello.Worker, link.Hello.Worker, StringComparison.OrdinalIgnoreCase))];
            _links.RemoveAll(replaced.Contains);
            _links.Add(link);
        }

        foreach (var old in replaced)
        {
            _ = old.DisposeAsync().AsTask();
        }
    }

    public Task<BridgeReply> SendAsync(BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var worker = clients?.Current.Worker;

        if (Named(worker) is not { } link)
        {
            return Task.FromResult(BridgeReply.Failed(worker is null ? BridgeError.Unavailable() : BridgeError.WorkerGone(worker)));
        }

        return link.SendAsync(command, Signed(parameters ?? []), timeout ?? TimeSpan.FromSeconds(options.RequestTimeoutSeconds), cancellationToken);
    }

    public bool IsConnectedTo(string worker) => Named(worker) is not null;

    public Task<BridgeReply> SendToAsync(string worker, BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        if (Named(worker) is not { } link)
        {
            return Task.FromResult(BridgeReply.Failed(BridgeError.WorkerGone(worker)));
        }

        return link.SendAsync(command, Signed(parameters ?? []), timeout ?? TimeSpan.FromSeconds(options.RequestTimeoutSeconds), cancellationToken);
    }

    /// <summary>Adds the agent name to the params: the calling client's over HTTP, the configured one otherwise; tools build a fresh object per
    /// call, so the object is written to rather than copied.</summary>
    private JsonObject Signed(JsonObject parameters)
    {
        var agent = clients?.Current.Agent ?? options.Agent;

        if (!string.IsNullOrWhiteSpace(agent))
        {
            parameters["agent"] = agent;
        }

        return parameters;
    }

    public async ValueTask DisposeAsync()
    {
        List<BridgeLink> links;

        lock (_gate)
        {
            links = [.. _links];
            _links.Clear();
        }

        foreach (var link in links)
        {
            await link.DisposeAsync().ConfigureAwait(false);
        }
    }
}
