namespace DocumentAtom.DataIngestion.Processors
{
    using System;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Readers;

    /// <summary>
    /// Configuration options for AtomDocumentProcessor.
    /// </summary>
    public class AtomDocumentProcessorOptions
    {
        #region Public-Members

        /// <summary>
        /// Reader options for document processing.
        /// </summary>
        public AtomDocumentReaderOptions ReaderOptions { get; set; } = new();

        /// <summary>
        /// Chunker options for splitting documents.
        /// </summary>
        public AtomChunkerOptions ChunkerOptions { get; set; } = new();

        /// <summary>
        /// Whether to use hierarchy-aware chunking.
        /// Default is true.
        /// </summary>
        public bool UseHierarchyAwareChunking { get; set; } = true;

        /// <summary>
        /// Whether to remove duplicate chunks based on content hash.
        /// Default is false.
        /// </summary>
        public bool RemoveDuplicates { get; set; } = false;

        /// <summary>
        /// Whether to skip empty or whitespace-only chunks.
        /// Default is true.
        /// </summary>
        public bool SkipEmptyChunks { get; set; } = true;

        /// <summary>
        /// Minimum content length for a chunk to be included.
        /// Default is 10.
        /// </summary>
        public int MinimumChunkLength
        {
            get
            {
                return _MinimumChunkLength;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MinimumChunkLength));
                _MinimumChunkLength = value;
            }
        }

        #endregion

        #region Private-Members

        private int _MinimumChunkLength = 10;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Configuration options for AtomDocumentProcessor.
        /// </summary>
        public AtomDocumentProcessorOptions()
        {
        }

        /// <summary>
        /// Create options optimized for RAG applications.
        /// </summary>
        /// <returns>Processor options.</returns>
        public static AtomDocumentProcessorOptions ForRag()
        {
            return new AtomDocumentProcessorOptions
            {
                ChunkerOptions = AtomChunkerOptions.ForRag(),
                UseHierarchyAwareChunking = true,
                RemoveDuplicates = true,
                SkipEmptyChunks = true
            };
        }

        /// <summary>
        /// Create options optimized for summarization.
        /// </summary>
        /// <returns>Processor options.</returns>
        public static AtomDocumentProcessorOptions ForSummarization()
        {
            return new AtomDocumentProcessorOptions
            {
                ChunkerOptions = AtomChunkerOptions.ForSummarization(),
                UseHierarchyAwareChunking = true,
                RemoveDuplicates = false,
                SkipEmptyChunks = true
            };
        }

        #endregion
    }
}
