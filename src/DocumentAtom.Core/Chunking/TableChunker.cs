namespace DocumentAtom.Core.Chunking
{
    /// <summary>
    /// Static methods for table-specific chunking strategies.
    /// All methods assume table[0] is the header row.
    /// Tables with 0 or 1 rows (header only) return empty.
    /// </summary>
    public static class TableChunker
    {
        #region Public-Methods

        /// <summary>
        /// Each data row as space-separated values (no headers).
        /// </summary>
        /// <param name="table">Table data where table[0] is the header row.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> ChunkByRow(List<List<string>> table)
        {
            if (table.Count <= 1) return new List<string>();
            List<string> chunks = new List<string>();
            for (int i = 1; i < table.Count; i++)
                chunks.Add(string.Join(" ", table[i]));
            return chunks;
        }

        /// <summary>
        /// Each data row as a markdown table with headers prepended.
        /// </summary>
        /// <param name="table">Table data where table[0] is the header row.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> ChunkByRowWithHeaders(List<List<string>> table)
        {
            if (table.Count <= 1) return new List<string>();
            List<string> headers = table[0];
            string headerLine = "| " + string.Join(" | ", headers) + " |";
            string separatorLine = "|" + string.Join("|", headers.Select(_ => "---")) + "|";
            List<string> chunks = new List<string>();
            for (int i = 1; i < table.Count; i++)
            {
                string rowLine = "| " + string.Join(" | ", table[i]) + " |";
                chunks.Add(headerLine + "\n" + separatorLine + "\n" + rowLine);
            }
            return chunks;
        }

        /// <summary>
        /// Groups of N data rows with headers prepended (markdown table format).
        /// </summary>
        /// <param name="table">Table data where table[0] is the header row.</param>
        /// <param name="groupSize">Number of data rows per group.  Minimum is 1.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> ChunkByRowGroupWithHeaders(List<List<string>> table, int groupSize)
        {
            if (table.Count <= 1) return new List<string>();
            if (groupSize < 1) groupSize = 1;
            List<string> headers = table[0];
            string headerLine = "| " + string.Join(" | ", headers) + " |";
            string separatorLine = "|" + string.Join("|", headers.Select(_ => "---")) + "|";
            List<string> chunks = new List<string>();
            for (int i = 1; i < table.Count; i += groupSize)
            {
                List<string> rowLines = new List<string>();
                for (int j = i; j < i + groupSize && j < table.Count; j++)
                    rowLines.Add("| " + string.Join(" | ", table[j]) + " |");
                chunks.Add(headerLine + "\n" + separatorLine + "\n" + string.Join("\n", rowLines));
            }
            return chunks;
        }

        /// <summary>
        /// Each data row as key-value pairs: "key1: val1, key2: val2, ...".
        /// </summary>
        /// <param name="table">Table data where table[0] is the header row.</param>
        /// <returns>List of chunked text segments.</returns>
        public static List<string> ChunkByKeyValuePairs(List<List<string>> table)
        {
            if (table.Count <= 1) return new List<string>();
            List<string> headers = table[0];
            List<string> chunks = new List<string>();
            for (int i = 1; i < table.Count; i++)
            {
                List<string> pairs = new List<string>();
                for (int j = 0; j < headers.Count && j < table[i].Count; j++)
                    pairs.Add(headers[j] + ": " + table[i][j]);
                chunks.Add(string.Join(", ", pairs));
            }
            return chunks;
        }

        /// <summary>
        /// Entire table as a single markdown table chunk.
        /// </summary>
        /// <param name="table">Table data where table[0] is the header row.</param>
        /// <returns>List containing a single chunked text segment.</returns>
        public static List<string> ChunkWholeTable(List<List<string>> table)
        {
            if (table.Count <= 1) return new List<string>();
            List<string> headers = table[0];
            string headerLine = "| " + string.Join(" | ", headers) + " |";
            string separatorLine = "|" + string.Join("|", headers.Select(_ => "---")) + "|";
            List<string> lines = new List<string> { headerLine, separatorLine };
            for (int i = 1; i < table.Count; i++)
                lines.Add("| " + string.Join(" | ", table[i]) + " |");
            return new List<string> { string.Join("\n", lines) };
        }

        #endregion
    }
}
