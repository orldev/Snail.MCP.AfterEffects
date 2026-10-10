namespace Snail.MCP.AfterEffects.Application.Skills;

/// <summary>Marks a tool class as part of a skill: a group of tools that joins the catalog on demand instead of being always on.</summary>
/// <remarks>The tool list a client holds costs tokens on every turn, so only the always-on layer is in it by default; a skill's tools arrive
/// when the skill is enabled, which is what the folder a tool class lives in and this attribute say together.</remarks>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
internal sealed class SkillAttribute(string name, string description) : Attribute
{
    public string Name => name;

    public string Description => description;
}
