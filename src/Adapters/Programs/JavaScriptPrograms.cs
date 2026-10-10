using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Jint;
using Jint.Native;
using Jint.Native.Object;
using Jint.Runtime;
using Jint.Runtime.Interop;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Adapters.Programs;

/// <summary>Runs a JavaScript program whose only door to the outside is the command catalog: no files, no network, no clock but its own.</summary>
/// <remarks>The engine is synchronous, so the program runs on a thread of its own and each command it sends waits there for the link. Nothing of
/// the host is reachable from the script — the engine is created without CLR access — and the program is bounded three ways: the timeout it was
/// given, a memory ceiling, and a cap on how many commands it may send, because a program is a way to spend one call, not to hide a thousand.
/// The script command is put to the server's scripting setting first: without this, a server with scripts switched off ran them by way of one
/// line of JavaScript.</remarks>
internal sealed class JavaScriptPrograms(IAfterEffectsBridge bridge, ServerConfig config, ToolScopes scopes, ClientSessions clients) : IProgramRunner
{
    /// <summary>Commands one program may send.</summary>
    public const int MostCalls = 200;

    private const long MemoryBytes = 32L * 1024 * 1024;

    private const int MostLogLines = 200;

    private const int LongestLogLine = 1000;

    /// <summary>Commands the answer names when a program asked for one that is not in the catalog.</summary>
    private const int NearestCommands = 3;

    private const string Guard = """
        (function (commands, missing) {
            return new Proxy(commands, {
                get: function (catalog, name) { return name in catalog ? catalog[name] : missing(name); }
            });
        })
        """;

    public Task<ProgramRun> RunAsync(string code, JsonObject? input, TimeSpan timeout, CancellationToken cancellationToken) =>
        Task.Run(() => Run(code, input, timeout, cancellationToken), cancellationToken);

    private ProgramRun Run(string code, JsonObject? input, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var calls = new List<ProgramCall>();
        var log = new List<string>();
        var running = Stopwatch.StartNew();
        using var engine = new Engine(options => options
            .Strict()
            .LimitMemory(MemoryBytes)
            .TimeoutInterval(timeout)
            .CancellationToken(cancellationToken));

        engine.SetValue("input", Parsed(engine, input ?? []));
        engine.SetValue("log", new ClrFunction(engine, "log", (_, written) => Write(log, written)));
        engine.SetValue("ae", Guarded(engine, Catalog(engine, calls, log, running, timeout, cancellationToken)));

        try
        {
            var answer = engine.Evaluate($"(() => {{\n{code}\n}})()");

            return new ProgramRun(Serialized(engine, answer), log, calls, null);
        }
        catch (ProgramStopped stopped)
        {
            return new ProgramRun(null, log, calls, stopped.Reason);
        }
        catch (JavaScriptException failure)
        {
            return new ProgramRun(null, log, calls, Thrown(failure));
        }
        catch (Exception failure) when (failure is TimeoutException or ExecutionCanceledException)
        {
            return new ProgramRun(null, log, calls, new BridgeError(BridgeError.TimeoutType, $"the program ran longer than the {timeout.TotalSeconds:0.#} s it was given; the commands it had already sent stand."));
        }
        catch (Exception failure) when (failure is MemoryLimitExceededException or StatementsCountOverflowException or RecursionDepthOverflowException)
        {
            return new ProgramRun(null, log, calls, new BridgeError("ProgramTooLarge", $"the program asked for more than a program may use ({failure.GetType().Name}); split the work into several."));
        }
    }

    /// <summary>The catalog as an object of functions: <c>ae.layer_add({ kind: "text" })</c> for every command the bridge answers.</summary>
    private ObjectInstance Catalog(Engine engine, List<ProgramCall> calls, List<string> log, Stopwatch running, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var catalog = new JsObject(engine);

        foreach (var command in BridgeCommands.All)
        {
            catalog.FastSetDataProperty(command.Name, new ClrFunction(engine, command.Name, (_, arguments) =>
                Send(engine, command, arguments, calls, running, timeout, cancellationToken)));
        }

        catalog.FastSetDataProperty("log", new ClrFunction(engine, "log", (_, written) => Write(log, written)));

        return catalog;
    }

    /// <summary>The catalog behind a guard that answers a name it does not hold with the names that come closest to it.</summary>
    /// <remarks>The tools read ae_create_comp and ae_remove_layers while the commands they send read comp_create and layer_remove — the same
    /// words the other way round — and a program reaches the commands. Six of the eighteen failed calls in a fortnight of logs were that guess,
    /// and a plain object answered every one of them with "Property 'remove_layers' of object is not a function": the miss, and nothing about
    /// what to call instead. The guard is a proxy, so the name is judged where the program reads it and the catalog itself still holds exactly
    /// the commands the bridge registers, which the contract tests compare with its schema. A name that is read but never called costs nothing:
    /// what comes back is a function, and it speaks only when it is called.</remarks>
    private static ObjectInstance Guarded(Engine engine, ObjectInstance catalog)
    {
        var guard = engine.Evaluate(Guard);
        var missing = new ClrFunction(engine, "missing", (_, asked) => Missing(engine, asked));

        return guard.Call(JsValue.Undefined, [catalog, missing]).AsObject();
    }

    private static JsValue Missing(Engine engine, JsValue[] asked)
    {
        var name = asked.Length == 0 ? string.Empty : asked[0].ToString();
        var nearest = BridgeCommands.Nearest(name, NearestCommands);

        return new ClrFunction(engine, name, (_, _) =>
            throw new ProgramStopped(new BridgeError("NoSuchCommand", Messages.NoSuchCommand(name, nearest))));
    }

    [SuppressMessage("Usage", "VSTHRD002", Justification = "The engine calls a command as a plain function and cannot await it; the program runs on a pool thread of its own (RunAsync), and only that thread waits for the link.")]
    private JsValue Send(Engine engine, BridgeCommand command, JsValue[] arguments, List<ProgramCall> calls, Stopwatch running, TimeSpan timeout, CancellationToken cancellationToken)
    {
        if (calls.Count >= MostCalls)
        {
            throw new ProgramStopped(new BridgeError("TooManyCalls", $"a program sends at most {MostCalls} commands; this one asked for more."));
        }

        var parameters = Arguments(engine, arguments);
        var left = timeout - running.Elapsed;

        if (left <= TimeSpan.Zero)
        {
            throw new ProgramStopped(new BridgeError(BridgeError.TimeoutType, $"the program ran longer than the {timeout.TotalSeconds:0.#} s it was given; the commands it had already sent stand."));
        }

        if (command == BridgeCommands.Script && config.Scripting is ScriptAccess.Off)
        {
            throw new ProgramStopped(new BridgeError("ScriptingDisabled", "the program asked for the script command while ae_script is switched off on this server."));
        }

        if (scopes.For(command.Name) is { } scope && !clients.Current.May(scope))
        {
            throw new ProgramStopped(new BridgeError("Forbidden", ToolScopes.Refusal(command.Name, scope, clients.Current.Name)));
        }

        var started = running.Elapsed;
        var reply = bridge.SendAsync(command, parameters, left, cancellationToken).GetAwaiter().GetResult();

        calls.Add(new ProgramCall(command.Name, reply.IsOk, (int)(running.Elapsed - started).TotalMilliseconds));

        return reply.IsOk
            ? Parsed(engine, reply.Result ?? new JsonObject())
            : throw new JavaScriptException(Failure(engine, reply.Error!));
    }

    /// <summary>What the program threw and did not catch; a failed command keeps the bridge's own type, so the reply says NotFound rather than only that a program stopped.</summary>
    private static BridgeError Thrown(JavaScriptException failure)
    {
        var where = $"the program stopped at line {failure.Location.Start.Line}";

        if (failure.Error is not ObjectInstance thrown || thrown.Get("type").IsUndefined())
        {
            return new BridgeError("ProgramFailed", $"{where}: {failure.Message}");
        }

        var type = thrown.Get("type").AsString();

        return new BridgeError(type, $"{where}: {type}: {thrown.Get("message")}");
    }

    /// <summary>The bridge's error as the exception a program catches: <c>failure.type</c> and <c>failure.message</c> as the reply carries them.</summary>
    private static JsValue Failure(Engine engine, BridgeError error)
    {
        var failure = new JsObject(engine);
        failure.FastSetDataProperty("type", error.Type);
        failure.FastSetDataProperty("message", error.Message);
        failure.FastSetDataProperty("details", error.Details is null ? JsValue.Null : Parsed(engine, error.Details));

        return failure;
    }

    private static JsValue Write(List<string> log, JsValue[] written)
    {
        if (log.Count < MostLogLines)
        {
            var line = string.Join(' ', written.Select(value => value.ToString()));

            log.Add(line.Length <= LongestLogLine ? line : $"{line[..LongestLogLine]}…");
        }

        return JsValue.Undefined;
    }

    private static JsonObject Arguments(Engine engine, JsValue[] arguments) =>
        arguments.Length == 0 || arguments[0].IsUndefined() || arguments[0].IsNull()
            ? []
            : Serialized(engine, arguments[0]) as JsonObject ?? [];

    /// <summary>JSON both ways rather than a converter of our own: the engine's own JSON.parse and JSON.stringify know every shape a script can make.</summary>
    private static JsValue Parsed(Engine engine, JsonNode value) =>
        engine.Evaluate("JSON.parse").Call(JsValue.Undefined, [value.ToJsonString()]);

    private static JsonNode? Serialized(Engine engine, JsValue value)
    {
        var text = engine.Evaluate("JSON.stringify").Call(JsValue.Undefined, [value]);

        return text.IsUndefined() || text.IsNull() ? null : JsonNode.Parse(text.AsString());
    }

    /// <summary>A program stopped by a rule of the server rather than by the script itself.</summary>
    private sealed class ProgramStopped(BridgeError reason) : Exception(reason.Message)
    {
        public BridgeError Reason { get; } = reason;
    }
}
