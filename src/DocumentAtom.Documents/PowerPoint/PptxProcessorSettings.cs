namespace DocumentAtom.Documents.PowerPoint
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for Microsoft PowerPoint .pptx processor.
    /// </summary>
    public class PptxProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on slide (page) grouping.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for Microsoft PowerPoint .pptx processor.
        /// </summary>
        public PptxProcessorSettings() 
        { 

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
