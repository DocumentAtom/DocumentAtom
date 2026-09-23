namespace DocumentAtom.Core.Chunking
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Data;
    using DocumentAtom.Core.Diagnostics;
    using DocumentAtom.Core.Enums;
    using SerializableDataTables;
    using TcChunk = TextChunker.Models.Chunk;
    using TcChunker = TextChunker.Chunking.Chunker;
    using TcOptions = TextChunker.Models.ChunkingOptions;

    /// <summary>
    /// Factory/dispatcher for chunking operations across different strategies and atom types.
    /// Routes chunking requests by atom type and strategy to the TextChunker engine, which performs
    /// the tokenization and splitting.
    /// </summary>
    public class ChunkingEngine
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly TcChunker _Chunker;
        private readonly Action<SeverityEnum, string> _Logger;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initialize a new ChunkingEngine.
        /// </summary>
        /// <param name="logger">Optional logger callback.  May be null.</param>
        public ChunkingEngine(Action<SeverityEnum, string> logger = null)
        {
            _Chunker = new TcChunker();
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

            long startTicks = Stopwatch.GetTimestamp();
            string outcome = "ok";
            List<Chunk> results = new List<Chunk>();

            using (Activity? activity = DocumentAtomDiagnostics.CoreActivitySource.StartActivity(
                "documentatom.chunking",
                ActivityKind.Internal))
            {
                activity?.SetTag("documentatom.atom.type", type.ToString());
                activity?.SetTag("documentatom.chunk.strategy", config.Strategy.ToString());

                try
                {
                    TcOptions options = BuildOptions(config);
                    List<TcChunk> produced = Produce(type, text, orderedList, unorderedList, table, options);

                    for (int i = 0; i < produced.Count; i++)
                        results.Add(MapChunk(produced[i], i));

                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return results;
                }
                catch (Exception e)
                {
                    outcome = "error";
                    DocumentAtomDiagnostics.RecordException(activity, e);
                    throw;
                }
                finally
                {
                    DocumentAtomDiagnostics.RecordChunking(
                        type.ToString(),
                        config.Strategy.ToString(),
                        outcome,
                        results.Count,
                        DocumentAtomDiagnostics.GetElapsedSeconds(startTicks));
                }
            }
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

        private List<TcChunk> Produce(
            AtomTypeEnum type,
            string text,
            List<string> orderedList,
            List<string> unorderedList,
            List<List<string>> table,
            TcOptions options)
        {
            switch (type)
            {
                case AtomTypeEnum.List:
                    return ProduceList(orderedList, unorderedList, options);

                case AtomTypeEnum.Table:
                    return ProduceTable(table, options);

                case AtomTypeEnum.Text:
                case AtomTypeEnum.Code:
                case AtomTypeEnum.Hyperlink:
                case AtomTypeEnum.Meta:
                case AtomTypeEnum.Binary:
                case AtomTypeEnum.Image:
                case AtomTypeEnum.Unknown:
                default:
                    if (string.IsNullOrEmpty(text)) return new List<TcChunk>();
                    return new List<TcChunk>(_Chunker.Chunk(text, options));
            }
        }

        private List<TcChunk> ProduceList(List<string> orderedList, List<string> unorderedList, TcOptions options)
        {
            List<string> items = orderedList ?? unorderedList;
            if (items == null || items.Count == 0) return new List<TcChunk>();

            bool ordered = orderedList != null;
            return Collect(_Chunker.ChunkList(items, ordered, options));
        }

        private List<TcChunk> ProduceTable(List<List<string>> table, TcOptions options)
        {
            if (table == null || table.Count == 0) return new List<TcChunk>();

            List<IReadOnlyList<string>> rows = new List<IReadOnlyList<string>>(table.Count);
            foreach (List<string> row in table) rows.Add(row);

            return Collect(_Chunker.ChunkTable(rows, options));
        }

        private static Chunk MapChunk(TcChunk source, int position)
        {
            return new Chunk
            {
                GUID = source.GUID,
                ParentGUID = source.ParentGUID,
                Position = position,
                Length = source.CharacterCount,
                TokenCount = source.TokenCount,
                StartOffset = source.StartOffset,
                EndOffset = source.EndOffset,
                HeaderContext = source.HeaderContext,
                Text = source.Text,
                MD5Hash = source.MD5Hash,
                SHA1Hash = source.SHA1Hash,
                SHA256Hash = source.SHA256Hash
            };
        }

        private static TcOptions BuildOptions(ChunkingConfiguration config)
        {
            return new TcOptions
            {
                Strategy = MapStrategy(config.Strategy),
                MaxTokens = config.FixedTokenCount,
                OverlapCount = config.OverlapCount,
                OverlapPercentage = config.OverlapPercentage,
                OverlapCharacters = config.OverlapCharacters,
                OverlapStrategy = MapOverlapStrategy(config.OverlapStrategy),
                RowGroupSize = config.RowGroupSize,
                RegexPattern = config.RegexPattern,
                ContextPrefix = config.ContextPrefix,
                TokenizerKind = MapTokenizerKind(config.TokenizerKind),
                ModelId = config.ModelId,
                Format = MapFormat(config.Format),
                Separators = config.Separators,
                HierarchyAware = config.HierarchyAware,
                ContextualizeHeaders = config.ContextualizeHeaders,
                HeaderContextSeparator = config.HeaderContextSeparator,
                SmallChunkMode = MapSmallChunkMode(config.SmallChunkMode),
                MinChunkTokens = config.MinChunkTokens,
                TrimWhitespace = config.TrimWhitespace,
                ComputeTokenCounts = config.ComputeTokenCounts,
                ComputeOffsets = config.ComputeOffsets,
                ComputeHashes = config.ComputeHashes
            };
        }

        private static TextChunker.Enums.ChunkStrategyEnum MapStrategy(ChunkStrategyEnum strategy)
        {
            switch (strategy)
            {
                case ChunkStrategyEnum.FixedTokenCount: return TextChunker.Enums.ChunkStrategyEnum.FixedTokenCount;
                case ChunkStrategyEnum.SentenceBased: return TextChunker.Enums.ChunkStrategyEnum.SentenceBased;
                case ChunkStrategyEnum.ParagraphBased: return TextChunker.Enums.ChunkStrategyEnum.ParagraphBased;
                case ChunkStrategyEnum.RegexBased: return TextChunker.Enums.ChunkStrategyEnum.RegexBased;
                case ChunkStrategyEnum.WholeList: return TextChunker.Enums.ChunkStrategyEnum.WholeList;
                case ChunkStrategyEnum.ListEntry: return TextChunker.Enums.ChunkStrategyEnum.ListEntry;
                case ChunkStrategyEnum.Row: return TextChunker.Enums.ChunkStrategyEnum.Row;
                case ChunkStrategyEnum.RowWithHeaders: return TextChunker.Enums.ChunkStrategyEnum.RowWithHeaders;
                case ChunkStrategyEnum.RowGroupWithHeaders: return TextChunker.Enums.ChunkStrategyEnum.RowGroupWithHeaders;
                case ChunkStrategyEnum.KeyValuePairs: return TextChunker.Enums.ChunkStrategyEnum.KeyValuePairs;
                case ChunkStrategyEnum.WholeTable: return TextChunker.Enums.ChunkStrategyEnum.WholeTable;
                case ChunkStrategyEnum.Recursive: return TextChunker.Enums.ChunkStrategyEnum.Recursive;
                default: return TextChunker.Enums.ChunkStrategyEnum.FixedTokenCount;
            }
        }

        private static TextChunker.Enums.OverlapStrategyEnum MapOverlapStrategy(OverlapStrategyEnum strategy)
        {
            switch (strategy)
            {
                case OverlapStrategyEnum.SlidingWindow: return TextChunker.Enums.OverlapStrategyEnum.SlidingWindow;
                case OverlapStrategyEnum.SentenceBoundaryAware: return TextChunker.Enums.OverlapStrategyEnum.SentenceBoundaryAware;
                case OverlapStrategyEnum.SemanticBoundaryAware: return TextChunker.Enums.OverlapStrategyEnum.SemanticBoundaryAware;
                default: return TextChunker.Enums.OverlapStrategyEnum.SlidingWindow;
            }
        }

        private static TextChunker.Enums.TokenizerKindEnum MapTokenizerKind(TokenizerKindEnum kind)
        {
            switch (kind)
            {
                case TokenizerKindEnum.Cl100kBase: return TextChunker.Enums.TokenizerKindEnum.Cl100kBase;
                case TokenizerKindEnum.O200kBase: return TextChunker.Enums.TokenizerKindEnum.O200kBase;
                case TokenizerKindEnum.BertWordPiece: return TextChunker.Enums.TokenizerKindEnum.BertWordPiece;
                case TokenizerKindEnum.Auto:
                default: return TextChunker.Enums.TokenizerKindEnum.Auto;
            }
        }

        private static TextChunker.Enums.ContentFormatEnum MapFormat(ContentFormatEnum format)
        {
            switch (format)
            {
                case ContentFormatEnum.Markdown: return TextChunker.Enums.ContentFormatEnum.Markdown;
                case ContentFormatEnum.CSharp: return TextChunker.Enums.ContentFormatEnum.CSharp;
                case ContentFormatEnum.Python: return TextChunker.Enums.ContentFormatEnum.Python;
                case ContentFormatEnum.JavaScript: return TextChunker.Enums.ContentFormatEnum.JavaScript;
                case ContentFormatEnum.Java: return TextChunker.Enums.ContentFormatEnum.Java;
                case ContentFormatEnum.Plain:
                default: return TextChunker.Enums.ContentFormatEnum.Plain;
            }
        }

        private static TextChunker.Enums.SmallChunkModeEnum MapSmallChunkMode(SmallChunkModeEnum mode)
        {
            switch (mode)
            {
                case SmallChunkModeEnum.MergeForward: return TextChunker.Enums.SmallChunkModeEnum.MergeForward;
                case SmallChunkModeEnum.Drop: return TextChunker.Enums.SmallChunkModeEnum.Drop;
                case SmallChunkModeEnum.Keep:
                default: return TextChunker.Enums.SmallChunkModeEnum.Keep;
            }
        }

        private static List<TcChunk> Collect(IAsyncEnumerable<TcChunk> source)
        {
            List<TcChunk> list = new List<TcChunk>();
            IAsyncEnumerator<TcChunk> enumerator = source.GetAsyncEnumerator();

            try
            {
                while (enumerator.MoveNextAsync().AsTask().GetAwaiter().GetResult())
                    list.Add(enumerator.Current);
            }
            finally
            {
                enumerator.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }

            return list;
        }

        #endregion
    }
}
