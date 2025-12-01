namespace DocumentAtom.McpServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.McpServer.Registrations;
    using DocumentAtom.Sdk;
    using SyslogLogging;
    using Voltaic;

    /// <summary>
    /// DocumentAtom MCP Server - Exposes DocumentAtom operations via Model Context Protocol.
    /// Supports document processing, text extraction, and atomization operations.
    /// </summary>
    public static class DocumentAtomMcpServer
    {
        #region Private-Members

        private static string _Header = "[DocumentAtom.McpServer] ";
        private static string _SoftwareVersion = Constants.Version;
        private static int _ProcessId = Environment.ProcessId;
        private static bool _ShowConfiguration = false;

        private static DocumentAtomMcpServerSettings _Settings = new DocumentAtomMcpServerSettings();
        private static LoggingModule _Logging = null!;
        private static DocumentAtomSdk? _McpSdk = null;
        private static SerializationHelper.Serializer _Serializer = new SerializationHelper.Serializer();
        private static McpHttpServer? _McpHttpServer = null;
        private static McpTcpServer? _McpTcpServer = null;
        private static McpWebsocketsServer? _McpWebsocketServer = null;
        private static Task? _McpHttpServerTask = null;
        private static Task? _McpTcpServerTask = null;
        private static Task? _McpWebsocketServerTask = null;

        private static CancellationTokenSource _TokenSource = new CancellationTokenSource();
        private static CancellationToken _Token;

        #endregion

        #region Public-Members

        #endregion

        #region Entrypoint

        /// <summary>
        /// Main.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static void Main(string[] args)
        {
            Welcome();
            ParseArguments(args);
            InitializeSettings();
            InitializeGlobals();

            _Logging.Info(_Header + "starting at " + DateTime.UtcNow + " using process ID " + _ProcessId + Environment.NewLine);

            EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            AssemblyLoadContext.Default.Unloading += (ctx) => waitHandle.Set();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                _Logging.Info(_Header + "termination signal received");
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

        private static void ParseArguments(string[] args)
        {
            if (args != null && args.Length > 0)
            {
                foreach (string arg in args)
                {
                    if (arg.StartsWith("--config="))
                    {
                        Constants.SettingsFile = arg.Substring(9);
                    }

                    if (arg.Equals("--showconfig"))
                    {
                        _ShowConfiguration = true;
                    }

                    if (arg.Equals("--help") || arg.Equals("-h"))
                    {
                        ShowHelp();
                        Environment.Exit(0);
                    }
                }
            }
        }

        private static void InitializeSettings()
        {
            Console.WriteLine("Using settings file '" + Constants.SettingsFile + "'");

            if (!File.Exists(Constants.SettingsFile))
            {
                Console.WriteLine("Settings file '" + Constants.SettingsFile + "' does not exist. Creating default configuration...");

                File.WriteAllBytes(Constants.SettingsFile, Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(_Settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })));
                Console.WriteLine("Created settings file '" + Constants.SettingsFile + "' with default configuration");
            }
            else
            {
                _Settings = System.Text.Json.JsonSerializer.Deserialize<DocumentAtomMcpServerSettings>(File.ReadAllText(Constants.SettingsFile), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
                File.WriteAllBytes(Constants.SettingsFile, Encoding.UTF8.GetBytes(System.Text.Json.JsonSerializer.Serialize(_Settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true })));
            }

            if (_ShowConfiguration)
            {
                Console.WriteLine();
                Console.WriteLine("Configuration:");
                Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(_Settings, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
                Console.WriteLine();
                Environment.Exit(0);
            }
        }

        private static void InitializeGlobals()
        {
            #region General

            _Token = _TokenSource.Token;

            #endregion

            #region Environment

            string? documentAtomEndpoint = Environment.GetEnvironmentVariable(Constants.DocumentAtomEndpointEnvironmentVariable);
            if (!String.IsNullOrEmpty(documentAtomEndpoint)) _Settings.DocumentAtom.Endpoint = documentAtomEndpoint;

            string? documentAtomApiKey = Environment.GetEnvironmentVariable(Constants.DocumentAtomAccessKeyEnvironmentVariable);
            if (!String.IsNullOrEmpty(documentAtomApiKey)) _Settings.DocumentAtom.AccessKey = documentAtomApiKey;

            string? httpHostname = Environment.GetEnvironmentVariable(Constants.McpHttpHostnameEnvironmentVariable);
            if (!String.IsNullOrEmpty(httpHostname)) _Settings.Http.Hostname = httpHostname;

            string? httpPort = Environment.GetEnvironmentVariable(Constants.McpHttpPortEnvironmentVariable);
            if (!String.IsNullOrEmpty(httpPort))
            {
                if (Int32.TryParse(httpPort, out int val))
                {
                    if (val > 0 && val <= 65535) _Settings.Http.Port = val;
                }
            }

            string? tcpAddressEnv = Environment.GetEnvironmentVariable(Constants.McpTcpAddressEnvironmentVariable);
            if (!String.IsNullOrEmpty(tcpAddressEnv)) _Settings.Tcp.Address = tcpAddressEnv;

            string? tcpPort = Environment.GetEnvironmentVariable(Constants.McpTcpPortEnvironmentVariable);
            if (!String.IsNullOrEmpty(tcpPort))
            {
                if (Int32.TryParse(tcpPort, out int val))
                {
                    if (val > 0 && val <= 65535) _Settings.Tcp.Port = val;
                }
            }

            string? wsHostname = Environment.GetEnvironmentVariable(Constants.McpWebSocketHostnameEnvironmentVariable);
            if (!String.IsNullOrEmpty(wsHostname)) _Settings.WebSocket.Hostname = wsHostname;

            string? wsPort = Environment.GetEnvironmentVariable(Constants.McpWebSocketPortEnvironmentVariable);
            if (!String.IsNullOrEmpty(wsPort))
            {
                if (Int32.TryParse(wsPort, out int val))
                {
                    if (val > 0 && val <= 65535) _Settings.WebSocket.Port = val;
                }
            }

            string? consoleLogging = Environment.GetEnvironmentVariable(Constants.ConsoleLoggingEnvironmentVariable);
            if (!String.IsNullOrEmpty(consoleLogging))
            {
                if (Int32.TryParse(consoleLogging, out int val))
                {
                    if (val > 0) _Settings.Logging.ConsoleLogging = true;
                    else _Settings.Logging.ConsoleLogging = false;
                }
            }

            #endregion

            #region Logging

            Console.WriteLine("Initializing logging");

            List<SyslogServer> syslogServers = new List<SyslogServer>();

            if (_Settings.Logging.Servers != null && _Settings.Logging.Servers.Count > 0)
            {
                foreach (SyslogServer server in _Settings.Logging.Servers)
                {
                    syslogServers.Add(server);
                    Console.WriteLine("| syslog://" + server.Hostname + ":" + server.Port);
                }
            }

            if (syslogServers.Count > 0)
                _Logging = new LoggingModule(syslogServers);
            else
                _Logging = new LoggingModule();

            _Logging.Settings.MinimumSeverity = (Severity)_Settings.Logging.MinimumSeverity;
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

            #region Storage

            Console.WriteLine("Initializing storage");

            if (!String.IsNullOrEmpty(_Settings.Storage.BackupsDirectory))
            {
                if (!Directory.Exists(_Settings.Storage.BackupsDirectory))
                    Directory.CreateDirectory(_Settings.Storage.BackupsDirectory);
            }

            if (!String.IsNullOrEmpty(_Settings.Storage.TempDirectory))
            {
                if (!Directory.Exists(_Settings.Storage.TempDirectory))
                    Directory.CreateDirectory(_Settings.Storage.TempDirectory);
            }

            #endregion

            #region DocumentAtom-SDK

            _Logging.Debug(_Header + "Initializing DocumentAtom SDK");

            if (string.IsNullOrEmpty(_Settings.DocumentAtom.Endpoint))
            {
                throw new InvalidOperationException("DocumentAtom endpoint is required. Please configure 'DocumentAtom.Endpoint' in settings or set DOCUMENTATOM_ENDPOINT environment variable.");
            }

            _Logging.Debug(_Header + "Connecting to DocumentAtom server at: " + _Settings.DocumentAtom.Endpoint);
            _McpSdk = new DocumentAtomSdk(_Settings.DocumentAtom.Endpoint, _Settings.DocumentAtom.AccessKey);

            if (_Logging != null)
            {
                _McpSdk.Logger = (sev, msg) =>
                {
                    Severity syslogSeverity = MapSdkSeverityToSyslog(sev);
                    _Logging.Log(syslogSeverity, msg);
                };
            }

            #endregion

            #region MCP-Server

            Console.WriteLine(
                "Starting MCP servers on:" + Environment.NewLine +
                "| HTTP         : http://" + _Settings.Http.Hostname + ":" + _Settings.Http.Port + "/rpc" + Environment.NewLine +
                "| TCP          : tcp://" + (_Settings.Tcp.Address.Equals("localhost", StringComparison.OrdinalIgnoreCase) ? "127.0.0.1" : _Settings.Tcp.Address) + ":" + _Settings.Tcp.Port + Environment.NewLine +
                "| WebSocket    : ws://" + _Settings.WebSocket.Hostname + ":" + _Settings.WebSocket.Port + "/mcp");

            string tcpAddressForBinding = _Settings.Tcp.Address.Equals("localhost", StringComparison.OrdinalIgnoreCase) ? "127.0.0.1" : _Settings.Tcp.Address;

            _McpHttpServer = new McpHttpServer(_Settings.Http.Hostname, _Settings.Http.Port, "/rpc", "/events", includeDefaultMethods: true);
            _McpTcpServer = new McpTcpServer(IPAddress.Parse(tcpAddressForBinding), _Settings.Tcp.Port, includeDefaultMethods: true);
            _McpWebsocketServer = new McpWebsocketsServer(_Settings.WebSocket.Hostname, _Settings.WebSocket.Port, "/mcp", includeDefaultMethods: true);

            _McpHttpServer.ServerName = "DocumentAtom.McpServer";
            _McpHttpServer.ServerVersion = "1.0.0";
            _McpTcpServer.ServerName = "DocumentAtom.McpServer";
            _McpTcpServer.ServerVersion = "1.0.0";
            _McpWebsocketServer.ServerName = "DocumentAtom.McpServer";
            _McpWebsocketServer.ServerVersion = "1.0.0";

            _McpHttpServer.ClientConnected += ClientConnected;
            _McpHttpServer.ClientDisconnected += ClientDisconnected;
            _McpHttpServer.RequestReceived += ClientRequestReceived;
            _McpHttpServer.ResponseSent += ClientResponseSent;

            _McpTcpServer.ClientConnected += ClientConnected;
            _McpTcpServer.ClientDisconnected += ClientDisconnected;
            _McpTcpServer.RequestReceived += ClientRequestReceived;
            _McpTcpServer.ResponseSent += ClientResponseSent;

            _McpWebsocketServer.ClientConnected += ClientConnected;
            _McpWebsocketServer.ClientDisconnected += ClientDisconnected;
            _McpWebsocketServer.RequestReceived += ClientRequestReceived;
            _McpWebsocketServer.ResponseSent += ClientResponseSent;

            RegisterMcpTools();

            _McpHttpServerTask = _McpHttpServer.StartAsync(_Token);
            _McpTcpServerTask = _McpTcpServer.StartAsync(_Token);
            _McpWebsocketServerTask = _McpWebsocketServer.StartAsync(_Token);

            #endregion

            Console.WriteLine("");
        }

        private static void ClientConnected(object? sender, ClientConnection e)
        {
            _Logging.Debug(_Header + "client connection started with session ID " + e.SessionId + " (" + e.Type + ")");
        }

        private static void ClientDisconnected(object? sender, ClientConnection e)
        {
            _Logging.Debug(_Header + "client connection terminated with session ID " + e.SessionId + " (" + e.Type + ")");
        }

        private static void ClientRequestReceived(object? sender, JsonRpcRequestEventArgs e)
        {
            _Logging.Debug(_Header + "client session " + e.Client.SessionId + " request " + e.Method);
        }

        private static void ClientResponseSent(object? sender, JsonRpcResponseEventArgs e)
        {
            _Logging.Debug(_Header + "client session " + e.Client.SessionId + " request " + e.Method + " completed (" + e.Duration.TotalMilliseconds + "ms)");
        }

        private static void RegisterMcpTools()
        {
            if (_McpHttpServer == null || _McpTcpServer == null || _McpWebsocketServer == null || _McpSdk == null)
                throw new InvalidOperationException("Servers and SDK have not been initialized");

            ImageRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            CsvRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            ExcelRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            HtmlRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            JsonRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            MarkdownRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            OcrRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            PdfRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            PowerPointRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);
            RichTextRegistrations.RegisterHttpTools(_McpHttpServer, _McpSdk, _Serializer);

            ImageRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            CsvRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            ExcelRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            HtmlRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            JsonRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            MarkdownRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            OcrRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            PdfRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            PowerPointRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);
            RichTextRegistrations.RegisterTcpMethods(_McpTcpServer, _McpSdk, _Serializer);

            ImageRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            CsvRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            ExcelRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            HtmlRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            JsonRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            MarkdownRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            OcrRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            PdfRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            PowerPointRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
            RichTextRegistrations.RegisterWebSocketMethods(_McpWebsocketServer, _McpSdk, _Serializer);
        }

        private static void ShowHelp()
        {
            Console.WriteLine("DocumentAtom MCP Server");
            Console.WriteLine();
            Console.WriteLine("Usage:");
            Console.WriteLine("  DocumentAtom.McpServer [options]");
            Console.WriteLine();
            Console.WriteLine("Options:");
            Console.WriteLine("  --config=<file>        Settings file path (default: ./documentatom.json)");
            Console.WriteLine("  --showconfig           Display configuration and exit");
            Console.WriteLine("  --help, -h             Show this help message");
            Console.WriteLine();
            Console.WriteLine("Configuration:");
            Console.WriteLine("  Settings are read from documentatom.json file.");
            Console.WriteLine("  If the file doesn't exist, it will be created with default values.");
            Console.WriteLine();
            Console.WriteLine("  Required settings:");
            Console.WriteLine("    DocumentAtom.Endpoint    - Remote DocumentAtom server endpoint URL (required)");
            Console.WriteLine("    DocumentAtom.AccessKey   - Access key for authentication (optional)");
            Console.WriteLine();
            Console.WriteLine("  Environment variables (optional, override JSON settings):");
            Console.WriteLine("    DOCUMENTATOM_ENDPOINT    - Override DocumentAtom.Endpoint");
            Console.WriteLine("    DOCUMENTATOM_ACCESS_KEY  - Override DocumentAtom.AccessKey");
            Console.WriteLine();
        }

        /// <summary>
        /// Maps DocumentAtom.Sdk.SeverityEnum to SyslogLogging.Severity.
        /// </summary>
        /// <param name="sdkSeverity">SDK severity.</param>
        /// <returns>Syslog severity.</returns>
        private static Severity MapSdkSeverityToSyslog(DocumentAtom.Core.Enums.SeverityEnum sdkSeverity)
        {
            return sdkSeverity switch
            {
                DocumentAtom.Core.Enums.SeverityEnum.Debug => Severity.Debug,
                DocumentAtom.Core.Enums.SeverityEnum.Info => Severity.Info,
                DocumentAtom.Core.Enums.SeverityEnum.Warn => Severity.Warn,
                DocumentAtom.Core.Enums.SeverityEnum.Error => Severity.Error,
                DocumentAtom.Core.Enums.SeverityEnum.Alert => Severity.Alert,
                DocumentAtom.Core.Enums.SeverityEnum.Critical => Severity.Critical,
                DocumentAtom.Core.Enums.SeverityEnum.Emergency => Severity.Emergency,
                _ => Severity.Debug
            };
        }

        #endregion
    }
}
