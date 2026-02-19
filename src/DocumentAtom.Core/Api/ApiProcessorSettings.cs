namespace DocumentAtom.Core.Api
{
    using DocumentAtom.Core.Chunking;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Flat settings class for API requests.
    /// All fields are nullable so callers can override only the settings they care about.
    /// Each route handler maps relevant fields to its processor-specific settings type.
    /// Security-sensitive fields (TempDirectory, TesseractDataDirectory, TesseractLanguage,
    /// StreamBufferSize, CommonHeaderRowTerms, HeaderRowPatternWeights, ListMarkers,
    /// ListNumberingPatterns, QuoteCharacters, DebugLogging) are intentionally excluded.
    /// </summary>
    public class ApiProcessorSettings
    {
        #region Public-Members

        // ----- Base settings (safe subset) -----

        /// <summary>
        /// True to trim any text output.
        /// </summary>
        [JsonPropertyName("TrimText")]
        public bool? TrimText { get; set; } = null;

        /// <summary>
        /// True to remove binary data from input text data.
        /// </summary>
        [JsonPropertyName("RemoveBinaryFromText")]
        public bool? RemoveBinaryFromText { get; set; } = null;

        /// <summary>
        /// True to extract atoms from images using OCR.
        /// </summary>
        [JsonPropertyName("ExtractAtomsFromImages")]
        public bool? ExtractAtomsFromImages { get; set; } = null;

        /// <summary>
        /// Chunking configuration.
        /// </summary>
        [JsonPropertyName("Chunking")]
        public ChunkingConfiguration? Chunking { get; set; } = null;

        // ----- Text / Markdown -----

        /// <summary>
        /// Delimiters for text and markdown processing.
        /// </summary>
        [JsonPropertyName("Delimiters")]
        public List<string>? Delimiters { get; set; } = null;

        /// <summary>
        /// Whether to build document hierarchy from heading structure.
        /// </summary>
        [JsonPropertyName("BuildHierarchy")]
        public bool? BuildHierarchy { get; set; } = null;

        // ----- CSV -----

        /// <summary>
        /// Row delimiter for CSV processing.
        /// </summary>
        [JsonPropertyName("RowDelimiter")]
        public string? RowDelimiter { get; set; } = null;

        /// <summary>
        /// Column delimiter for CSV processing.
        /// </summary>
        [JsonPropertyName("ColumnDelimiter")]
        public char? ColumnDelimiter { get; set; } = null;

        /// <summary>
        /// Whether the CSV has a header row.
        /// </summary>
        [JsonPropertyName("HasHeaderRow")]
        public bool? HasHeaderRow { get; set; } = null;

        /// <summary>
        /// Number of rows per atom for CSV processing.
        /// </summary>
        [JsonPropertyName("RowsPerAtom")]
        public int? RowsPerAtom { get; set; } = null;

        // ----- HTML -----

        /// <summary>
        /// Whether to process inline styles in HTML.
        /// </summary>
        [JsonPropertyName("ProcessInlineStyles")]
        public bool? ProcessInlineStyles { get; set; } = null;

        /// <summary>
        /// Whether to process meta tags in HTML.
        /// </summary>
        [JsonPropertyName("ProcessMetaTags")]
        public bool? ProcessMetaTags { get; set; } = null;

        /// <summary>
        /// Whether to process scripts in HTML.
        /// </summary>
        [JsonPropertyName("ProcessScripts")]
        public bool? ProcessScripts { get; set; } = null;

        /// <summary>
        /// Whether to process comments in HTML.
        /// </summary>
        [JsonPropertyName("ProcessComments")]
        public bool? ProcessComments { get; set; } = null;

        /// <summary>
        /// Whether to preserve whitespace in HTML.
        /// </summary>
        [JsonPropertyName("PreserveWhitespace")]
        public bool? PreserveWhitespace { get; set; } = null;

        /// <summary>
        /// Maximum text length for HTML processing.
        /// </summary>
        [JsonPropertyName("MaxTextLength")]
        public int? MaxTextLength { get; set; } = null;

        /// <summary>
        /// Whether to process SVG in HTML.
        /// </summary>
        [JsonPropertyName("ProcessSvg")]
        public bool? ProcessSvg { get; set; } = null;

        /// <summary>
        /// Whether to extract data attributes in HTML.
        /// </summary>
        [JsonPropertyName("ExtractDataAttributes")]
        public bool? ExtractDataAttributes { get; set; } = null;

        // ----- JSON / XML -----

        /// <summary>
        /// Maximum depth for JSON and XML processing.
        /// </summary>
        [JsonPropertyName("MaxDepth")]
        public int? MaxDepth { get; set; } = null;

        /// <summary>
        /// Whether to include attributes in XML processing.
        /// </summary>
        [JsonPropertyName("IncludeAttributes")]
        public bool? IncludeAttributes { get; set; } = null;

        // ----- Excel -----

        /// <summary>
        /// Header row score threshold for Excel processing.
        /// </summary>
        [JsonPropertyName("HeaderRowScoreThreshold")]
        public int? HeaderRowScoreThreshold { get; set; } = null;

        // ----- Image / OCR -----

        /// <summary>
        /// Line threshold for OCR processing.
        /// </summary>
        [JsonPropertyName("LineThreshold")]
        public int? LineThreshold { get; set; } = null;

        /// <summary>
        /// Paragraph threshold for OCR processing.
        /// </summary>
        [JsonPropertyName("ParagraphThreshold")]
        public int? ParagraphThreshold { get; set; } = null;

        /// <summary>
        /// Horizontal line length for OCR processing.
        /// </summary>
        [JsonPropertyName("HorizontalLineLength")]
        public int? HorizontalLineLength { get; set; } = null;

        /// <summary>
        /// Vertical line length for OCR processing.
        /// </summary>
        [JsonPropertyName("VerticalLineLength")]
        public int? VerticalLineLength { get; set; } = null;

        /// <summary>
        /// Table minimum area for OCR processing.
        /// </summary>
        [JsonPropertyName("TableMinArea")]
        public int? TableMinArea { get; set; } = null;

        /// <summary>
        /// Column alignment tolerance for OCR processing.
        /// </summary>
        [JsonPropertyName("ColumnAlignmentTolerance")]
        public int? ColumnAlignmentTolerance { get; set; } = null;

        /// <summary>
        /// Proximity threshold for OCR processing.
        /// </summary>
        [JsonPropertyName("ProximityThreshold")]
        public int? ProximityThreshold { get; set; } = null;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Flat settings class for API requests.
        /// </summary>
        public ApiProcessorSettings()
        {

        }

        #endregion
    }
}
