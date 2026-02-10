namespace DocumentAtom.DataIngestion.Readers
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.DataIngestion.Converters;
    using DocumentAtom.DataIngestion.Metadata;

    /// <summary>
    /// Reads documents and converts them to IngestionDocument format.
    /// </summary>
    public class AtomDocumentReader : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Reader options.
        /// </summary>
        public AtomDocumentReaderOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Options));
                _Options = value;
            }
        }

        /// <summary>
        /// Logger action for debug output.
        /// </summary>
        public Action<string>? Logger { get; set; } = null;

        #endregion

        #region Private-Members

        private AtomDocumentReaderOptions _Options;
        private readonly IProcessorFactory _ProcessorFactory;
        private readonly IAtomToIngestionElementConverter _Converter;
        private readonly TypeDetector _TypeDetector;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Reads documents and converts them to IngestionDocument format.
        /// </summary>
        /// <param name="options">Reader options.</param>
        /// <param name="processorFactory">Optional custom processor factory.</param>
        /// <param name="converter">Optional custom converter.</param>
        public AtomDocumentReader(
            AtomDocumentReaderOptions? options = null,
            IProcessorFactory? processorFactory = null,
            IAtomToIngestionElementConverter? converter = null)
        {
            _Options = options ?? new AtomDocumentReaderOptions();
            _ProcessorFactory = processorFactory ?? new DefaultProcessorFactory(_Options);
            _Converter = converter ?? new AtomToIngestionElementConverter(_Options);
            _TypeDetector = new TypeDetector(_Options.TempDirectory);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _TypeDetector?.Dispose();
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Read a document from a file path.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The ingestion document.</returns>
        public async Task<IngestionDocument> ReadAsync(string path, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("File not found.", path);

            cancellationToken.ThrowIfCancellationRequested();

            // Check file size
            FileInfo fileInfo = new FileInfo(path);
            if (_Options.MaxFileSizeBytes > 0 && fileInfo.Length > _Options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"File size {fileInfo.Length} exceeds maximum allowed size {_Options.MaxFileSizeBytes}.");
            }

            // Detect document type
            TypeResult typeResult = _TypeDetector.Process(path);

            Log($"Detected document type: {typeResult.Type} for {path}");

            return await ProcessDocumentAsync(path, typeResult, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Read a document from a byte array.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <param name="contentType">Optional content type hint.</param>
        /// <param name="originalFilename">Optional original filename.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>The ingestion document.</returns>
        public async Task<IngestionDocument> ReadAsync(
            byte[] data,
            string? contentType = null,
            string? originalFilename = null,
            CancellationToken cancellationToken = default)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));

            cancellationToken.ThrowIfCancellationRequested();

            // Check size
            if (_Options.MaxFileSizeBytes > 0 && data.Length > _Options.MaxFileSizeBytes)
            {
                throw new InvalidOperationException($"Data size {data.Length} exceeds maximum allowed size {_Options.MaxFileSizeBytes}.");
            }

            // Detect document type
            TypeResult typeResult = _TypeDetector.Process(data, contentType);

            Log($"Detected document type: {typeResult.Type}");

            // Write to temp file for processing
            string tempPath = Path.Combine(_Options.TempDirectory, Guid.NewGuid().ToString());
            if (!string.IsNullOrEmpty(typeResult.Extension))
            {
                tempPath += "." + typeResult.Extension;
            }

            try
            {
                await File.WriteAllBytesAsync(tempPath, data, cancellationToken).ConfigureAwait(false);

                IngestionDocument doc = await ProcessDocumentAsync(tempPath, typeResult, cancellationToken).ConfigureAwait(false);

                // Mark as byte array input
                doc.Metadata[AtomMetadataKeys.SourceWasByteArray] = true;
                if (!string.IsNullOrEmpty(originalFilename))
                {
                    doc.Metadata[AtomMetadataKeys.SourceOriginalFilename] = originalFilename;
                }

                return doc;
            }
            finally
            {
                // Clean up temp file
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
        }

        /// <summary>
        /// Read a document synchronously.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>The ingestion document.</returns>
        public IngestionDocument Read(string path)
        {
            return ReadAsync(path, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Read a document from a byte array synchronously.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <param name="contentType">Optional content type hint.</param>
        /// <param name="originalFilename">Optional original filename.</param>
        /// <returns>The ingestion document.</returns>
        public IngestionDocument Read(byte[] data, string? contentType = null, string? originalFilename = null)
        {
            return ReadAsync(data, contentType, originalFilename, CancellationToken.None).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get supported document types.
        /// </summary>
        /// <returns>List of supported document types.</returns>
        public static IReadOnlyList<DocumentTypeEnum> GetSupportedTypes()
        {
            return new List<DocumentTypeEnum>
            {
                DocumentTypeEnum.Pdf,
                DocumentTypeEnum.Docx,
                DocumentTypeEnum.Xlsx,
                DocumentTypeEnum.Pptx,
                DocumentTypeEnum.Html,
                DocumentTypeEnum.Markdown,
                DocumentTypeEnum.Json,
                DocumentTypeEnum.Csv,
                DocumentTypeEnum.Xml,
                DocumentTypeEnum.Rtf,
                DocumentTypeEnum.Png,
                DocumentTypeEnum.Jpeg,
                DocumentTypeEnum.Gif,
                DocumentTypeEnum.Tiff,
                DocumentTypeEnum.Bmp,
                DocumentTypeEnum.WebP,
                DocumentTypeEnum.Text
            };
        }

        #endregion

        #region Private-Methods

        private async Task<IngestionDocument> ProcessDocumentAsync(
            string path,
            TypeResult typeResult,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            IngestionDocument document = new IngestionDocument
            {
                SourcePath = path
            };

            // Add source metadata
            document.Metadata[AtomMetadataKeys.SourcePath] = path;
            document.Metadata[AtomMetadataKeys.SourceFilename] = Path.GetFileName(path);
            document.Metadata[AtomMetadataKeys.SourceExtension] = Path.GetExtension(path);
            document.Metadata[AtomMetadataKeys.SourceDocumentType] = typeResult.Type.ToString();
            document.Metadata[AtomMetadataKeys.SourceMimeType] = typeResult.MimeType ?? "application/octet-stream";
            document.Metadata[AtomMetadataKeys.AtomExtractionTimestamp] = DateTime.UtcNow.ToString("O");

            // Create processor
            using ProcessorBase? processor = _ProcessorFactory.CreateProcessor(typeResult.Type, path);

            if (processor == null)
            {
                Log($"No processor available for document type: {typeResult.Type}");
                return document;
            }

            // Extract atoms
            List<Atom> atoms = new List<Atom>();
            Dictionary<Guid, Atom> hierarchyMap = new Dictionary<Guid, Atom>();

            await Task.Run(() =>
            {
                foreach (Atom atom in processor.Extract(path))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    atoms.Add(atom);
                    hierarchyMap[atom.GUID] = atom;
                }
            }, cancellationToken).ConfigureAwait(false);

            document.Metadata[AtomMetadataKeys.AtomTotalAtoms] = atoms.Count;

            Log($"Extracted {atoms.Count} atoms from {path}");

            // Convert atoms to elements
            foreach (Atom atom in atoms)
            {
                cancellationToken.ThrowIfCancellationRequested();

                IngestionDocumentElement? element = _Converter.Convert(atom, hierarchyMap);

                if (element != null)
                {
                    // Add document-level metadata reference
                    element.Metadata[AtomMetadataKeys.AtomDocumentGuid] = document.Id;

                    // Serialize full atom if requested
                    if (_Options.PreserveFullAtomData)
                    {
                        element.Metadata[AtomMetadataKeys.AtomSerialized] = MetadataSerializer.SerializeAtom(atom);
                    }

                    document.Elements.Add(element);

                    // Organize by page/section
                    int pageNumber = atom.PageNumber ?? 0;
                    IngestionDocumentSection? section = document.Sections.FirstOrDefault(s => s.PageNumber == pageNumber);

                    if (section == null)
                    {
                        section = new IngestionDocumentSection
                        {
                            PageNumber = pageNumber
                        };
                        document.Sections.Add(section);
                    }

                    section.Elements.Add(element);

                    // Process quarks as sub-elements if present
                    if (atom.Quarks != null && atom.Quarks.Count > 0)
                    {
                        int quarkIndex = 0;
                        foreach (Atom quark in atom.Quarks)
                        {
                            cancellationToken.ThrowIfCancellationRequested();

                            IngestionDocumentElement? quarkElement = _Converter.Convert(quark, hierarchyMap);

                            if (quarkElement != null)
                            {
                                quarkElement.Metadata[AtomMetadataKeys.AtomParentAtomGuid] = atom.GUID.ToString();
                                quarkElement.Metadata[AtomMetadataKeys.AtomQuarkIndex] = quarkIndex;
                                quarkElement.Metadata[AtomMetadataKeys.AtomTotalQuarks] = atom.Quarks.Count;
                                quarkElement.Metadata[AtomMetadataKeys.AtomDocumentGuid] = document.Id;

                                document.Elements.Add(quarkElement);
                                quarkIndex++;
                            }
                        }
                    }
                }
            }

            // Sort sections by page number
            document.Sections = document.Sections.OrderBy(s => s.PageNumber ?? 0).ToList();

            Log($"Created {document.Elements.Count} elements in {document.Sections.Count} sections");

            return document;
        }

        private void Log(string message)
        {
            Logger?.Invoke($"[AtomDocumentReader] {message}");
        }

        #endregion
    }
}
