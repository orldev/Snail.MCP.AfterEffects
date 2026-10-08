namespace Snail.MCP.AfterEffects.Domain;

/// <summary>What the bridge says about itself in the first line after it connects: who it is, what it can do and the After Effects it runs in.</summary>
internal sealed record BridgeHello(
    string Token,
    string Version,
    int Protocol,
    IReadOnlyList<string> Abilities,
    string Digest,
    int Commands,
    string App,
    string Build,
    string Os,
    bool IsScriptingAllowed,
    string DataRoot,
    string Worker = BridgeHello.Desk,
    string Instance = "")
{
    /// <summary>The worker name of the After Effects a person works in: the one that set no name of its own.</summary>
    public const string Desk = "desk";

    /// <summary>The hello read off its line; null when the line is not a hello at all.</summary>
    public static BridgeHello? From(JsonNode? line)
    {
        if (line is not JsonObject envelope || envelope["hello"] is not JsonObject hello)
        {
            return null;
        }

        return new BridgeHello(
            Text(hello, "token"),
            Text(hello, "bridge"),
            hello["protocol"]?.GetValue<int>() ?? 0,
            [.. (hello["abilities"] as JsonArray ?? []).Select(ability => ability?.ToString() ?? string.Empty)],
            Text(hello, "digest"),
            hello["commands"]?.GetValue<int>() ?? 0,
            Text(hello, "app"),
            Text(hello, "build"),
            Text(hello, "os"),
            hello["scripting_allowed"]?.GetValue<bool>() ?? false,
            Text(hello, "data_root"),
            Text(hello, "worker") is { Length: > 0 } worker ? worker : Desk,
            Text(hello, "instance"));
    }

    /// <summary>The hello as the diagnose report shows it, without the token.</summary>
    public JsonObject Describe() => new()
    {
        ["app"] = App,
        ["build"] = Build,
        ["os"] = Os,
        ["bridge"] = Version,
        ["protocol"] = Protocol,
        ["abilities"] = new JsonArray([.. Abilities.Select(ability => (JsonNode)ability)]),
        ["digest"] = Digest,
        ["commands"] = Commands,
        ["scriptingAllowed"] = IsScriptingAllowed,
        ["dataRoot"] = DataRoot,
        ["worker"] = Worker,
        ["instance"] = Instance,
    };

    private static string Text(JsonObject hello, string key) => hello[key]?.ToString() ?? string.Empty;
}
