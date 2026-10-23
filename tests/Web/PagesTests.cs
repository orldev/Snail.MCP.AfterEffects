using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Storage;

namespace Snail.MCP.AfterEffects.Tests.Web;

/// <summary>What the operator's pages do with what they are asked for: a file opened on its own page, a path that leaves the area, a deletion
/// that was not chosen properly. Every refusal comes back as the page it was asked from, with a note, because a page that answers with a bare
/// status leaves the operator nowhere.</summary>
public sealed partial class PagesTests
{
    private const string Token = "pages-operator-token";

    /// <summary>A text file is shown on its page, a picture is linked rather than read, and a text file too large to read is linked as well:
    /// the page holds the whole preview in memory, so the size it will read is a decision and not an accident.</summary>
    [Fact]
    public async Task AFilePage_ShowsWhatCanBeRead_AndLinksTheRest()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);
        var files = server.Services.GetRequiredService<Volume>().RootOf(VolumeAreas.Files);
        Directory.CreateDirectory(files);
        await File.WriteAllTextAsync(Path.Combine(files, "notes.txt"), "a line of the project's notes");
        await File.WriteAllTextAsync(Path.Combine(files, "frame.png"), "not really a picture");
        await File.WriteAllTextAsync(Path.Combine(files, "huge.log"), new string('x', 300 * 1024));

        var text = await browser.GetStringAsync($"/{VolumeAreas.Files}/notes.txt");
        var picture = await browser.GetStringAsync($"/{VolumeAreas.Files}/frame.png");
        var huge = await browser.GetStringAsync($"/{VolumeAreas.Files}/huge.log");

        Assert.Contains("a line of the project&#x27;s notes", text, StringComparison.Ordinal);
        Assert.Contains("<img", picture, StringComparison.Ordinal);
        Assert.DoesNotContain("xxxxxxxxxx", huge, StringComparison.Ordinal);
        Assert.Contains("/raw/files/huge.log", huge, StringComparison.Ordinal);
    }

    /// <summary>A folder that is not there is said to be missing on the page of the area, rather than answered with a status a browser shows as
    /// its own error page.</summary>
    [Fact]
    public async Task AFolderThatIsNotThere_IsSaidOnThePage()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);

        var page = await browser.GetAsync($"/{VolumeAreas.Renders}/nothing-of-that-name");

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("is-error", await page.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    /// <summary>A deletion with nothing ticked, or with a name that is a path rather than a name, comes back to the page with a note and
    /// deletes nothing.</summary>
    [Fact]
    public async Task ADeletionThatWasNotChosenProperly_ComesBackWithANote()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);
        var files = server.Services.GetRequiredService<Volume>().RootOf(VolumeAreas.Files);
        Directory.CreateDirectory(Path.Combine(files, "notes"));
        await File.WriteAllTextAsync(Path.Combine(files, "notes", "hello.txt"), "hello");
        var page = await browser.GetStringAsync($"/{VolumeAreas.Files}/notes");

        var nothing = await browser.PostAsync("/delete", Form(("area", VolumeAreas.Files), ("path", "notes"), Antiforgery(page)));
        var aPath = await browser.PostAsync("/delete", Form(("area", VolumeAreas.Files), ("path", "notes"), ("only", "../hello.txt"), Antiforgery(page)));
        var outside = await browser.PostAsync("/delete", Form(("area", VolumeAreas.Files), ("path", "../.."), ("only", "hello.txt"), Antiforgery(page)));

        Assert.All([nothing, aPath, outside], answer => Assert.Equal(HttpStatusCode.Redirect, answer.StatusCode));
        Assert.All([nothing, aPath, outside], answer => Assert.Contains("problem", answer.Headers.Location!.ToString(), StringComparison.Ordinal));
        Assert.True(File.Exists(Path.Combine(files, "notes", "hello.txt")));
    }

    /// <summary>A deletion that names a file nobody wrote stops at it and says so, rather than reporting what it managed before.</summary>
    [Fact]
    public async Task ADeletionOfSomethingThatIsNotThere_SaysSo()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);
        var page = await browser.GetStringAsync($"/{VolumeAreas.Files}");

        var answer = await browser.PostAsync("/delete", Form(("area", VolumeAreas.Files), ("path", ""), ("only", "never-written.txt"), Antiforgery(page)));

        Assert.Equal(HttpStatusCode.Redirect, answer.StatusCode);
        Assert.Contains("problem", answer.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Only the job logs are pruned: the areas holding the owner's own files are not something a page may empty wholesale.</summary>
    [Fact]
    public async Task PruningAnythingButTheJobLogs_IsNotAPage()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);
        var page = await browser.GetStringAsync($"/{VolumeAreas.Files}");

        var answer = await browser.PostAsync("/prune", Form(("area", VolumeAreas.Files), Antiforgery(page)));

        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
    }

    /// <summary>An asset nobody ships is not there; the ones that are ship with the server itself.</summary>
    [Fact]
    public async Task AnAssetThatIsNotShipped_IsNotFound()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client();

        Assert.Equal(HttpStatusCode.NotFound, (await browser.GetAsync("/assets/nothing.css")).StatusCode);
    }

    /// <summary>A browser that already has a session is sent on rather than shown the sign-in page again, and the place it is sent to is one of
    /// this server's own pages.</summary>
    [Fact]
    public async Task TheSignInPage_WithASessionAlready_SendsTheBrowserOn()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var browser = server.Client(followRedirects: false);
        await SignInAsync(browser);

        var again = await browser.GetAsync("/login?returnUrl=%2Ffiles");
        var elsewhere = await browser.GetAsync("/login?returnUrl=https%3A%2F%2Felsewhere.example.com");

        Assert.Equal(HttpStatusCode.Redirect, again.StatusCode);
        Assert.Equal("/files", again.Headers.Location!.ToString());
        Assert.StartsWith("/", elsewhere.Headers.Location!.ToString(), StringComparison.Ordinal);
    }

    private static async Task SignInAsync(HttpClient browser)
    {
        var form = await browser.GetStringAsync("/login");
        var admitted = await browser.PostAsync("/login", Form(("token", Token), Antiforgery(form)));

        Assert.Equal(HttpStatusCode.Redirect, admitted.StatusCode);
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(field => new KeyValuePair<string, string>(field.Name, field.Value)));

    private static (string Name, string Value) Antiforgery(string page)
    {
        var field = AntiforgeryField().Match(page);

        return (field.Groups["name"].Value, field.Groups["value"].Value);
    }

    [GeneratedRegex(@"name=""(?<name>__RequestVerificationToken)""[^>]*value=""(?<value>[^""]+)""")]
    private static partial Regex AntiforgeryField();
}
