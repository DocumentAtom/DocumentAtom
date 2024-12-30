namespace DocumentAtom.Core
{
    /// <summary>
    /// A text atom is a self-contained unit of text from a document.
    /// </summary>
    public class TextAtom : AtomBase
    {
        #region Public-Members

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A text atom is a self-contained unit of text from a document.
        /// </summary>
        public TextAtom()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
