namespace DocumentAtom.Documents.Excel
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
    using DocumentAtom.Documents.Image;
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

            List<Atom> flatAtoms = ProcessFile(filename).ToList();

            if (_ProcessorSettings.BuildHierarchy)
            {
                return BuildHierarchy(flatAtoms);
            }
            else
            {
                // Ensure ParentGUID is null for flat list
                foreach (Atom atom in flatAtoms)
                {
                    atom.ParentGUID = null;
                }
                return flatAtoms;
            }
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

        /// <summary>
        /// Build hierarchical structure from flat list of atoms.
        /// Groups atoms by PageNumber (sheet number), making the first atom on each page the parent.
        /// </summary>
        /// <param name="flatAtoms">Flat list of atoms.</param>
        /// <returns>Root-level atoms with hierarchical structure.</returns>
        private IEnumerable<Atom> BuildHierarchy(List<Atom> flatAtoms)
        {
            if (flatAtoms == null || flatAtoms.Count == 0)
            {
                return Enumerable.Empty<Atom>();
            }

            // Group atoms by PageNumber (sheet number)
            IOrderedEnumerable<IGrouping<int?, Atom>> atomsByPage = flatAtoms.GroupBy(a => a.PageNumber).OrderBy(g => g.Key);

            List<Atom> rootAtoms = new List<Atom>();

            foreach (IGrouping<int?, Atom> pageGroup in atomsByPage)
            {
                List<Atom> atomsOnPage = pageGroup.ToList();

                if (atomsOnPage.Count == 0) continue;

                // First atom on the page becomes the parent
                Atom parentAtom = atomsOnPage[0];
                parentAtom.ParentGUID = null;
                rootAtoms.Add(parentAtom);

                // Remaining atoms become Quarks of the parent
                if (atomsOnPage.Count > 1)
                {
                    parentAtom.Quarks = new List<Atom>();

                    for (int i = 1; i < atomsOnPage.Count; i++)
                    {
                        Atom childAtom = atomsOnPage[i];
                        childAtom.ParentGUID = parentAtom.GUID;
                        parentAtom.Quarks.Add(childAtom);
                    }
                }
            }

            return rootAtoms;
        }

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            int position = 0;

            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(filename, false))
            {
                WorkbookPart workbookPart = doc.WorkbookPart;
                SharedStringTable sharedStringTable = workbookPart.SharedStringTablePart?.SharedStringTable;
                IEnumerable<Sheet> sheets = workbookPart.Workbook.Descendants<Sheet>();

                int sheetNumber = 1;
                foreach (Sheet sheet in sheets)
                {
                    WorksheetPart worksheetPart = (WorksheetPart)workbookPart.GetPartById(sheet.Id);
                    SheetData sheetData = worksheetPart.Worksheet.Elements<SheetData>().First();
                    List<Row> rows = sheetData.Elements<Row>().ToList();

                    HeaderRowResult headerRowResult = _HeaderRowDetector.Process(rows, sharedStringTable);
                    bool hasHeaderRow = headerRowResult.IsHeaderRow;

                    Log(SeverityEnum.Debug,
                        "header row detection result: " + hasHeaderRow + Environment.NewLine +
                        Serializer.SerializeJson(headerRowResult, false));

                    // Find all used cells to determine the actual range
                    HashSet<string> usedCells = new HashSet<string>();
                    Dictionary<string, List<CellData>> cellsByColumn = new Dictionary<string, List<CellData>>();

                    foreach (Row row in rows)
                    {
                        foreach (Cell cell in row.Elements<Cell>())
                        {
                            string cellValue = GetCellValue(cell, sharedStringTable);
                            if (!string.IsNullOrWhiteSpace(cellValue))
                            {
                                string columnReference = GetColumnReference(cell.CellReference);
                                usedCells.Add(cell.CellReference);

                                if (!cellsByColumn.ContainsKey(columnReference))
                                {
                                    cellsByColumn[columnReference] = new List<CellData>();
                                }

                                cellsByColumn[columnReference].Add(new CellData(GetRowIndex(cell.CellReference), cellValue));
                            }
                        }
                    }

                    // Process standalone text cells (not part of tables)
                    foreach (KeyValuePair<string, List<CellData>> columnData in cellsByColumn)
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
                            foreach (CellData cellData in columnData.Value)
                            {
                                string cellIdentifier = $"{columnRef}{cellData.RowIndex}";
                                yield return new Atom
                                {
                                    Type = AtomTypeEnum.Text,
                                    Text = cellData.Value,
                                    PageNumber = sheetNumber,
                                    SheetName = sheet.Name,
                                    CellIdentifier = cellIdentifier,
                                    Position = position++,
                                    MD5Hash = HashHelper.MD5Hash(cellData.Value),
                                    SHA1Hash = HashHelper.SHA1Hash(cellData.Value),
                                    SHA256Hash = HashHelper.SHA256Hash(cellData.Value),
                                    Length = cellData.Value.Length
                                };
                            }
                        }
                    }

                    // Create DataTable for the current worksheet
                    DataTable dt = new DataTable(sheet.Name);
                    if (rows.Any())
                    {
                        // Determine the maximum column index across all rows to ensure we capture all columns
                        int maxColumnIndex = 0;
                        HashSet<string> columnReferences = new HashSet<string>();

                        // Gather all column references from all rows to determine the maximum column index
                        foreach (Row row in rows)
                        {
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                string colRef = GetColumnReference(cell.CellReference);
                                columnReferences.Add(colRef);
                                int colIdx = GetColumnIndex(colRef);
                                maxColumnIndex = Math.Max(maxColumnIndex, colIdx);
                            }
                        }

                        // Ensure we have enough columns in the DataTable
                        for (int i = 0; i <= maxColumnIndex; i++)
                        {
                            dt.Columns.Add($"Column{i + 1}");
                        }

                        // If we have a header row, use the first row values as column names
                        if (hasHeaderRow && rows.Count > 0)
                        {
                            Row headerRow = rows.First();

                            // Map from column index to column name
                            Dictionary<int, string> columnNames = new Dictionary<int, string>();

                            // Process header row cells to get column names
                            foreach (Cell cell in headerRow.Elements<Cell>())
                            {
                                string colRef = GetColumnReference(cell.CellReference);
                                int colIdx = GetColumnIndex(colRef);
                                string columnName = GetCellValue(cell, sharedStringTable);

                                // Use default column name if header cell is empty
                                if (string.IsNullOrEmpty(columnName))
                                {
                                    columnName = $"Column{colIdx + 1}";
                                }

                                columnNames[colIdx] = columnName;
                            }

                            // Rename columns in DataTable based on header row
                            foreach (int colIdx in columnNames.Keys)
                            {
                                if (colIdx < dt.Columns.Count)
                                {
                                    string uniqueColumnName = columnNames[colIdx];
                                    int counter = 1;

                                    // Handle duplicate column names
                                    while (dt.Columns.Cast<DataColumn>()
                                             .Any(c => c.ColumnName != dt.Columns[colIdx].ColumnName &&
                                                       c.ColumnName == uniqueColumnName))
                                    {
                                        uniqueColumnName = $"{columnNames[colIdx]}_{counter++}";
                                    }

                                    dt.Columns[colIdx].ColumnName = uniqueColumnName;
                                }
                            }
                        }

                        // Process data rows - skip the first row if it's a header
                        IEnumerable<Row> dataRows = hasHeaderRow ? rows.Skip(1) : rows;

                        foreach (Row row in dataRows)
                        {
                            DataRow dataRow = dt.NewRow();

                            // Initialize all columns to DBNull.Value
                            for (int i = 0; i < dt.Columns.Count; i++)
                            {
                                dataRow[i] = DBNull.Value;
                            }

                            // Process each cell in the row
                            foreach (Cell cell in row.Elements<Cell>())
                            {
                                string cellRef = cell.CellReference.ToString();
                                string colRef = GetColumnReference(cellRef);
                                int colIdx = GetColumnIndex(colRef);

                                if (colIdx < dt.Columns.Count)
                                {
                                    // Get and assign cell value
                                    dataRow[colIdx] = GetCellValue(cell, sharedStringTable);
                                }
                            }

                            dt.Rows.Add(dataRow);
                        }

                        // Determine table range for cell identifier
                        string tableRange;

                        // Get the first and last cells to define the range
                        if (rows.Any() && rows.First().Elements<Cell>().Any())
                        {
                            Row firstRow = rows.First();
                            Row lastRow = rows.Last();

                            if (firstRow.Elements<Cell>().Any() && lastRow.Elements<Cell>().Any())
                            {
                                StringValue firstCell = firstRow.Elements<Cell>().First().CellReference;
                                StringValue lastCell = lastRow.Elements<Cell>().Last().CellReference;
                                tableRange = $"{firstCell}:{lastCell}";
                            }
                            else
                            {
                                // Fallback if we can't get both first and last cell
                                tableRange = sheet.Name;
                            }
                        }
                        else
                        {
                            // No cells in any row, use sheet name
                            tableRange = sheet.Name;
                        }

                        // Create and return the table atom
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Table,
                            Table = SerializableDataTable.FromDataTable(dt),
                            PageNumber = sheetNumber,
                            SheetName = sheet.Name,
                            CellIdentifier = tableRange,
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(dt),
                            SHA1Hash = HashHelper.SHA1Hash(dt),
                            SHA256Hash = HashHelper.SHA256Hash(dt),
                            Length = DataTableHelper.GetLength(dt)
                        };
                    }

                    // Process images in the worksheet
                    if (worksheetPart.DrawingsPart != null)
                    {
                        foreach (ImagePart imagePart in worksheetPart.DrawingsPart.ImageParts)
                        {
                            using (Stream stream = imagePart.GetStream())
                            using (MemoryStream ms = new MemoryStream())
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
                                    Position = position++,
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
                DrawingsPart drawingsPart = worksheetPart.DrawingsPart;
                if (drawingsPart == null) return null;

                string imageId = drawingsPart.GetIdOfPart(imagePart);

                // Look for the image reference in drawings
                IEnumerable<DocumentFormat.OpenXml.Drawing.Spreadsheet.TwoCellAnchor> twoCellAnchors = drawingsPart.WorksheetDrawing.Descendants<DocumentFormat.OpenXml.Drawing.Spreadsheet.TwoCellAnchor>();
                foreach (DocumentFormat.OpenXml.Drawing.Spreadsheet.TwoCellAnchor anchor in twoCellAnchors)
                {
                    DocumentFormat.OpenXml.Drawing.Blip blip = anchor.Descendants<DocumentFormat.OpenXml.Drawing.Blip>()
                        .FirstOrDefault(b => b.Embed?.Value == imageId);

                    if (blip != null)
                    {
                        DocumentFormat.OpenXml.Drawing.Spreadsheet.FromMarker from = anchor.FromMarker;
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