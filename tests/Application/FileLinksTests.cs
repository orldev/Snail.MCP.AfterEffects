using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Storage;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>The links the server hands out for its own files: signed for one method, one path, one format and one way of landing, and dead once
/// their minutes are up. Every test here asks what a link opens, because a link that opens more than it was issued for is the whole risk.</summary>
public sealed class FileLinksTests
{
    private static readonly DateTimeOffset Noon = new(2026, 10, 23, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ALink_CarriesItsExpiryAndSignature_AndTheAddressTheServerIsReachedAt()
    {
        var links = Links(out var config);
        config.Http.LinkMinutes = 10;

        var link = links.For("GET", VolumeAreas.Files, "plate.mov");

        Assert.StartsWith("http://127.0.0.1:8080/raw/files/plate.mov?", link.Url, StringComparison.Ordinal);
        Assert.Contains("signature=", link.Url, StringComparison.Ordinal);
        Assert.Equal(Noon.AddMinutes(10), link.Expires);
    }

    /// <summary>Behind a proxy the address a client reaches is not the one the server listens on, so the link carries the public one.</summary>
    [Fact]
    public void ALink_BehindAProxy_CarriesThePublicAddress()
    {
        var links = Links(out var config);
        config.Http.PublicUrl = "https://ae.example.com/";

        Assert.StartsWith("https://ae.example.com/raw/files/plate.mov", links.For("GET", VolumeAreas.Files, "plate.mov").Url, StringComparison.Ordinal);
    }

    [Fact]
    public void ALink_ForAFolderAsOneArchive_CarriesTheFormat()
    {
        var link = Links(out _).For("GET", VolumeAreas.Renders, "scene", FileLinks.ZipFormat);

        Assert.Contains($"format={FileLinks.ZipFormat}", link.Url, StringComparison.Ordinal);
    }

    /// <summary>A link is checked for exactly what it was issued for: the same method, path, format and way of landing, and nothing else opens with it.</summary>
    [Fact]
    public void ALink_Verifies_ForWhatItWasIssuedFor()
    {
        var links = Links(out _);
        var signature = Signature(links.For("PUT", VolumeAreas.Files, "plate.mov", FileLinks.TarFormat, overwrite: true).Url);
        var route = FileLinks.Route(VolumeAreas.Files, "plate.mov");
        var expires = Noon.AddMinutes(30).ToUnixTimeSeconds();

        Assert.True(links.Verifies("PUT", route, FileLinks.TarFormat, overwrite: true, expires, signature));
        Assert.False(links.Verifies("GET", route, FileLinks.TarFormat, overwrite: true, expires, signature));
        Assert.False(links.Verifies("PUT", FileLinks.Route(VolumeAreas.Files, "other.mov"), FileLinks.TarFormat, overwrite: true, expires, signature));
        Assert.False(links.Verifies("PUT", route, null, overwrite: true, expires, signature));
        Assert.False(links.Verifies("PUT", route, FileLinks.TarFormat, overwrite: false, expires, signature));
        Assert.False(links.Verifies("PUT", route, FileLinks.TarFormat, overwrite: true, expires + 1, signature));
    }

    /// <summary>curl -I asks with HEAD what a link opens with GET, so a read link answers both.</summary>
    [Fact]
    public void AReadLink_AnswersHeadAsWellAsGet()
    {
        var links = Links(out _);
        var route = FileLinks.Route(VolumeAreas.Captures, "frame.png");
        var expires = Noon.AddMinutes(30).ToUnixTimeSeconds();
        var signature = Signature(links.For("GET", VolumeAreas.Captures, "frame.png").Url);

        Assert.True(links.Verifies("head", route, null, overwrite: false, expires, signature));
    }

    /// <summary>An expiry in the past, or one so far out that it cannot be a moment at all, opens nothing — the second is what a caller tries
    /// after noticing the first.</summary>
    [Theory]
    [InlineData(-1L)]
    [InlineData(253402300800L)]
    public void AnExpiryThatIsNotAMoment_OpensNothing(long expires)
    {
        var links = Links(out _);

        Assert.False(links.Verifies("GET", FileLinks.Route(VolumeAreas.Files, "plate.mov"), null, overwrite: false, expires, "whatever"));
    }

    [Fact]
    public void ALinkWhoseMinutesAreUp_OpensNothing()
    {
        var time = new FakeTime(Noon);
        var config = Config();
        var links = new FileLinks(new ClientToken(config), config, time);
        var route = FileLinks.Route(VolumeAreas.Files, "plate.mov");
        var signature = Signature(links.For("GET", VolumeAreas.Files, "plate.mov").Url);
        var expires = Noon.AddMinutes(30).ToUnixTimeSeconds();

        Assert.True(links.Verifies("GET", route, null, overwrite: false, expires, signature));

        time.Now = Noon.AddMinutes(31);

        Assert.False(links.Verifies("GET", route, null, overwrite: false, expires, signature));
    }

    [Theory]
    [InlineData("", "/raw/files")]
    [InlineData("/", "/raw/files")]
    [InlineData("scene/frame.png", "/raw/files/scene/frame.png")]
    [InlineData("/scene/", "/raw/files/scene")]
    public void TheRoute_IsThePathARequestArrivesAt(string path, string route) =>
        Assert.Equal(route, FileLinks.Route(VolumeAreas.Files, path));

    private static string Signature(string url) =>
        url.Split("signature=")[1].Split('&')[0];

    private static ServerConfig Config()
    {
        var config = new ServerConfig { DataDirectory = TestFolder.Create(), Transport = Transport.Http };
        config.Http.Url = "http://127.0.0.1:8080";
        config.Http.Token = "a-token-of-this-test";
        config.Http.LinkMinutes = 30;

        return config;
    }

    private static FileLinks Links(out ServerConfig config)
    {
        config = Config();

        return new FileLinks(new ClientToken(config), config, new FakeTime(Noon));
    }

    private sealed class FakeTime(DateTimeOffset now) : TimeProvider
    {
        public DateTimeOffset Now { get; set; } = now;

        public override DateTimeOffset GetUtcNow() => Now;
    }
}
