namespace DocumentAtom.DataIngestion.Readers
{
    using DocumentAtom.Core;
    using DocumentAtom.TypeDetection;

    /// <summary>
    /// Factory interface for creating DocumentAtom processors based on document type.
    /// </summary>
    public interface IProcessorFactory
    {
        /// <summary>
        /// Create a processor for the specified document type.
        /// </summary>
        /// <param name="documentType">The document type.</param>
        /// <param name="path">Optional path to the document.</param>
        /// <returns>A processor instance, or null if the type is not supported.</returns>
        ProcessorBase? CreateProcessor(DocumentTypeEnum documentType, string? path = null);
    }
}
