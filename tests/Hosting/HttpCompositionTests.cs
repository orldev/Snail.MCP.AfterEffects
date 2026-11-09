using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Extensions;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Tests.Hosting;

/// <summary>The HTTP server composed as the program composes it, read without starting it: what is built on its address names the address it listens on.</summary>
public sealed class HttpCompositionTests
{
    /// <summary><c>SNAIL_MCP_AFTEREFFECTS_URL</c> (<c>Url</c> at the root) or a <c>Port</c> moves the listener away from <c>Http:Url</c>; a link signed
    /// with <c>Http:Url</c> led to an address nothing listened on.</summary>
    [Theory]
    [InlineData("Url")]
    [InlineData("Port")]
    public void Links_WhenASettingMovesTheListener_LeadToTheAddressItListensOn(string moving)
    {
        var port = Ports.Free();
        var listening = $"http://127.0.0.1:{port}";
        var settings = Settings(new()
        {
            [moving] = moving == "Port" ? port.ToString(CultureInfo.InvariantCulture) : listening,
            [$"{ServerConfig.SectionName}:Http:Url"] = "http://127.0.0.1:8080",
        });
        using var app = Build(settings);

        var link = app.Services.GetRequiredService<FileLinks>().For("GET", VolumeAreas.Files, "plate.mov");

        Assert.StartsWith($"{listening}/raw/files/plate.mov?", link.Url, StringComparison.Ordinal);
        Assert.Equal(listening, app.Services.GetRequiredService<ServerConfig>().Http.Url);
    }

    /// <summary>The public address of a reverse proxy still wins over the one the server listens on.</summary>
    [Fact]
    public void Links_BehindAPublicUrl_LeadToThePublicUrl()
    {
        var settings = Settings(new()
        {
            ["Url"] = $"http://127.0.0.1:{Ports.Free()}",
            [$"{ServerConfig.SectionName}:Http:PublicUrl"] = "https://ae.example.com",
        });
        using var app = Build(settings);

        var link = app.Services.GetRequiredService<FileLinks>().For("GET", VolumeAreas.Files, "plate.mov");

        Assert.StartsWith("https://ae.example.com/raw/files/plate.mov?", link.Url, StringComparison.Ordinal);
    }

    private static WebApplication Build(IConfiguration settings)
    {
        var endpoint = McpEndpoint.Read(settings, ServerConfigExtensions.DefaultUrl, ServerConfigExtensions.EndpointOptions);

        return HttpServerRegistrationExtensions.CreateHttpBuilder([], endpoint, settings).Build();
    }

    private static IConfiguration Settings(Dictionary<string, string?> values)
    {
        values[$"{ServerConfig.SectionName}:Transport"] = nameof(McpTransport.Http);
        values[$"{ServerConfig.SectionName}:Http:Token"] = "t";
        values[$"{ServerConfig.SectionName}:DataDirectory"] = TestFolder.Create();

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
