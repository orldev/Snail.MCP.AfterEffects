using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.Sdk.Mcp.Hosting;
using Snail.Sdk.Mcp.Hosting.Discovery;
using Snail.Sdk.Mcp.Hosting.Sessions;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Application.Transfer;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tools.Transfer;

/// <summary>The files this server keeps and how a client moves them in and out: the transfer skill.</summary>
[Skill(Skills.Transfer, SkillDescriptions.Transfer)]
[McpServerToolType]
internal sealed class TransferTools(Volume volume, FileLinks links, ClientCommands commands, ServerConfig config, ClientSessions clients)
{
    [McpServerTool(Name = "ae_files", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Transfer.Files)]
    public CallToolResult Files(
        [Description(ToolDescriptions.Parameters.Area)] string area = VolumeAreas.Files,
        [Description(ToolDescriptions.Parameters.RemotePath)] string? path = null,
        [Description("Add a signed download link per entry (HTTP only).")] bool withLinks = false)
    {
        if (!VolumeAreas.All.Contains(area))
        {
            return ToolAnswer.Failed(Messages.NoSuchArea(area), type: ToolFailures.Arguments);
        }

        var relative = AreaPaths.Normal(path);

        if (!AreaPaths.IsConfined(relative))
        {
            return ToolAnswer.Failed(Messages.PathOutsideArea(relative, area), type: Reached.Outside);
        }

        var listing = volume.List(area, relative);

        if (listing.Error is { } error)
        {
            return ToolAnswer.Failed(error.Message, type: error.Type);
        }

        var entries = listing.Entries.Select(entry =>
        {
            var described = new JsonObject
            {
                ["name"] = entry.Name,
                ["kind"] = entry.IsFolder ? "folder" : "file",
                ["bytes"] = entry.Bytes,
                ["files"] = entry.Files,
                ["modified"] = entry.Modified.ToString("O"),
                ["state"] = entry.State,
                ["detail"] = entry.Detail,
            };

            if (withLinks && IsHttp)
            {
                var child = relative.Length == 0 ? entry.Name : $"{relative}/{entry.Name}";
                described["download"] = links.For("GET", area, child, entry.IsFolder ? FileLinks.TarFormat : null).Url;
            }

            return (JsonNode)described;
        });

        return ToolAnswer.Ok(new JsonObject
        {
            ["area"] = area,
            ["path"] = relative,
            ["directory"] = listing.Directory,
            ["entries"] = new JsonArray([.. entries]),
            ["truncated"] = listing.IsTruncated,
        });
    }

    [McpServerTool(Name = "ae_upload", ReadOnly = false, Destructive = false, Idempotent = false, OpenWorld = true)]
    [Description(ToolDescriptions.Transfer.Upload)]
    public CallToolResult Upload(
        [Description("File or folder on the client's machine.")] string localPath,
        [Description("Where it lands in the files area; its own name when empty.")] string? remotePath = null,
        [Description("Replace files already there (delete work).")] bool overwrite = false)
    {
        var target = AreaPaths.Normal(remotePath ?? Path.GetFileName(localPath.TrimEnd('/', '\\')));

        if (target.Length == 0 || !AreaPaths.IsConfined(target))
        {
            return ToolAnswer.Failed(Messages.PathOutsideArea(target, VolumeAreas.Files), type: Reached.Outside);
        }

        if (overwrite && !clients.Current.May(Scopes.Delete))
        {
            return ToolAnswer.Failed(ToolScopes.Refusal("ae_upload with overwrite", Scopes.Delete, clients.Current.Name), Messages.ScopeHint, GrantOptions.NotGranted);
        }

        if (!IsHttp)
        {
            return ToolAnswer.Ok(new JsonObject
            {
                ["sameMachine"] = true,
                ["path"] = Path.Combine(volume.RootOf(VolumeAreas.Files), target.Replace('/', Path.DirectorySeparatorChar)),
                ["note"] = Messages.SameMachineUpload,
            });
        }

        var command = commands.Upload(localPath, target, overwrite);

        return ToolAnswer.Ok(new JsonObject
        {
            ["run"] = command.Command,
            ["expires"] = command.Expires.ToString("O"),
            ["lands"] = Path.Combine(volume.RootOf(VolumeAreas.Files), target.Replace('/', Path.DirectorySeparatorChar)),
            ["note"] = Messages.RunOnTheClient,
        });
    }

    [McpServerTool(Name = "ae_download", ReadOnly = true, Destructive = false, Idempotent = true, OpenWorld = true)]
    [Description(ToolDescriptions.Transfer.Download)]
    public CallToolResult Download(
        [Description(ToolDescriptions.Parameters.RemotePath)] string remotePath,
        [Description(ToolDescriptions.Parameters.Area)] string area = VolumeAreas.Files,
        [Description("Where it lands on the client's machine; its own name when empty.")] string? localPath = null,
        [Description("Replace files already on the client's machine.")] bool overwrite = false)
    {
        if (!VolumeAreas.All.Contains(area))
        {
            return ToolAnswer.Failed(Messages.NoSuchArea(area), type: ToolFailures.Arguments);
        }

        var relative = AreaPaths.Normal(remotePath);

        if (!AreaPaths.IsConfined(relative))
        {
            return ToolAnswer.Failed(Messages.PathOutsideArea(relative, area), type: Reached.Outside);
        }

        var lookup = volume.Find(area, relative);

        if (lookup.Item is not { } item)
        {
            return ToolAnswer.Failed(lookup.Error!.Message, type: lookup.Error.Type);
        }

        var name = relative.Length == 0 ? area : relative.Split('/')[^1];

        if (!IsHttp)
        {
            return ToolAnswer.Ok(new JsonObject
            {
                ["sameMachine"] = true,
                ["path"] = Path.Combine(volume.RootOf(area), relative.Replace('/', Path.DirectorySeparatorChar)),
                ["isFolder"] = item.IsFolder,
                ["bytes"] = item.Bytes,
                ["note"] = Messages.SameMachineDownload,
            });
        }

        var command = commands.Download(area, relative, localPath ?? name, item.IsFolder, overwrite);

        return ToolAnswer.Ok(new JsonObject
        {
            ["run"] = command.Command,
            ["expires"] = command.Expires.ToString("O"),
            ["isFolder"] = item.IsFolder,
            ["bytes"] = item.Bytes,
            ["files"] = item.Files.Count,
            ["note"] = Messages.RunOnTheClient,
        });
    }

    private bool IsHttp => config.Transport is McpTransport.Http;
}
