namespace DocumentAtom.Core.Chunking
{
    /// <summary>
    /// Each list item becomes its own chunk.
    /// Filters out whitespace-only items.
    /// </summary>
    public static class ListEntryChunker
    {
        #region Public-Methods

        /// <summary>
        /// Create one chunk per list item.
        /// </summary>
        /// <param name="items">List items to chunk.</param>
        /// <returns>List of chunked text segments, one per non-empty item.</returns>
        public static List<string> Chunk(List<string> items)
        {
            if (items == null || items.Count == 0) return new List<string>();
            return items.Where(item => !string.IsNullOrWhiteSpace(item)).ToList();
        }

        #endregion
    }
}
