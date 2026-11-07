using System.Diagnostics;
using Snail.MCP.AfterEffects.Adapters.AfterEffects;

namespace Snail.MCP.AfterEffects.Tests.Adapters;

/// <summary>The short-lived programs the Mac host runs — osascript, pgrep, ps — told by /bin/sh scripts that misbehave the ways those can.</summary>
public class ChildProcessTests
{
    private const int PastThePipe = 200_000;

    /// <summary>A program that fills the stream nobody used to read — standard error of the lookup — still ends, and both streams come back whole.</summary>
    [Fact]
    public async Task Program_WritingMoreThanAPipeHoldsToStandardError_EndsWithBothStreams()
    {
        var run = await ChildProcess.RunAsync(Shell($"head -c {PastThePipe} /dev/zero | tr '\\0' e >&2; echo done"), TimeSpan.FromSeconds(20), CancellationToken.None);

        Assert.False(run.HasTimedOut);
        Assert.Equal(0, run.ExitCode);
        Assert.Equal("done\n", run.Output);
        Assert.Equal(PastThePipe, run.Error.Length);
    }

    /// <summary>A program that fills standard output before it fails — the stream the osascript call left unread — still ends, with its error and exit code.</summary>
    [Fact]
    public async Task Program_WritingMoreThanAPipeHoldsToStandardOutput_EndsWithItsErrorAndExitCode()
    {
        var run = await ChildProcess.RunAsync(Shell($"head -c {PastThePipe} /dev/zero | tr '\\0' o; echo refused >&2; exit 3"), TimeSpan.FromSeconds(20), CancellationToken.None);

        Assert.False(run.HasTimedOut);
        Assert.Equal(3, run.ExitCode);
        Assert.Equal(PastThePipe, run.Output.Length);
        Assert.Equal("refused\n", run.Error);
    }

    /// <summary>A program that does not answer is stopped at the deadline together with what it started, and the answer says it timed out.</summary>
    [Fact]
    public async Task Program_ThatOutlastsItsTimeout_IsKilledWithWhatItStarted()
    {
        var pidFile = Path.Combine(TestFolder.Create(), "child.pid");
        var waited = Stopwatch.StartNew();

        var run = await ChildProcess.RunAsync(Shell($"sleep 60 & echo $! > '{pidFile}'; wait"), TimeSpan.FromMilliseconds(500), CancellationToken.None);

        Assert.True(run.HasTimedOut);
        Assert.Null(run.ExitCode);
        Assert.InRange(waited.Elapsed, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        Assert.True(await IsGoneAsync(int.Parse(File.ReadAllText(pidFile).Trim())), "the program's child outlived the timeout");
    }

    /// <summary>The caller's cancellation is cancellation, not a timeout, and still stops the program.</summary>
    [Fact]
    public async Task Program_WhoseCallerCancels_IsKilled_AndTheCancellationIsThrown()
    {
        using var cancel = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
        var waited = Stopwatch.StartNew();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => ChildProcess.RunAsync(Shell("sleep 60"), TimeSpan.FromSeconds(30), cancel.Token));

        Assert.InRange(waited.Elapsed, TimeSpan.Zero, TimeSpan.FromSeconds(10));
    }

    private static ProcessStartInfo Shell(string script)
    {
        var start = new ProcessStartInfo("/bin/sh");
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add(script);

        return start;
    }

    /// <summary>Whether a process is gone within a few seconds: a killed child is reaped by the system, not at once.</summary>
    private static async Task<bool> IsGoneAsync(int pid)
    {
        for (var attempt = 0; attempt < 50; attempt++)
        {
            try
            {
                using var process = Process.GetProcessById(pid);

                if (process.HasExited)
                {
                    return true;
                }
            }
            catch (ArgumentException)
            {
                return true;
            }

            await Task.Delay(100);
        }

        return false;
    }
}
