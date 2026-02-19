namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;

    /// <summary>
    /// Options for the AtomChunker.
    /// </summary>
    public class AtomChunkerOptions
    {
        #region Public-Members

        /// <summary>
        /// Chunking configuration controlling the strategy, token budget, and overlap.
        /// Default uses FixedTokenCount strategy with 256 tokens.
        /// </summary>
        public ChunkingConfiguration Chunking
        {
            get
            {
                return _Chunking;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Chunking));
                _Chunking = value;
            }
        }

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
        /// Minimum chunk size in characters to produce.
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

        #endregion

        #region Private-Members

        private ChunkingConfiguration _Chunking = new ChunkingConfiguration
        {
            Enable = true,
            Strategy = ChunkStrategyEnum.FixedTokenCount,
            FixedTokenCount = 256,
            OverlapCount = 0
        };

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
        /// Uses sentence-based chunking with 256 tokens and 2-sentence overlap.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForRag()
        {
            return new AtomChunkerOptions
            {
                Chunking = new ChunkingConfiguration
                {
                    Enable = true,
                    Strategy = ChunkStrategyEnum.SentenceBased,
                    FixedTokenCount = 256,
                    OverlapCount = 2,
                    OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware
                },
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = true
            };
        }

        /// <summary>
        /// Create options optimized for summarization.
        /// Uses paragraph-based chunking with 1024 tokens and 1-paragraph overlap.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForSummarization()
        {
            return new AtomChunkerOptions
            {
                Chunking = new ChunkingConfiguration
                {
                    Enable = true,
                    Strategy = ChunkStrategyEnum.ParagraphBased,
                    FixedTokenCount = 1024,
                    OverlapCount = 1
                },
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = false
            };
        }

        /// <summary>
        /// Create options for large context windows.
        /// Uses paragraph-based chunking with 2048 tokens and 1-paragraph overlap.
        /// </summary>
        /// <returns>Chunker options.</returns>
        public static AtomChunkerOptions ForLargeContext()
        {
            return new AtomChunkerOptions
            {
                Chunking = new ChunkingConfiguration
                {
                    Enable = true,
                    Strategy = ChunkStrategyEnum.ParagraphBased,
                    FixedTokenCount = 2048,
                    OverlapCount = 1
                },
                IncludeHeaderContext = true,
                UseQuarksIfAvailable = false
            };
        }

        #endregion
    }
}
