namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Runtime.Loader;
    using System.Text;
    using System.Threading;
    using SerializationHelper;
    using SyslogLogging;
    using WatsonWebserver;
    using System.Text.Json;
    using System.Text.Json.Serialization;

    /// <summary>
    /// DocumentAtom server.
    /// </summary>
    internal static class ServerRuntime
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
        private static ServerRuntimeContext _Context = null;
        private static ServerRouteHandlers _RouteHandlers = null;
        private static AtomRequestProcessor _AtomRequestProcessor = null;
        private static TextAtomRouteHandlers _TextAtomRoutes = null;
        private static DocumentAtomRouteHandlers _DocumentAtomRoutes = null;
        private static ImageAtomRouteHandlers _ImageAtomRoutes = null;

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
        public static void Run(string[] args)
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

            _Context = new ServerRuntimeContext(
                _Header,
                _Serializer,
                _Settings,
                _Logging,
                _JsonOptions);

            _RouteHandlers = new ServerRouteHandlers(_Context);
            _AtomRequestProcessor = new AtomRequestProcessor(_Context);
            _TextAtomRoutes = new TextAtomRouteHandlers(_Context, _AtomRequestProcessor);
            _DocumentAtomRoutes = new DocumentAtomRouteHandlers(_Context, _AtomRequestProcessor);
            _ImageAtomRoutes = new ImageAtomRouteHandlers(_Context, _AtomRequestProcessor);

            _RouteHandlers.ApplyCorsSettingsToWebserverDefaults();

            _RestServer = new Webserver(_Settings.Webserver, _RouteHandlers.DefaultRoute);
            _RestServer.Routes.PreRouting = _RouteHandlers.PreRoutingRoute;
            _RestServer.Routes.Preflight = _RouteHandlers.PreflightRoute;
            _RestServer.Routes.PostRouting = _RouteHandlers.PostRoutingRoute;

            OpenApiRouteRegistrar.RegisterRoutes(
                _RestServer,
                _RouteHandlers,
                _TextAtomRoutes,
                _DocumentAtomRoutes,
                _ImageAtomRoutes);
            OpenApiRouteRegistrar.ConfigureOpenApi(_RestServer);

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

        #endregion

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}

