using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace Snail.MCP.AfterEffects.Tools;

/// <summary>Tells the client its tool list changed, after a skill was loaded or unloaded during a call.</summary>
/// <remarks>The SDK raises the notification when the collection changes, but a client of the sessionless protocol revision holds no stream
/// to hear it on; the answer to the call that changed the list says so too, and this is the notice for those that listen.</remarks>
internal static class ToolListNotice
{
    public static Task SendAsync(McpServer? server, CancellationToken cancellationToken) =>
        server is null ? Task.CompletedTask : server.SendNotificationAsync(NotificationMethods.ToolListChangedNotification, cancellationToken: cancellationToken);
}
