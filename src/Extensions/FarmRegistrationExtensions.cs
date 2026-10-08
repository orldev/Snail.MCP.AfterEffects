using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Adapters.AfterEffects;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Application.Transfer;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>The render farm: aerender beside After Effects, and the queue of background jobs it runs.</summary>
internal static class FarmRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFarm()
        {
            services.AddSingleton<IAerender, Aerender>();
            services.AddSingleton<ProjectForRender>();
            services.AddSingleton<RenderFarm>();
            services.AddHostedService(provider => provider.GetRequiredService<RenderFarm>());

            return services;
        }

        /// <summary>The areas of files this server keeps, the links that open them over HTTP, and the commands a client moves them with.</summary>
        public IServiceCollection AddStorage()
        {
            services.TryAddSingleton(TimeProvider.System);
            services.TryAddSingleton<ClientToken>();
            services.AddSingleton<Volume>();
            services.AddSingleton<FileLinks>();
            services.AddSingleton<ClientCommands>();

            return services;
        }
    }
}
