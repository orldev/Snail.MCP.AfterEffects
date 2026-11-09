using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.Sdk.Mcp.Hosting.Sessions;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Who calls: the clients the server tells apart, the scopes their keys open, and the gate Snail.Sdk asks before every call.</summary>
internal static class ClientRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddClients()
        {
            services.AddSingleton(provider => new ClientSessions(provider.GetRequiredService<ServerConfig>().Agent, provider.GetService<IClientIdentity>()));
            services.AddSingleton<CommandSemantics>();
            services.AddSingleton<ToolScopes>();
            services.AddSingleton<IToolGate, ScopeGate>();

            return services;
        }
    }

    extension(IMcpServerBuilder builder)
    {
        /// <summary>Every tool class of the assembly, the skills' among them: a skill's tools stay out of a session's list until it loads the skill.</summary>
        /// <remarks>The cast picks the overload that scans the listed types; without it the call binds to the generic overload that treats the
        /// list itself as a tool instance and registers nothing.</remarks>
        public IMcpServerBuilder WithAllTools() =>
            builder.WithTools((IEnumerable<Type>)ToolTypes(typeof(ClientRegistrationExtensions).Assembly));
    }

    public static IReadOnlyList<Type> ToolTypes(Assembly assembly) =>
        [.. assembly.GetTypes().Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)];
}
