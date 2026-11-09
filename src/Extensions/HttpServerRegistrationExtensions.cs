using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Web.Access;
using Snail.MCP.AfterEffects.Web.Files;
using Snail.MCP.AfterEffects.Web.Pages;
using Snail.Sdk.Mcp.Hosting;
using Snail.Sdk.Mcp.Http;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Composition root of the server that runs on its own beside After Effects: the same bridge and tools, reached over HTTP behind client keys.</summary>
/// <remarks>No idle watchdog: over stdio it ends a server its client forgot, but a server beside After Effects is meant to outlive every client.
/// The host is a whole web host, as the pages, the sign-in and antiforgery need, made by Snail.Sdk to listen where the endpoint says, log on
/// stderr and trim the heap when idle (<c>UseMcpEndpoint</c>); the browser refusal of the MCP endpoint and the health check are Snail.Sdk's
/// too. What is this server's own is the keys, the pages, the files and what the health check says: whether After Effects answers.</remarks>
internal static class HttpServerRegistrationExtensions
{
    public const string McpPath = "/mcp";

    /// <summary>The host of the HTTP server over <paramref name="settings"/>, listening where <paramref name="endpoint"/> says, its services
    /// registered and nothing yet built: what the program runs, and what a test builds in its own process with a bridge of its own.</summary>
    public static WebApplicationBuilder CreateHttpBuilder(string[] args, McpEndpoint endpoint, IConfiguration settings)
    {
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions { Args = args });
        builder.Configuration.AddConfiguration(settings);

        return builder
            .UseMcpEndpoint(endpoint)
            .ConfigureHttpServer(settings);
    }

    extension(WebApplicationBuilder builder)
    {
        /// <summary>Binds the server to <paramref name="settings"/>, or to this machine's when none are given.</summary>
        public WebApplicationBuilder ConfigureHttpServer(IConfiguration? settings = null)
        {
            settings ??= ServerConfigExtensions.Settings();
            var config = ServerConfigExtensions.Peek(settings);

            builder.AddServerConfiguration(settings);

            builder.Services
                .AddAfterEffectsBridge()
                .AddAfterEffectsHost()
                .AddDiagnostics()
                .AddClients()
                .AddFarm()
                .AddWorkers()
                .AddStorage()
                .AddWebAccess()
                .AddMcpHttpTransport(config);

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseHttpServer()
        {
            app.UseBrowserRefusal(McpPath);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapHealth(check: LivenessAsync);
            app.MapRawFiles();
            app.MapPages();
            app.MapMcp(McpPath).RequireAuthorization(WebAccess.ClientPolicy);

            return app;
        }
    }

    /// <summary>The liveness answer, open to anyone: the server runs, and the body says whether After Effects answers it,
    /// <c>{"status":"ok","afterEffects":"up"}</c> or <c>"down"</c> — healthy either way, as the server outlives an After Effects that quit.</summary>
    private static async Task<HealthAnswer> LivenessAsync(IServiceProvider services, CancellationToken cancellationToken)
    {
        var liveness = await services.GetRequiredService<ServerHealth>().LivenessAsync(cancellationToken).ConfigureAwait(false);

        return new HealthAnswer(IsHealthy: true, liveness);
    }
}
