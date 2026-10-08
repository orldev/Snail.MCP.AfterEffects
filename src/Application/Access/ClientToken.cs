using System.Security.Cryptography;
using System.Text;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>The secret a client of the HTTP server proves itself with, and the root of the key file links are signed with.</summary>
/// <remarks>The link key is derived from the token rather than drawn at random, so a link outlives a restart of the container and dies with
/// the token. A server with no token accepts nobody and signs with a key of its own; the HTTP server refuses to start that way, the stdio
/// server never checks.</remarks>
internal sealed class ClientToken
{
    private static readonly byte[] LinkKeyPurpose = "snail file links"u8.ToArray();

    public ClientToken(ServerConfig config)
    {
        var secret = Read(config.Http) is { } token ? Encoding.UTF8.GetBytes(token) : null;

        LinkKey = secret is null ? RandomNumberGenerator.GetBytes(32) : HKDF.DeriveKey(HashAlgorithmName.SHA256, secret, 32, info: LinkKeyPurpose);
    }

    /// <summary>The key links to files are signed with.</summary>
    public byte[] LinkKey { get; }

    /// <summary>The configured token, else the content of the token file; null when neither holds one.</summary>
    internal static string? Read(HttpOptions http)
    {
        if (!string.IsNullOrWhiteSpace(http.Token))
        {
            return http.Token.Trim();
        }

        if (string.IsNullOrWhiteSpace(http.TokenFile) || !File.Exists(http.TokenFile))
        {
            return null;
        }

        var stored = File.ReadAllText(http.TokenFile).Trim();

        return stored.Length > 0 ? stored : null;
    }
}
