using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Adapters.AfterEffects;
using Snail.MCP.AfterEffects.Application.Installation;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>After Effects as this machine sees it, and the installation of the bridge into it.</summary>
internal static class AfterEffectsHostRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddAfterEffectsHost()
        {
            if (OperatingSystem.IsWindows())
            {
                services.AddSingleton<IAfterEffectsHost, WindowsAfterEffectsHost>();
            }
            else
            {
                services.AddSingleton<IAfterEffectsHost, MacAfterEffectsHost>();
            }

            services.AddSingleton<BridgeInstallation>();
            services.AddSingleton<Application.History.UndoHistory>();

            return services;
        }
    }
}
