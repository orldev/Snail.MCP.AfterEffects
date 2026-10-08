using Snail.Sdk.Mcp.Hosting;

namespace Snail.MCP.AfterEffects.Configuration;

/// <summary>Paths where the server keeps its data and ships the bridge; its settings file is found by Snail.Sdk's <see cref="ServerSettings"/>.</summary>
internal static class ServerPaths
{
    /// <summary>The server's short name, which names its folders: ~/.s3l-mcp-aftereffects and ~/.config/s3l-mcp-aftereffects.</summary>
    public const string ShortName = "aftereffects";

    public const string ExtensionFolderName = "extension";

    /// <summary>The folder of bridge modules beside the loader; the parentheses keep it out of After Effects' File → Scripts menu.</summary>
    public const string ModulesFolderName = "(snail_bridge)";

    public const string LoaderFileName = "snail_bridge.jsx";

    /// <summary>The folder inside the data directory where the server and the bridge keep what they leave between runs.</summary>
    public const string StateFolderName = "state";

    public static string Home => Environment.GetFolderPath(
        Environment.SpecialFolder.UserProfile,
        Environment.SpecialFolderOption.DoNotVerify);

    /// <summary><c>~/.s3l-mcp-aftereffects</c>, moved there from <c>~/.snail-mcp-aftereffects</c> when only that one exists.</summary>
    public static string DefaultDataDirectory => ServerFolders.DataDirectory(ShortName);

    /// <summary>The ExtendScript bridge shipped next to the server binary; installed into After Effects' scripts folder by the server.</summary>
    public static string BundledExtensionDirectory => Path.Combine(AppContext.BaseDirectory, ExtensionFolderName);

    /// <summary>The log the bridge's loader writes a module that failed to load to, inside the given data directory.</summary>
    public static string LoaderLog(string dataDirectory) => Path.Combine(dataDirectory, StateFolderName, "loader.log");
}
