namespace DocumentAtom.RichText
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Represents an RTF document structure.
    /// </summary>
    internal class RtfDocument
    {
        private List<RtfDocumentElement> _Elements = new List<RtfDocumentElement>();

        /// <summary>
        /// Document elements.
        /// </summary>
        public List<RtfDocumentElement> Elements
        {
            get
            {
                return _Elements;
            }
            set
            {
                if (value == null)
                {
                    _Elements = new List<RtfDocumentElement>();
                }
                else
                {
                    _Elements = value;
                }
            }
        }

        private RtfDocumentElement? _CurrentElement;
        private List<string>? _CurrentList;
        private string? _CurrentListItemPrefix;
        private DataTable? _CurrentTable;
        private List<string>? _CurrentTableRow;

        /// <summary>
        /// Add text to the document.
        /// </summary>
        /// <param name="text">Text to add.</param>
        /// <param name="context">Parsing context.</param>
        public void AddText(string text, RtfParseContext context)
        {
            if (context.InTable)
            {
                AddTableText(text, context);
                return;
            }

            // If we have a table in progress but are no longer in table context, finalize it
            if (_CurrentTable != null && !context.InTable)
            {
                FinalizeTables();
            }

            // Check if this looks like a list marker or list content
            bool isListMarker = IsListMarker(text.Trim());
            bool isInListContext = context.ListStyle > 0 || context.InListText || context.InListSequence;
            bool hasPendingListPrefix = !string.IsNullOrEmpty(_CurrentListItemPrefix);

            if (isInListContext || isListMarker || hasPendingListPrefix ||
                (context.InListSequence && IsLikelyContinuationOfList(text, context)))
            {
                AddListText(text, context);
                return;
            }

            // Regular text/paragraph - end any current list first
            if (_CurrentList != null)
            {
                EndCurrentElement();
                context.InListSequence = false;
            }

            if (_CurrentElement == null ||
                _CurrentElement.Type == RtfElementType.List ||
                _CurrentElement.Type == RtfElementType.Table)
            {
                var elementType = DetermineElementType(context);
                _CurrentElement = new RtfDocumentElement
                {
                    Type = elementType,
                    Text = text
                };
            }
            else
            {
                _CurrentElement.Text += text;
            }
        }

        /// <summary>
        /// Add a table cell.
        /// </summary>
        /// <param name="context">Parsing context.</param>
        public void AddTableCell(RtfParseContext context)
        {
            // Cell boundary - handled by AddTableText
        }

        /// <summary>
        /// End the current list.
        /// </summary>
        public void EndCurrentList()
        {
            if (_CurrentList != null && _CurrentList.Count > 0)
            {
                EndCurrentElement(); // This will add the list to Elements
            }
        }

        /// <summary>
        /// End the current table row.
        /// </summary>
        public void EndTableRow()
        {
            if (_CurrentTable != null && _CurrentTableRow != null && _CurrentTableRow.Count > 0)
            {
                // Filter out likely style metadata from table cells
                var filteredRow = new List<string>();
                foreach (string cell in _CurrentTableRow)
                {
                    string cleanCell = cell.Trim();
                    // Skip cells that look like style definitions
                    if (!string.IsNullOrEmpty(cleanCell) &&
                        !cleanCell.Contains("Table Grid") &&
                        !cleanCell.Contains("List Table") &&
                        !cleanCell.Contains("Accent") &&
                        !cleanCell.EndsWith(";") &&
                        cleanCell.Length < 200)
                    {
                        filteredRow.Add(cleanCell);
                    }
                }

                // Only create table if we have reasonable content (limit to max 10 columns)
                if (filteredRow.Count > 0 && filteredRow.Count <= 10)
                {
                    // Ensure we have enough columns for this row
                    while (_CurrentTable.Columns.Count < filteredRow.Count)
                    {
                        _CurrentTable.Columns.Add($"Column{_CurrentTable.Columns.Count + 1}", typeof(string));
                    }

                    var row = _CurrentTable.NewRow();
                    for (int i = 0; i < filteredRow.Count && i < _CurrentTable.Columns.Count; i++)
                    {
                        row[i] = filteredRow[i];
                    }
                    _CurrentTable.Rows.Add(row);
                }

                _CurrentTableRow.Clear();
            }
        }

        /// <summary>
        /// End the current paragraph.
        /// </summary>
        public void EndParagraph()
        {
            // Only end current element if it's not a list in progress
            if (_CurrentElement != null && _CurrentElement.Type != RtfElementType.List)
            {
                if (!string.IsNullOrWhiteSpace(_CurrentElement.Text))
                {
                    Elements.Add(_CurrentElement);
                }
                _CurrentElement = null;
            }
            // Don't end lists on paragraph breaks - they continue until explicitly ended
        }

        /// <summary>
        /// Finalize any pending tables.
        /// </summary>
        public void FinalizeTables()
        {
            EndCurrentElement(); // This will handle table finalization
        }

        private void AddListText(string text, RtfParseContext context)
        {
            if (_CurrentList == null)
            {
                // End any non-list current element
                if (_CurrentElement != null && _CurrentElement.Type != RtfElementType.List)
                {
                    EndCurrentElement();
                }
                _CurrentList = new List<string>();
            }

            string cleanText = text.Trim();
            if (!string.IsNullOrWhiteSpace(cleanText))
            {
                // Check if this text is a standalone list marker
                bool isStandaloneMarker = IsListMarker(cleanText) && !context.InListText;

                if (context.InListText || isStandaloneMarker)
                {
                    // This is a list marker - store as prefix for next content
                    if (isStandaloneMarker || string.IsNullOrEmpty(_CurrentListItemPrefix))
                    {
                        _CurrentListItemPrefix = cleanText;
                    }
                    // Don't add markers as separate list items
                }
                else
                {
                    // This is list item content
                    string itemText = cleanText;
                    if (!string.IsNullOrEmpty(_CurrentListItemPrefix))
                    {
                        // Combine prefix with content
                        itemText = $"{_CurrentListItemPrefix} {cleanText}".Trim();
                        _CurrentListItemPrefix = null; // Clear after using
                    }

                    if (!string.IsNullOrWhiteSpace(itemText))
                    {
                        _CurrentList.Add(itemText);
                    }
                }
            }
        }

        private void AddTableText(string text, RtfParseContext context)
        {
            if (_CurrentTable == null)
            {
                EndCurrentElement();
                _CurrentTable = new DataTable();
                _CurrentTableRow = new List<string>();
            }

            if (_CurrentTableRow != null)
            {
                _CurrentTableRow.Add(text.Trim());
            }
        }

        private void EndCurrentElement()
        {
            if (_CurrentElement != null)
            {
                if (!string.IsNullOrWhiteSpace(_CurrentElement.Text))
                {
                    Elements.Add(_CurrentElement);
                }
                _CurrentElement = null;
            }

            if (_CurrentList != null && _CurrentList.Count > 0)
            {
                Elements.Add(new RtfDocumentElement
                {
                    Type = RtfElementType.List,
                    ListItems = new List<string>(_CurrentList),
                    IsOrderedList = DetermineIfOrderedList(_CurrentList)
                });
                _CurrentList = null;
            }

            if (_CurrentTable != null)
            {
                EndTableRow();
                if (_CurrentTable.Rows.Count > 0)
                {
                    Elements.Add(new RtfDocumentElement
                    {
                        Type = RtfElementType.Table,
                        TableData = _CurrentTable
                    });
                }
                _CurrentTable = null;
                _CurrentTableRow = null;
            }
        }

        private RtfElementType DetermineElementType(RtfParseContext context)
        {
            if (context.OutlineLevel == 0) return RtfElementType.Header1;
            if (context.OutlineLevel == 1) return RtfElementType.Header2;
            if (context.OutlineLevel == 2) return RtfElementType.Header3;
            return RtfElementType.Paragraph;
        }

        private bool IsListMarker(string text)
        {
            return text.Trim() == "•" || text.Trim() == "◦" ||
                   System.Text.RegularExpressions.Regex.IsMatch(text.Trim(), @"^\d+\.$");
        }

        private bool IsLikelyContinuationOfList(string text, RtfParseContext context)
        {
            // Only continue list if we have an active list and the text looks like list content
            if (_CurrentList == null) return false;

            // If we just processed a list marker prefix, this is likely the content
            if (!string.IsNullOrEmpty(_CurrentListItemPrefix)) return true;

            // Don't continue if text looks like start of new paragraph/section
            var trimmed = text.Trim();
            if (string.IsNullOrWhiteSpace(trimmed)) return false;

            // Simple heuristic: if text doesn't start with capital letter after period,
            // it might be continuation. But be conservative.
            return false;
        }

        private bool DetermineIfOrderedList(List<string> listItems)
        {
            if (listItems == null || listItems.Count == 0) return false;

            // Check if any item starts with a number followed by a period
            foreach (string item in listItems)
            {
                if (string.IsNullOrWhiteSpace(item)) continue;

                string trimmed = item.Trim();
                // Look for numbered list pattern: "1. Item", "2. Item", etc.
                if (System.Text.RegularExpressions.Regex.IsMatch(trimmed, @"^\d+\."))
                {
                    return true;
                }
            }

            return false;
        }
    }
}