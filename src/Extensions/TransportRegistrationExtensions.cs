using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Adapters.Programs;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.MCP.AfterEffects.Web.Access;
using Snail.Sdk.Mcp.Hosting;
using Snail.Sdk.Mcp.Hosting.Discovery;
using Snail.Sdk.Mcp.Hosting.Jobs;
using Snail.Sdk.Mcp.Hosting.Sessions;
using Snail.Sdk.Mcp.Http;
using Snail.Sdk.Mcp.Programs;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Driving adapter: tools reach the model over stdio or HTTP, guidance rides along with initialize.</summary>
internal static class TransportRegistrationExtensions
{
    /// <summary>What a client calls itself over HTTP, which becomes the agent name on its commands when its key lets it name itself.</summary>
    public const string AgentHeader = "X-Snail-Agent";

    /// <summary>The short name the skill tools are named by — ae_skills, ae_enable_skill, ae_disable_skill — and the object a program calls the
    /// commands on: the prefix every tool of this server carries, rather than the server's folder name.</summary>
    public const string ShortName = "ae";

    extension(IServiceCollection services)
    {
        /// <summary>Registers the stdio MCP server.</summary>
        public IServiceCollection AddMcpTransport(ServerConfig config)
        {
            services.AddMcpServerWithTools(config).WithStdioServerTransport();

            return services;
        }

        /// <summary>Registers the same server over Streamable HTTP, each request served as the client whose key it presented.</summary>
        /// <remarks>A client that opens with initialize keeps a session, which is what carries tools/list_changed when a skill loads; a client of
        /// the sessionless protocol revision is served per request and holds no stream, and Snail.Sdk tells it in the reply to the call.</remarks>
        public IServiceCollection AddMcpHttpTransport(ServerConfig config)
        {
            services.AddSingleton<IClientIdentity, RequestIdentity>();
            services.AddMcpServerWithTools(config)
                .WithHttpTransport(options => options.SessionMode = HttpServerSessionMode.StatefulForInitializeClients)
                .WithSessionAddress();

            return services;
        }

        /// <summary>The one server both transports carry: its name and version, the guidance, the tools and the ways in Snail.Sdk spells for
        /// every server — the scopes held at the door, find_tool, skills loaded per session, run_program — the prompts and the client in hand.</summary>
        /// <remarks>The server names itself through ServerIdentity, by its command and the informational version: left to the SDK, it reports
        /// the assembly version, which reads 0.2.0.0 for the release 0.2.0 and drops the suffix of a prerelease, so a client would see another
        /// version than ae_diagnose. run_tools stays the server's own: its steps are bridge commands sent as one sequence, one undo step in
        /// After Effects, which Snail.Sdk's run_tools of tool calls cannot be.</remarks>
        private IMcpServerBuilder AddMcpServerWithTools(ServerConfig config) =>
            services
                .AddMcpServer(server =>
                {
                    server.ServerInfo = ServerIdentity.Of(typeof(TransportRegistrationExtensions).Assembly);
                    server.ServerInstructions = ServerGuidance.Instructions;
                })
                .ExplainFailures()
                .WithAllTools()
                .WithPrompts<PipelinePrompts>()
                .WithJobs("aftereffects")
                .WithSessionGrant(Granting)
                .WithArgumentCheck()
                .WithToolDiscovery(discovery =>
                {
                    discovery.Prefix = Application.Discovery.ToolCommands.Prefix;
                    discovery.Noise = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ShortName };
                    discovery.CommandOf = Application.Discovery.ToolCommands.CommandOf;
                    discovery.ParameterOf = Application.Discovery.ToolCommands.ParameterOf;
                    discovery.Examples = ["ease the keyframes of a position", "add a drop shadow", "render the composition to a file"];
                })
                .WithSkills(skills =>
                {
                    skills.Prefix = ShortName;
                    skills.SessionOf = SkillSetOf;
                    skills.IdleTimeout = TimeSpan.FromMinutes(config.SkillIdleMinutes);
                })
                .WithPrograms<BridgeProgramCommands>(programs =>
                {
                    programs.Global = ShortName;
                    programs.MaxCalls = ProgramLimits.MostCalls;
                    programs.Timeout = TimeSpan.FromSeconds(ProgramLimits.DefaultTimeoutSeconds);
                    programs.MaxTimeout = ToolLimits.Timeout(ToolLimits.MaxTimeoutSeconds);
                    programs.Description = ToolDescriptions.Application.Program;
                })
                .WithRequestFilters(filters => filters.AddCallToolFilter(Serving));

        /// <summary>How a call the client's key does not open is refused: the gate's words, the fleet's type, and this server's hint, which names
        /// the command that mints a key — one hint for the door, for a step of <c>run_tools</c>, and for a command of a <c>run_program</c> program.</summary>
        /// <remarks>Set as <see cref="GrantOptions.HintOf"/>, which every layer reads; a <see cref="GrantOptions.Refusal"/> set only for the hint
        /// reached the door and the forwarders, and a program's refused command said the fleet's default instead.</remarks>
        internal static void Granting(GrantOptions grant) =>
            grant.HintOf = (_, _) => Messages.ScopeHint;

        /// <summary>Whose skills a request loads and lists: a client of the sessionless revision by the name on its key, as the server told
        /// such clients apart before Snail.Sdk kept the skills; a client with a session, and one that names nobody, by its MCP session.</summary>
        private static string? SkillSetOf(IServiceProvider requestServices, McpServer? server) =>
            server?.SessionId is null ? requestServices.GetService<IClientIdentity>()?.Who()?.Name : null;

        /// <summary>Says whose call this is for as long as it runs — the worker its commands go to, the agent they are signed with — and hands a
        /// tool a number or a boolean as the string it takes.</summary>
        /// <remarks>Whether the client may make the call at all is the gate's (<see cref="Application.Access.ScopeGate"/>), asked by Snail.Sdk at
        /// the door before this runs.</remarks>
        private static McpRequestHandler<CallToolRequestParams, CallToolResult> Serving(
            McpRequestHandler<CallToolRequestParams, CallToolResult> next) =>
            async (context, cancellationToken) =>
            {
                var clients = (context.Services ?? context.Server.Services)?.GetService<ClientSessions>();
                using var serving = clients?.Serve(clients.OfRequest(), context.Server.ClientInfo?.Name);

                if (context.MatchedPrimitive is McpServerTool called && Coerced(context.Params, called) is { } coerced)
                {
                    context.Params = coerced;
                }

                return await next(context, cancellationToken);
            };

        /// <summary>A number or a boolean sent where the tool takes a string becomes that string, so a layer named by its index as 3 rather
        /// than "3" reaches the tool instead of failing in the SDK's binding with a message that names no parameter.</summary>
        private static CallToolRequestParams? Coerced(CallToolRequestParams? parameters, McpServerTool tool)
        {
            if (parameters?.Arguments is null || !tool.ProtocolTool.InputSchema.TryGetProperty("properties", out var properties) || properties.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            Dictionary<string, JsonElement>? changed = null;

            foreach (var (key, value) in parameters.Arguments)
            {
                if (value.ValueKind is not (JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False) || !properties.TryGetProperty(key, out var schema) || !TakesString(schema))
                {
                    continue;
                }

                changed ??= new Dictionary<string, JsonElement>(parameters.Arguments, StringComparer.Ordinal);
                changed[key] = JsonSerializer.SerializeToElement(value.GetRawText());
            }

            return changed is null ? null : new CallToolRequestParams { Name = parameters.Name, Arguments = changed, Meta = parameters.Meta };
        }

        private static bool TakesString(JsonElement schema) =>
            schema.TryGetProperty("type", out var type)
            && (type.ValueKind == JsonValueKind.String ? type.GetString() == "string" : type.ValueKind == JsonValueKind.Array && type.EnumerateArray().Any(name => name.GetString() == "string"));
    }
}
