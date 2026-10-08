namespace Snail.MCP.AfterEffects.Domain;

/// <summary>One number for a build of the bridge: the module files in name order, each as its name and its text, hashed the way the bridge hashes itself.</summary>
/// <remarks>FNV-1a over UTF-16 code units with line endings normalised, because that is what ExtendScript can compute over its own files
/// in a few hundred milliseconds at start-up; a cryptographic hash is not needed to tell two builds apart, only to tell them apart reliably.</remarks>
internal static class ExtensionFingerprint
{
    private const uint Offset = 2166136261;

    /// <summary>The digest of the modules in a bridge folder, the <c>(snail_bridge)</c> one next to the loader.</summary>
    public static string Of(string modulesDirectory) =>
        Of(Directory.EnumerateFiles(modulesDirectory, "*.jsx")
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .Select(file => (Path.GetFileName(file), File.ReadAllText(file))));

    public static string Of(IEnumerable<(string Name, string Text)> modules)
    {
        var hash = Offset;

        foreach (var (name, text) in modules)
        {
            hash = Fnv1a(hash, $"{name}\n");
            hash = Fnv1a(hash, text.Replace("\r\n", "\n", StringComparison.Ordinal));
        }

        return hash.ToString("x8");
    }

    private static uint Fnv1a(uint seed, string text)
    {
        var hash = seed;

        foreach (var unit in text)
        {
            hash ^= unit;
            hash += (hash << 1) + (hash << 4) + (hash << 7) + (hash << 8) + (hash << 24);
        }

        return hash;
    }
}
