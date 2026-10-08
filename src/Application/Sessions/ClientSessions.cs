using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Skills;

namespace Snail.MCP.AfterEffects.Application.Sessions;

/// <summary>One client of this server: the name its commands are signed with, the skills it alone has loaded, and what its key opens.</summary>
/// <remarks>A session owns its tool collection, so loading a skill changes the tool list of the client that asked and of nobody else.</remarks>
internal sealed class ClientSession(string name, string? agent, SkillCatalog skills, IReadOnlyList<string>? scopes = null)
{
    public string Name { get; } = name;

    /// <summary>The agent name riding on this client's commands, or none when neither the client nor the configuration gave one.</summary>
    public string? Agent { get; private set; } = agent;

    public SkillCatalog Skills { get; } = skills;

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

/// <summary>Every client this server is serving, one session each, and which of them the call in hand belongs to.</summary>
/// <remarks>A stdio server serves one client and hands it the process's own session. Over HTTP each MCP session opens one of its own, and the
/// call being served is found through the session's tool collection, which the SDK hands back with every request. Everything downstream, the
/// skills a tool sees and the agent a command is signed with, asks for the session in hand rather than for the process's one.</remarks>
internal sealed class ClientSessions(IReadOnlyList<Skill> skills, McpServerPrimitiveCollection<McpServerTool> alwaysOn, string? agent, IServiceProvider services, TimeProvider time)
{
    /// <summary>Clients that may be told apart by name, which is the only way to recognise one that holds no session.</summary>
    public const int MostNamedClients = 64;

    private readonly ConditionalWeakTable<McpServerPrimitiveCollection<McpServerTool>, ClientSession> _sessions = [];
    private readonly ConcurrentDictionary<string, ClientSession> _named = new(StringComparer.OrdinalIgnoreCase);
    private readonly AsyncLocal<ClientSession?> _serving = new();
    private readonly IReadOnlyList<Skill> _skills = skills;
    private readonly IServiceProvider _services = services;
    private readonly TimeProvider _time = time;

    /// <summary>The session of a server that opened none: the stdio process, and any caller outside a request.</summary>
    public ClientSession Default { get; } = new ClientSession("this server", Called(agent), new SkillCatalog(skills, alwaysOn, services, time));

    /// <summary>Every session open now, the process's own among them.</summary>
    public IReadOnlyList<ClientSession> All => [Default, .. _sessions.Select(entry => entry.Value)];

    /// <summary>The session of the call in hand, or the process's own when there is no call.</summary>
    public ClientSession Current => _serving.Value ?? Default;

    /// <summary>Opens a session for a client of its own, with a tool collection holding the always-on tools and nothing else.</summary>
    /// <remarks>A client of the sessionless protocol revision is handed fresh options on every request, so it is recognised by the name it
    /// gives and handed back the session of that name. A client that gives no name cannot be told from any other, so it is served the tool list
    /// of the server itself. Past the number of names kept, a client gets a session of its own scopes and shares the server's tool list.</remarks>
    public ClientSession Open(McpServerOptions session, string? agent, IReadOnlyList<string>? scopes = null)
    {
        if (Called(agent) is not { } name)
        {
            session.ToolCollection = Default.Skills.Tools;

            return Default;
        }

        var opened = _named.Count >= MostNamedClients && !_named.ContainsKey(name)
            ? new ClientSession(name, name, Default.Skills, scopes)
            : _named.GetOrAdd(name, called => Fresh(called, scopes));

        opened.Opens(scopes);
        session.ToolCollection = opened.Skills.Tools;

        return opened;
    }

    private ClientSession Fresh(string name, IReadOnlyList<string>? scopes)
    {
        var alwaysOn = new McpServerPrimitiveCollection<McpServerTool>();

        foreach (var tool in Default.Skills.Tools)
        {
            alwaysOn.TryAdd(tool);
        }

        var opened = new ClientSession(name, name, new SkillCatalog(_skills, alwaysOn, _services, _time), scopes);

        _sessions.AddOrUpdate(alwaysOn, opened);

        return opened;
    }

    /// <summary>Makes the session of this server the one the call in hand belongs to, until the returned handle is disposed.</summary>
    public IDisposable Serve(McpServer? server)
    {
        var session = For(server);

        session.NameItself(server?.ClientInfo?.Name);

        return Serve(session);
    }

    public IDisposable Serve(ClientSession session)
    {
        _serving.Value = session;

        return new Served(this);
    }

    /// <summary>The session a server belongs to; one this server never opened, a test or a transport of its own, is the process's.</summary>
    public ClientSession For(McpServer? server) =>
        server?.ServerOptions.ToolCollection is { } tools && _sessions.TryGetValue(tools, out var session) ? session : Default;

    private static string? Called(string? name) => string.IsNullOrWhiteSpace(name) ? null : name.Trim();

    private sealed class Served(ClientSessions sessions) : IDisposable
    {
        public void Dispose() => sessions._serving.Value = null;
    }
}
