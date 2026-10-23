using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Storage;

namespace Snail.MCP.AfterEffects.Tests.Web;

/// <summary>The server over HTTP, built and driven inside this test: who is let in, what the pages show and what the raw
/// endpoints do with the areas' files.</summary>
public sealed partial class HttpServerTests
{
    private const string Token = "in-process-operator-token";

    /// <summary>A browser with no session is sent to the sign-in page, a wrong key is refused there, and the right one opens
    /// the status page and the areas behind it.</summary>
    [Fact]
    public async Task Pages_TurnAwayAStranger_AndServeTheOperatorWhoSignsIn()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);

        var greeting = await browser.GetAsync("/");

        Assert.Equal(HttpStatusCode.Redirect, greeting.StatusCode);
        Assert.Contains("/login", greeting.Headers.Location!.ToString(), StringComparison.Ordinal);
        var form = await browser.GetStringAsync("/login");

        Assert.Contains("name=\"token\"", form, StringComparison.Ordinal);

        var refused = await browser.PostAsync("/login", Form(("token", "not the key"), Antiforgery(form)));

        Assert.Equal(HttpStatusCode.Unauthorized, refused.StatusCode);
        Assert.Contains("is-error", await refused.Content.ReadAsStringAsync(), StringComparison.Ordinal);

        var admitted = await browser.PostAsync("/login", Form(("token", Token), Antiforgery(await browser.GetStringAsync("/login"))));

        Assert.Equal(HttpStatusCode.Redirect, admitted.StatusCode);

        var status = await browser.GetStringAsync("/");

        Assert.Contains("After Effects", status, StringComparison.Ordinal);

        foreach (var area in VolumeAreas.All)
        {
            var page = await browser.GetAsync($"/{area}");

            Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        }
    }

    /// <summary>The stylesheet and the script the pages ask for are served to anyone, since a browser fetches them before it
    /// has a session.</summary>
    [Fact]
    public async Task Assets_AreServed_WithoutASession()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client();

        foreach (var name in new[] { "site.css", "pages.css", "pages.js" })
        {
            var asset = await browser.GetAsync($"/assets/{name}");

            Assert.Equal(HttpStatusCode.OK, asset.StatusCode);
            Assert.NotEmpty(await asset.Content.ReadAsByteArrayAsync());
        }

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/assets/nothing.css")).StatusCode);
    }

    /// <summary>A key opens the raw endpoints: a file written into the files area comes back, is listed by the page, and is
    /// gone once deleted. The areas the server fills are read-only.</summary>
    [Fact]
    public async Task RawFiles_TakeAWrite_ThenReadAndDeleteIt()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = server.Client();
        client.DefaultRequestHeaders.Authorization = new("Bearer", Token);

        var written = await client.PutAsync($"/raw/{VolumeAreas.Files}/notes/hello.txt", new StringContent("hello"));

        Assert.Equal(HttpStatusCode.OK, written.StatusCode);
        Assert.Equal("hello", await client.GetStringAsync($"/raw/{VolumeAreas.Files}/notes/hello.txt"));

        var missing = await client.GetAsync($"/raw/{VolumeAreas.Files}/notes/nothing.txt");

        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);

        var refused = await client.PutAsync($"/raw/{VolumeAreas.Captures}/hello.txt", new StringContent("hello"));

        Assert.True(refused.StatusCode is HttpStatusCode.Forbidden or HttpStatusCode.BadRequest or HttpStatusCode.NotFound);

        var removed = await client.DeleteAsync($"/raw/{VolumeAreas.Files}/notes/hello.txt");

        Assert.Equal(HttpStatusCode.OK, removed.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/raw/{VolumeAreas.Files}/notes/hello.txt")).StatusCode);
    }

    /// <summary>Health answers without a key, since a load balancer has none, and says that After Effects is up, which here
    /// is the bridge of this test.</summary>
    [Fact]
    public async Task Health_AnswersWithoutAKey()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = server.Client();

        var health = await client.GetFromJsonAsync<JsonObject>("/healthz");

        Assert.Equal("ok", health!["status"]!.ToString());
        Assert.Equal("up", health["afterEffects"]!.ToString());
    }

    /// <summary>Without a key nothing but the sign-in page and the assets answers.</summary>
    [Fact]
    public async Task RawFiles_WithoutAKey_AreRefused()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = server.Client(followRedirects: false);

        foreach (var path in new[] { $"/raw/{VolumeAreas.Files}/", $"/raw/{VolumeAreas.Captures}/" })
        {
            var answer = await client.GetAsync(path);

            Assert.True(answer.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden or HttpStatusCode.Redirect);
        }
    }


    /// <summary>What the pages do, not only what they show: a file is opened on its own page, deleted from the browse page
    /// with the flash note that follows, the job logs are pruned, and a job nobody submitted cannot be cancelled.</summary>
    [Fact]
    public async Task Pages_Delete_Prune_AndRefuseToCancelAJobThatIsNotThere()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);

        var volume = server.Services.GetRequiredService<Volume>();
        Directory.CreateDirectory(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes"));
        await File.WriteAllTextAsync(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes", "hello.txt"), "hello");

        var file = await browser.GetAsync($"/{VolumeAreas.Files}/notes/hello.txt");

        Assert.Equal(HttpStatusCode.OK, file.StatusCode);

        var browse = await browser.GetStringAsync($"/{VolumeAreas.Files}/notes");
        var deleted = await browser.PostAsync("/delete", Form(("area", VolumeAreas.Files), ("path", "notes"), ("only", "hello.txt"), Antiforgery(browse)));

        Assert.Equal(HttpStatusCode.Redirect, deleted.StatusCode);
        Assert.False(File.Exists(Path.Combine(volume.RootOf(VolumeAreas.Files), "notes", "hello.txt")));

        var afterwards = await browser.GetStringAsync(deleted.Headers.Location!.ToString());

        Assert.Contains("Deleted", afterwards, StringComparison.OrdinalIgnoreCase);

        var jobs = await browser.GetStringAsync($"/{VolumeAreas.Jobs}");
        var pruned = await browser.PostAsync("/prune", Form(("area", VolumeAreas.Jobs), Antiforgery(jobs)));

        Assert.Equal(HttpStatusCode.Redirect, pruned.StatusCode);

        var cancelled = await browser.PostAsync("/cancel", Form(("id", "no-such-job"), Antiforgery(jobs)));

        Assert.Equal(HttpStatusCode.Redirect, cancelled.StatusCode);
        Assert.Contains("problem", cancelled.Headers.Location!.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>Deleting from an area that does not exist is not a page at all.</summary>
    [Fact]
    public async Task Deleting_FromSomethingThatIsNotAnArea_IsNotFound()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);

        var page = await browser.GetStringAsync($"/{VolumeAreas.Files}");
        var answer = await browser.PostAsync("/delete", Form(("area", "nowhere"), ("path", ""), Antiforgery(page)));

        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
    }

    /// <summary>Signing out ends the session: the status page sends the browser back to the sign-in page.</summary>
    [Fact]
    public async Task SigningOut_EndsTheSession()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);

        var status = await browser.GetStringAsync("/");
        var out_ = await browser.PostAsync("/logout", Form(Antiforgery(status)));

        Assert.Equal(HttpStatusCode.Redirect, out_.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, (await browser.GetAsync("/")).StatusCode);
    }

    private static async Task SignInAsync(HttpClient browser)
    {
        var form = await browser.GetStringAsync("/login");
        var admitted = await browser.PostAsync("/login", Form(("token", Token), Antiforgery(form)));

        Assert.Equal(HttpStatusCode.Redirect, admitted.StatusCode);
    }

    [GeneratedRegex(@"name=""(?<name>__RequestVerificationToken)""[^>]*value=""(?<value>[^""]+)""")]
    private static partial Regex AntiforgeryField();

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    /// <summary>The antiforgery field of a rendered form, which every form of these pages carries.</summary>
    private static (string Name, string Value) Antiforgery(string page)
    {
        var found = AntiforgeryField().Match(page);

        Assert.True(found.Success, "the page carries no antiforgery field");

        return (found.Groups["name"].Value, found.Groups["value"].Value);
    }
}
