using System.Runtime.CompilerServices;
using Snail.MCP.AfterEffects.Application.Ports;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>An aerender that prints what the test scripted and exits as told, recording every command line.</summary>
internal sealed class FakeAerender(Func<IReadOnlyList<string>, FakeAerender.FakeRenderProcess> script, string? executable = "/fake/aerender") : IAerender
{
    private readonly Func<IReadOnlyList<string>, FakeRenderProcess> _script = script;

    public List<IReadOnlyList<string>> Started { get; } = [];

    public List<FakeRenderProcess> Processes { get; } = [];

    public string? Executable { get; } = executable;

    public IRenderProcess Start(IReadOnlyList<string> arguments)
    {
        lock (Started)
        {
            Started.Add(arguments);
            var process = _script(arguments);
            Processes.Add(process);

            return process;
        }
    }

    /// <summary>A render that prints its lines one at a time, pausing between them when asked, then exits with the code.</summary>
    public sealed class FakeRenderProcess(IReadOnlyList<string> lines, int exitCode, TimeSpan? pause = null) : IRenderProcess
    {
        private readonly TaskCompletionSource<int> _exit = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Pid => 4242;

        public bool WasKilled { get; private set; }

        public async IAsyncEnumerable<string> ReadLinesAsync([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            foreach (var line in lines)
            {
                if (pause is { } delay)
                {
                    await Task.Delay(delay, cancellationToken);
                }

                yield return line;
            }

            _exit.TrySetResult(exitCode);
        }

        public Task<int> WaitForExitAsync(CancellationToken cancellationToken) => _exit.Task.WaitAsync(cancellationToken);

        public void Kill()
        {
            WasKilled = true;
            _exit.TrySetResult(137);
        }

        public void Dispose()
        {
        }
    }
}
