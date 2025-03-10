namespace DocumentAtom.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Xml;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Image;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Spreadsheet;
    using SerializableDataTables;

    /// <summary>
    /// Create atoms from Microsoft Excel .xlsx documents.
    /// </summary>
    public class XlsxProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// Xlsx processor settings.
        /// </summary>
        public new XlsxProcessorSettings Settings
        {
            get
            {
                return _ProcessorSettings;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Settings));
                _ProcessorSettings = value;
            }
        }

        #endregion

        #region Private-Members

        private XlsxProcessorSettings _ProcessorSettings = new XlsxProcessorSettings();
        private ImageProcessorSettings _ImageProcessorSettings = null;
        private ImageProcessor _ImageProcessor = null;
        private HeaderRowDetector _HeaderRowDetector = null;

        private const string _MetadataFile = "docProps/core.xml";
        private const string _MetadataXPath = "/cp:coreProperties";

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from Excel documents.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public XlsxProcessor(XlsxProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new XlsxProcessorSettings();

            Header = "[Xlsx] ";

            _ProcessorSettings = settings;
            _ImageProcessorSettings = imageSettings;
            _HeaderRowDetector = new HeaderRowDetector(_ProcessorSettings);

            if (_ImageProcessorSettings != null) _ImageProcessor = new ImageProcessor(_ImageProcessorSettings);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected new void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _ImageProcessor?.Dispose();
                    _ImageProcessor = null;
                    _HeaderRowDetector?.Dispose();
                    _HeaderRowDetector = null;
                }

                base.Dispose(disposing);
                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public override IEnumerable<Atom> Extract(string filename)
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

            try
            {
                using (ZipArchive archive = ZipFile.OpenRead(filename))
                {
                    archive.ExtractToDirectory(_ProcessorSettings.TempDirectory);
                    if (File.Exists(_ProcessorSettings.TempDirectory + _MetadataFile))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.PreserveWhitespace = true;
                        xmlDoc.Load(_ProcessorSettings.TempDirectory + _MetadataFile);

                        foreach (XmlNode node in xmlDoc.ChildNodes)
                        {
                            if (node.NodeType != XmlNodeType.Element) continue;

                            if (node.HasChildNodes)
                            {
                                foreach (XmlNode child in node.ChildNodes)
                                {
                                    ret.Add(child.LocalName, child.InnerText);
                                }
                            }
                        }
                    }

                    return ret;
                }
            }
            finally
            {
                FileHelper.RecursiveDelete(_ProcessorSettings.TempDirectoryInfo, true);
                Directory.Delete(_ProcessorSettings.TempDirectory, true);
            }
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filename, false))
            {
                var workbookPart = doc.WorkbookPart;
                var sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                var sheets = workbookPart.Workbook.Descendants<Sheet>();

                int sheetNumber = 1;
                foreach (var sheet in sheets)
                {
                    var worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    var sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    var rows = sheetData.Elements<Row>().ToList();

                    // Find all used cells to determine the actual range
                    var usedCells = new HashSet<string>();
                    var cellsByColumn = new Dictionary<string, List<(int RowIndex, string Value)>>();

                    foreach (var row in rows)
                    {
                        foreach (var cell in row.Elements<Cell>())
                        {
                            string cellValue = GetCellValue(cell, sharedStringTable);
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                string columnReference = GetColumnReference(cell.CellReference);
                                usedCells.Add(cell.CellReference);

                                if (!cellsByColumn.ContainsKey(columnReference))
                                {
                                    cellsByColumn[columnReference] = new List<(int, string)>();
                                }

                                cellsByColumn[columnReference].Add((GetRowIndex(cell.CellReference), cellValue));
                            }
                        }
                    }

                    // Process standalone text cells (not part of tables)
                    foreach (var columnData in cellsByColumn)
                    {
                        bool isIsolatedColumn = true;
                        string columnRef = columnData.Key;

                        // Check if this column is part of a table by looking for adjacent columns with data
                        string prevCol = GetPreviousColumn(columnRef);
                        string nextCol = GetNextColumn(columnRef);

                        if (cellsByColumn.ContainsKey(prevCol) || cellsByColumn.ContainsKey(nextCol))
                        {
                            isIsolatedColumn = false;
                            continue; // Skip, as this will be handled in table processing
                        }

                        if (isIsolatedColumn)
                        {
                            foreach (var cellData in columnData.Value)
                            {
                                string cellIdentifier = $"{columnRef}{cellData.RowIndex}";
                                yield return new Atom
                                {
                                    Type = AtomTypeEnum.Text,
                                    Text = cellData.Value,
                                    PageNumber = sheetNumber,
                                    SheetName = sheet.Name,
                                    CellIdentifier = cellIdentifier,
                                    MD5Hash = HashHelper.MD5Hash(cellData.Value),
                                    SHA1Hash = HashHelper.SHA1Hash(cellData.Value),
                                    SHA256Hash = HashHelper.SHA256Hash(cellData.Value),
                                    Length = cellData.Value.Length
                                };
                            }
                        }
                    }

                    var dt = new DataTable(sheet.Name);
                    if (rows.Any())
                    {
                        // Process header row
                        var headerRow = rows.First();
                        foreach (var cell in headerRow.Elements<Cell>())
                        {
                            string columnName = GetCellValue(cell, sharedStringTable);
                            if (string.IsNullOrEmpty(columnName))
                                columnName = $"Column{dt.Columns.Count + 1}";

                            // Handle duplicate column names
                            string uniqueColumnName = columnName;
                            int counter = 1;
                            while (dt.Columns.Contains(uniqueColumnName))
                            {
                                uniqueColumnName = $"{columnName}_{counter++}";
                            }

                            dt.Columns.Add(uniqueColumnName);
                        }

                        // Process data rows
                        foreach (var row in rows.Skip(1))
                        {
                            DataRow dataRow = dt.NewRow();

                            // Initialize all columns to empty string or null
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                dataRow[i] = DBNull.Value;
                            }

                            // Process each cell by its reference
                            foreach (var cell in row.Elements<Cell>())
                            {
                                // Get the column reference from the cell's reference (e.g., "B5" -> "B")
                                string cellReference = cell.CellReference.ToString();
                                string columnReference = GetColumnReference(cellReference);

                                // Convert column reference to index (e.g., "A" -> 0, "B" -> 1)
                                int columnIndex = GetColumnIndex(columnReference);

                                if (columnIndex < dt.Columns.Count)
                                {
                                    dataRow[columnIndex] = GetCellValue(cell, sharedStringTable);
                                }
                            }

                            dt.Rows.Add(dataRow);
                        }

                        // Create atom for the sheet - FIXED SECTION
                        string tableRange;
                        if (headerRow.Elements<Cell>().Any())
                        {
                            var firstCell = headerRow.Elements<Cell>().First().CellReference;

                            if (rows.Any() && rows.Last().Elements<Cell>().Any())
                            {
                                var lastCell = rows.Last().Elements<Cell>().Last().CellReference;
                                tableRange = $"{firstCell}:{lastCell}";
                            }
                            else
                            {
                                // Last row has no cells, use first cell as range
                                tableRange = $"{firstCell}:{firstCell}";
                            }
                        }
                        else
                        {
                            // No cells in header row, use sheet name
                            tableRange = sheet.Name;
                        }

                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Table,
                            Table = SerializableDataTable.FromDataTable(dt),
                            PageNumber = sheetNumber,
                            SheetName = sheet.Name,
                            CellIdentifier = tableRange,
                            MD5Hash = HashHelper.MD5Hash(dt),
                            SHA1Hash = HashHelper.SHA1Hash(dt),
                            SHA256Hash = HashHelper.SHA256Hash(dt),
                            Length = DataTableHelper.GetLength(dt)
                        };
                    }

                    // Process images in the worksheet
                    if (worksheetPart.DrawingsPart != null)
                    {
                        foreach (var imagePart in worksheetPart.DrawingsPart.ImageParts)
                        {
                            using (var stream = imagePart.GetStream())
                            using (var ms = new MemoryStream())
                            {
                                stream.CopyTo(ms);
                                byte[] bytes = ms.ToArray();
                                string imageLocation = GetImageLocation(worksheetPart, imagePart) ?? "Unknown";

                                Atom atom = new Atom
                                {
                                    Type = AtomTypeEnum.Binary,
                                    Binary = bytes,
                                    PageNumber = sheetNumber,
                                    SheetName = sheet.Name,
                                    CellIdentifier = imageLocation,
                                    MD5Hash = HashHelper.MD5Hash(bytes),
                                    SHA1Hash = HashHelper.SHA1Hash(bytes),
                                    SHA256Hash = HashHelper.SHA256Hash(bytes),
                                    Length = bytes.Length
                                };

                                if (_ImageProcessor != null) atom.Quarks = _ImageProcessor.Extract(bytes).ToList();

                                yield return atom;
                            }
                        }
                    }

                    sheetNumber++;
                }
            }
        }

        private string GetColumnReference(string cellReference)
        {
            return string.Concat(cellReference.TakeWhile(c => !char.IsDigit(c)));
        }

        private int GetColumnIndex(string columnReference)
        {
            int columnIndex = 0;
            foreach (char c in columnReference)
            {
                columnIndex = columnIndex * 26 + (c - 'A' + 1);
            }
            return columnIndex - 1; // Zero-based index
        }

        private int GetRowIndex(string cellReference)
        {
            return int.Parse(string.Concat(cellReference.SkipWhile(c => !char.IsDigit(c))));
        }

        private string GetPreviousColumn(string column)
        {
            if (string.IsNullOrEmpty(column)) return null;

            char[] chars = column.ToCharArray();
            int i = chars.Length - 1;

            while (i >= 0)
            {
                if (chars[i] == 'A')
                {
                    chars[i] = 'Z';
                    i--;
                    continue;
                }

                chars[i]--;
                break;
            }

            return new string(chars);
        }

        private string GetNextColumn(string column)
        {
            if (string.IsNullOrEmpty(column)) return "A";

            char[] chars = column.ToCharArray();
            int i = chars.Length - 1;

            while (i >= 0)
            {
                if (chars[i] == 'Z')
                {
                    chars[i] = 'A';
                    i--;
                    continue;
                }

                chars[i]++;
                return new string(chars);
            }

            return "A" + new string(chars);
        }

        private string GetImageLocation(WorksheetPart worksheetPart, ImagePart imagePart)
        {
            try
            {
                var drawingsPart = worksheetPart.DrawingsPart;
                if (drawingsPart == null) return null;

                var imageId = drawingsPart.GetIdOfPart(imagePart);

                // Look for the image reference in drawings
                var twoCellAnchors = drawingsPart.WorksheetDrawing.Descendants<DocumentFormat.OpenXml.Drawing.Spreadsheet.TwoCellAnchor>();
                foreach (var anchor in twoCellAnchors)
                {
                    var blip = anchor.Descendants<DocumentFormat.OpenXml.Drawing.Blip>()
                        .FirstOrDefault(b => b.Embed?.Value == imageId);

                    if (blip != null)
                    {
                        var from = anchor.FromMarker;
                        return $"{GetColumnName(int.Parse(from.ColumnId.Text))}{from.RowId.Text}";
                    }
                }
            }
            catch
            {
                // If we can't determine the location, return null
                return null;
            }

            return null;
        }

        private string GetColumnName(int columnNumber)
        {
            string columnName = "";
            while (columnNumber > 0)
            {
                int remainder = (columnNumber - 1) % 26;
                columnName = ((char)(65 + remainder)).ToString() + columnName;
                columnNumber = (columnNumber - 1) / 26;
            }
            return columnName;
        }

        private string GetCellValue(Cell cell, SharedStringTable sharedStringTable)
        {
            if (cell.CellValue == null)
                return string.Empty;

            string value = cell.CellValue.Text;

            // If the cell represents a shared string
            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return sharedStringTable.ElementAt(int.Parse(value)).InnerText;
            }

            return value;
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8604 // Possible null reference argument.
    }
}