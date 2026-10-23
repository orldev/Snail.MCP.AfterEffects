using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Application.Installation;
using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Tests.Contracts;

namespace Snail.MCP.AfterEffects.Tests.Application;

public sealed class BridgeInstallationTests : IDisposable
{
    private readonly string _root = TestFolder.Create();

    /// <summary>The loader lands in Startup, the modules beside it, and what lands is the repository's bridge, module for module.</summary>
    [Fact]
    public async Task Install_PutsTheLoaderInStartup_AndTheModulesBesideIt()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");
        var host = new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: false);

        var report = await Installation(host).InstallAsync(CancellationToken.None);

        Assert.True(report["installed"]!.GetValue<bool>());
        Assert.True(File.Exists(Path.Combine(home, BridgeInstallation.StartupFolderName, ServerPaths.LoaderFileName)));
        Assert.Equal(ExtensionFingerprint.Of(ExtensionContractTests.ModulesSource()), ExtensionFingerprint.Of(Path.Combine(home, ServerPaths.ModulesFolderName)));
        Assert.Equal(report["digest"]!.ToString(), ExtensionFingerprint.Of(Path.Combine(home, ServerPaths.ModulesFolderName)));
        Assert.False(report["afterEffects"]!["running"]!.GetValue<bool>());
        Assert.Empty(host.ScriptsRun);
    }

    /// <summary>A module of an earlier build that this build no longer ships would still be loaded by name order; the install clears the folder first.</summary>
    [Fact]
    public async Task Install_RemovesModulesOfAnEarlierBuild()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");
        var modules = Path.Combine(home, ServerPaths.ModulesFolderName);
        Directory.CreateDirectory(modules);
        File.WriteAllText(Path.Combine(modules, "99_gone.jsx"), "var gone = true;");

        await Installation(new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: false)).InstallAsync(CancellationToken.None);

        Assert.False(File.Exists(Path.Combine(modules, "99_gone.jsx")));
    }

    [Fact]
    public async Task Install_IntoARunningAfterEffects_StartsTheBridgeThere()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");
        var host = new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: true);

        var report = await Installation(host).InstallAsync(CancellationToken.None);

        Assert.True(report["afterEffects"]!["started"]!.GetValue<bool>());
        Assert.Contains(ServerPaths.LoaderFileName, Assert.Single(host.ScriptsRun), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Install_PrefersTheNewestVersion()
    {
        var older = Path.Combine(_root, "25.0", "Scripts");
        var newer = Path.Combine(_root, "26.5", "Scripts");
        var host = new FakeAfterEffectsHost([new ScriptsHome("26.5", newer), new ScriptsHome("25.0", older)], isRunning: false);

        var report = await Installation(host).InstallAsync(CancellationToken.None);

        Assert.Equal("26.5", report["version"]!.ToString());
        Assert.False(Directory.Exists(older));
    }

    [Fact]
    public async Task Install_WithoutAScriptsFolder_SaysSo()
    {
        var report = await Installation(new FakeAfterEffectsHost([], isRunning: true)).InstallAsync(CancellationToken.None);

        Assert.False(report["installed"]!.GetValue<bool>());
        Assert.Contains("Start After Effects once", report["hint"]!.ToString(), StringComparison.Ordinal);
    }

    /// <summary>When the bridge in After Effects is not the one this server ships, the report carries the last words of the loader: a module
    /// that threw while it loaded says so there, and without it the answer would only say that the digests differ.</summary>
    [Fact]
    public void TheReport_CarriesWhatTheLoaderSaid_WhenTheBridgeIsNotTheOneShipped()
    {
        var config = new ServerConfig { DataDirectory = TestFolder.Create() };
        var state = Path.Combine(config.DataDirectory, "state");
        Directory.CreateDirectory(state);
        File.WriteAllLines(Path.Combine(state, "loader.log"), ["old line", "module 72_text.jsx failed to load: unexpected token (line 157)"]);
        var freshness = new ExtensionFreshness(config);

        var stale = freshness.Describe(Hello("another-digest"));

        Assert.Contains("72_text.jsx", stale["loader"]!.ToJsonString(), StringComparison.Ordinal);

        var current = freshness.Describe(Hello(freshness.BundledDigest ?? "none"));

        Assert.True(current["status"]!.ToString() is not "current" || current["loader"] is null);
    }

    /// <summary>The report of an install carries the fields its readers look for: a person follows the steps, and an agent reads where the
    /// modules went and whether After Effects took them. Of the twenty mutants that survived in this file on a mutation run, eleven were one of
    /// these names replaced by another string, which no test noticed.</summary>
    [Fact]
    public async Task TheReport_CarriesTheFieldsItsReadersLookFor()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");

        var report = await Installation(new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: true)).InstallAsync(CancellationToken.None);

        Assert.Equal(["afterEffects", "digest", "installed", "loader", "modules", "steps", "version"], Fields(report));
        Assert.Equal(["note", "running", "started"], Fields(report["afterEffects"]!));
        Assert.NotEmpty(report["steps"]!.AsArray());

        var refused = await Installation(new FakeAfterEffectsHost([], isRunning: true)).InstallAsync(CancellationToken.None);

        Assert.Equal(["error", "hint", "installed"], Fields(refused));
    }

    private static IReadOnlyList<string> Fields(JsonNode report) =>
        [.. report.AsObject().Select(field => field.Key).Order(StringComparer.Ordinal)];

    /// <summary>An install leaves the scripts folder holding only what it owns: the staged copy is renamed into place and the folder it
    /// replaced is removed, so nothing of the swap is left for the loader to read or for the next install to trip over.</summary>
    [Fact]
    public async Task Install_LeavesNothingBesideTheFoldersItOwns()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");
        var modules = Path.Combine(home, ServerPaths.ModulesFolderName);
        Directory.CreateDirectory(modules);
        await File.WriteAllTextAsync(Path.Combine(modules, "99_gone.jsx"), "var gone = true;");

        await Installation(new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: false)).InstallAsync(CancellationToken.None);

        Assert.Equal([ServerPaths.ModulesFolderName, BridgeInstallation.StartupFolderName], Directory.GetDirectories(home).Select(Path.GetFileName).Order(StringComparer.Ordinal));
        Assert.Empty(Directory.GetFiles(home));
        Assert.Equal([ServerPaths.LoaderFileName], Directory.GetFiles(Path.Combine(home, BridgeInstallation.StartupFolderName)).Select(Path.GetFileName));
    }

    /// <summary>Two servers installing into one After Effects at the same moment end with the build they both ship and nothing half-written:
    /// a dozen servers share this machine, and each of them may install.</summary>
    [Fact]
    public async Task Install_RunTwiceAtOnce_EndsWithTheWholeBridge()
    {
        var home = Path.Combine(_root, "26.5", "Scripts");
        var homes = new[] { new ScriptsHome("26.5", home) };

        await Task.WhenAll(
            Installation(new FakeAfterEffectsHost(homes, isRunning: false)).InstallAsync(CancellationToken.None),
            Installation(new FakeAfterEffectsHost(homes, isRunning: false)).InstallAsync(CancellationToken.None));

        Assert.Equal(ExtensionFingerprint.Of(ExtensionContractTests.ModulesSource()), ExtensionFingerprint.Of(Path.Combine(home, ServerPaths.ModulesFolderName)));
        Assert.Equal([ServerPaths.ModulesFolderName, BridgeInstallation.StartupFolderName], Directory.GetDirectories(home).Select(Path.GetFileName).Order(StringComparer.Ordinal));
    }

    /// <summary>What the loader said about the modules an install has just replaced does not come back in the next diagnosis: the loader only
    /// appends, so without this a failure of an earlier build is read as a failure of today whenever no bridge is connected.</summary>
    [Fact]
    public async Task Install_ForgetsWhatTheLoaderSaid_AboutTheModulesItReplaced()
    {
        var log = ServerPaths.LoaderLog(Config().DataDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(log)!);
        await File.WriteAllLinesAsync(log, ["module 72_text.jsx failed to load: a reserved word (line 157)"]);
        var home = Path.Combine(_root, "26.5", "Scripts");

        await Installation(new FakeAfterEffectsHost([new ScriptsHome("26.5", home)], isRunning: false)).InstallAsync(CancellationToken.None);

        var freshness = new ExtensionFreshness(Config());

        Assert.False(File.Exists(log));
        Assert.Null(freshness.Describe(Hello("another-digest"))["loader"]);
    }

    private static BridgeHello Hello(string digest) =>
        new(string.Empty, "0.1.0", BridgeProtocol.Version, BridgeProtocol.RequiredAbilities, digest, 1, "26.5x89", "26.5x89", "mac", true, "/tmp");

    private BridgeInstallation Installation(IAfterEffectsHost host)
    {
        var config = Config();

        return new BridgeInstallation(config, host, new ExtensionFreshness(config));
    }

    private ServerConfig Config()
    {
        var config = new ServerConfig { DataDirectory = Path.Combine(_root, "data") };
        ServerConfigExtensions.Normalize(config);

        return config;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root)) Directory.Delete(_root, recursive: true);
    }
}
