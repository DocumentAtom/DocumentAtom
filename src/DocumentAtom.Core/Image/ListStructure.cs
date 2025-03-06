namespace DocumentAtom.Core.Image
{
    using System;
    using System.Drawing;

    /// <summary>
    /// List structure.
    /// </summary>
    public class ListStructure : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// List items.
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Boolean indicating if the list is ordered.
        /// </summary>
        public bool IsOrdered { get; set; }

        /// <summary>
        /// Bounds for the list structure.
        /// </summary>
        public Rectangle Bounds { get; set; }

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// List structure.
        /// </summary>
        public ListStructure()
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
                    Items = null;
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
