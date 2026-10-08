
namespace Snail.MCP.AfterEffects.Domain;

/// <summary>Why a command did not produce a result: the bridge's own error, or a failure of the link itself.</summary>
internal sealed record BridgeError(string Type, string Message, JsonNode? Details = null)
{
    public const string UnavailableType = "Unavailable";

    public const string TimeoutType = "Timeout";

    public const string DroppedType = "Dropped";

    public const string MalformedType = "Malformed";

    public const string ExpiredType = "Expired";

    public const string WorkerGoneType = "WorkerGone";

    public static BridgeError Unavailable() =>
        new(UnavailableType, "No bridge inside After Effects is connected to this server.");

    /// <summary>The client is bound to a worker whose After Effects is not connected: it quit, crashed or has not come up yet.</summary>
    public static BridgeError WorkerGone(string worker) =>
        new(WorkerGoneType, $"The worker '{worker}' this client is bound to has no After Effects connected.", new JsonObject { ["worker"] = worker });

    /// <summary>The command is carried as data, not only inside the sentence: the hint that follows a timeout depends on what After Effects is busy with.</summary>
    public static BridgeError Timeout(BridgeCommand command, TimeSpan waited) =>
        new(TimeoutType, $"After Effects did not answer '{command.Name}' within {waited.TotalSeconds:0.#} s.", Of(command));

    public static BridgeError Dropped(BridgeCommand command) =>
        new(DroppedType, $"After Effects closed the link while '{command.Name}' was running.", Of(command));

    /// <summary>JSON has no NaN or infinity, so a request carrying one cannot be written at all.</summary>
    public static BridgeError NotFinite(BridgeCommand command) =>
        new("BadRequest", $"A number sent with '{command.Name}' is not finite (NaN or infinity), which JSON cannot carry.", Of(command));

    public static BridgeError Malformed(string line) =>
        new(MalformedType, $"The bridge answered with something that is not a reply: {Truncate(line)}");

    private static JsonObject Of(BridgeCommand command) => new() { ["command"] = command.Name };

    private static string Truncate(string text) => text.Length <= 200 ? text : $"{text[..200]}…";
}
