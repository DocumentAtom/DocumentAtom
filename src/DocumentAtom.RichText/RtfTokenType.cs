namespace DocumentAtom.RichText
{
    /// <summary>
    /// RTF token types.
    /// </summary>
    internal enum RtfTokenType
    {
        /// <summary>
        /// Group start token.
        /// </summary>
        GroupStart,
        /// <summary>
        /// Group end token.
        /// </summary>
        GroupEnd,
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
        Text
    }
}