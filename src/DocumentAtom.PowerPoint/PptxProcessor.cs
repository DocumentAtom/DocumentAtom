namespace DocumentAtom.PowerPoint
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
    using DocumentAtom.Image;
    using DocumentFormat.OpenXml;
    using DocumentFormat.OpenXml.Packaging;
    using DocumentFormat.OpenXml.Presentation;
    using SerializableDataTables;

    /// <summary>
    /// Create atoms from Microsoft PowerPoint .pptx documents.
    /// </summary>
    public class PptxProcessor : ProcessorBase
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

        private PptxProcessorSettings _Settings = new PptxProcessorSettings();

        private const string _MetadataFile = "docProps/core.xml";
        private const string _MetadataXPath = "/cp:coreProperties";

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from PowerPoint presentations.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public PptxProcessor(PptxProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new PptxProcessorSettings();

            Header = "[Pptx] ";
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
                    if (File.Exists(_Settings.TempDirectory + _MetadataFile))
                    {
                        XmlDocument xmlDoc = new XmlDocument();
                        xmlDoc.PreserveWhitespace = true;
                        xmlDoc.Load(_Settings.TempDirectory + _MetadataFile);

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
                FileHelper.RecursiveDelete(_Settings.TempDirectoryInfo, true);
                Directory.Delete(_Settings.TempDirectory, true);
            }
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            using (PresentationDocument pptx = PresentationDocument.Open(filename, false))
            {
                var presentationPart = pptx.PresentationPart;
                var presentation = presentationPart.Presentation;
                var slideIds = presentation.SlideIdList.ChildElements.OfType<SlideId>();

                int slideNumber = 1;
                foreach (var slideId in slideIds)
                {
                    var slidePart = (SlidePart)presentationPart.GetPartById(slideId.RelationshipId);
                    var slide = slidePart.Slide;

                    // Process slide title and subtitle
                    var titleAndSubtitle = ExtractTitleAndSubtitle(slide);
                    if (!string.IsNullOrEmpty(titleAndSubtitle.title))
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Text,
                            Title = titleAndSubtitle.title,
                            PageNumber = slideNumber,
                            MD5Hash = HashHelper.MD5Hash(titleAndSubtitle.title),
                            SHA1Hash = HashHelper.SHA1Hash(titleAndSubtitle.title),
                            SHA256Hash = HashHelper.SHA256Hash(titleAndSubtitle.title),
                            Length = titleAndSubtitle.title.Length
                        };
                    }

                    if (!string.IsNullOrEmpty(titleAndSubtitle.subtitle))
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Text,
                            Subtitle = titleAndSubtitle.subtitle,
                            PageNumber = slideNumber,
                            MD5Hash = HashHelper.MD5Hash(titleAndSubtitle.subtitle),
                            SHA1Hash = HashHelper.SHA1Hash(titleAndSubtitle.subtitle),
                            SHA256Hash = HashHelper.SHA256Hash(titleAndSubtitle.subtitle),
                            Length = titleAndSubtitle.subtitle.Length
                        };
                    }

                    // Process text content
                    foreach (var textContent in ExtractTextContent(slide))
                    {
                        if (!string.IsNullOrWhiteSpace(textContent))
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Text,
                                Text = textContent,
                                PageNumber = slideNumber,
                                MD5Hash = HashHelper.MD5Hash(textContent),
                                SHA1Hash = HashHelper.SHA1Hash(textContent),
                                SHA256Hash = HashHelper.SHA256Hash(textContent),
                                Length = textContent.Length
                            };
                        }
                    }

                    // Process lists
                    var lists = ExtractLists(slide);
                    foreach (var list in lists)
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.List,
                            UnorderedList = list,
                            OrderedList = null,
                            PageNumber = slideNumber,
                            MD5Hash = HashHelper.MD5Hash(list),
                            SHA1Hash = HashHelper.SHA1Hash(list),
                            SHA256Hash = HashHelper.SHA256Hash(list),
                            Length = list.Sum(i => i.Length)
                        };
                    }

                    // Process tables
                    var tables = ExtractTables(slide);
                    foreach (var table in tables)
                    {
                        yield return new Atom
                        {
                            Type = AtomTypeEnum.Table,
                            Table = SerializableDataTable.FromDataTable(table),
                            PageNumber = slideNumber,
                            MD5Hash = HashHelper.MD5Hash(table),
                            SHA1Hash = HashHelper.SHA1Hash(table),
                            SHA256Hash = HashHelper.SHA256Hash(table),
                            Length = DataTableHelper.GetLength(table)
                        };
                    }

                    // Process images
                    foreach (var imagePart in slidePart.ImageParts)
                    {
                        using (var stream = imagePart.GetStream())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            byte[] bytes = ms.ToArray();

                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Binary,
                                Binary = bytes,
                                PageNumber = slideNumber,
                                MD5Hash = HashHelper.MD5Hash(bytes),
                                SHA1Hash = HashHelper.SHA1Hash(bytes),
                                SHA256Hash = HashHelper.SHA256Hash(bytes),
                                Length = bytes.Length
                            };
                        }
                    }

                    slideNumber++;
                }
            }
        }

        private (string title, string subtitle) ExtractTitleAndSubtitle(Slide slide)
        {
            string title = string.Empty;
            string subtitle = string.Empty;

            var shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) return (title, subtitle);

            foreach (var shape in shapes)
            {
                var placeholderType = shape.NonVisualShapeProperties?
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

            return (title?.Trim(), subtitle?.Trim());
        }

        private IEnumerable<string> ExtractTextContent(Slide slide)
        {
            var shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) yield break;

            foreach (var shape in shapes)
            {
                var placeholderType = shape.NonVisualShapeProperties?
                    .ApplicationNonVisualDrawingProperties?
                    .PlaceholderShape?.Type?.Value;

                // Skip if this shape is a title or subtitle
                if (placeholderType == PlaceholderValues.Title ||
                    placeholderType == PlaceholderValues.CenteredTitle ||
                    placeholderType == PlaceholderValues.SubTitle)
                {
                    continue;
                }

                var textBody = shape.TextBody;
                if (textBody == null) continue;

                foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
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
            var textBody = shape.TextBody;
            if (textBody == null) return string.Empty;

            StringBuilder sb = new StringBuilder();
            foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
            {
                foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
                {
                    var text = run.Text?.Text;
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
            var shapes = slide.CommonSlideData?.ShapeTree?.Elements<Shape>();
            if (shapes == null) yield break;

            foreach (var shape in shapes)
            {
                var textBody = shape.TextBody;
                if (textBody == null) continue;

                List<string> currentList = new List<string>();

                foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
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
            foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
            {
                var text = run.Text?.Text;
                if (!string.IsNullOrEmpty(text))
                {
                    sb.Append(text);
                }
            }
            return sb.ToString().Trim();
        }

        private IEnumerable<DataTable> ExtractTables(Slide slide)
        {
            var graphicFrames = slide.CommonSlideData?.ShapeTree?.Elements<GraphicFrame>();
            if (graphicFrames == null) yield break;

            foreach (var frame in graphicFrames)
            {
                var table = frame.Graphic?.GraphicData?.GetFirstChild<DocumentFormat.OpenXml.Drawing.Table>();
                if (table == null) continue;

                var dt = new DataTable();

                // Process header row
                var headerRow = table.Elements<DocumentFormat.OpenXml.Drawing.TableRow>().FirstOrDefault();
                if (headerRow != null)
                {
                    var columnNames = new Dictionary<string, int>();  // Track column name occurrences

                    foreach (var cell in headerRow.Elements<DocumentFormat.OpenXml.Drawing.TableCell>())
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
                foreach (var row in table.Elements<DocumentFormat.OpenXml.Drawing.TableRow>().Skip(1))
                {
                    var dataRow = dt.NewRow();
                    int columnIndex = 0;

                    foreach (var cell in row.Elements<DocumentFormat.OpenXml.Drawing.TableCell>())
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

            foreach (var textBody in cell.Elements<DocumentFormat.OpenXml.Drawing.TextBody>())
            {
                foreach (var paragraph in textBody.Elements<DocumentFormat.OpenXml.Drawing.Paragraph>())
                {
                    foreach (var run in paragraph.Elements<DocumentFormat.OpenXml.Drawing.Run>())
                    {
                        var text = run.Text?.Text;
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