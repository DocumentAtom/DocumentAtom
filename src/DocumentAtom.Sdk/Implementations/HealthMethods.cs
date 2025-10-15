namespace DocumentAtom.Sdk.Implementations
{
    using DocumentAtom.Sdk.Interfaces;

    /// <summary>
    /// Implementation of health check methods.
    /// </summary>
    public class HealthMethods : IHealthMethods
    {
        #region Private-Members

        private readonly DocumentAtomSdk _Sdk;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the health methods implementation.
        /// </summary>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        public HealthMethods(DocumentAtomSdk sdk)
        {
            _Sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
        }

        #endregion

        #region Public-Methods

        /// <inheritdoc />
        public async Task<bool> IsHealthy(CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/";
            return await _Sdk.GetSuccessAsync(url, cancellationToken).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string?> GetStatus(CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/";
            return await _Sdk.GetAsync(url, cancellationToken).ConfigureAwait(false);
        }

        #endregion
    }
}