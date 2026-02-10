namespace DocumentAtom.Sdk.Interfaces
{
    /// <summary>
    /// Interface for health check methods.
    /// </summary>
    public interface IHealthMethods
    {
        /// <summary>
        /// Check if the DocumentAtom server is healthy.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if the server is healthy.</returns>
        Task<bool> IsHealthy(CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the status of the DocumentAtom server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Server status as string.</returns>
        Task<string?> GetStatus(CancellationToken cancellationToken = default);
    }
}