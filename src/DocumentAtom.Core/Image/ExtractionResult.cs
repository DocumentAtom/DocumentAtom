namespace DocumentAtom.Core.Image
{
    using SerializationHelper;
    using System;
    using System.Drawing;
    using System.Reflection.PortableExecutable;

    /// <summary>
    /// Extraction result.
    /// </summary>
    public class ExtractionResult : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Text elements.
        /// </summary>
        public List<TextElement> TextElements { get; set; }

        /// <summary>
        /// Table elements.
        /// </summary>
        public List<TableStructure> Tables { get; set; }

        /// <summary>
        /// List elements.
        /// </summary>
        public List<ListStructure> Lists { get; set; }

        #endregion

        #region Private-Members

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Extraction result.
        /// </summary>
        public ExtractionResult()
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
                    TextElements = null;
                    Tables = null;
                    Lists = null;
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
