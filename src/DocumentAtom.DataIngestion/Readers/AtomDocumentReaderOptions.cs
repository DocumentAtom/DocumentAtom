namespace DocumentAtom.DataIngestion.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using DocumentAtom.Core;
    using DocumentAtom.Core.TypeDetection;

    /// <summary>
    /// Configuration options for AtomDocumentReader.
    /// </summary>
    public class AtomDocumentReaderOptions
    {
        #region Public-Members

        /// <summary>
        /// Directory for temporary files during processing.
        /// Default is the system temp path with DocumentAtom subfolder.
        /// </summary>
        public string TempDirectory
        {
            get
            {
                return _TempDirectory;
            }
            set
            {
                if (string.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TempDirectory));
                value = value.Replace("\\", "/");
                if (!value.EndsWith("/")) value += "/";
                _TempDirectory = value;
                if (!Directory.Exists(_TempDirectory)) Directory.CreateDirectory(_TempDirectory);
            }
        }

        /// <summary>
        /// Whether to preserve full serialized Atom data in element metadata.
        /// Enables lossless round-trip and advanced chunking.
        /// Default is true.
        /// </summary>
        public bool PreserveFullAtomData { get; set; } = true;

        /// <summary>
        /// Whether to extract text from images using OCR.
        /// Default is true.
        /// </summary>
        public bool EnableOcr { get; set; } = true;

        /// <summary>
        /// Whether to build document hierarchy from heading structure.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        /// <summary>
        /// Whether to include binary content (images, attachments) in output.
        /// Default is true.
        /// </summary>
        public bool IncludeBinaryContent { get; set; } = true;

        /// <summary>
        /// Maximum file size in bytes to process. 0 = unlimited.
        /// Default is 0 (unlimited).
        /// </summary>
        public long MaxFileSizeBytes
        {
            get
            {
                return _MaxFileSizeBytes;
            }
            set
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(MaxFileSizeBytes));
                _MaxFileSizeBytes = value;
            }
        }

        /// <summary>
        /// Settings for chunking during extraction.
        /// </summary>
        public ChunkingSettings? ChunkingSettings { get; set; } = null;

        /// <summary>
        /// Custom processor settings by document type.
        /// </summary>
        public Dictionary<DocumentTypeEnum, ProcessorSettingsBase> ProcessorSettings { get; set; } = new();

        /// <summary>
        /// Metadata keys to exclude from output.
        /// </summary>
        public HashSet<string> ExcludedMetadataKeys { get; set; } = new();

        #endregion

        #region Private-Members

        private string _TempDirectory = Path.Combine(Path.GetTempPath(), "DocumentAtom/");
        private long _MaxFileSizeBytes = 0;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Configuration options for AtomDocumentReader.
        /// </summary>
        public AtomDocumentReaderOptions()
        {
            if (!Directory.Exists(_TempDirectory)) Directory.CreateDirectory(_TempDirectory);
        }

        #endregion
    }
}
