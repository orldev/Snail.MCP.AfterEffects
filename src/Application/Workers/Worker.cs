namespace Snail.MCP.AfterEffects.Application.Workers;

/// <summary>One After Effects in the pool: its name, which is what its bridge calls itself, who holds it, and its process when the pool
/// launched it; an adopted worker was started by somebody else and is known only through its bridge.</summary>
internal sealed class Worker(string name, IWorkerProcess? process, DateTimeOffset started)
{
    private readonly IWorkerProcess? _process = process;

    public string Name { get; } = name;

    /// <summary>launched by this server, or adopted when its bridge came on its own.</summary>
    public string Origin { get; } = process is null ? WorkerOrigins.Adopted : WorkerOrigins.Launched;

    public int? Pid { get; } = process?.Pid;

    public string State { get; internal set; } = WorkerStates.Starting;

    public DateTimeOffset Started { get; } = started;

    /// <summary>When its bridge was last seen connected.</summary>
    public DateTimeOffset? LastSeen { get; internal set; }

    /// <summary>When a client last took or gave it back, from which idleness is counted.</summary>
    public DateTimeOffset LastUsed { get; internal set; } = started;

    /// <summary>The client holding it, by session name; null while free.</summary>
    public string? LeasedBy { get; internal set; }

    /// <summary>Why it went, when it did.</summary>
    public string? Note { get; internal set; }

    internal IWorkerProcess? Process => _process;

    public bool IsAlive => State is not WorkerStates.Gone;

    public bool IsFree => State is WorkerStates.Ready && LeasedBy is null;

    public JsonObject Describe(DateTimeOffset now)
    {
        var out_ = new JsonObject
        {
            ["name"] = Name,
            ["state"] = State,
            ["origin"] = Origin,
            ["pid"] = Pid,
            ["uptime_seconds"] = (int)(now - Started).TotalSeconds,
            ["leased_by"] = LeasedBy,
            ["idle_seconds"] = LeasedBy is null ? (int)(now - LastUsed).TotalSeconds : null,
        };

        if (Note is not null)
        {
            out_["note"] = Note;
        }

        return out_;
    }
}
