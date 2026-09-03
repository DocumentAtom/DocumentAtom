namespace DocumentAtom.Telemetry
{
    using System;

    /// <summary>
    /// OpenTelemetry export settings. These settings control how the process exports the metrics and
    /// traces that DocumentAtom already emits through <c>System.Diagnostics.Metrics.Meter</c> and
    /// <c>System.Diagnostics.ActivitySource</c>. When <see cref="Enable"/> is false the process emits
    /// nothing over the wire and instrument overhead remains near zero.
    /// </summary>
    public class TelemetrySettings
    {
        #region Public-Members

        /// <summary>
        /// Enable OpenTelemetry export. Default is true. When false, no OTLP or Prometheus exporter is
        /// started and no telemetry leaves the process.
        /// </summary>
        public bool Enable { get; set; } = true;

        /// <summary>
        /// Logical service name reported as the <c>service.name</c> resource attribute. When null or empty
        /// the host supplies a default based on the component (for example "DocumentAtom.Server").
        /// </summary>
        public string? ServiceName { get; set; } = null;

        /// <summary>
        /// OTLP exporter endpoint for both metrics and traces. Default is "http://localhost:4317" (gRPC).
        /// Use the collector's HTTP/protobuf port (4318) when <see cref="OtlpProtocol"/> is "http".
        /// </summary>
        public string OtlpEndpoint { get; set; } = "http://localhost:4317";

        /// <summary>
        /// OTLP protocol. Valid values are "grpc" (default) and "http" (HTTP/protobuf).
        /// </summary>
        public string OtlpProtocol { get; set; } = "grpc";

        /// <summary>
        /// Optional OTLP headers in "key1=value1,key2=value2" form, used to authenticate against hosted
        /// backends such as Grafana Cloud or Honeycomb. Default is null.
        /// </summary>
        public string? OtlpHeaders { get; set; } = null;

        /// <summary>
        /// OTLP export timeout in milliseconds. Default is 10000. Minimum is 1000, maximum is 120000.
        /// </summary>
        public int OtlpTimeoutMs
        {
            get
            {
                return _OtlpTimeoutMs;
            }
            set
            {
                if (value < 1000) value = 1000;
                if (value > 120000) value = 120000;
                _OtlpTimeoutMs = value;
            }
        }

        /// <summary>
        /// Metric export interval in milliseconds for the periodic OTLP metric reader. Default is 15000.
        /// Minimum is 1000, maximum is 300000.
        /// </summary>
        public int MetricExportIntervalMs
        {
            get
            {
                return _MetricExportIntervalMs;
            }
            set
            {
                if (value < 1000) value = 1000;
                if (value > 300000) value = 300000;
                _MetricExportIntervalMs = value;
            }
        }

        /// <summary>
        /// Trace sampling ratio applied by a parent-based, ratio sampler. Default is 1.0 (sample all).
        /// Minimum is 0.0, maximum is 1.0.
        /// </summary>
        public double TracesSamplingRatio
        {
            get
            {
                return _TracesSamplingRatio;
            }
            set
            {
                if (value < 0.0) value = 0.0;
                if (value > 1.0) value = 1.0;
                _TracesSamplingRatio = value;
            }
        }

        /// <summary>
        /// Enable an in-process Prometheus scrape endpoint (an HttpListener) in addition to OTLP. Default
        /// is false. Useful when Prometheus scrapes the process directly instead of a collector.
        /// </summary>
        public bool EnablePrometheus { get; set; } = false;

        /// <summary>
        /// Hostname for the in-process Prometheus scrape endpoint. Default is "localhost".
        /// </summary>
        public string PrometheusHostname { get; set; } = "localhost";

        /// <summary>
        /// Port for the in-process Prometheus scrape endpoint. Default is 9464. Minimum is 1, maximum is 65535.
        /// </summary>
        public int PrometheusPort
        {
            get
            {
                return _PrometheusPort;
            }
            set
            {
                if (value < 1) value = 1;
                if (value > 65535) value = 65535;
                _PrometheusPort = value;
            }
        }

        /// <summary>
        /// Path for the in-process Prometheus scrape endpoint. Default is "/metrics".
        /// </summary>
        public string PrometheusPath { get; set; } = "/metrics";

        /// <summary>
        /// Include .NET runtime metrics (GC, heap, thread pool, JIT) via OpenTelemetry.Instrumentation.Runtime.
        /// Default is true.
        /// </summary>
        public bool EnableRuntimeMetrics { get; set; } = true;

        /// <summary>
        /// Include process-level metrics (working set, uptime, thread count) as observable gauges.
        /// Default is true.
        /// </summary>
        public bool EnableProcessMetrics { get; set; } = true;

        #endregion

        #region Private-Members

        private int _OtlpTimeoutMs = 10000;
        private int _MetricExportIntervalMs = 15000;
        private double _TracesSamplingRatio = 1.0;
        private int _PrometheusPort = 9464;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// OpenTelemetry export settings.
        /// </summary>
        public TelemetrySettings()
        {
        }

        #endregion
    }
}
