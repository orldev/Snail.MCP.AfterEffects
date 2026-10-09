using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Application.Installation;

/// <summary>Puts the bundled bridge into After Effects' user scripts folder and, when After Effects is running, starts it there without a restart.</summary>
/// <remarks>The user folder rather than the application's: it needs no administrator rights, survives an application update, and After Effects
/// runs its Startup scripts all the same. The modules go beside Startup rather than into it, because After Effects runs every file of Startup
/// and would run each module twice; the parentheses in the folder's name keep it out of the File → Scripts menu.</remarks>
internal sealed class BridgeInstallation(ServerConfig config, IAfterEffectsHost host, ExtensionFreshness freshness)
{
    public const string StartupFolderName = "Startup";

    public async Task<JsonObject> InstallAsync(CancellationToken cancellationToken)
    {
        var homes = host.ScriptsHomes();

        if (homes.Count == 0)
        {
            return new JsonObject { ["installed"] = false, ["error"] = Messages.NoScriptsHome, ["hint"] = Messages.NoScriptsHomeHint };
        }

        var home = homes[0];
        var loader = Copy(home);
        ForgetWhatTheLoaderSaid();

        var report = new JsonObject
        {
            ["installed"] = true,
            ["version"] = home.Version,
            ["loader"] = loader,
            ["modules"] = Path.Combine(home.Path, ServerPaths.ModulesFolderName),
            ["digest"] = freshness.BundledDigest,
            ["afterEffects"] = await StartAsync(loader, cancellationToken).ConfigureAwait(false),
            ["steps"] = new JsonArray([.. Messages.InstallSteps.Select(step => (JsonNode)step)]),
        };

        return report;
    }

    /// <summary>The loader into Startup and the modules beside it, replacing whatever an earlier build left there.</summary>
    /// <remarks>Nothing is written where After Effects reads it. The modules are copied into a folder of their own and put in place by rename,
    /// and the loader is copied beside itself and renamed over the old one, because After Effects may start while an install runs: emptying the
    /// folder the loader reads and filling it again, twenty-three files one at a time, left a window in which the bridge was assembled from some
    /// modules of this build and some of the last one, and answered with a digest belonging to no build at all (b5c756ee, seen once).</remarks>
    private static string Copy(ScriptsHome home)
    {
        var startup = Path.Combine(home.Path, StartupFolderName);
        var modules = Path.Combine(home.Path, ServerPaths.ModulesFolderName);
        Directory.CreateDirectory(startup);
        Swap(Staged(modules), modules);

        var loader = Path.Combine(startup, ServerPaths.LoaderFileName);
        Rename(Path.Combine(ServerPaths.BundledExtensionDirectory, ServerPaths.LoaderFileName), loader);

        return loader;
    }

    /// <summary>The bundled modules in a folder of their own beside the one the loader reads, which it passes by because it reads the one name.</summary>
    private static string Staged(string modules)
    {
        var staged = $"{modules}.staged-{Random.Shared.Next():x8}";
        Directory.CreateDirectory(staged);

        foreach (var file in Directory.EnumerateFiles(Path.Combine(ServerPaths.BundledExtensionDirectory, ServerPaths.ModulesFolderName), "*.jsx"))
        {
            File.Copy(file, Path.Combine(staged, Path.GetFileName(file)), overwrite: true);
        }

        return staged;
    }

    /// <summary>Puts the staged modules where the loader reads them, by two renames rather than by writing into the folder in use.</summary>
    /// <remarks>Several servers share one machine and each of them may install, so losing the race is not a failure: whoever got there first put
    /// the same build in the same place, and the staged copy is dropped rather than the call. A rename that fails for any other reason stands,
    /// because then the folder is gone and the install has to say so.</remarks>
    private static void Swap(string staged, string modules)
    {
        var retired = $"{modules}.retired-{Random.Shared.Next():x8}";

        try
        {
            if (Directory.Exists(modules))
            {
                Directory.Move(modules, retired);
            }

            Directory.Move(staged, modules);
        }
        catch (IOException) when (Directory.Exists(modules))
        {
            Forget(staged);
        }

        Forget(retired);
    }

    /// <summary>A file copied beside its place and renamed over it, so a reader sees the old one or the new one and never half of either.</summary>
    private static void Rename(string bundled, string destination)
    {
        var staged = $"{destination}.staged-{Random.Shared.Next():x8}";
        File.Copy(bundled, staged, overwrite: true);
        File.Move(staged, destination, overwrite: true);
    }

    /// <summary>Drops the loader's log, which from here on is about the modules this install has just put there.</summary>
    /// <remarks>The loader appends to its log and never truncates it, while the diagnosis carries the last lines of that log whenever the
    /// installed bridge is not the one shipped. A module that failed to load on the eighth of October was still read back as a failure of
    /// today on the twenty-third, next to a bridge that was simply not connected. An install is the one moment the file is known to be about
    /// modules that are gone.</remarks>
    private void ForgetWhatTheLoaderSaid() => Forget(ServerPaths.LoaderLog(config.DataDirectory));

    /// <summary>Removes what is at the path, file or folder, and keeps quiet when it cannot: nothing here is worth failing an install over.</summary>
    private static void Forget(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                File.Delete(path);
            }
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }

    /// <summary>Starts the freshly installed bridge inside the running After Effects by running its loader there; a restart would do the same.</summary>
    private async Task<JsonObject> StartAsync(string loader, CancellationToken cancellationToken)
    {
        if (!host.IsRunning)
        {
            return new JsonObject { ["running"] = false, ["started"] = false, ["note"] = Messages.StartAfterEffectsNote };
        }

        var inject = Path.Combine(config.DataDirectory, "state", "start_bridge.jsx");
        Directory.CreateDirectory(Path.GetDirectoryName(inject)!);
        await File.WriteAllTextAsync(inject, $"$.evalFile(new File({JsonValue.Create(loader).ToJsonString()}));\n", cancellationToken).ConfigureAwait(false);

        var run = await host.RunScriptAsync(inject, cancellationToken).ConfigureAwait(false);

        return new JsonObject
        {
            ["running"] = true,
            ["started"] = run.Succeeded,
            ["note"] = run.Succeeded ? Messages.BridgeStartedNote : run.Detail,
        };
    }
}
