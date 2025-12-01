using System;

namespace DocumentAtom.McpServer.Classes
{
    /// <summary>
    /// WebSocket server settings.
    /// </summary>
    public class WebSocketServerSettings
    {
        #region Public-Members

        /// <summary>
        /// WebSocket server hostname.
        /// </summary>
        public string Hostname { get; set; } = "127.0.0.1";

        /// <summary>
        /// WebSocket server port.
        /// </summary>
        public int Port
        {
            get
            {
                return _Port;
            }
            set
            {
                if (value < 0 || value > 65535) throw new ArgumentOutOfRangeException(nameof(Port));
                _Port = value;
            }
        }

        #endregion

        #region Private-Members

        private int _Port = 8202;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initializes a new instance of the <see cref="WebSocketServerSettings"/> class.
        /// </summary>
        public WebSocketServerSettings()
        {
        }

        #endregion
    }
}
