using System.Data;

namespace DocumentAtom.Core
{
    /// <summary>
    /// A table atom is an atom that contains a table with rows, columns, headers, and values.
    /// </summary>
    public class TableAtom : AtomBase
    {
        #region Public-Members

        /// <summary>
        /// The number of rows.
        /// </summary>
        public int Rows
        {
            get
            {
                return _Rows;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Rows));
                _Rows = value;
            }
        }

        /// <summary>
        /// The number of columns.
        /// </summary>
        public int Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Columns));
                _Columns = value;
            }
        }

        /// <summary>
        /// Boolean indicating if the geometry of the table is irregular.
        /// </summary>
        public bool Irregular { get; set; } = false;

        /// <summary>
        /// Data table.
        /// </summary>
        public DataTable Table { get; set; } = null;

        #endregion

        #region Private-Members

        private int _Rows = 0;
        private int _Columns = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A table atom is an atom that contains a table with rows, columns, headers, and values.
        /// </summary>
        public TableAtom() 
        { 
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
