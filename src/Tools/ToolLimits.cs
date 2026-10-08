namespace Snail.MCP.AfterEffects.Tools;

/// <summary>The numbers the tool descriptions promise; guard tests keep the prose equal to them.</summary>
internal static class ToolLimits
{
    public const int MaxTimeoutSeconds = 600;

    public static bool IsTimeout(int seconds) => seconds is >= 1 and <= MaxTimeoutSeconds;

    public static TimeSpan Timeout(int seconds) => TimeSpan.FromSeconds(seconds);
}
