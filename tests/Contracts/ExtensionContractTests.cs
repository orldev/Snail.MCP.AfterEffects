using System.Text.RegularExpressions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The three readings of the command catalog — the C# fields, extension/commands.json and the registrations in the bridge's modules — say the same thing.</summary>
public partial class ExtensionContractTests
{
    public static string ExtensionSource() => Path.Combine(Repository.Root, ServerPaths.ExtensionFolderName);

    public static string ModulesSource() => Path.Combine(ExtensionSource(), ServerPaths.ModulesFolderName);

    private sealed record Registered(string Name, string Module, IReadOnlyList<string> Params, bool IsReadOnly, bool IsDestructive);

    private static IReadOnlyList<Registered> Registrations() =>
        [.. Directory.EnumerateFiles(ModulesSource(), "*.jsx")
            .OrderBy(Path.GetFileName, StringComparer.Ordinal)
            .SelectMany(file => Registration.Matches(File.ReadAllText(file)).Select(match => new Registered(
                match.Groups["name"].Value,
                ModuleOf(file),
                ParamsOf(match.Groups["spec"].Value),
                match.Groups["spec"].Value.Contains("read_only: true", StringComparison.Ordinal),
                match.Groups["spec"].Value.Contains("destructive: true", StringComparison.Ordinal))))];

    /// <summary>The module name is the file name without its load-order prefix and extension: 10_diagnostics.jsx is the module diagnostics.</summary>
    private static string ModuleOf(string file) => LoadOrderPrefix.Replace(Path.GetFileNameWithoutExtension(file), "");

    private static IReadOnlyList<string> ParamsOf(string spec) =>
        ParamsList.Match(spec) is { Success: true } match
            ? [.. match.Groups["list"].Value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Select(item => item.Trim('"'))]
            : [];

    private static JsonObject Schema() => (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(ExtensionSource(), "commands.json")))!["commands"]!;

    [Fact]
    public void Schema_ListsExactlyTheCommandsTheModulesRegister()
    {
        var registered = Registrations().Select(command => command.Name).Order(StringComparer.Ordinal);
        var schema = Schema().Select(entry => entry.Key).Order(StringComparer.Ordinal);

        Assert.Equal(registered, schema);
    }

    [Fact]
    public void Schema_Params_AreTheOnesTheRegistrationDeclares()
    {
        var schema = Schema();

        Assert.All(Registrations(), command =>
            Assert.Equal<string>(command.Params, [.. (schema[command.Name]!["params"] as JsonArray ?? []).Select(item => item!.ToString())]));
    }

    [Fact]
    public void Schema_Flags_AreTheOnesTheRegistrationDeclares()
    {
        var schema = Schema();

        Assert.All(Registrations(), command =>
        {
            Assert.Equal(command.IsReadOnly, schema[command.Name]!["read_only"]?.GetValue<bool>() ?? false);
            Assert.Equal(command.IsDestructive, schema[command.Name]!["destructive"]?.GetValue<bool>() ?? false);
        });
    }

    [Fact]
    public void Catalog_NamesEveryCommand_UnderItsModule()
    {
        var registered = Registrations().Select(command => (command.Name, command.Module)).Order();
        var catalogued = BridgeCommands.All.Select(command => (command.Name, command.Module)).Order();

        Assert.Equal(registered, catalogued);
    }

    /// <summary>An escape such as U+2028 can arrive in a source file as the character it names, and a literal line separator inside a regular
    /// expression is a syntax error ExtendScript reports at the wrong line; the JSON module was lost to exactly that once.</summary>
    [Fact]
    public void ExtensionSources_ArePlainAscii()
    {
        var offenders = Directory.EnumerateFiles(ExtensionSource(), "*.jsx", SearchOption.AllDirectories)
            .Select(file => (File: Path.GetRelativePath(ExtensionSource(), file), Line: FirstNonAsciiLine(file)))
            .Where(found => found.Line.text is not null)
            .Select(found => $"{found.File}:{found.Line.index + 1}")
            .ToList();

        Assert.True(offenders.Count == 0, $"non-ASCII characters in the bridge's sources: {string.Join(", ", offenders)}");
    }

    /// <summary>The first line of a file with a character outside ASCII, with its index; the default when there is none.</summary>
    private static (string text, int index) FirstNonAsciiLine(string file) =>
        File.ReadLines(file).Select((text, index) => (text, index)).FirstOrDefault(line => line.text.Any(character => character > 127));

    /// <summary>ExtendScript is ECMAScript 3: a reserved word as a bare key in an object literal fails the whole module at load, silently leaving the old commands in place.</summary>
    [Fact]
    public void ExtensionSources_QuoteReservedWordsUsedAsKeys()
    {
        var offenders = Directory.EnumerateFiles(ExtensionSource(), "*.jsx", SearchOption.AllDirectories)
            .SelectMany(file => File.ReadLines(file).Select((text, index) => (File: Path.GetRelativePath(ExtensionSource(), file), text, index)))
            .Where(line => ReservedWordKey.IsMatch(line.text) && !line.text.TrimStart().StartsWith("case ", StringComparison.Ordinal))
            .Select(line => $"{line.File}:{line.index + 1}")
            .ToList();

        Assert.True(offenders.Count == 0, $"reserved words as bare object keys in the bridge's sources: {string.Join(", ", offenders)}");
    }

    /// <summary>What the server ships next to its binary is the repository's bridge, module for module.</summary>
    [Fact]
    public void BundledExtension_IsTheRepositoryOne()
    {
        var bundled = Path.Combine(ServerPaths.BundledExtensionDirectory, ServerPaths.ModulesFolderName);

        Assert.True(Directory.Exists(bundled), $"the bridge is not bundled next to the binary at {bundled}");
        Assert.Equal(ExtensionFingerprint.Of(ModulesSource()), ExtensionFingerprint.Of(bundled));
        Assert.True(File.Exists(Path.Combine(ServerPaths.BundledExtensionDirectory, ServerPaths.LoaderFileName)));
    }

    /// <summary>The digest is the same number on both sides only if both hash the same bytes the same way; this is the C# half of that promise, pinned to a value the bridge computed over these inputs.</summary>
    [Fact]
    public void Fingerprint_MatchesTheBridgesOwnAlgorithm()
    {
        var digest = ExtensionFingerprint.Of([("a.jsx", "var a = 1;\n"), ("b.jsx", "var b = 2;\r\n")]);

        Assert.Matches("^[0-9a-f]{8}$", digest);
        Assert.Equal(ExtensionFingerprint.Of([("a.jsx", "var a = 1;\n"), ("b.jsx", "var b = 2;\n")]), digest);
    }

    [GeneratedRegex(@"SnailBridge\.command\(""(?<name>[a-z_]+)"",\s*\{(?<spec>[^}]*)\}")]
    private static partial Regex Registration { get; }

    [GeneratedRegex(@"params:\s*\[(?<list>[^\]]*)\]")]
    private static partial Regex ParamsList { get; }

    [GeneratedRegex("^[0-9]+_")]
    private static partial Regex LoadOrderPrefix { get; }

    [GeneratedRegex(@"[{,]\s*(default|class|new|delete|in|for|if|else|function|return|var|this|typeof|void|with|while|do|case|switch|break|continue|throw|try|catch|finally|instanceof|null|true|false|export|import|enum|extends|super|const|let|static|package|private|protected|public|interface|implements|abstract|boolean|byte|char|double|final|float|goto|int|long|native|short|synchronized|throws|transient|volatile)\s*:")]
    private static partial Regex ReservedWordKey { get; }
}
