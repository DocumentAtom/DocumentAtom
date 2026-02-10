namespace DocumentAtom.Documents.RichText
{
    /// <summary>
    /// RTF destination types.
    /// </summary>
    internal enum RtfDestination
    {
        /// <summary>
        /// Normal destination.
        /// </summary>
        Normal,
        /// <summary>
        /// Font table destination.
        /// </summary>
        FontTable,
        /// <summary>
        /// Color table destination.
        /// </summary>
        ColorTable,
        /// <summary>
        /// Style sheet destination.
        /// </summary>
        StyleSheet,
        /// <summary>
        /// Info destination.
        /// </summary>
        Info,
        /// <summary>
        /// Picture destination.
        /// </summary>
        Picture,
        /// <summary>
        /// List table destination.
        /// </summary>
        ListTable,
        /// <summary>
        /// List override table destination.
        /// </summary>
        ListOverrideTable,
        /// <summary>
        /// Ignored destination.
        /// </summary>
        IgnoredDestination,
        /// <summary>
        /// Shape instance destination.
        /// </summary>
        ShapeInst,
        /// <summary>
        /// Shape property destination.
        /// </summary>
        ShapeProperty,
        /// <summary>
        /// Unknown destination.
        /// </summary>
        UnknownDestination
    }
}