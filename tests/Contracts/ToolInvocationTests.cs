using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Extensions;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>Every tool called once, against a bridge that only remembers: the command it sends is the one it declares, and
/// every argument it was given reaches the bridge under a snake_case name.</summary>
/// <remarks>The convention tests read the sources; this one runs them. A tool whose body names the wrong command, drops a
/// parameter or throws on a value it accepts fails here and nowhere else, since the contract tests over stdio and HTTP call
/// only a handful of tools. After Effects is never involved: the bridge is a stub, aerender and the launcher are fakes, and
/// the data directory is a folder of this test.</remarks>
public sealed class ToolInvocationTests : IDisposable
{
    /// <summary>Tools left out, each for a reason of its own; everything else is called.</summary>
    private static readonly Dictionary<string, string> NotCalled = new(StringComparer.Ordinal)
    {
        ["ae_install_bridge"] = "writes into the owner's After Effects scripts folder",
        ["ae_capture_frame"] = "waits for a PNG After Effects never writes here",
        ["ae_viewer_snapshot"] = "waits for a PNG After Effects never writes here",
    };

    private readonly string _dataDirectory = TestFolder.Create();
    private readonly ServiceProvider _services;
    private readonly StubBridge _bridge = new((_, _) => BridgeReply.Ok(new JsonObject { ["stub"] = true }));

    public ToolInvocationTests()
    {
        Directory.CreateDirectory(_dataDirectory);
        var builder = Host.CreateApplicationBuilder();
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{ServerConfig.SectionName}:DataDirectory"] = _dataDirectory,
            [$"{ServerConfig.SectionName}:ProjectDirectory"] = _dataDirectory,
        });
        builder.ConfigureMcpServer(builder.Configuration);
        builder.Services.AddSingleton<IAfterEffectsBridge>(_bridge);
        builder.Services.AddSingleton<IAerender>(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0)));
        builder.Services.AddSingleton<IWorkerLauncher>(new FakeLauncher());
        _services = builder.Services.BuildServiceProvider();
    }

    [Fact]
    public async Task EveryTool_Called_SendsTheCommandItDeclares()
    {
        var wrong = new List<string>();
        var threw = new List<string>();
        var called = 0;

        foreach (var (type, method, name) in Tools())
        {
            if (NotCalled.ContainsKey(name))
            {
                continue;
            }

            _bridge.Sent.Clear();
            var instance = ActivatorUtilities.CreateInstance(_services, type);

            try
            {
                await InvokeAsync(instance, method).ConfigureAwait(false);
                called++;
            }
            catch (Exception failure)
            {
                threw.Add($"{name}: {failure.InnerException?.Message ?? failure.Message}");
                continue;
            }

            var declared = method.GetCustomAttributes<SendsAttribute>().Select(sends => sends.Command).ToHashSet(StringComparer.Ordinal);
            var sent = _bridge.Sent.Select(call => call.Command.Name).Distinct(StringComparer.Ordinal).ToList();

            if (declared.Count == 0 || sent.Count == 0)
            {
                continue;
            }

            if (sent.Any(command => !declared.Contains(command)))
            {
                wrong.Add($"{name} declares {string.Join(", ", declared)} and sent {string.Join(", ", sent)}");
            }
        }

        Assert.True(threw.Count == 0, $"tools that threw on their own arguments: {string.Join("; ", threw)}");
        Assert.True(wrong.Count == 0, $"tools that sent another command: {string.Join("; ", wrong)}");
        Assert.True(called > 100, $"only {called} tools were called");
    }

    /// <summary>What a tool was given arrives at the bridge: every parameter of a sent command is a key of its params, in
    /// snake_case, and nothing the caller filled in is dropped on the way.</summary>
    [Fact]
    public async Task EveryArgument_ReachesTheBridge_UnderItsWireName()
    {
        var schema = Schema();
        var missing = new List<string>();

        foreach (var (type, method, name) in Tools())
        {
            if (NotCalled.ContainsKey(name))
            {
                continue;
            }

            _bridge.Sent.Clear();
            var instance = ActivatorUtilities.CreateInstance(_services, type);

            try
            {
                await InvokeAsync(instance, method).ConfigureAwait(false);
            }
            catch (Exception)
            {
                continue;
            }

            foreach (var call in _bridge.Sent)
            {
                if (schema[call.Command.Name] is not JsonObject known)
                {
                    continue;
                }

                var reads = (known["params"] as JsonArray ?? []).Select(entry => entry!.ToString()).ToHashSet(StringComparer.Ordinal);
                var strangers = call.Parameters.Select(pair => pair.Key).Where(key => key != "agent" && !reads.Contains(key)).ToList();

                if (strangers.Count > 0)
                {
                    missing.Add($"{name} sent {string.Join(", ", strangers)}, which '{call.Command.Name}' does not read");
                }
            }
        }

        Assert.True(missing.Count == 0, string.Join("; ", missing));
    }

    private static IEnumerable<(Type Type, MethodInfo Method, string Name)> Tools() =>
        typeof(ServerConfig).Assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                .Select(method => (Type: type, Method: method, Name: method.GetCustomAttribute<McpServerToolAttribute>()?.Name))
                .Where(found => found.Name is not null)
                .Select(found => (found.Type, found.Method, found.Name!)));

    private static JsonObject Schema() =>
        (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(Repository.Root, "extension", "commands.json")))!["commands"]!;

    private static async Task InvokeAsync(object instance, MethodInfo method)
    {
        var answer = method.Invoke(instance, [.. method.GetParameters().Select(Sample)]);

        switch (answer)
        {
            case Task<CallToolResult> result:
                await result.ConfigureAwait(false);
                break;
            case Task task:
                await task.ConfigureAwait(false);
                break;
        }
    }

    /// <summary>A value of the parameter's own type, so the body runs the line that passes it on; a name or an id is "1",
    /// which reads as both.</summary>
    private static object? Sample(ParameterInfo parameter)
    {
        var type = Nullable.GetUnderlyingType(parameter.ParameterType) ?? parameter.ParameterType;

        if (type == typeof(CancellationToken))
        {
            return CancellationToken.None;
        }

        if (type == typeof(McpServer))
        {
            return null;
        }

        if (type == typeof(string))
        {
            return "1";
        }

        if (type == typeof(bool))
        {
            return true;
        }

        if (type == typeof(int))
        {
            return 1;
        }

        if (type == typeof(double))
        {
            return 1d;
        }

        if (type == typeof(string[]))
        {
            return new[] { "1" };
        }

        if (type == typeof(int[]))
        {
            return new[] { 1 };
        }

        if (type == typeof(double[]))
        {
            return new[] { 1d, 2d };
        }

        if (type == typeof(JsonObject))
        {
            return new JsonObject { ["1"] = 1 };
        }

        if (type == typeof(JsonArray))
        {
            return new JsonArray(1, 2);
        }

        if (type == typeof(JsonNode))
        {
            return JsonValue.Create(1);
        }

        throw new NotSupportedException($"{parameter.Member.Name}.{parameter.Name} is a {type.Name}, which this test has no sample for.");
    }

    public void Dispose()
    {
        _services.Dispose();

        try
        {
            Directory.Delete(_dataDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
