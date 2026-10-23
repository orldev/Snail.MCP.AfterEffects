using Snail.MCP.AfterEffects.Application.Ports;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>An After Effects that is wherever the test says: a scripts folder in a temp directory, running or not, remembering the scripts it was asked to run.</summary>
internal sealed class FakeAfterEffectsHost(IReadOnlyList<ScriptsHome> homes, bool isRunning) : IAfterEffectsHost
{
    public bool IsRunning { get; set; } = isRunning;

    public List<string> ScriptsRun { get; } = [];

    public IReadOnlyList<ScriptsHome> ScriptsHomes() => homes;

    public Task<ScriptRun> RunScriptAsync(string scriptPath, CancellationToken cancellationToken)
    {
        ScriptsRun.Add(File.ReadAllText(scriptPath));

        return Task.FromResult(new ScriptRun(true, true, "accepted"));
    }
}
