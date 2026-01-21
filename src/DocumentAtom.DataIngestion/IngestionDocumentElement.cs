namespace DocumentAtom.DataIngestion
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents an element within an ingestion document.
    /// </summary>
    public class IngestionDocumentElement
    {
        #region Public-Members

        /// <summary>
        /// Unique identifier for this element.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// The type of element.
        /// </summary>
        public IngestionElementType ElementType { get; set; } = IngestionElementType.Paragraph;

        /// <summary>
        /// Text content of the element.
        /// </summary>
        public string? Content { get; set; } = null;

        /// <summary>
        /// Binary content for images or attachments.
        /// </summary>
        public byte[]? BinaryContent { get; set; } = null;

        /// <summary>
        /// Element-level metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Represents an element within an ingestion document.
        /// </summary>
        public IngestionDocumentElement()
        {
        }

        #endregion
    }
}
