namespace DocumentAtom.RichText
{
    using System.Collections.Generic;
    using System.Data;

    /// <summary>
    /// Represents an element within an RTF document.
    /// </summary>
    internal class RtfDocumentElement
    {
        /// <summary>
        /// Type of the element.
        /// </summary>
        public RtfElementType Type { get; set; }

        /// <summary>
        /// Text content of the element.
        /// </summary>
        public string Text { get; set; } = String.Empty;

        /// <summary>
        /// Raw RTF content.
        /// </summary>
        public string RawRtf { get; set; } = String.Empty;

        /// <summary>
        /// Whether this is an ordered list.
        /// </summary>
        public bool IsOrderedList { get; set; }

        /// <summary>
        /// Table data if this element is a table.
        /// </summary>
        public DataTable? TableData { get; set; }

        /// <summary>
        /// List items if this element is a list.
        /// </summary>
        public List<string>? ListItems { get; set; }
    }
}