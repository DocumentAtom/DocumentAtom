namespace DocumentAtom.Core.Atoms
{
    using System.Data;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;

    /// <summary>
    /// A pptx atom is a self-contained unit of information from a Microsoft PowerPoint .pptx presentation.
    /// </summary>
    public class PptxAtom : AtomBase<MarkdownAtom>
    {
        #region Public-Members

        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; set; } = null;

        /// <summary>
        /// Subtitle.
        /// </summary>
        public string Subtitle { get; set; } = null;

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// Unordered list elements.
        /// </summary>
        public List<string> UnorderedList { get; set; } = null;

        /// <summary>
        /// Ordered list elements.
        /// </summary>
        public List<string> OrderedList { get; set; } = null;

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
        /// A pptx atom is a self-contained unit of information from a Microsoft PowerPoint .pptx presentation.
        /// </summary>
        public PptxAtom()
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
            sb.Append("| Text          : " + (!String.IsNullOrEmpty(Text) ? Text : "(null)") + Environment.NewLine);

            if (!String.IsNullOrEmpty(Title))
                sb.Append("| Title         : " + Title + Environment.NewLine);

            if (!String.IsNullOrEmpty(Subtitle))
                sb.Append("| Subtitle      : " + Subtitle + Environment.NewLine);

            if (UnorderedList != null && UnorderedList.Count > 0)
            {
                sb.Append("| Unordered     : " + Environment.NewLine);
                foreach (string item in UnorderedList)
                    sb.Append("  | " + item + Environment.NewLine);
            }

            if (OrderedList != null && OrderedList.Count > 0)
            {
                sb.Append("| Ordered       : " + Environment.NewLine);
                foreach (string item in OrderedList)
                    sb.Append("  | " + item + Environment.NewLine);
            }

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
