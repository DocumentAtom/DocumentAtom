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

        /// <summary>
        /// Alternative overlap expressed as a number of characters, converted to an approximate token
        /// overlap by the tokenizer.  When set, takes precedence over OverlapCount but not over OverlapPercentage.
        /// Overlap precedence is OverlapPercentage, then OverlapCharacters, then OverlapCount.
        /// Default is null (disabled).  Minimum is 0.
        /// </summary>
        public int? OverlapCharacters
        {
            get
            {
                return _OverlapCharacters;
            }
            set
            {
                if (value.HasValue && value.Value < 0)
                    throw new ArgumentOutOfRangeException(nameof(OverlapCharacters), "OverlapCharacters must be at least 0.");
                _OverlapCharacters = value;
            }
        }

        /// <summary>
        /// Tokenizer family used to measure chunk sizes.
        /// Default is Auto, which resolves the tokenizer from ModelId (falling back to cl100k_base).
        /// </summary>
        public TokenizerKindEnum TokenizerKind
        {
            get
            {
                return _TokenizerKind;
            }
            set
            {
                _TokenizerKind = value;
            }
        }

        /// <summary>
        /// Optional model identifier (for example "text-embedding-3-small" or "all-minilm") used to
        /// resolve the tokenizer family and token budget when TokenizerKind is Auto.
        /// Default is null.
        /// </summary>
        public string? ModelId
        {
            get
            {
                return _ModelId;
            }
            set
            {
                _ModelId = value;
            }
        }

        /// <summary>
        /// Content format that selects the separator ladder used by the Recursive strategy.
        /// Ignored unless Strategy is Recursive and Separators is null or empty.
        /// Default is Plain.
        /// </summary>
        public ContentFormatEnum Format
        {
            get
            {
                return _Format;
            }
            set
            {
                _Format = value;
            }
        }

        /// <summary>
        /// Explicit ordered ladder of separators used by the Recursive strategy.
        /// When set (non-empty), overrides Format.
        /// Default is null.
        /// </summary>
        public List<string>? Separators
        {
            get
            {
                return _Separators;
            }
            set
            {
                _Separators = value;
            }
        }

        /// <summary>
        /// When true, markdown header hierarchy is honored: content is grouped by heading and each
        /// chunk is stamped with its header breadcrumb.  Applies to text content only.
        /// Default is false.
        /// </summary>
        public bool HierarchyAware
        {
            get
            {
                return _HierarchyAware;
            }
            set
            {
                _HierarchyAware = value;
            }
        }

        /// <summary>
        /// When true, the header breadcrumb is prepended into each chunk's text (its token cost is
        /// charged against the token budget).  Only meaningful when HierarchyAware is true.
        /// Default is false.
        /// </summary>
        public bool ContextualizeHeaders
        {
            get
            {
                return _ContextualizeHeaders;
            }
            set
            {
                _ContextualizeHeaders = value;
            }
        }

        /// <summary>
        /// Separator used to join header breadcrumb segments (for example "Guide &gt; Setup &gt; Windows").
        /// Default is " &gt; ".  May not be null.
        /// </summary>
        public string HeaderContextSeparator
        {
            get
            {
                return _HeaderContextSeparator;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(HeaderContextSeparator));
                _HeaderContextSeparator = value;
            }
        }

        /// <summary>
        /// Policy for chunks smaller than MinChunkTokens.
        /// Default is Keep.
        /// </summary>
        public SmallChunkModeEnum SmallChunkMode
        {
            get
            {
                return _SmallChunkMode;
            }
            set
            {
                _SmallChunkMode = value;
            }
        }

        /// <summary>
        /// Minimum chunk size in tokens.  When greater than zero and SmallChunkMode is not Keep,
        /// undersized chunks are merged or dropped per SmallChunkMode.
        /// Default is 0 (disabled).  Minimum is 0.
        /// </summary>
        public int MinChunkTokens
        {
            get
            {
                return _MinChunkTokens;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MinChunkTokens), "MinChunkTokens must be at least 0.");
                _MinChunkTokens = value;
            }
        }

        /// <summary>
        /// When true, leading and trailing whitespace is trimmed from each chunk (skipped when the trim
        /// would raise the chunk's token count under byte-pair tokenization).
        /// Default is true.
        /// </summary>
        public bool TrimWhitespace
        {
            get
            {
                return _TrimWhitespace;
            }
            set
            {
                _TrimWhitespace = value;
            }
        }

        /// <summary>
        /// When true, each chunk's token count is computed and populated.
        /// Default is true.
        /// </summary>
        public bool ComputeTokenCounts
        {
            get
            {
                return _ComputeTokenCounts;
            }
            set
            {
                _ComputeTokenCounts = value;
            }
        }

        /// <summary>
        /// When true, each chunk's start and end character offsets into the source text are computed
        /// where the chunk is a literal substring of the source.
        /// Default is true.
        /// </summary>
        public bool ComputeOffsets
        {
            get
            {
                return _ComputeOffsets;
            }
            set
            {
                _ComputeOffsets = value;
            }
        }

        /// <summary>
        /// When true, MD5, SHA1, and SHA256 hashes of each chunk's text are computed and populated.
        /// Default is true.
        /// </summary>
        public bool ComputeHashes
        {
            get
            {
                return _ComputeHashes;
            }
            set
            {
                _ComputeHashes = value;
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
        private int? _OverlapCharacters = null;
        private TokenizerKindEnum _TokenizerKind = TokenizerKindEnum.Auto;
        private string? _ModelId = null;
        private ContentFormatEnum _Format = ContentFormatEnum.Plain;
        private List<string>? _Separators = null;
        private bool _HierarchyAware = false;
        private bool _ContextualizeHeaders = false;
        private string _HeaderContextSeparator = " > ";
        private SmallChunkModeEnum _SmallChunkMode = SmallChunkModeEnum.Keep;
        private int _MinChunkTokens = 0;
        private bool _TrimWhitespace = true;
        private bool _ComputeTokenCounts = true;
        private bool _ComputeOffsets = true;
        private bool _ComputeHashes = true;

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
