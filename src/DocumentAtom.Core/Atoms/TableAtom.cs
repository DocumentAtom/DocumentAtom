using DocumentAtom.Core.Image;
using System.Data;

namespace DocumentAtom.Core.Atoms
{
    /// <summary>
    /// A table atom is an atom that contains a table with rows, columns, headers, and values.
    /// </summary>
    public class TableAtom : AtomBase<TableAtom>
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

        /// <summary>
        /// Create a table atom from a table structure.
        /// </summary>
        /// <param name="table">Table structure.</param>
        /// <returns>Table atom.</returns>
        public static TableAtom FromTableStructure(TableStructure table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (table == null) return null;

            var dt = new DataTable();

            // Add columns
            for (int i = 0; i < table.Columns; i++)
            {
                dt.Columns.Add($"Column{i}", typeof(string));
            }

            // Add rows
            if (table.Cells != null)
            {
                for (int i = 0; i < table.Rows && i < table.Cells.Count; i++)
                {
                    var row = dt.NewRow();
                    if (table.Cells[i] != null)
                    {
                        for (int j = 0; j < table.Columns && j < table.Cells[i].Count; j++)
                        {
                            row[j] = table.Cells[i][j] ?? string.Empty;
                        }
                    }
                    dt.Rows.Add(row);
                }
            }

            return new TableAtom
            {
                Rows = table.Rows,
                Columns = table.Columns,
                Table = dt
            };
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
