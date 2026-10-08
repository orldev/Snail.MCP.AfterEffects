using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Snail.MCP.AfterEffects.Configuration;

/// <summary>Root server configuration, bound from a JSON file and environment variables.</summary>
internal sealed class ServerConfig
{
    public const string SectionName = "Snail";

    /// <summary>This server's own data root: the token the bridge presents, the announcements the bridge reads to find running servers, and
    /// the frames it captures; defaults to <c>~/.s3l-mcp-aftereffects</c>.</summary>
    public string DataDirectory { get; set; } = ServerPaths.DefaultDataDirectory;

    [ValidateObjectMembers]
    public BridgeOptions Bridge { get; set; } = new();

    /// <summary>stdio when the client starts this server; http when it runs on its own beside After Effects and clients connect to it.</summary>
    public Transport Transport { get; set; } = Transport.Stdio;

    [ValidateObjectMembers]
    public HttpOptions Http { get; set; } = new();

    [ValidateObjectMembers]
    public WorkersOptions Workers { get; set; } = new();

    /// <summary>The folder on this machine the work belongs to: captures and renders asked for without a path land in it. Defaults to the directory
    /// the client starts the server in, which is the folder the client works in; the home folder and the root count as no project, and so does
    /// every folder of a server reached over HTTP, which sees none of the client's.</summary>
    public string? ProjectDirectory { get; set; }

    /// <summary>Minutes without a tool call after which the process exits; 0, the default, disables the watchdog.</summary>
    /// <remarks>Off by default because the transport already solves the problem it was for: a client that ends, crashes or is killed closes
    /// the pipe, and the stdio server exits within a second. A watchdog on top ended sessions that were merely idle between two tasks.</remarks>
    [Range(0, int.MaxValue)]
    public int IdleTimeoutMinutes { get; set; }

    /// <summary>Minutes a loaded skill may go unused before its tools leave the catalog; 0, the default, keeps skills loaded, because a client that
    /// caches the tool list loses tools it still expects when they unload.</summary>
    [Range(0, int.MaxValue)]
    public int SkillIdleMinutes { get; set; }

    /// <summary>How many aerender renders the farm runs at once; 1, the default, renders jobs one after another.</summary>
    [Range(1, 16)]
    public int FarmParallel { get; set; } = 1;

    /// <summary>The token file this server writes next to its state and the bridge reads before it connects.</summary>
    public string TokenFile => Path.Combine(DataDirectory, "state", "token");

    /// <summary>Name this server signs its requests with, so the bridge's journal tells agents apart; empty means anonymous.</summary>
    public string? Agent { get; set; }

    /// <summary>Whether ae_script, which runs any ExtendScript, is offered: allow, or off for shared or untrusted set-ups.</summary>
    public ScriptAccess Scripting { get; set; } = ScriptAccess.Allow;
}

/// <summary>Settings of the loopback listener the bridge inside After Effects dials.</summary>
/// <remarks>The direction is the bridge's constraint, not this server's choice: ExtendScript's Socket cannot listen on macOS, so After Effects
/// opens the connection and the server waits for it. Every server announces its port under the data directory, and the bridge calls each
/// one it finds there, which is how several clients share one After Effects.</remarks>
internal sealed class BridgeOptions
{
    /// <summary>Port this server listens on for the bridge; 0, the default, takes a free one, which the announcement under the data directory names.</summary>
    [Range(0, 65535)]
    public int Port { get; set; }

    /// <summary>The address the bridge dials to reach this server, written into the announcement; the loopback by default. In a container the
    /// service name of the server, which After Effects' container resolves.</summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>The address the listener binds; empty binds the loopback when <see cref="Host"/> is the loopback and every interface otherwise.</summary>
    public string? Listen { get; set; }

    /// <summary>Timeout for one bridge reply; renders and imports pass their own longer one.</summary>
    [Range(1, int.MaxValue)]
    public int RequestTimeoutSeconds { get; set; } = 30;

    /// <summary>The agent name the link signs requests with; filled from the top-level setting when the section leaves it empty.</summary>
    public string? Agent { get; set; }

    /// <summary>Shared secret the bridge must present when it connects; empty reads the token file, which the first server to start writes.</summary>
    public string? Token { get; set; }

    /// <summary>Where that token lives; filled from the data directory when the section leaves it empty.</summary>
    public string? TokenFile { get; set; }
}

/// <summary>Settings of the HTTP server: where it listens, the token clients present, and the links it hands out.</summary>
internal sealed class HttpOptions
{
    /// <summary>The address to listen on; a container listens on every interface, a server started by hand on the loopback only.</summary>
    public string Url { get; set; } = "http://127.0.0.1:8080";

    /// <summary>The token a client presents as a bearer and on the page login; empty reads <see cref="TokenFile"/>.</summary>
    public string? Token { get; set; }

    /// <summary>A file holding the token, the way a container receives a secret.</summary>
    public string? TokenFile { get; set; }

    /// <summary>The address clients reach this server at behind a reverse proxy, which links to files are built on; empty uses <see cref="Url"/>.</summary>
    public string? PublicUrl { get; set; }

    /// <summary>Minutes a signed link to a file stays valid.</summary>
    [Range(1, 1440)]
    public int LinkMinutes { get; set; } = 15;

    /// <summary>Megabytes an upload sends in one request, which a reverse proxy must read within its own body timeout.</summary>
    [Range(1, 64)]
    public int UploadPartMb { get; set; } = 16;
}

/// <summary>The pool of After Effects instances this server launches for clients that want one of their own, beside the desk a person works in.</summary>
internal sealed class WorkersOptions
{
    /// <summary>Workers kept running at all times; 0, the default, launches them only when a client leases one.</summary>
    [Range(0, 16)]
    public int Min { get; set; }

    /// <summary>Most workers alive at once; 0 switches launching off, so only After Effects started by hand connect.</summary>
    [Range(0, 16)]
    public int Max { get; set; } = 2;

    /// <summary>Minutes a worker may sit without a command before it quits on its own; 0 keeps workers alive.</summary>
    [Range(0, 1440)]
    public int IdleMinutes { get; set; } = 15;

    /// <summary>Seconds a launched After Effects has to connect its bridge before it counts as failed.</summary>
    [Range(10, 900)]
    public int StartupSeconds { get; set; } = 120;

    /// <summary>Start workers without the interface, which After Effects supports on Windows only; ignored elsewhere.</summary>
    /// <remarks>Bound from <c>Workers:Headless</c> (<c>SNAIL_MCP_AFTEREFFECTS_WORKERS__HEADLESS</c>), the name the setting has always had.</remarks>
    [ConfigurationKeyName("Headless")]
    public bool IsHeadless { get; set; }

    /// <summary>The After Effects executable to launch; the newest installed one when empty.</summary>
    public string? Executable { get; set; }
}
