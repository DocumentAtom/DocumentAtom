using System.Text;

namespace DocumentAtom.Core.Atoms
{
    /// <summary>
    /// A binary atom is a self-contained unit of binary content from a document.
    /// </summary>
    public class BinaryAtom : AtomBase<BinaryAtom>
    {
        #region Public-Members

        /// <summary>
        /// Binary content.
        /// </summary>
        public byte[] Bytes { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A binary atom is a self-contained unit of binary content from a document.
        /// </summary>
        public BinaryAtom()
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
            sb.Append(base.ToString());
            sb.Append("| Bytes         : " + (Bytes != null ? Convert.ToBase64String(Bytes) : "(null)") + Environment.NewLine);
            return sb.ToString();
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
