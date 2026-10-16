using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Application.Workers;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Workers;

/// <summary>After Effects instances the server launches and lends to clients: the workers skill.</summary>
[Skill(Skills.Workers, SkillDescriptions.Workers)]
[McpServerToolType]
internal sealed class WorkersTools(WorkerPool pool, ClientSessions clients, ServerConfig config, TimeProvider clock)
{
    [McpServerTool(Name = "ae_workers", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Workers.List)]
    public CallToolResult Workers()
    {
        pool.Reconcile();
        var now = clock.GetUtcNow();

        return ToolResponse.Success(new JsonObject
        {
            ["workers"] = new JsonArray([.. pool.Workers.Select(worker => (JsonNode)worker.Describe(now))]),
            ["bound_to"] = clients.Current.Worker,
            ["available"] = pool.IsAvailable,
            ["executable"] = pool.Executable,
            ["min"] = config.Workers.Min,
            ["max"] = config.Workers.Max,
            ["idle_minutes"] = config.Workers.IdleMinutes,
        });
    }

    [McpServerTool(Name = "ae_lease", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Workers.Lease)]
    public async Task<CallToolResult> LeaseAsync(
        [Description(ToolDescriptions.Parameters.WorkerName)] string? worker = null,
        [Description("Launch a new After Effects when no worker is free; true by default.")] bool launch = true,
        [Description("A project file on the server's machine to copy into the worker's folder and open there.")] string? project = null,
        [Description("Give the worker back instead of taking one.")] bool release = false,
        CancellationToken cancellationToken = default)
    {
        var client = clients.Current;

        if (release)
        {
            var released = pool.Release(client);

            return ToolResponse.Success(new JsonObject { ["released"] = released?.Name, ["bound_to"] = client.Worker });
        }

        if (worker is null && launch && !pool.IsAvailable && pool.Workers.All(known => !known.IsFree))
        {
            return ToolResponse.Failure(Messages.WorkersUnavailable, Messages.WorkersUnavailableHint);
        }

        var leased = await pool.LeaseAsync(client, worker, launch, cancellationToken).ConfigureAwait(false);

        if (!leased.IsOk)
        {
            var refused = leased.Worker?.Describe(clock.GetUtcNow());

            return ToolResponse.Failure(leased.Refusal!, Hint(leased), refused);
        }

        var answer = new JsonObject { ["worker"] = leased.Worker!.Describe(clock.GetUtcNow()), ["bound_to"] = client.Worker };

        if (project is not null)
        {
            var opened = await pool.OpenCopyAsync(leased.Worker.Name, project, cancellationToken).ConfigureAwait(false);
            answer["project"] = opened.IsOk ? opened.Result : null;

            if (!opened.IsOk)
            {
                answer["project_error"] = opened.Error!.Message;
            }
        }

        return ToolResponse.Success(answer);
    }

    private static string? Hint(Lease lease) =>
        lease.Worker?.LeasedBy is not null ? Messages.WorkerHeldHint : lease.Worker is null ? Messages.PoolFullHint : Messages.UnknownWorkerHint;

    [McpServerTool(Name = "ae_stop_worker", ReadOnly = false, Destructive = true, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Workers.Stop)]
    public async Task<CallToolResult> StopWorkerAsync(
        [Description(ToolDescriptions.Parameters.WorkerName)] string worker,
        CancellationToken cancellationToken = default)
    {
        var stopped = await pool.TryStopAsync(worker, cancellationToken).ConfigureAwait(false);

        return stopped
            ? ToolResponse.Success(new JsonObject { ["stopping"] = worker, ["bound_to"] = clients.Current.Worker })
            : ToolResponse.Failure(Messages.UnknownWorker(worker), Messages.UnknownWorkerHint);
    }

}
