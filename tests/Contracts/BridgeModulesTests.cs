using Jint;
using Jint.Native;
using Snail.MCP.AfterEffects.Domain;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The bridge's modules run, here, outside After Effects: every file is executed in order against a stand-in for the host, and
/// what they registered is compared with the schema the server ships.</summary>
/// <remarks>The other extension tests read the sources as text, which tells them what a file says and not what it does. A module that
/// throws while it loads — a reserved word used as a key, a helper that is not there, a table built from a value that is undefined — says
/// nothing to a reader and nothing to a regular expression: inside After Effects the loader stops at it and the bridge already in memory
/// keeps answering with the commands of an older build. That is what this test is for.</remarks>
public sealed class BridgeModulesTests
{
    /// <summary>What the modules may lean on while they load: the host's globals, enough of them to build the tables a module keeps at the
    /// top of its file. Nothing here answers for behaviour; the handlers are closures and none of them runs.</summary>
    private const string Host = """
        var $ = { global: this, os: "Macintosh OS 26.6.2/64", writeln: function () { }, evalFile: function () { } };

        function File(path) {
            if (!(this instanceof File)) { return new File(path); }
            this.fsName = String(path);
            this.name = String(path).split("/").pop();
            this.exists = false;
            this.length = 0;
            this.parent = { exists: true, fsName: "/tmp", create: function () { return true; } };
        }

        File.prototype.open = function () { return false; };
        File.prototype.close = function () { return true; };
        File.prototype.read = function () { return ""; };
        File.prototype.write = function () { return true; };
        File.prototype.remove = function () { return true; };

        function Folder(path) {
            if (!(this instanceof Folder)) { return new Folder(path); }
            this.fsName = String(path);
            this.name = String(path).split("/").pop();
            this.exists = false;
        }

        Folder.prototype.create = function () { return true; };
        Folder.prototype.getFiles = function () { return []; };
        Folder.userData = new Folder("/tmp/userData");

        function Socket() { this.connected = false; }

        Socket.prototype.open = function () { return false; };
        Socket.prototype.close = function () { return true; };
        Socket.prototype.read = function () { return ""; };
        Socket.prototype.writeln = function () { return true; };

        var app = {
            version: "26.5x89",
            buildName: "26.5x89",
            language: 1620,
            isoLanguage: "ru_RU",
            memoryInUse: 0,
            isRenderEngine: false,
            isWatchFolder: false,
            project: null,
            preferences: { havePref: function () { return false; }, getPrefAsLong: function () { return 0; } },
            settings: { haveSetting: function () { return false; } },
            fonts: null,
            activeViewer: null,
            availableGPUAccelTypes: [],
            getenv: function () { return ""; },
            scheduled: [],
            scheduleTask: function (code, every, repeat) { app.scheduled.push({ code: code, every: every, repeat: repeat }); return app.scheduled.length; },
            cancelTask: function () { return true; },
            beginUndoGroup: function () { },
            endUndoGroup: function () { },
            beginSuppressDialogs: function () { },
            endSuppressDialogs: function () { },
            findMenuCommandId: function () { return 0; },
            executeCommand: function () { }
        };
        """;

    /// <summary>Every module loads and the commands they register are exactly the ones the schema lists, with the same parameters and the
    /// same read-only and destructive flags.</summary>
    [Fact]
    public void EveryModule_Loads_AndRegistersTheCommandsOfTheSchema()
    {
        var engine = Loaded();
        var registry = engine.Evaluate("$.global.SnailBridge.registry").AsObject();
        var schema = (JsonObject)JsonNode.Parse(File.ReadAllText(Path.Combine(Repository.Root, "extension", "commands.json")))!["commands"]!;
        var registered = registry.GetOwnProperties().Select(entry => entry.Key.AsString()).Order(StringComparer.Ordinal).ToList();

        Assert.Equal(schema.Select(command => command.Key).Order(StringComparer.Ordinal), registered);

        var differences = new List<string>();

        foreach (var (name, listed) in schema)
        {
            var spec = registry.Get(name).AsObject();
            var parameters = Strings(spec.Get("params"));
            var expected = ((JsonArray?)listed!["params"] ?? []).Select(entry => entry!.ToString()).ToList();

            if (!parameters.SequenceEqual(expected, StringComparer.Ordinal))
            {
                differences.Add($"{name} reads {string.Join(", ", parameters)}, the schema says {string.Join(", ", expected)}");
            }

            Flag(differences, name, "read_only", listed, spec.Get("readOnly").AsBoolean());
            Flag(differences, name, "destructive", listed, spec.Get("destructive").AsBoolean());
        }

        Assert.True(differences.Count == 0, string.Join("; ", differences));
    }

    /// <summary>Every command has a handler to call, so a registration that forgot one fails here rather than when a client calls it.</summary>
    [Fact]
    public void EveryCommand_HasAHandler()
    {
        var engine = Loaded();
        var registry = engine.Evaluate("$.global.SnailBridge.registry").AsObject();
        var without = registry.GetOwnProperties()
            .Where(entry => entry.Value.Value.AsObject().Get("handler").IsCallable() == false)
            .Select(entry => entry.Key.AsString())
            .ToList();

        Assert.True(without.Count == 0, $"commands with no handler: {string.Join(", ", without)}");
    }

    /// <summary>The bridge names itself the way the server expects before anything else happens: version, protocol and the abilities the
    /// server looks for in a welcome.</summary>
    [Fact]
    public void TheBridge_NamesItself_WithTheProtocolTheServerSpeaks()
    {
        var engine = Loaded();

        Assert.Equal(BridgeProtocol.Version, (int)engine.Evaluate("$.global.SnailBridge.PROTOCOL").AsNumber());
        Assert.Contains("deadline", Strings(engine.Evaluate("$.global.SnailBridge.ABILITIES")));
        Assert.Equal("desk", engine.Evaluate("$.global.SnailBridge.workerName()").AsString());
    }

    /// <summary>The tick comes back on its own: a command that takes the scheduled task down with it used to leave the bridge dead inside a
    /// healthy After Effects, until somebody installed it again. A second task, rare and doing nothing that can fail, watches the first.</summary>
    [Fact]
    public void TheWatchdog_StartsTheTickAgain_WhenItHasGoneQuiet()
    {
        var engine = Loaded();
        engine.Evaluate("$.global.SnailBridge.start()");

        var scheduled = engine.Evaluate("app.scheduled.map(function (task) { return task.code; })");

        Assert.Equal(["__snailBridgeTick()", "__snailBridgeWatch()"], Strings(scheduled));

        engine.Evaluate("$.global.SnailBridge.state.lastTickAt = 0; $.global.__snailBridgeWatch();");

        Assert.Equal(1, (int)engine.Evaluate("$.global.SnailBridge.state.revivals").AsNumber());
        Assert.Equal("__snailBridgeTick()", engine.Evaluate("app.scheduled[app.scheduled.length - 1].code").AsString());
    }

    private static Engine Loaded()
    {
        var engine = new Engine(options => options.LimitMemory(64 * 1024 * 1024).TimeoutInterval(TimeSpan.FromSeconds(30)));
        engine.Execute(Host);

        foreach (var module in Modules())
        {
            try
            {
                engine.Execute(File.ReadAllText(module));
            }
            catch (Exception failure)
            {
                Assert.Fail($"{Path.GetFileName(module)} did not load: {failure.Message}");
            }
        }

        return engine;
    }

    private static IEnumerable<string> Modules() =>
        Directory.EnumerateFiles(Path.Combine(Repository.Root, "extension", "(snail_bridge)"), "*.jsx").Order(StringComparer.Ordinal);

    private static List<string> Strings(JsValue value) =>
        [.. value.AsArray().Select(entry => entry.AsString())];

    private static void Flag(List<string> differences, string name, string flag, JsonNode? listed, bool registered)
    {
        var expected = listed![flag]?.GetValue<bool>() ?? false;

        if (expected != registered)
        {
            differences.Add($"{name} is {(registered ? string.Empty : "not ")}{flag} in the module and {(expected ? string.Empty : "not ")}{flag} in the schema");
        }
    }
}
