namespace DocumentAtom.Core.Atoms
{
    using System.Data;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;

    /// <summary>
    /// An xlsx atom is a self-contained unit of information from a Microsoft Excel .xlsx workbook.
    /// </summary>
    public class XlsxAtom : AtomBase<MarkdownAtom>
    {
        #region Public-Members

        /// <summary>
        /// Sheet name, if any.
        /// </summary>
        public string SheetName { get; set; } = null;

        /// <summary>
        /// Cell identifier.
        /// </summary>
        public string CellIdentifier { get; set; } = null;

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// Data table.
        /// </summary>
        public SerializableDataTable Table { get; set; } = null;

        /// <summary>
        /// Binary data.
        /// </summary>
        public byte[] Binary { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A docx atom is a self-contained unit of information from a Microsoft Word .docx document.
        /// </summary>
        public XlsxAtom()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #region DataTable

        #endregion

        #endregion
    }
}
