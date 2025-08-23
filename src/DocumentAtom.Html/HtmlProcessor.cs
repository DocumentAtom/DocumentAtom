namespace DocumentAtom.Html
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using HtmlAgilityPack;
    using SerializableDataTables;

    /// <summary>
    /// HTML processor for extracting atoms from HTML files.
    /// </summary>
    public class HtmlProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Settings for the HTML processor.
        /// </summary>
        public new HtmlProcessorSettings Settings
        {
            get
            {
                return _Settings;
            }
        }

        #endregion

        #region Private-Members

        private HtmlProcessorSettings _Settings = null;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new HTML processor.
        /// </summary>
        /// <param name="settings">Settings for the HTML processor.</param>
        public HtmlProcessor(HtmlProcessorSettings settings = null)
        {
            _Settings = settings ?? new HtmlProcessorSettings();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Extract atoms from an HTML file.
        /// </summary>
        /// <param name="filename">The HTML file from which to extract atoms.</param>
        /// <returns>Enumeration of Atom objects.</returns>
        public override IEnumerable<Atom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            if (!File.Exists(filename)) throw new FileNotFoundException("File not found: " + filename);

            List<Atom> atoms = new List<Atom>();
            int position = 0;

            try
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(filename, Encoding.UTF8);

                // Check if DocumentNode exists
                if (doc?.DocumentNode == null)
                {
                    if (_Settings?.DebugLogging == true)
                        Console.WriteLine("Document node is null");
                    return atoms;
                }

                // Process the document body
                HtmlNode body = doc.DocumentNode.SelectSingleNode("//body") ?? doc.DocumentNode;

                ProcessNode(body, atoms, ref position, 1);
            }
            catch (Exception e)
            {
                if (_Settings?.DebugLogging == true)
                    Console.WriteLine($"Error processing HTML file: {e.Message}");
                throw;
            }

            return atoms;
        }

        /// <summary>
        /// Dispose of the HTML processor.
        /// </summary>
        public new void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Process an HTML node and extract atoms.
        /// </summary>
        /// <param name="node">The HTML node to process.</param>
        /// <param name="atoms">List to store extracted atoms.</param>
        /// <param name="position">Current position counter.</param>
        /// <param name="pageNumber">Current page number.</param>
        private void ProcessNode(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            // Null-safe name check
            string nodeName = node.Name?.ToLower() ?? string.Empty;

            switch (nodeName)
            {
                case "p":
                case "div":
                case "span":
                case "section":
                case "article":
                case "main":
                case "aside":
                case "header":
                case "footer":
                case "nav":
                    ProcessTextContainer(node, atoms, ref position, pageNumber);
                    break;

                case "h1":
                case "h2":
                case "h3":
                case "h4":
                case "h5":
                case "h6":
                    ProcessHeading(node, atoms, ref position, pageNumber);
                    break;

                case "ul":
                    ProcessUnorderedList(node, atoms, ref position, pageNumber);
                    break;

                case "ol":
                    ProcessOrderedList(node, atoms, ref position, pageNumber);
                    break;

                case "table":
                    ProcessTable(node, atoms, ref position, pageNumber);
                    break;

                case "img":
                    ProcessImage(node, atoms, ref position, pageNumber);
                    break;

                case "a":
                    ProcessHyperlink(node, atoms, ref position, pageNumber);
                    break;

                case "pre":
                case "code":
                    ProcessCodeBlock(node, atoms, ref position, pageNumber);
                    break;

                case "#text":
                    ProcessTextNode(node, atoms, ref position, pageNumber);
                    break;

                default:
                    // Recursively process child nodes for unhandled elements
                    if (node.ChildNodes != null)
                    {
                        foreach (HtmlNode child in node.ChildNodes)
                        {
                            ProcessNode(child, atoms, ref position, pageNumber);
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Process a text container element.
        /// </summary>
        private void ProcessTextContainer(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string text = GetCleanText(node);
            if (!String.IsNullOrWhiteSpace(text))
            {
                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Text,
                    Position = position++,
                    PageNumber = pageNumber,
                    Text = text,
                    Tag = node.Name?.ToLower() ?? string.Empty,
                    Id = node.GetAttributeValue("id", null),
                    Class = node.GetAttributeValue("class", null)
                };
                SetAtomHashes(atom, text);
                atoms.Add(atom);
            }

            // Process child nodes that aren't pure text
            if (node.ChildNodes != null)
            {
                foreach (HtmlNode child in node.ChildNodes.Where(n => n?.Name != "#text"))
                {
                    ProcessNode(child, atoms, ref position, pageNumber);
                }
            }
        }

        /// <summary>
        /// Process a heading element.
        /// </summary>
        private void ProcessHeading(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string text = GetCleanText(node);
            if (!String.IsNullOrWhiteSpace(text))
            {
                // Safely parse header level
                int headerLevel = 1;
                if (!String.IsNullOrEmpty(node.Name) && node.Name.Length > 1)
                {
                    if (int.TryParse(node.Name.Substring(1), out int level))
                    {
                        headerLevel = level;
                    }
                }

                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Text,
                    Position = position++,
                    PageNumber = pageNumber,
                    Text = text,
                    HeaderLevel = headerLevel,
                    Tag = node.Name?.ToLower() ?? string.Empty,
                    Id = node.GetAttributeValue("id", null),
                    Class = node.GetAttributeValue("class", null)
                };
                SetAtomHashes(atom, text);
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process an unordered list element.
        /// </summary>
        private void ProcessUnorderedList(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            List<string> items = new List<string>();

            // FIX: Check for null before iterating
            HtmlNodeCollection liNodes = node.SelectNodes(".//li");
            if (liNodes != null)
            {
                foreach (HtmlNode li in liNodes)
                {
                    if (li != null)
                    {
                        string itemText = GetCleanText(li);
                        if (!String.IsNullOrWhiteSpace(itemText))
                        {
                            items.Add(itemText);
                        }
                    }
                }
            }

            if (items.Count > 0)
            {
                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.List,
                    Position = position++,
                    PageNumber = pageNumber,
                    UnorderedList = items,
                    Tag = "ul",
                    Id = node.GetAttributeValue("id", null),
                    Class = node.GetAttributeValue("class", null)
                };
                SetAtomHashes(atom, String.Join("\n", items));
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process an ordered list element.
        /// </summary>
        private void ProcessOrderedList(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            List<string> items = new List<string>();

            // FIX: Check for null before iterating
            HtmlNodeCollection liNodes = node.SelectNodes(".//li");
            if (liNodes != null)
            {
                foreach (HtmlNode li in liNodes)
                {
                    if (li != null)
                    {
                        string itemText = GetCleanText(li);
                        if (!String.IsNullOrWhiteSpace(itemText))
                        {
                            items.Add(itemText);
                        }
                    }
                }
            }

            if (items.Count > 0)
            {
                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.List,
                    Position = position++,
                    PageNumber = pageNumber,
                    OrderedList = items,
                    Tag = "ol",
                    Id = node.GetAttributeValue("id", null),
                    Class = node.GetAttributeValue("class", null)
                };
                SetAtomHashes(atom, String.Join("\n", items));
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process a table element.
        /// </summary>
        private void ProcessTable(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            SerializableDataTable table = new SerializableDataTable();

            // Get headers from thead or first row
            HtmlNodeCollection headerNodes = node.SelectNodes(".//thead//th") ??
                                            node.SelectNodes(".//tr[1]//th") ??
                                            node.SelectNodes(".//tr[1]//td");

            List<string> headers = new List<string>();
            Dictionary<string, int> headerCounts = new Dictionary<string, int>();

            if (headerNodes != null)
            {
                foreach (HtmlNode header in headerNodes)
                {
                    if (header != null)
                    {
                        string headerText = GetCleanText(header);
                        if (String.IsNullOrWhiteSpace(headerText))
                        {
                            headerText = $"Column{headers.Count + 1}";
                        }

                        // Make header unique if it's a duplicate
                        string uniqueHeader = headerText;
                        if (headerCounts.ContainsKey(headerText))
                        {
                            headerCounts[headerText]++;
                            uniqueHeader = $"{headerText}_{headerCounts[headerText]}";
                        }
                        else
                        {
                            headerCounts[headerText] = 1;
                        }

                        headers.Add(uniqueHeader);
                    }
                }
            }

            // If no headers found, create default headers based on first row
            HtmlNode firstRow = node.SelectSingleNode(".//tr");
            if (headers.Count == 0 && firstRow != null)
            {
                HtmlNodeCollection firstRowCells = firstRow.SelectNodes(".//td");
                int colCount = firstRowCells?.Count ?? 0;
                for (int i = 0; i < colCount; i++)
                {
                    headers.Add($"Column{i + 1}");
                }
            }

            // Add columns to table
            foreach (string header in headers)
            {
                table.Columns.Add(new SerializableColumn
                {
                    Name = header,
                    Type = ColumnValueType.String
                });
            }

            // Add data rows
            HtmlNodeCollection rows = node.SelectNodes(".//tbody//tr") ?? node.SelectNodes(".//tr");
            if (rows != null)
            {
                bool skipFirst = node.SelectNodes(".//thead") == null && node.SelectNodes(".//tr[1]//th") != null;

                foreach (HtmlNode row in rows.Skip(skipFirst ? 1 : 0))
                {
                    if (row != null)
                    {
                        HtmlNodeCollection cells = row.SelectNodes(".//td");
                        if (cells != null)
                        {
                            Dictionary<string, object> rowData = new Dictionary<string, object>();
                            for (int i = 0; i < headers.Count; i++)
                            {
                                if (i < cells.Count && cells[i] != null)
                                {
                                    rowData[headers[i]] = GetCleanText(cells[i]);
                                }
                                else
                                {
                                    rowData[headers[i]] = "";
                                }
                            }
                            table.Rows.Add(rowData);
                        }
                    }
                }
            }

            if (table.Rows.Count > 0)
            {
                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Table,
                    Position = position++,
                    PageNumber = pageNumber,
                    Table = table,
                    Rows = table.Rows.Count,
                    Columns = table.Columns.Count,
                    Tag = "table",
                    Id = node.GetAttributeValue("id", null),
                    Class = node.GetAttributeValue("class", null)
                };

                // Create a string representation for hashing
                StringBuilder tableContent = new StringBuilder();
                if (table.Columns != null)
                {
                    foreach (var col in table.Columns)
                    {
                        if (col != null && col.Name != null)
                        {
                            tableContent.Append(col.Name + "\t");
                        }
                    }
                    tableContent.AppendLine();
                }

                if (table.Rows != null)
                {
                    foreach (var row in table.Rows)
                    {
                        if (row != null && table.Columns != null)
                        {
                            foreach (var col in table.Columns)
                            {
                                if (col != null && col.Name != null && row.ContainsKey(col.Name))
                                {
                                    tableContent.Append(row[col.Name]?.ToString() ?? "");
                                }
                                tableContent.Append("\t");
                            }
                            tableContent.AppendLine();
                        }
                    }
                }

                SetAtomHashes(atom, tableContent.ToString());
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process an image element.
        /// </summary>
        private void ProcessImage(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string src = node.GetAttributeValue("src", null);
            string alt = node.GetAttributeValue("alt", null);
            string title = node.GetAttributeValue("title", null);

            if (!String.IsNullOrWhiteSpace(src))
            {
                ImageAtom atom = new ImageAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Image,
                    Position = position++,
                    PageNumber = pageNumber,
                    Src = src,
                    Alt = alt,
                    Title = title,
                    Width = node.GetAttributeValue("width", null),
                    Height = node.GetAttributeValue("height", null)
                };

                // Set content based on alt text or src
                string content = !String.IsNullOrWhiteSpace(alt) ? alt : src;
                SetAtomHashes(atom, content);
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process a hyperlink element.
        /// </summary>
        private void ProcessHyperlink(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string href = node.GetAttributeValue("href", null);
            string text = GetCleanText(node);
            string title = node.GetAttributeValue("title", null);
            string target = node.GetAttributeValue("target", null);

            if (!String.IsNullOrWhiteSpace(href) || !String.IsNullOrWhiteSpace(text))
            {
                HyperlinkAtom atom = new HyperlinkAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Hyperlink,
                    Position = position++,
                    PageNumber = pageNumber,
                    Href = href,
                    Text = text,
                    Title = title,
                    Target = target
                };

                string content = !String.IsNullOrWhiteSpace(text) ? text : href ?? "";
                SetAtomHashes(atom, content);
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process a code block element.
        /// </summary>
        private void ProcessCodeBlock(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string code = node.InnerText;
            if (!String.IsNullOrWhiteSpace(code))
            {
                string classAttribute = node.GetAttributeValue("class", "");
                string language = null;

                if (!String.IsNullOrEmpty(classAttribute))
                {
                    // Safely split and process class attribute
                    string[] classes = classAttribute.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    language = classes
                        .FirstOrDefault(c => c != null && c.StartsWith("language-"))
                        ?.Replace("language-", "");
                }

                CodeAtom atom = new CodeAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Code,
                    Position = position++,
                    PageNumber = pageNumber,
                    Code = code,
                    Language = language
                };
                SetAtomHashes(atom, code);
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Process a text node.
        /// </summary>
        private void ProcessTextNode(HtmlNode node, List<Atom> atoms, ref int position, int pageNumber)
        {
            if (node == null) return;

            string text = GetCleanText(node);
            if (!String.IsNullOrWhiteSpace(text))
            {
                HtmlAtom atom = new HtmlAtom
                {
                    GUID = Guid.NewGuid(),
                    Type = AtomTypeEnum.Text,
                    Position = position++,
                    PageNumber = pageNumber,
                    Text = text
                };
                SetAtomHashes(atom, text);
                atoms.Add(atom);
            }
        }

        /// <summary>
        /// Get clean text from an HTML node.
        /// </summary>
        private string GetCleanText(HtmlNode node)
        {
            if (node == null) return String.Empty;

            // Safely get inner text
            string innerText = node.InnerText ?? String.Empty;
            string text = HtmlEntity.DeEntitize(innerText);
            text = text?.Trim() ?? String.Empty;

            // Remove excessive whitespace
            while (text.Contains("  "))
            {
                text = text.Replace("  ", " ");
            }

            return text;
        }

        /// <summary>
        /// Set hash values for an atom.
        /// </summary>
        private void SetAtomHashes(Atom atom, string content)
        {
            if (atom == null || String.IsNullOrEmpty(content)) return;

            byte[] bytes = Encoding.UTF8.GetBytes(content);
            atom.Length = bytes.Length;

            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                if (md5 != null)
                {
                    atom.MD5Hash = md5.ComputeHash(bytes);
                }
            }

            using (var sha1 = System.Security.Cryptography.SHA1.Create())
            {
                if (sha1 != null)
                {
                    atom.SHA1Hash = sha1.ComputeHash(bytes);
                }
            }

            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                if (sha256 != null)
                {
                    atom.SHA256Hash = sha256.ComputeHash(bytes);
                }
            }
        }

        /// <summary>
        /// Protected dispose method.
        /// </summary>
        /// <param name="disposing">Indicates if disposing.</param>
        protected new void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources if any
                }
                _Disposed = true;
            }
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}