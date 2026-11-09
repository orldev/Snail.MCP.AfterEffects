using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Hosting;
using Snail.Sdk.Mcp.Hosting;

try
{
    if (ClientCommandLine.IsAsked(args))
    {
        return ClientCommandLine.Run(args, Console.Out);
    }

    var settings = ServerConfigExtensions.Settings();
    var endpoint = McpEndpoint.Read(settings, ServerConfigExtensions.DefaultUrl, ServerConfigExtensions.EndpointOptions);

    if (endpoint.Transport is McpTransport.Http)
    {
        await HttpServerRegistrationExtensions.CreateHttpBuilder(args, endpoint, settings)
            .Build()
            .UseHttpServer()
            .RunAsync();

        return 0;
    }

    await McpHost.CreateBuilder(args, settings)
        .ConfigureMcpServer(settings)
        .Build()
        .RunAsync();

    return 0;
}
catch (StartRefusedException refused)
{
    await Console.Error.WriteLineAsync(refused.Message);

    return 1;
}
