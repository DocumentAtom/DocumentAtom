namespace DocumentAtom.DataIngestion.Readers
{
    using System;
    using DocumentAtom.Core;
    using DocumentAtom.Csv;
    using DocumentAtom.Excel;
    using DocumentAtom.Html;
    using DocumentAtom.Image;
    using DocumentAtom.Json;
    using DocumentAtom.Markdown;
    using DocumentAtom.Pdf;
    using DocumentAtom.PowerPoint;
    using DocumentAtom.RichText;
    using DocumentAtom.Text;
    using DocumentAtom.TypeDetection;
    using DocumentAtom.Word;
    using DocumentAtom.Xml;

    /// <summary>
    /// Default implementation of processor factory.
    /// </summary>
    public class DefaultProcessorFactory : IProcessorFactory
    {
        #region Public-Members

        #endregion

        #region Private-Members

        private readonly AtomDocumentReaderOptions _Options;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Default implementation of processor factory.
        /// </summary>
        /// <param name="options">Reader options.</param>
        public DefaultProcessorFactory(AtomDocumentReaderOptions options)
        {
            _Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Create a processor for the specified document type.
        /// </summary>
        /// <param name="documentType">The document type.</param>
        /// <param name="path">Optional path to the document.</param>
        /// <returns>A processor instance, or null if the type is not supported.</returns>
        public ProcessorBase? CreateProcessor(DocumentTypeEnum documentType, string? path = null)
        {
            _Options.ProcessorSettings.TryGetValue(documentType, out ProcessorSettingsBase? customSettings);

            return documentType switch
            {
                DocumentTypeEnum.Pdf => CreatePdfProcessor(customSettings),
                DocumentTypeEnum.Docx => CreateDocxProcessor(customSettings),
                DocumentTypeEnum.Xlsx => CreateXlsxProcessor(customSettings),
                DocumentTypeEnum.Pptx => CreatePptxProcessor(customSettings),
                DocumentTypeEnum.Html => CreateHtmlProcessor(customSettings),
                DocumentTypeEnum.Markdown => CreateMarkdownProcessor(customSettings),
                DocumentTypeEnum.Json => CreateJsonProcessor(customSettings),
                DocumentTypeEnum.Csv => CreateCsvProcessor(customSettings),
                DocumentTypeEnum.Xml => CreateXmlProcessor(customSettings),
                DocumentTypeEnum.Rtf => CreateRtfProcessor(customSettings),
                DocumentTypeEnum.Png or DocumentTypeEnum.Jpeg or
                DocumentTypeEnum.Gif or DocumentTypeEnum.Tiff or
                DocumentTypeEnum.Bmp or DocumentTypeEnum.WebP => CreateImageProcessor(customSettings),
                DocumentTypeEnum.Text or DocumentTypeEnum.Unknown => CreateTextProcessor(customSettings),
                _ => null
            };
        }

        #endregion

        #region Private-Methods

        private PdfProcessor CreatePdfProcessor(ProcessorSettingsBase? custom)
        {
            PdfProcessorSettings settings = custom as PdfProcessorSettings ?? new PdfProcessorSettings();
            ApplyCommonSettings(settings);
            return new PdfProcessor(settings);
        }

        private DocxProcessor CreateDocxProcessor(ProcessorSettingsBase? custom)
        {
            DocxProcessorSettings settings = custom as DocxProcessorSettings ?? new DocxProcessorSettings();
            ApplyCommonSettings(settings);
            settings.BuildHierarchy = _Options.BuildHierarchy;
            return new DocxProcessor(settings);
        }

        private XlsxProcessor CreateXlsxProcessor(ProcessorSettingsBase? custom)
        {
            XlsxProcessorSettings settings = custom as XlsxProcessorSettings ?? new XlsxProcessorSettings();
            ApplyCommonSettings(settings);
            return new XlsxProcessor(settings);
        }

        private PptxProcessor CreatePptxProcessor(ProcessorSettingsBase? custom)
        {
            PptxProcessorSettings settings = custom as PptxProcessorSettings ?? new PptxProcessorSettings();
            ApplyCommonSettings(settings);
            return new PptxProcessor(settings);
        }

        private HtmlProcessor CreateHtmlProcessor(ProcessorSettingsBase? custom)
        {
            HtmlProcessorSettings settings = new HtmlProcessorSettings();
            settings.BuildHierarchy = _Options.BuildHierarchy;
            return new HtmlProcessor(settings);
        }

        private MarkdownProcessor CreateMarkdownProcessor(ProcessorSettingsBase? custom)
        {
            MarkdownProcessorSettings settings = custom as MarkdownProcessorSettings ?? new MarkdownProcessorSettings();
            ApplyCommonSettings(settings);
            settings.BuildHierarchy = _Options.BuildHierarchy;
            return new MarkdownProcessor(settings);
        }

        private JsonProcessor CreateJsonProcessor(ProcessorSettingsBase? custom)
        {
            JsonProcessorSettings settings = custom as JsonProcessorSettings ?? new JsonProcessorSettings();
            ApplyCommonSettings(settings);
            return new JsonProcessor(settings);
        }

        private CsvProcessor CreateCsvProcessor(ProcessorSettingsBase? custom)
        {
            CsvProcessorSettings settings = custom as CsvProcessorSettings ?? new CsvProcessorSettings();
            ApplyCommonSettings(settings);
            return new CsvProcessor(settings);
        }

        private XmlProcessor CreateXmlProcessor(ProcessorSettingsBase? custom)
        {
            XmlProcessorSettings settings = custom as XmlProcessorSettings ?? new XmlProcessorSettings();
            ApplyCommonSettings(settings);
            return new XmlProcessor(settings);
        }

        private RtfProcessor CreateRtfProcessor(ProcessorSettingsBase? custom)
        {
            RtfProcessorSettings settings = custom as RtfProcessorSettings ?? new RtfProcessorSettings();
            ApplyCommonSettings(settings);
            return new RtfProcessor(settings);
        }

        private ImageProcessor CreateImageProcessor(ProcessorSettingsBase? custom)
        {
            ImageProcessorSettings settings = custom as ImageProcessorSettings ?? new ImageProcessorSettings();
            ApplyCommonSettings(settings);
            settings.ExtractAtomsFromImages = _Options.EnableOcr;
            return new ImageProcessor(settings);
        }

        private TextProcessor CreateTextProcessor(ProcessorSettingsBase? custom)
        {
            TextProcessorSettings settings = custom as TextProcessorSettings ?? new TextProcessorSettings();
            ApplyCommonSettings(settings);
            return new TextProcessor(settings);
        }

        private void ApplyCommonSettings(ProcessorSettingsBase settings)
        {
            settings.TempDirectory = _Options.TempDirectory;
            settings.ExtractAtomsFromImages = _Options.EnableOcr;

            if (_Options.ChunkingSettings != null)
            {
                settings.Chunking = _Options.ChunkingSettings;
            }
        }

        #endregion
    }
}
