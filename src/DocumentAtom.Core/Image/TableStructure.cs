namespace DocumentAtom.Core.Image
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Table structure.
    /// </summary>
    public class TableStructure : IDisposable
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

        private bool _Disposed = false;

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

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    Cells = null;
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
