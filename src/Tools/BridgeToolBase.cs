using ModelContextProtocol.Protocol;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools;

/// <summary>Shared base of bridge-backed tools: sends one command and turns the reply into a tool result.</summary>
internal abstract class BridgeToolBase(IAfterEffectsBridge bridge)
{
    protected async Task<CallToolResult> SendAsync(BridgeCommand command, JsonObject arguments, CancellationToken cancellationToken, TimeSpan? timeout = null) =>
        ToolResponse.From(await bridge.SendAsync(command, arguments, timeout, cancellationToken).ConfigureAwait(false));

    /// <summary>Runs <paramref name="send"/> with the requested timeout, or refuses a value outside the range the descriptions promise.</summary>
    protected static Task<CallToolResult> WithinAsync(int timeoutSeconds, Func<TimeSpan, Task<CallToolResult>> send) =>
        ToolLimits.IsTimeout(timeoutSeconds)
            ? send(ToolLimits.Timeout(timeoutSeconds))
            : Task.FromResult(ToolResponse.Failure(Messages.TimeoutOutOfRange, Messages.TimeoutRangeHint));

    /// <summary>The raw reply, for tools that turn a picture into image content.</summary>
    protected Task<BridgeReply> ExchangeAsync(BridgeCommand command, JsonObject arguments, TimeSpan? timeout, CancellationToken cancellationToken) =>
        bridge.SendAsync(command, arguments, timeout, cancellationToken);
}
