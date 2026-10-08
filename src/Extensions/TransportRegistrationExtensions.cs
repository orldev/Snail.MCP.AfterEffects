using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol.AspNetCore;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Hosting;
using Snail.MCP.AfterEffects.Tools;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.MCP.AfterEffects.Web.Access;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Extensions;

/// <summary>Driving adapter: tools reach the model over stdio or HTTP, guidance rides along with initialize.</summary>
internal static class TransportRegistrationExtensions
{
    /// <summary>What a client calls itself over HTTP, which becomes the agent name on its commands when its key lets it name itself.</summary>
    public const string AgentHeader = "X-Snail-Agent";

    extension(IServiceCollection services)
    {
        /// <summary>Registers the stdio MCP server with the base tools; skills join the collection later. Each call marks the server as active.</summary>
        public IServiceCollection AddMcpTransport()
        {
            services.AddMcpServerWithTools().WithStdioServerTransport();

            return services;
        }

        /// <summary>Registers the same server over Streamable HTTP.</summary>
        /// <remarks>A client that opens with initialize keeps a session, which is what carries tools/list_changed when a skill loads; a client of
        /// the sessionless protocol revision is served per request and holds no stream, so the tools that load or unload a skill tell it in the
        /// reply to the call.</remarks>
        public IServiceCollection AddMcpHttpTransport()
        {
            services.AddMcpServerWithTools().WithHttpTransport(options =>
            {
                options.SessionMode = HttpServerSessionMode.StatefulForInitializeClients;
                options.ConfigureSessionOptions = OpenASessionAsync;
            });

            return services;
        }

        /// <summary>The one server both transports carry: its name and version, the guidance, the base tools, the prompts and the activity filter.</summary>
        /// <remarks>The server names itself through ServerIdentity, by its command and the informational version: left to the SDK, it reports
        /// the assembly version, which reads 0.2.0.0 for the release 0.2.0 and drops the suffix of a prerelease, so a client would see another
        /// version than ae_diagnose.</remarks>
        private IMcpServerBuilder AddMcpServerWithTools() =>
            services
                .AddMcpServer(server =>
                {
                    server.ServerInfo = ServerIdentity.Of(typeof(ThisAssembly).Assembly);
                    server.ServerInstructions = ServerGuidance.Instructions;
                })
                .WithBaseTools()
                .WithPrompts<PipelinePrompts>()
                .WithRequestFilters(filters => filters.AddCallToolFilter(TrackActivity));

        /// <summary>Every session and every sessionless request gets options of its own, and with them a client of its own: a tool collection
        /// holding the always-on tools, and the name its commands are signed with.</summary>
        private static Task OpenASessionAsync(HttpContext context, McpServerOptions session, CancellationToken cancellationToken)
        {
            context.RequestServices.GetRequiredService<ClientSessions>().Open(session, Whom(context), Opens(context));

            return Task.CompletedTask;
        }

        /// <summary>The kinds of work the key this request presented opens; a request admitted some other way opens all of them.</summary>
        private static IReadOnlyList<string>? Opens(HttpContext context) =>
            context.User.FindAll(WebAccess.ScopeClaim).Select(claim => claim.Value).ToList() is { Count: > 0 } scopes ? scopes : null;

        /// <summary>Who this request is: the name on the key it presented. Only the configured token lets its holder name itself with the header.</summary>
        private static string? Whom(HttpContext context) =>
            context.User.FindFirst(WebAccess.ConfiguredClaim) is null
                ? context.User.Identity?.Name
                : context.Request.Headers[AgentHeader].ToString() is { Length: > 0 } named ? named : context.User.Identity?.Name;

        /// <summary>Marks the server and the calling client's skill as active, says whose call this is for as long as it runs, and turns the call
        /// away when the client's key does not open work of its kind.</summary>
        /// <remarks>The refusal is here rather than in each tool because it is about the caller, not about the call: a tool that had to ask
        /// would be a tool that could forget to.</remarks>
        private static McpRequestHandler<CallToolRequestParams, CallToolResult> TrackActivity(
            McpRequestHandler<CallToolRequestParams, CallToolResult> next) =>
            async (context, cancellationToken) =>
            {
                context.Services?.GetService<IActivityTracker>()?.Touch();

                var clients = context.Services?.GetService<ClientSessions>();
                using var serving = clients?.Serve(context.Server);

                if (clients is not null
                    && context.Services?.GetService<ToolScopes>() is { } scopes
                    && context.MatchedPrimitive is McpServerTool tool
                    && scopes.For(tool) is { } scope
                    && !clients.Current.May(scope))
                {
                    return ToolResponse.Failure(ToolScopes.Refusal(tool.ProtocolTool.Name, scope, clients.Current.Name), Messages.ScopeHint);
                }

                clients?.Current.Skills.Touch(context.Params?.Name);

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
