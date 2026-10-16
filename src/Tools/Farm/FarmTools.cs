using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Farm;

/// <summary>Background renders through aerender: the farm skill.</summary>
[Skill(Skills.Farm, SkillDescriptions.Farm)]
[McpServerToolType]
internal sealed class FarmTools(RenderFarm farm, ProjectForRender projects)
{
    [McpServerTool(Name = "ae_farm_submit", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Farm.Submit)]
    public async Task<CallToolResult> SubmitAsync(
        [Description("The composition to render, by name or id.")] string comp,
        [Description("Where to write the output: a file path, or a folder to take the composition's name; After Effects adds the extension the output module gives. Empty renders under the project folder's renders.")] string? output = null,
        [Description("A saved project file to render instead of the open project.")] string? project = null,
        [Description("Save the open project before rendering it, so aerender sees the latest changes; true by default.")] bool save = true,
        [Description("First frame to render; the composition's start when empty.")] int? startFrame = null,
        [Description("Last frame to render, inclusive; the composition's end when empty.")] int? endFrame = null,
        [Description("Render every nth frame; 1 renders all.")] int? increment = null,
        [Description("Render settings template by name, as ae_render_templates lists them.")] string? renderTemplate = null,
        [Description("Output module template by name, as ae_render_templates lists them.")] string? outputTemplate = null,
        [Description(ToolDescriptions.Parameters.AerenderSettings)] JsonObject? renderSettings = null,
        [Description(ToolDescriptions.Parameters.AerenderSettings)] JsonObject? outputSettings = null,
        [Description("Higher runs first among queued jobs; 0 by default.")] int priority = 0,
        [Description("How many times a failed job is tried again; 0 by default.")] int retries = 0,
        [Description("Multi-frame rendering on or off; aerender's default when empty.")] bool? multiFrame = null,
        [Description("Most CPU percent multi-frame rendering may take, 1 to 100.")] int? cpuPercent = null,
        CancellationToken cancellationToken = default)
    {
        if (!farm.IsAvailable)
        {
            return ToolResponse.Failure(Messages.FarmUnavailable, Messages.FarmUnavailableHint);
        }

        var resolved = await projects.ResolveAsync(project, comp, save, cancellationToken).ConfigureAwait(false);

        if (!resolved.IsResolved)
        {
            return ToolResponse.Failure(resolved.Refusal!);
        }

        var request = new JobRequest(
            resolved.Project!,
            resolved.Comp!,
            projects.OutputFor(output, resolved.Comp!),
            startFrame,
            endFrame,
            increment,
            renderTemplate,
            outputTemplate,
            Joined(renderSettings),
            Joined(outputSettings),
            priority,
            Math.Max(0, retries),
            multiFrame,
            cpuPercent);

        var job = farm.Submit(request);
        var described = job.Describe(0);
        described["project_saved"] = resolved.WasSaved;
        described["aerender"] = farm.Executable;

        return ToolResponse.Success(described);
    }

    [McpServerTool(Name = "ae_farm_jobs", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Farm.Jobs)]
    public CallToolResult Jobs(
        [Description(ToolDescriptions.Parameters.JobId)] string? id = null,
        [Description("Only jobs in this state: queued, running, done, failed or cancelled.")] string? state = null,
        [Description("Lines from the end of each job's log to include; 10 for one job, 0 for the list.")] int? logLines = null)
    {
        if (id is not null)
        {
            return farm.Find(id) is { } job
                ? ToolResponse.Success(job.Describe(logLines ?? 10))
                : ToolResponse.Failure(Messages.UnknownJob(id), Messages.UnknownJobHint);
        }

        var jobs = farm.Jobs
            .Where(job => state is null || string.Equals(job.State, state, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(job => job.Submitted)
            .Select(job => (JsonNode)job.Describe(logLines ?? 0));

        return ToolResponse.Success(new JsonObject
        {
            ["jobs"] = new JsonArray([.. jobs]),
            ["available"] = farm.IsAvailable,
            ["aerender"] = farm.Executable,
            ["parallel"] = farm.Jobs.Count(job => job.State == JobStates.Running),
        });
    }

    [McpServerTool(Name = "ae_farm_cancel", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Farm.Cancel)]
    public CallToolResult Cancel([Description(ToolDescriptions.Parameters.JobId)] string id)
    {
        if (farm.Find(id) is not { } job)
        {
            return ToolResponse.Failure(Messages.UnknownJob(id), Messages.UnknownJobHint);
        }

        return farm.TryCancel(id)
            ? ToolResponse.Success(job.Describe(0))
            : ToolResponse.Failure(Messages.JobNotActive(id, job.State));
    }

    [McpServerTool(Name = "ae_farm_retry", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Farm.Retry)]
    public CallToolResult Retry([Description(ToolDescriptions.Parameters.JobId)] string id)
    {
        if (!farm.IsAvailable)
        {
            return ToolResponse.Failure(Messages.FarmUnavailable, Messages.FarmUnavailableHint);
        }

        return farm.Retry(id) is { } job
            ? ToolResponse.Success(job.Describe(0))
            : ToolResponse.Failure(Messages.UnknownJob(id), Messages.UnknownJobHint);
    }

    [McpServerTool(Name = "ae_farm_cleanup", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Farm.Cleanup)]
    public CallToolResult Cleanup([Description("Forget finished jobs older than this many minutes; 0 forgets all of them.")] int olderThanMinutes = 0) =>
        ToolResponse.Success(new JsonObject { ["forgotten"] = farm.Cleanup(TimeSpan.FromMinutes(Math.Max(0, olderThanMinutes))), ["remaining"] = farm.Jobs.Count });

    /// <summary>Settings as aerender's command line takes them: name and value pairs joined by semicolons.</summary>
    private static string? Joined(JsonObject? settings) =>
        settings is null || settings.Count == 0 ? null : string.Join("; ", settings.Select(pair => $"{pair.Key}: {pair.Value?.ToString()}"));
}
