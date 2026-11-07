using System.Diagnostics;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>Runs a short-lived program to its end and hands back both of its streams, or stops it when it outlasts the time it was given.</summary>
/// <remarks>Both streams are read at once, from the moment the program starts. A redirected stream nobody reads fills its pipe — 64 KB on
/// macOS — and the program then blocks writing to it while the caller waits for the other stream or for the exit: the osascript call read only
/// standard error, and the process lookup read standard output with standard error redirected and never drained. The reads and the exit share one
/// deadline, because a wait placed after a synchronous read to the end bounds nothing: the read only returns once the program has gone. A program
/// that outlasts the deadline is killed with everything it started, so no child is left holding the pipes or the machine.</remarks>
internal static class ChildProcess
{
    /// <summary>Starts the program, reads both streams to their end and waits for it to exit, within the timeout.</summary>
    /// <remarks>The caller's cancellation also kills the program and is then thrown as cancellation, while the timeout is an answer: a lookup
    /// that hangs is a fact about the machine, not a failure of the caller.</remarks>
    public static async Task<ChildProcessRun> RunAsync(ProcessStartInfo start, TimeSpan timeout, CancellationToken cancellationToken)
    {
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        using var process = Process.Start(start) ?? throw new InvalidOperationException($"{start.FileName} did not start.");
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);

        var output = process.StandardOutput.ReadToEndAsync(deadline.Token);
        var error = process.StandardError.ReadToEndAsync(deadline.Token);

        try
        {
            await process.WaitForExitAsync(deadline.Token).ConfigureAwait(false);

            return new ChildProcessRun(process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false), HasTimedOut: false);
        }
        catch (OperationCanceledException)
        {
            Stop(process);
            await ((Task)Task.WhenAll(output, error)).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
            cancellationToken.ThrowIfCancellationRequested();

            return new ChildProcessRun(null, string.Empty, string.Empty, HasTimedOut: true);
        }
    }

    private static void Stop(Process process)
    {
        try
        {
            process.Kill(entireProcessTree: true);
        }
        catch (InvalidOperationException)
        {
        }
    }
}

/// <summary>How a short-lived program ended: its exit code and both streams, or that it was stopped for running past its time.</summary>
internal sealed record ChildProcessRun(int? ExitCode, string Output, string Error, bool HasTimedOut);
