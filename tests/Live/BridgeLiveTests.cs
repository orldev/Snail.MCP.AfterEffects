using Snail.MCP.AfterEffects.Adapters.AfterEffects;
using Snail.MCP.AfterEffects.Application.Installation;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Tests.Contracts;

namespace Snail.MCP.AfterEffects.Tests.Live;

/// <summary>The repository's bridge, injected into the After Effects that is running, dials this test's listener and answers.</summary>
/// <remarks>The injected bridge reads a data root of its own, so it never touches the files of the bridge a person may have installed; the
/// global it is pointed with is removed afterwards and the installed bridge restarted, because a global left behind in After Effects
/// outlives the test and would send the real bridge to a folder that no longer exists.</remarks>
public class BridgeLiveTests
{
    [AfterEffectsFact]
    public async Task TheRealBridge_DialsTheServer_AndAnswers_WithTheBundledDigest()
    {
        await using var stack = await ListeningServer.StartAsync(agent: "live");
        var scripts = Path.Combine(stack.DataDirectory, "scripts");
        Directory.CreateDirectory(scripts);

        try
        {
            await InjectAsync(scripts, "inject.jsx", $$"""
                $.global.SNAIL_BRIDGE_DATA_ROOT = "{{stack.DataDirectory}}";
                $.evalFile(new File("{{Path.Combine(ExtensionContractTests.ExtensionSource(), ServerPaths.LoaderFileName)}}"));
                """);
            await stack.UntilConnectedAsync(TimeSpan.FromSeconds(20));

            var ping = await stack.Links.SendAsync(BridgeCommands.Ping);
            var script = await stack.Links.SendAsync(BridgeCommands.Script, new JsonObject { ["code"] = "1 + 1" });
            var unknown = await stack.Links.SendAsync(new BridgeCommand("no_such_command", "none"));

            Assert.True(ping.IsOk, ping.Error?.Message);
            Assert.Equal(ExtensionFingerprint.Of(ExtensionContractTests.ModulesSource()), stack.Links.Hello!.Digest);
            Assert.Equal(BridgeCommands.All.Count, stack.Links.Hello.Commands);
            Assert.Equal("2", script.Result!["value"]!.ToString());
            Assert.Equal("UnknownCommand", unknown.Error!.Type);
            Assert.True(ping.Result!["scripting_allowed"]!.GetValue<bool>());
        }
        finally
        {
            await InjectAsync(scripts, "stop.jsx", $$"""
                if (typeof SnailBridge === "object") { SnailBridge.stop(); }
                delete $.global.SNAIL_BRIDGE_DATA_ROOT;
                var installed = new File({{JsonValue.Create(InstalledLoader()).ToJsonString()}});
                if (installed.exists) { $.evalFile(installed); }
                """);
        }
    }

    /// <summary>The bridge a person may have installed for real: the test's bridge replaced it in the shared global, so it is started again afterwards.</summary>
    private static string InstalledLoader()
    {
        var homes = new MacAfterEffectsHost(NullLogger<MacAfterEffectsHost>.Instance).ScriptsHomes();

        return homes.Count == 0 ? string.Empty : Path.Combine(homes[0].Path, BridgeInstallation.StartupFolderName, ServerPaths.LoaderFileName);
    }

    private static async Task InjectAsync(string folder, string name, string code)
    {
        var path = Path.Combine(folder, name);
        await File.WriteAllTextAsync(path, code);
        await AfterEffectsInstallation.RunScriptAsync(path, CancellationToken.None);
    }
}
