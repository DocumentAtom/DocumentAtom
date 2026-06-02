namespace DocumentAtom.Server
{
    using System.Collections.Generic;
    using WatsonWebserver;
    using WatsonWebserver.Core;
    using WatsonWebserver.Core.OpenApi;

    internal static class OpenApiRouteRegistrar
    {
        public static void ConfigureOpenApi(Webserver restServer)
        {
            restServer.UseOpenApi(openApi =>
            {
                openApi.EnableOpenApi = true;
                openApi.EnableSwaggerUi = true;
                openApi.DocumentPath = "/openapi.json";
                openApi.SwaggerUiPath = "/swagger";
                openApi.Info.Title = "DocumentAtom Server API";
                openApi.Info.Version = "3.0.0";
                openApi.Info.Description =
                    "DocumentAtom exposes document type detection, OCR extraction, and atomization routes. " +
                    "Atomization routes accept a JSON envelope containing base64 document data and optional processor settings. " +
                    "Type detection accepts raw bytes. Responses are JSON except for the HTML homepage, favicon, and HEAD/OPTIONS responses.";
                openApi.Tags.Add(new OpenApiTag { Name = "System", Description = "Health, homepage, and static server assets." });
                openApi.Tags.Add(new OpenApiTag { Name = "Type Detection", Description = "Detect MIME type, extension, and DocumentAtom document type from raw bytes." });
                openApi.Tags.Add(new OpenApiTag { Name = "Atomization", Description = "Extract structured atoms from text, markup, office documents, PDFs, and image data." });
                RegisterSchemas(openApi.Schemas);
            });
        }

        public static void RegisterRoutes(
            Webserver restServer,
            ServerRouteHandlers routes,
            TextAtomRouteHandlers textRoutes,
            DocumentAtomRouteHandlers documentRoutes,
            ImageAtomRouteHandlers imageRoutes)
        {
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.HEAD, "/", routes.LoopbackRoute, routes.ExceptionRoute, openApiMetadata: HeadRootMetadata());
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/", routes.RootRoute, routes.ExceptionRoute, openApiMetadata: RootMetadata());
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.HEAD, "/favicon.ico", routes.HeadFavicon, routes.ExceptionRoute, openApiMetadata: HeadFaviconMetadata());
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/favicon.ico", routes.GetFavicon, routes.ExceptionRoute, openApiMetadata: GetFaviconMetadata());

            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/typedetect", imageRoutes.TypeDetectionRoute, routes.ExceptionRoute, openApiMetadata: TypeDetectionMetadata());
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/csv", textRoutes.CsvAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("csvAtom", "Extract CSV atoms", "Extract tabular atoms from base64-encoded CSV content.", "Use RowDelimiter, ColumnDelimiter, HasHeaderRow, and RowsPerAtom for CSV-specific behavior."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/excel", documentRoutes.ExcelAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("excelAtom", "Extract Excel atoms", "Extract worksheet, row, cell, and table-oriented atoms from base64-encoded XLSX workbook content.", "Use BuildHierarchy and HeaderRowScoreThreshold for workbook structure. Set ExtractAtomsFromImages to run OCR over embedded images."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/html", textRoutes.HtmlAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("htmlAtom", "Extract HTML atoms", "Extract atoms from base64-encoded HTML markup.", "Use ProcessInlineStyles, ProcessMetaTags, ProcessScripts, ProcessComments, PreserveWhitespace, MaxTextLength, ProcessSvg, ExtractDataAttributes, and BuildHierarchy for HTML-specific behavior."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/json", textRoutes.JsonAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("jsonAtom", "Extract JSON atoms", "Extract atoms from base64-encoded JSON content.", "Use BuildHierarchy and MaxDepth for nested JSON traversal."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/markdown", textRoutes.MarkdownAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("markdownAtom", "Extract Markdown atoms", "Extract atoms from base64-encoded Markdown content.", "Use Delimiters and BuildHierarchy to influence heading, list, table, and paragraph structure."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/ocr", imageRoutes.OcrAtomRoute, routes.ExceptionRoute, openApiMetadata: OcrMetadata());
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/pdf", documentRoutes.PdfAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("pdfAtom", "Extract PDF atoms", "Extract atoms from base64-encoded PDF data.", "Set ExtractAtomsFromImages to run OCR over images discovered while processing PDF pages."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/png", imageRoutes.PngAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("pngAtom", "Extract PNG atoms", "Extract atoms from base64-encoded PNG image data using OCR-backed image analysis.", "Use LineThreshold, ParagraphThreshold, HorizontalLineLength, VerticalLineLength, TableMinArea, ColumnAlignmentTolerance, and ProximityThreshold for image segmentation."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/powerpoint", documentRoutes.PowerPointAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("powerPointAtom", "Extract PowerPoint atoms", "Extract atoms from base64-encoded PPTX presentation content.", "Use BuildHierarchy for slide structure. Set ExtractAtomsFromImages to run OCR over embedded images."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/rtf", documentRoutes.RtfAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("rtfAtom", "Extract RTF atoms", "Extract atoms from base64-encoded RTF document content.", "Set ExtractAtomsFromImages to run OCR over images discovered while processing the document."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/text", textRoutes.TextAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("textAtom", "Extract text atoms", "Extract atoms from base64-encoded plain text data.", "Use Delimiters to split content and Chunking to attach chunk output to returned atoms."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/word", documentRoutes.WordAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("wordAtom", "Extract Word atoms", "Extract atoms from base64-encoded DOCX document content.", "Use BuildHierarchy for document structure. Set ExtractAtomsFromImages to run OCR over embedded images."));
            restServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/xml", textRoutes.XmlAtomRoute, routes.ExceptionRoute, openApiMetadata: AtomMetadata("xmlAtom", "Extract XML atoms", "Extract atoms from base64-encoded XML content.", "Use BuildHierarchy, MaxDepth, IncludeAttributes, and PreserveWhitespace for XML traversal."));
        }

        private static OpenApiRouteMetadata HeadRootMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Check server availability", "System")
                .WithDescription("Returns headers only. Use this lightweight route for simple server reachability checks.")
                .WithResponse(200, OpenApiResponseMetadata.Create("Server is reachable. No response body is returned."));
            metadata.OperationId = "headRoot";
            return metadata;
        }

        private static OpenApiRouteMetadata RootMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Get server homepage", "System")
                .WithDescription("Returns the built-in HTML homepage for the server.")
                .WithResponse(200, new OpenApiResponseMetadata
                {
                    Description = "HTML homepage.",
                    Content = new Dictionary<string, OpenApiMediaTypeMetadata>
                    {
                        [Constants.HtmlContentType] = new OpenApiMediaTypeMetadata { Schema = OpenApiSchemaMetadata.String() }
                    }
                });
            metadata.OperationId = "getRoot";
            return metadata;
        }

        private static OpenApiRouteMetadata HeadFaviconMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Check favicon availability", "System")
                .WithDescription("Returns favicon headers only. No response body is returned.")
                .WithResponse(200, OpenApiResponseMetadata.Create("Favicon is available."));
            metadata.OperationId = "headFavicon";
            return metadata;
        }

        private static OpenApiRouteMetadata GetFaviconMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Get favicon", "System")
                .WithDescription("Returns the server favicon.")
                .WithResponse(200, OpenApiResponseMetadata.Binary("Favicon image bytes.", Constants.FaviconContentType));
            metadata.OperationId = "getFavicon";
            return metadata;
        }

        private static OpenApiRouteMetadata AtomMetadata(string operationId, string summary, string description, string processorNotes)
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create(summary, "Atomization")
                .WithDescription(
                    description + " The request body must be application/json with Data containing base64-encoded bytes. " +
                    "Settings is optional and only supplied properties override server defaults. " +
                    processorNotes + " Chunking can be enabled on routes that return atoms; when enabled, returned atoms include Chunks.")
                .WithRequestBody(JsonRequestBody("Atom extraction request. Data is required; Settings is optional.", OpenApiSchemaMetadata.CreateRef("AtomRequest"), AtomRequestExamples()))
                .WithResponse(200, JsonResponse("Extracted atoms. The array may be empty when the input contains no extractable content.", OpenApiSchemaMetadata.CreateArray(OpenApiSchemaMetadata.CreateRef("Atom")), AtomArrayExamples()))
                .WithResponse(400, JsonResponse("Bad request. Returned when the request body is missing, Data is not valid base64, JSON deserialization fails, or the processor rejects malformed input.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithResponse(500, JsonResponse("Internal server error. Returned when processing fails unexpectedly.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithDefaultResponse(JsonResponse("Unexpected error response.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()));
            metadata.OperationId = operationId;
            return metadata;
        }

        private static OpenApiRouteMetadata OcrMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Extract OCR content", "Atomization")
                .WithDescription(
                    "Extract OCR text elements, detected tables, and detected lists from base64-encoded image data. " +
                    "The route uses the server-configured Tesseract data directory and language. " +
                    "Image segmentation can be tuned with LineThreshold, ParagraphThreshold, HorizontalLineLength, VerticalLineLength, TableMinArea, ColumnAlignmentTolerance, and ProximityThreshold.")
                .WithRequestBody(JsonRequestBody("OCR extraction request. Data is required; Settings can include OCR tuning fields.", OpenApiSchemaMetadata.CreateRef("AtomRequest"), AtomRequestExamples()))
                .WithResponse(200, JsonResponse("OCR extraction result with text elements, tables, and lists.", OpenApiSchemaMetadata.CreateRef("ExtractionResult"), OcrResultExamples()))
                .WithResponse(400, JsonResponse("Bad request. Returned when the request body is missing, Data is not valid base64, JSON deserialization fails, or image data cannot be processed.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithResponse(500, JsonResponse("Internal server error. Returned when OCR processing fails unexpectedly.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithDefaultResponse(JsonResponse("Unexpected error response.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()));
            metadata.OperationId = "ocrAtom";
            return metadata;
        }

        private static OpenApiRouteMetadata TypeDetectionMetadata()
        {
            OpenApiRouteMetadata metadata = OpenApiRouteMetadata.Create("Detect document type", "Type Detection")
                .WithDescription(
                    "Detect the MIME type, recommended extension, and DocumentAtom document type for the raw request body. " +
                    "Unlike atomization routes, this route accepts raw bytes directly rather than an AtomRequest JSON envelope. " +
                    "When Content-Type is absent, application/octet-stream is assumed.")
                .WithRequestBody(BinaryRequestBody("Raw document bytes to inspect.", Constants.BinaryContentType, true))
                .WithResponse(200, JsonResponse("Detected type information.", OpenApiSchemaMetadata.CreateRef("TypeResult"), TypeResultExamples()))
                .WithResponse(400, JsonResponse("Bad request. Returned when the request body is missing or cannot be inspected.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithResponse(500, JsonResponse("Internal server error. Returned when type detection fails unexpectedly.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()))
                .WithDefaultResponse(JsonResponse("Unexpected error response.", OpenApiSchemaMetadata.CreateRef("ApiErrorResponse"), ErrorExamples()));
            metadata.OperationId = "detectType";
            return metadata;
        }

        private static void RegisterSchemas(Dictionary<string, OpenApiSchemaMetadata> schemas)
        {
            schemas["AtomRequest"] = ObjectSchema(
                "Request envelope used by all /atom/* routes. Data is base64-encoded file or document content. Settings is optional and contains route-specific overrides.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Settings"] = RefSchema("ApiProcessorSettings", "Optional processor overrides. Omit this property to use server defaults.", true),
                    ["Data"] = StringSchema("byte", "Base64-encoded document data. This property is required and must decode to the input bytes for the selected route.", false, null, null, "SGVsbG8gd29ybGQ=")
                },
                "Data");

            schemas["ApiProcessorSettings"] = ObjectSchema(
                "Optional flat processor settings. Only supplied properties override defaults. Some fields only apply to specific routes; unsupported fields are ignored by other routes.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["TrimText"] = BooleanSchema("Trim leading and trailing whitespace from extracted text where supported.", true, null),
                    ["RemoveBinaryFromText"] = BooleanSchema("Remove binary/control data from text inputs where supported.", true, null),
                    ["ExtractAtomsFromImages"] = BooleanSchema("For document routes that can contain images, run OCR over embedded images and include extracted atoms.", true, null),
                    ["Chunking"] = RefSchema("ChunkingConfiguration", "Optional chunking configuration applied after atom extraction.", true),
                    ["Delimiters"] = ArraySchema(StringSchema(null, "Text or Markdown delimiters used to split content into atoms.", false, null, null, "\\n\\n"), "Delimiters for text and Markdown routes.", true),
                    ["BuildHierarchy"] = BooleanSchema("Build structural hierarchy where the processor supports headings, worksheet structure, or nested markup/data.", true, null),
                    ["RowDelimiter"] = StringSchema(null, "CSV row delimiter. Common values are \\n and \\r\\n.", true, 1, null, "\\n"),
                    ["ColumnDelimiter"] = StringSchema(null, "CSV column delimiter represented as a one-character string.", true, 1, 1, ","),
                    ["HasHeaderRow"] = BooleanSchema("Treat the first CSV row as column headers.", true, null),
                    ["RowsPerAtom"] = IntegerSchema("Rows included in each CSV atom.", true, 1, null, 1),
                    ["ProcessInlineStyles"] = BooleanSchema("HTML route: include inline style information when extracting atoms.", true, null),
                    ["ProcessMetaTags"] = BooleanSchema("HTML route: emit metadata atoms from meta tags.", true, null),
                    ["ProcessScripts"] = BooleanSchema("HTML route: include script content when extracting atoms.", true, null),
                    ["ProcessComments"] = BooleanSchema("HTML route: include HTML comments when extracting atoms.", true, null),
                    ["PreserveWhitespace"] = BooleanSchema("HTML/XML routes: preserve input whitespace instead of normalizing extracted text.", true, null),
                    ["MaxTextLength"] = IntegerSchema("HTML route: maximum text length to include per extracted text segment.", true, 1, null, null),
                    ["ProcessSvg"] = BooleanSchema("HTML route: process SVG elements when present.", true, null),
                    ["ExtractDataAttributes"] = BooleanSchema("HTML route: extract data-* attributes into atom metadata where supported.", true, null),
                    ["MaxDepth"] = IntegerSchema("JSON/XML routes: maximum nesting depth to traverse.", true, 1, null, null),
                    ["IncludeAttributes"] = BooleanSchema("XML route: include XML attributes in extracted atoms.", true, null),
                    ["HeaderRowScoreThreshold"] = IntegerSchema("Excel route: threshold used when detecting likely header rows.", true, 0, null, null),
                    ["LineThreshold"] = IntegerSchema("Image/OCR routes: pixel distance threshold for grouping OCR text into lines.", true, 0, null, 5),
                    ["ParagraphThreshold"] = IntegerSchema("Image/OCR routes: pixel distance threshold for grouping lines into paragraphs.", true, 0, null, 30),
                    ["HorizontalLineLength"] = IntegerSchema("Image/OCR routes: minimum horizontal line length used during table detection.", true, 1, null, 80),
                    ["VerticalLineLength"] = IntegerSchema("Image/OCR routes: minimum vertical line length used during table detection.", true, 1, null, 40),
                    ["TableMinArea"] = IntegerSchema("Image/OCR routes: minimum detected table area in pixels.", true, 1, null, 5000),
                    ["ColumnAlignmentTolerance"] = IntegerSchema("Image/OCR routes: allowed horizontal variance when aligning OCR text into columns.", true, 0, null, 10),
                    ["ProximityThreshold"] = IntegerSchema("Image/OCR routes: proximity threshold for associating OCR text elements.", true, 0, null, 20)
                });

            schemas["ChunkingConfiguration"] = ObjectSchema(
                "Optional chunking configuration. When Enable is true, each returned atom may include Chunks derived from its text, list, or table content.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Enable"] = BooleanSchema("Enable chunk generation.", false, false),
                    ["Strategy"] = EnumSchema("Chunking strategy.", ChunkStrategyValues(), false, "FixedTokenCount"),
                    ["FixedTokenCount"] = IntegerSchema("Token budget per chunk. Also used by sentence and paragraph strategies.", false, 1, null, 256),
                    ["OverlapCount"] = IntegerSchema("Number of tokens, sentences, or paragraphs to overlap between adjacent chunks, depending on Strategy.", false, 0, null, 0),
                    ["OverlapPercentage"] = NumberSchema("double", "Alternative overlap as a percentage from 0.0 through 1.0. When supplied, this takes precedence over OverlapCount.", true, 0, 1, null),
                    ["OverlapStrategy"] = EnumSchema("Strategy for handling chunk overlap boundaries.", OverlapStrategyValues(), false, "SlidingWindow"),
                    ["RowGroupSize"] = IntegerSchema("Rows per group when Strategy is RowGroupWithHeaders.", false, 1, null, 5),
                    ["ContextPrefix"] = StringSchema(null, "Optional text prepended to each generated chunk.", true, null, null, null),
                    ["RegexPattern"] = StringSchema(null, "Required when Strategy is RegexBased. Regular expression used as the split delimiter.", true, 1, null, "\\n{2,}")
                });

            schemas["Atom"] = ObjectSchema(
                "A self-contained unit of document content. Fields are populated according to atom type and source processor.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["ParentGUID"] = StringSchema("uuid", "GUID of the parent atom when this atom is nested under another atom.", true, null, null, null),
                    ["GUID"] = StringSchema("uuid", "Unique atom identifier.", false, null, null, "8f77d7a4-9046-4eb5-a61f-3e6ebdf90a21"),
                    ["Type"] = EnumSchema("Atom type.", AtomTypeValues(), false, "Text"),
                    ["SheetName"] = StringSchema(null, "Excel worksheet name when the atom came from a workbook.", true, null, null, "Sheet1"),
                    ["CellIdentifier"] = StringSchema(null, "Excel cell reference when applicable.", true, null, null, "A1"),
                    ["PageNumber"] = IntegerSchema("Page number for page-oriented formats such as PDF.", true, 0, null, 1),
                    ["Position"] = IntegerSchema("Ordinal position within the source document or parent collection.", true, 0, null, 0),
                    ["Length"] = IntegerSchema("Length of the atom content.", false, 0, null, 11),
                    ["Rows"] = IntegerSchema("Number of rows when the atom represents a table.", true, 0, null, 2),
                    ["Columns"] = IntegerSchema("Number of columns when the atom represents a table.", true, 0, null, 3),
                    ["Title"] = StringSchema(null, "Optional atom title.", true, null, null, "Quarterly Results"),
                    ["Subtitle"] = StringSchema(null, "Optional atom subtitle.", true, null, null, "North America"),
                    ["MD5Hash"] = StringSchema("byte", "Base64-encoded MD5 hash of atom content when computed.", true, null, null, null),
                    ["SHA1Hash"] = StringSchema("byte", "Base64-encoded SHA1 hash of atom content when computed.", true, null, null, null),
                    ["SHA256Hash"] = StringSchema("byte", "Base64-encoded SHA256 hash of atom content when computed.", true, null, null, null),
                    ["HeaderLevel"] = IntegerSchema("Markdown or hierarchy header level. Minimum value is 1 when present.", true, 1, null, 1),
                    ["Formatting"] = EnumSchema("Markdown formatting classification when produced by Markdown processing.", MarkdownFormattingValues(), true, null),
                    ["BoundingBox"] = RefSchema("BoundingBox", "Bounding box for OCR/image-derived atoms.", true),
                    ["Text"] = StringSchema(null, "Extracted text content.", true, null, null, "Hello world"),
                    ["UnorderedList"] = ArraySchema(OpenApiSchemaMetadata.String(), "Unordered list items.", true),
                    ["OrderedList"] = ArraySchema(OpenApiSchemaMetadata.String(), "Ordered list items.", true),
                    ["Table"] = RefSchema("SerializableDataTable", "Serializable table payload when the atom represents tabular content.", true),
                    ["Binary"] = StringSchema("byte", "Base64-encoded binary payload when retained by the processor.", true, null, null, null),
                    ["Quarks"] = ArraySchema(OpenApiSchemaMetadata.CreateRef("Atom"), "Structural child atoms produced during hierarchy extraction.", true),
                    ["Chunks"] = ArraySchema(OpenApiSchemaMetadata.CreateRef("Chunk"), "Content chunks generated when request Settings.Chunking.Enable is true.", true)
                });

            schemas["Chunk"] = ObjectSchema(
                "A text fragment generated from an atom by a chunking strategy.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Position"] = IntegerSchema("Ordinal chunk index within the parent atom.", false, 0, null, 0),
                    ["Length"] = IntegerSchema("Chunk text length.", false, 0, null, 128),
                    ["MD5Hash"] = StringSchema("byte", "Base64-encoded MD5 hash of the chunk text.", true, null, null, null),
                    ["SHA1Hash"] = StringSchema("byte", "Base64-encoded SHA1 hash of the chunk text.", true, null, null, null),
                    ["SHA256Hash"] = StringSchema("byte", "Base64-encoded SHA256 hash of the chunk text.", true, null, null, null),
                    ["Text"] = StringSchema(null, "Chunk text.", true, null, null, "Chunk text")
                });

            schemas["ExtractionResult"] = ObjectSchema(
                "OCR extraction result returned by /atom/ocr.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["TextElements"] = ArraySchema(OpenApiSchemaMetadata.CreateRef("OcrTextElement"), "OCR text elements with confidence and bounding rectangle.", false),
                    ["Tables"] = ArraySchema(OpenApiSchemaMetadata.CreateRef("OcrTableStructure"), "Detected table structures.", false),
                    ["Lists"] = ArraySchema(OpenApiSchemaMetadata.CreateRef("OcrListStructure"), "Detected ordered and unordered lists.", false)
                });

            schemas["OcrTextElement"] = ObjectSchema(
                "Single OCR text element.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Text"] = StringSchema(null, "Recognized text.", true, null, null, "Invoice"),
                    ["Bounds"] = RefSchema("Rectangle", "Pixel bounds for the recognized text.", false),
                    ["Confidence"] = NumberSchema("float", "OCR confidence score from the recognition engine.", false, 0, 100, 96.4)
                });

            schemas["OcrTableStructure"] = ObjectSchema(
                "Detected table structure from OCR/image analysis.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Cells"] = ArraySchema(OpenApiSchemaMetadata.CreateArray(OpenApiSchemaMetadata.String()), "Rows of cell text.", false),
                    ["Bounds"] = RefSchema("Rectangle", "Pixel bounds for the detected table.", false),
                    ["Rows"] = IntegerSchema("Detected row count.", false, 0, null, 2),
                    ["Columns"] = IntegerSchema("Detected column count.", false, 0, null, 3)
                });

            schemas["OcrListStructure"] = ObjectSchema(
                "Detected list structure from OCR/image analysis.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Items"] = ArraySchema(OpenApiSchemaMetadata.String(), "Detected list item text.", false),
                    ["IsOrdered"] = BooleanSchema("True when the list appears ordered or numbered.", false, false),
                    ["Bounds"] = RefSchema("Rectangle", "Pixel bounds for the detected list.", false)
                });

            schemas["Rectangle"] = ObjectSchema(
                "System.Drawing.Rectangle-like pixel rectangle.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["X"] = IntegerSchema("Left coordinate in pixels.", false, null, null, 12),
                    ["Y"] = IntegerSchema("Top coordinate in pixels.", false, null, null, 30),
                    ["Width"] = IntegerSchema("Width in pixels.", false, 0, null, 240),
                    ["Height"] = IntegerSchema("Height in pixels.", false, 0, null, 32)
                });

            schemas["BoundingBox"] = ObjectSchema(
                "Bounding box for atom content located in an image or rendered document.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["UpperLeft"] = RefSchema("Point", "Upper-left point.", false),
                    ["UpperRight"] = RefSchema("Point", "Upper-right point.", false),
                    ["LowerRight"] = RefSchema("Point", "Lower-right point.", false),
                    ["LowerLeft"] = RefSchema("Point", "Lower-left point.", false),
                    ["Width"] = IntegerSchema("Computed width.", false, 0, null, null),
                    ["Height"] = IntegerSchema("Computed height.", false, 0, null, null)
                });

            schemas["Point"] = ObjectSchema(
                "2D point.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["X"] = IntegerSchema("X coordinate.", false, null, null, 0),
                    ["Y"] = IntegerSchema("Y coordinate.", false, null, null, 0)
                });

            schemas["SerializableDataTable"] = ObjectSchema(
                "SerializableDataTable payload used for table atoms. The concrete column and row shape depends on the source document.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Columns"] = ArraySchema(ObjectSchema("Serializable column metadata."), "Column definitions.", true),
                    ["Rows"] = ArraySchema(ObjectSchema("Serializable row data keyed by column."), "Row values.", true)
                });

            schemas["TypeResult"] = ObjectSchema(
                "Document type detection result.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["MimeType"] = StringSchema(null, "Detected MIME type.", true, null, null, "application/pdf"),
                    ["Extension"] = StringSchema(null, "Recommended extension without leading dot.", true, null, null, "pdf"),
                    ["Type"] = EnumSchema("Detected DocumentAtom document type.", DocumentTypeValues(), false, "Pdf")
                });

            schemas["ApiErrorResponse"] = ObjectSchema(
                "Standard API error response.",
                new Dictionary<string, OpenApiSchemaMetadata>
                {
                    ["Error"] = EnumSchema("Machine-readable error code.", ApiErrorValues(), false, "BadRequest"),
                    ["Message"] = StringSchema(null, "Stable human-readable message derived from Error.", false, null, null, "We were unable to discern your request.  Please check your URL, query, and request body."),
                    ["StatusCode"] = IntegerSchema("HTTP status code corresponding to Error.", false, 100, 599, 400),
                    ["Context"] = ObjectSchema("Optional contextual error details. Shape depends on the error source."),
                    ["Description"] = StringSchema(null, "Additional detail from the exception or request validator.", true, null, null, "No request body found.")
                });
        }

        private static OpenApiRequestBodyMetadata JsonRequestBody(string description, OpenApiSchemaMetadata schema, Dictionary<string, OpenApiExampleMetadata> examples)
        {
            return new OpenApiRequestBodyMetadata
            {
                Description = description,
                Required = true,
                Content = new Dictionary<string, OpenApiMediaTypeMetadata>
                {
                    [Constants.JsonContentType] = new OpenApiMediaTypeMetadata
                    {
                        Schema = schema,
                        Examples = examples
                    }
                }
            };
        }

        private static OpenApiRequestBodyMetadata BinaryRequestBody(string description, string contentType, bool required)
        {
            return new OpenApiRequestBodyMetadata
            {
                Description = description,
                Required = required,
                Content = new Dictionary<string, OpenApiMediaTypeMetadata>
                {
                    [contentType] = new OpenApiMediaTypeMetadata
                    {
                        Schema = new OpenApiSchemaMetadata
                        {
                            Type = "string",
                            Format = "binary",
                            Description = "Raw bytes for the document or file to inspect."
                        },
                        Examples = new Dictionary<string, OpenApiExampleMetadata>
                        {
                            ["pdf"] = new OpenApiExampleMetadata { Summary = "PDF bytes", Description = "Send the PDF file bytes directly as the request body." }
                        }
                    }
                }
            };
        }

        private static OpenApiResponseMetadata JsonResponse(string description, OpenApiSchemaMetadata schema, Dictionary<string, OpenApiExampleMetadata> examples)
        {
            return new OpenApiResponseMetadata
            {
                Description = description,
                Content = new Dictionary<string, OpenApiMediaTypeMetadata>
                {
                    [Constants.JsonContentType] = new OpenApiMediaTypeMetadata
                    {
                        Schema = schema,
                        Examples = examples
                    }
                }
            };
        }

        private static OpenApiSchemaMetadata ObjectSchema(string description)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "object",
                Description = description,
                Properties = new Dictionary<string, OpenApiSchemaMetadata>()
            };
        }

        private static OpenApiSchemaMetadata ObjectSchema(
            string description,
            Dictionary<string, OpenApiSchemaMetadata> properties,
            params string[] required)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "object",
                Description = description,
                Properties = properties,
                Required = new List<string>(required)
            };
        }

        private static OpenApiSchemaMetadata RefSchema(string schemaName, string description, bool nullable)
        {
            OpenApiSchemaMetadata schema = OpenApiSchemaMetadata.CreateRef(schemaName);
            schema.Description = description;
            schema.Nullable = nullable;
            return schema;
        }

        private static OpenApiSchemaMetadata StringSchema(string format, string description, bool nullable, int? minimumLength, int? maximumLength, object example)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "string",
                Format = format,
                Description = description,
                Nullable = nullable,
                MinLength = minimumLength,
                MaxLength = maximumLength,
                Example = example
            };
        }

        private static OpenApiSchemaMetadata IntegerSchema(string description, bool nullable, double? minimum, double? maximum, object example)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "integer",
                Format = "int32",
                Description = description,
                Nullable = nullable,
                Minimum = minimum,
                Maximum = maximum,
                Example = example
            };
        }

        private static OpenApiSchemaMetadata NumberSchema(string format, string description, bool nullable, double? minimum, double? maximum, object example)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "number",
                Format = format,
                Description = description,
                Nullable = nullable,
                Minimum = minimum,
                Maximum = maximum,
                Example = example
            };
        }

        private static OpenApiSchemaMetadata BooleanSchema(string description, bool nullable, object defaultValue)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "boolean",
                Description = description,
                Nullable = nullable,
                Default = defaultValue
            };
        }

        private static OpenApiSchemaMetadata ArraySchema(OpenApiSchemaMetadata itemSchema, string description, bool nullable)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "array",
                Items = itemSchema,
                Description = description,
                Nullable = nullable
            };
        }

        private static OpenApiSchemaMetadata EnumSchema(string description, List<object> values, bool nullable, object defaultValue)
        {
            return new OpenApiSchemaMetadata
            {
                Type = "string",
                Description = description,
                Nullable = nullable,
                Enum = values,
                Default = defaultValue
            };
        }

        private static Dictionary<string, OpenApiExampleMetadata> AtomRequestExamples()
        {
            return new Dictionary<string, OpenApiExampleMetadata>
            {
                ["plainText"] = new OpenApiExampleMetadata
                {
                    Summary = "Plain text with chunking",
                    Description = "Base64 for 'Hello world' with fixed-token chunking enabled.",
                    Value = new Dictionary<string, object>
                    {
                        ["Data"] = "SGVsbG8gd29ybGQ=",
                        ["Settings"] = new Dictionary<string, object>
                        {
                            ["TrimText"] = true,
                            ["Chunking"] = new Dictionary<string, object>
                            {
                                ["Enable"] = true,
                                ["Strategy"] = "FixedTokenCount",
                                ["FixedTokenCount"] = 128,
                                ["OverlapCount"] = 16
                            }
                        }
                    }
                },
                ["csv"] = new OpenApiExampleMetadata
                {
                    Summary = "CSV settings",
                    Description = "CSV request using comma delimiters and header row detection.",
                    Value = new Dictionary<string, object>
                    {
                        ["Data"] = "TmFtZSxWYWx1ZQpBbHBoYSwx",
                        ["Settings"] = new Dictionary<string, object>
                        {
                            ["ColumnDelimiter"] = ",",
                            ["RowDelimiter"] = "\\n",
                            ["HasHeaderRow"] = true,
                            ["RowsPerAtom"] = 1
                        }
                    }
                }
            };
        }

        private static Dictionary<string, OpenApiExampleMetadata> AtomArrayExamples()
        {
            return new Dictionary<string, OpenApiExampleMetadata>
            {
                ["textAtom"] = new OpenApiExampleMetadata
                {
                    Summary = "Single text atom",
                    Value = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["GUID"] = "8f77d7a4-9046-4eb5-a61f-3e6ebdf90a21",
                            ["Type"] = "Text",
                            ["Position"] = 0,
                            ["Length"] = 11,
                            ["Text"] = "Hello world"
                        }
                    }
                }
            };
        }

        private static Dictionary<string, OpenApiExampleMetadata> OcrResultExamples()
        {
            return new Dictionary<string, OpenApiExampleMetadata>
            {
                ["simpleOcr"] = new OpenApiExampleMetadata
                {
                    Summary = "OCR text element",
                    Value = new Dictionary<string, object>
                    {
                        ["TextElements"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["Text"] = "Invoice",
                                ["Confidence"] = 96.4,
                                ["Bounds"] = new Dictionary<string, object>
                                {
                                    ["X"] = 12,
                                    ["Y"] = 30,
                                    ["Width"] = 240,
                                    ["Height"] = 32
                                }
                            }
                        },
                        ["Tables"] = new object[] { },
                        ["Lists"] = new object[] { }
                    }
                }
            };
        }

        private static Dictionary<string, OpenApiExampleMetadata> TypeResultExamples()
        {
            return new Dictionary<string, OpenApiExampleMetadata>
            {
                ["pdf"] = new OpenApiExampleMetadata
                {
                    Summary = "PDF detection",
                    Value = new Dictionary<string, object>
                    {
                        ["MimeType"] = "application/pdf",
                        ["Extension"] = "pdf",
                        ["Type"] = "Pdf"
                    }
                }
            };
        }

        private static Dictionary<string, OpenApiExampleMetadata> ErrorExamples()
        {
            return new Dictionary<string, OpenApiExampleMetadata>
            {
                ["requestBodyMissing"] = new OpenApiExampleMetadata
                {
                    Summary = "Missing body",
                    Value = new Dictionary<string, object>
                    {
                        ["Error"] = "RequestBodyMissing",
                        ["Message"] = "A request body is required for this operation.",
                        ["StatusCode"] = 400,
                        ["Description"] = "No request body found."
                    }
                },
                ["badRequest"] = new OpenApiExampleMetadata
                {
                    Summary = "Bad request",
                    Value = new Dictionary<string, object>
                    {
                        ["Error"] = "BadRequest",
                        ["Message"] = "We were unable to discern your request.  Please check your URL, query, and request body.",
                        ["StatusCode"] = 400,
                        ["Description"] = "Deserialization error: invalid request body."
                    }
                }
            };
        }

        private static List<object> ChunkStrategyValues()
        {
            return new List<object> { "FixedTokenCount", "SentenceBased", "ParagraphBased", "RegexBased", "WholeList", "ListEntry", "Row", "RowWithHeaders", "RowGroupWithHeaders", "KeyValuePairs", "WholeTable" };
        }

        private static List<object> OverlapStrategyValues()
        {
            return new List<object> { "SlidingWindow", "SentenceBoundaryAware", "SemanticBoundaryAware" };
        }

        private static List<object> AtomTypeValues()
        {
            return new List<object> { "Text", "List", "Binary", "Table", "Unknown", "Image", "Hyperlink", "Code", "Meta" };
        }

        private static List<object> MarkdownFormattingValues()
        {
            return new List<object> { "Text", "Header", "Code", "UnorderedList", "OrderedList", "Link", "Image", "Url", "Table" };
        }

        private static List<object> DocumentTypeValues()
        {
            return new List<object> { "Unknown", "Bmp", "Csv", "DataTable", "Doc", "Docx", "Epub", "Gif", "Gpx", "Gzip", "Html", "Ico", "Jpeg", "Json", "Keynote", "Markdown", "Mov", "Mp3", "Mp4", "Numbers", "Odp", "Ods", "Odt", "Pages", "Parquet", "Pdf", "Png", "PostScript", "Ppt", "Pptx", "Rar", "Rtf", "SevenZip", "Sqlite", "Svg", "Tar", "Text", "Tiff", "Tsv", "WebP", "Xls", "Xlsx", "Xml" };
        }

        private static List<object> ApiErrorValues()
        {
            return new List<object> { "RequestBodyMissing", "RequiredPropertiesMissing", "UnknownTypeDetected", "AuthenticationFailed", "AuthorizationFailed", "BadGateway", "BadRequest", "Conflict", "DeserializationError", "InternalError", "NotFound", "Timeout" };
        }
    }
}
