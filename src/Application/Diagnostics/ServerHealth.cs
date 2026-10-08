using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Announcements;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Application.Diagnostics;

/// <summary>One report of the server, its configuration, the bridge that has connected and whether After Effects answers a ping right now.</summary>
/// <remarks>The ping has a short timeout of its own: a diagnose call is what a person makes when something is wrong, and a report that waits
/// out a render before it says "busy" answers after the question stopped mattering.</remarks>
internal sealed class ServerHealth(ServerConfig config, IAfterEffectsBridge bridge, BridgeToken token, ServerAnnouncement announcement, ExtensionFreshness freshness, ClientSessions? clients = null)
{
    public static readonly TimeSpan PingWithin = TimeSpan.FromSeconds(5);

    public async Task<JsonObject> ReportAsync(CancellationToken cancellationToken)
    {
        var ping = bridge.IsConnected
            ? await bridge.SendAsync(BridgeCommands.Ping, timeout: PingWithin, cancellationToken: cancellationToken).ConfigureAwait(false)
            : BridgeReply.Failed(BridgeError.Unavailable());

        var report = new JsonObject
        {
            ["server"] = new JsonObject
            {
                ["name"] = ThisAssembly.Name,
                ["version"] = ThisAssembly.InformationalVersion,
            },
            ["afterEffects"] = Describe(ping),
            ["bridge"] = new JsonObject
            {
                ["port"] = announcement.Port,
                ["requestTimeoutSeconds"] = config.Bridge.RequestTimeoutSeconds,
                ["agent"] = clients?.Current.Agent ?? config.Bridge.Agent,
                ["client"] = clients?.Current.Name,
                ["worker"] = clients?.Current.Worker,
                ["connected"] = new JsonArray([.. bridge.Connected.Select(hello => (JsonNode)new JsonObject { ["worker"] = hello.Worker, ["instance"] = hello.Instance, ["app"] = hello.App, ["digest"] = hello.Digest })]),
                ["scopes"] = clients is null ? null : new JsonArray([.. clients.Current.Scopes.Select(scope => (JsonNode)scope)]),
                ["token"] = token.Source,
                ["announcements"] = announcement.Folder,
            },
            ["extension"] = freshness.Describe(bridge.Hello),
            ["transport"] = new JsonObject { ["kind"] = config.Transport is Transport.Http ? "http" : "stdio" },
            ["project"] = config.ProjectDirectory,
            ["dataDirectory"] = config.DataDirectory,
            ["idleTimeoutMinutes"] = config.IdleTimeoutMinutes,
        };

        return report;
    }

    /// <summary>What a load balancer asks: that the server runs, and whether After Effects answers it right now.</summary>
    public async Task<JsonObject> LivenessAsync(CancellationToken cancellationToken)
    {
        var ping = bridge.IsConnected
            ? await bridge.SendAsync(BridgeCommands.Ping, timeout: PingWithin, cancellationToken: cancellationToken).ConfigureAwait(false)
            : BridgeReply.Failed(BridgeError.Unavailable());

        return new JsonObject { ["status"] = "ok", ["afterEffects"] = ping.IsOk ? "up" : "down" };
    }

    private JsonObject Describe(BridgeReply ping)
    {
        if (ping.IsOk)
        {
            var answer = ping.Result as JsonObject ?? [];
            answer["reachable"] = true;

            if (answer["scripting_allowed"]?.GetValue<bool>() == false)
            {
                answer["note"] = Messages.ScriptingNotAllowedNote;
            }

            return answer;
        }

        var report = new JsonObject
        {
            ["reachable"] = false,
            ["error"] = ping.Error!.Message,
            ["hint"] = Messages.HintFor(ping.Error),
        };

        if (bridge.Hello is { } hello)
        {
            report["connected"] = hello.Describe();
        }

        return report;
    }
}
