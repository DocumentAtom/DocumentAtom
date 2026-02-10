namespace DocumentAtom.Sdk
{
    using System.Text.Json;
    using System.Text.Json.Serialization;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Sdk.Implementations;
    using DocumentAtom.Sdk.Interfaces;
    using RestWrapper;

    /// <summary>
    /// DocumentAtom SDK for interacting with DocumentAtom server.
    /// </summary>
    public class DocumentAtomSdk : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable logging of request bodies.
        /// </summary>
        public bool LogRequests { get; set; } = false;

        /// <summary>
        /// Enable or disable logging of response bodies.
        /// </summary>
        public bool LogResponses { get; set; } = false;

        /// <summary>
        /// Method to invoke to send log messages.
        /// </summary>
        public Action<SeverityEnum, string>? Logger { get; set; }

        /// <summary>
        /// Endpoint URL for the DocumentAtom server.
        /// </summary>
        public string Endpoint { get; set; } = string.Empty;

        /// <summary>
        /// Timeout in milliseconds for HTTP requests.
        /// </summary>
        public int TimeoutMs { get; set; } = 300000; // 5 minutes

        /// <summary>
        /// Access key for authentication (if required).
        /// </summary>
        public string? AccessKey { get; set; }

        /// <summary>
        /// Document atomization methods.
        /// </summary>
        public IAtomMethods Atom { get; private set; }

        /// <summary>
        /// Type detection methods.
        /// </summary>
        public ITypeDetectionMethods TypeDetection { get; private set; }

        /// <summary>
        /// Health check methods.
        /// </summary>
        public IHealthMethods Health { get; private set; }

        #endregion

        #region Private-Members

        private bool _Disposed = false;
        private readonly JsonSerializerOptions _JsonOptions;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initialize the DocumentAtom SDK.
        /// </summary>
        /// <param name="endpoint">DocumentAtom server endpoint URL.</param>
        /// <param name="accessKey">Optional access key for authentication.</param>
        public DocumentAtomSdk(string endpoint, string? accessKey = null)
        {
            if (string.IsNullOrEmpty(endpoint))
                throw new ArgumentNullException(nameof(endpoint));

            Endpoint = endpoint.TrimEnd('/');
            AccessKey = accessKey;

            _JsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = true
            };
            _JsonOptions.Converters.Add(new JsonStringEnumConverter());

            Atom = new AtomMethods(this);
            TypeDetection = new TypeDetectionMethods(this);
            Health = new HealthMethods(this);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose of the SDK and clean up resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose of the SDK and clean up resources.
        /// </summary>
        /// <param name="disposing">True if disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
                _Disposed = true;
        }

        /// <summary>
        /// Log a message using the configured logger.
        /// </summary>
        /// <param name="severity">Log severity level.</param>
        /// <param name="message">Message to log.</param>
        public void Log(SeverityEnum severity, string message)
        {
            if (!string.IsNullOrEmpty(message))
                Logger?.Invoke(severity, message);
        }

        /// <summary>
        /// Send a POST request with binary data and return typed response.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="url">Full URL to send request to.</param>
        /// <param name="data">Binary data to send.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized response of type T.</returns>
        public async Task<T?> PostAsync<T>(string url, byte[] data, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            using (RestRequest req = new RestRequest(url, HttpMethod.Post))
            {
                req.TimeoutMilliseconds = TimeoutMs;
                req.ContentType = "application/octet-stream";

                if (!string.IsNullOrEmpty(AccessKey))
                    req.Authorization.BearerToken = AccessKey;

                if (LogRequests)
                    Log(SeverityEnum.Debug, $"POST request to {url} with {data.Length} bytes");

                using (RestResponse resp = await req.SendAsync(data, cancellationToken).ConfigureAwait(false))
                {
                    if (resp != null)
                    {
                        string? responseData = await ReadResponse(resp, url, cancellationToken).ConfigureAwait(false);

                        if (LogResponses)
                            Log(SeverityEnum.Debug, $"Response from {url} (status {resp.StatusCode}): {responseData}");

                        if (resp.StatusCode >= 200 && resp.StatusCode <= 299)
                        {
                            Log(SeverityEnum.Debug, $"Success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");

                            if (!string.IsNullOrEmpty(responseData))
                            {
                                Log(SeverityEnum.Debug, "Deserializing response body");
                                return JsonSerializer.Deserialize<T>(responseData, _JsonOptions);
                            }
                            else
                            {
                                Log(SeverityEnum.Debug, "Empty response body, returning null");
                                return default(T);
                            }
                        }
                        else
                        {
                            Log(SeverityEnum.Warn, $"Non-success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");
                            return default(T);
                        }
                    }
                    else
                    {
                        Log(SeverityEnum.Warn, $"No response from {url}");
                        return default(T);
                    }
                }
            }
        }

        /// <summary>
        /// Send a GET request and return typed response.
        /// </summary>
        /// <typeparam name="T">Response type.</typeparam>
        /// <param name="url">Full URL to send request to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Deserialized response of type T.</returns>
        public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            using (RestRequest req = new RestRequest(url, HttpMethod.Get))
            {
                req.TimeoutMilliseconds = TimeoutMs;

                if (!string.IsNullOrEmpty(AccessKey))
                    req.Authorization.BearerToken = AccessKey;

                if (LogRequests)
                    Log(SeverityEnum.Debug, $"GET request to {url}");

                using (RestResponse resp = await req.SendAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (resp != null)
                    {
                        string? responseData = await ReadResponse(resp, url, cancellationToken).ConfigureAwait(false);

                        if (LogResponses)
                            Log(SeverityEnum.Debug, $"Response from {url} (status {resp.StatusCode}): {responseData}");

                        if (resp.StatusCode >= 200 && resp.StatusCode <= 299)
                        {
                            Log(SeverityEnum.Debug, $"Success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");

                            if (!string.IsNullOrEmpty(responseData))
                            {
                                Log(SeverityEnum.Debug, "Deserializing response body");
                                return JsonSerializer.Deserialize<T>(responseData, _JsonOptions);
                            }
                            else
                            {
                                Log(SeverityEnum.Debug, "Empty response body, returning null");
                                return default(T);
                            }
                        }
                        else
                        {
                            Log(SeverityEnum.Warn, $"Non-success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");
                            return default(T);
                        }
                    }
                    else
                    {
                        Log(SeverityEnum.Warn, $"No response from {url}");
                        return default(T);
                    }
                }
            }
        }

        /// <summary>
        /// Send a GET request and return string response.
        /// </summary>
        /// <param name="url">Full URL to send request to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Response as string.</returns>
        public async Task<string?> GetAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            using (RestRequest req = new RestRequest(url, HttpMethod.Get))
            {
                req.TimeoutMilliseconds = TimeoutMs;

                if (!string.IsNullOrEmpty(AccessKey))
                    req.Authorization.BearerToken = AccessKey;

                if (LogRequests)
                    Log(SeverityEnum.Debug, $"GET request to {url}");

                using (RestResponse resp = await req.SendAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (resp != null)
                    {
                        string? responseData = await ReadResponse(resp, url, cancellationToken).ConfigureAwait(false);

                        if (LogResponses)
                            Log(SeverityEnum.Debug, $"Response from {url} (status {resp.StatusCode}): {responseData}");

                        if (resp.StatusCode >= 200 && resp.StatusCode <= 299)
                        {
                            Log(SeverityEnum.Debug, $"Success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");
                            return responseData;
                        }
                        else
                        {
                            Log(SeverityEnum.Warn, $"Non-success from {url}: {resp.StatusCode}, {resp.ContentLength} bytes");
                            return null;
                        }
                    }
                    else
                    {
                        Log(SeverityEnum.Warn, $"No response from {url}");
                        return null;
                    }
                }
            }
        }

        /// <summary>
        /// Send a GET request and return success status.
        /// </summary>
        /// <param name="url">Full URL to send request to.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>True if successful (2xx status code).</returns>
        public async Task<bool> GetSuccessAsync(string url, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(url))
                throw new ArgumentNullException(nameof(url));

            using (RestRequest req = new RestRequest(url, HttpMethod.Get))
            {
                req.TimeoutMilliseconds = TimeoutMs;

                if (!string.IsNullOrEmpty(AccessKey))
                    req.Authorization.BearerToken = AccessKey;

                if (LogRequests)
                    Log(SeverityEnum.Debug, $"GET request to {url}");

                using (RestResponse resp = await req.SendAsync(cancellationToken).ConfigureAwait(false))
                {
                    if (resp != null)
                    {
                        if (LogResponses)
                            Log(SeverityEnum.Debug, $"Response from {url} (status {resp.StatusCode})");

                        if (resp.StatusCode >= 200 && resp.StatusCode <= 299)
                        {
                            Log(SeverityEnum.Debug, $"Success from {url}: {resp.StatusCode}");
                            return true;
                        }
                        else
                        {
                            Log(SeverityEnum.Warn, $"Non-success from {url}: {resp.StatusCode}");
                            return false;
                        }
                    }
                    else
                    {
                        Log(SeverityEnum.Warn, $"No response from {url}");
                        return false;
                    }
                }
            }
        }

        /// <summary>
        /// Read response data from RestResponse, handling both chunked and non-chunked responses.
        /// </summary>
        /// <param name="resp">REST response.</param>
        /// <param name="url">URL from the request, useful for logging.</param>
        /// <param name="token">Cancellation token.</param>
        /// <returns>Response data as string.</returns>
        public async Task<string?> ReadResponse(RestResponse resp, string url, CancellationToken token = default)
        {
            if (resp == null) return null;

            string str = string.Empty;

            if (resp.ChunkedTransferEncoding)
            {
                Log(SeverityEnum.Debug, "reading chunked response from " + url);

                List<string> chunks = new List<string>();
                ChunkData? chunk = null;

                while ((chunk = await resp.ReadChunkAsync(token).ConfigureAwait(false)) != null)
                {
                    if (chunk.Data != null && chunk.Data.Length > 0)
                    {
                        chunks.Add(System.Text.Encoding.UTF8.GetString(chunk.Data));
                    }
                    if (chunk.IsFinal) break;
                }

                str = string.Join("", chunks);
            }
            else
            {
                str = resp.DataAsString;
            }

            return str;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}