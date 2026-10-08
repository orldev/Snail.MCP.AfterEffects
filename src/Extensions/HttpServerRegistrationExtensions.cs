using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Web.Access;
using Snail.MCP.AfterEffects.Web.Files;
using Snail.MCP.AfterEffects.Web.Pages;
using Snail.MCP.AfterEffects.Web.Health;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Composition root of the server that runs on its own beside After Effects: the same bridge and tools, reached over HTTP behind client keys.</summary>
/// <remarks>No idle watchdog: over stdio it ends a server its client forgot, but a server beside After Effects is meant to outlive every client.</remarks>
internal static class HttpServerRegistrationExtensions
{
    public const string McpPath = "/mcp";

    extension(WebApplicationBuilder builder)
    {
        /// <summary>Binds the server to <paramref name="settings"/>, or to this machine's when none are given, and hands the same settings to the host's
        /// configuration, where the stdio host's builder puts them itself; the full builder's other sources stay out of the server's settings.</summary>
        public WebApplicationBuilder ConfigureHttpServer(IConfiguration? settings = null)
        {
            settings ??= ServerConfigExtensions.Settings();

            builder.Configuration.AddConfiguration(settings);
            builder.AddServerConfiguration(settings);

            builder.Services
                .AddAfterEffectsBridge()
                .AddAfterEffectsHost()
                .AddDiagnostics()
                .AddSkills()
                .AddFarm()
                .AddWorkers()
                .AddStorage()
                .AddWebAccess()
                .AddMcpHttpTransport();

            return builder;
        }
    }

    extension(WebApplication app)
    {
        public WebApplication UseHttpServer()
        {
            app.Urls.Clear();
            app.Urls.Add(app.Services.GetRequiredService<ServerConfig>().Http.Url);

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapHealth();
            app.MapRawFiles();
            app.MapPages();
            app.MapMcp(McpPath).RequireAuthorization(WebAccess.ClientPolicy);

            return app;
        }
    }
}
