namespace Snail.MCP.AfterEffects.Application.Workers;

/// <summary>Where a worker is in its life: launched and not yet connected, connected and free, connected and leased, asked to quit, or gone.</summary>
internal static class WorkerStates
{
    public const string Starting = "starting";

    public const string Ready = "ready";

    public const string Leased = "leased";

    public const string Stopping = "stopping";

    public const string Gone = "gone";

    public static readonly IReadOnlyList<string> All = [Starting, Ready, Leased, Stopping, Gone];
}

/// <summary>Where a worker came from: this server started it, or its bridge connected on its own and the pool took it in.</summary>
internal static class WorkerOrigins
{
    public const string Launched = "launched";

    public const string Adopted = "adopted";
}

/// <summary>What came of a lease: the worker it is about, and why it was refused when it was.</summary>
/// <remarks>A refusal names the worker too, so the answer can say what state it is in or who holds it. The rule that a worker another
/// client holds is not handed over lives here rather than in the tool: a tool that had to ask would be a tool that could forget to.</remarks>
internal sealed record Lease(Worker? Worker, string? Refusal)
{
    public bool IsOk => Refusal is null && Worker is not null;
}
