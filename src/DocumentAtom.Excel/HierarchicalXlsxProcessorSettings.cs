namespace DocumentAtom.Excel
{
    /// <summary>
    /// Settings for hierarchical Microsoft Excel .xlsx processor.
    /// </summary>
    public class HierarchicalXlsxProcessorSettings : XlsxProcessorSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on sheet (page) grouping.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for hierarchical Microsoft Excel .xlsx processor.
        /// </summary>
        public HierarchicalXlsxProcessorSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
