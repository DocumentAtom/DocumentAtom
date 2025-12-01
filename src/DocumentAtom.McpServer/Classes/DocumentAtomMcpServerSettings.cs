namespace DocumentAtom.McpServer.Classes
{
    using System;

    /// <summary>
    /// DocumentAtom MCP Server settings.
    /// </summary>
    public class DocumentAtomMcpServerSettings
    {
        #region Public-Members

        /// <summary>
        /// Logging settings.
        /// </summary>
        public LoggingSettings Logging
        {
            get
            {
                return _Logging;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Logging));
                _Logging = value;
            }
        }

        /// <summary>
        /// Tesseract settings.
        /// </summary>
        public TesseractSettings Tesseract
        {
            get
            {
                return _Tesseract;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Tesseract));
                _Tesseract = value;
            }
        }

        /// <summary>
        /// DocumentAtom connection settings.
        /// </summary>
        public DocumentAtomSettings DocumentAtom { get; set; } = new DocumentAtomSettings();

        /// <summary>
        /// HTTP server settings.
        /// </summary>
        public HttpServerSettings Http { get; set; } = new HttpServerSettings();

        /// <summary>
        /// TCP server settings.
        /// </summary>
        public TcpServerSettings Tcp { get; set; } = new TcpServerSettings();

        /// <summary>
        /// WebSocket server settings.
        /// </summary>
        public WebSocketServerSettings WebSocket { get; set; } = new WebSocketServerSettings();

        /// <summary>
        /// Storage settings.
        /// </summary>
        public StorageSettings Storage { get; set; } = new StorageSettings();

        /// <summary>
        /// Debug settings.
        /// </summary>
        public DebugSettings Debug { get; set; } = new DebugSettings();

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private TesseractSettings _Tesseract = new TesseractSettings();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAtomMcpServerSettings"/> class.
        /// </summary>
        public DocumentAtomMcpServerSettings()
        {
        }

        #endregion
    }
}
