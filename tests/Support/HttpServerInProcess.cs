using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Extensions;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The HTTP server of this repository, built and started inside the test: the same configuration, services and
/// endpoints as the published one, on a port of its own, with a bridge that only remembers what it was sent.</summary>
/// <remarks>The contract tests start the server as a process, which is what a client does and what the stdio and HTTP
/// contracts are about. This one runs in the test's own process instead, so a test may reach into its services, and so the
/// pages and endpoints it exercises are measured like any other code.
///
/// Both ports are asked of the operating system rather than chosen: picking a free port and binding it a moment later is a
/// race, and with several servers starting at once it is lost often enough to fail a run — it failed one on 24.10.2026 with
/// "address already in use". The settings name port 0, the host is the one the program builds (<c>CreateHttpBuilder</c>), and
/// Snail.Sdk's <see cref="InProcessServer"/> reads back the address Kestrel bound; the settings are told that address afterwards,
/// because the links the server signs carry it.</remarks>
internal sealed class HttpServerInProcess : IAsyncDisposable
{
    private readonly InProcessServer _server;

    private HttpServerInProcess(InProcessServer server, string dataDirectory, Uri address, StubBridge bridge)
    {
        _server = server;
        DataDirectory = dataDirectory;
        Address = address;
        Bridge = bridge;
    }

    public string DataDirectory { get; }

    public Uri Address { get; }

    /// <summary>The bridge the server talks to; every command a tool sends is in its list.</summary>
    public StubBridge Bridge { get; }

    public IServiceProvider Services => _server.Services;

    public HttpClient Client(bool followRedirects = true) =>
        new(new HttpClientHandler { AllowAutoRedirect = followRedirects, UseCookies = true }) { BaseAddress = Address };

    public static async Task<HttpServerInProcess> StartAsync(string token, Func<BridgeCommand, JsonObject, BridgeReply>? answer = null)
    {
        var dataDirectory = TestFolder.Create();
        var bridge = new StubBridge(answer ?? ((_, _) => BridgeReply.Ok(new JsonObject { ["stub"] = true })));
        var settings = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            [$"{ServerConfig.SectionName}:Transport"] = nameof(McpTransport.Http),
            [$"{ServerConfig.SectionName}:DataDirectory"] = dataDirectory,
            [$"{ServerConfig.SectionName}:Http:Url"] = "http://127.0.0.1:0",
            [$"{ServerConfig.SectionName}:Http:Token"] = token,
            [$"{ServerConfig.SectionName}:Bridge:Port"] = "0",
        }).Build();

        var endpoint = McpEndpoint.Read(settings, ServerConfigExtensions.DefaultUrl, ServerConfigExtensions.EndpointOptions);
        var builder = HttpServerRegistrationExtensions.CreateHttpBuilder([], endpoint, settings);
        builder.Services.AddSingleton<IAfterEffectsBridge>(bridge);

        var server = await InProcessServer.ServeAsync(builder.Build().UseHttpServer(), HttpServerRegistrationExtensions.McpPath);

        server.Services.GetRequiredService<ServerConfig>().Http.Url = server.Address.ToString();

        return new HttpServerInProcess(server, dataDirectory, server.Address, bridge);
    }

    public async ValueTask DisposeAsync()
    {
        await _server.DisposeAsync();

        try
        {
            Directory.Delete(DataDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
