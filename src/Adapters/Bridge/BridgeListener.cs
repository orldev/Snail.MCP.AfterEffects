using System.Net;
using System.Net.Sockets;
using Microsoft.Extensions.Hosting;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Announcements;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Adapters.Bridge;

/// <summary>The loopback listener the bridge dials: it reads the hello, checks the token, welcomes or refuses, and hands the connection to the links.</summary>
/// <remarks>Listening on the loopback only: the bridge is always on this machine, and a port open to the network would be a port open to
/// anyone who knows the token. The token is minted before the announcement goes out and the announcement only once the listener is up: the
/// bridge reads the token file when it sees the announcement, and a bridge that read none would be refused and wait a minute before trying again.</remarks>
internal sealed class BridgeListener(
    BridgeOptions options,
    BridgeToken token,
    BridgeLinks links,
    ServerAnnouncement announcement,
    ILogger<BridgeListener> logger)
    : BackgroundService
{
    /// <summary>How long a connection may stay silent before its hello, after which it is not a bridge.</summary>
    public static readonly TimeSpan HelloWithin = TimeSpan.FromSeconds(5);

    private TcpListener? _listener;

    /// <summary>The port the listener took; 0 before it is up.</summary>
    public int Port => (_listener?.LocalEndpoint as IPEndPoint)?.Port ?? 0;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _listener = new TcpListener(BindAddress(options), options.Port);
        _listener.Start();
        Log.BridgeTokenSource(logger, token.Source);
        announcement.Publish(Port);
        Log.ListeningForBridge(logger, BindAddress(options), Port);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(stoppingToken).ConfigureAwait(false);
                _ = AdmitAsync(client, stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (SocketException failure)
        {
            Log.BridgeListenerStopped(logger, failure.Message);
        }
        finally
        {
            announcement.Withdraw();
            _listener.Stop();
        }
    }

    /// <summary>Reads the first line of a connection and either welcomes the bridge behind it or sends it away; a connection that is not
    /// welcomed takes its reader and its half-built link with it, since nobody else holds them.</summary>
    private async Task AdmitAsync(TcpClient client, CancellationToken stoppingToken)
    {
        var reader = BridgeLink.ReaderOf(client);
        var admitted = false;
        BridgeLink? link = null;

        try
        {
            using var patience = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
            patience.CancelAfter(HelloWithin);

            var first = await reader.ReadLineAsync(patience.Token).ConfigureAwait(false);
            var hello = first is null ? null : BridgeHello.From(Parse(first));

            if (hello is null)
            {
                await RefuseAsync(client, "the first line was not a hello", stoppingToken).ConfigureAwait(false);

                return;
            }

            if (!token.Matches(hello.Token))
            {
                Log.BridgeRefused(logger, hello.App);
                await RefuseAsync(client, "token", stoppingToken).ConfigureAwait(false);

                return;
            }

            link = new BridgeLink(client, reader, hello, logger);
            await link.TellAsync(Welcome(), stoppingToken).ConfigureAwait(false);
            links.Admit(link);
            admitted = true;
            Log.BridgeWelcomed(logger, hello.Version, hello.Digest, hello.App, hello.Commands);
        }
        catch (Exception failure) when (failure is IOException or SocketException or OperationCanceledException or ObjectDisposedException)
        {
            client.Dispose();
        }
        finally
        {
            if (!admitted)
            {
                if (link is not null)
                {
                    await link.DisposeAsync().ConfigureAwait(false);
                }

                reader.Dispose();
            }
        }
    }

    private static JsonObject Welcome() => new()
    {
        ["welcome"] = new JsonObject
        {
            ["name"] = ThisAssembly.Name,
            ["version"] = ThisAssembly.InformationalVersion,
            ["protocol"] = BridgeProtocol.Version,
            ["pid"] = Environment.ProcessId,
        },
    };

    private static async Task RefuseAsync(TcpClient client, string reason, CancellationToken cancellationToken)
    {
        var line = new JsonObject { ["refused"] = new JsonObject { ["reason"] = reason } }.ToJsonString();
        var bytes = System.Text.Encoding.UTF8.GetBytes($"{line}\n");

        try
        {
            await client.GetStream().WriteAsync(bytes, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            client.Dispose();
        }
    }

    private static JsonNode? Parse(string line)
    {
        try
        {
            return JsonNode.Parse(line);
        }
        catch (System.Text.Json.JsonException)
        {
            return null;
        }
    }

    public override void Dispose()
    {
        _listener?.Dispose();
        base.Dispose();
    }

    /// <summary>Where to listen: what the settings name, else the loopback when the announced host is the loopback and every interface otherwise.</summary>
    private static IPAddress BindAddress(BridgeOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.Listen))
        {
            return IPAddress.TryParse(options.Listen, out var named) ? named : IPAddress.Any;
        }

        return IPAddress.TryParse(options.Host, out var host) && IPAddress.IsLoopback(host) ? IPAddress.Loopback : IPAddress.Any;
    }
}
