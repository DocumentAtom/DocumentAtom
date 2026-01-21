namespace DocumentAtom.DataIngestion
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a chunk of content ready for embedding and vector storage.
    /// </summary>
    public class IngestionChunk
    {
        #region Public-Members

        /// <summary>
        /// Unique identifier for this chunk.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The document ID this chunk belongs to.
        /// </summary>
        public string? DocumentId { get; set; } = null;

        /// <summary>
        /// Index of this chunk within the document.
        /// </summary>
        public int ChunkIndex { get; set; } = 0;

        /// <summary>
        /// Text content of the chunk.
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Chunk-level metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Embedding vector, if computed.
        /// </summary>
        public float[]? Embedding { get; set; } = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Represents a chunk of content ready for embedding and vector storage.
        /// </summary>
        public IngestionChunk()
        {
        }

        /// <summary>
        /// Represents a chunk of content ready for embedding and vector storage.
        /// </summary>
        /// <param name="content">Text content.</param>
        /// <param name="documentId">Document ID.</param>
        /// <param name="chunkIndex">Chunk index.</param>
        public IngestionChunk(string content, string? documentId = null, int chunkIndex = 0)
        {
            Content = content ?? string.Empty;
            DocumentId = documentId;
            ChunkIndex = chunkIndex;
        }

        #endregion
    }
}
