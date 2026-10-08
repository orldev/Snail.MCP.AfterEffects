namespace Snail.MCP.AfterEffects.Domain;

/// <summary>A group of tools loaded on demand, so the always-on catalog stays small enough for a model to choose from.</summary>
internal sealed record Skill(string Name, string Description, IReadOnlyList<Type> ToolTypes);

/// <summary>What the model sees of a skill: whether it is loaded and which tools it brings.</summary>
internal sealed record SkillState(string Name, string Description, bool IsEnabled, IReadOnlyList<string> Tools);

/// <summary>Names of the skills this server ships; each is a folder under Tools with classes marked by the skill attribute.</summary>
internal static class Skills
{
    public const string Animation = "animation";

    public const string Text = "text";

    public const string Shapes = "shapes";

    public const string Effects = "effects";

    public const string ThreeD = "threed";

    public const string Compositing = "compositing";

    public const string Rendering = "rendering";

    public const string Farm = "farm";

    public const string Templates = "templates";

    public const string Transfer = "transfer";

    public const string Layout = "layout";

    public const string Settings = "settings";

    public const string Viewer = "viewer";

    public const string Workers = "workers";
}
