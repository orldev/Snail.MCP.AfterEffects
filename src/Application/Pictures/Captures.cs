using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Pictures;

/// <summary>Where captured frames land and how the picture comes back: the PNG After Effects wrote, read once it is complete.</summary>
/// <remarks>The server names the file rather than the bridge, because the server knows its data directory and the project folder, and the
/// path comes back in the answer so the user can find the picture after the model has looked at it.</remarks>
internal sealed class Captures(ServerConfig config)
{
    public const string FolderName = "captures";

    /// <summary>The path for the next capture of a composition: under the project folder when there is one, under the data directory otherwise.</summary>
    public string PathFor(string? comp, double? time)
    {
        var folder = Path.Combine(config.ProjectDirectory ?? config.DataDirectory, FolderName);
        var stem = string.IsNullOrWhiteSpace(comp) ? "frame" : string.Concat(comp.Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character));
        var stamp = time is { } at ? $"{at:0.###}s" : DateTime.Now.ToString("HHmmss");

        return Path.Combine(folder, $"{stem}-{stamp}.png");
    }

    /// <summary>The bytes of a capture once After Effects has finished writing it; null when nothing arrived within the wait.</summary>
    /// <remarks>After Effects writes the PNG after the script that asked for it has returned, on its own time, so the bridge cannot wait for
    /// it: the file is watched here until it exists and stops growing.</remarks>
    public static async Task<byte[]?> ReadWhenCompleteAsync(string path, TimeSpan within, CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + within;
        long last = -1;
        var stable = 0;

        while (DateTime.UtcNow < deadline)
        {
            await Task.Delay(100, cancellationToken).ConfigureAwait(false);

            var size = File.Exists(path) ? new FileInfo(path).Length : 0;
            stable = size > 0 && size == last ? stable + 1 : 0;
            last = size;

            if (stable >= 2)
            {
                return await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
            }
        }

        return null;
    }
}
