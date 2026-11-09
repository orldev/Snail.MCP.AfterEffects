using Snail.MCP.AfterEffects.Configuration;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>The secret the bridge presents when it connects: the configured one, or the one this server keeps in its state and writes on first use.</summary>
/// <remarks>Anything on the machine can connect to a loopback port, so a connection proves nothing by itself. The token lives in the user's
/// own data directory, where the bridge inside After Effects reads it and other users cannot, and it is compared in constant time. The file is
/// Snail.Sdk's <see cref="ServerSecret"/>: readable by its owner alone from its first byte, made by one server at a time when several start at
/// once, and never replaced, so a token the bridge has already read stays the one every server expects.</remarks>
internal sealed class BridgeToken(BridgeOptions options, ILogger<BridgeToken> logger)
{
    private readonly Lazy<string> _value = new(() => Resolve(options, logger));

    public string Value => _value.Value;

    /// <summary>configured when the server carries its own token, file when it keeps one in its state; asking writes the file when it is missing.</summary>
    public string Source
    {
        get
        {
            _ = Value;

            return string.IsNullOrWhiteSpace(options.Token) ? "file" : "configured";
        }
    }

    public bool Matches(string presented) => ServerSecret.Matches(Value, presented);

    private static string Resolve(BridgeOptions options, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            return options.Token.Trim();
        }

        var file = options.TokenFile ?? throw new InvalidOperationException("The bridge token file is not configured.");
        var isNew = !File.Exists(file);
        var token = ServerSecret.ReadOrCreateHex(file);

        if (isNew)
        {
            Log.BridgeTokenWritten(logger, file);
        }

        return token;
    }
}
