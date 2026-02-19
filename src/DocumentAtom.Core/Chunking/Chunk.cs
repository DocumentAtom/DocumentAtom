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
        /// Content length.
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
            sb.Append("| Position      : " + Position.ToString() + Environment.NewLine);
            sb.Append("| Length        : " + Length.ToString() + Environment.NewLine);

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
