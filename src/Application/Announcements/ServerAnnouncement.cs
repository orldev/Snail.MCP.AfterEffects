using System.Diagnostics;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Application.Announcements;

/// <summary>How the bridge finds this server: one file per server process under the data directory, naming the port it listens on.</summary>
/// <remarks>One file per process rather than one list: every MCP client starts a server of its own, and several of them writing one file
/// would race. A file is created when the listener is up and deleted when the server stops; a process that died without deleting it is
/// recognised by its pid and swept by the next server to start, and the bridge ignores a port nobody answers on.</remarks>
internal sealed class ServerAnnouncement(ServerConfig config, ILogger<ServerAnnouncement> logger)
{
    public const string FolderName = "servers";

    private string? _file;

    /// <summary>The port announced, once the listener is up; 0 before.</summary>
    public int Port { get; private set; }

    public string Folder => Path.Combine(config.DataDirectory, "state", FolderName);

    public void Publish(int port)
    {
        Directory.CreateDirectory(Folder);
        Sweep();

        var file = Path.Combine(Folder, FileName(Environment.ProcessId));
        var announcement = new JsonObject
        {
            ["pid"] = Environment.ProcessId,
            ["machine"] = Environment.MachineName,
            ["port"] = port,
            ["host"] = config.Bridge.Host,
            ["version"] = ThisAssembly.InformationalVersion,
            ["agent"] = config.Bridge.Agent,
            ["started"] = DateTimeOffset.UtcNow.ToString("O"),
        };

        File.WriteAllText(file, announcement.ToJsonString());
        _file = file;
        Port = port;
        Log.PortAnnounced(logger, port, file);
    }

    public void Withdraw()
    {
        if (_file is null)
        {
            return;
        }

        try
        {
            File.Delete(_file);
        }
        catch (IOException failure)
        {
            Log.AnnouncementNotRemoved(logger, _file, failure.Message);
        }

        _file = null;
        Port = 0;
    }

    /// <summary>The file a server of this machine announces itself in: its pid, then the machine, since a server in a container sees neither the
    /// host's processes nor their pids and must leave their announcements alone.</summary>
    public static string FileName(int pid) => $"{pid}.{MachineSlug(Environment.MachineName)}.json";

    /// <summary>Removes announcements of this machine's processes that are no longer running; another machine's, a container's or the host's
    /// as seen from the other side, are not this server's to judge.</summary>
    private void Sweep()
    {
        var own = MachineSlug(Environment.MachineName);

        foreach (var file in Directory.EnumerateFiles(Folder, "*.json"))
        {
            var parts = Path.GetFileNameWithoutExtension(file).Split('.', 2);
            var machine = parts.Length > 1 ? parts[1] : own;

            if (!int.TryParse(parts[0], out var pid) || pid == Environment.ProcessId || !string.Equals(machine, own, StringComparison.Ordinal) || IsRunning(pid))
            {
                continue;
            }

            try
            {
                File.Delete(file);
                Log.AnnouncementSwept(logger, pid);
            }
            catch (IOException)
            {
            }
        }
    }

    private static string MachineSlug(string machine)
    {
        var slug = new string([.. machine.ToLowerInvariant().Select(character => char.IsAsciiLetterOrDigit(character) ? character : '-')]).Trim('-');

        return slug.Length == 0 ? "machine" : slug;
    }

    private static bool IsRunning(int pid)
    {
        try
        {
            using var process = Process.GetProcessById(pid);

            return !process.HasExited;
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (InvalidOperationException)
        {
            return false;
        }
    }
}
