using Snail.MCP.AfterEffects.Tools;

namespace Snail.MCP.AfterEffects.Tests.Tools;

/// <summary>How a tool's parameters become the params of a bridge request: what the caller did not pass is left out, so the bridge's own
/// defaults apply, and the few parameters that mean something by being empty travel as an explicit null.</summary>
public sealed class ArgumentsTests
{
    [Fact]
    public void WhatTheCallerDidNotPass_IsLeftOut()
    {
        var built = new JsonObject()
            .With("name", (string?)null)
            .With("visible", (bool?)null)
            .With("index", (int?)null)
            .With("opacity", (double?)null)
            .With("layers", (int[]?)null)
            .With("sizes", (double[]?)null)
            .With("names", (string[]?)null)
            .With("extra", (JsonNode?)null);

        Assert.Empty(built);
    }

    [Fact]
    public void WhatTheCallerPassed_TravelsWithItsOwnType()
    {
        var built = new JsonObject()
            .With("name", "plate")
            .With("visible", true)
            .With("index", 2)
            .With("opacity", 42.5)
            .With("layers", [1, 2])
            .With("sizes", [0.5, 1.5])
            .With("names", ["a", "b"])
            .With("extra", new JsonObject { ["deep"] = 1 });

        Assert.Equal("plate", built["name"]!.GetValue<string>());
        Assert.True(built["visible"]!.GetValue<bool>());
        Assert.Equal(2, built["index"]!.GetValue<int>());
        Assert.Equal(42.5, built["opacity"]!.GetValue<double>());
        Assert.Equal([1, 2], built["layers"]!.AsArray().Select(value => value!.GetValue<int>()));
        Assert.Equal([0.5, 1.5], built["sizes"]!.AsArray().Select(value => value!.GetValue<double>()));
        Assert.Equal(["a", "b"], built["names"]!.AsArray().Select(value => value!.GetValue<string>()));
        Assert.Equal(1, built["extra"]!["deep"]!.GetValue<int>());
    }

    /// <summary>A node is cloned on the way in: a tool that passed one object to two parameters would otherwise hand the bridge a node that
    /// already has a parent, which System.Text.Json refuses.</summary>
    [Fact]
    public void ANodeThatIsPassedTwice_IsTakenBothTimes()
    {
        var shared = new JsonObject { ["x"] = 1 };

        var built = new JsonObject().With("first", shared).With("second", shared);

        Assert.Equal(1, built["first"]!["x"]!.GetValue<int>());
        Assert.Equal(1, built["second"]!["x"]!.GetValue<int>());
    }

    /// <summary>A reference is a name or an id, and a number travels as a number so the bridge looks it up by id rather than by a name of digits.</summary>
    [Theory]
    [InlineData("7", true)]
    [InlineData("Plate", false)]
    public void AReference_TravelsAsAnIdOnlyWhenItIsANumber(string reference, bool isNumber)
    {
        var built = new JsonObject().WithReference("layer", reference);

        Assert.Equal(isNumber, built["layer"]!.GetValueKind() is System.Text.Json.JsonValueKind.Number);
    }

    [Fact]
    public void References_AreEachANameOrAnId()
    {
        var built = new JsonObject().WithReferences("layers", ["1", "Plate"]);

        Assert.Equal(1, built["layers"]![0]!.GetValue<int>());
        Assert.Equal("Plate", built["layers"]![1]!.GetValue<string>());
        Assert.Empty(new JsonObject().WithReferences("layers", null));
    }

    /// <summary>An empty parent means "take the layer off its parent", which only an explicit null says to the bridge; not passing one at all
    /// leaves the parent as it is.</summary>
    [Fact]
    public void AnEmptyParent_TravelsAsAnExplicitNull()
    {
        Assert.True(new JsonObject().WithParent(string.Empty).ContainsKey("parent"));
        Assert.Null(new JsonObject().WithParent(string.Empty)["parent"]);
        Assert.Equal(3, new JsonObject().WithParent("3")["parent"]!.GetValue<int>());
        Assert.Equal("Null 1", new JsonObject().WithParent("Null 1")["parent"]!.GetValue<string>());
        Assert.Empty(new JsonObject().WithParent(null));
    }

    /// <summary>The word none takes the text off its path, and it is an explicit null that says so.</summary>
    [Fact]
    public void TheMaskNamedNone_TravelsAsAnExplicitNull()
    {
        Assert.True(new JsonObject().WithMask("NONE").ContainsKey("mask"));
        Assert.Null(new JsonObject().WithMask("none")["mask"]);
        Assert.Equal(2, new JsonObject().WithMask("2")["mask"]!.GetValue<int>());
        Assert.Equal("Mask 1", new JsonObject().WithMask("Mask 1")["mask"]!.GetValue<string>());
        Assert.Empty(new JsonObject().WithMask(null));
    }

    [Fact]
    public void ATrackMatte_TravelsAsTheLayerAndItsType()
    {
        var built = new JsonObject().WithTrackMatte("4", "alpha");

        Assert.Equal(4, built["track_matte"]!.GetValue<int>());
        Assert.Equal("alpha", built["track_matte_type"]!.GetValue<string>());
        Assert.Empty(new JsonObject().WithTrackMatte(null, null));
    }
}
