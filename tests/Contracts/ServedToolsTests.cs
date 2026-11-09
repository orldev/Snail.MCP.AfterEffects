using Microsoft.Extensions.DependencyInjection;
using ModelContextProtocol;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Extensions;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.Sdk.Mcp.Hosting.Answers;
using Snail.Sdk.Mcp.Hosting.Jobs;
using Snail.Sdk.Mcp.Hosting.Sessions;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The tools as a client over HTTP is served them, by the server built in this process over a bridge that only remembers: the
/// grant of a key held at every layer, and what a client of the 2026-07-28 revision, which keeps no session, is told.</summary>
public sealed class ServedToolsTests
{
    private const string Token = "served-tools-token";

    private const string Sessionless = "2026-07-28";

    private static readonly TimeSpan Patience = TimeSpan.FromSeconds(20);

    /// <summary>run_tools sends its steps to After Effects as one sequence, and a step the key does not open refuses the whole list before
    /// any of it is sent, in the one type every layer of the fleet refuses with.</summary>
    [Fact]
    public async Task RunTools_SendsItsStepsAsOneSequence_AndRefusesAStepTheKeyDoesNotOpen()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var (_, reader) = server.Services.GetRequiredService<ClientKeys>().Mint("viewer", [Scopes.Read]);
        await using var operatorClient = await ClientAsync(server, Token);
        await using var viewer = await ClientAsync(server, reader);
        var steps = new JsonArray(new JsonObject { ["command"] = "project_info", ["params"] = new JsonObject() }, new JsonObject { ["command"] = "layer_add", ["params"] = new JsonObject { ["kind"] = "null" } });

        var refused = await viewer.CallToolAsync("run_tools", new Dictionary<string, object?> { ["steps"] = steps });
        var sentWhenRefused = server.Bridge.Sent.Count;
        var ran = await operatorClient.CallToolAsync("run_tools", new Dictionary<string, object?> { ["steps"] = steps });

        Assert.True(refused.IsError);
        Assert.Equal(GrantOptions.NotGranted, refused.Type());
        Assert.Contains("layer_add", refused.Error(), StringComparison.Ordinal);
        Assert.Equal(0, sentWhenRefused);
        Assert.True(ran.IsOk(), ran.Text());
        Assert.Equal(["sequence"], server.Bridge.Sent.Select(sent => sent.Command.Name));
    }

    /// <summary>A call the key does not open is refused with the one hint that names what to do about the key, whichever layer refuses it: the
    /// door of a tool called directly, a step of run_tools, and a command of a run_program program.</summary>
    /// <remarks>The hint was given to the door alone (<c>GrantOptions.Refusal</c>), so a program's refused command said the fleet's default
    /// instead, and the model was told three different things about the same key.</remarks>
    [Fact]
    public async Task ACallTheKeyDoesNotOpen_IsRefusedWithTheSameHint_AtEveryLayer()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var (_, reader) = server.Services.GetRequiredService<ClientKeys>().Mint("viewer", [Scopes.Read]);
        await using var viewer = await ClientAsync(server, reader);
        var steps = new JsonArray(new JsonObject { ["command"] = "layer_add", ["params"] = new JsonObject { ["kind"] = "null" } });

        var direct = await viewer.CallToolAsync("ae_create_comp", new Dictionary<string, object?> { ["name"] = "Shot" });
        var forwarded = await viewer.CallToolAsync("run_tools", new Dictionary<string, object?> { ["steps"] = steps });
        var programmed = await viewer.CallToolAsync("run_program", new Dictionary<string, object?> { ["code"] = """ae.layer_add({ kind: "null" });""" });

        Assert.Equal(GrantOptions.NotGranted, direct.Type());
        Assert.Equal(GrantOptions.NotGranted, forwarded.Type());
        Assert.Equal(GrantOptions.NotGranted, programmed.Type());
        Assert.Equal(Messages.ScopeHint, direct.Hint());
        Assert.Equal(Messages.ScopeHint, forwarded.Hint());
        Assert.Equal(Messages.ScopeHint, programmed.Hint());
        Assert.Empty(server.Bridge.Sent);
    }

    /// <summary>A key that does not open a tool's work keeps the tool out of the list its holder is shown and out of find_tool's answers, while
    /// finding tools and loading a skill, which change only the holder's own list, stay open to a key that only reads.</summary>
    [Fact]
    public async Task ATool_TheKeyDoesNotOpen_IsLeftOutOfTheListAndOfFindTool()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var (_, reader) = server.Services.GetRequiredService<ClientKeys>().Mint("viewer", [Scopes.Read]);
        await using var viewer = await ClientAsync(server, reader);

        var listed = (await viewer.ListToolsAsync()).Select(tool => tool.Name).ToList();
        var found = await viewer.CallToolAsync("find_tool", new Dictionary<string, object?> { ["task"] = "create a composition" });
        var loaded = await viewer.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "text" });

        Assert.Contains("ae_list_layers", listed);
        Assert.Contains("find_tool", listed);
        Assert.DoesNotContain("ae_create_comp", listed);
        Assert.True(found.IsOk(), found.Text());
        Assert.DoesNotContain("ae_create_comp", found.Text(), StringComparison.Ordinal);
        Assert.True(loaded.IsOk(), loaded.Text());
    }

    /// <summary>A client of the 2026-07-28 revision hears that a skill's tools arrived through the subscription it asked for, there being no
    /// session to carry the notice, and its next list holds them.</summary>
    [Fact]
    public async Task ASkillLoadedByASessionlessClient_IsToldThroughItsSubscription()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        await using var client = await ClientAsync(server, Token, Sessionless);
        var changed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var handler = client.RegisterNotificationHandler(NotificationMethods.ToolListChangedNotification, (_, _) =>
        {
            changed.TrySetResult();

            return ValueTask.CompletedTask;
        });
        await using var listening = await ListenAsync(client, new JsonObject { ["toolsListChanged"] = true });

        var enabled = await client.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "text" });
        await changed.Task.WaitAsync(Patience);
        var listed = (await client.ListToolsAsync()).Select(tool => tool.Name);

        Assert.Equal(Sessionless, client.NegotiatedProtocolVersion);
        Assert.True(enabled.IsOk(), enabled.Text());
        Assert.Contains("ae_text_style", listed);
    }

    /// <summary>A client of the 2026-07-28 revision that subscribed to a job hears when it ends, and the report it reads then is the final one.</summary>
    [Fact]
    public async Task AJobWatchedByASessionlessClient_IsToldWhenItEnds()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        await using var client = await ClientAsync(server, Token, Sessionless);
        var release = new TaskCompletionSource();
        var job = server.Services.GetRequiredService<Jobs>().Start("probe", async (_, token) =>
        {
            await release.Task.WaitAsync(token);

            return Answer.Ok(new JsonObject { ["rendered"] = true });
        }, ToolAnswer.ObjectInfo);
        var told = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        await using var handler = client.RegisterNotificationHandler(NotificationMethods.ResourceUpdatedNotification, (notification, _) =>
        {
            if (notification.Params?["uri"]?.GetValue<string>() is { } uri)
            {
                told.TrySetResult(uri);
            }

            return ValueTask.CompletedTask;
        });
        await using var listening = await ListenAsync(client, new JsonObject { ["resourceSubscriptions"] = new JsonArray(job.Uri) });

        await Task.Delay(500);
        release.SetResult();
        var uri = await told.Task.WaitAsync(Patience);
        var read = await client.ReadResourceAsync(uri);
        var report = JobReport.FromJson(Assert.IsType<TextResourceContents>(Assert.Single(read.Contents)).Text);

        Assert.Equal(job.Uri, uri);
        Assert.Equal(JobState.Succeeded, report!.State);
    }

    /// <summary>Two clients of the 2026-07-28 revision, each with a key of its own, keep a list each: the skill one loads is not in the other's.</summary>
    /// <remarks>Snail.Sdk keeps the skills per MCP session, and a client without one is named by its key (<c>SkillOptions.SessionOf</c>).</remarks>
    [Fact]
    public async Task TwoSessionlessClients_WithKeysOfTheirOwn_KeepAListEach()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var keys = server.Services.GetRequiredService<ClientKeys>();
        var (_, titlesKey) = keys.Mint("titles", Scopes.All);
        var (_, editKey) = keys.Mint("edit", Scopes.All);
        await using var titles = await ClientAsync(server, titlesKey, Sessionless);
        await using var edit = await ClientAsync(server, editKey, Sessionless);

        await titles.CallToolAsync("ae_enable_skill", new Dictionary<string, object?> { ["name"] = "shapes" });
        var edited = (await edit.ListToolsAsync()).Select(tool => tool.Name);

        Assert.DoesNotContain("ae_add_shape", edited);
    }

    private static Task<McpClient> ClientAsync(HttpServerInProcess server, string key, string? protocolVersion = null) =>
        McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri(server.Address, HttpServerRegistrationExtensions.McpPath),
                AdditionalHeaders = new Dictionary<string, string> { ["Authorization"] = $"Bearer {key}" },
            }),
            new McpClientOptions { ProtocolVersion = protocolVersion });

    /// <summary>The listen request a client of the 2026-07-28 revision holds open for the notifications it asks for, ended when disposed.</summary>
    private static async Task<IAsyncDisposable> ListenAsync(McpClient client, JsonObject notifications)
    {
        var acknowledged = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var handler = client.RegisterNotificationHandler(NotificationMethods.SubscriptionsAcknowledgedNotification, (_, _) =>
        {
            acknowledged.TrySetResult();

            return ValueTask.CompletedTask;
        });
        var listening = new CancellationTokenSource();
        var request = new JsonRpcRequest { Id = new RequestId($"listen-{Guid.NewGuid():N}"), Method = RequestMethods.SubscriptionsListen, Params = new JsonObject { ["notifications"] = notifications } };
        var held = client.SendRequestAsync(request, listening.Token);

        await acknowledged.Task.WaitAsync(Patience);

        return new Listening(client, request.Id, handler, listening, held);
    }

    private sealed class Listening(McpClient client, RequestId id, IAsyncDisposable handler, CancellationTokenSource listening, Task held) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            await client.SendNotificationAsync(NotificationMethods.CancelledNotification, new CancelledNotificationParams { RequestId = id }, McpJsonUtilities.DefaultOptions, CancellationToken.None);
            await listening.CancelAsync();

            try
            {
                await held;
            }
            catch (Exception ended) when (ended is OperationCanceledException or McpException)
            {
            }

            listening.Dispose();
            await handler.DisposeAsync();
        }
    }
}
