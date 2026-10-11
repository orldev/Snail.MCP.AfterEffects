namespace Snail.MCP.AfterEffects.Application.Storage;

/// <summary>The areas of this server's storage a client may reach over HTTP, each a folder of its own.</summary>
internal static class VolumeAreas
{
    /// <summary>The project folder, or the server's files folder when there is no project: where uploads land and footage is kept.</summary>
    public const string Files = "files";

    /// <summary>Frames ae_capture_frame wrote.</summary>
    public const string Captures = "captures";

    /// <summary>What the farm and the render queue wrote when nobody named a place.</summary>
    public const string Renders = "renders";

    /// <summary>The logs of the farm's jobs, one per job id.</summary>
    public const string Jobs = "jobs";

    public static IReadOnlyList<string> All { get; } = [Files, Captures, Renders, Jobs];
}

/// <summary>Paths inside an area: relative, forward slashes, and never climbing out.</summary>
internal static class AreaPaths
{
    /// <summary>Whether a relative path stays inside its area: no absolute path, no drive, no segment that climbs up.</summary>
    public static bool IsConfined(string relative)
    {
        if (relative.Length == 0)
        {
            return true;
        }

        if (relative[0] is '/' or '\\' || relative.Contains(':', StringComparison.Ordinal) || relative.Contains('\0', StringComparison.Ordinal))
        {
            return false;
        }

        return relative.Split('/', '\\').All(segment => segment != "..");
    }

    /// <summary>The path with forward slashes and no leading or trailing ones, as the areas spell it.</summary>
    public static string Normal(string? relative) => (relative ?? string.Empty).Replace('\\', '/').Trim('/');
}
