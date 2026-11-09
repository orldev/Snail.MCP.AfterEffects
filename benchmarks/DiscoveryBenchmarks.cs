using BenchmarkDotNet.Attributes;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Extensions;
using Snail.Sdk.Mcp.Hosting.Discovery;

namespace Snail.MCP.AfterEffects.Benchmarks;

/// <summary>
/// find_tool, the call an agent makes before most others: the catalog read from the tools the server registers, then
/// ranked by Snail.Sdk against the words of a task.
/// </summary>
[MemoryDiagnoser]
public class DiscoveryBenchmarks
{
    private static readonly DiscoveryOptions Options = new() { Prefix = ToolCommands.Prefix, CommandOf = ToolCommands.CommandOf };

    private readonly IReadOnlyList<ToolEntry> _catalog = Read();

    /// <summary>The catalog read from the tool classes, as find_tool reads the registered tools.</summary>
    [Benchmark]
    public int Catalog() => Read().Count;

    /// <summary>One search in plain words.</summary>
    [Benchmark]
    public int Search() => ToolCatalog.Find(_catalog, "animate the opacity of a text layer over two seconds", 5, Options).Count;

    private static IReadOnlyList<ToolEntry> Read() =>
        [.. ClientRegistrationExtensions.ToolTypes(typeof(ToolCommands).Assembly).SelectMany(type => ToolCatalog.Of(type))];
}
