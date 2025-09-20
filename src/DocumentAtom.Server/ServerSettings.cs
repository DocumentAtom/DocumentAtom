namespace DocumentAtom.Server
{
    using System;
    using WatsonWebserver.Core;

    /// <summary>
    /// DocumentAtom server settings.
    /// </summary>
    public class ServerSettings
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
        /// Webserver settings.
        /// </summary>
        public WebserverSettings Webserver
        {
            get
            {
                return _Webserver;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Webserver));
                _Webserver = value;
            }
        }

        #endregion

        #region Private-Members

        private LoggingSettings _Logging = new LoggingSettings();
        private TesseractSettings _Tesseract = new TesseractSettings();
        private WebserverSettings _Webserver = new WebserverSettings("localhost", 8000, false);

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// DocumentAtom server settings.
        /// </summary>
        public ServerSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
