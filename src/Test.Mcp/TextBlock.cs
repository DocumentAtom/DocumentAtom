namespace Test.Mcp
{
    using System.Text.Json.Serialization;

    /// <summary>
    /// A single content block within an MCP tools/call result.
    /// </summary>
    public sealed class TextBlock
    {
        /// <summary>
        /// Content block type, for example "text".
        /// </summary>
        [JsonPropertyName("type")]
        public string? Type { get; set; } = null;

        /// <summary>
        /// Text payload of the content block.
        /// </summary>
        [JsonPropertyName("text")]
        public string? Text { get; set; } = null;
    }
}
