using System.Diagnostics;
using System.Net;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The server started as a separate process in HTTP mode, on a free loopback port, with no After Effects behind it.</summary>
internal sealed class HttpServerProcess : IAsyncDisposable
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    private readonly Process _process;
    private readonly List<string> _output = [];

    private HttpServerProcess(Process process, string dataDirectory, Uri address)
    {
        _process = process;
        DataDirectory = dataDirectory;
        Address = address;
    }

    public Uri Address { get; }

    /// <summary>Where this server keeps its state, so a test can mint a client key into it while it runs.</summary>
    public string DataDirectory { get; }

    public string Output
    {
        get
        {
            lock (_output) return string.Join('\n', _output);
        }
    }

    public static async Task<HttpServerProcess> StartAsync(string? token, bool waitForHealth = true)
    {
        var dataDirectory = TestFolder.Create();
        var address = new Uri($"http://127.0.0.1:{FreePort()}");
        var start = new ProcessStartInfo { FileName = "dotnet", RedirectStandardOutput = true, RedirectStandardError = true };

        start.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "Snail.MCP.AfterEffects.dll"));

        foreach (var name in start.Environment.Keys.Where(name => name.StartsWith("SNAIL_MCP_AFTEREFFECTS_", StringComparison.Ordinal)).ToList())
        {
            start.Environment.Remove(name);
        }

        start.Environment["SNAIL_MCP_AFTEREFFECTS_TRANSPORT"] = "Http";
        start.Environment["SNAIL_MCP_AFTEREFFECTS_HTTP__URL"] = address.ToString();
        start.Environment["SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY"] = dataDirectory;
        start.Environment["SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT"] = FreePort().ToString();
        start.Environment["SNAIL_MCP_AFTEREFFECTS_CONFIG"] = ServerProgram.NoSettingsFile;

        if (token is not null)
        {
            start.Environment["SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN"] = token;
        }

        var server = new HttpServerProcess(Process.Start(start)!, dataDirectory, address);
        server.Drain(server._process.StandardOutput);
        server.Drain(server._process.StandardError);

        if (waitForHealth)
        {
            await server.WaitForHealthAsync();
        }

        return server;
    }

    public async Task<int> WaitForExitAsync()
    {
        using var deadline = new CancellationTokenSource(Timeout);
        await _process.WaitForExitAsync(deadline.Token);

        return _process.ExitCode;
    }

    private async Task WaitForHealthAsync()
    {
        using var http = new HttpClient { BaseAddress = Address, Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTimeOffset.UtcNow + Timeout;

        while (DateTimeOffset.UtcNow < deadline)
        {
            if (_process.HasExited)
            {
                throw new InvalidOperationException($"the server exited with {_process.ExitCode}:\n{Output}");
            }

            try
            {
                using var response = await http.GetAsync("/healthz");

                if (response.IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException)
            {
            }

            await Task.Delay(200);
        }

        throw new TimeoutException($"the server did not answer /healthz:\n{Output}");
    }

    private void Drain(StreamReader reader) =>
        _ = Task.Run(async () =>
        {
            while (await reader.ReadLineAsync() is { } line)
            {
                lock (_output) _output.Add(line);
            }
        });

    private static int FreePort()
    {
        var listener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        return port;
    }

    public async ValueTask DisposeAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: true);
            await _process.WaitForExitAsync();
        }

        _process.Dispose();

        if (Directory.Exists(DataDirectory))
        {
            Directory.Delete(DataDirectory, recursive: true);
        }
    }
}
