namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.DataIngestion.Metadata;

    /// <summary>
    /// Chunker that preserves document hierarchy and context.
    /// Reconstructs a markdown view of the document and delegates to TextChunker's native
    /// hierarchy-aware, recursive chunking (via <see cref="ChunkingEngine"/>), which walks the
    /// header hierarchy and stamps each chunk with its header breadcrumb.
    /// </summary>
    public class HierarchyAwareChunker : IAtomChunker
    {
        #region Public-Members

        /// <summary>
        /// Chunker options.
        /// </summary>
        public AtomChunkerOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Options));
                _Options = value;
            }
        }

        #endregion

        #region Private-Members

        private AtomChunkerOptions _Options;
        private readonly ChunkingEngine _Engine;
        private const string _BreadcrumbSeparator = " > ";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Chunker that preserves document hierarchy and context.
        /// </summary>
        /// <param name="options">Chunker options.</param>
        public HierarchyAwareChunker(AtomChunkerOptions? options = null)
        {
            _Options = options ?? new AtomChunkerOptions();
            _Engine = new ChunkingEngine();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Chunk an ingestion document into smaller pieces.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks.</returns>
        public async IAsyncEnumerable<IngestionChunk> ChunkAsync(
            IngestionDocument document,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            List<IngestionChunk> chunks = await Task.Run(() =>
                ChunkInternal(document), cancellationToken).ConfigureAwait(false);

            foreach (IngestionChunk chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return chunk;
            }
        }

        /// <summary>
        /// Chunk an ingestion document into smaller pieces synchronously.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <returns>Enumerable of chunks.</returns>
        public IEnumerable<IngestionChunk> Chunk(IngestionDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return ChunkInternal(document);
        }

        #endregion

        #region Private-Methods

        private List<IngestionChunk> ChunkInternal(IngestionDocument document)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();

            string markdown = BuildMarkdown(document.Elements);
            if (string.IsNullOrWhiteSpace(markdown)) return results;

            ChunkingConfiguration config = BuildHierarchyConfiguration(_Options.Chunking);

            List<Chunk> engineChunks = _Engine.Chunk(
                AtomTypeEnum.Text,
                markdown,
                null,
                null,
                null,
                config);

            int chunkIndex = 0;

            foreach (Chunk engineChunk in engineChunks)
            {
                string chunkText = engineChunk.Text;

                if (string.IsNullOrEmpty(chunkText) || chunkText.Length < _Options.MinChunkSize)
                    continue;

                IngestionChunk chunk = new IngestionChunk
                {
                    DocumentId = document.Id,
                    ChunkIndex = chunkIndex++,
                    Content = chunkText
                };

                AddHierarchyMetadata(chunk, engineChunk);
                results.Add(chunk);
            }

            // If every produced chunk fell below the minimum size, emit the reconstructed content as a
            // single chunk so hierarchy chunking never silently drops a non-empty document.
            if (results.Count == 0)
            {
                string content = markdown.Trim();
                if (!string.IsNullOrEmpty(content))
                {
                    IngestionChunk chunk = new IngestionChunk
                    {
                        DocumentId = document.Id,
                        ChunkIndex = chunkIndex,
                        Content = content
                    };

                    chunk.Metadata[AtomMetadataKeys.ChunkSource] = "hierarchy";
                    results.Add(chunk);
                }
            }

            return results;
        }

        private string BuildMarkdown(List<IngestionDocumentElement> elements)
        {
            StringBuilder sb = new StringBuilder();

            foreach (IngestionDocumentElement element in elements)
            {
                if (string.IsNullOrEmpty(element.Content)) continue;

                if (element.ElementType == IngestionElementType.Header)
                {
                    int level = GetHeaderLevel(element);
                    if (level < 1) level = 1;
                    if (level > 6) level = 6;
                    sb.Append(new string('#', level));
                    sb.Append(' ');
                    sb.Append(element.Content);
                }
                else
                {
                    sb.Append(element.Content);
                }

                sb.Append("\n\n");
            }

            return sb.ToString();
        }

        private int GetHeaderLevel(IngestionDocumentElement element)
        {
            if (element.Metadata.TryGetValue(AtomMetadataKeys.AtomHeaderLevel, out object? levelObj))
            {
                if (levelObj is int level) return level;
                if (levelObj is long longLevel) return (int)longLevel;
                if (int.TryParse(levelObj?.ToString(), out int parsedLevel)) return parsedLevel;
            }

            return 1;
        }

        private ChunkingConfiguration BuildHierarchyConfiguration(ChunkingConfiguration source)
        {
            return new ChunkingConfiguration
            {
                Enable = true,
                Strategy = ChunkStrategyEnum.Recursive,
                Format = ContentFormatEnum.Markdown,
                HierarchyAware = true,
                ContextualizeHeaders = _Options.IncludeHeaderContext,
                HeaderContextSeparator = _BreadcrumbSeparator,
                FixedTokenCount = source.FixedTokenCount,
                OverlapCount = source.OverlapCount,
                OverlapPercentage = source.OverlapPercentage,
                OverlapCharacters = source.OverlapCharacters,
                OverlapStrategy = source.OverlapStrategy,
                TokenizerKind = source.TokenizerKind,
                ModelId = source.ModelId,
                TrimWhitespace = source.TrimWhitespace,
                SmallChunkMode = source.SmallChunkMode,
                MinChunkTokens = source.MinChunkTokens,
                ComputeTokenCounts = source.ComputeTokenCounts,
                ComputeOffsets = source.ComputeOffsets,
                ComputeHashes = source.ComputeHashes
            };
        }

        private void AddHierarchyMetadata(IngestionChunk chunk, Chunk engineChunk)
        {
            chunk.Metadata[AtomMetadataKeys.ChunkSource] = "hierarchy";
            chunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = engineChunk.Position;

            string? headerContext = engineChunk.HeaderContext;

            if (!string.IsNullOrEmpty(headerContext))
            {
                chunk.Metadata[AtomMetadataKeys.ChunkHeaderContext] = headerContext;

                string[] segments = headerContext.Split(new[] { _BreadcrumbSeparator }, StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length > 0)
                {
                    string title = segments[segments.Length - 1];
                    chunk.Metadata[AtomMetadataKeys.HierarchyLevel] = segments.Length;
                    chunk.Metadata[AtomMetadataKeys.SectionTitle] = title;
                    chunk.Metadata[AtomMetadataKeys.SectionLevel] = segments.Length;
                }
            }
        }

        #endregion
    }
}
