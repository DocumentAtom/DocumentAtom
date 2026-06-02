namespace DocumentAtom.Server
{
    /// <summary>
    /// Cross-origin resource sharing settings.
    /// </summary>
    public class CorsSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable CORS response and preflight headers.
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Allowed origin value.
        /// </summary>
        public string AllowOrigin { get; set; } = "*";

        /// <summary>
        /// Allowed HTTP methods.
        /// </summary>
        public List<string> AllowMethods { get; set; } = new List<string>
        {
            "OPTIONS",
            "HEAD",
            "GET",
            "POST"
        };

        /// <summary>
        /// Allowed request headers.
        /// </summary>
        public List<string> AllowHeaders { get; set; } = new List<string>
        {
            "*",
            "Content-Type",
            "X-Requested-With"
        };

        /// <summary>
        /// Exposed response headers.
        /// </summary>
        public List<string> ExposeHeaders { get; set; } = new List<string>
        {
            "Content-Type",
            "X-Requested-With"
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Cross-origin resource sharing settings.
        /// </summary>
        public CorsSettings()
        {

        }

        #endregion
    }
}
