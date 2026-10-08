using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Skills: the on-demand tool groups, one catalog per client, the index that finds a tool by intent, the scopes, and the watchdog that unloads idle skills.</summary>
internal static class SkillRegistrationExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddSkills()
        {
            services.TryAddSingleton(TimeProvider.System);
            services.AddSingleton(SkillCatalog.Discover(typeof(SkillAttribute).Assembly));
            services.AddSingleton(provider => new ClientSessions(
                provider.GetRequiredService<IReadOnlyList<Skill>>(),
                provider.GetRequiredService<IOptions<McpServerOptions>>().Value.ToolCollection ??= [],
                provider.GetRequiredService<ServerConfig>().Agent,
                provider,
                provider.GetRequiredService<TimeProvider>()));
            services.AddTransient(provider => provider.GetRequiredService<ClientSessions>().Current.Skills);
            services.AddSingleton(new ToolIndex(typeof(SkillAttribute).Assembly));
            services.AddSingleton<CommandSemantics>();
            services.AddSingleton<ToolScopes>();
            services.AddSingleton(provider => SkillExpiryOptions.For(provider.GetRequiredService<ServerConfig>().SkillIdleMinutes));
            services.AddHostedService<SkillExpiry>();

            return services;
        }
    }

    extension(IMcpServerBuilder builder)
    {
        /// <summary>Every tool class of the assembly that belongs to no skill: the always-on layer.</summary>
        /// <remarks>The cast picks the overload that scans the listed types; without it the call binds to the generic
        /// overload that treats the list itself as a tool instance and registers nothing.</remarks>
        public IMcpServerBuilder WithBaseTools() =>
            builder.WithTools((IEnumerable<Type>)BaseToolTypes(typeof(SkillAttribute).Assembly));
    }

    public static IReadOnlyList<Type> BaseToolTypes(Assembly assembly) =>
        [.. assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .Where(type => type.GetCustomAttribute<SkillAttribute>() is null)];
}
