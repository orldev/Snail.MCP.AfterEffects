using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.Sdk.Mcp.Hosting.Sessions;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>The scopes of a client's key as the gate Snail.Sdk asks before every call, for the list a session is shown, and for every command
/// a program sends.</summary>
/// <remarks>A name that is one of the server's tools is judged as that tool (<see cref="ToolScopes.For(McpServerTool)"/>); any other name is a
/// bridge command a program or run_tools sends, judged as that command (<see cref="ToolScopes.For(string)"/>). No tool of the server is named
/// like a command: the tools carry the ae_ prefix, and find_tool, run_tools and run_program are no commands of the bridge.</remarks>
internal sealed class ScopeGate(ToolScopes scopes, ClientSessions clients, IOptions<McpServerOptions> server) : IToolGate
{
    public string? Refusal(string tool, IServiceProvider services)
    {
        var client = clients.OfRequest();
        var scope = server.Value.ToolCollection is { } tools && tools.TryGetPrimitive(tool, out var found) ? scopes.For(found) : scopes.For(tool);

        return client.May(scope) ? null : ToolScopes.Refusal(tool, scope, client.Name);
    }
}
