namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SerializationHelper;
    using SyslogLogging;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Text;
    using DocumentAtom.Text.Csv;
    using DocumentAtom.Text.Html;
    using DocumentAtom.Text.Json;
    using DocumentAtom.Text.Markdown;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.Text.Xml;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Ocr;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Word;

    /// <summary>
    /// DocumentAtom server.
    /// </summary>
    public static class Program
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        #region Public-Members

        #endregion

        #region Private-Members

        private static string _Header = "[DocumentAtom] ";
        private static int _ProcessId = Environment.ProcessId;

        private static Serializer _Serializer = new Serializer();
        private static ServerSettings _Settings = new ServerSettings();
        private static LoggingModule _Logging = null;

        private static Webserver _RestServer = null;

        private static List<string> _Localhost = new List<string>
        {
            "127.0.0.1",
            "localhost",
            "::1"
        };

        private static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private static CancellationToken _Token;

        private static readonly JsonSerializerOptions _JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        #endregion

        #region Entrypoint

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            Welcome();
            InitializeSettings();
            InitializeGlobals();

            _Logging.Info(_Header + "starting at " + DateTime.UtcNow + " using process ID " + _ProcessId);

            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            AssemblyLoadContext.Default.Unloading += (ctx) => waitHandle.Set();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                waitHandle.Set();
                eventArgs.Cancel = true;
            };

            bool waitHandleSignal = false;
            do
            {
                waitHandleSignal = waitHandle.WaitOne(1000);
            }
            while (!waitHandleSignal);

            _Logging.Info(_Header + "stopping at " + DateTime.UtcNow);
        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        private static void Welcome()
        {
            Console.WriteLine(
                Environment.NewLine +
                Constants.Logo +
                Environment.NewLine +
                Constants.ProductName +
                Environment.NewLine +
                Constants.Copyright +
                Environment.NewLine);

            Console.WriteLine(
                "NOTICE" + Environment.NewLine +
                "------" + Environment.NewLine +
                "DocumentAtom requires that Tesseract v5.0.0 be installed on the host." + Environment.NewLine);
        }

        private static List<string> EnumerateFiles(string root, List<string> list = null)
        {
            if (list == null) list = new List<string>();

            string[] files = Directory.GetFiles(root);

            foreach (string fileName in files)
            {
                list.Add(fileName);
            }

            string[] subdirs = Directory.GetDirectories(root);
            foreach (string subdirectory in subdirs) EnumerateFiles(subdirectory, list);

            return list;
        }

        private static void InitializeSettings()
        {
            Console.WriteLine("Using settings file '" + Constants.SettingsFile + "'");

            if (!File.Exists(Constants.SettingsFile))
            {
                Console.WriteLine("Settings file '" + Constants.SettingsFile + "' does not exist, creating new");
                _Settings = new ServerSettings();
                File.WriteAllBytes(Constants.SettingsFile, Encoding.UTF8.GetBytes(_Serializer.SerializeJson(_Settings, true)));
            }
            else
            {
                _Settings = _Serializer.DeserializeJson<ServerSettings>(File.ReadAllText(Constants.SettingsFile));
            }
        }

        private static void InitializeGlobals()
        {
            #region General

            _Token = _TokenSource.Token;

            #endregion

            #region Logging

            Console.WriteLine("Initializing logging");

            List<SyslogServer> syslogServers = new List<SyslogServer>();

            if (_Settings.Logging.Servers != null && _Settings.Logging.Servers.Count > 0)
            {
                foreach (SyslogServer server in _Settings.Logging.Servers)
                {
                    syslogServers.Add(
                        new SyslogServer
                        {
                            Hostname = server.Hostname,
                            Port = server.Port
                        }
                    );

                    Console.WriteLine("| syslog://" + server.Hostname + ":" + server.Port);
                }
            }

            if (syslogServers.Count > 0)
                _Logging = new LoggingModule(syslogServers);
            else
                _Logging = new LoggingModule();

            _Logging.Settings.MinimumSeverity = (SyslogLogging.Severity)_Settings.Logging.MinimumSeverity;
            _Logging.Settings.EnableConsole = _Settings.Logging.ConsoleLogging;
            _Logging.Settings.EnableColors = _Settings.Logging.EnableColors;

            if (!String.IsNullOrEmpty(_Settings.Logging.LogDirectory))
            {
                if (!Directory.Exists(_Settings.Logging.LogDirectory))
                    Directory.CreateDirectory(_Settings.Logging.LogDirectory);

                _Settings.Logging.LogFilename = _Settings.Logging.LogDirectory + _Settings.Logging.LogFilename;
            }

            if (!String.IsNullOrEmpty(_Settings.Logging.LogFilename))
            {
                _Logging.Settings.FileLogging = FileLoggingMode.FileWithDate;
                _Logging.Settings.LogFilename = _Settings.Logging.LogFilename;
            }

            _Logging.Debug(_Header + "logging initialized");

            #endregion

            #region REST-Server

            _RestServer = new Webserver(_Settings.Webserver, DefaultRoute);
            _RestServer.Routes.PreRouting = PreRoutingRoute;
            _RestServer.Routes.Preflight = OptionsHandler;
            _RestServer.Routes.PostRouting = PostRoutingRoute;

            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.HEAD, "/", LoopbackRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/", RootRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.HEAD, "/favicon.ico", HeadFavicon, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.GET, "/favicon.ico", GetFavicon, ExceptionRoute);

            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/typedetect", TypeDetectionRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/csv", CsvAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/excel", ExcelAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/html", HtmlAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/json", JsonAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/markdown", MarkdownAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/ocr", OcrAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/pdf", PdfAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/png", PngAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/powerpoint", PowerPointAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/rtf", RtfAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/text", TextAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/word", WordAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/xml", XmlAtomRoute, ExceptionRoute);

            Console.WriteLine("Starting REST server on       : " + _Settings.Webserver.Prefix);
            _RestServer.Start();

            if (_Localhost.Contains(_Settings.Webserver.Hostname))
            {
                _Logging.Alert(_Header + Environment.NewLine + Environment.NewLine
                    + "NOTICE" + Environment.NewLine
                    + "------" + Environment.NewLine
                    + "DocumentAtom is configured to listen on localhost and will not be externally accessible." + Environment.NewLine
                    + "Modify " + Constants.SettingsFile + " to change the REST listener hostname to make externally accessible." + Environment.NewLine);
            }

            #endregion

            Console.WriteLine("");
        }

        private static async Task OptionsHandler(HttpContextBase ctx)
        {
            NameValueCollection responseHeaders = new NameValueCollection(StringComparer.InvariantCultureIgnoreCase);

            string[] requestedHeaders = null;
            string headers = "";

            if (ctx.Request.Headers != null)
            {
                for (int i = 0; i < ctx.Request.Headers.Count; i++)
                {
                    string key = ctx.Request.Headers.GetKey(i);
                    string value = ctx.Request.Headers.Get(i);
                    if (String.IsNullOrEmpty(key)) continue;
                    if (String.IsNullOrEmpty(value)) continue;
                    if (String.Compare(key.ToLower(), "access-control-request-headers") == 0)
                    {
                        requestedHeaders = value.Split(',');
                        break;
                    }
                }
            }

            if (requestedHeaders != null)
            {
                foreach (string curr in requestedHeaders)
                {
                    headers += ", " + curr;
                }
            }

            responseHeaders.Add("Access-Control-Allow-Methods", "OPTIONS, HEAD, GET, PUT, POST, DELETE");
            responseHeaders.Add("Access-Control-Allow-Headers", "*, Content-Type, X-Requested-With, " + headers);
            responseHeaders.Add("Access-Control-Expose-Headers", "Content-Type, X-Requested-With, " + headers);
            responseHeaders.Add("Access-Control-Allow-Origin", "*");
            responseHeaders.Add("Accept", "*/*");
            responseHeaders.Add("Accept-Language", "en-US, en");
            responseHeaders.Add("Accept-Charset", "ISO-8859-1, utf-8");
            responseHeaders.Add("Connection", "keep-alive");

            ctx.Response.StatusCode = 200;
            ctx.Response.Headers = responseHeaders;
            await ctx.Response.Send();
            return;
        }

        private static async Task PreRoutingRoute(HttpContextBase ctx)
        {
            ctx.Response.ContentType = Constants.JsonContentType;
        }

        private static async Task PostRoutingRoute(HttpContextBase ctx)
        {
            _Logging.Debug(_Header + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " from " + ctx.Request.Source.IpAddress + ": " + ctx.Response.StatusCode);
        }

        private static async Task RootRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.HtmlContentType;
            await ctx.Response.Send(Constants.HtmlHomepage);
        }

        private static async Task GetFavicon(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.FaviconContentType;
            await ctx.Response.Send(File.ReadAllBytes(Constants.FaviconFilename));
        }

        private static async Task HeadFavicon(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.FaviconContentType;
            await ctx.Response.Send();
        }

        private static async Task ExceptionRoute(HttpContextBase ctx, Exception e)
        {
            if (e is System.IO.FileFormatException)
            {
                _Logging.Warn(_Header + "file format exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is DocumentFormat.OpenXml.Packaging.OpenXmlPackageException)
            {
                _Logging.Warn(_Header + "OpenXML package exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is System.IO.InvalidDataException)
            {
                _Logging.Warn(_Header + "invalid data exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else if (e is UglyToad.PdfPig.Core.PdfDocumentFormatException)
            {
                _Logging.Warn(_Header + "PDF document format exception: " + Environment.NewLine + e.ToString());
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, e.Message), true));
                return;
            }
            else
            {
                _Logging.Warn(
                    _Header +
                    "handling exception for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " " +
                    "from " + ctx.Request.Source.IpAddress +
                    Environment.NewLine +
                    e.ToString());

                ctx.Response.StatusCode = 500;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.InternalError, null, e.Message), true));
                return;
            }
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            _Logging.Warn(_Header + "unknown verb or endpoint " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery + " from " + ctx.Request.Source.IpAddress);
            ctx.Response.StatusCode = 400;
            await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Unknown verb or endpoint."), true));
            return;
        }

        private static async Task LoopbackRoute(HttpContextBase ctx)
        {
            ctx.Response.StatusCode = 200;
            ctx.Response.ContentType = Constants.TextContentType;
            await ctx.Response.Send();
        }

        private static (byte[] data, ApiProcessorSettings settings) ParseRequest(HttpContextBase ctx)
        {
            if (ctx.Request.DataAsBytes == null || ctx.Request.ContentLength < 1)
                throw new InvalidOperationException("RequestBodyMissing");

            string json = Encoding.UTF8.GetString(ctx.Request.DataAsBytes);
            AtomRequest request = System.Text.Json.JsonSerializer.Deserialize<AtomRequest>(json, _JsonOptions);
            if (request == null)
                throw new InvalidOperationException("DeserializationError");

            byte[] data = request.GetDataBytes();
            return (data, request.Settings);
        }

        private static void ApplyBaseSettings(ProcessorSettingsBase defaults, ApiProcessorSettings api)
        {
            if (api == null) return;
            if (api.TrimText.HasValue) defaults.TrimText = api.TrimText.Value;
            if (api.RemoveBinaryFromText.HasValue) defaults.RemoveBinaryFromText = api.RemoveBinaryFromText.Value;
            if (api.ExtractAtomsFromImages.HasValue) defaults.ExtractAtomsFromImages = api.ExtractAtomsFromImages.Value;
            if (api.Chunking != null) defaults.Chunking = api.Chunking;
        }

        private static CsvProcessorSettings ApplyApiSettings(CsvProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.RowDelimiter != null) defaults.RowDelimiter = api.RowDelimiter;
            if (api.ColumnDelimiter.HasValue) defaults.ColumnDelimiter = api.ColumnDelimiter.Value;
            if (api.HasHeaderRow.HasValue) defaults.HasHeaderRow = api.HasHeaderRow.Value;
            if (api.RowsPerAtom.HasValue) defaults.RowsPerAtom = api.RowsPerAtom.Value;
            return defaults;
        }

        private static XlsxProcessorSettings ApplyApiSettings(XlsxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.HeaderRowScoreThreshold.HasValue) defaults.HeaderRowScoreThreshold = api.HeaderRowScoreThreshold.Value;
            return defaults;
        }

        private static HtmlProcessorSettings ApplyApiSettings(HtmlProcessorSettings defaults, ApiProcessorSettings api)
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

        private static JsonProcessorSettings ApplyApiSettings(JsonProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.MaxDepth.HasValue) defaults.MaxDepth = api.MaxDepth.Value;
            return defaults;
        }

        private static MarkdownProcessorSettings ApplyApiSettings(MarkdownProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.Delimiters != null) defaults.Delimiters = api.Delimiters;
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        private static TextProcessorSettings ApplyApiSettings(TextProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.Delimiters != null) defaults.Delimiters = api.Delimiters;
            return defaults;
        }

        private static XmlProcessorSettings ApplyApiSettings(XmlProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            if (api.MaxDepth.HasValue) defaults.MaxDepth = api.MaxDepth.Value;
            if (api.IncludeAttributes.HasValue) defaults.IncludeAttributes = api.IncludeAttributes.Value;
            if (api.PreserveWhitespace.HasValue) defaults.PreserveWhitespace = api.PreserveWhitespace.Value;
            return defaults;
        }

        private static PdfProcessorSettings ApplyApiSettings(PdfProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            return defaults;
        }

        private static DocxProcessorSettings ApplyApiSettings(DocxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        private static PptxProcessorSettings ApplyApiSettings(PptxProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            if (api.BuildHierarchy.HasValue) defaults.BuildHierarchy = api.BuildHierarchy.Value;
            return defaults;
        }

        private static RtfProcessorSettings ApplyApiSettings(RtfProcessorSettings defaults, ApiProcessorSettings api)
        {
            if (api == null) return defaults;
            ApplyBaseSettings(defaults, api);
            return defaults;
        }

        private static ImageProcessorSettings ApplyApiSettings(ImageProcessorSettings defaults, ApiProcessorSettings api)
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

        private static ImageProcessorSettings BuildImageSettings(ApiProcessorSettings api)
        {
            ImageProcessorSettings imageSettings = new ImageProcessorSettings
            {
                TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                TesseractLanguage = _Settings.Tesseract.Language,
            };
            return ApplyApiSettings(imageSettings, api);
        }

        private static void ApplyChunking(List<Atom> atoms, ChunkingConfiguration config)
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

        private static async Task CsvAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            CsvProcessorSettings settings = ApplyApiSettings(new CsvProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (CsvProcessor processor = new CsvProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task ExcelAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            XlsxProcessorSettings settings = ApplyApiSettings(new XlsxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (XlsxProcessor processor = new XlsxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task HtmlAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            HtmlProcessorSettings settings = ApplyApiSettings(new HtmlProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (HtmlProcessor processor = new HtmlProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, apiSettings?.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task JsonAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            JsonProcessorSettings settings = ApplyApiSettings(new JsonProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (JsonProcessor processor = new JsonProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task MarkdownAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            MarkdownProcessorSettings settings = ApplyApiSettings(new MarkdownProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (MarkdownProcessor processor = new MarkdownProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task OcrAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            using (ImageContentExtractor ice = new ImageContentExtractor(_Settings.Tesseract.DataDirectory, _Settings.Tesseract.Language))
            {
                ExtractionResult er = ice.ExtractContent(data);
                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(er, true));
                return;
            }
        }

        private static async Task PdfAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            PdfProcessorSettings settings = ApplyApiSettings(new PdfProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (PdfProcessor processor = new PdfProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task PngAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            ImageProcessorSettings settings = BuildImageSettings(apiSettings);

            List<Atom> ret = new List<Atom>();

            using (ImageProcessor processor = new ImageProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task PowerPointAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            PptxProcessorSettings settings = ApplyApiSettings(new PptxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (PptxProcessor processor = new PptxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task RtfAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            RtfProcessorSettings settings = ApplyApiSettings(new RtfProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (RtfProcessor processor = new RtfProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task TextAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            TextProcessorSettings settings = ApplyApiSettings(new TextProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (TextProcessor processor = new TextProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task WordAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            DocxProcessorSettings settings = ApplyApiSettings(new DocxProcessorSettings(), apiSettings);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = BuildImageSettings(apiSettings);
            }

            List<Atom> ret = new List<Atom>();

            using (DocxProcessor processor = new DocxProcessor(settings, imageSettings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task XmlAtomRoute(HttpContextBase ctx)
        {
            byte[] data;
            ApiProcessorSettings apiSettings;

            try
            {
                (data, apiSettings) = ParseRequest(ctx);
            }
            catch (InvalidOperationException ex) when (ex.Message == "RequestBodyMissing")
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }
            catch (Exception ex)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.BadRequest, null, "Deserialization error: " + ex.Message), true));
                return;
            }

            XmlProcessorSettings settings = ApplyApiSettings(new XmlProcessorSettings(), apiSettings);

            List<Atom> ret = new List<Atom>();

            using (XmlProcessor processor = new XmlProcessor(settings))
            {
                IEnumerable<Atom> atoms = processor.Extract(data).ToList();
                if (atoms != null && atoms.Any()) ret = atoms.ToList();
                ApplyChunking(ret, settings.Chunking);

                ctx.Response.StatusCode = 200;
                await ctx.Response.Send(_Serializer.SerializeJson(ret, true));
                return;
            }
        }

        private static async Task TypeDetectionRoute(HttpContextBase ctx)
        {
            if (ctx.Request.DataAsBytes == null || ctx.Request.ContentLength < 1)
            {
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            byte[] data = ctx.Request.DataAsBytes;

            string contentType = ctx.Request.ContentType;
            if (String.IsNullOrEmpty(contentType))
                contentType = "application/octet-stream";

            string dir = "./" + Guid.NewGuid() + "/";
            TypeResult tr = new TypeResult();

            try
            {
                using (TypeDetector td = new TypeDetector(dir))
                {
                    tr = td.Process(data, contentType);
                }
            }
            finally
            {
                FileHelper.RecursiveDelete(new DirectoryInfo(dir), true);
                Directory.Delete(dir);
            }

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(tr, true));
        }

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}