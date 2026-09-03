namespace DocumentAtom.Telemetry
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Metrics;
    using DocumentAtom.Core.Diagnostics;
    using OpenTelemetry;
    using OpenTelemetry.Exporter;
    using OpenTelemetry.Metrics;
    using OpenTelemetry.Resources;
    using OpenTelemetry.Trace;

    /// <summary>
    /// Owns the OpenTelemetry export pipeline for a DocumentAtom process. DocumentAtom domain, HTTP, MCP,
    /// SDK, and ingestion code emit through the BCL <c>Meter</c>/<c>ActivitySource</c> instruments declared
    /// in <see cref="DocumentAtomDiagnostics"/>; this host subscribes to those instruments by name and
    /// exports them over OTLP (and, optionally, an in-process Prometheus scrape endpoint). Instruments are
    /// no-ops until a host like this one subscribes, so the emit-side code carries no OpenTelemetry
    /// dependency. Dispose the host on shutdown to flush and release exporters.
    /// </summary>
    public sealed class TelemetryHost : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Service name reported as the <c>service.name</c> resource attribute.
        /// </summary>
        public string ServiceName
        {
            get
            {
                return _ServiceName;
            }
        }

        #endregion

        #region Private-Members

        private readonly string _ServiceName;
        private readonly MeterProvider? _MeterProvider;
        private readonly TracerProvider? _TracerProvider;
        private readonly Meter? _ProcessMeter;
        private readonly List<object> _ProcessInstruments = new List<object>();
        private readonly DateTime _ProcessStartUtc = DateTime.UtcNow;
        private bool _Disposed = false;

        private static readonly string[] _SourceNames = new string[]
        {
            DocumentAtomDiagnostics.CoreSourceName,
            DocumentAtomDiagnostics.ServerSourceName,
            DocumentAtomDiagnostics.McpServerSourceName,
            DocumentAtomDiagnostics.SdkSourceName,
            DocumentAtomDiagnostics.DataIngestionSourceName
        };

        #endregion

        #region Constructors-and-Factories

        private TelemetryHost(
            string serviceName,
            MeterProvider? meterProvider,
            TracerProvider? tracerProvider,
            Meter? processMeter)
        {
            _ServiceName = serviceName;
            _MeterProvider = meterProvider;
            _TracerProvider = tracerProvider;
            _ProcessMeter = processMeter;
        }

        /// <summary>
        /// Start the export pipeline for the supplied settings. Returns null when telemetry is disabled or
        /// settings are null, in which case the process emits nothing over the wire.
        /// </summary>
        /// <param name="settings">Telemetry settings.</param>
        /// <param name="defaultServiceName">Service name to use when <see cref="TelemetrySettings.ServiceName"/> is not set.</param>
        /// <returns>A started <see cref="TelemetryHost"/>, or null when disabled.</returns>
        public static TelemetryHost? Start(TelemetrySettings? settings, string defaultServiceName)
        {
            if (settings == null || !settings.Enable) return null;
            if (String.IsNullOrEmpty(defaultServiceName)) throw new ArgumentNullException(nameof(defaultServiceName));

            string serviceName = String.IsNullOrWhiteSpace(settings.ServiceName) ? defaultServiceName : settings.ServiceName!.Trim();
            string serviceVersion = DocumentAtomDiagnostics.Version;
            string instanceId = serviceName + "@" + Environment.MachineName + ":" + Environment.ProcessId;

            ResourceBuilder resourceBuilder = ResourceBuilder.CreateDefault()
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: serviceVersion,
                    autoGenerateServiceInstanceId: false,
                    serviceInstanceId: instanceId);

            Meter? processMeter = null;
            if (settings.EnableProcessMetrics) processMeter = new Meter(serviceName, serviceVersion);

            MeterProvider meterProvider = BuildMeterProvider(settings, resourceBuilder, serviceName);
            TracerProvider tracerProvider = BuildTracerProvider(settings, resourceBuilder);

            TelemetryHost host = new TelemetryHost(serviceName, meterProvider, tracerProvider, processMeter);
            if (processMeter != null) host.RegisterProcessInstruments(processMeter);

            return host;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Flush pending telemetry and release the export pipeline.
        /// </summary>
        public void Dispose()
        {
            if (_Disposed) return;
            _Disposed = true;

            try
            {
                _MeterProvider?.ForceFlush(5000);
                _TracerProvider?.ForceFlush(5000);
            }
            catch
            {
                // best-effort flush on shutdown
            }

            _MeterProvider?.Dispose();
            _TracerProvider?.Dispose();
            _ProcessMeter?.Dispose();
            _ProcessInstruments.Clear();
        }

        #endregion

        #region Private-Methods

        private static MeterProvider BuildMeterProvider(TelemetrySettings settings, ResourceBuilder resourceBuilder, string serviceName)
        {
            MeterProviderBuilder builder = Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(resourceBuilder);

            foreach (string sourceName in _SourceNames)
            {
                builder.AddMeter(sourceName);
            }

            if (settings.EnableProcessMetrics) builder.AddMeter(serviceName);
            if (settings.EnableRuntimeMetrics) builder.AddRuntimeInstrumentation();

            builder.AddView(instrument =>
            {
                if (instrument.GetType().GetGenericTypeDefinition() == typeof(Histogram<>)
                    && String.Equals(instrument.Unit, "s", StringComparison.Ordinal))
                {
                    return new ExplicitBucketHistogramConfiguration
                    {
                        Boundaries = new double[]
                        {
                            0.005, 0.01, 0.025, 0.05, 0.075, 0.1, 0.25, 0.5, 0.75, 1, 2.5, 5, 7.5, 10, 30
                        }
                    };
                }

                return null;
            });

            builder.AddOtlpExporter((exporterOptions, readerOptions) =>
            {
                ConfigureOtlp(exporterOptions, settings);
                readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = settings.MetricExportIntervalMs;
            });

            if (settings.EnablePrometheus)
            {
                builder.AddPrometheusHttpListener(listenerOptions =>
                {
                    listenerOptions.Host = settings.PrometheusHostname;
                    listenerOptions.Port = settings.PrometheusPort;
                    listenerOptions.ScrapeEndpointPath = settings.PrometheusPath;
                });
            }

            return builder.Build();
        }

        private static TracerProvider BuildTracerProvider(TelemetrySettings settings, ResourceBuilder resourceBuilder)
        {
            TracerProviderBuilder builder = Sdk.CreateTracerProviderBuilder()
                .SetResourceBuilder(resourceBuilder);

            foreach (string sourceName in _SourceNames)
            {
                builder.AddSource(sourceName);
            }

            builder.SetSampler(new ParentBasedSampler(new TraceIdRatioBasedSampler(settings.TracesSamplingRatio)));
            builder.AddOtlpExporter(exporterOptions => ConfigureOtlp(exporterOptions, settings));

            return builder.Build();
        }

        private static void ConfigureOtlp(OtlpExporterOptions options, TelemetrySettings settings)
        {
            if (!String.IsNullOrEmpty(settings.OtlpEndpoint)) options.Endpoint = new Uri(settings.OtlpEndpoint);

            options.Protocol = String.Equals(settings.OtlpProtocol, "http", StringComparison.OrdinalIgnoreCase)
                ? OtlpExportProtocol.HttpProtobuf
                : OtlpExportProtocol.Grpc;

            options.TimeoutMilliseconds = settings.OtlpTimeoutMs;

            if (!String.IsNullOrEmpty(settings.OtlpHeaders)) options.Headers = settings.OtlpHeaders;
        }

        private void RegisterProcessInstruments(Meter meter)
        {
            _ProcessInstruments.Add(meter.CreateObservableGauge<long>(
                "process.memory.usage",
                () => Process.GetCurrentProcess().WorkingSet64,
                "By",
                "Process working set in bytes."));

            _ProcessInstruments.Add(meter.CreateObservableGauge<double>(
                "process.uptime",
                () => (DateTime.UtcNow - _ProcessStartUtc).TotalSeconds,
                "s",
                "Seconds since the process started."));

            _ProcessInstruments.Add(meter.CreateObservableGauge<int>(
                "process.thread.count",
                () => Process.GetCurrentProcess().Threads.Count,
                "{thread}",
                "Operating system threads owned by the process."));
        }

        #endregion
    }
}
