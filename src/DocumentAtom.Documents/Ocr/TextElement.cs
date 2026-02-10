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
    /// Text element.
    /// </summary>
    public class TextElement
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Text.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// Bounds.
        /// </summary>
        public Rectangle Bounds { get; set; }

        /// <summary>
        /// Confidence.
        /// </summary>
        public float Confidence { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Text element.
        /// </summary>
        public TextElement()
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