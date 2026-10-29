using System.Text.Json.Serialization.Metadata;
using ModelContextProtocol;
using Snail.Sdk.Mcp.Hosting.Jobs;

namespace Snail.MCP.AfterEffects.Application.Farm;

/// <summary>A render of the farm followed as a job of the server.</summary>
/// <remarks>
/// The farm owns the render and moves it from queued to running to done; this follows one job of the farm every
/// <see cref="Interval"/> until it has ended, reports the frames as the job's progress, and ends the server's job with
/// the farm's description of the render. The following is the server's, not the model's: a client subscribed to the
/// job's resource is told when it ends, and a model reads the resource when it wants.
/// </remarks>
internal sealed class FarmJobs(RenderFarm farm, Jobs jobs, TimeProvider clock)
{
    /// <summary>The farm's description as the job's result: a JSON object, which the SDK's options know how to write.</summary>
    private static readonly JsonTypeInfo<JsonObject> AnswerType = (JsonTypeInfo<JsonObject>)McpJsonUtilities.DefaultOptions.GetTypeInfo(typeof(JsonObject));

    /// <summary>How many lines of the log the result carries.</summary>
    private const int LogLines = 10;

    /// <summary>How often the farm's job is looked at.</summary>
    public TimeSpan Interval { get; init; } = TimeSpan.FromSeconds(1);

    /// <summary>Submits the request to the farm and follows the render as a job of the server.</summary>
    public Job Submit(JobRequest request)
    {
        var render = farm.Submit(request);

        return jobs.Start("render", (progress, token) => FollowAsync(render, progress, token), AnswerType);
    }

    private async Task<JsonObject> FollowAsync(RenderJob render, IProgress<JobProgress> progress, CancellationToken cancellationToken)
    {
        progress.Report(ProgressOf(render));

        while (render.IsActive)
        {
            await Task.Delay(Interval, clock, cancellationToken).ConfigureAwait(false);
            progress.Report(ProgressOf(render));
        }

        return render.State switch
        {
            JobStates.Cancelled => throw new OperationCanceledException("cancelled on the farm"),
            JobStates.Failed => throw new InvalidOperationException(render.Describe(LogLines)["error"]?.GetValue<string>() ?? "the render failed"),
            _ => render.Describe(LogLines),
        };
    }

    private static JobProgress ProgressOf(RenderJob render)
    {
        var described = render.Describe(0);
        var frames = described["progress"] as JsonObject;
        var rendered = Number(frames?["frames_rendered"]) ?? 0;
        var total = Number(frames?["frames_total"]);

        return new JobProgress(rendered, total is > 0 ? total : null, $"{render.State}, {rendered:0} frames rendered");
    }

    /// <summary>A number of the description whatever width it was written with: a JsonValue gives back only the type it was made from.</summary>
    private static double? Number(JsonNode? node) =>
        node is JsonValue value && double.TryParse(value.ToJsonString(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var number) ? number : null;
}
