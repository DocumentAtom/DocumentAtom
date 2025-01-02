namespace DocumentAtom.Core.Image
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Extraction result.
    /// </summary>
    public class ExtractionResult
    {
        #region Public-Members

        /// <summary>
        /// Text elements.
        /// </summary>
        public List<TextElement> TextElements { get; set; }

        /// <summary>
        /// Table elements.
        /// </summary>
        public List<TableStructure> Tables { get; set; }

        /// <summary>
        /// List elements.
        /// </summary>
        public List<ListStructure> Lists { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Extraction result.
        /// </summary>
        public ExtractionResult()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
