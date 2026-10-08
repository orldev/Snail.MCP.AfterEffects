namespace Snail.MCP.AfterEffects.Configuration;

/// <summary>How a client reaches this server.</summary>
internal enum Transport
{
    /// <summary>The client starts the server and speaks JSON-RPC over its stdin and stdout; the server lives as long as the client.</summary>
    Stdio,

    /// <summary>The server runs on its own beside After Effects, and clients connect over Streamable HTTP with a bearer token.</summary>
    Http,
}
