namespace Snail.MCP.AfterEffects.Domain;

/// <summary>The words a refusal the server makes itself carries as its <c>type</c>, the one a program branches on instead of reading the sentence.</summary>
/// <remarks>
/// Spelled as the bridge spells its own types — <c>NotFound</c>, <c>ProjectDirty</c> — and the bridge's word where the server refuses the same
/// thing on its side. What the SDK names on every server is not repeated here: <see cref="Snail.Sdk.Mcp.Hosting.ToolFailures.Arguments"/> and
/// <see cref="Snail.Sdk.Mcp.Hosting.Sessions.Reached.Outside"/> are used by name.
/// The link's own failures are <see cref="BridgeError"/>'s.
/// </remarks>
internal static class FailureTypes
{
    /// <summary>ae_script, a run_tools list or a program asked for a script while scripts are switched off on this server.</summary>
    public const string ScriptingDisabled = "ScriptingDisabled";

    /// <summary>An item, a file or a folder nobody has: the bridge's word, and the server's for its own volume.</summary>
    public const string NotFound = "NotFound";

    /// <summary>No project file where a path names one: the bridge's word.</summary>
    public const string NoFile = "NoFile";

    /// <summary>A project aerender cannot read as it stands, never saved or with unsaved changes: the bridge's word.</summary>
    public const string ProjectDirty = "ProjectDirty";

    /// <summary>A frame After Effects did not write where it said it would: the bridge's word.</summary>
    public const string CaptureFailed = "CaptureFailed";

    /// <summary>No aerender beside any installed After Effects, so the farm cannot render.</summary>
    public const string FarmUnavailable = "FarmUnavailable";

    /// <summary>A render the farm ran and aerender failed, every retry spent.</summary>
    public const string RenderFailed = "RenderFailed";

    /// <summary>No render job has the id named.</summary>
    public const string NoSuchJob = "NoSuchJob";

    /// <summary>A job that is no longer queued or running, so it cannot be cancelled.</summary>
    public const string JobNotActive = "JobNotActive";

    /// <summary>No After Effects to launch as a worker: none installed where the server looks, or the pool's maximum is 0.</summary>
    public const string WorkersUnavailable = "WorkersUnavailable";

    /// <summary>No worker has the name given.</summary>
    public const string NoSuchWorker = "NoSuchWorker";

    /// <summary>The worker named is held by another client.</summary>
    public const string WorkerHeld = "WorkerHeld";

    /// <summary>Every worker is taken and the pool is at its maximum.</summary>
    public const string PoolFull = "PoolFull";

    /// <summary>The worker named is starting or stopping and cannot be leased now.</summary>
    public const string WorkerNotReady = "WorkerNotReady";

    /// <summary>After Effects runs, but the undo or redo handed to it from outside did not run.</summary>
    public const string UndoNotAsked = "UndoNotAsked";

    /// <summary>No installed After Effects has a Scripts folder to put the bridge in.</summary>
    public const string NoScriptsFolder = "NoScriptsFolder";
}
