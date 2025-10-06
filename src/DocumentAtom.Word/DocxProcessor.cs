namespace DocumentAtom.Word
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Core.Office;
    using DocumentAtom.Image;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Wordprocessing;
    using SerializableDataTables;

    /// <summary>
    /// Create atoms from Microsoft Word .docx documents.
    /// </summary>
    public class DocxProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8603 // Possible null reference return.

        #region Public-Members

        /// <summary>
        /// Docx processor settings.
        /// </summary>
        public new DocxProcessorSettings Settings
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

        private DocxProcessorSettings _ProcessorSettings = new DocxProcessorSettings();
        private ImageProcessorSettings _ImageProcessorSettings = null;
        private ImageProcessor _ImageProcessor = null;

        private const string _WXmlNamespace = @"http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string _CpXmlNamespace = @"http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string _DcXmlNamespace = @"http://purl.org/dc/elements/1.1/";
        private const string _AXmlNamespace = @"http://schemas.openxmlformats.org/drawingml/2006/main";
        private const string _RXmlNamespace = @"http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string _PXmlNamespace = @"http://schemas.openxmlformats.org/presentationml/2006/main";

        private const string _MetadataFile = "docProps/core.xml";
        private const string _MetadataXPath = "/cp:coreProperties";

        private const string _RelationshipsFile = "word/_rels/document.xml.rels";
        private Dictionary<string, string> _Relationships = new Dictionary<string, string>();

        private const string _DocumentBodyFile = "word/document.xml";
        private const string _DocumentBodyXPath = "/w:document/w:body";

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

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
                    _Relationships = null;
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
        /// Create atoms from text documents.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public DocxProcessor(DocxProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new DocxProcessorSettings();

            Header = "[Docx] ";

            _ProcessorSettings = settings;
            _ImageProcessorSettings = imageSettings;

            if (_ImageProcessorSettings != null) _ImageProcessor = new ImageProcessor(_ImageProcessorSettings);
        }

        #endregion

        #region Public-Methods

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
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(_ProcessorSettings.TempDirectory + _MetadataFile);

                    XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDoc.NameTable);
                    nsmgr.AddNamespace("w", _WXmlNamespace);
                    nsmgr.AddNamespace("cp", _CpXmlNamespace);
                    nsmgr.AddNamespace("dc", _DcXmlNamespace);
                    nsmgr.AddNamespace("a", _AXmlNamespace);
                    nsmgr.AddNamespace("r", _RXmlNamespace);
                    nsmgr.AddNamespace("p", _PXmlNamespace);

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
        /// </summary>
        /// <param name="flatAtoms">Flat list of atoms.</param>
        /// <returns>Root-level atoms with hierarchical structure.</returns>
        private IEnumerable<Atom> BuildHierarchy(List<Atom> flatAtoms)
        {
            if (flatAtoms == null || flatAtoms.Count == 0)
            {
                return Enumerable.Empty<Atom>();
            }

            // Track the current header at each level (1-9 for Word heading styles)
            Dictionary<int, Atom> currentHeaders = new Dictionary<int, Atom>();

            // Root-level atoms (top of tree)
            List<Atom> rootAtoms = new List<Atom>();

            foreach (Atom atom in flatAtoms)
            {
                if (atom.HeaderLevel != null && atom.HeaderLevel.Value > 0)
                {
                    // This is a header atom
                    int level = atom.HeaderLevel.Value;

                    // Find parent header (nearest header with level < current level)
                    Atom parent = FindParentHeader(currentHeaders, level);

                    if (parent != null)
                    {
                        // Add this header as a Quark (child) of the parent
                        if (parent.Quarks == null)
                        {
                            parent.Quarks = new List<Atom>();
                        }
                        atom.ParentGUID = parent.GUID;
                        parent.Quarks.Add(atom);
                    }
                    else
                    {
                        // No parent found - this is a root-level header
                        atom.ParentGUID = null;
                        rootAtoms.Add(atom);
                    }

                    // Update current header tracking
                    currentHeaders[level] = atom;

                    // Clear tracking for deeper levels (they're now out of scope)
                    ClearDeeperLevels(currentHeaders, level);
                }
                else
                {
                    // This is a non-header atom (text, list, table, image, etc.)
                    // Add it to the deepest current header, or to root if no headers exist
                    Atom parent = FindDeepestHeader(currentHeaders);

                    if (parent != null)
                    {
                        // Add as Quark to deepest header
                        if (parent.Quarks == null)
                        {
                            parent.Quarks = new List<Atom>();
                        }
                        atom.ParentGUID = parent.GUID;
                        parent.Quarks.Add(atom);
                    }
                    else
                    {
                        // No headers yet - add to root
                        atom.ParentGUID = null;
                        rootAtoms.Add(atom);
                    }
                }
            }

            return rootAtoms;
        }

        /// <summary>
        /// Find the parent header for a given header level.
        /// Returns the nearest header with level less than the specified level.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <param name="level">Header level to find parent for.</param>
        /// <returns>Parent header atom, or null if no parent exists.</returns>
        private Atom FindParentHeader(Dictionary<int, Atom> currentHeaders, int level)
        {
            // Search backwards from level-1 down to 1
            for (int i = level - 1; i >= 1; i--)
            {
                if (currentHeaders.ContainsKey(i))
                {
                    return currentHeaders[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find the deepest (highest level number) current header.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <returns>Deepest header atom, or null if no headers exist.</returns>
        private Atom FindDeepestHeader(Dictionary<int, Atom> currentHeaders)
        {
            if (currentHeaders.Count == 0)
            {
                return null;
            }

            int deepestLevel = currentHeaders.Keys.Max();
            return currentHeaders[deepestLevel];
        }

        /// <summary>
        /// Clear tracking for header levels deeper than the specified level.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <param name="level">Current level.</param>
        private void ClearDeeperLevels(Dictionary<int, Atom> currentHeaders, int level)
        {
            // Remove all levels > current level (max Word heading level is 9)
            for (int i = level + 1; i <= 9; i++)
            {
                currentHeaders.Remove(i);
            }
        }

        /// <summary>
        /// Get the heading level from a paragraph's style.
        /// </summary>
        /// <param name="paragraph">Paragraph to analyze.</param>
        /// <returns>Heading level (1-9) or null if not a heading.</returns>
        private int? GetHeadingLevel(Paragraph paragraph)
        {
            var styleId = paragraph.ParagraphProperties?.ParagraphStyleId?.Val?.Value;

            if (String.IsNullOrEmpty(styleId))
            {
                return null;
            }

            // Common heading style patterns:
            // - "Heading1", "Heading2", etc.
            // - "heading1", "heading2", etc.
            // - "Heading 1", "Heading 2", etc.
            // - "Title" (treat as Heading 1)
            // - "Subtitle" (treat as Heading 2)

            string styleLower = styleId.ToLower().Replace(" ", "");

            if (styleLower == "title")
            {
                return 1;
            }

            if (styleLower == "subtitle")
            {
                return 2;
            }

            // Try to extract number from "heading1", "heading2", etc.
            if (styleLower.StartsWith("heading"))
            {
                string numberPart = styleLower.Substring("heading".Length);
                if (int.TryParse(numberPart, out int level) && level >= 1 && level <= 9)
                {
                    return level;
                }
            }

            return null;
        }

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            int position = 0;

            using (WordprocessingDocument doc = WordprocessingDocument.Open(filename, false))
            {
                var body = doc.MainDocumentPart.Document.Body;

                List<string> currentList = null;
                bool isOrderedList = false;

                foreach (var element in body.Elements())
                {
                    if (element is Paragraph paragraph)
                    {
                        var numProperties = paragraph.ParagraphProperties?.NumberingProperties;
                        var isListItem = numProperties != null;

                        if (isListItem)
                        {
                            if (currentList == null)
                            {
                                currentList = new List<string>();
                                isOrderedList = IsOrderedList(doc, numProperties);
                            }

                            currentList.Add(ExtractTextFromParagraph(paragraph));
                        }
                        else
                        {
                            if (currentList != null)
                            {
                                yield return new Atom
                                {
                                    Type = AtomTypeEnum.List,
                                    OrderedList = isOrderedList ? currentList : null,
                                    UnorderedList = !isOrderedList ? currentList : null,
                                    Position = position++,
                                    MD5Hash = HashHelper.MD5Hash(currentList),
                                    SHA1Hash = HashHelper.SHA1Hash(currentList),
                                    SHA256Hash = HashHelper.SHA256Hash(currentList),
                                    Length = currentList.Sum(s => s.Length)
                                };

                                currentList = null;
                            }

                            string paragraphText = ExtractTextFromParagraph(paragraph);
                            if (!string.IsNullOrWhiteSpace(paragraphText))
                            {
                                // Detect heading level from paragraph style
                                int? headingLevel = GetHeadingLevel(paragraph);

                                yield return new Atom
                                {
                                    Type = AtomTypeEnum.Text,
                                    Text = paragraphText,
                                    HeaderLevel = headingLevel,
                                    Position = position++,
                                    MD5Hash = HashHelper.MD5Hash(paragraphText),
                                    SHA1Hash = HashHelper.SHA1Hash(paragraphText),
                                    SHA256Hash = HashHelper.SHA256Hash(paragraphText),
                                    Length = paragraphText.Length
                                };
                            }
                        }
                    }
                    else if (element is Table table)
                    {
                        if (currentList != null)
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.List,
                                OrderedList = isOrderedList ? currentList : null,
                                UnorderedList = !isOrderedList ? currentList : null,
                                Position = position++,
                                MD5Hash = HashHelper.MD5Hash(currentList),
                                SHA1Hash = HashHelper.SHA1Hash(currentList),
                                SHA256Hash = HashHelper.SHA256Hash(currentList),
                                Length = currentList.Sum(s => s.Length)
                            };

                            currentList = null;
                        }

                        DataTable dt = ConvertToDataTable(table);

                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Table,
                            Table = SerializableDataTable.FromDataTable(dt),
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(dt),
                            SHA1Hash = HashHelper.SHA1Hash(dt),
                            SHA256Hash = HashHelper.SHA256Hash(dt),
                            Length = DataTableHelper.GetLength(dt)
                        };
                    }
                }

                if (currentList != null)
                {
                    yield return new Atom
                    {
                        Type = AtomTypeEnum.List,
                        OrderedList = isOrderedList ? currentList : null,
                        UnorderedList = !isOrderedList ? currentList : null,
                        Position = position++,
                        MD5Hash = HashHelper.MD5Hash(currentList),
                        SHA1Hash = HashHelper.SHA1Hash(currentList),
                        SHA256Hash = HashHelper.SHA256Hash(currentList),
                        Length = currentList.Sum(s => s.Length)
                    };
                }

                // Handle images
                foreach (var imagePart in doc.MainDocumentPart.ImageParts)
                {
                    using (var stream = imagePart.GetStream())
                    using (var ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        byte[] bytes = ms.ToArray();

                        // Find the image reference and its page
                        var imageId = doc.MainDocumentPart.GetIdOfPart(imagePart);
                        var drawing = doc.MainDocumentPart.Document.Descendants<DocumentFormat.OpenXml.Drawing.Blip>()
                            .FirstOrDefault(b => b.Embed?.Value == imageId);

                        Atom atom = new Atom
                        {
                            Type = AtomTypeEnum.Binary,
                            Binary = bytes,
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
        }

        private string ExtractTextFromParagraph(Paragraph paragraph)
        {
            StringBuilder sb = new StringBuilder();

            foreach (var run in paragraph.Elements<Run>())
            {
                foreach (var text in run.Elements<Text>())
                {
                    sb.Append(text.Text);
                }
            }

            return sb.ToString().Trim();
        }

        private bool IsOrderedList(WordprocessingDocument doc, NumberingProperties numProperties)
        {
            try
            {
                var numberingPartId = numProperties.NumberingId.Val;
                var abstractNumId = doc.MainDocumentPart.NumberingDefinitionsPart
                    .Numbering
                    .Elements<NumberingInstance>()
                    .First(ni => ni.NumberID == numberingPartId)
                    .AbstractNumId.Val;

                var abstractNum = doc.MainDocumentPart.NumberingDefinitionsPart
                    .Numbering
                    .Elements<AbstractNum>()
                    .First(an => an.AbstractNumberId == abstractNumId);

                // Check first level's numFmt
                var level = abstractNum.Elements<Level>().FirstOrDefault();
                if (level?.NumberingFormat?.Val != null)
                {
                    return level.NumberingFormat.Val != NumberFormatValues.Bullet;
                }
            }
            catch
            {
                // If anything goes wrong, assume unordered
                return false;
            }

            return false;
        }

        private DataTable ConvertToDataTable(Table table)
        {
            var dt = new DataTable();

            // Process header row
            var headerRow = table.Elements<TableRow>().FirstOrDefault();
            if (headerRow != null)
            {
                var columnNames = new Dictionary<string, int>();  // Track column name occurrences

                foreach (var cell in headerRow.Elements<TableCell>())
                {
                    string columnName = ExtractTextFromParagraph(cell.Elements<Paragraph>().FirstOrDefault())
                        ?? "Column" + (dt.Columns.Count + 1);

                    // If column name already exists, append a number
                    if (columnNames.ContainsKey(columnName))
                    {
                        columnNames[columnName]++;
                        columnName = $"{columnName}_{columnNames[columnName]}";
                    }
                    else
                    {
                        columnNames[columnName] = 1;
                    }

                    dt.Columns.Add(columnName);
                }
            }

            // Process data rows
            foreach (var row in table.Elements<TableRow>().Skip(1))
            {
                var dataRow = dt.NewRow();
                int columnIndex = 0;

                foreach (var cell in row.Elements<TableCell>())
                {
                    if (columnIndex < dt.Columns.Count)
                    {
                        dataRow[columnIndex] = ExtractTextFromParagraph(cell.Elements<Paragraph>().FirstOrDefault());
                        columnIndex++;
                    }
                }

                dt.Rows.Add(dataRow);
            }

            return dt;
        }

        #endregion

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
