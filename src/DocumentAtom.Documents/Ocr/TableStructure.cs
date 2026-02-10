namespace DocumentAtom.Documents.Ocr
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using Tesseract;
    using SixLabors.ImageSharp;

    using Rectangle = System.Drawing.Rectangle;

    /// <summary>
    /// Table structure.
    /// </summary>
    public class TableStructure
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Cells.
        /// </summary>
        public List<List<string>> Cells { get; set; } = new List<List<string>>();

        /// <summary>
        /// Bounds.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Rows.
        /// </summary>
        public int Rows { get; set; }

        /// <summary>
        /// Counts.
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

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}