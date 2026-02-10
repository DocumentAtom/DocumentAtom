namespace DocumentAtom.Documents.Excel
{
    /// <summary>
    /// Represents cell data with row index and value information.
    /// </summary>
    public class CellData
    {
        /// <summary>
        /// Gets or sets the row index of the cell.
        /// </summary>
        public int RowIndex { get; set; }

        /// <summary>
        /// Gets or sets the cell value.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the CellData class.
        /// </summary>
        public CellData()
        {
        }

        /// <summary>
        /// Initializes a new instance of the CellData class with specified values.
        /// </summary>
        /// <param name="rowIndex">The row index.</param>
        /// <param name="value">The cell value.</param>
        public CellData(int rowIndex, string value)
        {
            RowIndex = rowIndex;
            Value = value;
        }
    }
}
