using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Hosting;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Host lifetime: the idle watchdog that exits an orphaned server process.</summary>
internal static class IdleShutdownRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddIdleShutdownWatcher()
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton<IActivityTracker, ActivityTracker>();
            services.AddSingleton(provider => IdleShutdownOptions.For(provider.GetRequiredService<ServerConfig>().IdleTimeoutMinutes));
            services.AddHostedService<IdleShutdownWatcher>();

            return services;
        }
    }
}
