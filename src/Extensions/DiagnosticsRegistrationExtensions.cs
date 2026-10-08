using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Diagnostics;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>What the server can say about itself: the health report behind ae_diagnose.</summary>
internal static class DiagnosticsRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDiagnostics()
        {
            services.AddSingleton<ExtensionFreshness>();
            services.AddSingleton<ServerHealth>();
            services.AddSingleton<Application.Pictures.Captures>();

            return services;
        }
    }
}
