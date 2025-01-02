namespace DocumentAtom.Core.Atoms
{
    using System.Data;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;

    /// <summary>
    /// A docx atom is a self-contained unit of information from a Microsoft Word .docx document.
    /// </summary>
    public class DocxAtom : AtomBase<MarkdownAtom>
    {
        #region Public-Members

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Header level, that is, the number of hash marks found at the beginning of the text.
        /// Minimum value is 1.
        /// </summary>
        public int? HeaderLevel
        {
            get
            {
                return _HeaderLevel;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(HeaderLevel));
                _HeaderLevel = value;
            }
        }

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

        private int? _HeaderLevel = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A docx atom is a self-contained unit of information from a Microsoft Word .docx document.
        /// </summary>
        public DocxAtom()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
