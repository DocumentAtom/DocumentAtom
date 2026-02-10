namespace DocumentAtom.Documents.Word
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for Microsoft Word .docx processor.
    /// </summary>
    public class DocxProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on heading styles (Heading 1, Heading 2, etc.).
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for Microsoft Word .docx processor.
        /// </summary>
        public DocxProcessorSettings() 
        { 

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
