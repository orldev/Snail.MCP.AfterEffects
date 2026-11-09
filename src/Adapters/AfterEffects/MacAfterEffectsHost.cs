using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>After Effects on macOS: user scripts under Preferences, the process found by name, scripts handed over through AppleScript's DoScriptFile.</summary>
/// <remarks>DoScriptFile returns nothing a caller can use, so success here means the request was accepted, and whether the bridge then came up
/// is read from the link. The application is addressed by the name of the bundle that is running, which is what AppleScript resolves. The
/// script is sent <c>with override</c>: the bridge's own scheduled task counts as a script already running, and without the override After
/// Effects refuses the second one with a dialog that then holds the application until somebody clicks it away.</remarks>
internal sealed class MacAfterEffectsHost(ILogger<MacAfterEffectsHost> logger) : IAfterEffectsHost
{
    public const string ProcessName = "After Effects";

    /// <summary>How long osascript may take to hand After Effects a script.</summary>
    /// <remarks>Under AppleScript's own two-minute wait for an Apple event, past which no answer is coming: After Effects accepts a script in
    /// seconds, and one that holds it longer is behind a dialog somebody has to click away.</remarks>
    private static readonly TimeSpan ScriptHandover = TimeSpan.FromSeconds(60);

    /// <summary>How long pgrep or ps may take to answer whether After Effects runs.</summary>
    private static readonly TimeSpan Lookup = TimeSpan.FromSeconds(5);

    private static string PreferencesRoot => Path.Combine(ServerFolders.Home, "Library", "Preferences", "Adobe", "After Effects");

    public bool IsRunning => RunningBundle() is not null;

    public IReadOnlyList<ScriptsHome> ScriptsHomes()
    {
        if (!Directory.Exists(PreferencesRoot))
        {
            return [];
        }

        return [.. Directory.EnumerateDirectories(PreferencesRoot)
            .Select(folder => new ScriptsHome(Path.GetFileName(folder), Path.Combine(folder, "Scripts")))
            .Where(home => Version.TryParse(home.Version, out _))
            .OrderByDescending(home => Version.Parse(home.Version))];
    }

    public async Task<ScriptRun> RunScriptAsync(string scriptPath, CancellationToken cancellationToken)
    {
        if (RunningBundle() is not { } bundle)
        {
            return new ScriptRun(false, false, "After Effects is not running.");
        }

        var start = new ProcessStartInfo("osascript");
        start.ArgumentList.Add("-e");
        start.ArgumentList.Add($"tell application \"{Path.GetFileNameWithoutExtension(bundle)}\" to DoScriptFile \"{scriptPath}\" with override");

        try
        {
            var run = await ChildProcess.RunAsync(start, ScriptHandover, cancellationToken).ConfigureAwait(false);

            if (run.HasTimedOut)
            {
                Log.OsascriptRefused(logger, scriptPath, $"no answer within {ScriptHandover.TotalSeconds:0} s");

                return new ScriptRun(true, false, $"osascript did not answer within {ScriptHandover.TotalSeconds:0} s; After Effects may be holding a dialog open.");
            }

            if (run.ExitCode != 0)
            {
                Log.OsascriptRefused(logger, scriptPath, run.Error.Trim());

                return new ScriptRun(true, false, $"osascript exited with {run.ExitCode}: {run.Error.Trim()}");
            }

            return new ScriptRun(true, true, "accepted");
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return new ScriptRun(true, false, failure.Message);
        }
    }

    /// <summary>The .app bundle of the running After Effects, read off its process path; null when none runs.</summary>
    /// <remarks>The process is found by its name, not by its command line: Adobe's broker is started with After Effects' path as an
    /// argument, so a search over command lines answers that After Effects runs when only the broker does, and the script meant for
    /// After Effects is then sent to a program whose dictionary has no word for it.</remarks>
    private static string? RunningBundle()
    {
        foreach (var pid in Pids())
        {
            if (Executable(pid) is { } path && path.IndexOf(".app/", StringComparison.Ordinal) is > 0 and var cut)
            {
                return path[..(cut + 4)];
            }
        }

        return null;
    }

    private static IEnumerable<int> Pids() =>
        Run("pgrep", ["-x", ProcessName]).Split('\n', StringSplitOptions.RemoveEmptyEntries).Select(line => int.TryParse(line.Trim(), out var pid) ? pid : 0).Where(pid => pid > 0);

    private static string? Executable(int pid) =>
        Run("ps", ["-p", pid.ToString(System.Globalization.CultureInfo.InvariantCulture), "-o", "comm="]).Trim() is { Length: > 0 } path ? path : null;

    /// <summary>What a lookup program wrote to standard output; empty when it could not start or did not answer in time.</summary>
    [SuppressMessage("Usage", "VSTHRD002", Justification = "IsRunning is a property of the port, read by synchronous callers; the wait is bounded by Lookup, and no synchronization context runs here.")]
    private static string Run(string command, string[] arguments)
    {
        try
        {
            var start = new ProcessStartInfo(command);

            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            var run = ChildProcess.RunAsync(start, Lookup, CancellationToken.None).GetAwaiter().GetResult();

            return run.Output;
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return string.Empty;
        }
    }
}
