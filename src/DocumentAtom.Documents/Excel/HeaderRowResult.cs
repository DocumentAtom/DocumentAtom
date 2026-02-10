namespace DocumentAtom.Documents.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Documents.Excel;
    using DocumentFormat.OpenXml.Spreadsheet;

    /// <summary>
    /// Result object from detection of existence of a header row.
    /// </summary>
    public class HeaderRowResult : IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Boolean indicating whether the first row is a header row.
        /// </summary>
        public bool IsHeaderRow { get; set; } = false;

        /// <summary>
        /// Scores.
        /// </summary>
        public Dictionary<string, double> Scores
        {
            get
            {
                return _Scores;
            }
            set
            {
                if (value == null) value = new Dictionary<string, double>();
                _Scores = value;
            }
        }

        #endregion

        #region Private-Members

        private Dictionary<string, double> _Scores = new Dictionary<string, double>();
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Result object from detection of existence of a header row.
        /// </summary>
        public HeaderRowResult()
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
                    _Scores = null;
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

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}