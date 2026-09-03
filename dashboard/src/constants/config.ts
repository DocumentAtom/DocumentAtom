export const apiEndpointURL = "http://localhost:3000";

export const MIN_PASSWORD_LENGTH = 8;

export const keepUnusedDataFor = 300; //5mins

/**
 * Observability service links surfaced in the dashboard's Observability tab. URLs default to the
 * host ports published by docker/compose.yaml and can be overridden with NEXT_PUBLIC_* env vars
 * at build time. These are the addresses the operator's browser uses, so localhost is correct for
 * the common single-host docker-compose deployment.
 */
export interface ObservabilityServiceLink {
  name: string;
  description: string;
  url: string;
  credentials: string;
  linkable: boolean;
}

export const observabilityServices: ObservabilityServiceLink[] = [
  {
    name: "Grafana",
    description: "Dashboards for DocumentAtom metrics and traces, organized in the DocumentAtom folder.",
    url: process.env.NEXT_PUBLIC_GRAFANA_URL || "http://localhost:3001",
    credentials: "admin / admin",
    linkable: true,
  },
  {
    name: "Prometheus",
    description: "Metrics store and query UI. Scrapes the OpenTelemetry Collector exporter.",
    url: process.env.NEXT_PUBLIC_PROMETHEUS_URL || "http://localhost:9090",
    credentials: "No login required",
    linkable: true,
  },
  {
    name: "Tempo",
    description: "Distributed trace backend. Best explored through Grafana's Explore view.",
    url: process.env.NEXT_PUBLIC_TEMPO_URL || "http://localhost:3200",
    credentials: "No login required",
    linkable: true,
  },
  {
    name: "Loki",
    description: "Log aggregation backend. Best explored through Grafana's Explore view.",
    url: process.env.NEXT_PUBLIC_LOKI_URL || "http://localhost:3100",
    credentials: "No login required",
    linkable: true,
  },
  {
    name: "OpenTelemetry Collector",
    description: "OTLP ingest endpoint (gRPC 4317 / HTTP 4318). DocumentAtom services export here.",
    url: process.env.NEXT_PUBLIC_OTEL_COLLECTOR_URL || "http://localhost:4317",
    credentials: "OTLP endpoint (no UI)",
    linkable: false,
  },
];
