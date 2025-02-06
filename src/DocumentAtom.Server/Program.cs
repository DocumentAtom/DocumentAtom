namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using SerializationHelper;
    using SyslogLogging;
    using WatsonWebserver;
    using WatsonWebserver.Core;

    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Excel;
    using DocumentAtom.Image;
    using DocumentAtom.Markdown;
    using DocumentAtom.Pdf;
    using DocumentAtom.PowerPoint;
    using DocumentAtom.Text;
    using DocumentAtom.TypeDetection;
    using DocumentAtom.Word;
    using System.Linq;

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

        private static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private static CancellationToken _Token;

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

            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.HEAD, "/", LoopbackRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/typedetect", TypeDetectionRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/excel", ExcelAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/markdown", MarkdownAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/pdf", PdfAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/png", PngAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/powerpoint", PowerPointAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/text", TextAtomRoute, ExceptionRoute);
            _RestServer.Routes.PreAuthentication.Static.Add(HttpMethod.POST, "/atom/word", WordAtomRoute, ExceptionRoute);

            Console.WriteLine("Starting REST server on       : " + _Settings.Webserver.Prefix);
            _RestServer.Start();

            #endregion

            Console.WriteLine("");
        }

        private static async Task ExceptionRoute(HttpContextBase ctx, Exception e)
        {
            ctx.Response.StatusCode = 500;
            await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.InternalError, null, e.Message), true));
            return;
        }

        private static async Task DefaultRoute(HttpContextBase ctx)
        {
            _Logging.Warn(_Header + "unknown verb or endpoint " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
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

        private static async Task ExcelAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            XlsxProcessorSettings settings = new XlsxProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";
            settings.ExtractAtomsFromImages = ctx.Request.QuerystringExists(Constants.OcrQuerystring);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = new ImageProcessorSettings
                {
                    TempDirectory = settings.TempDirectory,
                    TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                    TesseractLanguage = _Settings.Tesseract.Language,

                };
            }

            XlsxProcessor processor = new XlsxProcessor(settings, imageSettings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task MarkdownAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            MarkdownProcessorSettings settings = new MarkdownProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";

            MarkdownProcessor processor = new MarkdownProcessor(settings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task PdfAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            PdfProcessorSettings settings = new PdfProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";
            settings.ExtractAtomsFromImages = ctx.Request.QuerystringExists(Constants.OcrQuerystring);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = new ImageProcessorSettings
                {
                    TempDirectory = settings.TempDirectory,
                    TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                    TesseractLanguage = _Settings.Tesseract.Language,

                };
            }

            PdfProcessor processor = new PdfProcessor(settings, imageSettings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task PngAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            ImageProcessorSettings settings = new ImageProcessorSettings
            {
                TempDirectory = "./" + Guid.NewGuid() + "/",
                TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                TesseractLanguage = _Settings.Tesseract.Language,
            };

            ImageProcessor processor = new ImageProcessor(settings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task PowerPointAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            PptxProcessorSettings settings = new PptxProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";
            settings.ExtractAtomsFromImages = ctx.Request.QuerystringExists(Constants.OcrQuerystring);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = new ImageProcessorSettings
                {
                    TempDirectory = settings.TempDirectory,
                    TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                    TesseractLanguage = _Settings.Tesseract.Language,

                };
            }

            PptxProcessor processor = new PptxProcessor(settings, imageSettings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return; 
        }

        private static async Task TextAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            TextProcessorSettings settings = new TextProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";

            TextProcessor processor = new TextProcessor(settings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task WordAtomRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            DocxProcessorSettings settings = new DocxProcessorSettings();
            settings.TempDirectory = "./" + Guid.NewGuid() + "/";
            settings.ExtractAtomsFromImages = ctx.Request.QuerystringExists(Constants.OcrQuerystring);

            ImageProcessorSettings imageSettings = null;
            if (settings.ExtractAtomsFromImages)
            {
                imageSettings = new ImageProcessorSettings
                {
                    TempDirectory = settings.TempDirectory,
                    TesseractDataDirectory = _Settings.Tesseract.DataDirectory,
                    TesseractLanguage = _Settings.Tesseract.Language,

                };
            }

            DocxProcessor processor = new DocxProcessor(settings, imageSettings);
            List<Atom> ret = new List<Atom>();
            IEnumerable<Atom> atoms = processor.Extract(ctx.Request.DataAsBytes).ToList();
            if (atoms != null && atoms.Count() > 0) ret = atoms.ToList();

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(atoms, true));
            return;
        }

        private static async Task PreRoutingRoute(HttpContextBase ctx)
        {
            ctx.Response.ContentType = Constants.JsonContentType;
        }

        private static async Task TypeDetectionRoute(HttpContextBase ctx)
        {
            if (ctx.Request.Data == null && ctx.Request.ContentLength < 1)
            {
                _Logging.Warn(_Header + "request body missing for " + ctx.Request.Method + " " + ctx.Request.Url.RawWithQuery);
                ctx.Response.StatusCode = 400;
                await ctx.Response.Send(_Serializer.SerializeJson(new ApiErrorResponse(ApiErrorEnum.RequestBodyMissing, null, "No request body found."), true));
                return;
            }

            Guid guid = Guid.NewGuid();
            string dir = "./" + guid + "/";
            TypeDetector td = new TypeDetector(dir);
            TypeResult tr = td.Process(ctx.Request.DataAsBytes, ctx.Request.ContentType);

            ctx.Response.StatusCode = 200;
            await ctx.Response.Send(_Serializer.SerializeJson(tr, true));
        }

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}