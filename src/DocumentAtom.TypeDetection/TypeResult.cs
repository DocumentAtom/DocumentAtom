namespace DocumentAtom.TypeDetection
{
    /// <summary>
    /// Type detection results.
    /// </summary>
    public class TypeResult
    {
        #region Public-Members

        /// <summary>
        /// MIME type.
        /// </summary>
        public string MimeType { get; set; } = null;

        /// <summary>
        /// Extension.
        /// </summary>
        public string Extension { get; set; } = null;

        /// <summary>
        /// Data type.
        /// </summary>
        public DocumentTypeEnum Type { get; set; } = DocumentTypeEnum.Unknown;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Instantiate.
        /// </summary>
        public TypeResult()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
