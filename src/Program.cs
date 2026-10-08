using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Hosting;
using Snail.Sdk.Mcp.Hosting;

ServerFolders.MoveFromSnailNames(ServerPaths.ShortName);

if (ClientCommandLine.IsAsked(args))
{
    return ClientCommandLine.Run(args, Console.Out);
}

var settings = ServerConfigExtensions.Settings();

if (ServerConfigExtensions.Peek(settings).Transport is Transport.Http)
{
    await WebApplication.CreateBuilder(args)
        .ConfigureHttpServer(settings)
        .Build()
        .UseHttpServer()
        .RunAsync();

    return 0;
}

await McpHost.CreateBuilder(args)
    .ConfigureMcpServer(settings)
    .Build()
    .RunAsync();

return 0;
