using Snail.MCP.AfterEffects.Application.Ports;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>A launcher that starts nothing: it remembers the environment each launch asked for and hands back a process a test ends by hand.</summary>
internal sealed class FakeLauncher(string? executable = "/fake/After Effects") : IWorkerLauncher
{
    public List<IReadOnlyDictionary<string, string>> Launched { get; } = [];

    public List<FakeWorkerProcess> Processes { get; } = [];

    public string? Executable { get; } = executable;

    public IWorkerProcess Launch(IReadOnlyDictionary<string, string> environment)
    {
        Launched.Add(environment);
        var process = new FakeWorkerProcess(1000 + Processes.Count);
        Processes.Add(process);

        return process;
    }

    public sealed class FakeWorkerProcess(int pid) : IWorkerProcess
    {
        public int Pid { get; } = pid;

        public bool HasExited { get; private set; }

        public bool WasKilled { get; private set; }

        public void Exit() => HasExited = true;

        public void Kill()
        {
            WasKilled = true;
            HasExited = true;
        }

        public void Dispose()
        {
        }
    }
}
