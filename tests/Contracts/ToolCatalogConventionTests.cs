using System.ComponentModel;
using System.Reflection;
using System.Text.RegularExpressions;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Snail.MCP.AfterEffects.Adapters.Programs;
using Snail.MCP.AfterEffects.Application.Discovery;
using Snail.MCP.AfterEffects.Application.Skills;
using Snail.MCP.AfterEffects.Domain;
using Snail.MCP.AfterEffects.Domain.Commands;
using Snail.MCP.AfterEffects.Tools;
using Snail.MCP.AfterEffects.Tools.Prompts;

namespace Snail.MCP.AfterEffects.Tests.Contracts;

/// <summary>Prompt catalog guards: descriptions are hardcoded consts that drift silently as the code evolves; these tests catch drift at build time.</summary>
public partial class ToolCatalogConventionTests
{
    private static IReadOnlyList<MethodInfo> ToolMethods() =>
        [.. typeof(ToolDescriptions).Assembly.GetTypes()
            .Where(type => type.GetCustomAttribute<McpServerToolTypeAttribute>() is not null)
            .SelectMany(type => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            .Where(method => method.GetCustomAttribute<McpServerToolAttribute>() is not null)];

    private static string ToolName(MethodInfo method) => method.GetCustomAttribute<McpServerToolAttribute>()!.Name!;

    private static string? DescriptionOf(ICustomAttributeProvider member) =>
        member.GetCustomAttributes(typeof(DescriptionAttribute), false)
            .OfType<DescriptionAttribute>()
            .FirstOrDefault()?.Description;

    /// <summary>Catalog constants except the Parameters group, which holds parameter descriptions.</summary>
    private static Dictionary<string, string> CatalogEntries() =>
        typeof(ToolDescriptions).GetNestedTypes()
            .Where(group => group.Name != nameof(ToolDescriptions.Parameters))
            .SelectMany(group => group.GetFields(BindingFlags.Public | BindingFlags.Static))
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .ToDictionary(field => $"{field.DeclaringType!.Name}.{field.Name}", field => (string)field.GetRawConstantValue()!);

    [Fact]
    public void EveryTool_HasItsDescription_InTheCatalog()
    {
        var catalog = CatalogEntries().Values.ToHashSet(StringComparer.Ordinal);

        var offenders = ToolMethods()
            .Where(method => DescriptionOf(method) is not { Length: > 0 } description || !catalog.Contains(description))
            .Select(ToolName)
            .ToList();

        Assert.True(offenders.Count == 0,
            $"description missing or not from ToolDescriptions: {string.Join(", ", offenders)}");
    }

    [Fact]
    public void Catalog_HasNoEntry_WithoutATool()
    {
        var used = ToolMethods().Select(DescriptionOf).ToHashSet(StringComparer.Ordinal);

        var orphans = CatalogEntries()
            .Where(entry => !used.Contains(entry.Value))
            .Select(entry => entry.Key)
            .ToList();

        Assert.True(orphans.Count == 0, $"constants without a tool: {string.Join(", ", orphans)}");
    }

    [Fact]
    public void EveryParameter_IsDescribed()
    {
        var offenders = ToolMethods()
            .SelectMany(method => method.GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken) && parameter.ParameterType != typeof(IProgress<ProgressNotificationValue>) && parameter.ParameterType != typeof(McpServer))
                .Where(parameter => string.IsNullOrWhiteSpace(DescriptionOf(parameter)))
                .Select(parameter => $"{ToolName(method)}({parameter.Name})"))
            .ToList();

        Assert.True(offenders.Count == 0, $"parameters without a description: {string.Join(", ", offenders)}");
    }

    /// <summary>A tool named after After Effects' work carries the prefix; a tool about the fleet's own machinery does not.</summary>
    /// <remarks>The prefix tells this server's tools apart in a list that holds the tools of many servers. find_tool, run_tools and
    /// run_program are not After Effects: they are how any server of the fleet is asked what it has and how several of its tools are spent in
    /// one call, and the other servers spell them exactly so.</remarks>
    [Fact]
    public void A_tool_named_after_after_effects_carries_the_prefix_and_the_machinery_does_not()
    {
        string[] machinery = ["find_tool", "run_tools", "run_program"];
        var names = ToolMethods().Select(ToolName).ToList();

        Assert.All(names.Except(machinery), name => Assert.Matches("^ae_[a-z0-9_]+$", name));
        Assert.Equal(names.Count, names.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Descriptions_ReferenceOnlyExistingTools()
    {
        var known = ToolMethods().Select(ToolName).ToHashSet(StringComparer.Ordinal);

        var dangling = ToolMethods()
            .SelectMany(method => ToolReference.Matches(DescriptionOf(method) ?? string.Empty)
                .Select(match => match.Value)
                .Where(reference => !known.Contains(reference))
                .Select(reference => $"{ToolName(method)} → {reference}"))
            .Distinct()
            .ToList();

        Assert.True(dangling.Count == 0, $"references to non-existent tools: {string.Join(", ", dangling)}");
    }

    /// <summary>A recipe names tools by name; a prompt that names one that does not exist sends the model down a dead end.</summary>
    [Fact]
    public void Prompts_ReferenceOnlyExistingTools()
    {
        var known = ToolMethods().Select(ToolName).ToHashSet(StringComparer.Ordinal);
        var texts = typeof(PromptTexts).GetFields(BindingFlags.Public | BindingFlags.Static).Where(field => field.IsLiteral).Select(field => (string)field.GetRawConstantValue()!);

        var unknown = texts
            .SelectMany(text => AnyToolName.Matches(text).Select(match => match.Value))
            .Where(name => !known.Contains(name))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        Assert.True(unknown.Count == 0, $"prompts name tools that do not exist: {string.Join(", ", unknown)}");
    }

    [Fact]
    public void ServerGuidance_ReferencesOnlyExistingTools()
    {
        var known = ToolMethods().Select(ToolName).ToHashSet(StringComparer.Ordinal);

        var dangling = ToolReference.Matches(ServerGuidance.Instructions)
            .Select(match => match.Value)
            .Where(reference => !known.Contains(reference))
            .Distinct()
            .ToList();

        Assert.True(dangling.Count == 0, $"guidance names tools that do not exist: {string.Join(", ", dangling)}");
    }

    /// <summary>A description that promises something to the model is a message to the model, and a message that names a tool which does not
    /// exist teaches it a call that will fail.</summary>
    [Fact]
    public void Messages_ReferenceOnlyExistingTools()
    {
        var known = ToolMethods().Select(ToolName).ToHashSet(StringComparer.Ordinal);
        var texts = typeof(Messages).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => (string)field.GetRawConstantValue()!);

        var dangling = texts
            .SelectMany(text => ToolReference.Matches(text).Select(match => match.Value))
            .Where(reference => !known.Contains(reference))
            .Distinct()
            .ToList();

        Assert.True(dangling.Count == 0, $"messages name tools that do not exist: {string.Join(", ", dangling)}");
    }

    /// <summary>The read-only hint must say the same as the command schema the bridge reads: a tool whose commands are all read-only there is
    /// read-only here, and no other is.</summary>
    [Fact]
    public void ReadOnlyAnnotations_AgreeWithTheCommandSchema()
    {
        var readOnly = CommandsWhere("read_only");
        var sending = ToolMethods()
            .Select(method => (Tool: method.GetCustomAttribute<McpServerToolAttribute>()!, Commands: CommandsOf(method)))
            .Where(pair => pair.Commands.Count > 0);
        var mismatched = sending
            .Where(pair => pair.Tool.ReadOnly != pair.Commands.All(readOnly.Contains))
            .Select(pair => pair.Tool.Name)
            .ToList();

        Assert.True(mismatched.Count == 0, $"read-only hint disagrees with the command schema for: {string.Join(", ", mismatched)}");
    }

    /// <summary>Every tool states its destructive and open-world hints, and the command schema says which commands destroy and which reach beyond
    /// After Effects, so adding a command forces the question there rather than inheriting the protocol's default of destructive here.</summary>
    [Fact]
    public void DestructiveAnnotations_FollowTheCommandSchema()
    {
        var destructive = CommandsWhere("destructive");
        var openWorld = CommandsWhere("open_world");
        var changing = ToolMethods()
            .Select(method => (Tool: method.GetCustomAttribute<McpServerToolAttribute>()!, Commands: CommandsOf(method)))
            .Where(pair => pair.Commands.Count > 0 && !pair.Tool.ReadOnly);
        var mismatched = changing
            .Where(pair => pair.Tool.Destructive != pair.Commands.Any(destructive.Contains) || pair.Tool.OpenWorld != pair.Commands.Any(openWorld.Contains))
            .Select(pair => pair.Tool.Name)
            .ToList();

        Assert.True(mismatched.Count == 0, $"destructive or open-world hint disagrees with the command schema for: {string.Join(", ", mismatched)}");
    }

    /// <summary>Every command of the schema with the parameters it declares.</summary>
    private static Dictionary<string, HashSet<string>> CommandSchema()
    {
        var schema = JsonNode.Parse(File.ReadAllText(Path.Combine(ExtensionContractTests.ExtensionSource(), "commands.json")))!;

        return schema["commands"]!.AsObject().ToDictionary(
            command => command.Key,
            command => command.Value!["params"]!.AsArray().Select(parameter => parameter!.GetValue<string>()).ToHashSet(StringComparer.Ordinal),
            StringComparer.Ordinal);
    }

    /// <summary>The commands the schema marks with a flag.</summary>
    private static HashSet<string> CommandsWhere(string flag)
    {
        var schema = JsonNode.Parse(File.ReadAllText(Path.Combine(ExtensionContractTests.ExtensionSource(), "commands.json")))!;

        return [.. schema["commands"]!.AsObject()
            .Where(command => command.Value?[flag] is JsonValue value && value.TryGetValue<bool>(out var set) && set)
            .Select(command => command.Key)];
    }

    /// <summary>The bridge commands a tool method sends, read from its source: the attribute cannot name them, and the method body is the only place they appear.</summary>
    private static IReadOnlyList<string> CommandsOf(MethodInfo method)
    {
        var source = SourceOf(method.DeclaringType!);
        var name = method.GetCustomAttribute<McpServerToolAttribute>()!.Name!;
        var start = source.IndexOf($"Name = \"{name}\"", StringComparison.Ordinal);
        var next = ToolAttribute.Match(source[(start + 1)..]);
        var body = next.Success ? source[start..(start + 1 + next.Index)] : source[start..];
        var fields = CommandField.Matches(body).Select(match => match.Groups[1].Value).Distinct();

        return [.. fields.Select(field => ((BridgeCommand)typeof(BridgeCommands).GetField(field)!.GetValue(null)!).Name)];
    }

    private static readonly Dictionary<Type, string> Sources = [];

    private static string SourceOf(Type type)
    {
        lock (Sources)
        {
            if (!Sources.TryGetValue(type, out var text))
            {
                var file = Directory.EnumerateFiles(Path.Combine(Repository.Root, "src", "Tools"), $"{type.Name}.cs", SearchOption.AllDirectories).Single();
                text = File.ReadAllText(file);
                Sources[type] = text;
            }

            return text;
        }
    }

    [Fact]
    public void TimeoutParameters_StateTheLimit()
    {
        var parameters = ToolMethods().SelectMany(method => method.GetParameters().Where(parameter => parameter.Name == "timeoutSeconds")).ToList();

        Assert.All(parameters, parameter =>
            Assert.Contains(ToolLimits.MaxTimeoutSeconds.ToString(), DescriptionOf(parameter)!, StringComparison.Ordinal));
        Assert.Contains(ToolLimits.MaxTimeoutSeconds.ToString(), ToolDescriptions.Parameters.TimeoutSeconds, StringComparison.Ordinal);
    }

    /// <summary>What a tool declares it sends is what its body sends: the declaration feeds find_tool and run_program, the body the bridge.</summary>
    [Fact]
    public void SendsDeclarations_MatchTheCommandsInTheSource()
    {
        var mismatched = ToolMethods()
            .Where(method => !CommandsOf(method).Order(StringComparer.Ordinal).SequenceEqual(ToolCommands.Of(method).Order(StringComparer.Ordinal)))
            .Select(ToolName)
            .ToList();

        Assert.True(mismatched.Count == 0, $"[Sends] disagrees with the body for: {string.Join(", ", mismatched)}");
    }

    /// <summary>What find_tool says a command takes is what the bridge accepts: every tool parameter, in snake_case, is a parameter of the command it sends.</summary>
    [Fact]
    public void BridgeToolParameters_AreTheCommandsParameters_InSnakeCase()
    {
        var schema = CommandSchema();

        var strays = ToolMethods()
            .Where(method => ToolCommands.Of(method) is { Count: 1 })
            .SelectMany(method => method.GetParameters()
                .Where(parameter => parameter.ParameterType != typeof(CancellationToken) && parameter.Name != "timeoutSeconds")
                .Where(parameter => !schema[ToolCommands.Of(method)[0]].Contains(ToolIndex.WireName(parameter.Name!)))
                .Select(parameter => $"{ToolName(method)}.{parameter.Name}"))
            .ToList();

        Assert.True(strays.Count == 0, $"not a parameter of the command sent: {string.Join(", ", strays)}");
    }

    [Fact]
    public void ProgramCodeParameter_StatesTheCallLimit()
    {
        var code = Assert.Single(ParametersNamed("code"), parameter => ToolName((MethodInfo)parameter.Member) == "run_program");

        Assert.Contains(JavaScriptPrograms.MostCalls.ToString(), DescriptionOf(code)!, StringComparison.Ordinal);
    }

    [Fact]
    public void ClosedListParameters_NameEveryValue()
    {
        var kind = Assert.Single(ParametersNamed("kind"), parameter => ToolName((MethodInfo)parameter.Member) == "ae_add_layer");
        var areas = ParametersNamed("area");

        Assert.NotEmpty(areas);
        Assert.All(areas, parameter => Assert.All(global::Snail.MCP.AfterEffects.Application.Storage.VolumeAreas.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        var light = Assert.Single(ParametersNamed("lightType"));
        var blending = Assert.Single(ParametersNamed("blendingMode"));
        var matte = Assert.Single(ParametersNamed("trackMatteType"));
        var presets = ParametersNamed("preset").Where(parameter => ToolName((MethodInfo)parameter.Member).EndsWith("_keyframes", StringComparison.Ordinal)).ToList();
        var interpolations = ParametersNamed("inInterpolation").Concat(ParametersNamed("outInterpolation")).ToList();

        var animatorProperties = ParametersNamed("properties");
        var selectors = ParametersNamed("selector").Where(parameter => parameter.ParameterType == typeof(JsonObject)).ToList();

        var shapeKinds = ParametersNamed("kind").Where(parameter => ToolName((MethodInfo)parameter.Member) == "ae_add_shape").ToList();
        var maskModes = ParametersNamed("mode").Where(parameter => ToolName((MethodInfo)parameter.Member).EndsWith("_mask", StringComparison.Ordinal)).ToList();

        var styles = ParametersNamed("style");
        var falloffs = ParametersNamed("falloff");
        var timeDisplays = ParametersNamed("timeDisplay");
        var channels = ParametersNamed("channels");

        Assert.NotEmpty(channels);
        Assert.All(channels, parameter => Assert.All(ViewerChannels.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        var gpus = ParametersNamed("gpuAccel");
        var purges = ParametersNamed("target").Where(parameter => ToolName((MethodInfo)parameter.Member) == "ae_purge").ToList();

        Assert.NotEmpty(timeDisplays);
        Assert.NotEmpty(gpus);
        Assert.Single(purges);
        Assert.All(timeDisplays, parameter => Assert.All(TimeDisplays.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(gpus, parameter => Assert.All(GpuAccelerations.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(purges, parameter => Assert.All(PurgeTargets.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        var alphas = ParametersNamed("alpha");
        var postRenderActions = ParametersNamed("postRenderAction");

        Assert.NotEmpty(postRenderActions);
        Assert.All(postRenderActions, parameter => Assert.All(PostRenderActions.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        var fields = ParametersNamed("fields");

        Assert.NotEmpty(alphas);
        Assert.NotEmpty(fields);
        Assert.All(alphas, parameter => Assert.All(AlphaModes.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(fields, parameter => Assert.All(FieldSeparations.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        var shadowModes = ParametersNamed("castsShadows").Concat(ParametersNamed("acceptsShadows")).Where(parameter => parameter.ParameterType == typeof(string)).ToList();

        Assert.NotEmpty(falloffs);
        Assert.NotEmpty(shadowModes);
        Assert.All(falloffs, parameter => Assert.All(LightFalloffs.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(shadowModes, parameter => Assert.All(ShadowModes.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));

        Assert.NotEmpty(styles);
        Assert.All(styles, parameter => Assert.All(LayerStyles.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.Single(shapeKinds);
        Assert.NotEmpty(maskModes);
        Assert.All(shapeKinds, parameter => Assert.All(ShapeKinds.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(maskModes, parameter => Assert.All(MaskModes.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.NotEmpty(presets);
        Assert.NotEmpty(interpolations);
        Assert.NotEmpty(animatorProperties);
        Assert.NotEmpty(selectors);
        Assert.All(animatorProperties, parameter => Assert.All(TextAnimatorProperties.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(selectors, parameter => Assert.All(TextSelectorSettings.All, value => Assert.Contains(value, DescriptionOf(parameter)!, StringComparison.Ordinal)));
        Assert.All(presets, preset => Assert.All(EasingPresets.All, value => Assert.Contains(value, DescriptionOf(preset)!, StringComparison.Ordinal)));
        Assert.All(interpolations, interpolation => Assert.All(Interpolations.All, value => Assert.Contains(value, DescriptionOf(interpolation)!, StringComparison.Ordinal)));

        Assert.All(LayerKinds.All, value => Assert.Contains(value, DescriptionOf(kind)!, StringComparison.Ordinal));
        Assert.All(LightTypes.All, value => Assert.Contains(value, DescriptionOf(light)!, StringComparison.Ordinal));
        Assert.All(BlendingModes.All, value => Assert.Contains(value, DescriptionOf(blending)!, StringComparison.Ordinal));
        Assert.All(TrackMatteTypes.All, value => Assert.Contains(value, DescriptionOf(matte)!, StringComparison.Ordinal));
    }

    private static IReadOnlyList<ParameterInfo> ParametersNamed(string name) =>
        [.. ToolMethods().SelectMany(method => method.GetParameters().Where(parameter => parameter.Name == name))];

    [Fact]
    public void SkillClasses_CarryANonEmptyDescription()
    {
        var skills = typeof(ToolDescriptions).Assembly.GetTypes()
            .Select(type => type.GetCustomAttribute<SkillAttribute>())
            .OfType<SkillAttribute>()
            .ToList();

        Assert.All(skills, skill => Assert.False(string.IsNullOrWhiteSpace(skill.Description), $"skill {skill.Name} has no description"));
        Assert.All(skills, skill => Assert.Matches("^[a-z]+$", skill.Name));
    }

    [GeneratedRegex(@"\bae_[a-z0-9_]+")]
    private static partial Regex ToolReference { get; }

    [GeneratedRegex(@"\b(ae_[a-z0-9_]+|find_tool|run_tools|run_program)\b")]
    private static partial Regex AnyToolName { get; }

    [GeneratedRegex(@"\[McpServerTool\(")]
    private static partial Regex ToolAttribute { get; }

    [GeneratedRegex(@"BridgeCommands\.(\w+)")]
    private static partial Regex CommandField { get; }
}
