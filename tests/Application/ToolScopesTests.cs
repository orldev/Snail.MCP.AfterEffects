using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Domain.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>Which kind of work a call is, decided before After Effects is asked anything. The answers come from what the work already
/// declares — the command schema, or a tool's own annotations — so a tool added without a thought about keys is still guarded.</summary>
public sealed class ToolScopesTests
{
    private static readonly ToolScopes Scopes = new(new CommandSemantics(), new ToolIndex(typeof(ToolScopes).Assembly));

    /// <summary>Scripts and menu commands reach past every schema, so they are their own kind of work however harmless the call looks.</summary>
    [Theory]
    [InlineData("script")]
    [InlineData("menu_command")]
    [InlineData("system_call")]
    public void WhatReachesPastTheSchema_IsScriptWork(string command) =>
        Assert.Equal(Domain.Scopes.Script, Scopes.For(command));

    [Theory]
    [InlineData("render_start")]
    [InlineData("farm_submit")]
    [InlineData("farm_retry")]
    [InlineData("lease")]
    [InlineData("stop_worker")]
    public void WhatSpendsTheMachine_IsFarmWork(string command) =>
        Assert.Equal(Domain.Scopes.Farm, Scopes.For(command));

    /// <summary>Everything else is read from the schema: a command the bridge calls read-only is reading, a destructive one is deleting, and
    /// anything the schema does not know is treated as writing rather than waved through.</summary>
    [Theory]
    [InlineData("project_info", Domain.Scopes.Read)]
    [InlineData("layers_list", Domain.Scopes.Read)]
    [InlineData("layer_remove", Domain.Scopes.Delete)]
    [InlineData("layer_add", Domain.Scopes.Write)]
    [InlineData("nothing_the_schema_knows", Domain.Scopes.Write)]
    public void EveryOtherCommand_TakesTheScopeItsSchemaImplies(string command, string scope) =>
        Assert.Equal(scope, Scopes.For(command));

    /// <summary>A carrier is asked about every command inside it before any of them is sent: a sequence half refused is a composition half built.</summary>
    [Fact]
    public void ASequence_IsRefusedAtTheFirstStepTheKeyDoesNotOpen()
    {
        var client = Reading();
        var steps = new JsonArray(
            new JsonObject { ["command"] = BridgeCommands.ProjectInfo.Name },
            new JsonObject { ["without_a_command"] = true },
            new JsonObject { ["command"] = BridgeCommands.LayerAdd.Name },
            new JsonObject { ["command"] = BridgeCommands.Script.Name });

        var refused = Scopes.Refused(steps, client);

        Assert.Equal((BridgeCommands.LayerAdd.Name, Domain.Scopes.Write), refused);
    }

    [Fact]
    public void ASequenceTheKeyOpensWholly_IsNotRefused()
    {
        var steps = new JsonArray(new JsonObject { ["command"] = BridgeCommands.ProjectInfo.Name });

        Assert.Null(Scopes.Refused(steps, Reading()));
        Assert.Null(Scopes.Refused(null, Reading()));
    }

    /// <summary>The refusal names the work, the scope it needs and the key that was presented, because the reader has to ask the operator for
    /// exactly that scope.</summary>
    [Fact]
    public void TheRefusal_NamesTheWorkTheScopeAndTheKey()
    {
        var refusal = ToolScopes.Refusal("ae_script", Domain.Scopes.Script, "titles");

        Assert.Contains("ae_script", refusal, StringComparison.Ordinal);
        Assert.Contains(Domain.Scopes.Script, refusal, StringComparison.Ordinal);
        Assert.Contains("titles", refusal, StringComparison.Ordinal);
    }

    private static ClientSession Reading()
    {
        var clients = new ClientSessions([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);
        var client = clients.Open(new McpServerOptions(), "titles");
        client.Opens([Domain.Scopes.Read]);

        return client;
    }
}
