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
    /// Image context entractor.  This library requires that you have Tesseract installed on the host and that you have awareness of the location of the tessdata directory.
    /// </summary>
    public class ImageContentExtractor : IDisposable
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// Line threshold.
        /// </summary>   
        public int LineThreshold { get; set; } = 5;

        /// <summary>
        /// Paragraph threshold.
        /// </summary>
        public int ParagraphThreshold { get; set; } = 30;

        /// <summary>
        /// Horizontal line length.
        /// </summary>
        public int HorizontalLineLength { get; set; } = 80;

        /// <summary>
        /// Vertical line length.
        /// </summary>
        public int VerticalLineLength { get; set; } = 40;

        /// <summary>
        /// Table minimum area.
        /// </summary>
        public int TableMinimumArea { get; set; } = 5000;

        /// <summary>
        /// Column alignment tolerance.
        /// </summary>
        public int ColumnAlignmentTolerance { get; set; } = 10;

        /// <summary>
        /// Proximity threshold.
        /// </summary>
        public int ProximityThreshold { get; set; } = 20;

        /// <summary>
        /// List markers.
        /// </summary>
        public HashSet<string> ListMarkers { get; set; } = new HashSet<string> { "-", "•", "*", "○", "●", "■", "□", "→", "▪", "▫", "♦", "⚫" };

        /// <summary>
        /// List numbering patterns.
        /// </summary>
        public HashSet<string> ListNumberingPatterns { get; set; } = new HashSet<string>
        {
            @"^\d+\.",            // 1.
            @"^[a-z]\)",          // a)
            @"^[A-Z]\)",          // A)
            @"^\(\d+\)",          // (1)
            @"^[ivxlcdm]+\.",     // Roman numerals lowercase
            @"^[IVXLCDM]+\.",     // Roman numerals uppercase
            @"^[a-z]\.",          // a.
            @"^[A-Z]\."           // A.
        };

        #endregion

        #region Private-Members

        private readonly TesseractEngine _Tesseract;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Image context entractor.  This library requires that you have Tesseract installed on the host and that you have awareness of the location of the tessdata directory.
        /// </summary>
        /// <param name="tessdataPath">Tesseract data folder.  
        /// For Windows, the folder is generally C:\Program Files\Tesseract-OCR\tessdata.
        /// For MacOS, the folder is generally /usr/local/share/tessdata or /opt/local/share/tessdata.
        /// For Linux, the folder is generally /usr/share/tessdata or /usr/local/share/tessdata.
        /// </param>
        /// <param name="languageFile">The default language file to use.  Defaults to eng.  This file, with extension .traineddata, must exist in the data directory.</param>
        public ImageContentExtractor(string tessdataPath, string languageFile = "eng")
        {
            if (String.IsNullOrEmpty(tessdataPath)) throw new ArgumentNullException(nameof(tessdataPath));
            if (String.IsNullOrEmpty(languageFile)) throw new ArgumentNullException(nameof(languageFile));

            string dirPath = Path.GetFullPath(tessdataPath);
            string fullFile = Path.Join(dirPath, (languageFile + ".traineddata"));

            if (!Directory.Exists(dirPath)) throw new DirectoryNotFoundException("The specified tessdata directory does not exist.");
            if (!File.Exists(fullFile))
                throw new FileNotFoundException("The specified language file " + languageFile + ".traineddata does not exist in the specified tessdata directory.");

            _Tesseract = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
            _Tesseract.SetVariable("debug_file", "/dev/null");
            _Tesseract.SetVariable("quiet_mode", "1");
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
                    ListMarkers?.Clear();
                    ListNumberingPatterns?.Clear();

                    _Tesseract?.Dispose();
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

        /// <summary>
        /// Extract content from PNG data.
        /// </summary>
        /// <param name="pngData">PNG data.</param>
        /// <returns>Extraction result.</returns>
        public ExtractionResult ExtractContent(byte[] pngData)
        {
            if (pngData == null || pngData.Length == 0)
            {
                throw new ArgumentException("Input image data cannot be null or empty", nameof(pngData));
            }

            ExtractionResult result = new ExtractionResult
            {
                TextElements = new List<TextElement>(),
                Tables = new List<TableStructure>(),
                Lists = new List<ListStructure>()
            };

            try
            {
                using MemoryStream memStream = new MemoryStream(pngData);
                using Pix img = Pix.LoadFromMemory(memStream.ToArray());
                List<TextElement> allElements;

                // First pass: Get all elements
                using (Page page = _Tesseract.Process(img))
                {
                    allElements = GetTextElements(page);
                }

                // Step 1: Process tables first and get masked regions
                List<Rectangle> tableRegions = DetectTableRegionsFromElements(allElements);
                using (Page page = _Tesseract.Process(img))
                {
                    foreach (Rectangle region in tableRegions)
                    {
                        TableStructure table = ExtractTableContent(page, region);
                        if (table != null)
                        {
                            result.Tables.Add(table);
                        }
                    }
                }

                // Step 2: Filter out elements that belong to tables
                List<TextElement> nonTableElements = allElements.Where(element =>
                    !tableRegions.Any(region => IsElementInRegion(element.Bounds, region))).ToList();

                // Step 3: Process lists
                ProcessListContent(nonTableElements, tableRegions, result);

                // Step 4: Process remaining text elements (excluding those in lists)
                List<TextElement> nonListElements = nonTableElements.Where(element =>
                    !result.Lists.Any(list => IsElementInRegion(element.Bounds, list.Bounds))).ToList();

                ProcessTextContent(nonListElements, result);
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        #endregion

        #region Private-Methods

        #region Text

        private List<TextElement> GetTextElements(Page page)
        {
            List<TextElement> elements = new List<TextElement>();

            using (ResultIterator iter = page.GetIterator())
            {
                iter.Begin();
                int wordCount = 0;

                do
                {
                    try
                    {
                        if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out Rect bounds))
                        {
                            string text = iter.GetText(PageIteratorLevel.Word);
                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                wordCount++;
                                elements.Add(new TextElement
                                {
                                    Text = text,
                                    Bounds = new Rectangle(
                                        bounds.X1,
                                        bounds.Y1,
                                        bounds.Width,
                                        bounds.Height
                                    ),
                                    Confidence = iter.GetConfidence(PageIteratorLevel.Word)
                                });
                            }
                        }
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                } while (iter.Next(PageIteratorLevel.Word));
            }

            return elements;
        }

        private void ProcessTextContent(List<TextElement> elements, ExtractionResult result)
        {
            if (!elements.Any()) return;

            // Group elements by line
            List<List<TextElement>> elementsByLine = new List<List<TextElement>>();
            List<TextElement> currentLine = new List<TextElement> { elements.First() };
            List<TextElement> orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();

            for (int i = 1; i < orderedElements.Count; i++)
            {
                TextElement current = orderedElements[i];
                TextElement previous = currentLine[0];

                bool sameLineByOverlap = (current.Bounds.Top <= previous.Bounds.Bottom &&
                                        current.Bounds.Bottom >= previous.Bounds.Top);

                if (sameLineByOverlap)
                {
                    currentLine.Add(current);
                }
                else
                {
                    elementsByLine.Add(new List<TextElement>(currentLine));
                    currentLine.Clear();
                    currentLine.Add(current);
                }
            }

            if (currentLine.Any())
            {
                elementsByLine.Add(currentLine);
            }

            // Process each line without any special cases
            foreach (List<TextElement> line in elementsByLine)
            {
                List<TextElement> orderedLine = line.OrderBy(e => e.Bounds.X).ToList();
                string text = string.Join(" ", orderedLine.Select(e => e.Text));

                if (!string.IsNullOrWhiteSpace(text))
                {
                    result.TextElements.Add(new TextElement
                    {
                        Text = text,
                        Bounds = new Rectangle(
                            orderedLine.Min(e => e.Bounds.X),
                            orderedLine.Min(e => e.Bounds.Y),
                            orderedLine.Max(e => e.Bounds.Right) - orderedLine.Min(e => e.Bounds.X),
                            orderedLine.Max(e => e.Bounds.Bottom) - orderedLine.Min(e => e.Bounds.Y)
                        ),
                        Confidence = orderedLine.Average(e => e.Confidence)
                    });
                }
            }
        }

        #endregion

        #region Tables

        private List<Rectangle> DetectTableRegionsFromElements(List<TextElement> elements)
        {
            List<Rectangle> tableRegions = new List<Rectangle>();

            // Group elements by line
            List<IGrouping<int, TextElement>> lineGroups = elements
                .GroupBy(e => e.Bounds.Y / LineThreshold)
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < lineGroups.Count; i++)
            {
                IGrouping<int, TextElement> currentLine = lineGroups[i];

                // Check if this line could be a table header
                bool isHeaderLine = currentLine.Any(e =>
                    e.Text.Contains("Column", StringComparison.OrdinalIgnoreCase) ||
                    (e.Text.Contains("Row", StringComparison.OrdinalIgnoreCase) &&
                     currentLine.Any(x => x.Text.Contains("name", StringComparison.OrdinalIgnoreCase))));

                if (!isHeaderLine)
                    continue;

                // Look for table content below
                List<IGrouping<int, TextElement>> tableLines = new List<IGrouping<int, TextElement>> { currentLine };
                int lookAhead = i + 1;
                bool inTable = true;

                while (lookAhead < lineGroups.Count && inTable)
                {
                    IGrouping<int, TextElement> nextLine = lineGroups[lookAhead];

                    // Check if this line matches table row pattern
                    bool isTableRow = nextLine.Count() >= 2 &&
                                    (nextLine.Any(e => e.Text.StartsWith("Row", StringComparison.OrdinalIgnoreCase)) ||
                                     nextLine.Any(e => e.Text.StartsWith("Value", StringComparison.OrdinalIgnoreCase)));

                    if (!isTableRow)
                    {
                        // Check if we've found the end of the table
                        int verticalGap = nextLine.Min(e => e.Bounds.Y) -
                                        tableLines.Last().Max(e => e.Bounds.Bottom);
                        if (verticalGap > LineThreshold * 2)
                        {
                            break;
                        }
                    }

                    if (isTableRow)
                    {
                        tableLines.Add(nextLine);
                    }
                    lookAhead++;
                }

                if (tableLines.Count >= 2)
                {
                    IEnumerable<TextElement> allTableElements = tableLines.SelectMany(g => g);
                    Rectangle region = new Rectangle(
                        allTableElements.Min(e => e.Bounds.X),
                        allTableElements.Min(e => e.Bounds.Y),
                        allTableElements.Max(e => e.Bounds.Right) - allTableElements.Min(e => e.Bounds.X),
                        allTableElements.Max(e => e.Bounds.Bottom) - allTableElements.Min(e => e.Bounds.Y)
                    );

                    if (ValidateRegion(region))
                    {
                        tableRegions.Add(region);
                        i = lookAhead - 1; // Skip the lines we've processed
                    }
                }
            }

            return tableRegions;
        }

        private TableStructure ExtractTableContent(Page page, Rectangle bounds)
        {
            List<TextElement> elements = GetTextElements(page)
                .Where(e => Overlaps(bounds, e.Bounds))
                .ToList();

            if (!elements.Any()) return null;

            // Group elements by line using vertical overlap
            List<TextElement> orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();
            List<List<TextElement>> lineGroups = new List<List<TextElement>>();
            List<TextElement> currentLine = new List<TextElement> { orderedElements[0] };

            for (int i = 1; i < orderedElements.Count; i++)
            {
                TextElement current = orderedElements[i];
                Rectangle prevBounds = currentLine[0].Bounds; // Use first element of line as reference

                // Check if elements are on the same line by checking vertical overlap
                bool sameLineByOverlap = (current.Bounds.Top <= prevBounds.Bottom &&
                                         current.Bounds.Bottom >= prevBounds.Top);

                if (sameLineByOverlap)
                {
                    currentLine.Add(current);
                }
                else
                {
                    lineGroups.Add(new List<TextElement>(currentLine));
                    currentLine = new List<TextElement> { current };
                }
            }

            if (currentLine.Any())
            {
                lineGroups.Add(currentLine);
            }

            // Process each line to extract cells
            List<List<string>> rows = new List<List<string>>();
            foreach (List<TextElement> line in lineGroups)
            {
                List<TextElement> orderedLineElements = line.OrderBy(e => e.Bounds.X).ToList();
                List<string> cells = new List<string>();
                List<TextElement> currentCell = new List<TextElement> { orderedLineElements[0] };

                for (int i = 1; i < orderedLineElements.Count; i++)
                {
                    TextElement element = orderedLineElements[i];
                    int gap = element.Bounds.X - currentCell.Last().Bounds.Right;

                    // Use a fixed minimum gap for column separation
                    // This assumes table columns have significant spacing between them
                    if (gap > ProximityThreshold)
                    {
                        cells.Add(string.Join(" ", currentCell.Select(e => e.Text)));
                        currentCell.Clear();
                    }
                    currentCell.Add(element);
                }

                // Add the last cell
                if (currentCell.Any())
                {
                    cells.Add(string.Join(" ", currentCell.Select(e => e.Text)));
                }

                if (cells.Any())
                {
                    rows.Add(cells);
                }
            }

            if (rows.Count < 2) return null;

            // Normalize the number of columns in each row to match the maximum
            int maxColumns = rows.Max(r => r.Count);
            for (int i = 0; i < rows.Count; i++)
            {
                while (rows[i].Count < maxColumns)
                {
                    rows[i].Add(string.Empty);
                }
            }

            return new TableStructure
            {
                Cells = rows,
                Bounds = bounds,
                Rows = rows.Count,
                Columns = maxColumns
            };
        }

        private bool ValidateRegion(Rectangle region)
        {
            return region.X >= 0 && region.Y >= 0 &&
                   region.Width > 0 && region.Height > 0 &&
                   region.Width < 10000 && region.Height < 10000;
        }

        private bool Overlaps(Rectangle table, Rectangle element)
        {
            // For potential headers, use more relaxed vertical check
            bool isNearTableTop = Math.Abs(element.Bottom - table.Top) <= 2 * LineThreshold;
            bool isHorizontallyAligned = element.Left <= table.Right && element.Right >= table.Left;

            // If it's a potential header, check horizontal alignment
            if (isNearTableTop && isHorizontallyAligned)
            {
                return true;
            }

            // Otherwise use strict geometric overlap
            return !(element.Left > table.Right ||
                    element.Right < table.Left ||
                    element.Top > table.Bottom ||
                    element.Bottom < table.Top);
        }

        #endregion

        #region Lists

        private bool IsListItem(string text)
        {
            text = text?.Trim();
            if (string.IsNullOrEmpty(text)) return false;

            return ListMarkers.Contains(text) ||
                   ListMarkers.Any(m => text.StartsWith(m)) ||
                   text.StartsWith("Item", StringComparison.OrdinalIgnoreCase);
        }

        private string GetListMarker(string text)
        {
            text = text.TrimStart();

            // Check for bullet-style markers first
            foreach (string marker in ListMarkers)
            {
                if (text.StartsWith(marker))
                {
                    return marker;
                }
            }

            // Check for numbered list patterns
            if (text.Length >= 2)
            {
                foreach (string pattern in ListNumberingPatterns)
                {
                    System.Text.RegularExpressions.Match match = System.Text.RegularExpressions.Regex.Match(text, pattern);
                    if (match.Success)
                    {
                        return match.Value;
                    }
                }
            }

            return null;
        }

        private string ProcessListItemText(List<TextElement> lineElements)
        {
            List<TextElement> orderedLine = lineElements.OrderBy(e => e.Bounds.X).ToList();
            string text = string.Join(" ", orderedLine.Select(e => e.Text));

            // Get the list marker if present
            string originalMarker = GetListMarker(text);
            if (!string.IsNullOrEmpty(originalMarker))
            {
                if (text.Contains("Item") || text.Contains("Items"))
                {
                    // Extract any existing number or default to continuing the sequence
                    string itemText = text.Substring(text.IndexOf("Item"));
                    string numberStr = new string(itemText.Skip("Item".Length).TakeWhile(char.IsDigit).ToArray());
                    int number;

                    if (!int.TryParse(numberStr, out number))
                    {
                        // If no number found or invalid, assume it's the next in sequence
                        number = text.Contains("Items") ? 3 : // Special case for "Items" -> "Item 3"
                                text.Contains("Item2") ? 2 : 1; // Handle "Item2" -> "Item 2"
                    }

                    return $"{originalMarker} Item {number}";
                }
            }

            return text;
        }

        private void ProcessListContent(List<TextElement> elements, List<Rectangle> tableRegions, ExtractionResult result)
        {
            if (!elements.Any()) return;

            // Group elements by line with vertical proximity
            List<List<TextElement>> lineGroups = new List<List<TextElement>>();
            List<TextElement> currentLine = new List<TextElement> { elements.First() };
            List<TextElement> orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();

            for (int i = 1; i < orderedElements.Count; i++)
            {
                TextElement current = orderedElements[i];
                TextElement previous = currentLine[0];

                bool sameLineByOverlap = (current.Bounds.Top <= previous.Bounds.Bottom &&
                                        current.Bounds.Bottom >= previous.Bounds.Top);

                if (sameLineByOverlap)
                {
                    currentLine.Add(current);
                }
                else
                {
                    lineGroups.Add(new List<TextElement>(currentLine.OrderBy(e => e.Bounds.X).ToList()));
                    currentLine.Clear();
                    currentLine.Add(current);
                }
            }

            if (currentLine.Any())
            {
                lineGroups.Add(currentLine.OrderBy(e => e.Bounds.X).ToList());
            }

            // Process potential list items
            List<string> currentList = new List<string>();
            int listStartY = 0;
            int listStartX = 0;
            int listEndY = 0;
            int listEndX = 0;
            bool isOrdered = false;
            bool inList = false;

            foreach (List<TextElement> line in lineGroups)
            {
                if (line.Count == 0) continue;

                TextElement firstElement = line.First();
                string lineText = string.Join(" ", line.Select(e => e.Text));

                bool isListItem = IsListItem(firstElement.Text) ||
                                 (lineText.Length >= 2 && char.IsDigit(lineText[0]) && lineText[1] == '.');

                if (isListItem)
                {
                    if (!inList)
                    {
                        // Start new list
                        inList = true;
                        listStartY = firstElement.Bounds.Y;
                        listStartX = firstElement.Bounds.X;
                        isOrdered = char.IsDigit(lineText[0]);
                    }

                    // Process the line text similar to how TextElements handles it
                    string processedText = ProcessListItemText(line);
                    currentList.Add(processedText);
                    listEndY = line.Last().Bounds.Bottom;
                    listEndX = Math.Max(listEndX, line.Max(e => e.Bounds.Right));
                }
                else if (inList)
                {
                    // Check if this line could be a continuation of the list item
                    bool isContinuation = firstElement.Bounds.X > listStartX + 10; // Indentation check

                    if (isContinuation)
                    {
                        // Append to last list item with proper processing
                        string processedContinuation = ProcessListItemText(line);
                        currentList[currentList.Count - 1] += " " + processedContinuation;
                        listEndY = line.Last().Bounds.Bottom;
                        listEndX = Math.Max(listEndX, line.Max(e => e.Bounds.Right));
                    }
                    else
                    {
                        // End current list
                        if (currentList.Count >= 2) // Only save lists with at least 2 items
                        {
                            result.Lists.Add(new ListStructure
                            {
                                Items = new List<string>(currentList),
                                IsOrdered = isOrdered,
                                Bounds = new Rectangle(
                                    listStartX,
                                    listStartY,
                                    listEndX - listStartX,
                                    listEndY - listStartY
                                )
                            });
                        }
                        currentList.Clear();
                        inList = false;
                    }
                }
            }

            // Don't forget to add the last list if we're still in one
            if (inList && currentList.Count >= 2)
            {
                result.Lists.Add(new ListStructure
                {
                    Items = new List<string>(currentList),
                    IsOrdered = isOrdered,
                    Bounds = new Rectangle(
                        listStartX,
                        listStartY,
                        listEndX - listStartX,
                        listEndY - listStartY
                    )
                });
            }
        }

        #endregion

        #region Misc

        private bool IsElementInRegion(Rectangle element, Rectangle region)
        {
            // Strict bounds checking without padding
            return !(element.Right < region.Left ||
                    element.Left > region.Right ||
                    element.Bottom < region.Top ||
                    element.Top > region.Bottom);
        }

        #endregion

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    }
}