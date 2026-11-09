namespace Snail.MCP.AfterEffects.Application.Ports;

/// <summary>Who the request being served comes from: the name its commands are signed with and the kinds of work its key opens.</summary>
/// <remarks>Over HTTP every request presents a key, so the answer is read from the request itself rather than remembered from the
/// one that opened a session; over stdio there is no request and no implementation, and the process is its own client.</remarks>
internal interface IClientIdentity
{
    /// <summary>The caller of the request in hand, or null outside a request.</summary>
    ClientIdentity? Who();
}

/// <summary>A caller as its request names it: null scopes when the request was admitted without a key's.</summary>
internal sealed record ClientIdentity(string? Name, IReadOnlyList<string>? Scopes);
