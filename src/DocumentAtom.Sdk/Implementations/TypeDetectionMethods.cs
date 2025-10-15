namespace DocumentAtom.Sdk.Implementations
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Sdk.Interfaces;
    using DocumentAtom.TypeDetection;

    /// <summary>
    /// Implementation of type detection methods.
    /// </summary>
    public class TypeDetectionMethods : ITypeDetectionMethods
    {
        #region Private-Members

        private readonly DocumentAtomSdk _Sdk;

        #endregion

        #region Constructors

        /// <summary>
        /// Initialize the type detection methods implementation.
        /// </summary>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        public TypeDetectionMethods(DocumentAtomSdk sdk)
        {
            _Sdk = sdk ?? throw new ArgumentNullException(nameof(sdk));
        }

        #endregion

        #region Public-Methods

        /// <inheritdoc />
        public async Task<TypeResult?> DetectType(byte[] data, string? contentType = null, CancellationToken cancellationToken = default)
        {
            string url = _Sdk.Endpoint + "/typedetect";

            using (RestWrapper.RestRequest req = new RestWrapper.RestRequest(url, HttpMethod.Post))
            {
                req.TimeoutMilliseconds = _Sdk.TimeoutMs;
                req.ContentType = contentType ?? "application/octet-stream";

                if (!string.IsNullOrEmpty(_Sdk.AccessKey))
                {
                    req.Authorization.BearerToken = _Sdk.AccessKey;
                }

                if (_Sdk.LogRequests)
                    _Sdk.Log(SeverityEnum.Debug, $"POST request to {url} with {data.Length} bytes");

                using (RestWrapper.RestResponse resp = await req.SendAsync(data, cancellationToken).ConfigureAwait(false))
                {
                    if (resp != null)
                    {
                        string? responseData = await _Sdk.ReadResponse(resp, url, cancellationToken).ConfigureAwait(false);

                        if (_Sdk.LogResponses)
                            _Sdk.Log(SeverityEnum.Debug, $"Response from {url} (status {resp.StatusCode}): {responseData}");

                        if (resp.StatusCode >= 200 && resp.StatusCode <= 299)
                        {
                            _Sdk.Log(SeverityEnum.Debug, $"Success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");

                            if (!string.IsNullOrEmpty(responseData))
                            {
                                _Sdk.Log(SeverityEnum.Debug, "Deserializing response body");
                                try
                                {
                                    JsonSerializerOptions options = new JsonSerializerOptions
                                    {
                                        PropertyNameCaseInsensitive = true
                                    };
                                    options.Converters.Add(new JsonStringEnumConverter());
                                    return System.Text.Json.JsonSerializer.Deserialize<TypeResult>(responseData, options);
                                }
                                catch (JsonException ex)
                                {
                                    _Sdk.Log(SeverityEnum.Error, $"JSON deserialization error: {ex.Message}");
                                    _Sdk.Log(SeverityEnum.Error, $"Raw response data: {responseData}");
                                    return null;
                                }
                            }
                            else
                            {
                                _Sdk.Log(SeverityEnum.Debug, "Empty response body, returning null");
                                return null;
                            }
                        }
                        else
                        {
                            _Sdk.Log(SeverityEnum.Warn, $"Non-success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");
                            return null;
                        }
                    }
                    else
                    {
                        _Sdk.Log(SeverityEnum.Warn, $"No response from {url}");
                        return null;
                    }
                }
            }
        }

        #endregion
    }
}