namespace DocumentAtom.Core.Enums
{
    /// <summary>
    /// Chunking strategy for breaking content into chunks.
    /// </summary>
    public enum ChunkStrategyEnum
    {
        /// <summary>
        /// Fixed-size token chunks using a sliding window.
        /// </summary>
        FixedTokenCount = 0,

        /// <summary>
        /// Split at sentence boundaries, group to token budget.
        /// </summary>
        SentenceBased = 1,

        /// <summary>
        /// Split at paragraph boundaries, group to token budget.
        /// </summary>
        ParagraphBased = 2,

        /// <summary>
        /// Split at user-supplied regex pattern.
        /// </summary>
        RegexBased = 3,

        /// <summary>
        /// Entire list as a single chunk.
        /// </summary>
        WholeList = 4,

        /// <summary>
        /// Each list item as its own chunk.
        /// </summary>
        ListEntry = 5,

        /// <summary>
        /// Each table data row as space-separated values (no headers).
        /// </summary>
        Row = 6,

        /// <summary>
        /// Each data row as a markdown table with headers.
        /// </summary>
        RowWithHeaders = 7,

        /// <summary>
        /// Groups of N rows with headers as markdown tables.
        /// </summary>
        RowGroupWithHeaders = 8,

        /// <summary>
        /// Each row as key-value pairs: "col1: val1, col2: val2".
        /// </summary>
        KeyValuePairs = 9,

        /// <summary>
        /// Entire table as a single markdown table chunk.
        /// </summary>
        WholeTable = 10
    }
}
