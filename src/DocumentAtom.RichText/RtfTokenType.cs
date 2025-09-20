namespace DocumentAtom.RichText
{
    /// <summary>
    /// Type of RTF token.
    /// </summary>
    internal enum RtfTokenType
    {
        /// <summary>
        /// Control word token.
        /// </summary>
        ControlWord,
        /// <summary>
        /// Control symbol token.
        /// </summary>
        ControlSymbol,
        /// <summary>
        /// Text token.
        /// </summary>
        Text,
        /// <summary>
        /// Group start token.
        /// </summary>
        GroupStart,
        /// <summary>
        /// Group end token.
        /// </summary>
        GroupEnd
    }
}