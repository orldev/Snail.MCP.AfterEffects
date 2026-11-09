using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Configuration;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Composition root: lists the features to wire; how the transport or the listener is configured is known one level down.</summary>
internal static class McpServerRegistrationExtensions
{
    extension(HostApplicationBuilder builder)
    {
        /// <summary>Binds the server to <paramref name="settings"/>, the ones the transport was chosen from, or to this machine's when none are given.</summary>
        /// <remarks>The idle watchdog is Snail.Sdk's and off unless IdleTimeoutMinutes sets it: a client that ends closes the pipe, and the
        /// stdio server exits within a second without it.</remarks>
        public HostApplicationBuilder ConfigureMcpServer(IConfiguration? settings = null)
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
                .AddIdleShutdown(TimeSpan.FromMinutes(config.IdleTimeoutMinutes), IsRendering)
                .AddMcpTransport(config);

            return builder;
        }
    }

    /// <summary>Whether a render of the farm is queued or running: a client that waits for its frames sends nothing meanwhile,
    /// and the idle shutdown would otherwise stop the server, and the renders it follows, under it.</summary>
    private static bool IsRendering(IServiceProvider services) =>
        services.GetService<RenderFarm>()?.Jobs.Any(job => job.IsActive) == true;
}
