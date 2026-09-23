namespace DocumentAtom.Core.Chunking
{
    using DocumentAtom.Core.Helpers;
    using System.Text;

    /// <summary>
    /// A chunk is a content fragment produced by running a chunking strategy on an atom's content.
    /// Unlike quarks (which are structural child atoms), chunks are lightweight text segments
    /// with positional tracking and content hashes.
    /// </summary>
    public class Chunk
    {
        #region Public-Members

        /// <summary>
        /// Globally unique identifier for this chunk.
        /// </summary>
        public Guid GUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// Identifier of the parent this chunk was produced from, when known.
        /// Default is null.
        /// </summary>
        public Guid? ParentGUID { get; set; } = null;

        /// <summary>
        /// Ordinal index (0, 1, 2, ...) within the parent atom's chunk list.
        /// Must be greater than or equal to zero.
        /// </summary>
        public int Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Position));
                _Position = value;
            }
        }

        /// <summary>
        /// Content length in characters.
        /// Must be greater than or equal to zero.
        /// </summary>
        public int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Length));
                _Length = value;
            }
        }

        /// <summary>
        /// Number of tokens in the chunk text as measured by the configured tokenizer.
        /// Zero when token counting is disabled.  Must be greater than or equal to zero.
        /// </summary>
        public int TokenCount
        {
            get
            {
                return _TokenCount;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(TokenCount));
                _TokenCount = value;
            }
        }

        /// <summary>
        /// Inclusive start character offset of the chunk within the source text, or -1 when the chunk
        /// is not a literal substring of the source (for example serialized list or table content) or
        /// when offset computation is disabled.
        /// </summary>
        public int StartOffset { get; set; } = -1;

        /// <summary>
        /// Exclusive end character offset of the chunk within the source text, or -1 when the chunk is
        /// not a literal substring of the source or when offset computation is disabled.
        /// </summary>
        public int EndOffset { get; set; } = -1;

        /// <summary>
        /// Header breadcrumb describing where this chunk sits in the document hierarchy
        /// (for example "Guide &gt; Setup &gt; Windows"), or null when hierarchy tracking is not in effect.
        /// </summary>
        public string HeaderContext { get; set; } = null;

        /// <summary>
        /// MD5 hash of the text content.
        /// </summary>
        public byte[] MD5Hash { get; set; } = null;

        /// <summary>
        /// SHA1 hash of the text content.
        /// </summary>
        public byte[] SHA1Hash { get; set; } = null;

        /// <summary>
        /// SHA256 hash of the text content.
        /// </summary>
        public byte[] SHA256Hash { get; set; } = null;

        /// <summary>
        /// Chunk text content.
        /// </summary>
        public string Text { get; set; } = null;

        #endregion

        #region Private-Members

        private int _Position = 0;
        private int _Length = 0;
        private int _TokenCount = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A chunk is a content fragment produced by running a chunking strategy on an atom's content.
        /// </summary>
        public Chunk()
        {

        }

        /// <summary>
        /// Create a chunk from text content with computed hashes.
        /// </summary>
        /// <param name="text">Text content.</param>
        /// <param name="position">Ordinal position within the parent atom's chunk list.</param>
        /// <returns>Chunk with computed hashes.</returns>
        public static Chunk FromText(string text, int position)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            return new Chunk
            {
                Position = position,
                Length = text.Length,
                Text = text,
                MD5Hash = HashHelper.MD5Hash(bytes),
                SHA1Hash = HashHelper.SHA1Hash(bytes),
                SHA256Hash = HashHelper.SHA256Hash(bytes)
            };
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Produce a human-readable string of this object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Chunk" + Environment.NewLine);
            sb.Append("| GUID          : " + GUID.ToString() + Environment.NewLine);

            if (ParentGUID != null)
                sb.Append("| Parent GUID   : " + ParentGUID.ToString() + Environment.NewLine);

            sb.Append("| Position      : " + Position.ToString() + Environment.NewLine);
            sb.Append("| Length        : " + Length.ToString() + Environment.NewLine);
            sb.Append("| Token count   : " + TokenCount.ToString() + Environment.NewLine);

            if (StartOffset >= 0)
                sb.Append("| Offsets       : " + StartOffset.ToString() + "-" + EndOffset.ToString() + Environment.NewLine);

            if (!string.IsNullOrEmpty(HeaderContext))
                sb.Append("| Header context: " + HeaderContext + Environment.NewLine);

            if (MD5Hash != null)
                sb.Append("| MD5 hash      : " + Convert.ToBase64String(MD5Hash) + Environment.NewLine);

            if (SHA1Hash != null)
                sb.Append("| SHA1 hash     : " + Convert.ToBase64String(SHA1Hash) + Environment.NewLine);

            if (SHA256Hash != null)
                sb.Append("| SHA256 hash   : " + Convert.ToBase64String(SHA256Hash) + Environment.NewLine);

            return sb.ToString();
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
