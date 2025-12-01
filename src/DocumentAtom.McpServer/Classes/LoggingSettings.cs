namespace DocumentAtom.McpServer.Classes
{
    using SyslogLogging;
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Logging settings.
    /// </summary>
    public class LoggingSettings
    {
        /// <summary>
        /// List of syslog servers.
        /// </summary>
        public List<SyslogServer> Servers
        {
            get
            {
                return _Servers;
            }
            set
            {
                if (value == null) _Servers = new List<SyslogServer>();
                else _Servers = value;
            }
        }

        /// <summary>
        /// Log directory.
        /// </summary>
        public string LogDirectory { get; set; } = "./logs/";

        /// <summary>
        /// Log filename.
        /// </summary>
        public string LogFilename { get; set; } = "documentatom-mcp.log";

        /// <summary>
        /// Enable or disable console logging.
        /// </summary>
        public bool ConsoleLogging { get; set; } = true;

        /// <summary>
        /// Enable colors in logging.
        /// </summary>
        public bool EnableColors { get; set; } = true;

        /// <summary>
        /// Minimum severity.
        /// </summary>
        public int MinimumSeverity
        {
            get
            {
                return _MinimumSeverity;
            }
            set
            {
                if (value < 0 || value > 7) throw new ArgumentOutOfRangeException(nameof(MinimumSeverity));
                _MinimumSeverity = value;
            }
        }

        private int _MinimumSeverity = 0;
        private List<SyslogServer> _Servers = new List<SyslogServer>
        {
            new SyslogServer("127.0.0.1", 514)
        };

        /// <summary>
        /// Instantiate.
        /// </summary>
        public LoggingSettings()
        {

        }
    }
}
