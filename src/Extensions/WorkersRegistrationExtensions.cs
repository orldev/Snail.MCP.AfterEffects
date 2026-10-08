using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Adapters.AfterEffects;
using Snail.MCP.AfterEffects.Application.Workers;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>The worker pool: After Effects instances this server launches and lends to clients.</summary>
internal static class WorkersRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddWorkers()
        {
            services.AddSingleton<IWorkerLauncher, AfterEffectsLauncher>();
            services.AddSingleton<WorkerPool>();
            services.AddHostedService(provider => provider.GetRequiredService<WorkerPool>());

            return services;
        }
    }
}
