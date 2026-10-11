using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Diagnostics;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Tools.Prompts;
using Snail.MCP.AfterEffects.Web.Access;

namespace Snail.MCP.AfterEffects.Web.Pages;

/// <summary>The pages of the server beside After Effects: signing in with a key, the status, and the areas to look through and clean.</summary>
/// <remarks>Every action is a form that posts and returns to the page it came from, so the pages need no script to work; the script only asks
/// before a deletion and shows times in the reader's zone. Forms carry an antiforgery token, and the session cookie is SameSite=Strict.</remarks>
internal static class PageEndpoints
{
    private const string LoginPath = "/login";

    public static IEndpointRouteBuilder MapPages(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(LoginPath, ShowLoginAsync).AllowAnonymous();
        endpoints.MapPost(LoginPath, SignInAsync).AllowAnonymous();
        endpoints.MapPost("/logout", (Delegate)SignOutAsync).RequireAuthorization(WebAccess.BrowserPolicy);
        endpoints.MapGet("/assets/{name}", Asset).AllowAnonymous();

        endpoints.MapGet("/", StatusAsync).RequireAuthorization(WebAccess.BrowserPolicy);

        foreach (var area in VolumeAreas.All)
        {
            endpoints.MapGet($"/{area}/{{**path}}", (Volume volume, string? path, int? deleted, long? freed, string? cancelled, string? problem, CancellationToken cancellationToken) =>
                    BrowseAsync(volume, area, path, PageViews.FlashOf(deleted, freed, cancelled, problem), cancellationToken))
                .RequireAuthorization(WebAccess.BrowserPolicy);
        }

        endpoints.MapPost("/delete", Delete).RequireAuthorization(WebAccess.BrowserDeletePolicy);
        endpoints.MapPost("/prune", Prune).RequireAuthorization(WebAccess.BrowserDeletePolicy);
        endpoints.MapPost("/cancel", Cancel).RequireAuthorization(WebAccess.BrowserFarmPolicy);

        return endpoints;
    }

    /// <summary>The sign-in page, or the page the browser came for when the session it already has is still good.</summary>
    /// <remarks>The session is read from its own cookie scheme rather than from <c>context.User</c>: the default scheme of this server is the
    /// bearer one a client presents, so on an anonymous endpoint an operator holding a session cookie and no token looks like a stranger and
    /// was shown the form again.</remarks>
    private static async Task<IResult> ShowLoginAsync(HttpContext context, string? returnUrl) =>
        (await context.AuthenticateAsync(WebAccess.CookieScheme)).Succeeded
            ? Results.LocalRedirect(Local(returnUrl))
            : Page<LoginPage>(new { ReturnUrl = Local(returnUrl) });

    /// <summary>Signs a browser in with any client's key, and remembers whose it was: the session is that client's, and ends when its key does.</summary>
    private static async Task<IResult> SignInAsync(HttpContext context, ClientKeys keys, [FromForm(Name = "token")] string? presented, [FromForm] string? returnUrl)
    {
        if (keys.Holder(presented) is not { } client)
        {
            return Page<LoginPage>(new { ReturnUrl = Local(returnUrl), IsRefused = true }, StatusCodes.Status401Unauthorized);
        }

        var identity = new ClaimsIdentity([new Claim(ClaimTypes.Name, client.Name), .. client.Scopes.Select(scope => new Claim(WebAccess.ScopeClaim, scope))], WebAccess.CookieScheme);
        await context.SignInAsync(WebAccess.CookieScheme, new ClaimsPrincipal(identity));

        return Results.LocalRedirect(Local(returnUrl));
    }

    private static async Task<IResult> SignOutAsync(HttpContext context)
    {
        await context.SignOutAsync(WebAccess.CookieScheme);

        return Results.LocalRedirect(LoginPath);
    }

    private static async Task<IResult> StatusAsync(ServerHealth health, Volume volume, RenderFarm farm, ServerConfig config, TimeProvider time, CancellationToken cancellationToken)
    {
        var report = await health.ReportAsync(cancellationToken);
        var running = time.GetUtcNow() - new DateTimeOffset(Process.GetCurrentProcess().StartTime.ToUniversalTime());

        return Page<StatusPage>(new { Groups = PageViews.Status(report, volume.Measure(), farm.Jobs, farm, config, running) });
    }

    private static async Task<IResult> BrowseAsync(Volume volume, string area, string? path, Flash? flash, CancellationToken cancellationToken)
    {
        var relative = AreaPaths.Normal(path);

        if (!AreaPaths.IsConfined(relative))
        {
            return Page<BrowsePage>(new { View = PageViews.Unavailable(area, string.Empty, new BridgeError("BadRequest", Messages.FolderOutsideTheArea), flash) });
        }

        var listing = volume.List(area, relative);

        if (listing.Error is null)
        {
            return Page<BrowsePage>(new { View = PageViews.Browse(area, relative, listing, flash) });
        }

        if (relative.Length > 0 && volume.Find(area, relative) is { Item: { IsFolder: false } file })
        {
            var text = PageViews.IsText(relative) && file.Bytes <= PageViews.LargestTextPreview ? await ReadTextAsync(volume, area, relative, cancellationToken) : null;

            return Page<FilePage>(new { View = PageViews.File(area, relative, file.Bytes, text), Flash = flash });
        }

        return Page<BrowsePage>(new { View = PageViews.Unavailable(area, relative, listing.Error, flash) });
    }

    /// <summary>The form is read whole: a list of ticked names does not bind to a string array on this framework.</summary>
    private static IResult Delete(Volume volume, IFormCollection form)
    {
        var area = form["area"].ToString();

        if (!VolumeAreas.All.Contains(area))
        {
            return Results.NotFound();
        }

        var folder = AreaPaths.Normal(form["path"].ToString());
        string[] chosen = form.TryGetValue("only", out var only) ? [only.ToString()] : [.. form["names"].OfType<string>()];

        if (!AreaPaths.IsConfined(folder))
        {
            return Back(area, string.Empty, problem: Messages.FolderOutsideTheArea);
        }

        if (chosen.Length == 0)
        {
            return Back(area, folder, problem: Messages.NothingSelected);
        }

        if (chosen.Any(name => name.Trim().Length == 0 || name.Contains('/') || name.Contains('\\') || !AreaPaths.IsConfined(name)))
        {
            return Back(area, folder, problem: Messages.NotOneName);
        }

        var (count, bytes, problem) = DeleteEach(volume, area, [.. chosen.Select(name => PageViews.Join(folder, name))]);

        return Back(area, folder, count, bytes, problem);
    }

    private static IResult Prune(RenderFarm farm, [FromForm] string area)
    {
        if (area != VolumeAreas.Jobs)
        {
            return Results.NotFound();
        }

        var bytes = farm.Jobs.Where(job => !job.IsActive && File.Exists(job.LogPath)).Sum(job => new FileInfo(job.LogPath).Length);
        var forgotten = farm.Cleanup(TimeSpan.Zero);

        return Back(area, string.Empty, forgotten, bytes);
    }

    private static IResult Cancel(RenderFarm farm, [FromForm] string id) =>
        farm.TryCancel(id)
            ? Results.LocalRedirect($"/{VolumeAreas.Jobs}?cancelled={Uri.EscapeDataString(id)}")
            : Back(VolumeAreas.Jobs, string.Empty, problem: Messages.UnknownJob(id));

    private static IResult Asset(string name)
    {
        var stream = typeof(PageEndpoints).Assembly.GetManifestResourceStream($"assets/{name}");

        return stream is null
            ? Results.NotFound()
            : Results.Stream(stream, name.EndsWith(".css", StringComparison.Ordinal) ? "text/css; charset=utf-8" : "text/javascript; charset=utf-8");
    }

    /// <summary>Deletes one path after another and stops at the first refusal, so a page says exactly what did not go.</summary>
    private static (int Count, long Bytes, string? Problem) DeleteEach(Volume volume, string area, IReadOnlyList<string> paths)
    {
        var count = 0;
        long bytes = 0;

        foreach (var path in paths)
        {
            var deleted = volume.Delete(area, path);

            if (!deleted.IsOk)
            {
                return (count, bytes, deleted.Error!.Message);
            }

            count++;
            bytes += deleted.Bytes;
        }

        return (count, bytes, null);
    }

    private static async Task<string> ReadTextAsync(Volume volume, string area, string path, CancellationToken cancellationToken)
    {
        await using var content = volume.OpenRead(area, path);
        using var reader = new StreamReader(content, Encoding.UTF8);

        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static RedirectHttpResult Back(string area, string folder, int? deleted = null, long? freed = null, string? problem = null)
    {
        var query = new List<string>();

        if (deleted is { } count)
        {
            query.Add($"deleted={count}&freed={freed ?? 0}");
        }

        if (problem is not null)
        {
            query.Add($"problem={Uri.EscapeDataString(problem)}");
        }

        return TypedResults.LocalRedirect($"{PageViews.Page(area, folder)}{(query.Count == 0 ? string.Empty : $"?{string.Join('&', query)}")}");
    }

    /// <summary>A return address only when it stays on this site: a path, never another host.</summary>
    private static string Local(string? returnUrl) =>
        returnUrl is { Length: > 0 } && returnUrl[0] == '/' && !returnUrl.StartsWith("//", StringComparison.Ordinal) && !returnUrl.StartsWith("/\\", StringComparison.Ordinal) ? returnUrl : "/";

    private static IResult Page<TPage>(object parameters, int status = StatusCodes.Status200OK) where TPage : IComponent =>
        new RazorComponentResult<TPage>(parameters) { StatusCode = status };
}
