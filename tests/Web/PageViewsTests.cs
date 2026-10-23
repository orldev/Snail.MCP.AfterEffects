using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Farm;
using Snail.MCP.AfterEffects.Application.Sessions;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Web.Pages;

namespace Snail.MCP.AfterEffects.Tests.Web;

/// <summary>What the operator's pages show, read out of the server's own reports: the same reports the tools answer with, so a page says what a
/// tool would say. The tests ask the two readings of every row — the one where the server is well and the one where it is not — because a page
/// that shows a problem as if it were health is worse than a page that shows nothing.</summary>
public sealed class PageViewsTests
{
    [Fact]
    public void TheStatus_OfAServerThatIsWell_ShowsNoProblem()
    {
        var groups = PageViews.Status(Report(isReachable: true), Storage(), [], Farm("/Applications/aerender"), Config(), TimeSpan.FromMinutes(90));
        var rows = groups.SelectMany(group => group.Rows).ToList();

        Assert.Equal(["After Effects", "Farm", "Storage", "Server"], groups.Select(group => group.Caption));
        Assert.DoesNotContain(rows, row => row.IsProblem);
        Assert.Contains(rows, row => row.Label == "After Effects" && row.Value.Contains("answers", StringComparison.Ordinal));
        Assert.Contains(rows, row => row.Label == "Bridge" && row.Value.Contains("the build this server ships", StringComparison.Ordinal));
        Assert.Contains(rows, row => row.Label == "Running for" && row.Value == "1 h 30 min");
        Assert.Contains(rows, row => row.Label == "Disk" && row.Value.Contains("free of", StringComparison.Ordinal));
    }

    /// <summary>A server nothing answers in: every row that can say so says so, and the page shows the reason the bridge gave rather than only
    /// that something is wrong.</summary>
    [Fact]
    public void TheStatus_OfAServerNothingAnswersIn_ShowsEveryProblem()
    {
        var rows = PageViews.Status(Report(isReachable: false), Storage(), [], Farm(executable: null), Config(), TimeSpan.FromSeconds(20))
            .SelectMany(group => group.Rows)
            .ToList();

        Assert.Contains(rows, row => row.Label == "After Effects" && row.IsProblem && row.Value.Contains("no bridge", StringComparison.Ordinal));
        Assert.Contains(rows, row => row.Label == "Scripting" && row.IsProblem);
        Assert.Contains(rows, row => row.Label == "aerender" && row.IsProblem);
        Assert.Contains(rows, row => row.Label == "Render jobs" && row.Value == "none");
        Assert.Contains(rows, row => row.Label == "Running for" && row.Value == "20 s");
    }

    /// <summary>The bridge row reads the freshness the diagnosis reports, and every reading but "current" is a problem to show.</summary>
    [Theory]
    [InlineData("current", false)]
    [InlineData("differs", true)]
    [InlineData("older", true)]
    [InlineData("unknown", true)]
    public void TheBridgeRow_ReadsTheFreshnessOfTheInstalledBridge(string status, bool isProblem)
    {
        var report = Report(isReachable: true);
        report["extension"]!["status"] = status;

        var row = PageViews.Status(report, Storage(), [], Farm("/aerender"), Config(), TimeSpan.Zero)
            .SelectMany(group => group.Rows)
            .First(candidate => candidate.Label == "Bridge");

        Assert.Equal(isProblem, row.IsProblem);
    }

    /// <summary>A disk with almost nothing left is a problem; the same disk half full is not.</summary>
    [Fact]
    public void TheDiskRow_IsAProblem_OnceAlmostNothingIsLeft()
    {
        var tight = PageViews.Status(Report(true), Storage(freeBytes: 1024), [], Farm("/aerender"), Config(), TimeSpan.Zero);

        Assert.True(tight.SelectMany(group => group.Rows).First(row => row.Label == "Disk").IsProblem);
    }

    /// <summary>A report with none of its parts, as a server that has just started answers: the page shows rows rather than throwing.</summary>
    [Fact]
    public void TheStatus_OfAReportWithNothingInIt_StillShowsItsRows()
    {
        var rows = PageViews.Status([], [], [], Farm(executable: null), Config(), TimeSpan.Zero).SelectMany(group => group.Rows).ToList();

        Assert.Contains(rows, row => row.Label == "Bridge port" && row.Value == "none");
        Assert.Contains(rows, row => row.Label == "Version" && row.Value.Length == 0);
        Assert.DoesNotContain(rows, row => row.Label == "Disk");
    }

    [Fact]
    public void Browsing_AFolder_LinksEveryEntryAndTheArchiveOfTheFolderItself()
    {
        var listing = new VolumeListing("/data/files", [Folder("scene"), File("plate.mov", 2048)], IsTruncated: false, Error: null);

        var view = PageViews.Browse(VolumeAreas.Files, "work", listing, null);

        Assert.Equal("Files", view.Title);
        Assert.Equal(["files", "work"], view.Crumbs.Select(crumb => crumb.Label));
        Assert.Contains("format=zip", view.ZipHref!, StringComparison.Ordinal);
        Assert.Contains("format=zip", view.Entries[0].DownloadHref, StringComparison.Ordinal);
        Assert.Contains("download=true", view.Entries[1].DownloadHref, StringComparison.Ordinal);
        Assert.Null(view.Entries[0].JobId);
        Assert.False(view.CanPrune);
    }

    /// <summary>A folder with nothing in it offers no archive of itself, and a name with a space or a hash is escaped into the link.</summary>
    [Fact]
    public void Browsing_AnEmptyFolder_OffersNoArchive()
    {
        var empty = PageViews.Browse(VolumeAreas.Files, string.Empty, new VolumeListing("/data/files", [], false, null), null);
        var odd = PageViews.Browse(VolumeAreas.Files, "two words", new VolumeListing("/data/files", [File("a#b.png", 1)], false, null), null);

        Assert.Null(empty.ZipHref);
        Assert.Single(empty.Crumbs);
        Assert.Contains("two%20words", odd.Entries[0].Href, StringComparison.Ordinal);
        Assert.Contains("a%23b.png", odd.Entries[0].Href, StringComparison.Ordinal);
    }

    /// <summary>The jobs area offers to prune once a job has finished, and never while every job it lists is still running.</summary>
    [Fact]
    public void TheJobsArea_OffersToPrune_OnceAJobHasFinished()
    {
        var done = new VolumeListing("/data/jobs", [File("a7.json", 12) with { State = "done" }], false, null);
        var running = new VolumeListing("/data/jobs", [File("a7.json", 12) with { State = "running", InUse = "rendering" }], false, null);

        Assert.True(PageViews.Browse(VolumeAreas.Jobs, string.Empty, done, null).CanPrune);
        Assert.False(PageViews.Browse(VolumeAreas.Jobs, string.Empty, running, null).CanPrune);
        Assert.Equal("a7", PageViews.Browse(VolumeAreas.Jobs, string.Empty, done, null).Entries[0].JobId);
    }

    [Fact]
    public void AnAreaThatCannotBeRead_ShowsWhatTheServerSaid()
    {
        var view = PageViews.Unavailable(VolumeAreas.Renders, "scene", new BridgeError("NotFound", "no such folder"), null);

        Assert.Equal("no such folder", view.Problem);
        Assert.Equal("Renders", view.Title);
        Assert.Empty(view.Entries);
    }

    [Fact]
    public void AFile_CarriesItsFolderItsLinksAndWhetherItIsAPicture()
    {
        var picture = PageViews.File(VolumeAreas.Captures, "scene/frame.png", 4096, null);
        var text = PageViews.File(VolumeAreas.Files, "notes.txt", 12, "hello");

        Assert.Equal("frame.png", picture.Name);
        Assert.Equal("scene", picture.Folder);
        Assert.True(picture.IsImage);
        Assert.Contains("download=true", picture.DownloadHref, StringComparison.Ordinal);
        Assert.Equal(string.Empty, text.Folder);
        Assert.False(text.IsImage);
        Assert.Equal("hello", text.Text);
    }

    [Theory]
    [InlineData("frame.PNG", true, false)]
    [InlineData("notes.TXT", false, true)]
    [InlineData("plate.mov", false, false)]
    public void APicture_AndSomethingToRead_AreTheOnesThePageShowsInPlace(string name, bool isImage, bool isText)
    {
        Assert.Equal(isImage, PageViews.IsImage(name));
        Assert.Equal(isText, PageViews.IsText(name));
    }

    /// <summary>One flash at a time, and a problem is the one that wins: a deletion that partly failed must not read as a deletion that worked.</summary>
    [Fact]
    public void TheFlash_SaysOneThing_AndAProblemComesFirst()
    {
        Assert.True(PageViews.FlashOf(4, 2048, "a7", "it went wrong")!.IsError);
        Assert.Contains("4 entries", PageViews.FlashOf(4, 2048, null, null)!.Text, StringComparison.Ordinal);
        Assert.Contains("Nothing", PageViews.FlashOf(0, 0, null, null)!.Text, StringComparison.Ordinal);
        Assert.Contains("a7", PageViews.FlashOf(null, null, "a7", null)!.Text, StringComparison.Ordinal);
        Assert.Null(PageViews.FlashOf(null, null, null, null));
    }

    [Theory]
    [InlineData(0L, "0 B")]
    [InlineData(512L, "512 B")]
    [InlineData(2048L, "2 KB")]
    [InlineData(1536L, "1.5 KB")]
    [InlineData(123L * 1024 * 1024, "123 MB")]
    public void ASize_ReadsAsAPersonWouldSayIt(long bytes, string read) => Assert.Equal(read, Readable.Size(bytes));

    [Fact]
    public void AMomentThatIsNotThere_ReadsAsNothing()
    {
        Assert.Equal(string.Empty, Readable.Moment(null));
        Assert.Contains("2026", Readable.Moment(new DateTimeOffset(2026, 10, 23, 12, 0, 0, TimeSpan.Zero)), StringComparison.Ordinal);
    }

    private static VolumeEntry Folder(string name) => new(name, IsFolder: true, Bytes: 0, Files: 2, Modified: DateTimeOffset.UnixEpoch);

    private static VolumeEntry File(string name, long bytes) => new(name, IsFolder: false, bytes, Files: 1, Modified: DateTimeOffset.UnixEpoch);

    private static JsonObject Report(bool isReachable) => new()
    {
        ["server"] = new JsonObject { ["version"] = "0.1.0" },
        ["afterEffects"] = isReachable
            ? new JsonObject { ["reachable"] = true, ["app"] = "26.5x89", ["scripting_allowed"] = true }
            : new JsonObject { ["reachable"] = false, ["scripting_allowed"] = false },
        ["extension"] = new JsonObject { ["status"] = "current", ["installed"] = new JsonObject { ["digest"] = "bef5455a" }, ["bundled"] = new JsonObject { ["digest"] = "bef5455a" } },
        ["bridge"] = new JsonObject { ["port"] = 61323 },
    };

    private static JsonObject Storage(long freeBytes = 400L * 1024 * 1024 * 1024) => new()
    {
        ["areas"] = new JsonArray(new JsonObject { ["area"] = VolumeAreas.Files, ["bytes"] = 2048L, ["files"] = 3, ["directory"] = "/data/files" }),
        ["disk"] = new JsonObject { ["free_bytes"] = freeBytes, ["total_bytes"] = 500L * 1024 * 1024 * 1024 },
    };

    private static ServerConfig Config()
    {
        var config = new ServerConfig { DataDirectory = TestFolder.Create(), Transport = Transport.Http };
        ServerConfigExtensions.Normalize(config);

        return config;
    }

    private static RenderFarm Farm(string? executable)
    {
        var config = Config();
        var clients = new ClientSessions([], [], null, new ServiceCollection().BuildServiceProvider(), TimeProvider.System);

        return new RenderFarm(new FakeAerender(_ => new FakeAerender.FakeRenderProcess([], 0), executable), config, clients, TimeProvider.System, NullLogger<RenderFarm>.Instance);
    }
}
