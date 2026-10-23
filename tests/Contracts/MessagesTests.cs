using System.Reflection;
using System.Text.RegularExpressions;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>The texts a failed call carries: every error the bridge and the link can raise gets a hint that tells the model
/// what to do next, and every message that is built from values names them.</summary>
public sealed partial class MessagesTests
{
    /// <summary>Every failure of the link, and every error type a bridge module raises, has a hint of its own; a hint that
    /// merely repeats the error would help nobody, so each is its own sentence.</summary>
    [Theory]
    [InlineData(BridgeError.UnavailableType)]
    [InlineData(BridgeError.TimeoutType)]
    [InlineData(BridgeError.DroppedType)]
    [InlineData(BridgeError.MalformedType)]
    [InlineData(BridgeError.ExpiredType)]
    [InlineData(BridgeError.WorkerGoneType)]
    [InlineData("UnknownCommand")]
    [InlineData("BadRequest")]
    [InlineData("ScriptingDisabled")]
    [InlineData("NotFound")]
    [InlineData("NotAText")]
    [InlineData("Busy")]
    [InlineData("NoViewer")]
    [InlineData("NotRendering")]
    [InlineData("Forbidden")]
    [InlineData("NotSupported")]
    [InlineData("ProjectDirty")]
    [InlineData("ExpressionError")]
    public void EveryKnownFailure_HasAHint(string type)
    {
        var hint = Messages.HintFor(new BridgeError(type, "something went wrong"));

        Assert.False(string.IsNullOrWhiteSpace(hint), $"'{type}' has no hint");
        Assert.EndsWith(".", hint!.TrimEnd(), StringComparison.Ordinal);
    }

    /// <summary>Every failure the bridge's own modules raise has a hint: the list is read off the sources, so a module that starts raising a new
    /// kind of failure fails this test until somebody writes what to do about it.</summary>
    /// <remarks>The theory above is a list somebody wrote by hand, and a hand-written list is exactly what drifts. Two types were missing when
    /// this was first run — NoFile and CannotImport, both of them failures an agent has to act on.</remarks>
    [Fact]
    public void EveryFailureTheBridgeRaises_HasAHint()
    {
        var raised = RaisedByTheBridge();

        Assert.NotEmpty(raised);

        var without = raised.Where(type => Messages.HintFor(new BridgeError(type, "something went wrong")) is null).Order(StringComparer.Ordinal).ToList();

        Assert.True(without.Count == 0, $"failures the bridge raises with no hint: {string.Join(", ", without)}");
    }

    private static IReadOnlyList<string> RaisedByTheBridge()
    {
        var modules = Directory.EnumerateFiles(Path.Combine(Repository.Root, "extension", "(snail_bridge)"), "*.jsx");
        var types = new SortedSet<string>(StringComparer.Ordinal);

        foreach (var module in modules)
        {
            foreach (var raise in RaisedType().Matches(File.ReadAllText(module)).Cast<Match>())
            {
                types.Add(raise.Groups["type"].Value);
            }
        }

        return [.. types];
    }

    [GeneratedRegex(@"fail\(""(?<type>[A-Za-z]+)""")]
    private static partial Regex RaisedType();

    /// <summary>A failure nobody wrote a hint for is left without one, rather than given a hint for another failure.</summary>
    [Fact]
    public void AnUnknownFailure_HasNoHint() =>
        Assert.Null(Messages.HintFor(new BridgeError("SomethingNew", "something went wrong")));

    /// <summary>A bad request whose details list what is accepted is pointed at that list; one without is told to read the
    /// error itself.</summary>
    [Fact]
    public void ABadRequest_IsPointedAtWhatItMaySend()
    {
        var listed = new BridgeError("BadRequest", "no", new JsonObject { ["accepted"] = new JsonArray("a", "b") });

        Assert.Equal(Messages.BadRequestHint, Messages.HintFor(listed));
        Assert.Equal(Messages.BadValueHint, Messages.HintFor(new BridgeError("BadRequest", "no")));
    }

    /// <summary>The timeout names the command that did not answer and how long it was waited for, since both decide what to
    /// do next.</summary>
    [Fact]
    public void ATimeout_NamesTheCommand_AndTheWait()
    {
        var error = BridgeError.Timeout(BridgeCommands.Ping, TimeSpan.FromSeconds(30));

        Assert.Contains("ping", error.Message, StringComparison.Ordinal);
        Assert.Contains("30", error.Message, StringComparison.Ordinal);
    }

    /// <summary>Every message built from values puts them in the sentence; a constant that came out empty would leave a
    /// tool answering with nothing.</summary>
    [Fact]
    public void EveryMessage_SaysSomething()
    {
        var empty = typeof(Messages)
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.FieldType == typeof(string) && string.IsNullOrWhiteSpace((string?)field.GetValue(null)))
            .Select(field => field.Name)
            .ToList();

        Assert.True(empty.Count == 0, $"messages with no text: {string.Join(", ", empty)}");
    }

    /// <summary>Every message built from values answers with a sentence, whichever way its values fall: a builder is a sentence with holes in
    /// it, and one that came out empty, or threw on a value a caller can pass, would leave a tool answering with nothing.</summary>
    /// <remarks>Called by reflection rather than one by one, so a message added without a test of its own is still called here. Each parameter
    /// is given both of the readings that change the sentence — a value and an empty one — because the ternaries inside these builders are the
    /// part that goes wrong.</remarks>
    [Fact]
    public void EveryMessageBuiltFromValues_AnswersWithASentence()
    {
        var builders = typeof(Messages)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(method => method.ReturnType == typeof(string) && !method.IsSpecialName)
            .Where(method => method.GetParameters().All(parameter => Samples(parameter.ParameterType).Count > 0));

        Assert.NotEmpty(builders);

        var empty = new List<string>();

        foreach (var builder in builders)
        {
            IReadOnlyList<IReadOnlyList<object?>> choices = [.. builder.GetParameters().Select(parameter => Samples(parameter.ParameterType))];

            foreach (var arguments in Combinations(choices))
            {
                if ((string?)builder.Invoke(null, [.. arguments]) is not { Length: > 0 })
                {
                    empty.Add(builder.Name);
                }
            }
        }

        Assert.True(empty.Count == 0, $"messages that came out empty: {string.Join(", ", empty.Distinct(StringComparer.Ordinal))}");
    }

    private static IReadOnlyList<object?> Samples(Type type) =>
        type == typeof(string) ? ["a value", string.Empty]
        : type == typeof(int) ? [0, 4]
        : type == typeof(long) ? [0L, 4096L]
        : type == typeof(bool) ? [true, false]
        : type == typeof(double) ? [0.0, 1.5]
        : type == typeof(TimeSpan) ? [TimeSpan.Zero, TimeSpan.FromSeconds(30)]
        : type == typeof(IEnumerable<string>) || type == typeof(IReadOnlyList<string>) ? [Array.Empty<string>(), new[] { "one", "two" }]
        : [];

    private static IEnumerable<IReadOnlyList<object?>> Combinations(IReadOnlyList<IReadOnlyList<object?>> choices)
    {
        IEnumerable<IReadOnlyList<object?>> rows = [[]];

        foreach (var choice in choices)
        {
            rows = rows.SelectMany(row => choice.Select(value => (IReadOnlyList<object?>)[.. row, value]));
        }

        return rows;
    }

    [Fact]
    public void MessagesBuiltFromValues_NameThem()
    {
        Assert.Contains("w1", Messages.UnknownWorker("w1"), StringComparison.Ordinal);
        Assert.Contains("stopping", Messages.WorkerNotReady("w1", "stopping"), StringComparison.Ordinal);
        Assert.Contains("j7", Messages.UnknownJob("j7"), StringComparison.Ordinal);
        Assert.Contains("j7", Messages.JobNotActive("j7", "done"), StringComparison.Ordinal);
        Assert.Contains("text", Messages.UnknownSkill("text"), StringComparison.Ordinal);
        Assert.Contains("text", Messages.KnownSkills(["text", "shapes"]), StringComparison.Ordinal);
        Assert.Contains("nowhere", Messages.NoSuchArea("nowhere"), StringComparison.Ordinal);
        Assert.Contains("files", Messages.PathOutsideArea("../x", "files"), StringComparison.Ordinal);
        Assert.Contains("plate.png", Messages.CaptureNotWritten("plate.png"), StringComparison.Ordinal);
        Assert.Contains("blur", Messages.NoToolFor("blur"), StringComparison.Ordinal);
        Assert.Contains("j7", Messages.CancelledFromThePage("j7"), StringComparison.Ordinal);
    }

    /// <summary>A server that ships skills says which; one that ships none says that instead of listing nothing.</summary>
    [Fact]
    public void TheSkillsHint_ReadsRight_WithSkillsAndWithout()
    {
        Assert.Contains("ae_skills", Messages.KnownSkills(["text"]), StringComparison.Ordinal);
        Assert.Contains("always on", Messages.KnownSkills([]), StringComparison.Ordinal);
    }

    /// <summary>Nothing deleted is said as such, and a deletion names what went.</summary>
    [Fact]
    public void TheDeletionNote_CountsWhatWent()
    {
        Assert.Contains("Nothing", Messages.DeletedFromThePage(0, "0 B"), StringComparison.Ordinal);
        Assert.Contains("entry", Messages.DeletedFromThePage(1, "1 KB"), StringComparison.Ordinal);
        Assert.Contains("entries", Messages.DeletedFromThePage(4, "4 KB"), StringComparison.Ordinal);
    }
}
