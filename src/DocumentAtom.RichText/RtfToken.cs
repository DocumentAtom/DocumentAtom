namespace DocumentAtom.RichText
{
    /// <summary>
    /// Represents a token in RTF parsing.
    /// </summary>
    internal class RtfToken
    {
        /// <summary>
        /// Type of the token.
        /// </summary>
        public RtfTokenType Type { get; set; }

        /// <summary>
        /// Text content of the token.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Optional parameter value for control words.
        /// </summary>
        public int? Parameter { get; set; }
    }
}