using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace Snail.MCP.AfterEffects.Adapters.AfterEffects;

/// <summary>aerender as installed beside After Effects: under /Applications on macOS, under Program Files on Windows, the newest version first.</summary>
internal sealed class Aerender(ILogger<Aerender> logger) : IAerender
{
    private readonly Lazy<string?> _executable = new(Find);

    public string? Executable => _executable.Value;

    public IRenderProcess Start(IReadOnlyList<string> arguments)
    {
        var executable = Executable ?? throw new InvalidOperationException("aerender is not installed on this machine.");
        var start = new ProcessStartInfo(executable) { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };

        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        Log.AerenderStarting(logger, executable, string.Join(' ', arguments.Select(Quote)));

        return new RenderProcess(Process.Start(start) ?? throw new InvalidOperationException("aerender did not start."));
    }

    private static string Quote(string argument) => argument.Contains(' ', StringComparison.Ordinal) ? $"\"{argument}\"" : argument;

    /// <summary>The newest aerender under the folders Adobe installs to; the version is the number in the folder name.</summary>
    private static string? Find()
    {
        var candidates = OperatingSystem.IsWindows()
            ? Roots(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), Path.Combine("Adobe"))
                .SelectMany(root => Folders(root, "Adobe After Effects *"))
                .Select(folder => Path.Combine(folder, "Support Files", "aerender.exe"))
            : Folders("/Applications", "Adobe After Effects *").Select(folder => Path.Combine(folder, "aerender"));

        return candidates
            .Where(File.Exists)
            .OrderByDescending(path => VersionOf(path))
            .FirstOrDefault();
    }

    private static IEnumerable<string> Roots(string programFiles, string adobe) =>
        [Path.Combine(programFiles, adobe)];

    private static IEnumerable<string> Folders(string root, string pattern) =>
        Directory.Exists(root) ? Directory.EnumerateDirectories(root, pattern) : [];

    private static int VersionOf(string path)
    {
        var digits = new string([.. path.Where(char.IsDigit)]);

        return int.TryParse(digits.Length > 6 ? digits[^6..] : digits, out var version) ? version : 0;
    }

    /// <summary>A started aerender, its two streams merged into one sequence of lines in the order they arrive.</summary>
    private sealed class RenderProcess : IRenderProcess
    {
        private readonly Process _process;
        private readonly Channel<string> _lines = Channel.CreateUnbounded<string>();
        private int _open = 2;

        public RenderProcess(Process process)
        {
            _process = process;
            _process.OutputDataReceived += (_, received) => Receive(received.Data);
            _process.ErrorDataReceived += (_, received) => Receive(received.Data);
            _process.BeginOutputReadLine();
            _process.BeginErrorReadLine();
        }

        public int Pid => _process.Id;

        public async IAsyncEnumerable<string> ReadLinesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            while (await _lines.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (_lines.Reader.TryRead(out var line))
                {
                    yield return line;
                }
            }
        }

        public async Task<int> WaitForExitAsync(CancellationToken cancellationToken)
        {
            await _process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);

            return _process.ExitCode;
        }

        public void Kill()
        {
            try
            {
                _process.Kill(entireProcessTree: true);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public void Dispose() => _process.Dispose();

        /// <summary>A null line is a stream closing; the channel completes when both have.</summary>
        private void Receive(string? data)
        {
            if (data is not null)
            {
                _lines.Writer.TryWrite(data);
            }
            else if (Interlocked.Decrement(ref _open) == 0)
            {
                _lines.Writer.TryComplete();
            }
        }
    }
}
