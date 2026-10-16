using System.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>After Effects on macOS: user scripts under Preferences, the process found by name, scripts handed over through AppleScript's DoScriptFile.</summary>
/// <remarks>DoScriptFile returns nothing a caller can use, so success here means the request was accepted, and whether the bridge then came up
/// is read from the link. The application is addressed by the name of the bundle that is running, which is what AppleScript resolves. The
/// script is sent <c>with override</c>: the bridge's own scheduled task counts as a script already running, and without the override After
/// Effects refuses the second one with a dialog that then holds the application until somebody clicks it away.</remarks>
internal sealed class MacAfterEffectsHost(ILogger<MacAfterEffectsHost> logger) : IAfterEffectsHost
{
    public const string ProcessName = "After Effects";

    private static string PreferencesRoot => Path.Combine(ServerPaths.Home, "Library", "Preferences", "Adobe", "After Effects");

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

        var start = new ProcessStartInfo("osascript") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-e");
        start.ArgumentList.Add($"tell application \"{Path.GetFileNameWithoutExtension(bundle)}\" to DoScriptFile \"{scriptPath}\" with override");

        try
        {
            using var process = Process.Start(start)!;
            var error = await process.StandardError.ReadToEndAsync(cancellationToken).ConfigureAwait(false);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            if (process.ExitCode != 0)
            {
                Log.OsascriptRefused(logger, scriptPath, error.Trim());

                return new ScriptRun(true, false, $"osascript exited with {process.ExitCode}: {error.Trim()}");
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

    private static string Run(string command, string[] arguments)
    {
        try
        {
            var start = new ProcessStartInfo(command) { RedirectStandardOutput = true, RedirectStandardError = true };

            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            using var process = Process.Start(start)!;
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);

            return output;
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return string.Empty;
        }
    }
}
