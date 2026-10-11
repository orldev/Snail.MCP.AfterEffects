using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.StaticFiles;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Web.Access;

namespace Snail.MCP.AfterEffects.Web.Files;

/// <summary>The areas' files over HTTP: read a file or a folder as an archive, write a file or unpack a tar into a folder, delete.</summary>
/// <remarks>Only the files area takes writes: captures, renders and job logs are made by the server, and a page or a link only reads or removes
/// them. What is read comes sandboxed and unsniffed: an SVG or an HTML file from the volume must not run script in the pages' origin.</remarks>
internal static class RawEndpoints
{
    private static readonly FileExtensionContentTypeProvider ContentTypes = new();

    public static IEndpointRouteBuilder MapRawFiles(this IEndpointRouteBuilder endpoints)
    {
        var raw = endpoints.MapGroup(FileLinks.RawPrefix);

        raw.MapGet("/{area}/{**path}", ReadAsync).RequireAuthorization(WebAccess.FilesReadPolicy);
        raw.MapPut("/{area}/{**path}", WriteAsync).RequireAuthorization(WebAccess.FilesWritePolicy);
        raw.MapDelete("/{area}/{**path}", Delete).RequireAuthorization(WebAccess.FilesDeletePolicy);

        return endpoints;
    }

    private static Task<IResult> ReadAsync(HttpContext context, Volume volume, string area, string? path, string? format, bool? download, CancellationToken cancellationToken)
    {
        if (Refused(area, path) is { } refusal)
        {
            return Task.FromResult(refusal);
        }

        context.Response.Headers.ContentSecurityPolicy = "sandbox";
        context.Response.Headers.XContentTypeOptions = "nosniff";

        var relative = AreaPaths.Normal(path);
        var lookup = volume.Find(area, relative);

        if (lookup.Item is not { } item)
        {
            return Task.FromResult(Failure(lookup.Error!));
        }

        var name = Name(area, relative);

        if (!item.IsFolder)
        {
            return Task.FromResult(Results.File(
                volume.OpenRead(area, relative),
                ContentTypes.TryGetContentType(name, out var contentType) ? contentType : "application/octet-stream",
                download == true ? name : null,
                enableRangeProcessing: true));
        }

        if (item.IsTruncated)
        {
            return Task.FromResult(Results.Problem(statusCode: StatusCodes.Status413PayloadTooLarge, detail: $"'{relative}' holds more files than one archive carries; download its folders one by one"));
        }

        if (format != FileLinks.ZipFormat)
        {
            return Task.FromResult(Results.Stream(body => volume.WriteTarAsync(area, relative, item, body, cancellationToken), "application/x-tar", $"{name}.tar"));
        }

        AllowSynchronousWrites(context);

        return Task.FromResult(Results.Stream(body => volume.WriteZipAsync(area, relative, item, body, cancellationToken), "application/zip", $"{name}.zip"));
    }

    /// <summary>A body is a whole file, or with offset and last one part of a file sent in several; a tar sent in parts gathers in a staging
    /// file and is unpacked when its last part arrives.</summary>
    private static async Task<IResult> WriteAsync(
        HttpContext context, Volume volume, string area, string? path, string? format, bool? overwrite, long? offset, bool? last, string? sha256, CancellationToken cancellationToken)
    {
        if (Refused(area, path) is { } refusal)
        {
            return refusal;
        }

        if (offset is < 0 || (sha256 is not null && !IsDigest(sha256)))
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "offset is a byte position of 0 or more, and sha256 is 64 hexadecimal characters");
        }

        if (overwrite == true && context.User.Identity?.AuthenticationType != WebAccess.LinkScheme && !context.User.HasClaim(WebAccess.ScopeClaim, Scopes.Delete))
        {
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: "replacing a file that exists is delete work, and this key does not open it");
        }

        if (area != VolumeAreas.Files)
        {
            return Results.Problem(statusCode: StatusCodes.Status403Forbidden, detail: $"only the {VolumeAreas.Files} area takes uploads");
        }

        if (context.Features.Get<IHttpMaxRequestBodySizeFeature>() is { IsReadOnly: false } limit)
        {
            limit.MaxRequestBodySize = null;
        }

        var part = offset is null && last is null ? null : new UploadPart(offset ?? 0, last ?? true, sha256?.ToLowerInvariant());
        var relative = AreaPaths.Normal(path);

        if (format == FileLinks.TarFormat && part is null)
        {
            return Unpacked(await volume.UnpackTarAsync(area, relative, context.Request.Body, overwrite == true, cancellationToken));
        }

        if (format == FileLinks.TarFormat)
        {
            var staged = Volume.StagingPath(relative, context.User.Identity?.Name);
            var gathered = await volume.WritePartAsync(area, staged, context.Request.Body, part!, overwrite: true, cancellationToken);

            if (!gathered.IsOk)
            {
                return Failure(gathered.Error!);
            }

            return part!.IsLast
                ? Unpacked(await volume.UnpackStagedTarAsync(area, relative, staged, overwrite == true, cancellationToken))
                : Results.Ok(new { path = relative, received = gathered.Received });
        }

        if (relative.Length == 0)
        {
            return Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: "a file needs a path inside the area; a tar with format=tar unpacks into a folder");
        }

        var written = part is null
            ? await volume.WriteAsync(area, relative, context.Request.Body, overwrite == true, cancellationToken)
            : await volume.WritePartAsync(area, relative, context.Request.Body, part, overwrite == true, cancellationToken);

        return written.IsOk ? Written(relative, written) : Failure(written.Error!);
    }

    private static IResult Delete(Volume volume, string area, string? path)
    {
        if (Refused(area, path) is { } refusal)
        {
            return refusal;
        }

        var deleted = volume.Delete(area, AreaPaths.Normal(path));

        return deleted.IsOk ? Results.Ok(new { path = AreaPaths.Normal(path), bytes = deleted.Bytes }) : Failure(deleted.Error!);
    }

    private static void AllowSynchronousWrites(HttpContext context)
    {
        if (context.Features.Get<IHttpBodyControlFeature>() is { } body)
        {
            body.AllowSynchronousIO = true;
        }
    }

    private static IResult Written(string relative, VolumeWrite written) =>
        written.Files.Count == 0
            ? Results.Ok(new { path = relative, received = written.Received })
            : Results.Ok(new { path = written.Files[0].To, size = written.Files[0].Size, sha256 = written.Files[0].Sha256 });

    private static IResult Unpacked(VolumeWrite unpacked) =>
        unpacked.Error is { } error
            ? Failure(error)
            : Results.Ok(new { files = unpacked.Files.Select(file => new { path = file.To, size = file.Size, sha256 = file.Sha256 }), bytes = unpacked.Files.Sum(file => file.Size) });

    private static bool IsDigest(string value) => value.Length == 64 && value.All(char.IsAsciiHexDigit);

    private static IResult? Refused(string area, string? path)
    {
        if (!VolumeAreas.All.Contains(area))
        {
            return Results.Problem(statusCode: StatusCodes.Status404NotFound, detail: $"no area '{area}'; the areas are {string.Join(", ", VolumeAreas.All)}");
        }

        var relative = AreaPaths.Normal(path);

        return !AreaPaths.IsConfined(relative) ? Results.Problem(statusCode: StatusCodes.Status400BadRequest, detail: $"'{relative}' must stay inside the {area} area") : null;
    }

    private static string Name(string area, string relative) => relative.Length == 0 ? area : relative.Split('/')[^1];

    private static IResult Failure(BridgeError error) => Results.Problem(
        statusCode: error.Type switch
        {
            "NotFound" => StatusCodes.Status404NotFound,
            "BadRequest" => StatusCodes.Status400BadRequest,
            "InUse" or "Exists" => StatusCodes.Status409Conflict,
            "Corrupt" => StatusCodes.Status422UnprocessableEntity,
            _ => StatusCodes.Status500InternalServerError,
        },
        title: error.Type,
        detail: error.Message);
}
