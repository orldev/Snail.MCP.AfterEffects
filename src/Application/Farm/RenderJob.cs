using System.Text.RegularExpressions;
using Snail.MCP.AfterEffects.Application.Skills;

namespace Snail.MCP.AfterEffects.Application.Farm;

/// <summary>What a render job asks aerender for: the saved project, the composition, the output and the span, templates and settings.</summary>
internal sealed record JobRequest(
    string Project,
    string Comp,
    string Output,
    int? StartFrame = null,
    int? EndFrame = null,
    int? Increment = null,
    string? RenderTemplate = null,
    string? OutputTemplate = null,
    string? RenderSettings = null,
    string? OutputSettings = null,
    int Priority = 0,
    int Retries = 0,
    bool? MultiFrame = null,
    int? CpuPercent = null)
{
    /// <summary>How many frames the job renders when its span is known; null when aerender takes the span from the project.</summary>
    public int? FramesTotal =>
        StartFrame is { } start && EndFrame is { } end && end >= start ? (end - start) / Math.Max(1, Increment ?? 1) + 1 : null;

    /// <summary>The aerender command line for this request.</summary>
    public IReadOnlyList<string> Arguments()
    {
        var arguments = new List<string> { "-project", Project, "-comp", Comp, "-output", Output };

        Add(arguments, "-s", StartFrame);
        Add(arguments, "-e", EndFrame);
        Add(arguments, "-i", Increment);
        Add(arguments, "-RStemplate", RenderTemplate);
        Add(arguments, "-OMtemplate", OutputTemplate);
        Add(arguments, "-renderSettings", RenderSettings);
        Add(arguments, "-outputSettings", OutputSettings);

        if (MultiFrame is { } multiFrame)
        {
            arguments.AddRange(["-mfr", multiFrame ? "ON" : "OFF", Math.Clamp(CpuPercent ?? 100, 1, 100).ToString()]);
        }

        arguments.AddRange(["-v", "ERRORS_AND_PROGRESS", "-close", "DO_NOT_SAVE_CHANGES", "-sound", "OFF"]);

        return arguments;
    }

    private static void Add(List<string> arguments, string flag, int? value)
    {
        if (value is { } number)
        {
            arguments.AddRange([flag, number.ToString()]);
        }
    }

    private static void Add(List<string> arguments, string flag, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            arguments.AddRange([flag, value]);
        }
    }
}

/// <summary>The states a job passes through; a job is active while it is queued or running.</summary>
internal static class JobStates
{
    public const string Queued = "queued";

    public const string Running = "running";

    public const string Done = "done";

    public const string Failed = "failed";

    public const string Cancelled = "cancelled";

    public static bool IsActive(string state) => state is Queued or Running;
}

/// <summary>One render on the farm: its request, where it stands, what aerender has said so far.</summary>
/// <remarks>aerender's output is localised, so progress is read from the shape of the line (a timecode and a frame number in parentheses)
/// and never from its words; an error is any line that is not progress and names one.</remarks>
internal sealed partial class RenderJob(string id, JobRequest request, DateTimeOffset submitted, string logPath, SkillCatalog? owner = null)
{

    private readonly Lock _gate = new();
    private readonly Queue<string> _tail = new();
    private string _state = JobStates.Queued;
    private int _attempt;
    private int _framesRendered;
    private int? _lastFrame;
    private string? _lastTimecode;
    private string? _error;
    private DateTimeOffset? _started;
    private DateTimeOffset? _finished;
    private int? _pid;

    public const int TailLines = 200;

    public string Id => id;

    public JobRequest Request => request;

    public string LogPath => logPath;

    /// <summary>The skill catalog of the client that queued the job, which stays pinned while the job is active.</summary>
    public SkillCatalog? Owner => owner;

    public DateTimeOffset Submitted => submitted;

    public string State
    {
        get
        {
            lock (_gate)
            {
                return _state;
            }
        }
    }

    public bool IsActive => JobStates.IsActive(State);

    public int RetriesLeft
    {
        get
        {
            lock (_gate)
            {
                return Math.Max(0, request.Retries - (_attempt - 1));
            }
        }
    }

    public DateTimeOffset? Finished
    {
        get
        {
            lock (_gate)
            {
                return _finished;
            }
        }
    }

    public void Start(DateTimeOffset now, int pid)
    {
        lock (_gate)
        {
            _state = JobStates.Running;
            _attempt++;
            _started = now;
            _finished = null;
            _pid = pid;
            _framesRendered = 0;
            _error = null;
        }
    }

    /// <summary>Reads one output line: a frame rendered, or trouble.</summary>
    public void Observe(string line)
    {
        lock (_gate)
        {
            _tail.Enqueue(line);

            while (_tail.Count > TailLines)
            {
                _tail.Dequeue();
            }

            if (Progress.Match(line) is { Success: true } progress)
            {
                _framesRendered++;
                _lastTimecode = progress.Groups[1].Value;
                _lastFrame = int.Parse(progress.Groups[2].Value);
            }
            else if (!line.StartsWith("PROGRESS:", StringComparison.Ordinal) && Trouble.IsMatch(line))
            {
                _error = line.Trim();
            }
        }
    }

    public void Finish(DateTimeOffset now, int exitCode)
    {
        lock (_gate)
        {
            _finished = now;
            _pid = null;

            if (exitCode == 0 && _error is null)
            {
                _state = JobStates.Done;
            }
            else
            {
                _state = JobStates.Failed;
                _error ??= $"aerender exited with code {exitCode}.";
            }
        }
    }

    public void Fail(DateTimeOffset now, string reason)
    {
        lock (_gate)
        {
            _finished = now;
            _pid = null;
            _state = JobStates.Failed;
            _error = reason;
        }
    }

    public void Cancel(DateTimeOffset now)
    {
        lock (_gate)
        {
            _finished = now;
            _pid = null;
            _state = JobStates.Cancelled;
        }
    }

    /// <summary>Back to the queue for another attempt, keeping the log of the last one.</summary>
    public void Requeue()
    {
        lock (_gate)
        {
            _state = JobStates.Queued;
            _finished = null;
        }
    }

    public JsonObject Describe(int logLines)
    {
        lock (_gate)
        {
            var seconds = _started is { } started ? Math.Round(((_finished ?? DateTimeOffset.UtcNow) - started).TotalSeconds, 1) : (double?)null;
            var total = request.FramesTotal;
            var described = new JsonObject
            {
                ["id"] = id,
                ["state"] = _state,
                ["comp"] = request.Comp,
                ["project"] = request.Project,
                ["output"] = request.Output,
                ["priority"] = request.Priority,
                ["attempt"] = _attempt,
                ["retries_left"] = Math.Max(0, request.Retries - Math.Max(0, _attempt - 1)),
                ["submitted"] = submitted.ToString("O"),
                ["started"] = _started?.ToString("O"),
                ["finished"] = _finished?.ToString("O"),
                ["seconds"] = seconds,
                ["pid"] = _pid,
                ["progress"] = new JsonObject
                {
                    ["frames_rendered"] = _framesRendered,
                    ["frames_total"] = total,
                    ["percent"] = total is { } count and > 0 ? Math.Round(100.0 * _framesRendered / count, 1) : null,
                    ["last_frame"] = _lastFrame,
                    ["last_timecode"] = _lastTimecode,
                },
                ["error"] = _error,
                ["log"] = logPath,
            };

            if (logLines > 0)
            {
                described["log_tail"] = new JsonArray([.. _tail.TakeLast(logLines).Select(line => (JsonNode)line)]);
            }

            if (_state == JobStates.Done)
            {
                described["output_exists"] = File.Exists(request.Output);
            }

            return described;
        }
    }

    [GeneratedRegex(@"^PROGRESS:\s+(\d+[:;]\d+[:;]\d+[:;]\d+)\s+\((\d+)\)")]
    private static partial Regex Progress { get; }

    [GeneratedRegex(@"\b(ERROR|Error)\b")]
    private static partial Regex Trouble { get; }
}
