using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>A bridge answered in-process from a script, for the code above the link: it remembers every command and answers as told.</summary>
internal sealed class StubBridge(Func<BridgeCommand, JsonObject, BridgeReply> answer) : IAfterEffectsBridge
{
    public List<(BridgeCommand Command, JsonObject Parameters)> Sent { get; } = [];

    /// <summary>Whether the code above the link should believe After Effects is there; a test of a refusal sets it false.</summary>
    public bool IsConnected { get; set; } = true;

    public BridgeHello? Hello => null;
    public IReadOnlyList<BridgeHello> Connected => [];
    public bool IsConnectedTo(string worker) => false;
    public Task<BridgeReply> SendToAsync(string worker, BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default) =>
        SendAsync(command, parameters, timeout, cancellationToken);

    public Task<BridgeReply> SendAsync(BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        var sent = parameters ?? [];
        Sent.Add((command, sent));

        return Task.FromResult(answer(command, sent));
    }
}
