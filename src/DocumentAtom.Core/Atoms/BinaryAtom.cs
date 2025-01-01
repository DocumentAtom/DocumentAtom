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

        #endregion

        #region Private-Methods

        #endregion
    }
}
