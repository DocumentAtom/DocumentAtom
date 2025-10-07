namespace DocumentAtom.Json
{
    using DocumentAtom.Core;
    using System.Text.Json;

    /// <summary>
    /// Settings for JSON processor.
    /// </summary>
    public class JsonProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// JSON document options.
        /// </summary>
        public JsonDocumentOptions JsonOptions
        {
            get
            {
                return _JsonOptions;
            }
            set
            {
                _JsonOptions = value;
            }
        }

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on JSON object/array nesting.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        /// <summary>
        /// Maximum depth for JSON traversal.
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

        #endregion

        #region Private-Members

        private JsonDocumentOptions _JsonOptions = new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
            MaxDepth = 100
        };

        private int _MaxDepth = 100;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for JSON processor.
        /// </summary>
        public JsonProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
