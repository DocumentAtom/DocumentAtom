namespace DocumentAtom.Core.Diagnostics
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;

    /// <summary>
    /// Shared telemetry instruments for DocumentAtom. The source names are the stable contract that
    /// an observing host subscribes to with OpenTelemetry, Prometheus exporters, or another listener.
    /// </summary>
    public static class DocumentAtomDiagnostics
    {
        #region Public-Members

        /// <summary>
        /// DocumentAtom telemetry version.
        /// </summary>
        public const string Version = "3.1.1";

        /// <summary>
        /// Meter and activity source name for core parsing operations.
        /// </summary>
        public const string CoreSourceName = "DocumentAtom.Core";

        /// <summary>
        /// Meter and activity source name for the REST server.
        /// </summary>
        public const string ServerSourceName = "DocumentAtom.Server";

        /// <summary>
        /// Meter and activity source name for the MCP server.
        /// </summary>
        public const string McpServerSourceName = "DocumentAtom.McpServer";

        /// <summary>
        /// Meter and activity source name for the C# SDK.
        /// </summary>
        public const string SdkSourceName = "DocumentAtom.Sdk";

        /// <summary>
        /// Meter and activity source name for data ingestion operations.
        /// </summary>
        public const string DataIngestionSourceName = "DocumentAtom.DataIngestion";

        /// <summary>
        /// Core parser meter.
        /// </summary>
        public static readonly Meter CoreMeter = new Meter(CoreSourceName, Version);

        /// <summary>
        /// Core parser activity source.
        /// </summary>
        public static readonly ActivitySource CoreActivitySource = new ActivitySource(CoreSourceName, Version);

        /// <summary>
        /// REST server meter.
        /// </summary>
        public static readonly Meter ServerMeter = new Meter(ServerSourceName, Version);

        /// <summary>
        /// REST server activity source.
        /// </summary>
        public static readonly ActivitySource ServerActivitySource = new ActivitySource(ServerSourceName, Version);

        /// <summary>
        /// MCP server meter.
        /// </summary>
        public static readonly Meter McpServerMeter = new Meter(McpServerSourceName, Version);

        /// <summary>
        /// MCP server activity source.
        /// </summary>
        public static readonly ActivitySource McpServerActivitySource = new ActivitySource(McpServerSourceName, Version);

        /// <summary>
        /// C# SDK meter.
        /// </summary>
        public static readonly Meter SdkMeter = new Meter(SdkSourceName, Version);

        /// <summary>
        /// C# SDK activity source.
        /// </summary>
        public static readonly ActivitySource SdkActivitySource = new ActivitySource(SdkSourceName, Version);

        /// <summary>
        /// Data ingestion meter.
        /// </summary>
        public static readonly Meter DataIngestionMeter = new Meter(DataIngestionSourceName, Version);

        /// <summary>
        /// Data ingestion activity source.
        /// </summary>
        public static readonly ActivitySource DataIngestionActivitySource = new ActivitySource(DataIngestionSourceName, Version);

        #endregion

        #region Private-Members

        private const string UnitBytes = "By";
        private const string UnitSeconds = "s";
        private const string UnitRequest = "{request}";
        private const string UnitOperation = "{operation}";
        private const string UnitAtom = "{atom}";
        private const string UnitChunk = "{chunk}";
        private const string UnitDocument = "{document}";
        private const string UnitConnection = "{connection}";

        private const string AttributeOutcome = "outcome";
        private const string AttributeHttpMethod = "http.request.method";
        private const string AttributeHttpStatusCode = "http.response.status_code";
        private const string AttributeHttpRoute = "http.route";
        private const string AttributeUrlScheme = "url.scheme";
        private const string AttributeRpcSystem = "rpc.system";
        private const string AttributeRpcService = "rpc.service";
        private const string AttributeRpcMethod = "rpc.method";
        private const string AttributeNetworkTransport = "network.transport";
        private const string AttributeProcessor = "documentatom.processor";
        private const string AttributeInputKind = "documentatom.input.kind";
        private const string AttributeDocumentType = "documentatom.document.type";
        private const string AttributeAtomType = "documentatom.atom.type";
        private const string AttributeChunkStrategy = "documentatom.chunk.strategy";

        private static readonly Counter<long> _HttpServerRequests =
            ServerMeter.CreateCounter<long>("documentatom.http.server.requests", UnitRequest, "Inbound REST API requests.");

        private static readonly UpDownCounter<long> _HttpServerActiveRequests =
            ServerMeter.CreateUpDownCounter<long>("http.server.active_requests", UnitRequest, "Concurrent inbound REST API requests.");

        private static readonly Histogram<double> _HttpServerRequestDuration =
            ServerMeter.CreateHistogram<double>("http.server.request.duration", UnitSeconds, "Duration of inbound REST API requests.");

        private static readonly Histogram<long> _HttpServerRequestBodySize =
            ServerMeter.CreateHistogram<long>("http.server.request.body.size", UnitBytes, "Size of inbound REST API request bodies.");

        private static readonly Counter<long> _McpServerRequests =
            McpServerMeter.CreateCounter<long>("documentatom.mcp.server.requests", UnitRequest, "Inbound MCP RPC requests.");

        private static readonly UpDownCounter<long> _McpServerActiveRequests =
            McpServerMeter.CreateUpDownCounter<long>("documentatom.mcp.server.active_requests", UnitRequest, "Concurrent inbound MCP RPC requests.");

        private static readonly UpDownCounter<long> _McpServerConnections =
            McpServerMeter.CreateUpDownCounter<long>("documentatom.mcp.server.connections", UnitConnection, "Open MCP client connections.");

        private static readonly Histogram<double> _RpcServerDuration =
            McpServerMeter.CreateHistogram<double>("rpc.server.duration", UnitSeconds, "Duration of inbound MCP RPC calls.");

        private static readonly Counter<long> _SdkRequests =
            SdkMeter.CreateCounter<long>("documentatom.sdk.client.requests", UnitRequest, "Outbound DocumentAtom SDK HTTP requests.");

        private static readonly Histogram<double> _HttpClientRequestDuration =
            SdkMeter.CreateHistogram<double>("http.client.request.duration", UnitSeconds, "Duration of outbound DocumentAtom SDK HTTP requests.");

        private static readonly Histogram<long> _HttpClientRequestBodySize =
            SdkMeter.CreateHistogram<long>("http.client.request.body.size", UnitBytes, "Size of outbound DocumentAtom SDK HTTP request bodies.");

        private static readonly Counter<long> _ProcessorExtractions =
            CoreMeter.CreateCounter<long>("documentatom.processor.extractions", UnitOperation, "Document atom extraction operations.");

        private static readonly Histogram<double> _ProcessorExtractionDuration =
            CoreMeter.CreateHistogram<double>("documentatom.processor.extraction.duration", UnitSeconds, "Duration of document atom extraction operations.");

        private static readonly Counter<long> _ExtractedAtoms =
            CoreMeter.CreateCounter<long>("documentatom.processor.atoms", UnitAtom, "Atoms extracted by processors.");

        private static readonly Counter<long> _TypeDetections =
            CoreMeter.CreateCounter<long>("documentatom.type_detection.requests", UnitOperation, "Document type detection operations.");

        private static readonly Histogram<double> _TypeDetectionDuration =
            CoreMeter.CreateHistogram<double>("documentatom.type_detection.duration", UnitSeconds, "Duration of document type detection operations.");

        private static readonly Histogram<long> _TypeDetectionInputSize =
            CoreMeter.CreateHistogram<long>("documentatom.type_detection.input.size", UnitBytes, "Input size for document type detection operations.");

        private static readonly Counter<long> _ChunkingOperations =
            CoreMeter.CreateCounter<long>("documentatom.chunking.operations", UnitOperation, "Atom chunking operations.");

        private static readonly Histogram<double> _ChunkingDuration =
            CoreMeter.CreateHistogram<double>("documentatom.chunking.duration", UnitSeconds, "Duration of atom chunking operations.");

        private static readonly Counter<long> _Chunks =
            CoreMeter.CreateCounter<long>("documentatom.chunking.chunks", UnitChunk, "Chunks produced from atoms.");

        private static readonly Counter<long> _IngestionDocuments =
            DataIngestionMeter.CreateCounter<long>("documentatom.ingestion.documents", UnitDocument, "Documents processed for ingestion.");

        private static readonly Histogram<double> _IngestionDuration =
            DataIngestionMeter.CreateHistogram<double>("documentatom.ingestion.duration", UnitSeconds, "Duration of document ingestion operations.");

        private static readonly Counter<long> _IngestionChunks =
            DataIngestionMeter.CreateCounter<long>("documentatom.ingestion.chunks", UnitChunk, "Chunks produced for ingestion.");

        #endregion

        #region Public-Methods

        /// <summary>
        /// Calculate elapsed seconds from a <see cref="Stopwatch.GetTimestamp"/> value.
        /// </summary>
        /// <param name="startTicks">Start timestamp.</param>
        /// <returns>Elapsed seconds.</returns>
        public static double GetElapsedSeconds(long startTicks)
        {
            return (Stopwatch.GetTimestamp() - startTicks) / (double)Stopwatch.Frequency;
        }

        /// <summary>
        /// Record an exception event and error status on an activity.
        /// </summary>
        /// <param name="activity">Activity.</param>
        /// <param name="exception">Exception.</param>
        public static void RecordException(Activity? activity, Exception exception)
        {
            if (activity == null || exception == null) return;

            ActivityTagsCollection tags = new ActivityTagsCollection
            {
                ["exception.type"] = exception.GetType().FullName,
                ["exception.message"] = exception.Message
            };

            if (!String.IsNullOrEmpty(exception.StackTrace))
                tags["exception.stacktrace"] = exception.StackTrace;

            activity.AddEvent(new ActivityEvent("exception", default(DateTimeOffset), tags));
            activity.SetStatus(ActivityStatusCode.Error, exception.Message);
        }

        /// <summary>
        /// Start a REST server span.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="route">Matched route.</param>
        /// <param name="scheme">URL scheme.</param>
        /// <returns>Activity, or null when no listener is sampling.</returns>
        public static Activity? StartHttpServerActivity(string? method, string? route, string? scheme)
        {
            string normalizedMethod = Normalize(method, "UNKNOWN");
            string normalizedRoute = Normalize(route, "unknown");

            Activity? activity = ServerActivitySource.StartActivity(
                "HTTP " + normalizedMethod + " " + normalizedRoute,
                ActivityKind.Server);

            if (activity != null)
            {
                activity.SetTag(AttributeHttpMethod, normalizedMethod);
                activity.SetTag(AttributeHttpRoute, normalizedRoute);
                activity.SetTag(AttributeUrlScheme, Normalize(scheme, "http"));
            }

            return activity;
        }

        /// <summary>
        /// Track active REST server requests.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="scheme">URL scheme.</param>
        /// <param name="delta">Delta.</param>
        public static void AddHttpServerActiveRequest(string? method, string? scheme, long delta)
        {
            TagList tags = new TagList
            {
                { AttributeHttpMethod, Normalize(method, "UNKNOWN") },
                { AttributeUrlScheme, Normalize(scheme, "http") }
            };

            _HttpServerActiveRequests.Add(delta, tags);
        }

        /// <summary>
        /// Record a completed REST server request.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="route">Matched route.</param>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="requestBodyBytes">Request body bytes.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordHttpServerRequest(
            string? method,
            string? route,
            int statusCode,
            long requestBodyBytes,
            double elapsedSeconds)
        {
            string normalizedMethod = Normalize(method, "UNKNOWN");
            string normalizedRoute = Normalize(route, "unknown");
            string outcome = OutcomeFromStatusCode(statusCode);

            TagList requestTags = new TagList
            {
                { AttributeHttpMethod, normalizedMethod },
                { AttributeHttpRoute, normalizedRoute },
                { AttributeHttpStatusCode, statusCode },
                { AttributeOutcome, outcome }
            };

            _HttpServerRequests.Add(1, requestTags);
            _HttpServerRequestDuration.Record(elapsedSeconds, requestTags);

            if (requestBodyBytes >= 0)
            {
                TagList sizeTags = new TagList
                {
                    { AttributeHttpMethod, normalizedMethod },
                    { AttributeHttpRoute, normalizedRoute }
                };
                _HttpServerRequestBodySize.Record(requestBodyBytes, sizeTags);
            }
        }

        /// <summary>
        /// Start a detached MCP server span. The span is not installed as Activity.Current.
        /// </summary>
        /// <param name="method">RPC method.</param>
        /// <param name="transport">Transport.</param>
        /// <returns>Activity, or null when no listener is sampling.</returns>
        public static Activity? StartMcpServerActivity(string? method, string? transport)
        {
            Activity? previous = Activity.Current;

            string normalizedMethod = Normalize(method, "unknown");
            Activity? activity = McpServerActivitySource.StartActivity(
                "JSON-RPC " + normalizedMethod,
                ActivityKind.Server);

            Activity.Current = previous;

            if (activity != null)
            {
                activity.SetTag(AttributeRpcSystem, "jsonrpc");
                activity.SetTag(AttributeRpcService, McpServerSourceName);
                activity.SetTag(AttributeRpcMethod, normalizedMethod);
                activity.SetTag(AttributeNetworkTransport, NormalizeTransport(transport));
            }

            return activity;
        }

        /// <summary>
        /// Track active MCP requests.
        /// </summary>
        /// <param name="method">RPC method.</param>
        /// <param name="transport">Transport.</param>
        /// <param name="delta">Delta.</param>
        public static void AddMcpServerActiveRequest(string? method, string? transport, long delta)
        {
            TagList tags = new TagList
            {
                { AttributeRpcSystem, "jsonrpc" },
                { AttributeRpcMethod, Normalize(method, "unknown") },
                { AttributeNetworkTransport, NormalizeTransport(transport) }
            };

            _McpServerActiveRequests.Add(delta, tags);
        }

        /// <summary>
        /// Track open MCP client connections.
        /// </summary>
        /// <param name="transport">Transport.</param>
        /// <param name="delta">Delta.</param>
        public static void AddMcpServerConnection(string? transport, long delta)
        {
            TagList tags = new TagList
            {
                { AttributeNetworkTransport, NormalizeTransport(transport) }
            };

            _McpServerConnections.Add(delta, tags);
        }

        /// <summary>
        /// Record a completed MCP request.
        /// </summary>
        /// <param name="method">RPC method.</param>
        /// <param name="transport">Transport.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordMcpServerRequest(
            string? method,
            string? transport,
            string? outcome,
            double elapsedSeconds)
        {
            TagList tags = new TagList
            {
                { AttributeRpcSystem, "jsonrpc" },
                { AttributeRpcService, McpServerSourceName },
                { AttributeRpcMethod, Normalize(method, "unknown") },
                { AttributeNetworkTransport, NormalizeTransport(transport) },
                { AttributeOutcome, NormalizeOutcome(outcome) }
            };

            _McpServerRequests.Add(1, tags);
            _RpcServerDuration.Record(elapsedSeconds, tags);
        }

        /// <summary>
        /// Start an SDK HTTP client span.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="route">Route path.</param>
        /// <returns>Activity, or null when no listener is sampling.</returns>
        public static Activity? StartSdkHttpClientActivity(string? method, string? route)
        {
            string normalizedMethod = Normalize(method, "UNKNOWN");
            string normalizedRoute = Normalize(route, "unknown");

            Activity? activity = SdkActivitySource.StartActivity(
                "HTTP " + normalizedMethod + " " + normalizedRoute,
                ActivityKind.Client);

            if (activity != null)
            {
                activity.SetTag(AttributeHttpMethod, normalizedMethod);
                activity.SetTag(AttributeHttpRoute, normalizedRoute);
            }

            return activity;
        }

        /// <summary>
        /// Record a completed SDK HTTP client request.
        /// </summary>
        /// <param name="method">HTTP method.</param>
        /// <param name="route">Route path.</param>
        /// <param name="statusCode">HTTP status code, or 0 when unavailable.</param>
        /// <param name="requestBodyBytes">Request body bytes.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordSdkHttpClientRequest(
            string? method,
            string? route,
            int statusCode,
            long requestBodyBytes,
            double elapsedSeconds)
        {
            string outcome = statusCode == 0 ? "error" : OutcomeFromStatusCode(statusCode);
            TagList tags = new TagList
            {
                { AttributeHttpMethod, Normalize(method, "UNKNOWN") },
                { AttributeHttpRoute, Normalize(route, "unknown") },
                { AttributeHttpStatusCode, statusCode },
                { AttributeOutcome, outcome }
            };

            _SdkRequests.Add(1, tags);
            _HttpClientRequestDuration.Record(elapsedSeconds, tags);

            if (requestBodyBytes >= 0)
            {
                TagList sizeTags = new TagList
                {
                    { AttributeHttpMethod, Normalize(method, "UNKNOWN") },
                    { AttributeHttpRoute, Normalize(route, "unknown") }
                };
                _HttpClientRequestBodySize.Record(requestBodyBytes, sizeTags);
            }
        }

        /// <summary>
        /// Record a completed processor extraction operation.
        /// </summary>
        /// <param name="processor">Processor name.</param>
        /// <param name="inputKind">Input kind.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="atomCount">Atom count.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordProcessorExtraction(
            string? processor,
            string? inputKind,
            string? outcome,
            long atomCount,
            double elapsedSeconds)
        {
            TagList tags = new TagList
            {
                { AttributeProcessor, Normalize(processor, "unknown") },
                { AttributeInputKind, Normalize(inputKind, "unknown") },
                { AttributeOutcome, NormalizeOutcome(outcome) }
            };

            _ProcessorExtractions.Add(1, tags);
            _ProcessorExtractionDuration.Record(elapsedSeconds, tags);
            if (atomCount > 0) _ExtractedAtoms.Add(atomCount, tags);
        }

        /// <summary>
        /// Record a completed type detection operation.
        /// </summary>
        /// <param name="documentType">Detected document type.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="inputBytes">Input bytes.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordTypeDetection(
            string? documentType,
            string? outcome,
            long inputBytes,
            double elapsedSeconds)
        {
            TagList tags = new TagList
            {
                { AttributeDocumentType, Normalize(documentType, "Unknown") },
                { AttributeOutcome, NormalizeOutcome(outcome) }
            };

            _TypeDetections.Add(1, tags);
            _TypeDetectionDuration.Record(elapsedSeconds, tags);
            if (inputBytes >= 0) _TypeDetectionInputSize.Record(inputBytes, tags);
        }

        /// <summary>
        /// Record a completed chunking operation.
        /// </summary>
        /// <param name="atomType">Atom type.</param>
        /// <param name="strategy">Chunking strategy.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="chunkCount">Chunk count.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordChunking(
            string? atomType,
            string? strategy,
            string? outcome,
            long chunkCount,
            double elapsedSeconds)
        {
            TagList tags = new TagList
            {
                { AttributeAtomType, Normalize(atomType, "unknown") },
                { AttributeChunkStrategy, Normalize(strategy, "unknown") },
                { AttributeOutcome, NormalizeOutcome(outcome) }
            };

            _ChunkingOperations.Add(1, tags);
            _ChunkingDuration.Record(elapsedSeconds, tags);
            if (chunkCount > 0) _Chunks.Add(chunkCount, tags);
        }

        /// <summary>
        /// Record a completed data ingestion operation.
        /// </summary>
        /// <param name="inputKind">Input kind.</param>
        /// <param name="outcome">Outcome.</param>
        /// <param name="chunkCount">Chunk count.</param>
        /// <param name="elapsedSeconds">Elapsed seconds.</param>
        public static void RecordDataIngestion(
            string? inputKind,
            string? outcome,
            long chunkCount,
            double elapsedSeconds)
        {
            TagList tags = new TagList
            {
                { AttributeInputKind, Normalize(inputKind, "unknown") },
                { AttributeOutcome, NormalizeOutcome(outcome) }
            };

            _IngestionDocuments.Add(1, tags);
            _IngestionDuration.Record(elapsedSeconds, tags);
            if (chunkCount > 0) _IngestionChunks.Add(chunkCount, tags);
        }

        #endregion

        #region Private-Methods

        private static string Normalize(string? value, string fallback)
        {
            if (String.IsNullOrWhiteSpace(value)) return fallback;
            return value.Trim();
        }

        private static string NormalizeOutcome(string? outcome)
        {
            string normalized = Normalize(outcome, "ok").ToLowerInvariant();
            if (normalized == "error" || normalized == "ok") return normalized;
            return normalized;
        }

        private static string NormalizeTransport(string? transport)
        {
            return Normalize(transport, "unknown").ToLowerInvariant();
        }

        private static string OutcomeFromStatusCode(int statusCode)
        {
            if (statusCode >= 400 || statusCode == 0) return "error";
            return "ok";
        }

        #endregion
    }
}
