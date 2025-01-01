namespace DocumentAtom.Pdf
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using SerializableDataTables;
    using Tabula;
    using Tabula.Extractors;
    using System.Drawing;
    using UglyToad.PdfPig.Core;

    /// <summary>
    /// Create atoms from PDF documents.
    /// </summary>
    public class PdfProcessor : ProcessorBase
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// PDF processor settings.
        /// </summary>
        public new PdfProcessorSettings Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Settings));
                _Settings = value;
            }
        }

        #endregion

        #region Private-Members

        private PdfProcessorSettings _Settings = new PdfProcessorSettings();
        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from PDF documents.
        /// </summary>
        public PdfProcessor(PdfProcessorSettings settings = null)
        {
            if (settings == null) settings = new PdfProcessorSettings();

            Header = "[Pdf] ";
            _Settings = settings;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public IEnumerable<PdfAtom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return ProcessFile(filename);
        }

        /// <summary>
        /// Retrieve metadata from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Dictionary.</returns>
        public Dictionary<string, string> GetMetadata(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            Dictionary<string, string> ret = new Dictionary<string, string>();

            using (PdfDocument document = PdfDocument.Open(filename))
            {
                var info = document.Information;
                if (info.Author != null) ret.Add("Author", info.Author);
                if (info.Creator != null) ret.Add("Creator", info.Creator);
                if (info.Title != null) ret.Add("Title", info.Title);
                if (info.Subject != null) ret.Add("Subject", info.Subject);
                if (info.Keywords != null) ret.Add("Keywords", info.Keywords);
                if (info.Producer != null) ret.Add("Producer", info.Producer);
            }

            return ret;
        }

        #endregion

        #region Private-Methods

        private IEnumerable<PdfAtom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            using (PdfDocument document = PdfDocument.Open(filename))
            {
                int pageNumber = 1;
                foreach (var page in document.GetPages())
                {
                    // First, extract tables and their regions
                    var tableRegions = new List<PdfTableRegion>();
                    ObjectExtractor oe = new ObjectExtractor(document);
                    PageArea pageArea = oe.Extract(pageNumber);
                    IExtractionAlgorithm ea = new SpreadsheetExtractionAlgorithm();
                    List<Table> tables = ea.Extract(pageArea);

                    foreach (var table in tables)
                    {
                        var dt = ConvertTabulaToDataTable(table);
                        if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                        {
                            // Store table region
                            tableRegions.Add(new PdfTableRegion
                            {
                                BoundingBox = table.BoundingBox,
                                Table = dt
                            });

                            // Emit table atom
                            yield return new PdfAtom
                            {
                                Type = AtomTypeEnum.Table,
                                Table = SerializableDataTable.FromDataTable(dt),
                                BoundingBox = new BoundingBox
                                {
                                    UpperLeft = new Point(Math.Max(0, (int)table.BoundingBox.TopLeft.X), Math.Max(0, (int)table.BoundingBox.TopLeft.Y)),
                                    UpperRight = new Point(Math.Max(0, (int)table.BoundingBox.TopRight.X), Math.Max(0, (int)table.BoundingBox.TopRight.Y)),
                                    LowerLeft = new Point(Math.Max(0, (int)table.BoundingBox.BottomLeft.X), Math.Max(0, (int)table.BoundingBox.BottomLeft.Y)),
                                    LowerRight = new Point(Math.Max(0, (int)table.BoundingBox.BottomRight.X), Math.Max(0, (int)table.BoundingBox.BottomRight.Y))
                                },
                                PageNumber = pageNumber,
                                MD5Hash = HashHelper.MD5Hash(dt),
                                SHA1Hash = HashHelper.SHA1Hash(dt),
                                SHA256Hash = HashHelper.SHA256Hash(dt),
                                Length = DataTableHelper.GetLength(dt)
                            };
                        }
                    }

                    // Process text using RecursiveXYCut
                    var words = page.GetWords();
                    var xycut = new RecursiveXYCut(new RecursiveXYCut.RecursiveXYCutOptions()
                    {
                        MinimumWidth = page.Width / 3.0,
                        DominantFontWidthFunc = _ => (page.Letters.Average(l => l.GlyphRectangle.Width) * 2),
                        DominantFontHeightFunc = _ => (page.Letters.Average(l => l.GlyphRectangle.Height) * 2)
                    });

                    IReadOnlyList<TextBlock> blocks = xycut.GetBlocks(words);
                    foreach (var block in blocks)
                    {
                        // Skip if block overlaps with any table region
                        if (IsBlockInTableRegion(block, tableRegions)) continue;

                        StringBuilder sb = new StringBuilder();
                        foreach (TextLine textLine in block.TextLines)
                        {
                            foreach (Word word in textLine.Words)
                            {
                                sb.Append(word.Text + " ");
                            }
                        }

                        string text = sb.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            yield return new PdfAtom
                            {
                                Type = AtomTypeEnum.Text,
                                Text = text,
                                BoundingBox = new BoundingBox
                                {
                                    UpperLeft = new Point(Math.Max(0, (int)block.BoundingBox.TopLeft.X), Math.Max(0, (int)block.BoundingBox.TopLeft.Y)),
                                    UpperRight = new Point(Math.Max(0, (int)block.BoundingBox.TopRight.X), Math.Max(0, (int)block.BoundingBox.TopRight.Y)),
                                    LowerLeft = new Point(Math.Max(0, (int)block.BoundingBox.BottomLeft.X), Math.Max(0, (int)block.BoundingBox.BottomLeft.Y)),
                                    LowerRight = new Point(Math.Max(0, (int)block.BoundingBox.BottomRight.X), Math.Max(0, (int)block.BoundingBox.BottomRight.Y))
                                },
                                PageNumber = pageNumber,
                                MD5Hash = HashHelper.MD5Hash(text),
                                SHA1Hash = HashHelper.SHA1Hash(text),
                                SHA256Hash = HashHelper.SHA256Hash(text),
                                Length = text.Length
                            };
                        }
                    }

                    // Process images (remains the same)
                    foreach (var image in page.GetImages())
                    {
                        byte[] bytes;
                        if (image.TryGetPng(out byte[] pngBytes)) bytes = pngBytes;
                        else bytes = image.RawBytes.ToArray();

                        yield return new PdfAtom
                        {
                            Type = AtomTypeEnum.Image,
                            Binary = bytes,
                            BoundingBox = new BoundingBox
                            {
                                UpperLeft = new Point(Math.Max(0, (int)image.Bounds.TopLeft.X), Math.Max(0, (int)image.Bounds.TopLeft.Y)),
                                UpperRight = new Point(Math.Max(0, (int)image.Bounds.TopRight.X), Math.Max(0, (int)image.Bounds.TopRight.Y)),
                                LowerLeft = new Point(Math.Max(0, (int)image.Bounds.BottomLeft.X), Math.Max(0, (int)image.Bounds.BottomLeft.Y)),
                                LowerRight = new Point(Math.Max(0, (int)image.Bounds.BottomRight.X), Math.Max(0, (int)image.Bounds.BottomRight.Y))
                            },
                            PageNumber = pageNumber,
                            MD5Hash = HashHelper.MD5Hash(bytes),
                            SHA1Hash = HashHelper.SHA1Hash(bytes),
                            SHA256Hash = HashHelper.SHA256Hash(bytes),
                            Length = bytes.Length
                        };
                    }

                    pageNumber++;
                }
            }
        }

        private class PdfTableRegion
        {
            public PdfRectangle BoundingBox { get; set; }

            public DataTable Table { get; set; } = null;

            public PdfTableRegion()
            {

            }
        }

        private bool IsBlockInTableRegion(TextBlock block, List<PdfTableRegion> tableRegions)
        {
            // Check for overlap with any table region
            foreach (var region in tableRegions)
            {
                if (DoRectanglesOverlap(block.BoundingBox, region.BoundingBox))
                {
                    return true;
                }
            }

            return false;
        }

        private bool DoRectanglesOverlap(PdfRectangle rect1, PdfRectangle rect2)
        {
            return !(rect1.Left > rect2.Right ||
                    rect1.Right < rect2.Left ||
                    rect1.Bottom > rect2.Top ||
                    rect1.Top < rect2.Bottom);
        }

        private DataTable ConvertTabulaToDataTable(Table table)
        {
            var dt = new DataTable();

            var rows = table.Rows;
            if (rows == null || !rows.Any()) return dt;

            // Add columns based on first row
            for (int i = 0; i < rows[0].Count; i++)
            {
                dt.Columns.Add($"Column{i + 1}");
            }

            // Add data rows
            foreach (var row in rows)
            {
                var dataRow = dt.NewRow();
                for (int i = 0; i < row.Count; i++)
                {
                    if (i < dt.Columns.Count)
                    {
                        dataRow[i] = row[i].GetText().Trim();
                    }
                }
                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}