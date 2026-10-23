using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The HTTP contract of the real server process, the way a container runs it: nothing without a key, liveness for anyone, and the same tools as over stdio.</summary>
public class HttpContractTests
{
    private const string Token = "http-contract-token";

    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(60);

    [Fact]
    public async Task Server_OverHttp_AdmitsOnlyItsToken_AndServesTheToolsOfTheStdioServer()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        using var http = new HttpClient { BaseAddress = server.Address };

        var health = JsonNode.Parse(await http.GetStringAsync("/healthz"))!;
        var anonymous = await http.PostAsync("/mcp", JsonBody());
        var stranger = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post, "/mcp") { Content = JsonBody(), Headers = { Authorization = new AuthenticationHeaderValue("Bearer", "guess") } });

        Assert.Equal("ok", health["status"]!.ToString());
        Assert.Equal("down", health["afterEffects"]!.ToString());
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, stranger.StatusCode);

        await using var client = await McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(server.Address, HttpServerRegistrationExtensions.McpPath),
            AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {Token}" },
        }));

        var tools = (await client.ListToolsAsync()).Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal);
        var diagnosed = await client.CallToolAsync("ae_diagnose");
        var numbered = await client.CallToolAsync("ae_item_info", new Dictionary<string, object?> { ["item"] = 7 });
        await client.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "animation" });
        var withSkill = (await client.ListToolsAsync()).Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal);

        Assert.Equal(ServerGuidance.Instructions, client.ServerInstructions);
        Assert.Equal(BaseToolNamesOfAssembly(), tools);
        Assert.NotEqual(true, diagnosed.IsError);
        Assert.Contains("No bridge", numbered.Content.OfType<TextContentBlock>().First().Text, StringComparison.Ordinal);
        Assert.Contains("ae_set_keyframes", withSkill);
    }

    /// <summary>A client that opens with initialize keeps a session, and the session is what tells it a skill's tools arrived.</summary>
    [Fact]
    public async Task Server_OverHttp_TellsASessionClient_ThatASkillsToolsArrived()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        await using var client = await McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(server.Address, HttpServerRegistrationExtensions.McpPath),
                AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {Token}" },
            }),
            new McpClientOptions { ProtocolVersion = "2025-11-25" });

        await using var subscription = client.RegisterNotificationHandler(NotificationMethods.ToolListChangedNotification, (_, _) =>
        {
            changed.TrySetResult();

            return ValueTask.CompletedTask;
        });

        await client.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "text" });
        await changed.Task.WaitAsync(Timeout);

        Assert.False(string.IsNullOrEmpty(client.SessionId));
        Assert.Contains("ae_text_style", (await client.ListToolsAsync()).Select(tool => tool.Name));
    }

    /// <summary>One server over HTTP serves several clients, and a tool list belongs to one of them: the skill one client loads is in its list and in no other.</summary>
    [Fact]
    public async Task Server_OverHttp_KeepsTheToolsOfOneClient_OutOfAnothersList()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        await using var titles = await ClientAsync(server, "titles");
        await using var edit = await ClientAsync(server, "edit");

        await titles.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "shapes" });
        var titled = (await titles.ListToolsAsync()).Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal);
        var edited = (await edit.ListToolsAsync()).Select(tool => tool.Name).ToHashSet(StringComparer.Ordinal);

        Assert.Contains("ae_add_shape", titled);
        Assert.DoesNotContain("ae_add_shape", edited);
        Assert.Equal(BaseToolNamesOfAssembly(), edited);
    }

    /// <summary>A client is who its key says it is, and may do what the key opens: a key minted for reading names its holder on every command
    /// and turns away the call that would change the project, whatever header the caller sends; a program is refused at the command that destroys.</summary>
    [Fact]
    public async Task Server_OverHttp_NamesAClientByItsKey_AndRefusesWorkTheKeyDoesNotOpen()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        var keys = new ClientKeys(new ServerConfig { DataDirectory = server.DataDirectory }, TimeProvider.System);
        var (_, reader) = keys.Mint("viewer", [Scopes.Read]);
        var (_, maker) = keys.Mint("maker", [Scopes.Read, Scopes.Write]);

        await using var viewer = await ClientAsync(server, agent: "pretender", key: reader);
        await using var builder = await ClientAsync(server, agent: "maker", key: maker);

        static string Text(CallToolResult result) => result.Content.OfType<TextContentBlock>().First().Text;

        var diagnosed = await viewer.CallToolAsync("ae_diagnose");
        var written = await viewer.CallToolAsync("ae_create_comp", new Dictionary<string, object?> { ["name"] = "Main" });
        var program = await builder.CallToolAsync("run_program", new Dictionary<string, object?> { ["code"] = "return ae.layer_remove({layers: [1]});" });
        await builder.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "farm" });
        var rendering = await builder.CallToolAsync("ae_farm_submit", new Dictionary<string, object?> { ["comp"] = "Main" });
        var scripted = await builder.CallToolAsync("ae_script", new Dictionary<string, object?> { ["code"] = "1" });

        Assert.NotEqual(true, diagnosed.IsError);
        Assert.Equal("viewer", JsonNode.Parse(Text(diagnosed))!["data"]!["bridge"]!["agent"]!.ToString());
        Assert.True(written.IsError);
        Assert.Contains("does not open it", Text(written), StringComparison.Ordinal);
        Assert.Contains("delete", Text(program), StringComparison.Ordinal);
        Assert.Contains("does not open", Text(program), StringComparison.Ordinal);
        Assert.Contains("farm work", Text(rendering), StringComparison.Ordinal);
        Assert.Contains("script work", Text(scripted), StringComparison.Ordinal);
    }

    /// <summary>Revoking a key turns its holder away at once: the list is read again when it changes, because a key is revoked from a command
    /// line while the server it admits is running.</summary>
    [Fact]
    public async Task Client_WhoseKeyWasRevoked_IsTurnedAwayWithoutRestartingTheServer()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        var keys = new ClientKeys(new ServerConfig { DataDirectory = server.DataDirectory }, TimeProvider.System);
        var (_, secret) = keys.Mint("titles");

        await using (var admitted = await ClientAsync(server, agent: "titles", key: secret))
        {
            Assert.NotEmpty(await admitted.ListToolsAsync());
        }

        Assert.True(keys.TryRevoke("titles"));

        using var http = new HttpClient { BaseAddress = server.Address };
        var refused = await http.SendAsync(new HttpRequestMessage(HttpMethod.Post, HttpServerRegistrationExtensions.McpPath)
        {
            Content = JsonBody(),
            Headers = { Authorization = new AuthenticationHeaderValue("Bearer", secret) },
        });

        Assert.Equal(HttpStatusCode.Unauthorized, refused.StatusCode);
    }

    /// <summary>A server started without its secret would turn every client away while looking healthy, so it refuses to start and names the setting.</summary>
    [Fact]
    public async Task Server_OverHttpWithoutAToken_ExitsBeforeListening_NamingTheSetting()
    {
        await using var server = await HttpServerProcess.StartAsync(token: null, waitForHealth: false);

        var exit = await server.WaitForExitAsync();

        Assert.NotEqual(0, exit);
        Assert.Contains("SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN", server.Output, StringComparison.Ordinal);
    }

    /// <summary>The scopes are not only about tool calls: a key minted to read cannot write or delete over the file endpoints either, and a link
    /// signed for one file opens that file without any key.</summary>
    [Fact]
    public async Task Files_OverHttp_AreOpenedByTheScopesOfTheKey_AndBySignedLinks()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        var config = new ServerConfig { DataDirectory = server.DataDirectory, Http = { Token = Token, Url = server.Address.ToString() } };
        var keys = new ClientKeys(config, TimeProvider.System);
        var (_, reader) = keys.Mint("viewer", [Scopes.Read]);
        var (_, writer) = keys.Mint("editor", [Scopes.Read, Scopes.Write]);
        using var http = new HttpClient { BaseAddress = server.Address };

        Assert.Equal(HttpStatusCode.Forbidden, (await http.SendAsync(Signed(HttpMethod.Put, "/raw/files/shot.txt", reader))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.SendAsync(Signed(HttpMethod.Delete, "/raw/files/shot.txt", writer))).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await http.SendAsync(Signed(HttpMethod.Put, "/raw/files/shot.txt", writer))).StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, (await http.SendAsync(Signed(HttpMethod.Put, "/raw/files/shot.txt?overwrite=true", writer))).StatusCode);

        var read = await http.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/raw/files/shot.txt") { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", reader) } });
        var link = new FileLinks(new ClientToken(config), config, TimeProvider.System).For("GET", "files", "shot.txt");
        var linked = await http.GetAsync(link.Url);
        var anonymous = await http.GetAsync("/raw/files/shot.txt");

        Assert.Equal(HttpStatusCode.OK, read.StatusCode);
        Assert.Equal([1, 2, 3], await read.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.OK, linked.StatusCode);
        Assert.Equal([1, 2, 3], await linked.Content.ReadAsByteArrayAsync());
        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
    }

    /// <summary>The pages are for a signed-in browser: the login page is open, everything else sends a browser to it, and a bearer does not sign a page in.</summary>
    [Fact]
    public async Task Pages_OverHttp_AskABrowserToSignIn_AndServeTheirAssets()
    {
        await using var server = await HttpServerProcess.StartAsync(Token);
        using var http = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false }) { BaseAddress = server.Address };

        var login = await http.GetAsync("/login");
        var status = await http.GetAsync("/");
        var files = await http.GetAsync("/files");
        var css = await http.GetAsync("/assets/pages.css");

        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Contains("Sign in", await login.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, status.StatusCode);
        Assert.StartsWith("/login", status.Headers.Location!.ToString(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Redirect, files.StatusCode);
        Assert.Equal(HttpStatusCode.OK, css.StatusCode);
    }

    private static HttpRequestMessage Signed(HttpMethod method, string path, string key) =>
        new(method, path) { Headers = { Authorization = new AuthenticationHeaderValue("Bearer", key) }, Content = new ByteArrayContent([1, 2, 3]) };

    private static StringContent JsonBody() => new("""{"jsonrpc":"2.0","id":1,"method":"tools/list"}""", MediaTypeHeaderValue.Parse("application/json"));

    /// <summary>A client that says who it is, the way a studio tells two of them apart; with a key of its own, the key is what names it.</summary>
    private static Task<McpClient> ClientAsync(HttpServerProcess server, string agent, string? key = null) =>
        McpClient.CreateAsync(new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(server.Address, HttpServerRegistrationExtensions.McpPath),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {key ?? Token}",
                [TransportRegistrationExtensions.AgentHeader] = agent,
            },
        }));

    private static HashSet<string> BaseToolNamesOfAssembly() =>
        SkillRegistrationExtensions.BaseToolTypes(typeof(ToolDescriptions).Assembly)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Select(method => method.GetCustomAttribute<McpServerToolAttribute>()?.Name)
            .OfType<string>()
            .ToHashSet(StringComparer.Ordinal);
}
