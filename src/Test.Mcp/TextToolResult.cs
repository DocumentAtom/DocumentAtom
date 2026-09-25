namespace Test.Mcp
{
    using System.Collections.Generic;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Typed view of an MCP tools/call result whose content blocks are text.
    /// </summary>
    public sealed class TextToolResult
    {
        /// <summary>
        /// Content blocks returned by the tool.
        /// </summary>
        [JsonPropertyName("content")]
        public List<TextBlock> Content { get; set; } = new List<TextBlock>();

        /// <summary>
        /// True if the tool reported an execution error.
        /// </summary>
        [JsonPropertyName("isError")]
        public bool IsError { get; set; } = false;
    }
}
