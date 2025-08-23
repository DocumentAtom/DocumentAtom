namespace DocumentAtom.Core.Atoms
{
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Core.Image;
    using SerializableDataTables;
    using System.Data;
    using System.Text;

    /// <summary>
    /// An atom is a small, self-contained unit of content from a document.
    /// </summary>
    public class Atom : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// GUID.
        /// </summary>
        public Guid GUID { get; set; } = Guid.NewGuid();

        /// <summary>
        /// The type of the content for this atom.
        /// </summary>
        public AtomTypeEnum Type { get; set; } = AtomTypeEnum.Text;

        /// <summary>
        /// Sheet name.
        /// </summary>
        public string SheetName { get; set; } = null;

        /// <summary>
        /// Cell identifier.
        /// </summary>
        public string CellIdentifier { get; set; } = null;

        /// <summary>
        /// Page number.
        /// </summary>
        public int? PageNumber
        {
            get
            {
                return _PageNumber;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(PageNumber));
                _PageNumber = value;
            }
        }

        /// <summary>
        /// The ordinal position of the atom.
        /// </summary>
        public int? Position
        {
            get
            {
                return _Position;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(Position));
                _Position = value;
            }
        }

        /// <summary>
        /// The length of the atom content.
        /// </summary>
        public int Length
        {
            get
            {
                return _Length;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(Length));
                _Length = value;
            }
        }

        /// <summary>
        /// The number of rows.
        /// </summary>
        public int? Rows
        {
            get
            {
                return _Rows;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(Rows));
                _Rows = value;
            }
        }

        /// <summary>
        /// The number of columns.
        /// </summary>
        public int? Columns
        {
            get
            {
                return _Columns;
            }
            set
            {
                if (value != null && value.Value < 0) throw new ArgumentOutOfRangeException(nameof(Columns));
                _Columns = value;
            }
        }

        /// <summary>
        /// Title.
        /// </summary>
        public string Title { get; set; } = null;

        /// <summary>
        /// Subtitle.
        /// </summary>
        public string Subtitle { get; set; } = null;

        /// <summary>
        /// MD5 hash of the content.
        /// </summary>
        public byte[] MD5Hash { get; set; } = null;

        /// <summary>
        /// SHA1 hash of the content.
        /// </summary>
        public byte[] SHA1Hash { get; set; } = null;

        /// <summary>
        /// SHA256 hash of the content.
        /// </summary>
        public byte[] SHA256Hash { get; set; } = null;

        /// <summary>
        /// Header level, that is, the number of hash marks found at the beginning of the text.
        /// Minimum value is 1.
        /// </summary>
        public int? HeaderLevel
        {
            get
            {
                return _HeaderLevel;
            }
            set
            {
                if (value != null && value.Value < 1) throw new ArgumentOutOfRangeException(nameof(HeaderLevel));
                _HeaderLevel = value;
            }
        }

        /// <summary>
        /// Markdown formatting type for this atom.
        /// </summary>
        public MarkdownFormattingEnum? Formatting { get; set; } = null;

        /// <summary>
        /// Bounding box.
        /// </summary>
        public BoundingBox BoundingBox { get; set; } = null;

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Unordered list elements.
        /// </summary>
        public List<string> UnorderedList { get; set; } = null;

        /// <summary>
        /// Ordered list elements.
        /// </summary>
        public List<string> OrderedList { get; set; } = null;

        /// <summary>
        /// Data table.
        /// </summary>
        public SerializableDataTable Table { get; set; } = null;

        /// <summary>
        /// Binary data.
        /// </summary>
        public byte[] Binary { get; set; } = null;

        /// <summary>
        /// A quark is a subset of the content from an atom, used when intentionally breaking content into smaller chunks.
        /// </summary>
        public List<Atom> Quarks { get; set; } = null;

        #endregion

        #region Private-Members

        private int? _Rows = null;
        private int? _Columns = null;
        private int? _HeaderLevel = null;
        private int? _PageNumber = null;
        private int? _Position = null;
        private int _Length = 0;

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// An atom is a small, self-contained unit of content from a document.
        /// </summary>
        public Atom()
        {

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
        /// Produce a human-readable string of this object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Atom" + Environment.NewLine);
            sb.Append("| GUID          : " + GUID.ToString() + Environment.NewLine);
            sb.Append("| Type          : " + Type.ToString() + Environment.NewLine);

            if (PageNumber != null)
                sb.Append("| Page Number   : " + PageNumber.ToString() + Environment.NewLine);

            if (Position != null)
                sb.Append("| Position      : " + Position.ToString() + Environment.NewLine);

            sb.Append("| Length        : " + Length.ToString() + Environment.NewLine);
            sb.Append("| MD5 hash      : " + Convert.ToBase64String(MD5Hash) + Environment.NewLine);
            sb.Append("| SHA1 hash     : " + Convert.ToBase64String(SHA1Hash) + Environment.NewLine);
            sb.Append("| SHA256 hash   : " + Convert.ToBase64String(SHA256Hash) + Environment.NewLine);

            if (Quarks != null && Quarks.Count > 0)
            {
                sb.Append("| Quarks        : " + Quarks.Count + Environment.NewLine);
                foreach (Atom quark in Quarks)
                {
                    sb.Append(quark.ToString());
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Create a table atom from a table structure.
        /// </summary>
        /// <param name="table">Table structure.</param>
        /// <returns>Table atom.</returns>
        public static Atom FromTableStructure(TableStructure table)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));

            // Validate table structure
            if (!ValidateTableStructure(table))
            {
                throw new ArgumentException("Invalid table structure provided", nameof(table));
            }

            var dt = new DataTable();

            // Add columns with guaranteed unique names
            for (int i = 0; i < table.Columns; i++)
            {
                string columnName = GetUniqueColumnName(dt, $"Column{i}", i);
                dt.Columns.Add(columnName, typeof(string));
            }

            // Add rows with bounds checking
            if (table.Cells != null)
            {
                for (int i = 0; i < table.Rows && i < table.Cells.Count; i++)
                {
                    var row = dt.NewRow();
                    if (table.Cells[i] != null)
                    {
                        for (int j = 0; j < table.Columns && j < table.Cells[i].Count; j++)
                        {
                            // Ensure we don't exceed the actual column count in DataTable
                            if (j < dt.Columns.Count)
                            {
                                row[j] = table.Cells[i][j] ?? string.Empty;
                            }
                        }
                    }
                    dt.Rows.Add(row);
                }
            }

            return new Atom
            {
                Type = AtomTypeEnum.Table,
                Rows = table.Rows,
                Columns = table.Columns,
                Table = SerializableDataTable.FromDataTable(dt)
            };
        }

        /// <summary>
        /// Create an atom from text content.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <param name="position">Position.</param>
        /// <param name="settings">Settings.</param>
        /// <returns></returns>
        public static Atom FromTextContent(string text, int position, ChunkingSettings settings)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (settings == null) settings = new ChunkingSettings();

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            Atom atom = new Atom
            {
                Type = AtomTypeEnum.Text,
                Position = position,
                Length = text.Length,
                Text = text,
                MD5Hash = HashHelper.MD5Hash(bytes),
                SHA1Hash = HashHelper.SHA1Hash(bytes),
                SHA256Hash = HashHelper.SHA256Hash(bytes)
            };

            if (settings.Enable)
            {
                if (text.Length >= settings.MaximumLength)
                {
                    int quarkPosition = 0;

                    IEnumerable<string> subStrings = StringHelper.GetSubstringsFromString(text, settings.MaximumLength, settings.ShiftSize, settings.MaximumWords);

                    atom.Quarks = new List<Atom>();

                    foreach (string substring in subStrings)
                    {
                        if (!string.IsNullOrEmpty(substring))
                        {
                            byte[] substringBytes = Encoding.UTF8.GetBytes(substring);

                            Atom quark = new Atom
                            {
                                Type = AtomTypeEnum.Text,
                                Position = quarkPosition,
                                Length = substring.Length,
                                Text = substring,
                                MD5Hash = HashHelper.MD5Hash(substringBytes),
                                SHA1Hash = HashHelper.SHA1Hash(substringBytes),
                                SHA256Hash = HashHelper.SHA256Hash(substringBytes),
                                Quarks = null
                            };

                            atom.Quarks.Add(quark);
                            quarkPosition++;
                        }
                    }
                }
            }

            return atom;
        }

        /// <summary>
        /// Produce an atom with quarks, if chunking is enabled.
        /// </summary>
        /// <param name="text">Text content.</param>
        /// <param name="position">Atom position.</param>
        /// <param name="settings">Chunking settings.</param>
        /// <returns>Markdown atom.</returns>
        public static Atom FromMarkdownContent(string text, int position, ChunkingSettings settings)
        {
            if (string.IsNullOrEmpty(text)) throw new ArgumentNullException(nameof(text));
            if (position < 0) throw new ArgumentOutOfRangeException(nameof(position));
            if (settings == null) settings = new ChunkingSettings();

            byte[] bytes = Encoding.UTF8.GetBytes(text);

            #region Preprocessing

            int? headerLevel = null;
            MarkdownFormattingEnum formatting = MarkdownFormattingEnum.Text;

            if (text.Trim().StartsWith("#"))
            {
                formatting = MarkdownFormattingEnum.Header;
                headerLevel = 0;
                string header = text;

                while (header.StartsWith("#"))
                {
                    headerLevel++;
                    header = header.Substring(1);
                }
            }
            else if (text.Trim().StartsWith("|"))
            {
                formatting = MarkdownFormattingEnum.Table;
            }
            else if (IsMarkdownUnorderedListItem(text))
            {
                formatting = MarkdownFormattingEnum.UnorderedList;
            }
            else if (IsMarkdownOrderedListItem(text))
            {
                formatting = MarkdownFormattingEnum.OrderedList;
            }
            else if (IsMarkdownTableItem(text))
            {
                formatting = MarkdownFormattingEnum.Table;
            }

            #endregion

            Atom atom = new Atom
            {
                Type = AtomTypeEnum.Text,
                Position = position,
                Length = text.Length,
                Formatting = formatting,
                HeaderLevel = headerLevel,
                Text = text,
                MD5Hash = HashHelper.MD5Hash(bytes),
                SHA1Hash = HashHelper.SHA1Hash(bytes),
                SHA256Hash = HashHelper.SHA256Hash(bytes)
            };

            if (settings.Enable && atom.Formatting == MarkdownFormattingEnum.Text)
            {
                #region Chunk-Text

                if (text.Length >= settings.MaximumLength)
                {
                    int quarkPosition = 0;

                    IEnumerable<string> subStrings = StringHelper.GetSubstringsFromString(text, settings.MaximumLength, settings.ShiftSize, settings.MaximumWords);

                    atom.Quarks = new List<Atom>();

                    foreach (string substring in subStrings)
                    {
                        if (!string.IsNullOrEmpty(substring))
                        {
                            byte[] substringBytes = Encoding.UTF8.GetBytes(substring);

                            Atom quark = new Atom
                            {
                                Type = AtomTypeEnum.Text,
                                Position = quarkPosition,
                                Length = substring.Length,
                                Text = substring,
                                MD5Hash = HashHelper.MD5Hash(substringBytes),
                                SHA1Hash = HashHelper.SHA1Hash(substringBytes),
                                SHA256Hash = HashHelper.SHA256Hash(substringBytes),
                                Quarks = null
                            };

                            atom.Quarks.Add(quark);
                            quarkPosition++;
                        }
                    }
                }

                #endregion
            }

            if (atom.Formatting == MarkdownFormattingEnum.Image)
            {
                #region Image

                #endregion
            }
            else if (atom.Formatting == MarkdownFormattingEnum.Table)
            {
                #region Table

                atom.Table = MarkdownTextToDataTable(atom.Text);

                #endregion
            }
            else if (atom.Formatting == MarkdownFormattingEnum.OrderedList)
            {
                #region Ordered-List

                atom.OrderedList = MarkdownTextToList(atom.Text);

                #endregion
            }
            else if (atom.Formatting == MarkdownFormattingEnum.UnorderedList)
            {
                #region Unordered-List

                atom.UnorderedList = MarkdownTextToList(atom.Text);

                #endregion
            }

            return atom;
        }

        /// <summary>
        /// Determine if a text item is part of a markdown unordered list.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if the text represents an unordered list.</returns>
        public static bool IsMarkdownUnorderedListItem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // Trim leading whitespace while preserving the rest of the string
            string test = text.TrimStart();

            // Check if the string starts with a valid unordered list marker
            if (!test.StartsWith("-") && !test.StartsWith("*") && !test.StartsWith("+")) return false;

            // Ensure there's at least one whitespace character after the marker
            if (test.Length < 2 || !char.IsWhiteSpace(test[1])) return false;

            // Ensure there's actual content after the whitespace
            return test.Length > 2 && test.Skip(2).Any(c => !char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Determine if a text item is part of a markdown ordered list.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if the text represents an ordered list.</returns>
        public static bool IsMarkdownOrderedListItem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // Trim leading whitespace while preserving the rest of the string
            string test = text.TrimStart();

            // Find the position of the first period
            int periodIndex = test.IndexOf('.');
            if (periodIndex <= 0) return false;

            // Check if everything before the period is a number
            if (!int.TryParse(test.Substring(0, periodIndex), out _)) return false;

            // Ensure there's at least one whitespace character after the period
            if (periodIndex + 1 >= test.Length || !char.IsWhiteSpace(test[periodIndex + 1])) return false;

            // Ensure there's actual content after the whitespace
            return test.Skip(periodIndex + 2).Any(c => !char.IsWhiteSpace(c));
        }

        /// <summary>
        /// Determine if a text item is part of a markdown table.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if item is part of a table.</returns>
        public static bool IsMarkdownTableItem(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;
            string test = text.TrimStart();
            if (test.StartsWith("|")) return true;
            return false;
        }

        /// <summary>
        /// Convert a markdown table to a DataTable.
        /// </summary>
        /// <param name="text">Markdown table.</param>
        /// <returns>DataTable.</returns>
        public static SerializableDataTable MarkdownTextToDataTable(string text)
        {
            if (string.IsNullOrEmpty(text)) return null;

            var dataTable = new DataTable();

            // Split into lines and remove empty ones
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count < 1) return null;

            // Process header row
            var headers = ParseRow(lines[0]);
            if (headers.Count == 0) return null;

            // Create columns with unique names
            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < headers.Count; i++)
            {
                string columnName = SanitizeColumnName(headers[i]);

                // Handle empty headers
                if (string.IsNullOrWhiteSpace(columnName))
                {
                    columnName = $"Column{i + 1}";
                }

                // Ensure uniqueness
                string originalName = columnName;
                int suffix = 1;
                while (columnNames.Contains(columnName))
                {
                    columnName = $"{originalName}_{suffix}";
                    suffix++;
                }

                columnNames.Add(columnName);
                dataTable.Columns.Add(new DataColumn(columnName));
            }

            // Process data rows (skip separator row if it exists)
            int startRow = 1;
            if (lines.Count > 1 && IsSeparatorRow(lines[1]))
            {
                startRow = 2;
            }

            for (int i = startRow; i < lines.Count; i++)
            {
                if (!IsSeparatorRow(lines[i]))
                {
                    var row = ParseRow(lines[i]);

                    // Create data row with proper column count handling
                    var dataRow = dataTable.NewRow();
                    for (int j = 0; j < Math.Min(row.Count, dataTable.Columns.Count); j++)
                    {
                        dataRow[j] = row[j] ?? string.Empty;
                    }

                    // Handle case where row has fewer values than columns
                    for (int j = row.Count; j < dataTable.Columns.Count; j++)
                    {
                        dataRow[j] = string.Empty;
                    }

                    dataTable.Rows.Add(dataRow);
                }
            }

            return SerializableDataTable.FromDataTable(dataTable);
        }

        /// <summary>
        /// Convert a markdown list to a list of strings.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>List of strings.</returns>
        public static List<string> MarkdownTextToList(string text)
        {
            var results = new List<string>();

            if (string.IsNullOrWhiteSpace(text)) return results;

            // Split into lines and process each non-empty line
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line));

            foreach (var line in lines)
            {
                string content = null;

                // Check for unordered list markers (-, *, or +)
                if (line.StartsWith("-") || line.StartsWith("*") || line.StartsWith("+"))
                {
                    content = line.Substring(1).Trim();
                }
                // Check for ordered list markers (number followed by period or right parenthesis)
                else if (line.Length > 2)
                {
                    int periodIndex = line.IndexOf('.');
                    int parenthesisIndex = line.IndexOf(')');

                    // Get the earliest marker found (if any)
                    int markerIndex = -1;
                    if (periodIndex > 0 && (parenthesisIndex == -1 || periodIndex < parenthesisIndex))
                    {
                        markerIndex = periodIndex;
                    }
                    else if (parenthesisIndex > 0)
                    {
                        markerIndex = parenthesisIndex;
                    }

                    if (markerIndex > 0 &&
                        markerIndex < line.Length - 1 &&
                        int.TryParse(line.Substring(0, markerIndex), out _))
                    {
                        content = line.Substring(markerIndex + 1).Trim();
                    }
                }

                if (!string.IsNullOrWhiteSpace(content))
                {
                    results.Add(content);
                }
            }

            return results;
        }

        #endregion

        #region Private-Methods

        /// <summary>
        /// Get a unique column name for a DataTable.
        /// </summary>
        /// <param name="dt">DataTable to check against.</param>
        /// <param name="proposedName">Proposed column name.</param>
        /// <param name="columnIndex">Column index for default naming.</param>
        /// <returns>Unique column name.</returns>
        private static string GetUniqueColumnName(DataTable dt, string proposedName, int columnIndex)
        {
            // Handle null/empty names
            if (string.IsNullOrWhiteSpace(proposedName))
            {
                proposedName = $"Column{columnIndex}";
            }

            // Ensure uniqueness
            string baseName = proposedName;
            int suffix = 1;
            while (dt.Columns.Contains(proposedName))
            {
                proposedName = $"{baseName}_{suffix}";
                suffix++;
            }

            return proposedName;
        }

        /// <summary>
        /// Validate the structure of a table.
        /// </summary>
        /// <param name="table">Table structure to validate.</param>
        /// <returns>True if valid, false otherwise.</returns>
        private static bool ValidateTableStructure(TableStructure table)
        {
            if (table == null) return false;
            if (table.Rows < 0 || table.Columns < 0) return false;

            // Additional validation: check if any row has more cells than declared columns
            if (table.Cells != null && table.Cells.Any(row => row != null && row.Count > table.Columns))
            {
                // Could log warning here if needed
                // For now, we'll still consider it valid but will handle it during processing
            }

            return true;
        }

        /// <summary>
        /// Sanitize a column name by removing invalid characters and trimming whitespace.
        /// </summary>
        /// <param name="name">Column name to sanitize.</param>
        /// <returns>Sanitized column name.</returns>
        private static string SanitizeColumnName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // Remove leading and trailing whitespace
            name = name.Trim();

            // Additional sanitization can be added here if needed
            // For example, removing special characters that DataTable doesn't allow

            return name;
        }

        /// <summary>
        /// Parse a row from a markdown table.
        /// </summary>
        /// <param name="line">Line to parse.</param>
        /// <returns>List of cell values.</returns>
        private static List<string> ParseRow(string line)
        {
            string test = line.Trim();
            if (test.StartsWith("|")) test = test.Substring(1);
            if (test.EndsWith("|")) test = test.Substring(0, test.Length - 1);

            return test.Split('|')
                .Select(cell => cell.Trim())
                .ToList();
        }

        /// <summary>
        /// Determine if a line is a markdown table separator row.
        /// </summary>
        /// <param name="line">Line to check.</param>
        /// <returns>True if the line is a separator row.</returns>
        private static bool IsSeparatorRow(string line)
        {
            return line.Replace("|", "").Trim().Replace("-", "").Replace(":", "").Trim().Length == 0;
        }

        #endregion
    }
}