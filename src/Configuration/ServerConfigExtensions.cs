using System.Collections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Configuration;

/// <summary>Builds <see cref="ServerConfig"/>: defaults → settings file → environment variables, validated before the server answers anything.</summary>
internal static class ServerConfigExtensions
{
    /// <summary>Adds the configuration sources and binds them through the options pipeline; what a consumer injects is the bound <see cref="ServerConfig"/> itself.</summary>
    /// <remarks>The ranges are attributes on the settings and <see cref="ServerConfigValidation"/> is generated from them, so a setting arrives with
    /// its rule or with none at all. ValidateOnStart runs that validator while the host starts: a typo in the environment stops the process with a
    /// message naming the setting instead of being repaired in silence.</remarks>
    public static IHostApplicationBuilder AddServerConfiguration(this IHostApplicationBuilder builder) =>
        builder.AddServerConfiguration(Settings());

    /// <summary>The same, from settings read already: those the transport was chosen from, or a test's instead of this machine's.</summary>
    internal static IHostApplicationBuilder AddServerConfiguration(this IHostApplicationBuilder builder, IConfiguration settings)
    {
        builder.Services
            .AddOptions<ServerConfig>()
            .Bind(settings.GetSection(ServerConfig.SectionName))
            .Bind(settings)
            .PostConfigure(Normalize)
            .ValidateOnStart();

        builder.Services.AddSingleton<IValidateOptions<ServerConfig>, ServerConfigValidation>();
        builder.Services.AddSingleton(provider => provider.GetRequiredService<IOptions<ServerConfig>>().Value);
        builder.Services.AddSingleton(provider => provider.GetRequiredService<ServerConfig>().Bridge);

        return builder;
    }

    /// <summary>What this server reads, and nothing else: its settings file, found by the chain every Snail server shares, and the
    /// SNAIL_MCP_AFTEREFFECTS_ variables, which win over the file.</summary>
    /// <remarks>The host's own configuration is left out on purpose. Bound as well, it brought in every unprefixed variable, an appsettings.json
    /// in whatever folder the client started the server in, and the command line: TRANSPORT=Http in an unrelated environment turned a stdio
    /// server's transport into http while the process still spoke over stdio, because the transport is chosen from these sources alone.</remarks>
    public static IConfiguration Settings() => ServerSettings.Read(ServerPaths.ShortName);

    /// <summary>Where the HTTP server listens when the settings name no address: the loopback, for a server started by hand; a container
    /// listens on every interface by itself.</summary>
    public static Uri DefaultUrl { get; } = new(HttpOptions.DefaultUrl);

    /// <summary>How the endpoint is read: keys under the server's section as well as at the root, and no reach narrower than the settings
    /// ask for — the HTTP server is meant to be reached from other machines of a studio, and every request to it presents a key
    /// (<c>Web.Access.BearerAuthentication</c>), so a LAN address or every interface is the operator's to name. A refusal names the variable
    /// by the server's name, in a test's process too.</summary>
    public static McpEndpointOptions EndpointOptions { get; } = new()
    {
        FallbackSection = ServerConfig.SectionName,
        Reach = McpReach.Unrestricted,
        Server = ServerPaths.ShortName,
    };

    /// <summary>The same from a settings file named outright and the variables a caller hands over; a named file that does not exist reads none.</summary>
    public static IConfiguration Settings(string? file, IDictionary variables) =>
        new ConfigurationBuilder().AddServerSettings(ServerPaths.ShortName, file, variables).Build();

    /// <summary>The settings as a host would bind them, read before one is built: stdio and HTTP need different hosts.</summary>
    /// <remarks>No host is there yet to refuse a value of the wrong type, so <see cref="ServerSettings.Bind(string, IConfiguration, Action{IConfiguration})"/>
    /// says it as a <see cref="StartRefusedException"/> naming the variable, where the binder crashed the process with a trace.</remarks>
    /// <exception cref="StartRefusedException">A setting holds a value its property's type cannot hold.</exception>
    public static ServerConfig Peek(IConfiguration? configuration = null)
    {
        configuration ??= Settings();

        var config = new ServerConfig();

        ServerSettings.Bind(ServerPaths.ShortName, configuration, settings =>
        {
            settings.GetSection(ServerConfig.SectionName).Bind(config);
            settings.Bind(config);
        });

        return config;
    }

    /// <summary>Expands <c>~</c>, and fills the link's agent and token file from the top-level settings.</summary>
    internal static void Normalize(ServerConfig config)
    {
        config.DataDirectory = ServerFolders.FullPath(config.DataDirectory, ServerPaths.DefaultDataDirectory);

        if (string.IsNullOrWhiteSpace(config.Bridge.Agent))
        {
            config.Bridge.Agent = config.Agent;
        }

        if (string.IsNullOrWhiteSpace(config.Bridge.TokenFile))
        {
            config.Bridge.TokenFile = config.TokenFile;
        }

        config.ProjectDirectory = config.Transport is McpTransport.Http ? null : ServerFolders.FullPath(config.ProjectDirectory, Environment.CurrentDirectory);
        config.Bridge.TokenFile = string.IsNullOrWhiteSpace(config.Bridge.TokenFile) ? null : ServerFolders.ExpandHome(config.Bridge.TokenFile);
    }
}
