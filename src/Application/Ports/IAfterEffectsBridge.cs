
namespace Snail.MCP.AfterEffects.Application.Ports;

/// <summary>The link into After Effects: one command in, one reply out, over whichever bridge has connected.</summary>
internal interface IAfterEffectsBridge
{
    /// <summary>Whether a bridge is connected and welcomed right now.</summary>
    bool IsConnected { get; }

    /// <summary>What the connected bridge said about itself; null while none is connected.</summary>
    BridgeHello? Hello { get; }

    /// <summary>Every bridge connected right now, by the worker name each gave; the desk among them when a person's After Effects is open.</summary>
    IReadOnlyList<BridgeHello> Connected { get; }

    Task<BridgeReply> SendAsync(BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);

    /// <summary>Whether the bridge of this worker is connected right now.</summary>
    bool IsConnectedTo(string worker);

    /// <summary>Sends to one worker by name, whatever the calling client is bound to.</summary>
    Task<BridgeReply> SendToAsync(string worker, BridgeCommand command, JsonObject? parameters = null, TimeSpan? timeout = null, CancellationToken cancellationToken = default);
}
