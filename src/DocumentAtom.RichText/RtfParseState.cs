namespace DocumentAtom.RichText
{
    /// <summary>
    /// RTF parser state.
    /// </summary>
    internal enum RtfParseState
    {
        /// <summary>
        /// Normal parsing state.
        /// </summary>
        Normal,
        /// <summary>
        /// In header parsing state.
        /// </summary>
        InHeader,
        /// <summary>
        /// In list parsing state.
        /// </summary>
        InList,
        /// <summary>
        /// In table parsing state.
        /// </summary>
        InTable,
        /// <summary>
        /// In image parsing state.
        /// </summary>
        InImage,
        /// <summary>
        /// Skip group parsing state.
        /// </summary>
        SkipGroup
    }
}