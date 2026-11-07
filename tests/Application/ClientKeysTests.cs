using Microsoft.Extensions.Time.Testing;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>The keys clients are admitted by, read back from a file somebody may have edited by hand.</summary>
public class ClientKeysTests
{
    private readonly string _directory = TestFolder.Create();

    /// <summary>One line of the file that is not a digest turns away only the client it belonged to: before, decoding it threw out of the
    /// authentication handler and every client got a 500.</summary>
    [Theory]
    [InlineData("not base64 at all!")]
    [InlineData("AAAA")]
    public void File_WithOneDigestThatCannotBeRead_StillAdmitsTheOtherKeys(string corrupt)
    {
        var minted = Build();
        var (_, broken) = minted.Mint("lighting");
        var (_, kept) = minted.Mint("modeling");
        Corrupt("lighting", corrupt);

        var keys = Build();

        Assert.Equal("modeling", keys.Holder(kept)?.Name);
        Assert.Null(keys.Holder(broken));
        Assert.Null(keys.Holder("a guess"));
    }

    private void Corrupt(string name, string digest)
    {
        var file = Path.Combine(_directory, "state", "clients.json");
        var document = JsonNode.Parse(File.ReadAllText(file))!;
        var client = document["clients"]!.AsArray().First(entry => entry!["name"]!.GetValue<string>() == name)!;

        client["digest"] = digest;
        File.WriteAllText(file, document.ToJsonString());
    }

    private ClientKeys Build() => new(new ServerConfig { DataDirectory = _directory }, new FakeTimeProvider());
}
