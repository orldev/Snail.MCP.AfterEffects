using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Ports;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Tools.Base;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>Which kind of work a call is, decided before After Effects is asked anything. The answers come from what the work already
/// declares — the command schema, or a tool's own annotations — so a tool added without a thought about keys is still guarded.</summary>
public sealed class ToolScopesTests
{
    private static readonly ToolScopes Scopes = new(new CommandSemantics());

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

    /// <summary>The gate Snail.Sdk asks judges a tool of the server as that tool and any other name as the bridge command a program or
    /// run_tools sends, with the scopes of the key the request presented.</summary>
    [Fact]
    public void TheGate_JudgesToolsAsToolsAndOtherNamesAsCommands_ByTheKeyOfTheRequest()
    {
        var gate = Gate(new ClientIdentity("titles", [Domain.Scopes.Read]));

        Assert.Null(gate.Refusal("ae_list_layers", EmptyServices));
        Assert.Contains("titles", gate.Refusal("ae_add_layer", EmptyServices), StringComparison.Ordinal);
        Assert.Null(gate.Refusal(BridgeCommands.ProjectInfo.Name, EmptyServices));
        Assert.Contains(Domain.Scopes.Write, gate.Refusal(BridgeCommands.LayerAdd.Name, EmptyServices), StringComparison.Ordinal);
        Assert.Contains(Domain.Scopes.Script, gate.Refusal(BridgeCommands.Script.Name, EmptyServices), StringComparison.Ordinal);
    }

    /// <summary>Finding a tool and loading a skill change only the caller's list, so a key that only reads may do both.</summary>
    [Fact]
    public void FindingAToolAndLoadingASkill_AreReading()
    {
        var find = McpServerTool.Create(() => "found", new McpServerToolCreateOptions { Name = "find_tool", ReadOnly = false });
        var enable = McpServerTool.Create(() => "loaded", new McpServerToolCreateOptions { Name = "ae_enable_skill", ReadOnly = false });

        Assert.Equal(Domain.Scopes.Read, Scopes.For(find));
        Assert.Equal(Domain.Scopes.Read, Scopes.For(enable));
    }

    /// <summary>A server over stdio, or a request admitted without a key's scopes, is its own client and may do anything.</summary>
    [Fact]
    public void TheGate_OfAServerWithoutKeys_RefusesNothing()
    {
        var gate = Gate(identity: null);

        Assert.Null(gate.Refusal("ae_add_layer", EmptyServices));
        Assert.Null(gate.Refusal(BridgeCommands.Script.Name, EmptyServices));
    }

    private static readonly IServiceProvider EmptyServices = new ServiceCollection().BuildServiceProvider();

    private static ScopeGate Gate(ClientIdentity? identity)
    {
        var tools = new McpServerPrimitiveCollection<McpServerTool>();
        tools.TryAdd(Tool(nameof(LayerTools.ListLayersAsync)));
        tools.TryAdd(Tool(nameof(LayerTools.AddLayerAsync)));

        return new ScopeGate(Scopes, new ClientSessions(null, new FixedIdentity(identity)), Options.Create(new McpServerOptions { ToolCollection = tools }));
    }

    private static McpServerTool Tool(string method) =>
        McpServerTool.Create(typeof(LayerTools).GetMethod(method)!, _ => throw new NotSupportedException(), new McpServerToolCreateOptions());

    private sealed class FixedIdentity(ClientIdentity? identity) : IClientIdentity
    {
        public ClientIdentity? Who() => identity;
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
        var clients = new ClientSessions(null);
        var client = clients.Of("titles");
        client.Opens([Domain.Scopes.Read]);

        return client;
    }
}
