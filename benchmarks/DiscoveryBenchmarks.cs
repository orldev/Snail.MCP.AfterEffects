using BenchmarkDotNet.Attributes;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Domain;

namespace Snail.MCP.AfterEffects.Benchmarks;

/// <summary>
/// find_tool, the call an agent makes before most others: the whole catalog read from the server's attributes once at
/// start, then searched by the words of an intent.
/// </summary>
[MemoryDiagnoser]
public class DiscoveryBenchmarks
{
    private readonly ToolIndex _index = new(typeof(Skill).Assembly);

    /// <summary>The catalog read from the assembly, as the server does once at start.</summary>
    [Benchmark]
    public int Index() => new ToolIndex(typeof(Skill).Assembly).Tools.Count;

    /// <summary>One search in plain words.</summary>
    [Benchmark]
    public int Search() => _index.Search("animate the opacity of a text layer over two seconds", 5).Count;
}
