using System.Net;

namespace Snail.MCP.AfterEffects;

/// <summary>What the server says about the bridge, its announcements, the render farm and its workers.</summary>
/// <remarks>
/// Generated rather than written through the logging extensions: a debug line costs nothing while Debug is off, where
/// the extensions boxed and formatted their arguments first. The messages are the ones the server has always written.
/// </remarks>
internal static partial class Log
{
    [LoggerMessage(EventId = 1, Level = LogLevel.Debug, Message = "The bridge token comes from {Source}")]
    internal static partial void BridgeTokenSource(ILogger logger, string source);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Listening for the bridge on {Address}:{Port}")]
    internal static partial void ListeningForBridge(ILogger logger, IPAddress address, int port);

    [LoggerMessage(EventId = 3, Level = LogLevel.Error, Message = "The bridge listener stopped: {Reason}")]
    internal static partial void BridgeListenerStopped(ILogger logger, string reason);

    [LoggerMessage(EventId = 4, Level = LogLevel.Warning, Message = "Refused a bridge that presented a wrong token (After Effects {App})")]
    internal static partial void BridgeRefused(ILogger logger, string app);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Welcomed the bridge {Bridge} ({Digest}) of After Effects {App} with {Commands} commands")]
    internal static partial void BridgeWelcomed(ILogger logger, string bridge, string digest, string app, int commands);

    [LoggerMessage(EventId = 6, Level = LogLevel.Warning, Message = "The bridge sent a line that is not JSON: {Line}")]
    internal static partial void BridgeLineNotJson(ILogger logger, string line);

    [LoggerMessage(EventId = 7, Level = LogLevel.Debug, Message = "Dropping a reply nobody waits for: {Id}")]
    internal static partial void ReplyDropped(ILogger logger, string? id);

    [LoggerMessage(EventId = 8, Level = LogLevel.Warning, Message = "AfterFX could not be asked to run {Script}: {Error}")]
    internal static partial void AfterFxNotAsked(ILogger logger, string script, string error);

    [LoggerMessage(EventId = 9, Level = LogLevel.Warning, Message = "osascript refused to run {Script}: {Error}")]
    internal static partial void OsascriptRefused(ILogger logger, string script, string error);

    [LoggerMessage(EventId = 10, Level = LogLevel.Information, Message = "Starting {Executable} {Arguments}")]
    internal static partial void AerenderStarting(ILogger logger, string executable, string arguments);

    [LoggerMessage(EventId = 11, Level = LogLevel.Warning, Message = "Render job {Id} failed; retrying ({Left} left)")]
    internal static partial void RenderJobRetrying(ILogger logger, string id, int left);

    [LoggerMessage(EventId = 12, Level = LogLevel.Information, Message = "Render job {Id} is {State}")]
    internal static partial void RenderJobEnded(ILogger logger, string id, string state);

    [LoggerMessage(EventId = 13, Level = LogLevel.Warning, Message = "No MCP tool has been called for {IdleFor}; the process looks orphaned, shutting down")]
    internal static partial void ShuttingDownOrphaned(ILogger logger, TimeSpan idleFor);

    [LoggerMessage(EventId = 14, Level = LogLevel.Information, Message = "Wrote a new bridge token to {File}")]
    internal static partial void BridgeTokenWritten(ILogger logger, string file);

    [LoggerMessage(EventId = 15, Level = LogLevel.Information, Message = "Announced port {Port} in {File}")]
    internal static partial void PortAnnounced(ILogger logger, int port, string file);

    [LoggerMessage(EventId = 16, Level = LogLevel.Warning, Message = "The announcement {File} could not be removed: {Reason}")]
    internal static partial void AnnouncementNotRemoved(ILogger logger, string? file, string reason);

    [LoggerMessage(EventId = 17, Level = LogLevel.Information, Message = "Swept the announcement of the dead server {Pid}")]
    internal static partial void AnnouncementSwept(ILogger logger, int pid);

    [LoggerMessage(EventId = 18, Level = LogLevel.Information, Message = "Launched worker {Worker} as pid {Pid}")]
    internal static partial void WorkerLaunched(ILogger logger, string worker, int pid);

    [LoggerMessage(EventId = 19, Level = LogLevel.Warning, Message = "Worker {Worker} did not take the quit ({Reason}); killing it")]
    internal static partial void WorkerKilled(ILogger logger, string worker, string? reason);

    [LoggerMessage(EventId = 20, Level = LogLevel.Information, Message = "Worker {Worker} sat idle; asking it to quit")]
    internal static partial void WorkerIdle(ILogger logger, string worker);

    [LoggerMessage(EventId = 21, Level = LogLevel.Warning, Message = "The worker pool could not reconcile: {Reason}")]
    internal static partial void WorkersNotReconciled(ILogger logger, string reason);

    [LoggerMessage(EventId = 22, Level = LogLevel.Information, Message = "Adopted the worker {Worker}, whose bridge connected on its own")]
    internal static partial void WorkerAdopted(ILogger logger, string worker);

    [LoggerMessage(EventId = 23, Level = LogLevel.Information, Message = "Worker {Worker} is gone: {Why}")]
    internal static partial void WorkerGone(ILogger logger, string worker, string why);

    [LoggerMessage(EventId = 24, Level = LogLevel.Information, Message = "Unloaded idle skills: {Skills}")]
    internal static partial void SkillsUnloaded(ILogger logger, string skills);
}
