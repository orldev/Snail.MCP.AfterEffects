using System.Diagnostics;
using System.Reflection;
using System.Text.Json;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The stdio contract of the real server process: stdout carries JSON-RPC and nothing else, the composition root boots, guidance and the version ae_diagnose reports reach the client, and every tool of the assembly reaches the model.</summary>
public class StdioContractTests
{
    private const string ProtocolVersion = "2025-06-18";

    /// <summary>A budget for the tool list every client reads before it can do anything; measured once the always-on layer settles, and not
    /// a number to keep updating: a new tool that would live here earns its place or moves behind a skill.</summary>
    private const double MostKilobytesOfTools = 40;

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Server_AfterHandshake_KeepsStdoutCleanOfEverythingButJsonRpc()
    {
        using var session = await ServerSession.StartAsync();

        var initialize = await session.CallAsync(1, "initialize", new
        {
            protocolVersion = ProtocolVersion,
            capabilities = new { },
            clientInfo = new { name = "stdio-contract", version = "1.0" },
        });

        var result = initialize.GetProperty("result");
        var serverInfo = result.GetProperty("serverInfo");

        Assert.Equal("s3l-mcp-aftereffects", serverInfo.GetProperty("name").GetString());
        Assert.Equal(ThisAssembly.InformationalVersion, serverInfo.GetProperty("version").GetString());
        Assert.Equal(ServerGuidance.Instructions, result.GetProperty("instructions").GetString());

        await session.NotifyAsync("notifications/initialized");

        var listed = await session.CallAsync(2, "tools/list", new { });
        var names = listed.GetProperty("result").GetProperty("tools")
            .EnumerateArray()
            .Select(tool => tool.GetProperty("name").GetString()!)
            .ToHashSet(StringComparer.Ordinal);

        Assert.Equal(BaseToolNamesOfAssembly(), names);
        Assert.True(Kilobytes(listed) <= MostKilobytesOfTools, $"the always-on tool list is {Kilobytes(listed):0.0} KB, above the {MostKilobytesOfTools} KB budget");

        var called = await session.CallAsync(3, "tools/call", new
        {
            name = "ae_diagnose",
            arguments = new { },
        });

        var payload = called.GetProperty("result").GetProperty("content")[0].GetProperty("text").GetString()!;
        var report = JsonDocument.Parse(payload).RootElement;

        Assert.True(report.GetProperty("ok").GetBoolean());
        Assert.False(report.GetProperty("data").GetProperty("afterEffects").GetProperty("reachable").GetBoolean());
        Assert.Equal(ThisAssembly.InformationalVersion, report.GetProperty("data").GetProperty("server").GetProperty("version").GetString());
        Assert.True(report.GetProperty("data").GetProperty("bridge").GetProperty("port").GetInt32() > 0, "the server did not announce a port for the bridge");
        Assert.Equal("file", report.GetProperty("data").GetProperty("bridge").GetProperty("token").GetString());
        Assert.Single(Directory.EnumerateFiles(Path.Combine(session.DataDirectory, "state", "servers"), "*.json"));

        var exit = await session.ShutdownAsync();

        Assert.Equal(0, exit);
        Assert.All(session.StdoutLines, line => Assert.Equal("2.0", session.ParseOrFail(line).GetProperty("jsonrpc").GetString()));
    }

    /// <summary>
    /// The guard rules of every Snail server, on the list a model receives with every skill loaded: names in snake case and listed once,
    /// the tool and every parameter described, read-only or destructive stated, every allowed value named, English.
    /// </summary>
    [Fact]
    public async Task Tools_AsListedWithEverySkill_KeepTheGuardRules()
    {
        var dataDirectory = TestFolder.Create();
        var variables = new Dictionary<string, string?>
        {
            ["SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY"] = dataDirectory,
            ["SNAIL_MCP_AFTEREFFECTS_IDLE_TIMEOUT_MINUTES"] = "0",
        };

        try
        {
            await using var client = await ServerProgram.StartAsync("Snail.MCP.AfterEffects", "SNAIL_AFTEREFFECTS_SERVER", variables);

            foreach (var skill in SkillCatalog.Discover(typeof(ToolDescriptions).Assembly))
            {
                var enabled = await client.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = skill.Name });

                Assert.NotEqual(true, enabled.IsError);
            }

            var listed = await client.ListToolsAsync();

            Assert.True(listed.Count > BaseToolNamesOfAssembly().Count, $"only {listed.Count} tools listed with every skill loaded");
            Assert.Empty(ToolGuards.Problems(listed.Select(tool => tool.ProtocolTool)));
        }
        finally
        {
            if (Directory.Exists(dataDirectory))
            {
                Directory.Delete(dataDirectory, recursive: true);
            }
        }
    }

    private static double Kilobytes(JsonElement listed) => listed.GetProperty("result").GetProperty("tools").GetRawText().Length / 1024.0;

    /// <summary>Tools are discovered by the SDK through attributes, so a misconfigured attribute is invisible to reflection-only guards; skill tools stay out until a skill is enabled.</summary>
    private static HashSet<string> BaseToolNamesOfAssembly() =>
        SkillRegistrationExtensions.BaseToolTypes(typeof(ToolDescriptions).Assembly)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(method => method.GetCustomAttribute<McpServerToolAttribute>()?.Name)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>The server started as a separate process and spoken to over stdin/stdout, exactly as an MCP client does.</summary>
    private sealed class ServerSession : IDisposable
    {
        private readonly Process _process;
        private readonly string _dataDirectory;
        private readonly List<string> _stderr = [];

        private ServerSession(Process process, string dataDirectory)
        {
            _process = process;
            _dataDirectory = dataDirectory;
        }

        public List<string> StdoutLines { get; } = [];

        public string DataDirectory => _dataDirectory;

        /// <summary>The dll beside the tests, or the publication <c>SNAIL_AFTEREFFECTS_SERVER</c> names.</summary>
        public static Task<ServerSession> StartAsync()
        {
            var dataDirectory = TestFolder.Create();
            var (command, arguments) = ServerProgram.Command("Snail.MCP.AfterEffects", "SNAIL_AFTEREFFECTS_SERVER");
            var start = new ProcessStartInfo
            {
                FileName = command,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };

            foreach (var argument in arguments)
            {
                start.ArgumentList.Add(argument);
            }

            start.Environment["SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY"] = dataDirectory;
            start.Environment["SNAIL_MCP_AFTEREFFECTS_IDLE_TIMEOUT_MINUTES"] = "0";
            start.Environment.Remove("SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT");
            start.Environment["SNAIL_MCP_AFTEREFFECTS_CONFIG"] = ServerProgram.NoSettingsFile;

            var session = new ServerSession(Process.Start(start)!, dataDirectory);
            session.DrainStderr();

            return Task.FromResult(session);
        }

        public async Task<JsonElement> CallAsync(int id, string method, object parameters)
        {
            await NotifyAsync(method, parameters, id);

            return await ReadAsync();
        }

        public async Task NotifyAsync(string method, object? parameters = null, int? id = null)
        {
            var envelope = new Dictionary<string, object> { ["jsonrpc"] = "2.0", ["method"] = method };

            if (id is { } value) envelope["id"] = value;
            if (parameters is not null) envelope["params"] = parameters;

            await _process.StandardInput.WriteLineAsync(JsonSerializer.Serialize(envelope));
            await _process.StandardInput.FlushAsync();
        }

        public async Task<int> ShutdownAsync()
        {
            _process.StandardInput.Close();

            using var deadline = new CancellationTokenSource(Timeout);

            while (await _process.StandardOutput.ReadLineAsync(deadline.Token) is { } line)
            {
                if (line.Length > 0) StdoutLines.Add(line);
            }

            await _process.WaitForExitAsync(deadline.Token);

            return _process.ExitCode;
        }

        /// <summary>stdio carries JSON-RPC only: a single stray Console.Write breaks every MCP client, so the line itself is reported.</summary>
        public JsonElement ParseOrFail(string line)
        {
            try
            {
                return JsonDocument.Parse(line).RootElement;
            }
            catch (JsonException)
            {
                Assert.Fail($"stdout carries a line that is not JSON-RPC: {line}\nstderr:\n{Stderr()}");

                throw;
            }
        }

        private string Stderr()
        {
            lock (_stderr) return string.Join('\n', _stderr);
        }

        private async Task<JsonElement> ReadAsync()
        {
            using var deadline = new CancellationTokenSource(Timeout);

            while (await _process.StandardOutput.ReadLineAsync(deadline.Token) is { } line)
            {
                if (line.Length == 0) continue;

                StdoutLines.Add(line);

                var message = ParseOrFail(line);

                if (message.TryGetProperty("result", out _) || message.TryGetProperty("error", out _))
                {
                    return message;
                }
            }

            throw new InvalidOperationException($"the server closed stdout without answering. stderr:\n{Stderr()}");
        }

        private void DrainStderr() => _ = Task.Run(async () =>
        {
            while (await _process.StandardError.ReadLineAsync() is { } line)
            {
                lock (_stderr) _stderr.Add(line);
            }
        });

        public void Dispose()
        {
            if (!_process.HasExited) _process.Kill(entireProcessTree: true);

            _process.Dispose();

            if (Directory.Exists(_dataDirectory)) Directory.Delete(_dataDirectory, recursive: true);
        }
    }
}
