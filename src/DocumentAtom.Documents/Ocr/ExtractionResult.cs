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
    /// Extraction result.
    /// </summary>
    public class ExtractionResult
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Text elements.
        /// </summary>
        public List<TextElement> TextElements { get; set; } = new List<TextElement>();

        /// <summary>
        /// Tables.
        /// </summary>
        public List<TableStructure> Tables { get; set; } = new List<TableStructure>();

        /// <summary>
        /// Lists.
        /// </summary>
        public List<ListStructure> Lists { get; set; } = new List<ListStructure>();

        #endregion

        #region Private-Members

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

        #endregion

        #region Private-Methods

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}