using System.Text.Encodings.Web;
using System.Text.Json;
using ModelContextProtocol.Protocol;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools;

/// <summary>Uniform tool results: the payload as text and as structured content, a failure flagged with <c>isError</c> so clients see it without reading the text.</summary>
/// <remarks>The text keeps the ok flag the model has always read; the structured copy and the error flag are what an orchestrator or an
/// evaluation harness reads, because a failure that looks like a success in the protocol is invisible to anything but the model.</remarks>
internal static class ToolResponse
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    /// <summary>The text of a successful response.</summary>
    public static string Ok(JsonNode data) => new JsonObject { ["ok"] = true, ["data"] = data }.ToJsonString(Options);

    /// <summary>The text of a failure in a shape the model can act on; <paramref name="data"/> carries per-item breakdowns of partially failed batches.</summary>
    public static string Error(string message, string? hint = null, JsonNode? data = null)
    {
        var envelope = new JsonObject
        {
            ["ok"] = false,
            ["error"] = message,
            ["hint"] = hint,
        };

        if (data is not null)
        {
            envelope["data"] = data;
        }

        return envelope.ToJsonString(Options);
    }

    /// <summary>A successful result: the payload as text and as structured content.</summary>
    public static CallToolResult Success(JsonNode data) => new()
    {
        Content = [new TextContentBlock { Text = Ok(data) }],
        StructuredContent = Element(data),
    };

    /// <summary>A failed result: flagged as an error for the protocol, explained with a hint for the model.</summary>
    public static CallToolResult Failure(string message, string? hint = null, JsonNode? data = null) => new()
    {
        IsError = true,
        Content = [new TextContentBlock { Text = Error(message, hint, data?.DeepClone()) }],
        StructuredContent = Element(new JsonObject { ["error"] = message, ["hint"] = hint, ["data"] = data?.DeepClone() }),
    };

    /// <summary>A bridge reply as a tool result: the result as is, or the bridge's error with the hint that fits its type.</summary>
    public static CallToolResult From(BridgeReply reply) =>
        reply.IsOk
            ? Success(reply.Result ?? new JsonObject())
            : Failure(reply.Error!.Message, Messages.HintFor(reply.Error), Decorate(reply.Error));

    /// <summary>A result with a picture next to the JSON: the picture travels as image content the model can look at.</summary>
    /// <remarks>The SDK's image block carries the base64 text itself as UTF-8 bytes, not the decoded picture: decoding here put raw PNG
    /// bytes on the wire, which the client rejected as invalid base64.</remarks>
    public static CallToolResult Pictured(JsonNode data, byte[] picture, string mimeType) => new()
    {
        Content =
        [
            new TextContentBlock { Text = Ok(data) },
            new ImageContentBlock { Data = System.Text.Encoding.UTF8.GetBytes(Convert.ToBase64String(picture)), MimeType = mimeType },
        ],
        StructuredContent = Element(data),
    };

    private static JsonNode Decorate(BridgeError error)
    {
        if (error.Details is not JsonObject details)
        {
            return new JsonObject { ["type"] = error.Type };
        }

        var decorated = details.DeepClone().AsObject();
        decorated["type"] = error.Type;

        return decorated;
    }

    private static JsonElement Element(JsonNode node) => JsonSerializer.SerializeToElement(node, Options);
}
