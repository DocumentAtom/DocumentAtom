namespace DocumentAtom.Text.Html
{
    using System;

    /// <summary>
    /// Settings for the HTML processor.
    /// </summary>
    public class HtmlProcessorSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable debug logging.
        /// </summary>
        public bool DebugLogging { get; set; } = false;

        /// <summary>
        /// Process inline styles as separate atoms.
        /// </summary>
        public bool ProcessInlineStyles { get; set; } = false;

        /// <summary>
        /// Process meta tags as separate atoms.
        /// </summary>
        public bool ProcessMetaTags { get; set; } = false;

        /// <summary>
        /// Process script content as code atoms.
        /// </summary>
        public bool ProcessScripts { get; set; } = false;

        /// <summary>
        /// Process comments as text atoms.
        /// </summary>
        public bool ProcessComments { get; set; } = false;

        /// <summary>
        /// Preserve whitespace in pre and code elements.
        /// </summary>
        public bool PreserveWhitespace { get; set; } = true;

        /// <summary>
        /// Maximum text length per atom. 0 for unlimited.
        /// </summary>
        public int MaxTextLength { get; set; } = 0;

        /// <summary>
        /// Process embedded SVG elements as images.
        /// </summary>
        public bool ProcessSvg { get; set; } = true;

        /// <summary>
        /// Extract data attributes from HTML elements.
        /// </summary>
        public bool ExtractDataAttributes { get; set; } = false;

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on heading levels.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate a new HtmlProcessorSettings object.
        /// </summary>
        public HtmlProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}