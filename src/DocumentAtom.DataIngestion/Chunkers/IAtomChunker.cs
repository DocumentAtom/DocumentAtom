namespace DocumentAtom.DataIngestion.Chunkers
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// Interface for chunking ingestion documents.
    /// </summary>
    public interface IAtomChunker
    {
        /// <summary>
        /// Chunk an ingestion document into smaller pieces.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks.</returns>
        IAsyncEnumerable<IngestionChunk> ChunkAsync(IngestionDocument document, CancellationToken cancellationToken = default);

        /// <summary>
        /// Chunk an ingestion document into smaller pieces synchronously.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <returns>Enumerable of chunks.</returns>
        IEnumerable<IngestionChunk> Chunk(IngestionDocument document);
    }
}
