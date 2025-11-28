namespace DocumentAtom.McpServer.Classes
{
    /// <summary>
    /// DocumentAtom connection settings.
    /// </summary>
    public class DocumentAtomSettings
    {
        #region Public-Members

        /// <summary>
        /// REST API endpoint URL for remote DocumentAtom server.
        /// </summary>
        public string? Endpoint { get; set; } = "http://localhost:8000";

        /// <summary>
        /// Access key for authentication with DocumentAtom server.
        /// </summary>
        public string? AccessKey { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentAtomSettings"/> class.
        /// </summary>
        public DocumentAtomSettings()
        {
        }

        #endregion
    }
}
