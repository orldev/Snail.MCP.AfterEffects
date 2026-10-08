namespace Snail.MCP.AfterEffects.Domain;

/// <summary>What a program did: what it returned, what it wrote to its log, and the commands it sent on the way.</summary>
internal sealed record ProgramRun(JsonNode? Result, IReadOnlyList<string> Log, IReadOnlyList<ProgramCall> Calls, BridgeError? Error);

/// <summary>One command a program sent, as its trace reports it.</summary>
internal sealed record ProgramCall(string Command, bool IsOk, int Milliseconds);
