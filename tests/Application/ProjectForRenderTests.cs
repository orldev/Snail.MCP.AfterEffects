using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>What a render job is made of before aerender is started: a project file on disk and a composition's name. aerender reads the file
/// and not the application's memory, so every answer here is about the difference between the two.</summary>
public sealed class ProjectForRenderTests
{
    private readonly string _root = TestFolder.Create();

    [Fact]
    public async Task AProjectThatWasGiven_IsTakenAsItIs_WithoutAskingAfterEffects()
    {
        var project = Path.Combine(_root, "scene.aep");
        await File.WriteAllTextAsync(project, "not really a project");
        var bridge = new StubBridge((_, _) => BridgeReply.Ok(new JsonObject()));

        var resolved = await Resolver(bridge).ResolveAsync(project, "Main", save: true, CancellationToken.None);

        Assert.True(resolved.IsResolved);
        Assert.Equal(project, resolved.Project);
        Assert.Equal("Main", resolved.Comp);
        Assert.False(resolved.WasSaved);
        Assert.Empty(bridge.Sent);
    }

    [Fact]
    public async Task AProjectThatIsNotThere_IsRefusedWithThePathItLookedAt()
    {
        var resolved = await Resolver(Connected()).ResolveAsync(Path.Combine(_root, "gone.aep"), "Main", save: true, CancellationToken.None);

        Assert.False(resolved.IsResolved);
        Assert.Contains("gone.aep", resolved.Refusal!, StringComparison.Ordinal);
    }

    /// <summary>A composition given by its id has to be looked up, and without After Effects there is nothing to look it up in.</summary>
    [Fact]
    public async Task ACompositionByIdWithoutAfterEffects_IsRefused()
    {
        var resolved = await Resolver(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject())) { IsConnected = false })
            .ResolveAsync(Path.Combine(_root, "scene.aep"), "17", save: true, CancellationToken.None);

        Assert.Contains("pass its name", resolved.Refusal!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACompositionById_IsResolvedToItsName()
    {
        var project = Path.Combine(_root, "scene.aep");
        await File.WriteAllTextAsync(project, string.Empty);
        var bridge = Answering(new() { [BridgeCommands.ItemInfo.Name] = BridgeReply.Ok(new JsonObject { ["name"] = "Titles" }) });

        var resolved = await Resolver(bridge).ResolveAsync(project, "17", save: false, CancellationToken.None);

        Assert.Equal("Titles", resolved.Comp);
        Assert.Equal(17, bridge.Sent[0].Parameters["item"]!.GetValue<int>());
    }

    [Fact]
    public async Task AnIdNoItemHas_IsRefused()
    {
        var withoutName = Answering(new() { [BridgeCommands.ItemInfo.Name] = BridgeReply.Ok(new JsonObject()) });
        var failing = Answering(new() { [BridgeCommands.ItemInfo.Name] = BridgeReply.Failed(new BridgeError("NotFound", "no item 17")) });

        Assert.Contains("17", (await Resolver(withoutName).ResolveAsync(null, "17", true, CancellationToken.None)).Refusal!, StringComparison.Ordinal);
        Assert.Equal("no item 17", (await Resolver(failing).ResolveAsync(null, "17", true, CancellationToken.None)).Refusal);
    }

    [Fact]
    public async Task WithoutAProjectAndWithoutAfterEffects_TheAnswerSaysToPassAPath()
    {
        var resolved = await Resolver(new StubBridge((_, _) => BridgeReply.Ok(new JsonObject())) { IsConnected = false })
            .ResolveAsync(null, "Main", save: true, CancellationToken.None);

        Assert.Contains("pass the path of a saved project", resolved.Refusal!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnAfterEffectsThatDoesNotAnswerAboutItsProject_IsRefusedWithWhatItSaid()
    {
        var bridge = Answering(new() { [BridgeCommands.ProjectInfo.Name] = BridgeReply.Failed(new BridgeError("Busy", "a render is running")) });

        var resolved = await Resolver(bridge).ResolveAsync(null, "Main", save: true, CancellationToken.None);

        Assert.Equal("a render is running", resolved.Refusal);
    }

    /// <summary>A project that was never saved has no file for aerender to read, and no amount of saving by this server would give it a name.</summary>
    [Fact]
    public async Task AProjectThatWasNeverSaved_IsRefused()
    {
        var bridge = Answering(new() { [BridgeCommands.ProjectInfo.Name] = BridgeReply.Ok(new JsonObject { ["dirty"] = true }) });

        var resolved = await Resolver(bridge).ResolveAsync(null, "Main", save: true, CancellationToken.None);

        Assert.Contains("ae_project_save", resolved.Refusal!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheOpenProjectWithNothingUnsaved_IsRenderedWhereItAlreadyIs()
    {
        var bridge = Answering(new() { [BridgeCommands.ProjectInfo.Name] = BridgeReply.Ok(new JsonObject { ["file"] = "/work/scene.aep", ["dirty"] = false }) });

        var resolved = await Resolver(bridge).ResolveAsync(null, "Main", save: false, CancellationToken.None);

        Assert.Equal("/work/scene.aep", resolved.Project);
        Assert.False(resolved.WasSaved);
    }

    /// <summary>Unsaved changes are the one thing aerender cannot see, so they are either saved first or the job is refused.</summary>
    [Fact]
    public async Task AProjectWithUnsavedChanges_IsSavedWhenAllowed_AndRefusedWhenNot()
    {
        var dirty = new JsonObject { ["file"] = "/work/scene.aep", ["dirty"] = true };
        var saving = Answering(new()
        {
            [BridgeCommands.ProjectInfo.Name] = BridgeReply.Ok(dirty),
            [BridgeCommands.ProjectSave.Name] = BridgeReply.Ok(new JsonObject()),
        });
        var refusing = Answering(new() { [BridgeCommands.ProjectInfo.Name] = BridgeReply.Ok(dirty.DeepClone().AsObject()) });

        var saved = await Resolver(saving).ResolveAsync(null, "Main", save: true, CancellationToken.None);
        var refused = await Resolver(refusing).ResolveAsync(null, "Main", save: false, CancellationToken.None);

        Assert.True(saved.WasSaved);
        Assert.Contains(BridgeCommands.ProjectSave.Name, saving.Sent.Select(sent => sent.Command.Name));
        Assert.Contains("save true", refused.Refusal!, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AProjectThatCouldNotBeSaved_IsRefusedWithWhatWentWrong()
    {
        var bridge = Answering(new()
        {
            [BridgeCommands.ProjectInfo.Name] = BridgeReply.Ok(new JsonObject { ["file"] = "/work/scene.aep", ["dirty"] = true }),
            [BridgeCommands.ProjectSave.Name] = BridgeReply.Failed(new BridgeError("Forbidden", "the file is read only")),
        });

        var resolved = await Resolver(bridge).ResolveAsync(null, "Main", save: true, CancellationToken.None);

        Assert.Equal("the file is read only", resolved.Refusal);
    }

    /// <summary>Where the render lands: nothing given puts it under the project's renders folder, a folder given is completed with the
    /// composition's name, and the folder it lands in is made before aerender looks for it.</summary>
    [Fact]
    public void TheOutput_IsCompletedWithTheCompositionsName_AndItsFolderIsMade()
    {
        var resolver = Resolver(Connected());
        var folder = Path.Combine(_root, "out");
        Directory.CreateDirectory(folder);

        var byDefault = resolver.OutputFor(null, "Main");
        var intoFolder = resolver.OutputFor(folder, "Main");
        var withSeparator = resolver.OutputFor($"{Path.Combine(_root, "deep")}{Path.DirectorySeparatorChar}", "Main");
        var exact = resolver.OutputFor(Path.Combine(_root, "exact.mov"), "Main");

        Assert.Equal(Path.Combine(_root, "renders", "Main"), byDefault);
        Assert.Equal(Path.Combine(folder, "Main"), intoFolder);
        Assert.Equal(Path.Combine(_root, "deep", "Main"), withSeparator);
        Assert.Equal(Path.Combine(_root, "exact.mov"), exact);
        Assert.True(Directory.Exists(Path.Combine(_root, "renders")));
        Assert.True(Directory.Exists(Path.Combine(_root, "deep")));
    }

    /// <summary>A path the owner wrote with a tilde is theirs to mean their home folder, which aerender does not expand.</summary>
    [Fact]
    public void AnOutputUnderTheHomeFolder_IsExpanded() =>
        Assert.StartsWith(ServerPaths.Home, Resolver(Connected()).OutputFor("~/renders/scene.mov", "Main"), StringComparison.Ordinal);

    private static StubBridge Connected() => new((_, _) => BridgeReply.Ok(new JsonObject()));

    private static StubBridge Answering(Dictionary<string, BridgeReply> replies) =>
        new((command, _) => replies.TryGetValue(command.Name, out var reply) ? reply : BridgeReply.Ok(new JsonObject()));

    private ProjectForRender Resolver(StubBridge bridge)
    {
        var config = new ServerConfig { DataDirectory = _root, ProjectDirectory = _root };
        ServerConfigExtensions.Normalize(config);

        return new ProjectForRender(bridge, config);
    }
}
