using DocumentAtom.Core.Enums;

namespace DocumentAtom.Core.Atoms
{
    /// <summary>
    /// An atom is a small, self-contained unit of content from a document.
    /// </summary>
    public class AtomBase<T>
    {
        #region Public-Members

        /// <summary>
        /// GUID.
        /// </summary>
        public Guid GUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The type of the content for this atom.
        /// </summary>
        public AtomTypeEnum Type { get; set; } = AtomTypeEnum.Text;

        /// <summary>
        /// Page number.
        /// </summary>
        public int? PageNumber
        {
            get
            {
                return _PageNumber;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(PageNumber));
                _PageNumber = value;
            }
        }

        /// <summary>
        /// The ordinal position of the atom.
        /// </summary>
        public int? Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(Position));
                _Position = value;
            }
        }

        /// <summary>
        /// The length of the atom content.
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
        /// MD5 hash of the content.
        /// </summary>
        public byte[] MD5Hash { get; set; } = null;

        /// <summary>
        /// SHA1 hash of the content.
        /// </summary>
        public byte[] SHA1Hash { get; set; } = null;

        /// <summary>
        /// SHA256 hash of the content.
        /// </summary>
        public byte[] SHA256Hash { get; set; } = null;

        /// <summary>
        /// A quark is a subset of the content from an atom, used when intentionally breaking content into smaller chunks.
        /// </summary>
        public List<T> Quarks { get; set; } = new List<T>();

        #endregion

        #region Private-Members

        private int? _PageNumber = null;
        private int? _Position = null;
        private int _Length = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// An atom is a small, self-contained unit of content from a document.
        /// </summary>
        public AtomBase()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Remove binary data from a string.
        /// </summary>
        /// <param name="input">String.</param>
        /// <returns>String.</returns>
        public string RemoveBinaryData(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            return new string(input.Where(c =>
                !char.IsControl(c) ||
                char.IsWhiteSpace(c)).ToArray());
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
