using System.Security.Cryptography;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Access;

/// <summary>The secret the bridge presents when it connects: the configured one, or the one this server keeps in its state and writes on first use.</summary>
/// <remarks>Anything on the machine can connect to a loopback port, so a connection proves nothing by itself. The token lives in the user's
/// own data directory, where the bridge inside After Effects reads it and other users cannot, and it is compared in constant time.</remarks>
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

    public bool Matches(string presented)
    {
        var expected = System.Text.Encoding.UTF8.GetBytes(Value);
        var given = System.Text.Encoding.UTF8.GetBytes(presented);

        return expected.Length == given.Length && CryptographicOperations.FixedTimeEquals(expected, given);
    }

    private static string Resolve(BridgeOptions options, ILogger logger)
    {
        if (!string.IsNullOrWhiteSpace(options.Token))
        {
            return options.Token.Trim();
        }

        var file = options.TokenFile ?? throw new InvalidOperationException("The bridge token file is not configured.");

        if (File.Exists(file) && File.ReadAllText(file).Trim() is { Length: > 0 } stored)
        {
            return stored;
        }

        var minted = Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(32));
        Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        File.WriteAllText(file, minted);
        Protect(file);
        Log.BridgeTokenWritten(logger, file);

        return minted;
    }

    private static void Protect(string file)
    {
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(file, UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }
}
