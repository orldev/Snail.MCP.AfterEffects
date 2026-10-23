using System.Diagnostics;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The After Effects on this machine, for the live tests: whether one is running, and how to hand it a script.</summary>
/// <remarks>macOS only for now: the live tests inject the bridge through osascript, which is how the server itself reaches a running After
/// Effects there. On another system the live tests are skipped rather than failed.</remarks>
internal static class AfterEffectsInstallation
{
    public const string ApplicationName = "Adobe After Effects 2026";

    public static bool IsRunning { get; } = OperatingSystem.IsMacOS() && IsProcessListed();

    public static string? SkipReason => IsRunning ? null : "After Effects is not running on this machine (macOS only)";

    public static async Task RunScriptAsync(string path, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo("osascript") { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-e");
        start.ArgumentList.Add($"tell application \"{ApplicationName}\" to DoScriptFile \"{path}\"");

        using var process = Process.Start(start)!;
        await process.WaitForExitAsync(cancellationToken);
    }

    private static bool IsProcessListed()
    {
        try
        {
            var start = new ProcessStartInfo("pgrep") { RedirectStandardOutput = true, RedirectStandardError = true };
            start.ArgumentList.Add("-x");
            start.ArgumentList.Add("After Effects");

            using var process = Process.Start(start)!;
            process.WaitForExit(5000);

            return process.ExitCode == 0;
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return false;
        }
    }
}

/// <summary>A fact that runs only against a running After Effects, and is skipped, not failed, without one.</summary>
internal sealed class AfterEffectsFactAttribute : FactAttribute
{
    public AfterEffectsFactAttribute()
    {
        Skip = AfterEffectsInstallation.SkipReason;
    }
}
