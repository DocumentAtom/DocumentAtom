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
    /// List structure.
    /// </summary>
    public class ListStructure
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// List items.
        /// </summary>
        public List<string> Items { get; set; } = new List<string>();

        /// <summary>
        /// Boolean indicating if the list is ordered.
        /// </summary>
        public bool IsOrdered { get; set; }

        /// <summary>
        /// Bounds.
        /// </summary>
        public Rectangle Bounds { get; set; }

        #endregion

        #region Private-Members

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

        #endregion

        #region Private-Methods

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}