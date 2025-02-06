namespace DocumentAtom.Word
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Runtime.CompilerServices;
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
    public class DocxProcessor : ProcessorBase
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.

        #region Public-Members

        /// <summary>
        /// Docx processor settings.
        /// </summary>
        public new DocxProcessorSettings Settings
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

        private DocxProcessorSettings _Settings = new DocxProcessorSettings();

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

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from text documents.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public DocxProcessor(DocxProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new DocxProcessorSettings();

            Header = "[Docx] ";

            _Settings = settings;
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
                    archive.ExtractToDirectory(_Settings.TempDirectory);
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.PreserveWhitespace = true;
                    xmlDoc.Load(_Settings.TempDirectory + _MetadataFile);

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
                FileHelper.RecursiveDelete(_Settings.TempDirectoryInfo, true);
                Directory.Delete(_Settings.TempDirectory, true);
            }
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

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
                                yield return new Atom
                                {
                                    Type = AtomTypeEnum.Text,
                                    Text = paragraphText,
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

                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Binary,
                            Binary = bytes,
                            MD5Hash = HashHelper.MD5Hash(bytes),
                            SHA1Hash = HashHelper.SHA1Hash(bytes),
                            SHA256Hash = HashHelper.SHA256Hash(bytes),
                            Length = bytes.Length
                        };
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

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
