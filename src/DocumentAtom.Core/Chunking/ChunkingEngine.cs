namespace DocumentAtom.Core.Chunking
{
    using DocumentAtom.Core.Enums;
    using SerializableDataTables;
    using SharpToken;
    using System.Data;

    /// <summary>
    /// Factory/dispatcher for chunking operations across different strategies and atom types.
    /// Routes chunking requests by atom type and strategy to the appropriate chunker implementation.
    /// </summary>
    public class ChunkingEngine
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly GptEncoding _Encoding;
        private readonly Action<SeverityEnum, string> _Logger;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initialize a new ChunkingEngine.
        /// </summary>
        /// <param name="logger">Optional logger callback.  May be null.</param>
        public ChunkingEngine(Action<SeverityEnum, string> logger = null)
        {
            _Encoding = GptEncoding.GetEncoding("cl100k_base");
            _Logger = logger;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Chunk content based on atom type and configuration.
        /// </summary>
        /// <param name="type">The type of the atom being chunked.</param>
        /// <param name="text">Text content (for text, code, hyperlink, meta, unknown atoms).</param>
        /// <param name="orderedList">Ordered list items (for list atoms).</param>
        /// <param name="unorderedList">Unordered list items (for list atoms).</param>
        /// <param name="table">Table data as list of rows (for table atoms).  Row 0 is assumed to be headers.</param>
        /// <param name="config">Chunking configuration.</param>
        /// <returns>List of Chunk objects with text, hashes, and position indices.</returns>
        public List<Chunk> Chunk(
            AtomTypeEnum type,
            string text,
            List<string> orderedList,
            List<string> unorderedList,
            List<List<string>> table,
            ChunkingConfiguration config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));

            List<string> rawChunks = GetRawChunks(type, text, orderedList, unorderedList, table, config);

            List<Chunk> results = new List<Chunk>();

            for (int i = 0; i < rawChunks.Count; i++)
            {
                string chunkText = rawChunks[i];

                if (!string.IsNullOrEmpty(config.ContextPrefix))
                    chunkText = config.ContextPrefix + chunkText;

                Chunk chunk = Chunking.Chunk.FromText(chunkText, i);
                results.Add(chunk);
            }

            return results;
        }

        /// <summary>
        /// Convert a SerializableDataTable to a List of List of strings.
        /// Row 0 contains column headers.
        /// </summary>
        /// <param name="sdt">SerializableDataTable to convert.</param>
        /// <returns>List of rows, each row being a list of cell values.</returns>
        public static List<List<string>> SerializableDataTableToList(SerializableDataTable sdt)
        {
            if (sdt == null) return new List<List<string>>();

            DataTable dt = sdt.ToDataTable();
            List<List<string>> result = new List<List<string>>();

            List<string> headers = new List<string>();
            foreach (DataColumn col in dt.Columns)
                headers.Add(col.ColumnName ?? string.Empty);
            result.Add(headers);

            foreach (DataRow row in dt.Rows)
            {
                List<string> rowData = new List<string>();
                foreach (object item in row.ItemArray)
                    rowData.Add(item?.ToString() ?? string.Empty);
                result.Add(rowData);
            }

            return result;
        }

        #endregion

        #region Private-Methods

        private List<string> GetRawChunks(
            AtomTypeEnum type,
            string text,
            List<string> orderedList,
            List<string> unorderedList,
            List<List<string>> table,
            ChunkingConfiguration config)
        {
            switch (type)
            {
                case AtomTypeEnum.Text:
                case AtomTypeEnum.Code:
                case AtomTypeEnum.Hyperlink:
                case AtomTypeEnum.Meta:
                    return ChunkText(text ?? string.Empty, config);

                case AtomTypeEnum.List:
                    return ChunkList(orderedList, unorderedList, config);

                case AtomTypeEnum.Table:
                    return ChunkTable(table, config);

                case AtomTypeEnum.Binary:
                case AtomTypeEnum.Image:
                    if (!string.IsNullOrEmpty(text))
                        return ChunkText(text, config);
                    return new List<string>();

                case AtomTypeEnum.Unknown:
                default:
                    if (!string.IsNullOrEmpty(text))
                        return ChunkText(text, config);
                    return new List<string>();
            }
        }

        private List<string> ChunkText(string text, ChunkingConfiguration config)
        {
            if (string.IsNullOrEmpty(text)) return new List<string>();

            switch (config.Strategy)
            {
                case ChunkStrategyEnum.FixedTokenCount:
                    return FixedTokenChunker.Chunk(text, config, _Encoding);

                case ChunkStrategyEnum.SentenceBased:
                    return SentenceChunker.Chunk(text, config, _Encoding);

                case ChunkStrategyEnum.ParagraphBased:
                    return ParagraphChunker.Chunk(text, config, _Encoding);

                case ChunkStrategyEnum.RegexBased:
                    return RegexChunker.Chunk(text, config, _Encoding);

                case ChunkStrategyEnum.WholeList:
                    return new List<string> { text };

                case ChunkStrategyEnum.ListEntry:
                    return text.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

                default:
                    return FixedTokenChunker.Chunk(text, config, _Encoding);
            }
        }

        private List<string> ChunkList(
            List<string> orderedList,
            List<string> unorderedList,
            ChunkingConfiguration config)
        {
            List<string> items = orderedList ?? unorderedList;
            if (items == null || items.Count == 0) return new List<string>();

            bool ordered = orderedList != null;

            switch (config.Strategy)
            {
                case ChunkStrategyEnum.WholeList:
                    return WholeListChunker.Chunk(items, ordered);

                case ChunkStrategyEnum.ListEntry:
                    return ListEntryChunker.Chunk(items);

                default:
                    string serialized = SerializeList(items, ordered);
                    return ChunkText(serialized, config);
            }
        }

        private List<string> ChunkTable(List<List<string>> table, ChunkingConfiguration config)
        {
            if (table == null || table.Count == 0) return new List<string>();

            switch (config.Strategy)
            {
                case ChunkStrategyEnum.Row:
                    return TableChunker.ChunkByRow(table);

                case ChunkStrategyEnum.RowWithHeaders:
                    return TableChunker.ChunkByRowWithHeaders(table);

                case ChunkStrategyEnum.RowGroupWithHeaders:
                    return TableChunker.ChunkByRowGroupWithHeaders(table, config.RowGroupSize);

                case ChunkStrategyEnum.KeyValuePairs:
                    return TableChunker.ChunkByKeyValuePairs(table);

                case ChunkStrategyEnum.WholeTable:
                    return TableChunker.ChunkWholeTable(table);

                default:
                    string serialized = SerializeTable(table);
                    return ChunkText(serialized, config);
            }
        }

        private string SerializeList(List<string> items, bool ordered)
        {
            List<string> lines = new List<string>();
            for (int i = 0; i < items.Count; i++)
            {
                if (ordered)
                    lines.Add((i + 1).ToString() + ". " + items[i]);
                else
                    lines.Add("- " + items[i]);
            }
            return string.Join("\n", lines);
        }

        private string SerializeTable(List<List<string>> table)
        {
            List<string> lines = new List<string>();

            for (int i = 0; i < table.Count; i++)
            {
                lines.Add("| " + string.Join(" | ", table[i]) + " |");

                if (i == 0)
                    lines.Add("|" + string.Join("|", table[i].Select(_ => "---")) + "|");
            }

            return string.Join("\n", lines);
        }

        #endregion
    }
}
