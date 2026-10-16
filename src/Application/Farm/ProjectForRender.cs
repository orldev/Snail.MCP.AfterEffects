using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Farm;

/// <summary>What aerender needs from the open project before a job can be queued: the project saved to disk and the composition's name.</summary>
/// <remarks>aerender reads the project file, not the application's memory, so unsaved work would not render; the project is saved first unless
/// the caller forbids it, and a project that was never saved is refused with the reason.</remarks>
internal sealed class ProjectForRender(IAfterEffectsBridge bridge, ServerConfig config)
{
    public async Task<Resolution> ResolveAsync(string? project, string comp, bool save, CancellationToken cancellationToken)
    {
        var compName = comp;

        if (int.TryParse(comp, out var id))
        {
            if (!bridge.IsConnected)
            {
                return Resolution.Refused("The composition is given by id, but After Effects is not connected to look it up; pass its name.");
            }

            var info = await bridge.SendAsync(BridgeCommands.ItemInfo, new JsonObject { ["item"] = id }, null, cancellationToken).ConfigureAwait(false);

            if (!info.IsOk || info.Result?["name"]?.GetValue<string>() is not { } name)
            {
                return Resolution.Refused(info.Error?.Message ?? $"No item has the id {id}.");
            }

            compName = name;
        }

        if (project is not null)
        {
            var given = Expand(project);

            return File.Exists(given) ? Resolution.Of(given, compName, saved: false) : Resolution.Refused($"No project file at {given}.");
        }

        if (!bridge.IsConnected)
        {
            return Resolution.Refused("After Effects is not connected; pass the path of a saved project.");
        }

        var state = await bridge.SendAsync(BridgeCommands.ProjectInfo, [], null, cancellationToken).ConfigureAwait(false);

        if (!state.IsOk)
        {
            return Resolution.Refused(state.Error?.Message ?? "After Effects did not answer.");
        }

        var file = state.Result?["file"]?.GetValue<string>();
        var dirty = state.Result?["dirty"]?.GetValue<bool>() ?? false;

        if (file is null)
        {
            return Resolution.Refused("The open project has never been saved; save it with ae_project_save first, since aerender reads the file on disk.");
        }

        if (!dirty)
        {
            return Resolution.Of(file, compName, saved: false);
        }

        if (!save)
        {
            return Resolution.Refused("The open project has unsaved changes aerender would not see; pass save true, or save it yourself.");
        }

        var saved = await bridge.SendAsync(BridgeCommands.ProjectSave, [], null, cancellationToken).ConfigureAwait(false);

        return saved.IsOk ? Resolution.Of(file, compName, saved: true) : Resolution.Refused(saved.Error?.Message ?? "Saving the project failed.");
    }

    /// <summary>The output path as aerender takes it: a tilde expanded, a folder completed with the composition's name, the parent created.</summary>
    public string OutputFor(string? output, string comp)
    {
        var path = output is null ? Path.Combine(RenderRoot, comp) : Expand(output);

        if (Directory.Exists(path) || path.EndsWith(Path.DirectorySeparatorChar) || path.EndsWith('/'))
        {
            path = Path.Combine(path, comp);
        }

        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path))!);

        return Path.GetFullPath(path);
    }

    private string RenderRoot => Path.Combine(config.ProjectDirectory ?? config.DataDirectory, "renders");

    private static string Expand(string path) =>
        path.StartsWith('~') ? Path.Combine(ServerPaths.Home, path.TrimStart('~').TrimStart('/', '\\')) : path;

    /// <summary>The project path and composition name a job can be made of, or why not.</summary>
    public sealed record Resolution(string? Project, string? Comp, bool WasSaved, string? Refusal)
    {
        public bool IsResolved => Refusal is null;

        public static Resolution Of(string project, string comp, bool saved) => new(project, comp, saved, null);

        public static Resolution Refused(string reason) => new(null, null, false, reason);
    }
}
