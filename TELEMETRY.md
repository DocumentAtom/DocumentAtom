# DocumentAtom Telemetry

DocumentAtom is instrumented for observability end to end. Every REST API endpoint, every MCP tool
(across all three transports), the core document processors, chunking, type detection, OCR, data
ingestion, and the C# SDK's outbound HTTP calls emit **metrics** and **distributed traces** using the
.NET built-in diagnostics primitives (`System.Diagnostics.Metrics.Meter` and
`System.Diagnostics.ActivitySource`). A process-local OpenTelemetry pipeline subscribes to those
instruments and exports them over **OTLP** so DocumentAtom can plug into any OpenTelemetry-compatible
backend — the bundled Prometheus / Tempo / Loki / Grafana stack, or your existing platform
(Grafana Cloud, Honeycomb, Datadog, New Relic, Dynatrace, and so on).

This document describes what is emitted, how to turn it on and point it at a backend, and what a
devops team needs to fold DocumentAtom into a broader observability stack.

---

## 1. Architecture

```
  ┌─────────────────────────────────────────────┐
  │ DocumentAtom process (Server / MCP / SDK)    │
  │                                              │
  │  Emit side (no OpenTelemetry dependency):    │
  │   Meter / ActivitySource in                  │
  │     DocumentAtom.Core        (processing)    │
  │     DocumentAtom.Server      (HTTP)          │
  │     DocumentAtom.McpServer   (MCP RPC)       │
  │     DocumentAtom.Sdk         (HTTP client)   │
  │     DocumentAtom.DataIngestion (ingestion)   │
  │                                              │
  │  Host side (DocumentAtom.Telemetry):         │
  │   TelemetryHost builds an OpenTelemetry      │
  │   MeterProvider + TracerProvider that        │
  │   subscribes by source name and exports OTLP │
  └───────────────┬──────────────────────────────┘
                  │ OTLP (gRPC 4317 / HTTP 4318)
                  ▼
        ┌───────────────────────┐
        │ OpenTelemetry Collector│
        └───┬──────────┬─────────┘
       metrics      traces        logs
            │          │            │
            ▼          ▼            ▼
      Prometheus     Tempo        Loki
            └──────────┴────────────┘
                       ▼
                    Grafana
```

The emit-side code carries **no** OpenTelemetry dependency. Instruments are no-ops until a host
subscribes to them, so instrumentation cost is near zero when telemetry is disabled. Only the
`DocumentAtom.Telemetry` project references the OpenTelemetry SDK, and only the two executables
(`DocumentAtom.Server`, `DocumentAtom.McpServer`) construct a `TelemetryHost`.

### Instrumentation source names

An observing host subscribes to these five stable source names (they are both the `Meter` name and
the `ActivitySource` name):

| Source name                    | Covers                                             |
|--------------------------------|----------------------------------------------------|
| `DocumentAtom.Core`            | Processors, type detection, chunking, OCR          |
| `DocumentAtom.Server`          | REST API server (Watson) HTTP requests             |
| `DocumentAtom.McpServer`       | MCP JSON-RPC over HTTP, TCP, and WebSocket         |
| `DocumentAtom.Sdk`             | Outbound SDK HTTP client requests                  |
| `DocumentAtom.DataIngestion`   | Data-ingestion (RAG) document/chunk pipeline       |

`TelemetryHost` always subscribes to all five, plus a process-level meter named after the service.

---

## 2. Metrics catalog

Metric names below are the **instrument names** as emitted. When exported through the OpenTelemetry
Collector's Prometheus exporter, names are transformed by the standard OTel→Prometheus mapping: dots
become underscores, counters gain a `_total` suffix, and the UCUM unit becomes a suffix
(`s`→`_seconds`, `By`→`_bytes`). The Prometheus name column shows the result.

### HTTP server (`DocumentAtom.Server`)

| Instrument | Kind | Unit | Prometheus name | Key attributes |
|---|---|---|---|---|
| `documentatom.http.server.requests` | Counter | `{request}` | `documentatom_http_server_requests_total` | `http.request.method`, `http.route`, `http.response.status_code`, `outcome` |
| `http.server.active_requests` | UpDownCounter | `{request}` | `http_server_active_requests` | `http.request.method`, `url.scheme` |
| `http.server.request.duration` | Histogram | `s` | `http_server_request_duration_seconds` | `http.request.method`, `http.route`, `http.response.status_code`, `outcome` |
| `http.server.request.body.size` | Histogram | `By` | `http_server_request_body_size_bytes` | `http.request.method`, `http.route` |

### MCP server (`DocumentAtom.McpServer`)

| Instrument | Kind | Unit | Prometheus name | Key attributes |
|---|---|---|---|---|
| `documentatom.mcp.server.requests` | Counter | `{request}` | `documentatom_mcp_server_requests_total` | `rpc.method`, `network.transport`, `outcome` |
| `documentatom.mcp.server.active_requests` | UpDownCounter | `{request}` | `documentatom_mcp_server_active_requests` | `rpc.method`, `network.transport` |
| `documentatom.mcp.server.connections` | UpDownCounter | `{connection}` | `documentatom_mcp_server_connections` | `network.transport` |
| `rpc.server.duration` | Histogram | `s` | `rpc_server_duration_seconds` | `rpc.method`, `network.transport`, `outcome` |

`rpc.method` is the MCP tool name (for example `pdf/process`, `csv/process`, `typedetection/detect`),
so per-tool rates and latencies are available by filtering on that label.

### SDK / HTTP client (`DocumentAtom.Sdk`)

| Instrument | Kind | Unit | Prometheus name | Key attributes |
|---|---|---|---|---|
| `documentatom.sdk.client.requests` | Counter | `{request}` | `documentatom_sdk_client_requests_total` | `http.request.method`, `http.route`, `http.response.status_code`, `outcome` |
| `http.client.request.duration` | Histogram | `s` | `http_client_request_duration_seconds` | `http.request.method`, `http.route`, `outcome` |
| `http.client.request.body.size` | Histogram | `By` | `http_client_request_body_size_bytes` | `http.request.method`, `http.route` |

### Processing (`DocumentAtom.Core`)

| Instrument | Kind | Unit | Prometheus name | Key attributes |
|---|---|---|---|---|
| `documentatom.processor.extractions` | Counter | `{operation}` | `documentatom_processor_extractions_total` | `documentatom.processor`, `documentatom.input.kind`, `outcome` |
| `documentatom.processor.extraction.duration` | Histogram | `s` | `documentatom_processor_extraction_duration_seconds` | `documentatom.processor`, `documentatom.input.kind`, `outcome` |
| `documentatom.processor.atoms` | Counter | `{atom}` | `documentatom_processor_atoms_total` | `documentatom.processor`, `documentatom.input.kind`, `outcome` |
| `documentatom.type_detection.requests` | Counter | `{operation}` | `documentatom_type_detection_requests_total` | `documentatom.document.type`, `outcome` |
| `documentatom.type_detection.duration` | Histogram | `s` | `documentatom_type_detection_duration_seconds` | `documentatom.document.type`, `outcome` |
| `documentatom.type_detection.input.size` | Histogram | `By` | `documentatom_type_detection_input_size_bytes` | `documentatom.document.type`, `outcome` |
| `documentatom.chunking.operations` | Counter | `{operation}` | `documentatom_chunking_operations_total` | `documentatom.atom.type`, `documentatom.chunk.strategy`, `outcome` |
| `documentatom.chunking.duration` | Histogram | `s` | `documentatom_chunking_duration_seconds` | `documentatom.atom.type`, `documentatom.chunk.strategy`, `outcome` |
| `documentatom.chunking.chunks` | Counter | `{chunk}` | `documentatom_chunking_chunks_total` | `documentatom.atom.type`, `documentatom.chunk.strategy`, `outcome` |

### Data ingestion (`DocumentAtom.DataIngestion`)

> These metrics are emitted only when a host application uses the optional
> `DocumentAtom.DataIngestion` library (the RAG ingestion adapter). The DocumentAtom REST
> server and MCP server do not perform ingestion, so the bundled stack ships no ingestion
> dashboard; the instruments are documented here for applications that consume the library.

| Instrument | Kind | Unit | Prometheus name | Key attributes |
|---|---|---|---|---|
| `documentatom.ingestion.documents` | Counter | `{document}` | `documentatom_ingestion_documents_total` | `documentatom.input.kind`, `outcome` |
| `documentatom.ingestion.duration` | Histogram | `s` | `documentatom_ingestion_duration_seconds` | `documentatom.input.kind`, `outcome` |
| `documentatom.ingestion.chunks` | Counter | `{chunk}` | `documentatom_ingestion_chunks_total` | `documentatom.input.kind`, `outcome` |

### Process and .NET runtime (all services)

Emitted on a meter named after the service (`service.name`). Process gauges are always available when
`EnableProcessMetrics` is true; runtime metrics come from `OpenTelemetry.Instrumentation.Runtime`
when `EnableRuntimeMetrics` is true (metric names follow that library's `process.runtime.dotnet.*`
convention and may vary by version).

| Instrument | Kind | Unit | Prometheus name |
|---|---|---|---|
| `process.memory.usage` | ObservableGauge | `By` | `process_memory_usage_bytes` |
| `process.uptime` | ObservableGauge | `s` | `process_uptime_seconds` |
| `process.thread.count` | ObservableGauge | `{thread}` | `process_thread_count` |
| .NET GC / thread-pool / exceptions | various | — | `process_runtime_dotnet_*` |

Every metric carries the resource attributes `service.name` and `service.instance.id`. With the
collector's `resource_to_telemetry_conversion` enabled these become the labels `service_name` and
`service_instance_id`, which the bundled Grafana dashboards use for their `$service` template
variable.

---

## 3. Traces

Spans are produced from the same five `ActivitySource`s.

| Span (activity) name | Kind | Source | Notes |
|---|---|---|---|
| `HTTP <METHOD> <route>` | Server | `DocumentAtom.Server` | One span per inbound REST request; tags include method, route, scheme, status. |
| `JSON-RPC <method>` | Server | `DocumentAtom.McpServer` | One span per MCP call; tags include `rpc.method`, `network.transport`, session/request ids. |
| `HTTP <METHOD> <route>` | Client | `DocumentAtom.Sdk` | Outbound SDK request span; correlates the MCP server to the REST server. |
| Processor / chunking / type-detection spans | Internal | `DocumentAtom.Core` | Emitted around the corresponding units of work. |

Exceptions are recorded on the active span as an `exception` event (`exception.type`,
`exception.message`, `exception.stacktrace`) and the span status is set to `Error`. Trace context is
propagated with the W3C `traceparent` standard, so a call that flows MCP → SDK → REST server produces
a single connected trace.

Sampling is parent-based on top of a ratio sampler (`TracesSamplingRatio`, default `1.0` = sample
everything).

---

## 4. Configuration

Telemetry is configured per process under the `Telemetry` object in the service's JSON settings file
(`documentatom.json`). **Telemetry export is enabled by default** and targets
`http://localhost:4317` (OTLP gRPC).

```json
"Telemetry": {
  "Enable": true,
  "ServiceName": "DocumentAtom.Server",
  "OtlpEndpoint": "http://localhost:4317",
  "OtlpProtocol": "grpc",
  "OtlpHeaders": null,
  "OtlpTimeoutMs": 10000,
  "MetricExportIntervalMs": 15000,
  "TracesSamplingRatio": 1.0,
  "EnablePrometheus": false,
  "PrometheusHostname": "localhost",
  "PrometheusPort": 9464,
  "PrometheusPath": "/metrics",
  "EnableRuntimeMetrics": true,
  "EnableProcessMetrics": true
}
```

| Setting | Default | Meaning |
|---|---|---|
| `Enable` | `true` | Master switch. When false, nothing is exported and instrument overhead stays near zero. |
| `ServiceName` | (per service) | `service.name` resource attribute. Defaults to `DocumentAtom.Server` / `DocumentAtom.McpServer`. |
| `OtlpEndpoint` | `http://localhost:4317` | OTLP endpoint for metrics and traces. Use port `4318` with the `http` protocol. |
| `OtlpProtocol` | `grpc` | `grpc` or `http` (HTTP/protobuf). |
| `OtlpHeaders` | `null` | `key1=value1,key2=value2` headers for authenticating to hosted backends. |
| `OtlpTimeoutMs` | `10000` | Export timeout (clamped 1000–120000). |
| `MetricExportIntervalMs` | `15000` | Metric push interval (clamped 1000–300000). |
| `TracesSamplingRatio` | `1.0` | Ratio sampler (clamped 0.0–1.0), wrapped in a parent-based sampler. |
| `EnablePrometheus` | `false` | Start an in-process Prometheus scrape endpoint in addition to OTLP. |
| `PrometheusHostname` / `PrometheusPort` / `PrometheusPath` | `localhost` / `9464` / `/metrics` | In-process scrape endpoint address. |
| `EnableRuntimeMetrics` | `true` | Include .NET runtime (GC, thread pool, exceptions) metrics. |
| `EnableProcessMetrics` | `true` | Include process working-set/uptime/thread-count gauges. |

### Environment-variable overrides (MCP server)

The MCP server accepts these overrides (they take precedence over the JSON file):

| Variable | Overrides |
|---|---|
| `DOCUMENTATOM_TELEMETRY_ENABLE` | `Telemetry.Enable` (`true`/`false` or `1`/`0`) |
| `DOCUMENTATOM_OTLP_ENDPOINT` | `Telemetry.OtlpEndpoint` |
| `DOCUMENTATOM_OTLP_PROTOCOL` | `Telemetry.OtlpProtocol` |

### Disabling telemetry

Set `"Telemetry": { "Enable": false }` in the settings file, or (MCP only)
`DOCUMENTATOM_TELEMETRY_ENABLE=false`. The instruments remain in the code but nothing is exported and
no exporter threads or sockets are created.

---

## 5. The bundled observability stack

A complete reference stack ships in [`docker/compose.yaml`](docker/compose.yaml). From the `docker/`
directory:

```bash
docker compose up -d
```

This starts DocumentAtom (server, dashboard, MCP) alongside the observability backends. All host ports
are non-conflicting:

| Service | URL | Default credentials | Purpose |
|---|---|---|---|
| DocumentAtom Server | http://localhost:8000 | — | REST API |
| DocumentAtom Dashboard | http://localhost:3000 | — | Product UI (Observability tab links to the services below) |
| DocumentAtom MCP | :8200 (HTTP), :8201 (TCP), :8202 (WS) | — | MCP transports |
| **Grafana** | http://localhost:3001 | **admin / admin** | Dashboards and trace/log exploration |
| **Prometheus** | http://localhost:9090 | none | Metrics store and query UI |
| **Tempo** | http://localhost:3200 | none | Trace backend (explore via Grafana) |
| **Loki** | http://localhost:3100 | none | Log backend (explore via Grafana) |
| **OpenTelemetry Collector** | :4317 (gRPC), :4318 (HTTP), :8889 (Prometheus exporter) | none | OTLP ingest and fan-out |

> Grafana's default port is 3000, which the DocumentAtom dashboard already uses, so Grafana is
> published on **3001**.

The dashboard's **Observability** tab surfaces cards for Grafana, Prometheus, Tempo, Loki, and the
collector — each showing the service name, its default credentials, and its URL, and opening in a new
window. Those card URLs are configurable at dashboard build time via `NEXT_PUBLIC_GRAFANA_URL`,
`NEXT_PUBLIC_PROMETHEUS_URL`, `NEXT_PUBLIC_TEMPO_URL`, `NEXT_PUBLIC_LOKI_URL`, and
`NEXT_PUBLIC_OTEL_COLLECTOR_URL`.

### Grafana dashboards

Dashboards are auto-provisioned into a single top-level **DocumentAtom** folder (no subfolders),
organized by domain:

- **DocumentAtom - Overview** — cross-cutting golden signals
- **DocumentAtom - HTTP** — REST server throughput, latency percentiles, errors, body sizes
- **DocumentAtom - MCP** — RPC rates/latency by tool and transport, connections
- **DocumentAtom - Processing** — extraction, type detection, and chunking by processor/strategy
- **DocumentAtom - SDK** — outbound HTTP client rates, latency, errors
- **DocumentAtom - Runtime** — process and .NET runtime health

Datasources (Prometheus, Tempo, Loki) are provisioned with trace↔log correlation.

---

## 6. Connecting to an existing observability platform

DocumentAtom speaks plain OTLP, so pointing it at your own stack is a configuration change, not a code
change.

**Your own collector or OTLP-native backend.** Set `OtlpEndpoint` (and `OtlpProtocol`) to your
collector. gRPC on 4317 or HTTP/protobuf on 4318 are both supported.

**Hosted backends (Grafana Cloud, Honeycomb, New Relic, Datadog OTLP, …).** Point `OtlpEndpoint` at
the vendor's OTLP URL and pass credentials with `OtlpHeaders`, for example:

```json
"Telemetry": {
  "Enable": true,
  "OtlpEndpoint": "https://otlp-gateway.example.com/otlp",
  "OtlpProtocol": "http",
  "OtlpHeaders": "authorization=Bearer <token>,x-scope-orgid=1234"
}
```

**Direct Prometheus scrape (no collector).** Set `EnablePrometheus: true`; the process exposes
`http://<PrometheusHostname>:<PrometheusPort><PrometheusPath>` (default `:9464/metrics`). Add a scrape
job to your Prometheus:

```yaml
scrape_configs:
  - job_name: documentatom
    static_configs:
      - targets: ["<host>:9464"]
```

**Logs.** DocumentAtom application logging uses SyslogLogging (console/file/syslog), configured under
the `Logging` object in each settings file — it is independent of the OTLP metrics/traces pipeline.
The bundled collector includes a logs pipeline and Loki is provisioned in Grafana, so you can route
container stdout or syslog into Loki with your preferred agent (Promtail, Grafana Alloy, the syslog
receiver, etc.) if you want logs alongside metrics and traces.

---

## 7. Notes for the devops team

- **Cardinality.** Metric attributes are deliberately low-cardinality (method, route, processor,
  transport, outcome, document type). High-cardinality identifiers (request ids, session ids) are put
  on **spans**, never on metric labels. Keep this discipline if you add instrumentation.
- **Export is off-thread.** Metrics are pushed on a background timer every `MetricExportIntervalMs`;
  traces are batched. Export failures (for example, no collector reachable) are handled by the
  OpenTelemetry SDK's internal retry/backoff and do not block or crash the application — so it is safe
  to leave telemetry enabled even when a collector is temporarily unavailable.
- **Resource attributes.** Each process reports `service.name` and a stable
  `service.instance.id` of the form `<service>@<machine>:<pid>`. Use these to distinguish replicas.
- **Sampling.** For high-traffic deployments, lower `TracesSamplingRatio` (for example `0.1`) to cap
  trace volume; metrics are unaffected by sampling.
- **Security.** OTLP endpoints are unauthenticated by default and intended for a trusted network or a
  local collector. For hosted backends, authenticate with `OtlpHeaders` over TLS. The bundled Grafana
  ships with `admin/admin` — change it before exposing the stack beyond localhost.
- **Versioning.** The telemetry contract (source names, instrument names, attribute keys) is stable
  and reported as the `service.version` resource attribute
  (`DocumentAtom.Core.Diagnostics.DocumentAtomDiagnostics.Version`).
