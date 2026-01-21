namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.DataIngestion.Metadata;

    /// <summary>
    /// Default chunker implementation for IngestionDocuments.
    /// </summary>
    public class AtomChunker : IAtomChunker
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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Default chunker implementation for IngestionDocuments.
        /// </summary>
        /// <param name="options">Chunker options.</param>
        public AtomChunker(AtomChunkerOptions? options = null)
        {
            _Options = options ?? new AtomChunkerOptions();
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
            int chunkIndex = 0;
            string? currentHeaderContext = null;

            foreach (IngestionDocumentElement element in document.Elements)
            {
                // Track header context
                if (element.ElementType == IngestionElementType.Header && !string.IsNullOrEmpty(element.Content))
                {
                    currentHeaderContext = element.Content;
                }

                // Skip elements without content
                if (string.IsNullOrEmpty(element.Content))
                {
                    continue;
                }

                // Check if element has quarks and we should use them
                if (_Options.UseQuarksIfAvailable &&
                    element.Metadata.TryGetValue(AtomMetadataKeys.AtomHasQuarks, out object? hasQuarksObj) &&
                    hasQuarksObj is bool hasQuarks && hasQuarks)
                {
                    continue;
                }

                // Check if this is a quark element
                bool isQuark = element.Metadata.ContainsKey(AtomMetadataKeys.AtomParentAtomGuid);

                // Chunk the element content
                List<IngestionChunk> elementChunks = ChunkText(
                    element.Content,
                    document.Id,
                    chunkIndex,
                    currentHeaderContext,
                    element,
                    isQuark);

                results.AddRange(elementChunks);
                chunkIndex += elementChunks.Count;
            }

            return results;
        }

        private List<IngestionChunk> ChunkText(
            string text,
            string documentId,
            int startIndex,
            string? headerContext,
            IngestionDocumentElement sourceElement,
            bool isQuark)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();
            int currentIndex = startIndex;

            if (string.IsNullOrEmpty(text))
            {
                return results;
            }

            // For quarks, use content directly without re-chunking
            if (isQuark)
            {
                IngestionChunk chunk = CreateChunk(text, documentId, currentIndex, headerContext, sourceElement);
                chunk.Metadata[AtomMetadataKeys.ChunkSource] = "quark";
                results.Add(chunk);
                return results;
            }

            // Check if content fits in a single chunk
            string contextPrefix = _Options.IncludeHeaderContext && !string.IsNullOrEmpty(headerContext)
                ? headerContext + _Options.HeaderContextSeparator
                : string.Empty;

            if (text.Length + contextPrefix.Length <= _Options.MaxChunkSize)
            {
                results.Add(CreateChunk(text, documentId, currentIndex, headerContext, sourceElement));
                return results;
            }

            // Split content into chunks
            List<string> segments = SplitText(text);
            StringBuilder currentChunk = new StringBuilder();
            int splitIndex = 0;

            foreach (string segment in segments)
            {
                // Check if adding this segment would exceed max size
                int projectedLength = currentChunk.Length + segment.Length + contextPrefix.Length;

                if (projectedLength > _Options.MaxChunkSize && currentChunk.Length > 0)
                {
                    // Emit current chunk
                    string chunkText = currentChunk.ToString().Trim();

                    if (chunkText.Length >= _Options.MinChunkSize)
                    {
                        IngestionChunk chunk = CreateChunk(chunkText, documentId, currentIndex++, headerContext, sourceElement);
                        chunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = splitIndex++;
                        results.Add(chunk);
                    }

                    // Start new chunk with overlap
                    currentChunk.Clear();

                    if (_Options.ChunkOverlap > 0 && chunkText.Length > _Options.ChunkOverlap)
                    {
                        string overlap = chunkText.Substring(chunkText.Length - _Options.ChunkOverlap);
                        currentChunk.Append(overlap);
                    }
                }

                currentChunk.Append(segment);

                // Add back separator that may have been lost
                if (!segment.EndsWith(" ") && !segment.EndsWith("\n"))
                {
                    currentChunk.Append(" ");
                }
            }

            // Emit final chunk
            if (currentChunk.Length > 0)
            {
                string finalText = currentChunk.ToString().Trim();

                if (finalText.Length >= _Options.MinChunkSize)
                {
                    IngestionChunk chunk = CreateChunk(finalText, documentId, currentIndex, headerContext, sourceElement);
                    chunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = splitIndex;
                    results.Add(chunk);
                }
            }

            return results;
        }

        private List<string> SplitText(string text)
        {
            List<string> segments = new List<string>();

            // Try to split by each separator in order of preference
            if (_Options.PreserveParagraphs)
            {
                foreach (string separator in _Options.SplitSeparators)
                {
                    if (text.Contains(separator))
                    {
                        string[] parts = text.Split(new[] { separator }, StringSplitOptions.None);

                        for (int i = 0; i < parts.Length; i++)
                        {
                            string part = parts[i];

                            if (i < parts.Length - 1)
                            {
                                part += separator; // Preserve separator for context
                            }

                            if (!string.IsNullOrWhiteSpace(part))
                            {
                                segments.Add(part);
                            }
                        }

                        if (segments.Count > 1)
                        {
                            return segments;
                        }

                        segments.Clear();
                    }
                }
            }

            // Fallback: split by words
            string[] words = text.Split(' ');

            foreach (string word in words)
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    segments.Add(word + " ");
                }
            }

            return segments;
        }

        private IngestionChunk CreateChunk(
            string content,
            string documentId,
            int index,
            string? headerContext,
            IngestionDocumentElement sourceElement)
        {
            IngestionChunk chunk = new IngestionChunk
            {
                DocumentId = documentId,
                ChunkIndex = index
            };

            // Add header context if configured
            if (_Options.IncludeHeaderContext && !string.IsNullOrEmpty(headerContext))
            {
                chunk.Content = headerContext + _Options.HeaderContextSeparator + content;
                chunk.Metadata[AtomMetadataKeys.ChunkHeaderContext] = headerContext;
            }
            else
            {
                chunk.Content = content;
            }

            // Copy selected metadata from source element
            if (_Options.PreserveElementMetadata)
            {
                string[] keysToPreserve = new[]
                {
                    AtomMetadataKeys.AtomType,
                    AtomMetadataKeys.AtomPageNumber,
                    AtomMetadataKeys.AtomPosition,
                    AtomMetadataKeys.AtomGuid,
                    AtomMetadataKeys.AtomParentAtomGuid,
                    AtomMetadataKeys.AtomDocumentGuid,
                    AtomMetadataKeys.SourceFilename,
                    AtomMetadataKeys.SourceDocumentType
                };

                foreach (string key in keysToPreserve)
                {
                    if (sourceElement.Metadata.TryGetValue(key, out object? value))
                    {
                        chunk.Metadata[key] = value;
                    }
                }
            }

            chunk.Metadata[AtomMetadataKeys.ChunkIndex] = index;
            chunk.Metadata[AtomMetadataKeys.ChunkSource] = "element";

            return chunk;
        }

        #endregion
    }
}
