namespace DocumentAtom.Core.Atoms
{
    using System.Data;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;

    /// <summary>
    /// A docx atom is a self-contained unit of information from a Microsoft Word .docx document.
    /// </summary>
    public class DocxAtom : AtomBase<MarkdownAtom>
    {
        #region Public-Members

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; }

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

        #endregion

        #region Private-Members

        private int? _HeaderLevel = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A docx atom is a self-contained unit of information from a Microsoft Word .docx document.
        /// </summary>
        public DocxAtom()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Determine if a text item is part of an unordered list.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if the text represents an unordered list.</returns>
        public static bool IsUnorderedListItem(string text)
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
        /// Determine if a text item is part of an ordered list.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if the text represents an ordered list.</returns>
        public static bool IsOrderedListItem(string text)
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
        /// Determine if a text item is part of a table.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>True if item is part of a table.</returns>
        public static bool IsTableItem(string text)
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
        public static SerializableDataTable TextToDataTable(string text)
        {
            if (string.IsNullOrEmpty(text)) return null; var dataTable = new DataTable();

            // Split into lines and remove empty ones
            var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();

            if (lines.Count < 1) return null;

            // Process header row
            var headers = ParseRow(lines[0]);
            foreach (var header in headers)
            {
                dataTable.Columns.Add(new DataColumn(header ?? string.Empty));
            }

            // Process data rows (skip separator row if it exists)
            for (int i = 1; i < lines.Count; i++)
            {
                var row = ParseRow(lines[i]);
                if (row.Count == headers.Count && !IsSeparatorRow(lines[i]))
                {
                    dataTable.Rows.Add(row.ToArray());
                }
            }

            return SerializableDataTable.FromDataTable(dataTable);
        }

        /// <summary>
        /// Convert a markdown list to a list of strings.
        /// </summary>
        /// <param name="text">Text.</param>
        /// <returns>List of strings.</returns>
        public static List<string> TextToList(string text)
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

        #region DataTable

        private static List<string> ParseRow(string line)
        {
            string test = line.Trim();
            if (test.StartsWith("|")) test = test.Substring(1);
            if (test.EndsWith("|")) test = test.Substring(0, test.Length - 1);

            return test.Split('|')
                .Select(cell => cell.Trim())
                .ToList();
        }

        private static bool IsSeparatorRow(string line)
        {
            return line.Replace("|", "").Trim().Replace("-", "").Replace(":", "").Trim().Length == 0;
        }

        #endregion

        #endregion
    }
}
