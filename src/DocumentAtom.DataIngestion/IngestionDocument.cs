namespace DocumentAtom.DataIngestion
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a document that has been processed for ingestion.
    /// </summary>
    public class IngestionDocument
    {
        #region Public-Members

        /// <summary>
        /// Unique identifier for this document.
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();

        /// <summary>
        /// Source path of the document.
        /// </summary>
        public string? SourcePath { get; set; } = null;

        /// <summary>
        /// Document-level metadata.
        /// </summary>
        public Dictionary<string, object> Metadata { get; set; } = new();

        /// <summary>
        /// Sections within the document, organized by page or logical grouping.
        /// </summary>
        public List<IngestionDocumentSection> Sections { get; set; } = new();

        /// <summary>
        /// Flattened list of all elements in the document.
        /// </summary>
        public List<IngestionDocumentElement> Elements { get; set; } = new();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Represents a document that has been processed for ingestion.
        /// </summary>
        public IngestionDocument()
        {
        }

        #endregion
    }
}
