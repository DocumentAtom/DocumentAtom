namespace DocumentAtom.Core.Chunking
{
    using DocumentAtom.Core.Enums;

    /// <summary>
    /// Configuration for how content is chunked.
    /// Controls the chunking strategy, token budget, overlap, and strategy-specific parameters.
    /// </summary>
    public class ChunkingConfiguration
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable chunking.
        /// Default is false.
        /// </summary>
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Chunking strategy to use.
        /// Default is FixedTokenCount.
        /// </summary>
        public ChunkStrategyEnum Strategy
        {
            get
            {
                return _Strategy;
            }
            set
            {
                _Strategy = value;
            }
        }

        /// <summary>
        /// Number of tokens per chunk.
        /// Also used as the token budget for sentence-based and paragraph-based strategies.
        /// Default is 256.  Minimum is 1.
        /// </summary>
        public int FixedTokenCount
        {
            get
            {
                return _FixedTokenCount;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(FixedTokenCount), "FixedTokenCount must be at least 1.");
                _FixedTokenCount = value;
            }
        }

        /// <summary>
        /// Number of tokens, sentences, or paragraphs to overlap between chunks depending on strategy.
        /// Default is 0.  Minimum is 0.
        /// </summary>
        public int OverlapCount
        {
            get
            {
                return _OverlapCount;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(OverlapCount), "OverlapCount must be at least 0.");
                _OverlapCount = value;
            }
        }

        /// <summary>
        /// Alternative overlap as a percentage of chunk size (0.0 to 1.0).
        /// When set, takes precedence over OverlapCount.
        /// Default is null (disabled).
        /// </summary>
        public double? OverlapPercentage
        {
            get
            {
                return _OverlapPercentage;
            }
            set
            {
                if (value.HasValue && (value.Value < 0.0 || value.Value > 1.0))
                    throw new ArgumentOutOfRangeException(nameof(OverlapPercentage), "OverlapPercentage must be between 0.0 and 1.0.");
                _OverlapPercentage = value;
            }
        }

        /// <summary>
        /// Strategy for handling overlap boundaries.
        /// Default is SlidingWindow.
        /// </summary>
        public OverlapStrategyEnum OverlapStrategy
        {
            get
            {
                return _OverlapStrategy;
            }
            set
            {
                _OverlapStrategy = value;
            }
        }

        /// <summary>
        /// Number of rows per group for the RowGroupWithHeaders table strategy.
        /// Default is 5.  Minimum is 1.
        /// </summary>
        public int RowGroupSize
        {
            get
            {
                return _RowGroupSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(RowGroupSize), "RowGroupSize must be at least 1.");
                _RowGroupSize = value;
            }
        }

        /// <summary>
        /// Text prefix prepended to each chunk.
        /// Default is null (disabled).
        /// </summary>
        public string? ContextPrefix
        {
            get
            {
                return _ContextPrefix;
            }
            set
            {
                _ContextPrefix = value;
            }
        }

        /// <summary>
        /// Regular expression pattern used as a split delimiter for the RegexBased strategy.
        /// Required when Strategy is RegexBased.
        /// Default is null.
        /// </summary>
        public string? RegexPattern
        {
            get
            {
                return _RegexPattern;
            }
            set
            {
                _RegexPattern = value;
            }
        }

        #endregion

        #region Private-Members

        private ChunkStrategyEnum _Strategy = ChunkStrategyEnum.FixedTokenCount;
        private int _FixedTokenCount = 256;
        private int _OverlapCount = 0;
        private double? _OverlapPercentage = null;
        private OverlapStrategyEnum _OverlapStrategy = OverlapStrategyEnum.SlidingWindow;
        private int _RowGroupSize = 5;
        private string? _ContextPrefix = null;
        private string? _RegexPattern = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Configuration for how content is chunked.
        /// </summary>
        public ChunkingConfiguration()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
