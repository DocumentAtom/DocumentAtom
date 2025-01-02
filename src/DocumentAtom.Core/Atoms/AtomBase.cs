using DocumentAtom.Core.Enums;
using System.Text;

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
        public List<T> Quarks { get; set; } = null;

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
        /// Produce a human-readable string of this object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Atom" + Environment.NewLine);
            sb.Append("| GUID          : " + GUID.ToString() + Environment.NewLine);
            sb.Append("| Type          : " + Type.ToString() + Environment.NewLine);

            if (PageNumber != null)
                sb.Append("| Page Number   : " + PageNumber.ToString() + Environment.NewLine);

            if (Position != null)
                sb.Append("| Position      : " + Position.ToString() + Environment.NewLine);

            sb.Append("| Length        : " + Length.ToString() + Environment.NewLine);
            sb.Append("| MD5 hash      : " + Convert.ToBase64String(MD5Hash) + Environment.NewLine);
            sb.Append("| SHA1 hash     : " + Convert.ToBase64String(SHA1Hash) + Environment.NewLine);
            sb.Append("| SHA256 hash   : " + Convert.ToBase64String(SHA256Hash) + Environment.NewLine);

            if (Quarks != null && Quarks.Count > 0)
            {
                sb.Append("| Quarks        : " + Quarks.Count + Environment.NewLine);
                foreach (T quark in Quarks)
                {
                    sb.Append(quark.ToString());
                }
            }

            return sb.ToString();
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
