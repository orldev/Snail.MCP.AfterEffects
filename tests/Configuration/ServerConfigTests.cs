using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Snail.MCP.AfterEffects.Configuration;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Tests.Configuration;

public class ServerConfigTests
{
    /// <summary>The bridge dials this server, so the port is free by default: whatever the listener gets is what the announcement names.</summary>
    [Fact]
    public void Defaults_LeaveThePortToTheListener()
    {
        var config = new ServerConfig();

        Assert.Equal(0, config.Bridge.Port);
        Assert.Equal(30, config.Bridge.RequestTimeoutSeconds);
    }

    [Fact]
    public void Defaults_UseHomeDataDirectory()
    {
        var config = new ServerConfig();

        Assert.Equal(Snail.Sdk.Mcp.Hosting.ServerFolders.DataDirectory(ServerPaths.ShortName), config.DataDirectory);
    }

    /// <summary>A wrong value is refused with its name, never repaired in silence: a typo in the environment must not listen somewhere else without a word.</summary>
    [Fact]
    public void Validation_InvalidValues_NamesEveryFault()
    {
        var config = new ServerConfig { Bridge = { Port = 70000, RequestTimeoutSeconds = -1 }, SkillIdleMinutes = -1 };

        var result = new ServerConfigValidation().Validate(Options.DefaultName, config);

        Assert.True(result.Failed);
        Assert.Contains("Bridge.Port", result.FailureMessage, StringComparison.Ordinal);
        Assert.Contains("RequestTimeoutSeconds", result.FailureMessage, StringComparison.Ordinal);
        Assert.Contains("SkillIdleMinutes", result.FailureMessage, StringComparison.Ordinal);
    }

    /// <summary>The whole path as the server takes it: sources, binding, normalisation and the generated validator behind one injected config.</summary>
    [Fact]
    public void Configuration_PortOutOfRange_RefusesToHandOutTheConfig()
    {
        using var provider = WithVariables(new Dictionary<string, string?> { ["SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT"] = "70000" });

        var failure = Assert.Throws<OptionsValidationException>(provider.GetRequiredService<ServerConfig>);

        Assert.Contains("Bridge.Port", failure.Message, StringComparison.Ordinal);
    }

    /// <summary>The client starts the server in the folder it works in; that folder is the project unless the environment names another.</summary>
    [Fact]
    public void Normalize_ProjectDirectory_IsTheWorkingDirectory_UnlessNamed_AndLosesItsTilde()
    {
        var unnamed = new ServerConfig();
        var named = new ServerConfig { ProjectDirectory = "~/Projects/lobby" };

        ServerConfigExtensions.Normalize(unnamed);
        ServerConfigExtensions.Normalize(named);

        Assert.Equal(Path.GetFullPath(Environment.CurrentDirectory), unnamed.ProjectDirectory);
        Assert.Equal(Path.Combine(ServerPaths.Home, "Projects", "lobby"), named.ProjectDirectory);
    }

    /// <summary>A server reached over HTTP sees none of the client's folders, so it has no project however the working directory looks.</summary>
    [Fact]
    public void Normalize_OverHttp_TheServerHasNoProject()
    {
        var config = new ServerConfig { Transport = Transport.Http };

        ServerConfigExtensions.Normalize(config);

        Assert.Null(config.ProjectDirectory);
    }

    /// <summary>The host is chosen before one exists, so the transport is read from the same sources the host binds later.</summary>
    [Fact]
    public void Peek_ReadsTheTransport_BeforeAHostIsBuilt()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?> { ["Snail:Transport"] = "http" }).Build();

        var peeked = ServerConfigExtensions.Peek(configuration);

        Assert.Equal(Transport.Http, peeked.Transport);
    }

    [Fact]
    public void Configuration_DataDirectoryFromTheEnvironment_ReachesTheLinkTokenFile()
    {
        using var provider = WithVariables(new Dictionary<string, string?> { ["SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY"] = "~/aftereffects-from-the-environment" });

        var link = provider.GetRequiredService<BridgeOptions>();

        Assert.Equal(Path.Combine(ServerPaths.Home, "aftereffects-from-the-environment", "state", "token"), link.TokenFile);
        Assert.Same(provider.GetRequiredService<ServerConfig>().Bridge, link);
    }

    /// <summary>A host configured from these variables alone, without touching the environment of the test process, which the tests running
    /// beside this one share.</summary>
    private static ServiceProvider WithVariables(Dictionary<string, string?> variables) => WithSettings(null, variables);

    /// <summary>A host configured from a settings file named outright and these variables; no file at all when none is named.</summary>
    private static ServiceProvider WithSettings(string? file, Dictionary<string, string?> variables)
    {
        var builder = Host.CreateEmptyApplicationBuilder(null);
        builder.AddServerConfiguration(ServerConfigExtensions.Settings(file, new Hashtable(variables)));

        return builder.Services.BuildServiceProvider();
    }

    /// <summary>The token belongs in the settings file; a variable of the server still wins over the file, setting by setting.</summary>
    [Fact]
    public void Configuration_TheSettingsFile_ReachesTheOptions_AndTheServersVariableWins()
    {
        var file = TemporarySettings("""{ "Bridge": { "Host": "10.0.0.5", "Port": 9911 }, "Http": { "Token": "from-the-file" } }""");

        try
        {
            using var provider = WithSettings(file, new Dictionary<string, string?> { ["SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT"] = "9922" });

            var config = provider.GetRequiredService<ServerConfig>();

            Assert.Equal("10.0.0.5", config.Bridge.Host);
            Assert.Equal(9922, config.Bridge.Port);
            Assert.Equal("from-the-file", config.Http.Token);
        }
        finally
        {
            File.Delete(file);
        }
    }

    /// <summary>The variable that names the settings file is the first place of the chain, and the transport is peeked from that file too.</summary>
    [Fact]
    public void Settings_TheFileTheVariableNames_IsWhatThePeekReads()
    {
        var file = TemporarySettings("""{ "Snail": { "Transport": "Http", "Agent": "compositing" } }""");

        try
        {
            var peeked = RunWithEnvironment(
                new Dictionary<string, string?> { [ServerSettings.FileVariableOf(ServerPaths.ShortName)] = file },
                () => ServerConfigExtensions.Peek(ServerConfigExtensions.Settings()));

            Assert.Equal(Transport.Http, peeked.Transport);
            Assert.Equal("compositing", peeked.Agent);
        }
        finally
        {
            File.Delete(file);
        }
    }

    private static string TemporarySettings(string json)
    {
        var file = Path.Combine(TestFolder.Create(), "s3l-mcp-aftereffects-test.json");
        File.WriteAllText(file, json);

        return file;
    }

    [Fact]
    public void Normalize_TildeInDataDirectory_IsExpanded()
    {
        var config = new ServerConfig { DataDirectory = "~/custom-aftereffects-data" };

        ServerConfigExtensions.Normalize(config);

        Assert.Equal(Path.Combine(ServerPaths.Home, "custom-aftereffects-data"), config.DataDirectory);
    }

    [Theory]
    [InlineData("DATA_DIRECTORY", "DataDirectory")]
    [InlineData("IDLE_TIMEOUT_MINUTES", "IdleTimeoutMinutes")]
    [InlineData("BRIDGE__PORT", "Bridge:Port")]
    [InlineData("BRIDGE__REQUEST_TIMEOUT_SECONDS", "Bridge:RequestTimeoutSeconds")]
    [InlineData("BRIDGE__TOKEN", "Bridge:Token")]
    public void EnvironmentName_BecomesConfigurationKey(string variable, string expected) =>
        Assert.Equal(expected, EnvironmentSettings.ToConfigurationKey(variable), ignoreCase: true);

    [Fact]
    public void EnvironmentValues_BindIntoConfig_AndBlankOnesAreIgnored()
    {
        var variables = new Hashtable
        {
            ["SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT"] = " 9900 ",
            ["SNAIL_MCP_AFTEREFFECTS_BRIDGE__REQUEST_TIMEOUT_SECONDS"] = "   ",
            ["SNAIL_MCP_AFTEREFFECTS_WORKERS__HEADLESS"] = "true",
            ["PATH"] = "/usr/bin",
        };

        var configuration = ServerConfigExtensions.Settings(null, variables);
        var config = new ServerConfig();
        configuration.Bind(config);
        ServerConfigExtensions.Normalize(config);

        Assert.Equal(9900, config.Bridge.Port);
        Assert.Equal(30, config.Bridge.RequestTimeoutSeconds);
        Assert.True(config.Workers.IsHeadless);
        Assert.Null(configuration["PATH"]);
    }

    /// <summary>The server reads its own variables and its own file, and nothing else: bound to the host's configuration as well, an unprefixed
    /// variable of the machine — TRANSPORT — turned a stdio server's transport into http while it still spoke over stdio.</summary>
    [Fact]
    public void Configuration_AnUnprefixedVariable_IsNotRead()
    {
        var read = RunWithEnvironment(
            new Dictionary<string, string?> { ["TRANSPORT"] = "Http" },
            () =>
            {
                var builder = Host.CreateEmptyApplicationBuilder(null);
                builder.AddServerConfiguration();

                return builder.Services.BuildServiceProvider().GetRequiredService<ServerConfig>();
            });

        Assert.Equal(Transport.Stdio, read.Transport);
    }

    private static T RunWithEnvironment<T>(Dictionary<string, string?> values, Func<T> action)
    {
        var previous = values.Keys.ToDictionary(key => key, Environment.GetEnvironmentVariable);

        try
        {
            foreach (var (key, value) in values)
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            return action();
        }
        finally
        {
            foreach (var (key, value) in previous)
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }

    [Fact]
    public void Normalize_TopLevelAgent_ReachesTheLink()
    {
        var config = new ServerConfig { Agent = "titles" };

        ServerConfigExtensions.Normalize(config);

        Assert.Equal("titles", config.Bridge.Agent);
    }
}
