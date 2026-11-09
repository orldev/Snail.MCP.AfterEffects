using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Application.Installation;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>Health of the server and of the link into After Effects, plus the bridge installer.</summary>
[McpServerToolType]
internal sealed class DiagnosticsTools(ServerHealth health, BridgeInstallation installation)
{
    [McpServerTool(Name = "ae_diagnose", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Diagnostics.Diagnose)]
    public async Task<CallToolResult> DiagnoseAsync(CancellationToken cancellationToken = default) =>
        ToolAnswer.Ok(await health.ReportAsync(cancellationToken).ConfigureAwait(false));

    [McpServerTool(Name = "ae_install_bridge", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = false)]
    [Description(ToolDescriptions.Diagnostics.InstallBridge)]
    public async Task<CallToolResult> InstallBridgeAsync(CancellationToken cancellationToken = default)
    {
        var report = await installation.InstallAsync(cancellationToken).ConfigureAwait(false);

        return report["installed"]?.GetValue<bool>() == true
            ? ToolAnswer.Ok(report)
            : ToolAnswer.Failed(report["error"]!.ToString(), report["hint"]?.ToString(), FailureTypes.NoScriptsFolder);
    }
}
