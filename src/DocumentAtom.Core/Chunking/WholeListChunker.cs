namespace DocumentAtom.Core.Chunking
{
    /// <summary>
    /// Treats the entire list as a single chunk.
    /// Serializes as numbered (ordered) or bulleted (unordered) list.
    /// </summary>
    public static class WholeListChunker
    {
        #region Public-Methods

        /// <summary>
        /// Serialize an entire list into a single chunk.
        /// </summary>
        /// <param name="items">List items.</param>
        /// <param name="ordered">True for numbered list, false for bulleted list.</param>
        /// <returns>List containing a single chunked text segment.</returns>
        public static List<string> Chunk(List<string> items, bool ordered)
        {
            if (items == null || items.Count == 0) return new List<string>();

            List<string> lines = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (ordered)
                    lines.Add((i + 1).ToString() + ". " + items[i]);
                else
                    lines.Add("- " + items[i]);
            }

            return new List<string> { string.Join("\n", lines) };
        }

        #endregion
    }
}
