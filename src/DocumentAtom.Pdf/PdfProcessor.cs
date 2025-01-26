namespace DocumentAtom.Pdf
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Security.Cryptography;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Image;
    using UglyToad.PdfPig;
    using UglyToad.PdfPig.Content;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.DocumentLayoutAnalysis;
    using UglyToad.PdfPig.DocumentLayoutAnalysis.PageSegmenter;
    using SerializableDataTables;
    using Tabula;
    using Tabula.Extractors;

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
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public PdfProcessor(PdfProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
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
        public IEnumerable<Atom> Extract(string filename)
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

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            using (PdfDocument document = PdfDocument.Open(filename))
            {
                int pageNumber = 1;
                foreach (var page in document.GetPages())
                {
                    #region Variables-and-State

                    ObjectExtractor oe = new ObjectExtractor(document);
                    PageArea pageArea = oe.Extract(pageNumber);
                    List<PdfRegion> regions = new List<PdfRegion>();
                    List<string> imageHashes = new List<string>();

                    #endregion

                    #region Tables

                    List<Table> tables = ExtractTables(pageArea);
                    foreach (var table in tables)
                    {
                        var dt = TabulaToDataTable(table);
                        if (dt.Rows.Count > 0 && dt.Columns.Count > 0)
                        {
                            var tableRegion = new PdfTableRegion
                            {
                                BoundingBox = table.BoundingBox,
                                Table = dt,
                                Processed = true
                            };
                            regions.Add(tableRegion);

                            yield return new Atom
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

                    #endregion

                    #region Lists

                    var listRegions = ExtractLists(page, regions);
                    regions.AddRange(listRegions);

                    foreach (var listRegion in listRegions.Cast<PdfListRegion>())
                    {
                        if (listRegion.Items != null && listRegion.Items.Count > 0)
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.List,
                                OrderedList = (listRegion.IsOrdered ? listRegion.Items : null),
                                UnorderedList = (listRegion.IsOrdered ? null : listRegion.Items),
                                BoundingBox = new BoundingBox
                                {
                                    UpperLeft = new Point(Math.Max(0, (int)listRegion.BoundingBox.TopLeft.X), Math.Max(0, (int)listRegion.BoundingBox.TopLeft.Y)),
                                    UpperRight = new Point(Math.Max(0, (int)listRegion.BoundingBox.TopRight.X), Math.Max(0, (int)listRegion.BoundingBox.TopRight.Y)),
                                    LowerLeft = new Point(Math.Max(0, (int)listRegion.BoundingBox.BottomLeft.X), Math.Max(0, (int)listRegion.BoundingBox.BottomLeft.Y)),
                                    LowerRight = new Point(Math.Max(0, (int)listRegion.BoundingBox.BottomRight.X), Math.Max(0, (int)listRegion.BoundingBox.BottomRight.Y))
                                },
                                PageNumber = pageNumber,
                                MD5Hash = HashHelper.MD5Hash(listRegion.Items),
                                SHA1Hash = HashHelper.SHA1Hash(listRegion.Items),
                                SHA256Hash = HashHelper.SHA256Hash(listRegion.Items),
                                Length = listRegion.Items.Sum(c => c.Length)
                            };
                        }
                    }

                    #endregion

                    #region Text
                     
                    var words = page.GetWords();
                    var xycut = new RecursiveXYCut(new RecursiveXYCut.RecursiveXYCutOptions()
                    {
                        MinimumWidth = page.Width / 3.0,
                        DominantFontWidthFunc = _ => (page.Letters.Average(l => l.GlyphRectangle.Width) * 2),
                        DominantFontHeightFunc = _ => (page.Letters.Average(l => l.GlyphRectangle.Height) * 2)
                    });

                    IReadOnlyList<TextBlock> blocks = xycut.GetBlocks(words);
                    var orderedBlocks = blocks.OrderByDescending(b => b.BoundingBox.Top).ToList();

                    foreach (var block in orderedBlocks)
                    {
                        if (IsRegionAlreadyProcessed(block, regions)) continue;

                        StringBuilder sb = new StringBuilder();
                        foreach (TextLine textLine in block.TextLines)
                        {
                            foreach (Word word in textLine.Words)
                                sb.Append(word.Text + " ");
                        }

                        string text = sb.ToString().Trim();
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            yield return new Atom
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

                    #endregion

                    #region Images

                    foreach (var image in page.GetImages())
                    {
                        byte[] bytes;
                        if (image.TryGetPng(out byte[] pngBytes)) bytes = pngBytes;
                        else bytes = image.RawBytes.ToArray();

                        string sha256 = Convert.ToBase64String(SHA256.Create().ComputeHash(bytes));
                        if (imageHashes.Contains(sha256))
                        {
                            // duplicate
                        }
                        else
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Binary,
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
                    }

                    #endregion

                    pageNumber++;
                }
            }
        }

        private static bool IsRegionAlreadyProcessed(TextBlock block, List<PdfRegion> regions)
        {
            var blockRect = block.BoundingBox;

            foreach (var region in regions)
            {
                if (region.Processed && OverlapExists(blockRect, region.BoundingBox))
                {
                    var intersectionArea = GetIntersectionArea(blockRect, region.BoundingBox);
                    var blockArea = (blockRect.Right - blockRect.Left) * (blockRect.Top - blockRect.Bottom);

                    if (intersectionArea / blockArea > 0.2)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        private static double GetIntersectionArea(PdfRectangle rect1, PdfRectangle rect2)
        {
            double x1 = Math.Max(rect1.Left, rect2.Left);
            double y1 = Math.Max(rect1.Bottom, rect2.Bottom);
            double x2 = Math.Min(rect1.Right, rect2.Right);
            double y2 = Math.Min(rect1.Top, rect2.Top);

            if (x2 >= x1 && y2 >= y1)
                return (x2 - x1) * (y2 - y1);

            return 0;
        }

        private static bool OverlapExists(PdfRectangle rect1, PdfRectangle rect2)
        {
            return !(rect1.Left > rect2.Right ||
                    rect1.Right < rect2.Left ||
                    rect1.Bottom > rect2.Top ||
                    rect1.Top < rect2.Bottom);
        }

        #region Tables

        private static List<Table> ExtractTables(PageArea pageArea)
        {
            var tables = new List<Table>();

            // Try both algorithms and combine results
            IExtractionAlgorithm basicAlgorithm = new BasicExtractionAlgorithm();
            IExtractionAlgorithm spreadsheetAlgorithm = new SpreadsheetExtractionAlgorithm();

            tables.AddRange(basicAlgorithm.Extract(pageArea).Where(IsTable));
            tables.AddRange(spreadsheetAlgorithm.Extract(pageArea).Where(IsTable));

            return tables.Distinct().ToList();
        }

        private static bool IsTable(Table table)
        {
            if (table == null || table.Rows == null || !table.Rows.Any())
                return false;

            // Basic size requirements - minimum dimensions for a table
            if (table.RowCount < 2 || table.Rows[0].Count < 2)  // At least 2 rows and 2 columns
                return false;

            // Analyze row structure consistency
            var rowLengths = table.Rows.Select(row => row.Count(cell =>
                !string.IsNullOrWhiteSpace(cell?.GetText()?.Trim()))).ToList();

            // Get the most common non-zero row length (mode)
            var mostCommonLength = rowLengths.Where(len => len > 0)
                .GroupBy(len => len)
                .OrderByDescending(group => group.Count())
                .ThenByDescending(group => group.Key)
                .FirstOrDefault()?.Key ?? 0;

            if (mostCommonLength < 2) return false; // at least 2 columns consistently

            // Calculate structural consistency
            int rowsMatchingCommonPattern = rowLengths.Count(len =>
                len == mostCommonLength || len == 0); // Allow empty rows
            double structuralConsistency = (double)rowsMatchingCommonPattern / table.RowCount;

            // Check for cell density (percentage of non-empty cells)
            int totalCells = table.Rows.Sum(row => row.Count);
            int nonEmptyCells = table.Rows.Sum(row => row.Count(cell =>
                !string.IsNullOrWhiteSpace(cell?.GetText()?.Trim())));
            double cellDensity = (double)nonEmptyCells / totalCells;

            // Check for alignment consistency
            bool hasConsistentAlignment = CheckAlignmentConsistency(table);

            // Table should have:
            // 1. Consistent structure in majority of rows
            // 2. Reasonable cell density
            // 3. Consistent column alignment
            return structuralConsistency >= 0.7 &&
                   cellDensity >= 0.3 &&
                   hasConsistentAlignment;
        }

        private static bool CheckAlignmentConsistency(Table table)
        {
            if (table.Rows.Count < 2) return false;

            // Get column positions from the first non-empty row
            var firstNonEmptyRow = table.Rows.FirstOrDefault(row =>
                row.Any(cell => !string.IsNullOrWhiteSpace(cell?.GetText())));
            if (firstNonEmptyRow == null) return false;

            var columnPositions = firstNonEmptyRow
                .Where(cell => cell != null)
                .Select(cell => cell.BoundingBox.Left)
                .ToList();

            // Tolerance for position variation
            const double positionTolerance = 5.0;

            // Check if subsequent rows maintain similar column positions
            int alignedRows = table.Rows.Count(row =>
            {
                var rowPositions = row.Where(cell => cell != null)
                    .Select(cell => cell.BoundingBox.Left)
                    .ToList();

                if (!rowPositions.Any()) return true;

                // Check if row positions align with reference positions
                return rowPositions.Count <= columnPositions.Count &&
                       rowPositions.All(pos =>
                           columnPositions.Any(refPos =>
                               Math.Abs(pos - refPos) <= positionTolerance));
            });

            return (double)alignedRows / table.RowCount >= 0.7;
        }

        private static DataTable TabulaToDataTable(Table table)
        {
            var dt = new DataTable();

            var rows = table.Rows;
            if (rows == null || !rows.Any()) return dt;

            for (int i = 0; i < rows[0].Count; i++) dt.Columns.Add($"Column{i + 1}");

            foreach (var row in rows)
            {
                var dataRow = dt.NewRow();
                for (int i = 0; i < row.Count; i++)
                {
                    if (i < dt.Columns.Count) dataRow[i] = row[i].GetText().Trim();
                }
                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        #endregion

        #region Lists

        private static List<PdfRegion> ExtractLists(Page page, List<PdfRegion> existingRegions)
        {
            var listRegions = new List<PdfRegion>();
            var words = page.GetWords()
                .Where(w => !IsWordInProcessedRegion(w, existingRegions))
                .OrderByDescending(w => w.BoundingBox.Top)
                .ThenBy(w => w.BoundingBox.Left)
                .ToList();

            // Find potential list start (any marker)
            for (int i = 0; i < words.Count; i++)
            {
                if (IsListMarker(words[i].Text))
                {
                    var markers = new List<Word>();
                    var items = new List<string>();
                    double markerX = words[i].BoundingBox.Left;
                    double tolerance = 5;

                    // Look for aligned markers
                    for (int j = i; j < words.Count; j++)
                    {
                        var word = words[j];

                        // Check if this word is a marker at same indentation
                        if (IsListMarker(word.Text) &&
                            Math.Abs(word.BoundingBox.Left - markerX) <= tolerance)
                        {
                            markers.Add(word);

                            // Get words on same line as this marker
                            var lineWords = GetWordsOnSameLine(words, word);
                            if (lineWords.Any())
                            {
                                items.Add(string.Join(" ", lineWords.Select(w => w.Text)));
                            }
                        }
                    }

                    // If we found multiple markers and items, create a list region
                    if (markers.Count >= 2)
                    {
                        var region = new PdfListRegion
                        {
                            Items = items,
                            BoundingBox = GetBoundingBox(markers),
                            Processed = true
                        };

                        listRegions.Add(region);
                        i = words.IndexOf(markers.Last());
                    }
                }
            }

            return listRegions;
        }

        private static List<Word> GetWordsOnSameLine(List<Word> words, Word marker)
        {
            var lineWords = new List<Word>();
            double markerTop = marker.BoundingBox.Top;
            double markerBottom = marker.BoundingBox.Bottom;

            foreach (var word in words)
            {
                // Only include words to the right of the marker
                if (word.BoundingBox.Left <= marker.BoundingBox.Left)
                    continue;

                // Check if word is vertically aligned with marker
                if (word.BoundingBox.Bottom <= markerTop &&
                    word.BoundingBox.Top >= markerBottom)
                {
                    lineWords.Add(word);
                }
            }

            return lineWords;
        }

        private static PdfRectangle GetBoundingBox(List<Word> words)
        {
            if (!words.Any())
            {
                return new PdfRectangle(0, 0, 0, 0);
            }

            double minX = words.Min(w => w.BoundingBox.Left);
            double maxX = words.Max(w => w.BoundingBox.Right);
            double maxY = words.Max(w => w.BoundingBox.Top);
            double minY = words.Min(w => w.BoundingBox.Bottom);

            return new PdfRectangle(minX, minY, maxX, maxY);
        }

        private static bool IsWordInProcessedRegion(Word word, List<PdfRegion> regions)
        {
            foreach (var region in regions)
            {
                if (region.Processed && OverlapExists(word.BoundingBox, region.BoundingBox))
                {
                    return true;
                }
            }
            return false;
        }

        private static bool IsListMarker(string text)
        {
            text = text.Trim();

            // Check for bullet-style markers
            if (text == "-" || text == "•" || text == "*" || text == "‣" || text == "◦" || text == "⁃")
                return true;

            // Check for numbered list markers
            if (text.Length <= 4)  // Reasonable length for a marker
            {
                // Handle parenthesized markers: (1), (a), etc.
                if (text.StartsWith("(") && text.EndsWith(")"))
                {
                    var inner = text.Substring(1, text.Length - 2);
                    return char.IsLetterOrDigit(inner[0]);
                }

                // Handle dot-suffixed markers: 1., a., etc.
                if (text.EndsWith("."))
                {
                    var prefix = text.Substring(0, text.Length - 1);
                    return prefix.All(c => char.IsLetterOrDigit(c));
                }
            }

            return false;
        }

        #endregion

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}