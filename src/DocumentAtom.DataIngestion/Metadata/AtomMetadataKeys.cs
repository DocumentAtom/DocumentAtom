namespace DocumentAtom.DataIngestion.Metadata
{
    /// <summary>
    /// Constants for metadata key names used in the DataIngestion pipeline.
    /// </summary>
    public static class AtomMetadataKeys
    {
        #region Atom-Keys

        /// <summary>
        /// Unique identifier for the atom.
        /// </summary>
        public const string AtomGuid = "atom:guid";

        /// <summary>
        /// Parent atom GUID for hierarchy.
        /// </summary>
        public const string AtomParentGuid = "atom:parent_guid";

        /// <summary>
        /// Atom type name.
        /// </summary>
        public const string AtomType = "atom:type";

        /// <summary>
        /// Source page number.
        /// </summary>
        public const string AtomPageNumber = "atom:page_number";

        /// <summary>
        /// Ordinal position in document.
        /// </summary>
        public const string AtomPosition = "atom:position";

        /// <summary>
        /// Content length.
        /// </summary>
        public const string AtomLength = "atom:length";

        /// <summary>
        /// MD5 hash of content.
        /// </summary>
        public const string AtomMd5 = "atom:md5";

        /// <summary>
        /// SHA1 hash of content.
        /// </summary>
        public const string AtomSha1 = "atom:sha1";

        /// <summary>
        /// SHA256 hash of content.
        /// </summary>
        public const string AtomSha256 = "atom:sha256";

        /// <summary>
        /// Header level (1-6).
        /// </summary>
        public const string AtomHeaderLevel = "atom:header_level";

        /// <summary>
        /// Atom title.
        /// </summary>
        public const string AtomTitle = "atom:title";

        /// <summary>
        /// Atom subtitle.
        /// </summary>
        public const string AtomSubtitle = "atom:subtitle";

        /// <summary>
        /// Markdown formatting type.
        /// </summary>
        public const string AtomFormatting = "atom:formatting";

        /// <summary>
        /// Bounding box JSON.
        /// </summary>
        public const string AtomBoundingBox = "atom:bounding_box";

        /// <summary>
        /// Excel sheet name.
        /// </summary>
        public const string AtomSheetName = "atom:sheet_name";

        /// <summary>
        /// Excel cell identifier.
        /// </summary>
        public const string AtomCellId = "atom:cell_id";

        /// <summary>
        /// Table row count.
        /// </summary>
        public const string AtomRows = "atom:rows";

        /// <summary>
        /// Table column count.
        /// </summary>
        public const string AtomColumns = "atom:columns";

        /// <summary>
        /// Whether atom has quarks.
        /// </summary>
        public const string AtomHasQuarks = "atom:has_quarks";

        /// <summary>
        /// Number of quarks.
        /// </summary>
        public const string AtomQuarkCount = "atom:quark_count";

        /// <summary>
        /// Full serialized atom JSON.
        /// </summary>
        public const string AtomSerialized = "atom:serialized";

        /// <summary>
        /// Total atoms in document.
        /// </summary>
        public const string AtomTotalAtoms = "atom:total_atoms";

        /// <summary>
        /// Extraction timestamp.
        /// </summary>
        public const string AtomExtractionTimestamp = "atom:extraction_timestamp";

        /// <summary>
        /// Quark index within parent atom.
        /// </summary>
        public const string AtomQuarkIndex = "atom:quark_index";

        /// <summary>
        /// Total quarks in parent atom.
        /// </summary>
        public const string AtomTotalQuarks = "atom:total_quarks";

        /// <summary>
        /// Parent atom GUID for quark.
        /// </summary>
        public const string AtomParentAtomGuid = "atom:parent_atom_guid";

        /// <summary>
        /// Document-level GUID.
        /// </summary>
        public const string AtomDocumentGuid = "atom:document_guid";

        #endregion

        #region Source-Keys

        /// <summary>
        /// Source file path.
        /// </summary>
        public const string SourcePath = "source:path";

        /// <summary>
        /// Source filename.
        /// </summary>
        public const string SourceFilename = "source:filename";

        /// <summary>
        /// Source file extension.
        /// </summary>
        public const string SourceExtension = "source:extension";

        /// <summary>
        /// Document type.
        /// </summary>
        public const string SourceDocumentType = "source:document_type";

        /// <summary>
        /// MIME type.
        /// </summary>
        public const string SourceMimeType = "source:mime_type";

        /// <summary>
        /// Original filename for byte array inputs.
        /// </summary>
        public const string SourceOriginalFilename = "source:original_filename";

        /// <summary>
        /// Indicates input was a byte array.
        /// </summary>
        public const string SourceWasByteArray = "source:was_byte_array";

        #endregion

        #region Chunk-Keys

        /// <summary>
        /// Chunk index.
        /// </summary>
        public const string ChunkIndex = "chunk:index";

        /// <summary>
        /// Header context breadcrumb.
        /// </summary>
        public const string ChunkHeaderContext = "chunk:header_context";

        /// <summary>
        /// Chunk source type.
        /// </summary>
        public const string ChunkSource = "chunk:source";

        /// <summary>
        /// Split index for token-split chunks.
        /// </summary>
        public const string ChunkSplitIndex = "chunk:split_index";

        #endregion

        #region Hierarchy-Keys

        /// <summary>
        /// Hierarchy ID.
        /// </summary>
        public const string HierarchyId = "hierarchy:id";

        /// <summary>
        /// Hierarchy parent ID.
        /// </summary>
        public const string HierarchyParentId = "hierarchy:parent_id";

        /// <summary>
        /// Hierarchy level.
        /// </summary>
        public const string HierarchyLevel = "hierarchy:level";

        #endregion

        #region Computed-Keys

        /// <summary>
        /// Computed SHA256 hash.
        /// </summary>
        public const string ComputedSha256 = "computed:sha256";

        #endregion

        #region Processor-Keys

        /// <summary>
        /// Number of duplicates removed.
        /// </summary>
        public const string ProcessorDuplicatesRemoved = "processor:duplicates_removed";

        #endregion

        #region Section-Keys

        /// <summary>
        /// Section title.
        /// </summary>
        public const string SectionTitle = "section:title";

        /// <summary>
        /// Section level.
        /// </summary>
        public const string SectionLevel = "section:level";

        /// <summary>
        /// Atom count in section.
        /// </summary>
        public const string SectionAtomCount = "section:atom_count";

        #endregion
    }
}
