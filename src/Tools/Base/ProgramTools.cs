using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>A program that calls the commands itself: one call for a build of many steps, one reply with what it returns.</summary>
[McpServerToolType]
internal sealed class ProgramTools(IProgramRunner programs)
{
    [McpServerTool(Name = "run_program", ReadOnly = false, Destructive = true, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Application.Program)]
    public async Task<CallToolResult> ProgramAsync(
        [Description(ToolDescriptions.Parameters.ProgramCode)] string code,
        [Description("Values the program reads as input, e.g. {\"count\": 8}.")] JsonObject? input = null,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 120,
        CancellationToken cancellationToken = default)
    {
        if (!ToolLimits.IsTimeout(timeoutSeconds))
        {
            return ToolResponse.Failure(Messages.TimeoutOutOfRange, Messages.TimeoutRangeHint);
        }

        var run = await programs.RunAsync(code, input, ToolLimits.Timeout(timeoutSeconds), cancellationToken).ConfigureAwait(false);
        var data = new JsonObject
        {
            ["result"] = run.Result?.DeepClone(),
            ["calls"] = run.Calls.Count,
            ["log"] = new JsonArray([.. run.Log.Select(line => JsonValue.Create(line))]),
            ["commands"] = new JsonArray([.. run.Calls.Select(call => (JsonNode)new JsonObject
            {
                ["command"] = call.Command,
                ["ok"] = call.IsOk,
                ["ms"] = call.Milliseconds,
            })]),
        };

        return run.Error is null
            ? ToolResponse.Success(data)
            : ToolResponse.Failure(run.Error.Message, Messages.HintFor(run.Error) ?? Messages.ProgramStoppedHint, data);
    }
}
