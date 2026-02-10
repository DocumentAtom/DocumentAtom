namespace DocumentAtom.Documents.PowerPoint
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Documents.Image;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;
    using SerializableDataTables;

    /// <summary>
    /// Create atoms from Microsoft PowerPoint .pptx documents.
    /// </summary>
    public class PptxProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8619 // Nullability of reference types in value doesn't match target type.

        #region Public-Members

        /// <summary>
        /// Pptx processor settings.
        /// </summary>
        public new PptxProcessorSettings Settings
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

        private PptxProcessorSettings _ProcessorSettings = new PptxProcessorSettings();
        private ImageProcessorSettings _ImageProcessorSettings = null;
        private ImageProcessor _ImageProcessor = null;

        private const string _MetadataFile = "docProps/core.xml";
        private const string _MetadataXPath = "/cp:coreProperties";

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
        /// Create atoms from PowerPoint presentations.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public PptxProcessor(PptxProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new PptxProcessorSettings();

            Header = "[Pptx] ";

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
        /// Groups atoms by PageNumber, making the first atom on each page the parent.
        /// </summary>
        /// <param name="flatAtoms">Flat list of atoms.</param>
        /// <returns>Root-level atoms with hierarchical structure.</returns>
        private IEnumerable<Atom> BuildHierarchy(List<Atom> flatAtoms)
        {
            if (flatAtoms == null || flatAtoms.Count == 0)
            {
                return Enumerable.Empty<Atom>();
            }

            // Group atoms by PageNumber
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

            using (PresentationDocument pptx = PresentationDocument.Open(filename, false))
            {
                PresentationPart presentationPart = pptx.PresentationPart;
                Presentation presentation = presentationPart.Presentation;
                IEnumerable<SlideId> slideIds = presentation.SlideIdList.ChildElements.OfType<SlideId>();

                int slideNumber = 1;
                foreach (SlideId slideId in slideIds)
                {
                    SlidePart slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);
                    Slide slide = slidePart.Slide;

                    // Process slide title and subtitle
                    SlideTitleInfo titleAndSubtitle = ExtractTitleAndSubtitle(slide);
                    if (!string.IsNullOrEmpty(titleAndSubtitle.Title))
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Text,
                            Title = titleAndSubtitle.Title,
                            PageNumber = slideNumber,
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(titleAndSubtitle.Title),
                            SHA1Hash = HashHelper.SHA1Hash(titleAndSubtitle.Title),
                            SHA256Hash = HashHelper.SHA256Hash(titleAndSubtitle.Title),
                            Length = titleAndSubtitle.Title.Length
                        };
                    }

                    if (!string.IsNullOrEmpty(titleAndSubtitle.Subtitle))
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Text,
                            Subtitle = titleAndSubtitle.Subtitle,
                            PageNumber = slideNumber,
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(titleAndSubtitle.Subtitle),
                            SHA1Hash = HashHelper.SHA1Hash(titleAndSubtitle.Subtitle),
                            SHA256Hash = HashHelper.SHA256Hash(titleAndSubtitle.Subtitle),
                            Length = titleAndSubtitle.Subtitle.Length
                        };
                    }

                    // Process text content
                    foreach (string textContent in ExtractTextContent(slide))
                    {
                        if (!string.IsNullOrWhiteSpace(textContent))
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Text,
                                Text = textContent,
                                PageNumber = slideNumber,
                                Position = position++,
                                MD5Hash = HashHelper.MD5Hash(textContent),
                                SHA1Hash = HashHelper.SHA1Hash(textContent),
                                SHA256Hash = HashHelper.SHA256Hash(textContent),
                                Length = textContent.Length
                            };
                        }
                    }

                    // Process lists
                    IEnumerable<List<string>> lists = ExtractLists(slide);
                    foreach (List<string> list in lists)
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.List,
                            UnorderedList = list,
                            OrderedList = null,
                            PageNumber = slideNumber,
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(list),
                            SHA1Hash = HashHelper.SHA1Hash(list),
                            SHA256Hash = HashHelper.SHA256Hash(list),
                            Length = list.Sum(i => i.Length)
                        };
                    }

                    // Process tables
                    IEnumerable<DataTable> tables = ExtractTables(slide);
                    foreach (DataTable table in tables)
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Table,
                            Table = SerializableDataTable.FromDataTable(table),
                            PageNumber = slideNumber,
                            Position = position++,
                            MD5Hash = HashHelper.MD5Hash(table),
                            SHA1Hash = HashHelper.SHA1Hash(table),
                            SHA256Hash = HashHelper.SHA256Hash(table),
                            Length = DataTableHelper.GetLength(table)
                        };
                    }

                    // Process images
                    foreach (ImagePart imagePart in slidePart.ImageParts)
                    {
                        using (Stream stream = imagePart.GetStream())
                        using (MemoryStream ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            byte[] bytes = ms.ToArray();

                            Atom atom = new Atom
                            {
                                Type = AtomTypeEnum.Binary,
                                Binary = bytes,
                                PageNumber = slideNumber,
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

                    slideNumber++;
                }
            }
        }

        private SlideTitleInfo ExtractTitleAndSubtitle(Slide slide)
        {
            string title = string.Empty;
            string subtitle = string.Empty;

            IEnumerable<Shape> shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) return new SlideTitleInfo(title, subtitle);

            foreach (Shape shape in shapes)
            {
                PlaceholderValues? placeholderType = shape.NonVisualShapeProperties?
                    .ApplicationNonVisualDrawingProperties?
                    .PlaceholderShape?.Type?.Value;

                // Handle both regular title and centered title (from title slides)
                if (placeholderType == PlaceholderValues.Title ||
                    placeholderType == PlaceholderValues.CenteredTitle)
                {
                    title = ExtractTextFromShape(shape);
                }
                else if (placeholderType == PlaceholderValues.SubTitle)
                {
                    subtitle = ExtractTextFromShape(shape);
                }
            }

            return new SlideTitleInfo(title?.Trim(), subtitle?.Trim());
        }

        private IEnumerable<string> ExtractTextContent(Slide slide)
        {
            IEnumerable<Shape> shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) yield break;

            foreach (Shape shape in shapes)
            {
                PlaceholderValues? placeholderType = shape.NonVisualShapeProperties?
                    .ApplicationNonVisualDrawingProperties?
                    .PlaceholderShape?.Type?.Value;

                // Skip if this shape is a title or subtitle
                if (placeholderType == PlaceholderValues.Title ||
                    placeholderType == PlaceholderValues.CenteredTitle ||
                    placeholderType == PlaceholderValues.SubTitle)
                {
                    continue;
                }

                TextBody textBody = shape.TextBody;
                if (textBody == null) continue;

                foreach (DocumentFormat.OpenXml.Drawing.Paragraph paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    // Skip list items
                    bool isBulletPoint = paragraph.ParagraphProperties != null &&
                                        paragraph.ParagraphProperties.Level != null;
                    if (isBulletPoint)
                    {
                        continue;
                    }

                    string text = ExtractTextFromParagraph(paragraph);
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        yield return text;
                    }
                }
            }
        }

        private string ExtractTextFromShape(Shape shape)
        {
            TextBody textBody = shape.TextBody;
            if (textBody == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (DocumentFormat.OpenXml.Drawing.Paragraph paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
            {
                foreach (DocumentFormat.OpenXml.Drawing.Run run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
                {
                    string text = run.Text?.Text;
                    if (!string.IsNullOrEmpty(text))
                    {
                        sb.AppendLine(text);
                    }
                }
            }

            return sb.ToString().Trim();
        }

        private IEnumerable<List<string>> ExtractLists(Slide slide)
        {
            IEnumerable<Shape> shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) yield break;

            foreach (Shape shape in shapes)
            {
                TextBody textBody = shape.TextBody;
                if (textBody == null) continue;

                List<string> currentList = new List<string>();

                foreach (DocumentFormat.OpenXml.Drawing.Paragraph paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    // Check for bullet level in paragraph properties
                    bool isBulletPoint = paragraph.ParagraphProperties != null &&
                                        paragraph.ParagraphProperties.Level != null;

                    if (isBulletPoint)
                    {
                        string text = ExtractTextFromParagraph(paragraph);
                        if (!string.IsNullOrWhiteSpace(text))
                        {
                            currentList.Add(text);
                        }
                    }
                    else if (currentList.Any())
                    {
                        // End of list detected
                        yield return currentList;
                        currentList = new List<string>();
                    }
                }

                // Return any remaining list
                if (currentList.Any())
                {
                    yield return currentList;
                }
            }
        }

        private string ExtractTextFromParagraph(DocumentFormat.OpenXml.Drawing.Paragraph paragraph)
        {
            StringBuilder sb = new StringBuilder();
            foreach (DocumentFormat.OpenXml.Drawing.Run run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
            {
                string text = run.Text?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                }
            }
            return sb.ToString().Trim();
        }

        private IEnumerable<DataTable> ExtractTables(Slide slide)
        {
            IEnumerable<GraphicFrame> graphicFrames = slide.CommonSlideData?.ShapeTree?.Elements<GraphicFrame>();
            if (graphicFrames == null) yield break;

            foreach (GraphicFrame frame in graphicFrames)
            {
                DocumentFormat.OpenXml.Drawing.Table table = frame.Graphic?.GraphicData?.GetFirstChild<DocumentFormat.OpenXml.Drawing.Table>();
                if (table == null) continue;

                DataTable dt = new DataTable();

                // Process header row
                DocumentFormat.OpenXml.Drawing.TableRow headerRow = table.Elements<DocumentFormat.OpenXml.Drawing.TableRow>().FirstOrDefault();
                if (headerRow != null)
                {
                    Dictionary<string, int> columnNames = new Dictionary<string, int>();  // Track column name occurrences

                    foreach (DocumentFormat.OpenXml.Drawing.TableCell cell in headerRow.Elements<DocumentFormat.OpenXml.Drawing.TableCell>())
                    {
                        string columnName = ExtractTextFromTableCell(cell) ?? $"Column{dt.Columns.Count + 1}";

                        // Handle duplicate column names
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
                foreach (DocumentFormat.OpenXml.Drawing.TableRow row in table.Elements<DocumentFormat.OpenXml.Drawing.TableRow>().Skip(1))
                {
                    DataRow dataRow = dt.NewRow();
                    int columnIndex = 0;

                    foreach (DocumentFormat.OpenXml.Drawing.TableCell cell in row.Elements<DocumentFormat.OpenXml.Drawing.TableCell>())
                    {
                        if (columnIndex < dt.Columns.Count)
                        {
                            dataRow[columnIndex] = ExtractTextFromTableCell(cell);
                            columnIndex++;
                        }
                    }

                    dt.Rows.Add(dataRow);
                }

                yield return dt;
            }
        }

        private string ExtractTextFromTableCell(DocumentFormat.OpenXml.Drawing.TableCell cell)
        {
            StringBuilder sb = new StringBuilder();

            foreach (DocumentFormat.OpenXml.Drawing.TextBody textBody in cell.Elements<DocumentFormat.OpenXml.Drawing.TextBody>())
            {
                foreach (DocumentFormat.OpenXml.Drawing.Paragraph paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    foreach (DocumentFormat.OpenXml.Drawing.Run run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
                    {
                        string text = run.Text?.Text;
                        if (!string.IsNullOrEmpty(text))
                        {
                            sb.Append(text);
                        }
                    }
                }
            }

            return sb.ToString().Trim();
        }

        #endregion

#pragma warning restore CS8619 // Nullability of reference types in value doesn't match target type.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}