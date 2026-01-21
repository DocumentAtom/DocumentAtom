namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;

    /// <summary>
    /// Options for the AtomChunker.
    /// </summary>
    public class AtomChunkerOptions
    {
        #region Public-Members

        /// <summary>
        /// Maximum number of characters per chunk.
        /// Default is 1000.
        /// </summary>
        public int MaxChunkSize
        {
            get
            {
                return _MaxChunkSize;
            }
            set
            {
                if (value < 100) throw new ArgumentOutOfRangeException(nameof(MaxChunkSize), "MaxChunkSize must be at least 100.");
                _MaxChunkSize = value;
            }
        }

        /// <summary>
        /// Number of overlapping characters between chunks.
        /// Default is 100.
        /// </summary>
        public int ChunkOverlap
        {
            get
            {
                return _ChunkOverlap;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(ChunkOverlap), "ChunkOverlap cannot be negative.");
                _ChunkOverlap = value;
            }
        }

        /// <summary>
        /// Whether to preserve paragraph boundaries during chunking.
        /// Default is true.
        /// </summary>
        public bool PreserveParagraphs { get; set; } = true;

        /// <summary>
        /// Whether to include header context in chunks.
        /// Default is true.
        /// </summary>
        public bool IncludeHeaderContext { get; set; } = true;

        /// <summary>
        /// Separator to use between header context and content.
        /// Default is ": ".
        /// </summary>
        public string HeaderContextSeparator { get; set; } = ": ";

        /// <summary>
        /// Whether to include metadata from source elements.
        /// Default is true.
        /// </summary>
        public bool PreserveElementMetadata { get; set; } = true;

        /// <summary>
        /// Whether to use DocumentAtom quarks if available.
        /// Default is true.
        /// </summary>
        public bool UseQuarksIfAvailable { get; set; } = true;

        /// <summary>
        /// Minimum chunk size to produce.
        /// Chunks smaller than this will be merged with adjacent content.
        /// Default is 50.
        /// </summary>
        public int MinChunkSize
        {
            get
            {
                return _MinChunkSize;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MinChunkSize), "MinChunkSize cannot be negative.");
                _MinChunkSize = value;
            }
        }

        /// <summary>
        /// Separator pattern to use when splitting text.
        /// Default splits on paragraphs.
        /// </summary>
        public string[] SplitSeparators { get; set; } = new[] { "\n\n", "\n", ". ", " " };

        #endregion

        #region Private-Members

        private int _MaxChunkSize = 1000;
        private int _ChunkOverlap = 100;
        private int _MinChunkSize = 50;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Options for the AtomChunker.
        /// </summary>
        public AtomChunkerOptions()
        {
        }

        /// <summary>
        /// Create options optimized for RAG applications.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForRag()
        {
            return new AtomChunkerOptions
            {
                MaxChunkSize = 500,
                ChunkOverlap = 50,
                PreserveParagraphs = true,
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = true
            };
        }

        /// <summary>
        /// Create options optimized for summarization.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForSummarization()
        {
            return new AtomChunkerOptions
            {
                MaxChunkSize = 2000,
                ChunkOverlap = 200,
                PreserveParagraphs = true,
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = false
            };
        }

        /// <summary>
        /// Create options for large context windows.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForLargeContext()
        {
            return new AtomChunkerOptions
            {
                MaxChunkSize = 4000,
                ChunkOverlap = 400,
                PreserveParagraphs = true,
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = false
            };
        }

        #endregion
    }
}
