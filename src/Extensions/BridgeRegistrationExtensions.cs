using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Adapters.Bridge;
using Snail.MCP.AfterEffects.Adapters.Programs;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Announcements;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>The link into After Effects: the token the bridge presents, the announcement it reads, the listener it dials and the links it opens.</summary>
internal static class BridgeRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAfterEffectsBridge()
        {
            services.AddSingleton<BridgeToken>();
            services.AddSingleton<ServerAnnouncement>();
            services.AddSingleton<BridgeLinks>();
            services.AddSingleton<IAfterEffectsBridge>(provider => provider.GetRequiredService<BridgeLinks>());
            services.AddSingleton<BridgeListener>();
            services.AddHostedService(provider => provider.GetRequiredService<BridgeListener>());
            services.AddSingleton<IProgramRunner, JavaScriptPrograms>();

            return services;
        }
    }
}
