namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.DataIngestion.Metadata;

    /// <summary>
    /// Default chunker implementation for IngestionDocuments.
    /// Delegates content splitting to ChunkingEngine.
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
        private readonly ChunkingEngine _Engine;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Default chunker implementation for IngestionDocuments.
        /// </summary>
        /// <param name="options">Chunker options.</param>
        public AtomChunker(AtomChunkerOptions? options = null)
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
            int chunkIndex = 0;
            string? currentHeaderContext = null;

            foreach (IngestionDocumentElement element in document.Elements)
            {
                if (element.ElementType == IngestionElementType.Header && !string.IsNullOrEmpty(element.Content))
                {
                    currentHeaderContext = element.Content;
                }

                if (string.IsNullOrEmpty(element.Content))
                {
                    continue;
                }

                if (_Options.UseQuarksIfAvailable &&
                    element.Metadata.TryGetValue(AtomMetadataKeys.AtomHasQuarks, out object? hasQuarksObj) &&
                    hasQuarksObj is bool hasQuarks && hasQuarks)
                {
                    continue;
                }

                bool isQuark = element.Metadata.ContainsKey(AtomMetadataKeys.AtomParentAtomGuid);

                List<IngestionChunk> elementChunks = ChunkElement(
                    element,
                    document.Id,
                    chunkIndex,
                    currentHeaderContext,
                    isQuark);

                results.AddRange(elementChunks);
                chunkIndex += elementChunks.Count;
            }

            return results;
        }

        private List<IngestionChunk> ChunkElement(
            IngestionDocumentElement sourceElement,
            string documentId,
            int startIndex,
            string? headerContext,
            bool isQuark)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();
            string text = sourceElement.Content;

            if (string.IsNullOrEmpty(text))
            {
                return results;
            }

            if (isQuark)
            {
                IngestionChunk chunk = CreateChunk(text, documentId, startIndex, headerContext, sourceElement);
                chunk.Metadata[AtomMetadataKeys.ChunkSource] = "quark";
                results.Add(chunk);
                return results;
            }

            List<Chunk> engineChunks = _Engine.Chunk(
                AtomTypeEnum.Text,
                text,
                null,
                null,
                null,
                _Options.Chunking);

            int currentIndex = startIndex;

            foreach (Chunk engineChunk in engineChunks)
            {
                string chunkText = engineChunk.Text;

                if (!string.IsNullOrEmpty(chunkText) && chunkText.Length >= _Options.MinChunkSize)
                {
                    IngestionChunk ingestionChunk = CreateChunk(chunkText, documentId, currentIndex, headerContext, sourceElement);
                    ingestionChunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = engineChunk.Position;
                    results.Add(ingestionChunk);
                    currentIndex++;
                }
            }

            if (results.Count == 0 && !string.IsNullOrEmpty(text))
            {
                results.Add(CreateChunk(text, documentId, startIndex, headerContext, sourceElement));
            }

            return results;
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

            if (_Options.IncludeHeaderContext && !string.IsNullOrEmpty(headerContext))
            {
                chunk.Content = headerContext + _Options.HeaderContextSeparator + content;
                chunk.Metadata[AtomMetadataKeys.ChunkHeaderContext] = headerContext;
            }
            else
            {
                chunk.Content = content;
            }

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
