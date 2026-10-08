using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Pictures;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Base;

/// <summary>Looking at the work: a frame of a composition as a picture the model can see.</summary>
[McpServerToolType]
internal sealed class CaptureTools(IAfterEffectsBridge bridge, Captures captures) : BridgeToolBase(bridge)
{
    [McpServerTool(Name = "ae_capture_frame", ReadOnly = false, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Capture.Frame)]
    [Sends("capture_frame")]
    public async Task<CallToolResult> CaptureFrameAsync(
        [Description(ToolDescriptions.Parameters.Comp)] string? comp = null,
        [Description(ToolDescriptions.Parameters.Time)] double? time = null,
        [Description("Width in pixels, scaled down from the composition; 1280 by default.")] int width = 1280,
        [Description("Where to write the PNG on After Effects' machine; the captures folder otherwise.")] string? path = null,
        [Description("Keep the background transparent rather than the composition's colour.")] bool transparent = false,
        [Description(ToolDescriptions.Parameters.TimeoutSeconds)] int timeoutSeconds = 60,
        CancellationToken cancellationToken = default) =>
        await WithinAsync(timeoutSeconds, async timeout =>
        {
            var target = path ?? captures.PathFor(comp, time);
            var reply = await ExchangeAsync(BridgeCommands.CaptureFrame,
                new JsonObject().WithReference("comp", comp).With("time", time).With("path", target).With("width", width).With("transparent", transparent), timeout, cancellationToken).ConfigureAwait(false);

            if (!reply.IsOk || reply.Result?["path"]?.ToString() is not { } written)
            {
                return ToolResponse.From(reply);
            }

            var picture = await Captures.ReadWhenCompleteAsync(target, timeout, cancellationToken).ConfigureAwait(false);

            if (picture is null)
            {
                return ToolResponse.Failure(Messages.CaptureNotWritten(written), Messages.CaptureFailedHint);
            }

            var data = reply.Result.AsObject();
            data.Remove("pending");
            data["bytes"] = picture.Length;

            return ToolResponse.Pictured(data, picture, "image/png");
        }).ConfigureAwait(false);
}
