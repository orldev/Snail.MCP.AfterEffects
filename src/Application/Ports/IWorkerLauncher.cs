namespace Snail.MCP.AfterEffects.Application.Ports;

/// <summary>After Effects as a process this server starts: where the executable is, and an instance launched with a name of its own.</summary>
internal interface IWorkerLauncher
{
    /// <summary>Path of the After Effects executable; null when none is installed where this server looks.</summary>
    string? Executable { get; }

    /// <summary>Starts one more After Effects with these variables in its environment, which the bridge inside reads to name itself.</summary>
    IWorkerProcess Launch(IReadOnlyDictionary<string, string> environment);
}

/// <summary>One After Effects this server launched: whether it still runs, and the means to end it when it will not quit.</summary>
internal interface IWorkerProcess : IDisposable
{
    int Pid { get; }

    bool HasExited { get; }

    /// <summary>Ends the process at once, with everything it started.</summary>
    void Kill();
}
