using System.Net.Sockets;
using System.Text;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>What After Effects does on the wire, without After Effects: connects to the server's port, says hello, and answers requests the way a test scripts it.</summary>
/// <remarks>The hello and the reply envelope are copied from the real bridge's lines, so a test over this fake exercises the same parser the
/// real bridge meets; a shape invented here would only test itself.</remarks>
internal sealed class FakeBridge : IAsyncDisposable
{
    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly Stream _stream;
    private readonly Func<JsonObject, Task<JsonObject?>> _answer;
    private readonly CancellationTokenSource _closing = new();
    private readonly Task _serving;

    private FakeBridge(TcpClient client, Func<JsonObject, Task<JsonObject?>> answer, string? welcome, string? refusal, string instance = "")
    {
        Instance = instance;
        _client = client;
        _stream = client.GetStream();
        _reader = new StreamReader(_stream, new UTF8Encoding(false));
        _answer = answer;
        WelcomedBy = welcome;
        Refusal = refusal;
        _serving = refusal is null ? Task.Run(ServeAsync) : Task.CompletedTask;
    }

    /// <summary>The name of the server that welcomed this bridge; null when it was refused.</summary>
    public string? WelcomedBy { get; }

    public string? Refusal { get; }

    /// <summary>The instance id this bridge said hello with.</summary>
    public string Instance { get; }

    public List<JsonObject> Requests { get; } = [];

    /// <summary>An answer that needs no waiting, as most scripted replies are.</summary>
    public static Func<JsonObject, Task<JsonObject?>> Always(Func<JsonObject, JsonObject?> answer) => request => Task.FromResult(answer(request));

    public static async Task<FakeBridge> ConnectAsync(int port, string token, Func<JsonObject, Task<JsonObject?>> answer, string app = "26.5x89", string digest = "fake0000", string worker = "desk")
    {
        var client = new TcpClient();
        await client.ConnectAsync("127.0.0.1", port);

        var instance = Guid.NewGuid().ToString("N")[..8];
        var stream = client.GetStream();
        var hello = new JsonObject
        {
            ["hello"] = new JsonObject
            {
                ["token"] = token,
                ["bridge"] = "0.1.0",
                ["protocol"] = 1,
                ["abilities"] = new JsonArray("hello_token", "params_gate", "undo_group", "deadline", "expired"),
                ["digest"] = digest,
                ["commands"] = 3,
                ["app"] = app,
                ["build"] = app,
                ["os"] = "Macintosh OS 26.6.2/64",
                ["scripting_allowed"] = true,
                ["data_root"] = "/tmp/fake",
                ["worker"] = worker,
                ["instance"] = instance,
            },
        };

        await stream.WriteAsync(Encoding.UTF8.GetBytes($"{hello.ToJsonString()}\n"));

        var reader = new StreamReader(stream, new UTF8Encoding(false), false, 65536, leaveOpen: true);
        var first = await reader.ReadLineAsync().WaitAsync(TimeSpan.FromSeconds(10));
        var answerLine = first is null ? null : JsonNode.Parse(first) as JsonObject;

        return new FakeBridge(client, answer, answerLine?["welcome"]?["name"]?.ToString(), answerLine?["refused"]?["reason"]?.ToString(), instance);
    }

    /// <summary>The reply the real bridge gives: the id, ok, and either the result or the error.</summary>
    public static JsonObject Ok(JsonObject request, JsonNode result) => new() { ["id"] = request["id"]?.DeepClone(), ["ok"] = true, ["result"] = result, ["ms"] = 1 };

    public static JsonObject Error(JsonObject request, string type, string message) => new()
    {
        ["id"] = request["id"]?.DeepClone(),
        ["ok"] = false,
        ["error"] = new JsonObject { ["type"] = type, ["message"] = message },
    };

    /// <summary>Writes one line exactly as given, for a test about what the server does with a line no bridge should send.</summary>
    public Task SayAsync(string line) => _stream.WriteAsync(Encoding.UTF8.GetBytes($"{line}\n"), _closing.Token).AsTask();

    private async Task ServeAsync()
    {
        try
        {
            while (await _reader.ReadLineAsync(_closing.Token) is { } line)
            {
                if (line.Length == 0) continue;

                var request = (JsonObject)JsonNode.Parse(line)!;

                lock (Requests) Requests.Add(request);

                if (await _answer(request) is { } reply)
                {
                    await _stream.WriteAsync(Encoding.UTF8.GetBytes($"{reply.ToJsonString()}\n"), _closing.Token);
                }
            }
        }
        catch (Exception failure) when (failure is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _closing.CancelAsync();
        _client.Dispose();

        try
        {
            await _serving;
        }
        catch (Exception)
        {
        }

        _reader.Dispose();
        await _stream.DisposeAsync();
        _closing.Dispose();
    }
}
