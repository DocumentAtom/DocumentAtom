namespace DocumentAtom.Server
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// Constants.
    /// </summary>
    public static class Constants
    {
        #region General

        /// <summary>
        /// Logo.  See https://patorjk.com/software/taag/#p=testall&f=Small&t=DocumentAtom
        /// </summary>
        public static string Logo =
            @"  ___                             _     _  _              " + Environment.NewLine +
            @" |   \ ___  __ _  _ _ __  ___ _ _| |_  / \| |_ ___ _ __   " + Environment.NewLine +
            @" |  ) / _ \/ _| || | '  \/ -_) ' \  _|/ _ \  _/ _ \ '  \  " + Environment.NewLine +
            @" |___/\___/\__|\_,_|_|_|_\___|_||_\__/_/ \_\__\___/_|_|_| " + Environment.NewLine;

        /// <summary>
        /// Timestamp format.
        /// </summary>
        public static string TimestampFormat = "yyyy-MM-ddTHH:mm:ss.ffffffZ";

        #endregion

        #region Product-Names

        /// <summary>
        /// Config server product name.
        /// </summary>
        public static string ProductName = "DocumentAtom Server";

        #endregion

        #region Node

        /// <summary>
        /// Settings file.
        /// </summary>
        public static string SettingsFile = "./documentatom.json";

        /// <summary>
        /// Log filename.
        /// </summary>
        public static string LogFilename = "./documentatom.log";

        /// <summary>
        /// Log directory.
        /// </summary>
        public static string LogDirectory = "./logs/";

        /// <summary>
        /// Copyright.
        /// </summary>
        public static string Copyright = "(c)2025 Joel Christner";

        #endregion

        #region Content-Types

        /// <summary>
        /// Binary content type.
        /// </summary>
        public static string BinaryContentType = "application/octet-stream";

        /// <summary>
        /// JSON content type.
        /// </summary>
        public static string JsonContentType = "application/json";

        /// <summary>
        /// HTML content type.
        /// </summary>
        public static string HtmlContentType = "text/html";

        /// <summary>
        /// PNG content type.
        /// </summary>
        public static string PngContentType = "image/png";

        /// <summary>
        /// Text content type.
        /// </summary>
        public static string TextContentType = "text/plain";

        /// <summary>
        /// Favicon filename.
        /// </summary>
        public static string FaviconFilename = "assets/favicon.png";

        /// <summary>
        /// Favicon content type.
        /// </summary>
        public static string FaviconContentType = "image/png";

        /// <summary>
        /// Default GUID.
        /// </summary>
        public static string DefaultGUID = default(Guid).ToString();

        #endregion

        #region Default-Homepage

        /// <summary>
        /// Default HTML homepage.
        /// </summary>
        public static string HtmlHomepage =
            @"<html>" + Environment.NewLine +
            @"  <head>" + Environment.NewLine +
            @"    <title>Node is Operational</title>" + Environment.NewLine +
            @"  </head>" + Environment.NewLine +
            @"  <body>" + Environment.NewLine +
            @"    <div>" + Environment.NewLine +
            @"      <pre>" + Environment.NewLine + Environment.NewLine +
            Logo + Environment.NewLine +
            @"      </pre>" + Environment.NewLine +
            @"    </div>" + Environment.NewLine +
            @"    <div style='font-family: Arial, sans-serif;'>" + Environment.NewLine +
            @"      <h2>Your node is operational</h2>" + Environment.NewLine +
            @"      <p>Congratulations, your node is operational.  Please refer to the documentation for use.</p>" + Environment.NewLine +
            @"    <div>" + Environment.NewLine +
            @"  </body>" + Environment.NewLine +
            @"</html>" + Environment.NewLine;

        #endregion

        #region Querystring

        /// <summary>
        /// Querystring value for OCR.
        /// </summary>
        public static string OcrQuerystring = "ocr";

        #endregion
    }
}
