namespace DocumentAtom.Csv
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for CSV processor.
    /// </summary>
    public class CsvProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Delimiter found between rows.
        /// Default is Environment.NewLine.
        /// </summary>
        public string RowDelimiter
        {
            get
            {
                return _RowDelimiter;
            }
            set
            {
                if (value == null || value.Length < 1) throw new ArgumentNullException(nameof(RowDelimiter));
                _RowDelimiter = value;
            }
        }

        /// <summary>
        /// Delimiter found between columns within a row.
        /// Default is comma (,).
        /// </summary>
        public char ColumnDelimiter
        {
            get
            {
                return _ColumnDelimiter;
            }
            set
            {
                _ColumnDelimiter = value;
            }
        }

        /// <summary>
        /// Characters to strip if found at the beginning or end of a value.
        /// Default is double quote and single quote.
        /// </summary>
        public char[] QuoteCharacters { get; set; } = new char[] { '\"', '\'' };

        /// <summary>
        /// Indicates whether the first row contains header names.
        /// Default is true.
        /// </summary>
        public bool HasHeaderRow { get; set; } = true;

        /// <summary>
        /// Number of rows per atom.
        /// 0 = entire table as one atom (default).
        /// 1 = each row as separate atom.
        /// N = N rows per atom.
        /// Minimum value is 0.
        /// </summary>
        public int RowsPerAtom
        {
            get
            {
                return _RowsPerAtom;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(RowsPerAtom));
                _RowsPerAtom = value;
            }
        }

        #endregion

        #region Private-Members

        private string _RowDelimiter = Environment.NewLine;
        private char _ColumnDelimiter = ',';
        private int _RowsPerAtom = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for CSV processor.
        /// </summary>
        public CsvProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
