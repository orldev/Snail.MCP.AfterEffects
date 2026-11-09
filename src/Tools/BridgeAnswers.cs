using ModelContextProtocol.Protocol;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools;

/// <summary>A reply of the bridge as a tool's answer: its result as the data, or its error with the hint that fits its type.</summary>
/// <remarks>The bridge's type of failure is the answer's <c>type</c>, and what the bridge reported beside its error is the
/// answer's <c>data</c>: <c>details.accepted</c> of a bad request, <c>details.candidates</c> of an ambiguous name.</remarks>
internal static class BridgeAnswers
{
    public static CallToolResult Of(BridgeReply reply) =>
        reply.IsOk ? ToolAnswer.Ok(reply.Result ?? new JsonObject()) : Failed(reply.Error!);

    public static CallToolResult Failed(BridgeError error) =>
        ToolAnswer.Failed(error.Message, Messages.HintFor(error), error.Type, error.Details?.DeepClone());
}
