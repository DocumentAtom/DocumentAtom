namespace DocumentAtom.Xml
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for XML processor.
    /// </summary>
    public class XmlProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on XML element nesting.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        /// <summary>
        /// Maximum depth for XML traversal.
        /// Default is 100.
        /// Minimum value is 1.
        /// </summary>
        public int MaxDepth
        {
            get
            {
                return _MaxDepth;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaxDepth));
                _MaxDepth = value;
            }
        }

        /// <summary>
        /// Include XML attributes as part of the text content.
        /// Default is true.
        /// </summary>
        public bool IncludeAttributes { get; set; } = true;

        /// <summary>
        /// Preserve whitespace in XML content.
        /// Default is false.
        /// </summary>
        public bool PreserveWhitespace { get; set; } = false;

        #endregion

        #region Private-Members

        private int _MaxDepth = 100;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for XML processor.
        /// </summary>
        public XmlProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
