using System.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>After Effects as installed on this machine, started as one more instance: the binary inside the bundle on macOS, AfterFX.exe on Windows.</summary>
/// <remarks>The binary is run directly rather than through the launcher of the operating system, so the process is this server's child: its
/// pid is known, its environment is set, and its output never reaches the pipe the MCP client reads. <c>-m</c> asks for a further instance
/// beside the one a person may have open; <c>-noui</c> hides the interface, which only the Windows build honours.</remarks>
internal sealed class AfterEffectsLauncher(ServerConfig config) : IWorkerLauncher
{
    private readonly Lazy<string?> _executable = new(() => Find(config.Workers.Executable));

    public string? Executable => _executable.Value;

    public IWorkerProcess Launch(IReadOnlyDictionary<string, string> environment)
    {
        var executable = Executable ?? throw new InvalidOperationException("After Effects is not installed on this machine.");
        var start = new ProcessStartInfo(executable)
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
        };
        start.ArgumentList.Add("-m");

        if (config.Workers.IsHeadless && OperatingSystem.IsWindows())
        {
            start.ArgumentList.Add("-noui");
        }

        foreach (var (name, value) in environment)
        {
            start.Environment[name] = value;
        }

        var process = Process.Start(start) ?? throw new InvalidOperationException("After Effects did not start.");
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        return new WorkerProcess(process);
    }

    /// <summary>The newest After Effects under the folders Adobe installs to, or the configured executable when it exists.</summary>
    private static string? Find(string? configured)
    {
        if (!string.IsNullOrWhiteSpace(configured))
        {
            return File.Exists(configured) ? configured : null;
        }

        var candidates = OperatingSystem.IsWindows()
            ? Folders(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Adobe"), "Adobe After Effects *")
                .Select(folder => Path.Combine(folder, "Support Files", "AfterFX.exe"))
            : Folders("/Applications", "Adobe After Effects *")
                .SelectMany(folder => Folders(folder, "Adobe After Effects *.app"))
                .Where(bundle => !bundle.Contains("Render Engine", StringComparison.OrdinalIgnoreCase))
                .Select(bundle => Path.Combine(bundle, "Contents", "MacOS", "After Effects"));

        return candidates.Where(File.Exists).OrderByDescending(VersionOf).FirstOrDefault();
    }

    private static IEnumerable<string> Folders(string root, string pattern) =>
        Directory.Exists(root) ? Directory.EnumerateDirectories(root, pattern) : [];

    private static int VersionOf(string path)
    {
        var digits = new string([.. Path.GetFileName(Path.GetDirectoryName(Path.GetDirectoryName(Path.GetDirectoryName(path))) ?? path).Where(char.IsDigit)]);

        return int.TryParse(digits, out var version) ? version : 0;
    }

    private sealed class WorkerProcess(Process process) : IWorkerProcess
    {
        public int Pid => process.Id;

        public bool HasExited
        {
            get
            {
                try
                {
                    return process.HasExited;
                }
                catch (InvalidOperationException)
                {
                    return true;
                }
            }
        }

        public void Kill()
        {
            try
            {
                process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Dispose() => process.Dispose();
    }
}
