namespace Snail.MCP.AfterEffects.Application.Ports;

/// <summary>The aerender command line beside the installed After Effects: where it is, and a render started with it.</summary>
internal interface IAerender
{
    /// <summary>Path of the aerender executable; null when no After Effects with one is installed.</summary>
    string? Executable { get; }

    /// <summary>Starts aerender with the arguments and hands back the running process, whose output lines carry the progress.</summary>
    IRenderProcess Start(IReadOnlyList<string> arguments);
}

/// <summary>One running aerender: its output as it comes, its exit, and the means to kill it.</summary>
internal interface IRenderProcess : IDisposable
{
    int Pid { get; }

    /// <summary>The lines aerender prints, standard output and error together, until it exits.</summary>
    IAsyncEnumerable<string> ReadLinesAsync(CancellationToken cancellationToken);

    Task<int> WaitForExitAsync(CancellationToken cancellationToken);

    /// <summary>Ends the render at once, with the After Effects engine it launched.</summary>
    void Kill();
}
