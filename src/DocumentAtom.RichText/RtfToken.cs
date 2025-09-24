namespace DocumentAtom.RichText
{
    /// <summary>
    /// Represents an RTF token.
    /// </summary>
    internal class RtfToken
    {
        /// <summary>
        /// Token type.
        /// </summary>
        public RtfTokenType Type { get; set; }

        /// <summary>
        /// Token text.
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// Parameter value for control words.
        /// </summary>
        public int? Parameter { get; set; }
    }
}