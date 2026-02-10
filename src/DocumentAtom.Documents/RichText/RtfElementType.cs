namespace DocumentAtom.Documents.RichText
{
    /// <summary>
    /// Type of RTF document element.
    /// </summary>
    internal enum RtfElementType
    {
        /// <summary>
        /// Paragraph element.
        /// </summary>
        Paragraph,
        /// <summary>
        /// Header level 1 element.
        /// </summary>
        Header1,
        /// <summary>
        /// Header level 2 element.
        /// </summary>
        Header2,
        /// <summary>
        /// Header level 3 element.
        /// </summary>
        Header3,
        /// <summary>
        /// List item element.
        /// </summary>
        ListItem,
        /// <summary>
        /// List element.
        /// </summary>
        List,
        /// <summary>
        /// Table element.
        /// </summary>
        Table,
        /// <summary>
        /// Table cell element.
        /// </summary>
        TableCell
    }
}