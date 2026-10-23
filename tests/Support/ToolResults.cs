using ModelContextProtocol.Protocol;

namespace Snail.MCP.AfterEffects.Tests.Support;

/// <summary>What a test reads off a tool result: the text the model sees, and whether the protocol flagged it as an error.</summary>
internal static class ToolResults
{
    extension(CallToolResult result)
    {
        public string Text() => ((TextContentBlock)result.Content[0]).Text;

        public bool Failed() => result.IsError == true;

        /// <summary>The data of an answer that went well; a failed one is a test's own mistake, so it says what went wrong.</summary>
        public JsonObject Data()
        {
            var answer = (JsonObject)JsonNode.Parse(result.Text())!;

            Assert.True(answer["ok"]?.GetValue<bool>() == true, result.Text());

            return (JsonObject)answer["data"]!;
        }
    }
}
