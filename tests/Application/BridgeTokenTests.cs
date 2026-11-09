using Microsoft.Extensions.Logging;
using Snail.MCP.AfterEffects.Application.Access;
using Snail.MCP.AfterEffects.Configuration;

namespace Snail.MCP.AfterEffects.Tests.Application;

/// <summary>The secret the bridge presents, kept in a file the server writes on first use.</summary>
public sealed class BridgeTokenTests
{
    /// <summary>Servers that start at once share one token file, and the log says it was written by the one that wrote it, not by every one
    /// that found it missing a moment before another wrote it.</summary>
    [Fact]
    public async Task TokenFile_AskedForByServersStartingAtOnce_IsSaidToBeWrittenOnce()
    {
        var file = Path.Combine(TestFolder.Create(), "bridge.token");
        var logger = new WrittenLines();
        using var start = new Barrier(16);

        var tokens = await Task.WhenAll(Enumerable.Range(0, 16).Select(_ => Task.Run(() =>
        {
            var token = new BridgeToken(new BridgeOptions { TokenFile = file }, logger);
            start.SignalAndWait();

            return token.Value;
        })));

        Assert.Single(tokens.Distinct(StringComparer.Ordinal));
        Assert.Equal(1, logger.Count);
    }

    /// <summary>A logger that counts what it is told.</summary>
    private sealed class WrittenLines : ILogger<BridgeToken>
    {
        private int _count;

        public int Count => _count;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) =>
            Interlocked.Increment(ref _count);
    }
}
