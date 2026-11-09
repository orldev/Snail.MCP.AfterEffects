namespace Snail.MCP.AfterEffects.Tools;

/// <summary>The numbers the tool descriptions promise; guard tests keep the prose equal to them.</summary>
internal static class ToolLimits
{
    public const int MaxTimeoutSeconds = 600;

    public static bool IsTimeout(int seconds) => seconds is >= 1 and <= MaxTimeoutSeconds;

    public static TimeSpan Timeout(int seconds) => TimeSpan.FromSeconds(seconds);
}

/// <summary>The bounds of run_program this server sets, which Snail.Sdk's run_program states in its description.</summary>
internal static class ProgramLimits
{
    /// <summary>Commands one program may send: a program is a way to spend one call, not to hide a thousand.</summary>
    public const int MostCalls = 200;

    /// <summary>Seconds a program runs when the call names no time.</summary>
    public const int DefaultTimeoutSeconds = 120;
}
