using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Diagnostics;

/// <summary>Whether the bridge running inside After Effects is the build this server ships: current, differs, older, or unknown while none is connected.</summary>
/// <remarks>Versions cannot catch a stale install, because the bridge's version stays put across fixes; the digest of its files can, and the
/// protocol number with the abilities says whether a difference costs anything. A bridge ahead of this server is reported as differing, not
/// as broken: it answers everything this server knows.</remarks>
internal sealed class ExtensionFreshness(ServerConfig config)
{
    /// <summary>Lines of the loader's log the report carries when something is wrong; the newest are the ones that explain it.</summary>
    public const int LoaderLines = 3;

    private readonly Lazy<string?> _bundled = new(() => Bundled());

    /// <summary>The digest of the bridge shipped next to this binary; null when the folder is missing, as in a test run from another folder.</summary>
    public string? BundledDigest => _bundled.Value;

    public JsonObject Describe(BridgeHello? hello)
    {
        var report = new JsonObject
        {
            ["bundled"] = BundledDigest,
            ["installed"] = hello?.Digest,
            ["status"] = Status(hello),
            ["protocol"] = new JsonObject
            {
                ["server"] = BridgeProtocol.Version,
                ["bridge"] = hello?.Protocol,
            },
        };

        if (hello is not null && BridgeProtocol.Missing(hello) is { Count: > 0 } missing)
        {
            report["protocol"]!["missing"] = new JsonArray([.. missing.Select(ability => (JsonNode)ability)]);
        }

        if (report["status"]!.ToString() is not "current" && Loader() is { Count: > 0 } said)
        {
            report["loader"] = new JsonArray([.. said.Select(line => (JsonNode)line)]);
        }

        return report;
    }

    private string Status(BridgeHello? hello)
    {
        if (hello is null || BundledDigest is null)
        {
            return "unknown";
        }

        if (hello.Protocol < BridgeProtocol.Version || BridgeProtocol.Missing(hello).Count > 0)
        {
            return "older";
        }

        return string.Equals(hello.Digest, BundledDigest, StringComparison.Ordinal) ? "current" : "differs";
    }

    /// <summary>The last words of the loader inside After Effects, which is where a module that failed to load says so.</summary>
    /// <remarks>A module that throws while it loads leaves the bridge of an earlier build answering, and the server sees nothing but a
    /// digest that differs or no bridge at all. The loader writes the reason to its log because a dialog at start-up would stop After
    /// Effects; the report carries it, so the answer says what to fix instead of only that something is wrong.</remarks>
    private IReadOnlyList<string> Loader()
    {
        var log = ServerPaths.LoaderLog(config.DataDirectory);

        try
        {
            return File.Exists(log) ? [.. File.ReadLines(log).TakeLast(LoaderLines)] : [];
        }
        catch (IOException)
        {
            return [];
        }
        catch (UnauthorizedAccessException)
        {
            return [];
        }
    }

    private static string? Bundled()
    {
        var modules = Path.Combine(ServerPaths.BundledExtensionDirectory, ServerPaths.ModulesFolderName);

        return Directory.Exists(modules) ? ExtensionFingerprint.Of(modules) : null;
    }
}
