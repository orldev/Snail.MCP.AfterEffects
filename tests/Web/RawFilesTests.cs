using System.Formats.Tar;
using System.Net;
using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Application.Storage;

namespace Snail.MCP.AfterEffects.Tests.Web;

/// <summary>The areas' files over HTTP, asked for the way a client on another machine asks: whole files and folders as archives, uploads in
/// parts, and links that carry their own right to one path.</summary>
/// <remarks>The pages are driven elsewhere; this is the surface a curl command reaches, which is what the transfer tools tell an agent to run.
/// Every answer is checked by its status as well as its body, because a client that cannot read the body still has to act on the status.</remarks>
public sealed class RawFilesTests
{
    private const string Token = "raw-operator-token";

    /// <summary>A folder comes as one archive: a tar by default, a zip when a browser asks for one, each named after the folder.</summary>
    [Fact]
    public async Task AFolder_ComesAsOneArchive_TarByDefaultAndZipWhenAsked()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);
        await WriteAsync(server, "scene/frame.png", "a picture, almost");
        await WriteAsync(server, "scene/notes.txt", "two files make a folder worth archiving");

        var tar = await client.GetAsync($"/raw/{VolumeAreas.Files}/scene");
        var zip = await client.GetAsync($"/raw/{VolumeAreas.Files}/scene?format={FileLinks.ZipFormat}");

        Assert.Equal("application/x-tar", tar.Content.Headers.ContentType!.MediaType);
        Assert.Equal("application/zip", zip.Content.Headers.ContentType!.MediaType);
        Assert.Contains("scene.tar", tar.Content.Headers.ContentDisposition!.ToString(), StringComparison.Ordinal);
        Assert.Equal(["frame.png", "notes.txt"], await NamesInTarAsync(await tar.Content.ReadAsStreamAsync()));
        Assert.True((await zip.Content.ReadAsByteArrayAsync()).Length > 0);
    }

    /// <summary>A whole area is an archive too, which is how a client takes everything at once.</summary>
    [Fact]
    public async Task AnArea_WithNoPathAtAll_ComesAsOneArchive()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);
        await WriteAsync(server, "plate.mov", "not really a movie");

        var archive = await client.GetAsync($"/raw/{VolumeAreas.Files}");

        Assert.Equal(HttpStatusCode.OK, archive.StatusCode);
        Assert.Contains($"{VolumeAreas.Files}.tar", archive.Content.Headers.ContentDisposition!.ToString(), StringComparison.Ordinal);
    }

    /// <summary>A file is served as what it is, sandboxed and unsniffed so an HTML or SVG file out of an area cannot run script in the pages'
    /// origin, and as an attachment when the caller asks to download it.</summary>
    [Fact]
    public async Task AFile_IsServedSandboxed_AndAsAnAttachmentWhenAsked()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);
        await WriteAsync(server, "notes.txt", "hello");
        await WriteAsync(server, "plate.unheardof", "bytes of something");

        var text = await client.GetAsync($"/raw/{VolumeAreas.Files}/notes.txt");
        var unknown = await client.GetAsync($"/raw/{VolumeAreas.Files}/plate.unheardof");
        var attachment = await client.GetAsync($"/raw/{VolumeAreas.Files}/notes.txt?download=true");

        Assert.Equal("text/plain", text.Content.Headers.ContentType!.MediaType);
        Assert.Equal("application/octet-stream", unknown.Content.Headers.ContentType!.MediaType);
        Assert.Equal("sandbox", text.Headers.GetValues("Content-Security-Policy").Single());
        Assert.Equal("nosniff", text.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("attachment", attachment.Content.Headers.ContentDisposition!.DispositionType);
    }

    /// <summary>A file comes in ranges, which is how a player seeks in a render without fetching it whole.</summary>
    [Fact]
    public async Task AFile_AnsweredInRanges_GivesThePartThatWasAskedFor()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);
        await WriteAsync(server, "notes.txt", "0123456789");

        using var request = new HttpRequestMessage(HttpMethod.Get, $"/raw/{VolumeAreas.Files}/notes.txt");
        request.Headers.Range = new RangeHeaderValue(2, 4);
        var part = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.PartialContent, part.StatusCode);
        Assert.Equal("234", await part.Content.ReadAsStringAsync());
    }

    /// <summary>An area nobody named and a path climbing out of one are refused before anything is read, and the answer names the areas.</summary>
    [Fact]
    public async Task AnAreaThatIsNotOne_AndAPathLeavingIt_AreRefused()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var nowhere = await client.GetAsync("/raw/nowhere/plate.mov");
        var climbing = await client.GetAsync($"/raw/{VolumeAreas.Files}/../secrets");

        Assert.Equal(HttpStatusCode.NotFound, nowhere.StatusCode);
        Assert.Contains(VolumeAreas.Files, await nowhere.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.True(climbing.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound);
    }

    /// <summary>A file sent in parts gathers under its own path: each part but the last answers with how much has arrived, and the last one
    /// answers with the file.</summary>
    [Fact]
    public async Task AFileSentInParts_GathersUntilTheLastOne()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var first = await client.PutAsync($"/raw/{VolumeAreas.Files}/split.txt?offset=0&last=false", new StringContent("half "));
        var second = await client.PutAsync($"/raw/{VolumeAreas.Files}/split.txt?offset=5&last=true", new StringContent("and half"));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("received", await first.Content.ReadAsStringAsync(), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal("half and half", await client.GetStringAsync($"/raw/{VolumeAreas.Files}/split.txt"));
    }

    /// <summary>Values a client could only have mistyped are refused with what they take, rather than written somewhere unexpected.</summary>
    [Theory]
    [InlineData("?offset=-1")]
    [InlineData("?sha256=nothexadecimal")]
    public async Task AWriteWithAValueThatCannotBeOne_IsRefused(string query)
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var refused = await client.PutAsync($"/raw/{VolumeAreas.Files}/plate.mov{query}", new StringContent("bytes"));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("sha256", await refused.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    /// <summary>A write needs a path inside the area; the area itself takes a tar, which is what the answer says to send instead.</summary>
    [Fact]
    public async Task AWriteWithNoPath_SaysWhatToSendInstead()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var refused = await client.PutAsync($"/raw/{VolumeAreas.Files}", new StringContent("bytes"));

        Assert.Equal(HttpStatusCode.BadRequest, refused.StatusCode);
        Assert.Contains("format=tar", await refused.Content.ReadAsStringAsync(), StringComparison.Ordinal);
    }

    /// <summary>A tar unpacks into a folder of the area, and the answer lists what landed with the digest of each file.</summary>
    [Fact]
    public async Task ATarSentWhole_UnpacksIntoTheArea()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var unpacked = await client.PutAsync(
            $"/raw/{VolumeAreas.Files}/incoming?format={FileLinks.TarFormat}",
            new ByteArrayContent(await TarOfAsync(("one.txt", "first"), ("two.txt", "second"))));

        Assert.Equal(HttpStatusCode.OK, unpacked.StatusCode);
        var landed = await unpacked.Content.ReadAsStringAsync();

        Assert.Contains("one.txt", landed, StringComparison.Ordinal);
        Assert.Contains("sha256", landed, StringComparison.Ordinal);
        Assert.Equal("second", await client.GetStringAsync($"/raw/{VolumeAreas.Files}/incoming/two.txt"));
    }

    /// <summary>A tar sent in parts gathers in a staging file of its own and unpacks when the last part arrives, so a large archive needs no
    /// request that stays open for all of it.</summary>
    [Fact]
    public async Task ATarSentInParts_UnpacksWhenTheLastPartArrives()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);
        var archive = await TarOfAsync(("split.txt", "a tar in two halves"));
        var middle = archive.Length / 2;

        var first = await client.PutAsync($"/raw/{VolumeAreas.Files}/gathered?format={FileLinks.TarFormat}&offset=0&last=false", new ByteArrayContent(archive[..middle]));
        var last = await client.PutAsync($"/raw/{VolumeAreas.Files}/gathered?format={FileLinks.TarFormat}&offset={middle}&last=true", new ByteArrayContent(archive[middle..]));

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.OK, last.StatusCode);
        Assert.Equal("a tar in two halves", await client.GetStringAsync($"/raw/{VolumeAreas.Files}/gathered/split.txt"));
    }

    /// <summary>A signed link opens the one path it was issued for, to a client with no key at all, and nothing else: another path, or a format
    /// it was not signed for, opens nothing. Asking for the same bytes as an attachment is not another path, so the signature leaves it alone.</summary>
    [Fact]
    public async Task ASignedLink_OpensItsOwnPath_AndNothingElse()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        await WriteAsync(server, "shared.txt", "for whoever holds the link");
        var links = server.Services.GetRequiredService<FileLinks>();
        var link = links.For("GET", VolumeAreas.Files, "shared.txt");
        using var stranger = server.Client();

        var opened = await stranger.GetAsync(Relative(link.Url));
        var asAttachment = await stranger.GetAsync($"{Relative(link.Url)}&download=true");
        var asArchive = await stranger.GetAsync($"{Relative(link.Url)}&format={FileLinks.ZipFormat}");
        var elsewhere = await stranger.GetAsync(Relative(link.Url).Replace("shared.txt", "other.txt", StringComparison.Ordinal));

        Assert.Equal("for whoever holds the link", await opened.Content.ReadAsStringAsync());
        Assert.Equal("attachment", asAttachment.Content.Headers.ContentDisposition!.DispositionType);
        Assert.True(asArchive.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
        Assert.True(elsewhere.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    /// <summary>A link issued to write does not read, and one issued to read does not write: a link carries one method.</summary>
    [Fact]
    public async Task ALinkIssuedToWrite_DoesNotRead()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        var links = server.Services.GetRequiredService<FileLinks>();
        var writing = links.For("PUT", VolumeAreas.Files, "arriving.txt");
        using var stranger = server.Client();

        var written = await stranger.PutAsync(Relative(writing.Url), new StringContent("arrived"));
        var read = await stranger.GetAsync(Relative(writing.Url));

        Assert.Equal(HttpStatusCode.OK, written.StatusCode);
        Assert.True(read.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden);
    }

    /// <summary>Deleting what is not there is not an error to retry but an answer: nothing of that name is in the area.</summary>
    [Fact]
    public async Task DeletingWhatIsNotThere_IsNotFound()
    {
        await using var server = await HttpServerInProcess.StartAsync(Token);
        using var client = Keyed(server);

        var answer = await client.DeleteAsync($"/raw/{VolumeAreas.Files}/never-written.txt");

        Assert.Equal(HttpStatusCode.NotFound, answer.StatusCode);
    }

    private static HttpClient Keyed(HttpServerInProcess server)
    {
        var client = server.Client();
        client.DefaultRequestHeaders.Authorization = new("Bearer", Token);

        return client;
    }

    private static string Relative(string url) => new Uri(url).PathAndQuery;

    private static async Task WriteAsync(HttpServerInProcess server, string path, string text)
    {
        var volume = server.Services.GetRequiredService<Volume>();
        var full = Path.Combine(volume.RootOf(VolumeAreas.Files), path.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);

        await File.WriteAllTextAsync(full, text);
    }

    private static async Task<byte[]> TarOfAsync(params (string Name, string Text)[] files)
    {
        var archive = new MemoryStream();

        await using (var writer = new TarWriter(archive, TarEntryFormat.Pax, leaveOpen: true))
        {
            foreach (var (name, text) in files)
            {
                await writer.WriteEntryAsync(new PaxTarEntry(TarEntryType.RegularFile, name) { DataStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes(text)) });
            }
        }

        return archive.ToArray();
    }

    private static async Task<IReadOnlyList<string>> NamesInTarAsync(Stream archive)
    {
        var names = new List<string>();
        await using var reader = new TarReader(archive);

        while (await reader.GetNextEntryAsync() is { } entry)
        {
            names.Add(entry.Name.Split('/')[^1]);
        }

        return [.. names.Order(StringComparer.Ordinal)];
    }
}
