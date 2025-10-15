namespace DocumentAtom.Sdk.Interfaces
{
    using DocumentAtom.TypeDetection;

    /// <summary>
    /// Interface for type detection methods.
    /// </summary>
    public interface ITypeDetectionMethods
    {
        /// <summary>
        /// Detect the type of a document based on its content.
        /// </summary>
        /// <param name="data">Document data as byte array.</param>
        /// <param name="contentType">Optional content type hint.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Type detection result.</returns>
        Task<TypeResult?> DetectType(byte[] data, string? contentType = null, CancellationToken cancellationToken = default);
    }
}