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

        /// <summary>
        /// Produce a human-readable string of this object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(base.ToString());
            sb.Append("| Sheet name    : " + (!String.IsNullOrEmpty(SheetName) ? SheetName : "(null)") + Environment.NewLine);
            sb.Append("| Cell          : " + (!String.IsNullOrEmpty(CellIdentifier) ? CellIdentifier : "(null)") + Environment.NewLine);
            sb.Append("| Text          : " + (!String.IsNullOrEmpty(Text) ? Text : "(null)") + Environment.NewLine);

            if (Table != null)
            {
                sb.Append("| Data table    : " + Environment.NewLine);
                if (Table.Columns != null && Table.Columns.Count > 0)
                {
                    foreach (SerializableColumn item in Table.Columns)
                        sb.Append("  | Column : " + item.Name + Environment.NewLine);
                }

                if (Table.Rows != null && Table.Rows.Count > 0)
                {
                    sb.Append("  | Rows" + Environment.NewLine);

                    for (int i = 0; i < Table.Rows.Count; i++)
                    {
                        sb.Append("    | Row " + i + Environment.NewLine);

                        foreach (KeyValuePair<string, object> row in Table.Rows[i])
                            sb.Append("      | " + row.Key + " | " + (row.Value != null ? row.Value.ToString() : "(null)") + Environment.NewLine);
                    }
                }
            }

            if (Binary != null)
                sb.Append("| Binary        : " + Convert.ToBase64String(Binary) + Environment.NewLine);

            return sb.ToString();
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
