namespace DocumentAtom.DataIngestion
{
    using System.Collections.Generic;

    /// <summary>
    /// Represents a section within an ingestion document.
    /// </summary>
    public class IngestionDocumentSection
    {
        #region Public-Members

        /// <summary>
        /// Page number for this section, if applicable.
        /// </summary>
        public int? PageNumber { get; set; } = null;

        /// <summary>
        /// Section title, if applicable.
        /// </summary>
        public string? Title { get; set; } = null;

        /// <summary>
        /// Elements within this section.
        /// </summary>
        public List<IngestionDocumentElement> Elements { get; set; } = new();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Represents a section within an ingestion document.
        /// </summary>
        public IngestionDocumentSection()
        {
        }

        #endregion
    }
}
