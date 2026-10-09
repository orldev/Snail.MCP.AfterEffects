using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Snail.MCP.AfterEffects.Adapters.Bridge;

/// <summary>One connection a bridge opened: requests go out as lines with ids, a reader matches the replies to them, and a reply that comes after its caller gave up is dropped by its id.</summary>
/// <remarks>Several requests may be in flight: the bridge serves them in order on one tick, so pipelining costs nothing and a client that
/// sends two tools in a row waits one tick, not two. The deadline travels both as a duration from arrival and as a clock time, because the
/// bridge and this server share a machine and a clock, and the bridge can only read the request once its main thread is free.</remarks>
internal sealed class BridgeLink : IAsyncDisposable
{
    private static readonly UTF8Encoding Utf8 = new(encoderShouldEmitUTF8Identifier: false);

    private static readonly JsonSerializerOptions Wire = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

    private readonly TcpClient _client;
    private readonly StreamReader _reader;
    private readonly Stream _stream;
    private readonly SemaphoreSlim _writing = new(1, 1);
    private readonly ConcurrentDictionary<string, TaskCompletionSource<JsonObject>> _pending = new(StringComparer.Ordinal);
    private readonly ILogger _logger;
    private readonly CancellationTokenSource _closing = new();
    private long _sequence;

    public BridgeLink(TcpClient client, StreamReader reader, BridgeHello hello, ILogger logger)
    {
        _client = client;
        _reader = reader;
        _stream = client.GetStream();
        _logger = logger;
        Hello = hello;
        Reading = Task.Run(ReadAsync);
    }

    public BridgeHello Hello { get; }

    /// <summary>The reader loop; it ends when the bridge closes the connection or the link is disposed.</summary>
    public Task Reading { get; }

    public bool IsAlive => !Reading.IsCompleted && _client.Connected;

    public DateTimeOffset LastReplyAt { get; private set; } = DateTimeOffset.UtcNow;

    public async Task<BridgeReply> SendAsync(BridgeCommand command, JsonObject parameters, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var id = Interlocked.Increment(ref _sequence).ToString();
        var request = new JsonObject
        {
            ["id"] = id,
            ["command"] = command.Name,
            ["params"] = parameters,
            ["deadline_s"] = Math.Round(timeout.TotalSeconds, 3),
            ["deadline_at"] = DateTimeOffset.UtcNow.Add(timeout).ToUnixTimeMilliseconds(),
        };

        if (Serialized(request) is not { } line)
        {
            return BridgeReply.Failed(BridgeError.NotFinite(command));
        }

        var answer = new TaskCompletionSource<JsonObject>(TaskCreationOptions.RunContinuationsAsynchronously);
        _pending[id] = answer;

        try
        {
            await WriteAsync(line, cancellationToken).ConfigureAwait(false);

            var reply = await answer.Task.WaitAsync(timeout, cancellationToken).ConfigureAwait(false);

            return Interpret(reply);
        }
        catch (TimeoutException)
        {
            return BridgeReply.Failed(BridgeError.Timeout(command, timeout));
        }
        catch (Exception failure) when (failure is IOException or SocketException or ObjectDisposedException)
        {
            return BridgeReply.Failed(BridgeError.Dropped(command));
        }
        finally
        {
            _pending.TryRemove(id, out _);
        }
    }

    private async Task WriteAsync(ReadOnlyMemory<byte> line, CancellationToken cancellationToken)
    {
        await _writing.WaitAsync(cancellationToken).ConfigureAwait(false);

        try
        {
            await _stream.WriteAsync(line, cancellationToken).ConfigureAwait(false);
            await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _writing.Release();
        }
    }

    private async Task ReadAsync()
    {
        try
        {
            while (await _reader.ReadLineAsync(_closing.Token).ConfigureAwait(false) is { } line)
            {
                if (line.Length == 0)
                {
                    continue;
                }

                Deliver(line);
            }
        }
        catch (Exception failure) when (failure is IOException or SocketException or ObjectDisposedException or OperationCanceledException)
        {
        }

        foreach (var waiting in _pending.Values)
        {
            waiting.TrySetException(new IOException("The bridge closed the connection."));
        }
    }

    private void Deliver(string line)
    {
        LastReplyAt = DateTimeOffset.UtcNow;

        if (TryParse(line) is not { } reply)
        {
            Log.BridgeLineNotJson(_logger, line.Length > 200 ? $"{line[..200]}…" : line);

            return;
        }

        var id = reply["id"]?.ToString();

        if (id is null || !_pending.TryGetValue(id, out var waiting))
        {
            Log.ReplyDropped(_logger, id);

            return;
        }

        waiting.TrySetResult(reply);
    }

    private static ReadOnlyMemory<byte>? Serialized(JsonObject request)
    {
        try
        {
            using var written = new MemoryStream();

            using (var writer = new Utf8JsonWriter(written, new JsonWriterOptions { Encoder = Wire.Encoder }))
            {
                request.WriteTo(writer, Wire);
            }

            written.WriteByte((byte)'\n');

            return written.TryGetBuffer(out var buffer)
                ? new ReadOnlyMemory<byte>(buffer.Array, buffer.Offset, buffer.Count)
                : written.ToArray();
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    private static JsonObject? TryParse(string line)
    {
        try
        {
            return JsonNode.Parse(line) as JsonObject;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static BridgeReply Interpret(JsonObject reply)
    {
        if (reply["ok"] is not JsonValue ok || !ok.TryGetValue<bool>(out var isOk))
        {
            return BridgeReply.Failed(BridgeError.Malformed(reply.ToJsonString()));
        }

        if (isOk)
        {
            return BridgeReply.Ok(reply["result"]?.DeepClone());
        }

        var error = reply["error"] as JsonObject;

        return BridgeReply.Failed(new BridgeError(
            error?["type"]?.ToString() ?? "Error",
            error?["message"]?.ToString() ?? "The bridge reported a failure without a message.",
            error?["details"]?.DeepClone()));
    }

    /// <summary>Writes one control message, such as the welcome, outside the request numbering.</summary>
    public Task TellAsync(JsonObject message, CancellationToken cancellationToken) =>
        Serialized(message) is { } line ? WriteAsync(line, cancellationToken) : Task.CompletedTask;

    public async ValueTask DisposeAsync()
    {
        await _closing.CancelAsync().ConfigureAwait(false);
        _client.Dispose();
        _stream.Dispose();
        _reader.Dispose();

        try
        {
            await Reading.ConfigureAwait(false);
        }
        catch (Exception)
        {
        }

        _closing.Dispose();
        _writing.Dispose();
    }

    /// <summary>The reader a listener hands over after it has read the hello off the stream itself.</summary>
    public static StreamReader ReaderOf(TcpClient client) => new(client.GetStream(), Utf8, detectEncodingFromByteOrderMarks: false, bufferSize: 65536, leaveOpen: true);
}
