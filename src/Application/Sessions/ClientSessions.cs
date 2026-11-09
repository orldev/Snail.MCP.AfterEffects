using System.Collections.Concurrent;

namespace Snail.MCP.AfterEffects.Application.Sessions;

/// <summary>One client of this server: the name its commands are signed with, the worker it is bound to, and what its key opens.</summary>
/// <remarks>Which skills a client has loaded is Snail.Sdk's (<c>WithSkills</c>), kept per MCP session; what is kept here is what
/// After Effects needs to know about the caller of a command.</remarks>
internal sealed class ClientSession(string name, string? agent, IReadOnlyList<string>? scopes = null)
{
    public string Name { get; } = name;

    /// <summary>The agent name riding on this client's commands, or none when neither the client nor the configuration gave one.</summary>
    public string? Agent { get; private set; } = agent;

    /// <summary>The kinds of work this client's key opens; a client that presented no key, over stdio or the server itself, may do anything.</summary>
    public IReadOnlyList<string> Scopes { get; private set; } = scopes ?? Domain.Scopes.All;

    public bool May(string scope) => Scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);

    /// <summary>The worker this client's commands go to; null sends them to the desk, the After Effects a person works in.</summary>
    public string? Worker { get; private set; }

    /// <summary>Binds this client to a worker, or unbinds it with null.</summary>
    public void Bind(string? worker)
    {
        Worker = string.IsNullOrWhiteSpace(worker) ? null : worker.Trim();
    }

    /// <summary>A client that named no agent of its own is named after itself once it has said who it is with initialize.</summary>
    internal void NameItself(string? clientName)
    {
        Agent ??= string.IsNullOrWhiteSpace(clientName) ? null : clientName.Trim();
    }

    /// <summary>Takes the scopes of the key just presented, which may be narrower than the ones this session was opened with.</summary>
    internal void Opens(IReadOnlyList<string>? presented)
    {
        if (presented is { Count: > 0 })
        {
            Scopes = presented;
        }
    }
}

/// <summary>Every client this server is serving, one each by the name its key carries, and which of them the call in hand belongs to.</summary>
/// <remarks>A stdio server serves one client, the process's own. Over HTTP the request names its client — the holder of the key it
/// presented (<see cref="IClientIdentity"/>) — and every request of that holder, in one MCP session or in none, is served as the same
/// client, so a worker it leased stays bound to it. Everything downstream, the agent a command is signed with and the worker it goes to,
/// asks for the client in hand rather than for the process's one.</remarks>
internal sealed class ClientSessions(string? agent, IClientIdentity? identity = null)
{
    /// <summary>Clients that may be told apart by name, which is the only way to recognise one that holds no session.</summary>
    public const int MostNamedClients = 64;

    private readonly ConcurrentDictionary<string, ClientSession> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly AsyncLocal<ClientSession?> _serving = new();

    /// <summary>The client of a server that names none: the stdio process, a request that presented no name, and any caller outside a request.</summary>
    public ClientSession Default { get; } = new ClientSession("this server", Called(agent));

    /// <summary>Every client known now, the process's own among them.</summary>
    public IReadOnlyList<ClientSession> All => [Default, .. _named.Values];

    /// <summary>The client of the call in hand, or the process's own when there is no call.</summary>
    public ClientSession Current => _serving.Value ?? Default;

    /// <summary>The client the request being served names, or the process's own when it names none or there is no request.</summary>
    public ClientSession OfRequest() =>
        identity?.Who() is { } who ? Of(who.Name, who.Scopes) : Default;

    /// <summary>The client of this name with the scopes its key opens now; past the number of names kept, a client of its own that is not remembered.</summary>
    public ClientSession Of(string? name, IReadOnlyList<string>? scopes = null)
    {
        if (Called(name) is not { } called)
        {
            return Default;
        }

        var client = _named.Count >= MostNamedClients && !_named.ContainsKey(called)
            ? new ClientSession(called, called, scopes)
            : _named.GetOrAdd(called, fresh => new ClientSession(fresh, fresh, scopes));

        client.Opens(scopes);

        return client;
    }

    /// <summary>Makes <paramref name="client"/> the one the call in hand belongs to, named after <paramref name="clientName"/> when it named no
    /// agent, until the returned handle is disposed.</summary>
    public IDisposable Serve(ClientSession client, string? clientName = null)
    {
        client.NameItself(clientName);
        _serving.Value = client;

        return new Served(this);
    }

    private static string? Called(string? name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();

    private sealed class Served(ClientSessions sessions) : IDisposable
    {
        public void Dispose() => sessions._serving.Value = null;
    }
}
