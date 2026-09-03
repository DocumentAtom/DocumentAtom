"use client";

import React from "react";
import { Button, Card, Col, Row, Tag, Typography } from "antd";
import { ExportOutlined, KeyOutlined, LinkOutlined } from "@ant-design/icons";
import DocuAtomFlex from "#/components/base/flex/Flex";
import { observabilityServices } from "#/constants/config";

const { Title, Text, Paragraph } = Typography;

const Observability = () => {
  return (
    <div style={{ padding: "16px 8px", overflowY: "auto", height: "100%" }}>
      <Title level={4} style={{ marginTop: 0 }}>
        Observability
      </Title>
      <Paragraph type="secondary" style={{ maxWidth: 820 }}>
        DocumentAtom exports metrics and traces over OpenTelemetry (OTLP). When the bundled
        observability stack is running (<Text code>docker compose up</Text> from the{" "}
        <Text code>docker/</Text> directory), the services below let you explore that data. Grafana
        hosts the DocumentAtom dashboards; Prometheus, Tempo, and Loki are the metric, trace, and log
        backends behind it.
      </Paragraph>

      <Row gutter={[16, 16]} align="stretch">
        {observabilityServices.map((svc) => (
          <Col key={svc.name} xs={24} sm={12} lg={8} style={{ display: "flex" }}>
            <Card
              size="small"
              style={{ width: "100%", display: "flex", flexDirection: "column" }}
              styles={{ body: { flex: 1, display: "flex", flexDirection: "column" } }}
              title={
                <DocuAtomFlex align="center" gap={8} justify="space-between">
                  <span>{svc.name}</span>
                  {svc.linkable ? (
                    <Tag color="green">Web UI</Tag>
                  ) : (
                    <Tag color="default">Endpoint</Tag>
                  )}
                </DocuAtomFlex>
              }
            >
              <DocuAtomFlex vertical gap={10} style={{ flex: 1 }}>
                <Text type="secondary">{svc.description}</Text>

                <DocuAtomFlex align="center" gap={8}>
                  <LinkOutlined />
                  <Text copyable code>
                    {svc.url}
                  </Text>
                </DocuAtomFlex>

                <DocuAtomFlex align="center" gap={8}>
                  <KeyOutlined />
                  <Text>{svc.credentials}</Text>
                </DocuAtomFlex>

                {svc.linkable ? (
                  <Button
                    type="primary"
                    icon={<ExportOutlined />}
                    onClick={() =>
                      window.open(svc.url, "_blank", "noopener,noreferrer")
                    }
                    style={{ marginTop: "auto", color: "#ffffff" }}
                    aria-label={`Open ${svc.name} in a new window`}
                  >
                    Open {svc.name}
                  </Button>
                ) : (
                  <Button type="default" disabled style={{ marginTop: "auto" }}>
                    OTLP ingest endpoint
                  </Button>
                )}
              </DocuAtomFlex>
            </Card>
          </Col>
        ))}
      </Row>
    </div>
  );
};

export default Observability;
