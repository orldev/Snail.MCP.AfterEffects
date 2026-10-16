using System.Diagnostics;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>After Effects on Windows: user scripts under AppData, the process found by its executable name, scripts handed over with <c>AfterFX.exe -r</c>.</summary>
/// <remarks>Written from the documented command line and not yet exercised on a Windows machine; the live tests run on macOS.</remarks>
internal sealed class WindowsAfterEffectsHost(ILogger<WindowsAfterEffectsHost> logger) : IAfterEffectsHost
{
    public const string ProcessName = "AfterFX";

    private static string AppDataRoot => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Adobe", "After Effects");

    public bool IsRunning => Process.GetProcessesByName(ProcessName).Length > 0;

    public IReadOnlyList<ScriptsHome> ScriptsHomes()
    {
        if (!Directory.Exists(AppDataRoot))
        {
            return [];
        }

        return [.. Directory.EnumerateDirectories(AppDataRoot)
            .Select(folder => new ScriptsHome(Path.GetFileName(folder), Path.Combine(folder, "Scripts")))
            .Where(home => Version.TryParse(home.Version, out _))
            .OrderByDescending(home => Version.Parse(home.Version))];
    }

    public async Task<ScriptRun> RunScriptAsync(string scriptPath, CancellationToken cancellationToken)
    {
        if (Executable() is not { } executable)
        {
            return new ScriptRun(false, false, "After Effects is not running.");
        }

        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true };
        start.ArgumentList.Add("-r");
        start.ArgumentList.Add(scriptPath);

        try
        {
            using var process = Process.Start(start)!;
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            return new ScriptRun(true, process.ExitCode == 0, $"AfterFX exited with {process.ExitCode}");
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            Log.AfterFxNotAsked(logger, scriptPath, failure.Message);

            return new ScriptRun(true, false, failure.Message);
        }
    }

    private static string? Executable()
    {
        try
        {
            return Process.GetProcessesByName(ProcessName).Select(process => process.MainModule?.FileName).FirstOrDefault(path => path is not null);
        }
        catch (Exception failure) when (failure is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            return null;
        }
    }
}
