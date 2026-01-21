namespace DocumentAtom.DataIngestion.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.DataIngestion.Metadata;
    using DocumentAtom.DataIngestion.Readers;
    using SerializableDataTables;

    /// <summary>
    /// Converts Atoms to IngestionDocumentElements.
    /// </summary>
    public class AtomToIngestionElementConverter : IAtomToIngestionElementConverter
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly AtomDocumentReaderOptions _Options;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Converts Atoms to IngestionDocumentElements.
        /// </summary>
        /// <param name="options">Reader options.</param>
        public AtomToIngestionElementConverter(AtomDocumentReaderOptions? options = null)
        {
            _Options = options ?? new AtomDocumentReaderOptions();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Convert an Atom to an IngestionDocumentElement.
        /// </summary>
        /// <param name="atom">The atom to convert.</param>
        /// <param name="hierarchyMap">Map of GUID to Atom for hierarchy lookups.</param>
        /// <returns>The converted element, or null if the atom should be skipped.</returns>
        public IngestionDocumentElement? Convert(Atom atom, Dictionary<Guid, Atom>? hierarchyMap = null)
        {
            if (atom == null) return null;

            IngestionDocumentElement element = new IngestionDocumentElement
            {
                Id = atom.GUID.ToString()
            };

            // Determine element type and extract content
            ExtractedContent extracted = ExtractContent(atom);

            // Skip meta atoms and empty content unless it's binary
            if (atom.Type == AtomTypeEnum.Meta)
            {
                return null;
            }

            if (string.IsNullOrEmpty(extracted.TextContent) && extracted.BinaryContent == null)
            {
                return null;
            }

            // Skip binary content if not configured to include it
            if (extracted.BinaryContent != null && !_Options.IncludeBinaryContent)
            {
                return null;
            }

            element.ElementType = extracted.ElementType;
            element.Content = extracted.TextContent;
            element.BinaryContent = extracted.BinaryContent;

            // Add metadata
            Dictionary<string, object> metadata = MetadataSerializer.SerializeAtomMetadata(atom);
            foreach (KeyValuePair<string, object> kvp in metadata)
            {
                if (!_Options.ExcludedMetadataKeys.Contains(kvp.Key))
                {
                    element.Metadata[kvp.Key] = kvp.Value;
                }
            }

            return element;
        }

        #endregion

        #region Private-Methods

        private ExtractedContent ExtractContent(Atom atom)
        {
            switch (atom.Type)
            {
                case AtomTypeEnum.Text:
                    return ExtractTextContent(atom);

                case AtomTypeEnum.List:
                    return ExtractListContent(atom);

                case AtomTypeEnum.Table:
                    return ExtractTableContent(atom);

                case AtomTypeEnum.Image:
                    return ExtractImageContent(atom);

                case AtomTypeEnum.Binary:
                    return ExtractBinaryContent(atom);

                case AtomTypeEnum.Hyperlink:
                    return ExtractHyperlinkContent(atom);

                case AtomTypeEnum.Code:
                    return ExtractCodeContent(atom);

                case AtomTypeEnum.Meta:
                    return new ExtractedContent(IngestionElementType.Unknown, null, null);

                case AtomTypeEnum.Unknown:
                default:
                    return new ExtractedContent(IngestionElementType.Unknown, atom.Text, null);
            }
        }

        private ExtractedContent ExtractTextContent(Atom atom)
        {
            // Check if this is a header
            if (atom.HeaderLevel.HasValue && atom.HeaderLevel.Value > 0)
            {
                return new ExtractedContent(IngestionElementType.Header, atom.Text, null);
            }

            // Check markdown formatting
            if (atom.Formatting.HasValue)
            {
                switch (atom.Formatting.Value)
                {
                    case MarkdownFormattingEnum.Header:
                        return new ExtractedContent(IngestionElementType.Header, atom.Text, null);

                    case MarkdownFormattingEnum.OrderedList:
                    case MarkdownFormattingEnum.UnorderedList:
                        return ExtractListContent(atom);

                    case MarkdownFormattingEnum.Table:
                        return ExtractTableContent(atom);

                    case MarkdownFormattingEnum.Code:
                        return new ExtractedContent(IngestionElementType.Code, atom.Text, null);
                }
            }

            return new ExtractedContent(IngestionElementType.Paragraph, atom.Text, null);
        }

        private ExtractedContent ExtractListContent(Atom atom)
        {
            StringBuilder sb = new StringBuilder();

            if (atom.OrderedList != null && atom.OrderedList.Count > 0)
            {
                for (int i = 0; i < atom.OrderedList.Count; i++)
                {
                    sb.AppendLine($"{i + 1}. {atom.OrderedList[i]}");
                }
            }
            else if (atom.UnorderedList != null && atom.UnorderedList.Count > 0)
            {
                foreach (string item in atom.UnorderedList)
                {
                    sb.AppendLine($"- {item}");
                }
            }
            else if (!string.IsNullOrEmpty(atom.Text))
            {
                return new ExtractedContent(IngestionElementType.List, atom.Text, null);
            }

            string content = sb.ToString().TrimEnd();
            return new ExtractedContent(IngestionElementType.List, string.IsNullOrEmpty(content) ? null : content, null);
        }

        private ExtractedContent ExtractTableContent(Atom atom)
        {
            if (atom.Table == null)
            {
                return new ExtractedContent(IngestionElementType.Table, atom.Text, null);
            }

            string markdown = RenderTableAsMarkdown(atom.Table);
            return new ExtractedContent(IngestionElementType.Table, markdown, null);
        }

        private ExtractedContent ExtractImageContent(Atom atom)
        {
            // Return text content (OCR result) and binary content
            return new ExtractedContent(IngestionElementType.Image, atom.Text, atom.Binary);
        }

        private ExtractedContent ExtractBinaryContent(Atom atom)
        {
            return new ExtractedContent(IngestionElementType.Binary, null, atom.Binary);
        }

        private ExtractedContent ExtractHyperlinkContent(Atom atom)
        {
            string linkText = atom.Title ?? atom.Text ?? "link";
            string url = atom.Text ?? string.Empty;
            string markdown = $"[{linkText}]({url})";
            return new ExtractedContent(IngestionElementType.Paragraph, markdown, null);
        }

        private ExtractedContent ExtractCodeContent(Atom atom)
        {
            string content = $"```\n{atom.Text}\n```";
            return new ExtractedContent(IngestionElementType.Code, content, null);
        }

        private string RenderTableAsMarkdown(SerializableDataTable table)
        {
            StringBuilder sb = new StringBuilder();

            // Header row
            if (table.Columns != null && table.Columns.Count > 0)
            {
                sb.AppendLine("| " + string.Join(" | ", table.Columns) + " |");
                sb.AppendLine("| " + string.Join(" | ", table.Columns.Select(_ => "---")) + " |");
            }

            // Data rows - use DataTable conversion for reliable access
            try
            {
                System.Data.DataTable dt = table.ToDataTable();

                foreach (System.Data.DataRow row in dt.Rows)
                {
                    IEnumerable<string> cellValues = row.ItemArray.Select(cell => cell?.ToString() ?? string.Empty);
                    sb.AppendLine("| " + string.Join(" | ", cellValues) + " |");
                }
            }
            catch
            {
                // Fallback if conversion fails
            }

            return sb.ToString().TrimEnd();
        }

        #endregion

        #region Private-Classes

        /// <summary>
        /// Holds the result of content extraction from an Atom.
        /// </summary>
        private class ExtractedContent
        {
            /// <summary>
            /// The type of element.
            /// </summary>
            public IngestionElementType ElementType { get; }

            /// <summary>
            /// Text content.
            /// </summary>
            public string? TextContent { get; }

            /// <summary>
            /// Binary content.
            /// </summary>
            public byte[]? BinaryContent { get; }

            /// <summary>
            /// Creates a new ExtractedContent instance.
            /// </summary>
            /// <param name="elementType">Element type.</param>
            /// <param name="textContent">Text content.</param>
            /// <param name="binaryContent">Binary content.</param>
            public ExtractedContent(IngestionElementType elementType, string? textContent, byte[]? binaryContent)
            {
                ElementType = elementType;
                TextContent = textContent;
                BinaryContent = binaryContent;
            }
        }

        #endregion
    }
}
