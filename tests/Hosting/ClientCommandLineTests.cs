using Snail.MCP.AfterEffects.Configuration;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Hosting;

namespace Snail.MCP.AfterEffects.Tests.Hosting;

/// <summary>The <c>clients</c> command, which mints and withdraws the keys an HTTP server admits clients by, run the way the
/// program runs it: arguments in, text out, the keys file beside them.</summary>
public sealed class ClientCommandLineTests : IDisposable
{
    private readonly string _dataDirectory = TestFolder.Create();
    private readonly StringWriter _output = new();

    private ServerConfig Settings => new() { DataDirectory = _dataDirectory };

    [Theory]
    [InlineData("clients")]
    [InlineData("CLIENTS")]
    public void TheVerb_IsRecognised_WhateverItsCase(string verb) =>
        Assert.True(ClientCommandLine.IsAsked([verb, "list"]));

    [Fact]
    public void AnythingElse_IsNotTheClientsCommand() =>
        Assert.False(ClientCommandLine.IsAsked(["--help"]));

    /// <summary>A key is minted, shown once, listed afterwards and then withdrawn; the list says so.</summary>
    [Fact]
    public void AKey_IsMinted_Listed_AndRevoked()
    {
        Assert.Equal(0, Run("clients", "add", "titles"));

        var minted = _output.ToString();

        Assert.Contains("titles", minted, StringComparison.Ordinal);

        _output.GetStringBuilder().Clear();
        Assert.Equal(0, Run("clients", "list"));

        var listed = _output.ToString();

        Assert.Contains("titles", listed, StringComparison.Ordinal);
        Assert.DoesNotContain("revoked", listed, StringComparison.OrdinalIgnoreCase);

        _output.GetStringBuilder().Clear();
        Assert.Equal(0, Run("clients", "revoke", "titles"));

        _output.GetStringBuilder().Clear();
        Run("clients", "list");

        Assert.Contains("revoked", _output.ToString(), StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>A key may be narrower than the operator's: the scopes asked for are the ones the listing shows.</summary>
    [Fact]
    public void AKey_KeepsTheScopes_ItWasMintedWith()
    {
        Run("clients", "add", "reader", "--scope", $"{Scopes.Read},{Scopes.Farm}");
        _output.GetStringBuilder().Clear();
        Run("clients", "list");

        var listed = _output.ToString();

        Assert.Contains(Scopes.Read, listed, StringComparison.Ordinal);
        Assert.Contains(Scopes.Farm, listed, StringComparison.Ordinal);
        Assert.DoesNotContain(Scopes.Delete, listed, StringComparison.Ordinal);
    }

    /// <summary>With no clients the listing says how to mint one, and an unknown action prints the usage.</summary>
    [Fact]
    public void WithoutKeys_ItSaysHowToMintOne_AndAnUnknownActionPrintsTheUsage()
    {
        Run("clients", "list");

        Assert.Contains("clients add", _output.ToString(), StringComparison.Ordinal);

        _output.GetStringBuilder().Clear();

        Assert.NotEqual(0, Run("clients", "sprinkle"));
        Assert.Contains("usage:", _output.ToString(), StringComparison.Ordinal);
    }

    /// <summary>Withdrawing a key nobody minted is refused rather than silently done.</summary>
    [Fact]
    public void RevokingAStranger_Fails()
    {
        Assert.NotEqual(0, Run("clients", "revoke", "nobody"));
    }

    private int Run(params string[] args) => ClientCommandLine.Run(args, _output, Settings);

    public void Dispose()
    {
        _output.Dispose();

        try
        {
            Directory.Delete(_dataDirectory, recursive: true);
        }
        catch (IOException)
        {
        }
    }
}
