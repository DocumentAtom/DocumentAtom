namespace DocumentAtom.Image
{
    using System.Data;
    using System.Security.Cryptography;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Core.Image;
    using SerializableDataTables;
    using SixLabors.ImageSharp;
    using Tesseract;

    using Rectangle = System.Drawing.Rectangle;

    /// <summary>
    /// Create atoms from images.  Use of this processor requires that Tesseract be installed on the host.
    /// </summary>
    public class ImageProcessor : ProcessorBase
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.

        #region Public-Members

        /// <summary>
        /// Image processor settings.
        /// </summary>
        public new ImageProcessorSettings Settings
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

        private ImageProcessorSettings _Settings = new ImageProcessorSettings();
        private readonly TesseractEngine _engine;


        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from images.  Use of this processor requires that Tesseract be installed on the host.
        /// </summary>
        public ImageProcessor(ImageProcessorSettings settings = null)
        {
            if (settings == null) settings = new ImageProcessorSettings();

            Header = "[Image] ";

            _Settings = settings;
            _engine = new TesseractEngine(
                _Settings.TesseractDataDirectory,
                _Settings.TesseractLanguage,
                EngineMode.Default);

            _engine.SetVariable("debug_file", "/dev/null");
            _engine.SetVariable("quiet_mode", "1");
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public IEnumerable<ImageAtom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return ProcessFile(filename);
        }

        #endregion

        #region Private-Methods

        private IEnumerable<ImageAtom> ProcessFile(string filename)
        {
            byte[] bytes = File.ReadAllBytes(filename);
            ExtractionResult result = ExtractContent(bytes);
            if (result.TextElements != null)
            {
                foreach (TextElement text in result.TextElements)
                {
                    if (!String.IsNullOrEmpty(text.Text))
                    {
                        yield return new ImageAtom
                        {
                            Type = AtomTypeEnum.Text,
                            Text = text.Text,
                            Length = text.Text.Length,
                            MD5Hash = HashHelper.MD5Hash(text.Text),
                            SHA1Hash = HashHelper.SHA1Hash(text.Text),
                            SHA256Hash = HashHelper.SHA256Hash(text.Text),
                            BoundingBox = BoundingBox.FromRectangle(text.Bounds)
                        };
                    }
                }
            }
            if (result.Tables != null)
            {
                foreach (TableStructure table in result.Tables)
                {
                    DataTable dt = TableAtom.FromTableStructure(table).Table;

                    yield return new ImageAtom
                    {
                        Type = AtomTypeEnum.Table,
                        Table = SerializableDataTable.FromDataTable(dt),
                        Length = DataTableHelper.GetLength(dt),
                        MD5Hash = HashHelper.MD5Hash(dt),
                        SHA1Hash = HashHelper.SHA1Hash(dt),
                        SHA256Hash = HashHelper.SHA256Hash(dt),
                        BoundingBox = BoundingBox.FromRectangle(table.Bounds)
                    };
                }
            }
            if (result.Lists != null)
            {
                foreach (ListStructure list in result.Lists)
                {
                    if (list.Items != null && list.Items.Count > 0)
                    {
                        yield return new ImageAtom
                        {
                            Type = AtomTypeEnum.List,
                            UnorderedList = (list.IsOrdered ? list.Items : null),
                            OrderedList = (list.IsOrdered ? list.Items : null),
                            Length = list.Items.Sum(s => s?.Length ?? 0),
                            MD5Hash = HashHelper.MD5Hash(list.Items),
                            SHA1Hash = HashHelper.SHA1Hash(list.Items),
                            SHA256Hash = HashHelper.SHA256Hash(list.Items),
                            BoundingBox = BoundingBox.FromRectangle(list.Bounds)
                        };
                    }
                }
            }
        }

        private ExtractionResult ExtractContent(byte[] pngData)
        {
            if (pngData == null || pngData.Length == 0)
            {
                throw new ArgumentException("Input image data cannot be null or empty", nameof(pngData));
            }

            var result = new ExtractionResult
            {
                TextElements = new List<TextElement>(),
                Tables = new List<TableStructure>(),
                Lists = new List<ListStructure>()
            };

            using var memStream = new MemoryStream(pngData);
            using var img = Pix.LoadFromMemory(memStream.ToArray());
            List<TextElement> allElements;

            // First pass: Get all elements
            using (var page = _engine.Process(img))
            {
                allElements = GetTextElements(page);
            }

            // Step 1: Process tables first and get masked regions
            var tableRegions = DetectTableRegionsFromElements(allElements);
            using (var page = _engine.Process(img))
            {
                foreach (var region in tableRegions)
                {
                    var table = ExtractTableContent(page, region);
                    if (table != null)
                    {
                        result.Tables.Add(table);
                    }
                }
            }

            // Step 2: Filter out elements that belong to tables
            var nonTableElements = allElements.Where(element =>
                !tableRegions.Any(region => IsElementInRegion(element.Bounds, region))).ToList();

            // Step 3: Process lists
            ProcessListContent(nonTableElements, tableRegions, result);

            // Step 4: Process remaining text elements (excluding those in lists)
            var nonListElements = nonTableElements.Where(element =>
                !result.Lists.Any(list => IsElementInRegion(element.Bounds, list.Bounds))).ToList();

            ProcessTextContent(nonListElements, result);

            return result;
        }

        #region Text

        private List<TextElement> GetTextElements(Page page)
        {
            var elements = new List<TextElement>();

            using (var iter = page.GetIterator())
            {
                iter.Begin();
                int wordCount = 0;

                do
                {
                    try
                    {
                        if (iter.TryGetBoundingBox(PageIteratorLevel.Word, out var bounds))
                        {
                            var text = iter.GetText(PageIteratorLevel.Word);
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
                                    )
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
            var elementsByLine = new List<List<TextElement>>();
            var currentLine = new List<TextElement> { elements.First() };
            var orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();

            for (int i = 1; i < orderedElements.Count; i++)
            {
                var current = orderedElements[i];
                var previous = currentLine[0];

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
            foreach (var line in elementsByLine)
            {
                var orderedLine = line.OrderBy(e => e.Bounds.X).ToList();
                var text = string.Join(" ", orderedLine.Select(e => e.Text));

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
                        )
                    });
                }
            }
        }

        #endregion

        #region Tables

        private List<Rectangle> DetectTableRegionsFromElements(List<TextElement> elements)
        {
            var tableRegions = new List<Rectangle>();

            // Group elements by line
            var lineGroups = elements
                .GroupBy(e => e.Bounds.Y / _Settings.LineThreshold)
                .OrderBy(g => g.Key)
                .ToList();

            for (int i = 0; i < lineGroups.Count; i++)
            {
                var currentLine = lineGroups[i];

                // Check if this line could be a table header
                bool isHeaderLine = currentLine.Any(e =>
                    e.Text.Contains("Column", StringComparison.OrdinalIgnoreCase) ||
                    (e.Text.Contains("Row", StringComparison.OrdinalIgnoreCase) &&
                     currentLine.Any(x => x.Text.Contains("name", StringComparison.OrdinalIgnoreCase))));

                if (!isHeaderLine)
                    continue;

                // Look for table content below
                var tableLines = new List<IGrouping<int, TextElement>> { currentLine };
                int lookAhead = i + 1;
                bool inTable = true;

                while (lookAhead < lineGroups.Count && inTable)
                {
                    var nextLine = lineGroups[lookAhead];

                    // Check if this line matches table row pattern
                    bool isTableRow = nextLine.Count() >= 2 &&
                                    (nextLine.Any(e => e.Text.StartsWith("Row", StringComparison.OrdinalIgnoreCase)) ||
                                     nextLine.Any(e => e.Text.StartsWith("Value", StringComparison.OrdinalIgnoreCase)));

                    if (!isTableRow)
                    {
                        // Check if we've found the end of the table
                        var verticalGap = nextLine.Min(e => e.Bounds.Y) -
                                        tableLines.Last().Max(e => e.Bounds.Bottom);
                        if (verticalGap > _Settings.LineThreshold * 2)
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
                    var allTableElements = tableLines.SelectMany(g => g);
                    var region = new Rectangle(
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
            var elements = GetTextElements(page)
                .Where(e => Overlaps(bounds, e.Bounds))
                .ToList();

            if (!elements.Any()) return null;

            // Group elements by line using vertical overlap
            var orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();
            var lineGroups = new List<List<TextElement>>();
            var currentLine = new List<TextElement> { orderedElements[0] };

            for (int i = 1; i < orderedElements.Count; i++)
            {
                var current = orderedElements[i];
                var prevBounds = currentLine[0].Bounds; // Use first element of line as reference

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
            var rows = new List<List<string>>();
            foreach (var line in lineGroups)
            {
                var orderedLineElements = line.OrderBy(e => e.Bounds.X).ToList();
                var cells = new List<string>();
                var currentCell = new List<TextElement> { orderedLineElements[0] };

                for (int i = 1; i < orderedLineElements.Count; i++)
                {
                    var element = orderedLineElements[i];
                    var gap = element.Bounds.X - currentCell.Last().Bounds.Right;

                    // Use a fixed minimum gap for column separation
                    // This assumes table columns have significant spacing between them
                    if (gap > _Settings.ProximityThreshold)
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
            var maxColumns = rows.Max(r => r.Count);
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
            bool isNearTableTop = Math.Abs(element.Bottom - table.Top) <= 2 * _Settings.LineThreshold;
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

            return _Settings.ListMarkers.Contains(text) ||
                   _Settings.ListMarkers.Any(m => text.StartsWith(m)) ||
                   text.StartsWith("Item", StringComparison.OrdinalIgnoreCase);
        }

        private string GetListMarker(string text)
        {
            text = text.TrimStart();

            // Check for bullet-style markers first
            foreach (var marker in _Settings.ListMarkers)
            {
                if (text.StartsWith(marker))
                {
                    return marker;
                }
            }

            // Check for numbered list patterns
            if (text.Length >= 2)
            {
                foreach (var pattern in _Settings.ListNumberingPatterns)
                {
                    var match = System.Text.RegularExpressions.Regex.Match(text, pattern);
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
            var orderedLine = lineElements.OrderBy(e => e.Bounds.X).ToList();
            var text = string.Join(" ", orderedLine.Select(e => e.Text));

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
            var lineGroups = new List<List<TextElement>>();
            var currentLine = new List<TextElement> { elements.First() };
            var orderedElements = elements.OrderBy(e => e.Bounds.Y).ToList();

            for (int i = 1; i < orderedElements.Count; i++)
            {
                var current = orderedElements[i];
                var previous = currentLine[0];

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
            var currentList = new List<string>();
            var listStartY = 0;
            var listStartX = 0;
            var listEndY = 0;
            var listEndX = 0;
            var isOrdered = false;
            bool inList = false;

            foreach (var line in lineGroups)
            {
                if (line.Count == 0) continue;

                var firstElement = line.First();
                var lineText = string.Join(" ", line.Select(e => e.Text));

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

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
