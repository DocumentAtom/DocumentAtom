namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Word;
    using DocumentAtom.Text;
    using DocumentAtom.Text.Csv;
    using DocumentAtom.Text.Html;
    using DocumentAtom.Text.Json;
    using DocumentAtom.Text.Markdown;
    using DocumentAtom.Text.Xml;
    using WatsonWebserver.Core;

    internal sealed class AtomRequestProcessor
    {
        private readonly ServerRuntimeContext _Context;

        public AtomRequestProcessor(ServerRuntimeContext context)
        {
            _Context = context;
        }

        public AtomRouteRequest ParseRequest(HttpContextBase ctx)
        {
            if (ctx.Request.DataAsBytes == null || ctx.Request.ContentLength < 1)
                throw new InvalidOperationException("RequestBodyMissing");

            string json = Encoding.UTF8.GetString(ctx.Request.DataAsBytes);
            AtomRequest request = System.Text.Json.JsonSerializer.Deserialize<AtomRequest>(json, _Context.JsonOptions);
            if (request == null)
                throw new InvalidOperationException("DeserializationError");

            return new AtomRouteRequest
            {
                Data = request.GetDataBytes(),
                Settings = request.Settings
            };
        }

        public CsvProcessorSettings ApplyApiSettings(CsvProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.RowDelimiter != null) defaults.RowDelimiter = api.RowDelimiter;
            if (api.ColumnDelimiter.HasValue) defaults.ColumnDelimiter = api.ColumnDelimiter.Value;
            if (api.HasHeaderRow.HasValue) defaults.HasHeaderRow = api.HasHeaderRow.Value;
            if (api.RowsPerAtom.HasValue) defaults.RowsPerAtom = api.RowsPerAtom.Value;
            return defaults;
        }

        public XlsxProcessorSettings ApplyApiSettings(XlsxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.HeaderRowScoreThreshold.HasValue) defaults.HeaderRowScoreThreshold = api.HeaderRowScoreThreshold.Value;
            return defaults;
        }

        public HtmlProcessorSettings ApplyApiSettings(HtmlProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            if (api.ProcessInlineStyles.HasValue) defaults.ProcessInlineStyles = api.ProcessInlineStyles.Value;
            if (api.ProcessMetaTags.HasValue) defaults.ProcessMetaTags = api.ProcessMetaTags.Value;
            if (api.ProcessScripts.HasValue) defaults.ProcessScripts = api.ProcessScripts.Value;
            if (api.ProcessComments.HasValue) defaults.ProcessComments = api.ProcessComments.Value;
            if (api.PreserveWhitespace.HasValue) defaults.PreserveWhitespace = api.PreserveWhitespace.Value;
            if (api.MaxTextLength.HasValue) defaults.MaxTextLength = api.MaxTextLength.Value;
            if (api.ProcessSvg.HasValue) defaults.ProcessSvg = api.ProcessSvg.Value;
            if (api.ExtractDataAttributes.HasValue) defaults.ExtractDataAttributes = api.ExtractDataAttributes.Value;
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        public JsonProcessorSettings ApplyApiSettings(JsonProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.MaxDepth.HasValue) defaults.MaxDepth = api.MaxDepth.Value;
            return defaults;
        }

        public MarkdownProcessorSettings ApplyApiSettings(MarkdownProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.Delimiters != null) defaults.Delimiters = api.Delimiters;
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        public TextProcessorSettings ApplyApiSettings(TextProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.Delimiters != null) defaults.Delimiters = api.Delimiters;
            return defaults;
        }

        public XmlProcessorSettings ApplyApiSettings(XmlProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.MaxDepth.HasValue) defaults.MaxDepth = api.MaxDepth.Value;
            if (api.IncludeAttributes.HasValue) defaults.IncludeAttributes = api.IncludeAttributes.Value;
            if (api.PreserveWhitespace.HasValue) defaults.PreserveWhitespace = api.PreserveWhitespace.Value;
            return defaults;
        }

        public PdfProcessorSettings ApplyApiSettings(PdfProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            return defaults;
        }

        public DocxProcessorSettings ApplyApiSettings(DocxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        public PptxProcessorSettings ApplyApiSettings(PptxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        public RtfProcessorSettings ApplyApiSettings(RtfProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            return defaults;
        }

        public ImageProcessorSettings ApplyApiSettings(ImageProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.LineThreshold.HasValue) defaults.LineThreshold = api.LineThreshold.Value;
            if (api.ParagraphThreshold.HasValue) defaults.ParagraphThreshold = api.ParagraphThreshold.Value;
            if (api.HorizontalLineLength.HasValue) defaults.HorizontalLineLength = api.HorizontalLineLength.Value;
            if (api.VerticalLineLength.HasValue) defaults.VerticalLineLength = api.VerticalLineLength.Value;
            if (api.TableMinArea.HasValue) defaults.TableMinArea = api.TableMinArea.Value;
            if (api.ColumnAlignmentTolerance.HasValue) defaults.ColumnAlignmentTolerance = api.ColumnAlignmentTolerance.Value;
            if (api.ProximityThreshold.HasValue) defaults.ProximityThreshold = api.ProximityThreshold.Value;
            return defaults;
        }

        public ImageProcessorSettings BuildImageSettings(ApiProcessorSettings api)
        {
            ImageProcessorSettings imageSettings = new ImageProcessorSettings
            {
                TesseractDataDirectory = _Context.Settings.Tesseract.DataDirectory,
                TesseractLanguage = _Context.Settings.Tesseract.Language,
            };
            return ApplyApiSettings(imageSettings, api);
        }

        public void ApplyChunking(List<Atom> atoms, ChunkingConfiguration config)
        {
            if (config == null) return;
            if (!config.Enable) return;

            ChunkingEngine engine = new ChunkingEngine();

            foreach (Atom atom in atoms)
            {
                List<List<string>> tableData = null;
                if (atom.Table != null)
                    tableData = ChunkingEngine.SerializableDataTableToList(atom.Table);

                List<DocumentAtom.Core.Chunking.Chunk> chunks = engine.Chunk(
                    atom.Type,
                    atom.Text,
                    atom.OrderedList,
                    atom.UnorderedList,
                    tableData,
                    config);

                if (chunks != null && chunks.Count > 0)
                    atom.Chunks = chunks;
            }
        }

        private static void ApplyBaseSettings(ProcessorSettingsBase defaults, ApiProcessorSettings api)
        {
            if (api == null) return;
            if (api.TrimText.HasValue) defaults.TrimText = api.TrimText.Value;
            if (api.RemoveBinaryFromText.HasValue) defaults.RemoveBinaryFromText = api.RemoveBinaryFromText.Value;
            if (api.ExtractAtomsFromImages.HasValue) defaults.ExtractAtomsFromImages = api.ExtractAtomsFromImages.Value;
            if (api.Chunking != null) defaults.Chunking = api.Chunking;
        }
    }

    internal sealed class AtomRouteRequest
    {
        public byte[] Data { get; set; } = null;

        public ApiProcessorSettings Settings { get; set; } = null;
    }
}
