namespace Snail.MCP.AfterEffects.Application.Ports;

/// <summary>The After Effects installed on this machine, seen from outside it: where its user scripts live, whether it runs, and how to hand it a script.</summary>
internal interface IAfterEffectsHost
{
    /// <summary>The user scripts folders of the installed versions, newest first; empty when After Effects has never run for this user.</summary>
    IReadOnlyList<ScriptsHome> ScriptsHomes();

    bool IsRunning { get; }

    /// <summary>Runs a script file inside the running After Effects; the result says whether the request reached it, not what the script did.</summary>
    Task<ScriptRun> RunScriptAsync(string scriptPath, CancellationToken cancellationToken);
}

/// <summary>A user scripts folder of one After Effects version: the <c>Scripts</c> folder with <c>Startup</c> beside the modules folder.</summary>
internal sealed record ScriptsHome(string Version, string Path);

/// <summary>What came of handing After Effects a script: whether something was asked at all, and what the asker said.</summary>
internal sealed record ScriptRun(bool WasAttempted, bool Succeeded, string Detail);
