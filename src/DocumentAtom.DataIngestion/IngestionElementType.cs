namespace DocumentAtom.DataIngestion
{
    /// <summary>
    /// Types of ingestion document elements.
    /// </summary>
    public enum IngestionElementType
    {
        /// <summary>
        /// Paragraph text.
        /// </summary>
        Paragraph,

        /// <summary>
        /// Header element.
        /// </summary>
        Header,

        /// <summary>
        /// Table element.
        /// </summary>
        Table,

        /// <summary>
        /// Image element.
        /// </summary>
        Image,

        /// <summary>
        /// List element.
        /// </summary>
        List,

        /// <summary>
        /// Code block element.
        /// </summary>
        Code,

        /// <summary>
        /// Binary/attachment element.
        /// </summary>
        Binary,

        /// <summary>
        /// Unknown element type.
        /// </summary>
        Unknown
    }
}
