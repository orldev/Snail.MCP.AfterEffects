namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>The server started as a separate process in HTTP mode by Snail.Sdk's <see cref="HttpServerProgram"/>, on a free loopback port,
/// with no After Effects behind it and a data directory of its own, which a test mints client keys into while it runs.</summary>
internal sealed class HttpServerProcess(HttpServerProgram program, string dataDirectory) : IAsyncDisposable
{
    /// <summary>The variable this server reads its address from, which is not Snail.Sdk's default <c>SNAIL_MCP_AFTEREFFECTS_URL</c>.</summary>
    public const string AddressVariable = "SNAIL_MCP_AFTEREFFECTS_HTTP__URL";

    public Uri Address => program.Address;

    public string DataDirectory { get; } = dataDirectory;

    /// <summary>The server started and answering <c>/healthz</c>; a server that exits at start fails the start with its exit code and its log.</summary>
    public static async Task<HttpServerProcess> StartAsync(string? token)
    {
        var dataDirectory = TestFolder.Create();
        var environment = new Dictionary<string, string?>
        {
            ["SNAIL_MCP_AFTEREFFECTS_DATA_DIRECTORY"] = dataDirectory,
            ["SNAIL_MCP_AFTEREFFECTS_BRIDGE__PORT"] = Ports.Free().ToString(System.Globalization.CultureInfo.InvariantCulture),
            ["SNAIL_MCP_AFTEREFFECTS_HTTP__TOKEN"] = token,
        };

        try
        {
            var program = await HttpServerProgram.StartAsync("Snail.MCP.AfterEffects", "SNAIL_AFTEREFFECTS_SERVER", environment, AddressVariable);

            return new HttpServerProcess(program, dataDirectory);
        }
        catch
        {
            Directory.Delete(dataDirectory, recursive: true);

            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        await program.DisposeAsync();

        if (Directory.Exists(DataDirectory))
        {
            Directory.Delete(DataDirectory, recursive: true);
        }
    }
}
