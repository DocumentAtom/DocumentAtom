namespace DocumentAtom.Core.Image
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Table structure.
    /// </summary>
    public class TableStructure
    {
        #region Public-Members

        /// <summary>
        /// Table cells.
        /// </summary>
        public List<List<string>> Cells { get; set; }

        /// <summary>
        /// Bounds for the table structure.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Number of rows.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Number of columns.
        /// </summary>
        public int Columns { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Table structure.
        /// </summary>
        public TableStructure()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
