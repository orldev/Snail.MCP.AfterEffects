using Snail.MCP.AfterEffects.Adapters.Bridge;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Announcements;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The server side of the bridge, built the way the composition root builds it, over a data directory of its own.</summary>
internal sealed class ListeningServer : IAsyncDisposable
{
    private ListeningServer(string dataDirectory, ServerConfig config, BridgeToken token, BridgeLinks links, ServerAnnouncement announcement, BridgeListener listener)
    {
        DataDirectory = dataDirectory;
        Config = config;
        Token = token;
        Links = links;
        Announcement = announcement;
        Listener = listener;
    }

    public string DataDirectory { get; }

    public ServerConfig Config { get; }

    public BridgeToken Token { get; }

    public BridgeLinks Links { get; }

    public ServerAnnouncement Announcement { get; }

    public BridgeListener Listener { get; }

    public int Port => Announcement.Port;

    public static async Task<ListeningServer> StartAsync(string? agent = null)
    {
        var dataDirectory = TestFolder.Create();
        var config = new ServerConfig { DataDirectory = dataDirectory, Agent = agent };
        ServerConfigExtensions.Normalize(config);

        var token = new BridgeToken(config.Bridge, NullLogger<BridgeToken>.Instance);
        var links = new BridgeLinks(config.Bridge);
        var announcement = new ServerAnnouncement(config, NullLogger<ServerAnnouncement>.Instance);
        var listener = new BridgeListener(config.Bridge, token, links, announcement, NullLogger<BridgeListener>.Instance);

        await listener.StartAsync(CancellationToken.None);
        await Until(() => announcement.Port > 0, TimeSpan.FromSeconds(5));

        return new ListeningServer(dataDirectory, config, token, links, announcement, listener);
    }

    public Task UntilConnectedAsync(TimeSpan? within = null) => Until(() => Links.IsConnected, within ?? TimeSpan.FromSeconds(5));

    public Task UntilDisconnectedAsync(TimeSpan? within = null) => Until(() => !Links.IsConnected, within ?? TimeSpan.FromSeconds(5));

    public static async Task Until(Func<bool> condition, TimeSpan within)
    {
        var deadline = DateTime.UtcNow + within;

        while (!condition())
        {
            if (DateTime.UtcNow > deadline) throw new TimeoutException("the condition did not come true in time");

            await Task.Delay(25);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await Listener.StopAsync(CancellationToken.None);
        await Links.DisposeAsync();
        Listener.Dispose();

        if (Directory.Exists(DataDirectory)) Directory.Delete(DataDirectory, recursive: true);
    }
}
