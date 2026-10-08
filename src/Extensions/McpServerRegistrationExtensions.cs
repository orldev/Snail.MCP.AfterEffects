using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Composition root: lists the features to wire; how the transport or the listener is configured is known one level down.</summary>
internal static class McpServerRegistrationExtensions
{
    extension(HostApplicationBuilder builder)
    {
        /// <summary>Binds the server to <paramref name="settings"/>, the ones the transport was chosen from, or to this machine's when none are given.</summary>
        public HostApplicationBuilder ConfigureMcpServer(IConfiguration? settings = null)
        {
            builder.AddServerConfiguration(settings ?? ServerConfigExtensions.Settings());

            builder.Services
                .AddAfterEffectsBridge()
                .AddAfterEffectsHost()
                .AddDiagnostics()
                .AddSkills()
                .AddFarm()
                .AddWorkers()
                .AddStorage()
                .AddIdleShutdownWatcher()
                .AddMcpTransport();

            return builder;
        }
    }
}
