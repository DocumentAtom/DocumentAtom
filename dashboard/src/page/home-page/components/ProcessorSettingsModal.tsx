import React, { useState, useCallback } from "react";
import { Modal, Switch, Select, InputNumber, Collapse, Button, Form } from "antd";
import type {
  ApiProcessorSettings,
  ChunkStrategyEnum,
  OverlapStrategyEnum,
} from "documentatom-sdk/dist/types/types";

const CHUNK_STRATEGY_OPTIONS: { label: string; value: ChunkStrategyEnum }[] = [
  { label: "Fixed Token Count", value: "FixedTokenCount" },
  { label: "Sentence Based", value: "SentenceBased" },
  { label: "Paragraph Based", value: "ParagraphBased" },
  { label: "Regex Based", value: "RegexBased" },
  { label: "Whole List", value: "WholeList" },
  { label: "List Entry", value: "ListEntry" },
  { label: "Row", value: "Row" },
  { label: "Row With Headers", value: "RowWithHeaders" },
  { label: "Row Group With Headers", value: "RowGroupWithHeaders" },
  { label: "Key Value Pairs", value: "KeyValuePairs" },
  { label: "Whole Table", value: "WholeTable" },
];

const OVERLAP_STRATEGY_OPTIONS: {
  label: string;
  value: OverlapStrategyEnum;
}[] = [
  { label: "Sliding Window", value: "SlidingWindow" },
  { label: "Sentence Boundary Aware", value: "SentenceBoundaryAware" },
  { label: "Semantic Boundary Aware", value: "SemanticBoundaryAware" },
];

const DEFAULT_SETTINGS: ApiProcessorSettings = {
  ExtractAtomsFromImages: false,
  Chunking: {
    Enable: false,
    Strategy: "FixedTokenCount",
    FixedTokenCount: 512,
    OverlapCount: 0,
    OverlapStrategy: "SlidingWindow",
  },
};

interface ProcessorSettingsModalProps {
  open: boolean;
  onOk: (settings: ApiProcessorSettings | null) => void;
  onCancel: () => void;
}

const ProcessorSettingsModal: React.FC<ProcessorSettingsModalProps> = ({
  open,
  onOk,
  onCancel,
}) => {
  const [settings, setSettings] =
    useState<ApiProcessorSettings>(DEFAULT_SETTINGS);

  const updateSettings = useCallback(
    (patch: Partial<ApiProcessorSettings>) => {
      setSettings((prev) => ({ ...prev, ...patch }));
    },
    []
  );

  const updateChunking = useCallback(
    (patch: Partial<NonNullable<ApiProcessorSettings["Chunking"]>>) => {
      setSettings((prev) => ({
        ...prev,
        Chunking: { ...prev.Chunking, ...patch },
      }));
    },
    []
  );

  const handleReset = () => {
    setSettings({ ...DEFAULT_SETTINGS, Chunking: { ...DEFAULT_SETTINGS.Chunking! } });
  };

  const handleOk = () => {
    onOk(settings);
  };

  return (
    <Modal
      title="Processor Settings"
      open={open}
      onOk={handleOk}
      onCancel={onCancel}
      width={520}
      footer={[
        <Button key="reset" onClick={handleReset}>
          Reset to Defaults
        </Button>,
        <Button key="cancel" onClick={onCancel}>
          Cancel
        </Button>,
        <Button key="ok" type="primary" onClick={handleOk}>
          OK
        </Button>,
      ]}
    >
      <Form layout="vertical" style={{ marginTop: 16 }}>
        <Form.Item label="Extract Atoms From Images">
          <Switch
            checked={settings.ExtractAtomsFromImages ?? false}
            onChange={(checked) =>
              updateSettings({ ExtractAtomsFromImages: checked })
            }
          />
        </Form.Item>

        <Collapse
          style={{ marginBottom: 16 }}
          items={[
            {
              key: "chunking",
              label: "Chunking",
              children: (
                <>
                  <Form.Item label="Enable Chunking">
                    <Switch
                      checked={settings.Chunking?.Enable ?? false}
                      onChange={(checked) =>
                        updateChunking({ Enable: checked })
                      }
                    />
                  </Form.Item>

                  <Form.Item label="Strategy">
                    <Select
                      options={CHUNK_STRATEGY_OPTIONS}
                      value={settings.Chunking?.Strategy ?? "FixedTokenCount"}
                      onChange={(value) =>
                        updateChunking({ Strategy: value as ChunkStrategyEnum })
                      }
                      disabled={!settings.Chunking?.Enable}
                    />
                  </Form.Item>

                  <Form.Item label="Fixed Token Count">
                    <InputNumber
                      min={1}
                      value={settings.Chunking?.FixedTokenCount ?? 512}
                      onChange={(value) =>
                        updateChunking({
                          FixedTokenCount: value ?? 512,
                        })
                      }
                      disabled={!settings.Chunking?.Enable}
                      style={{ width: "100%" }}
                    />
                  </Form.Item>

                  <Form.Item label="Overlap Count">
                    <InputNumber
                      min={0}
                      value={settings.Chunking?.OverlapCount ?? 0}
                      onChange={(value) =>
                        updateChunking({ OverlapCount: value ?? 0 })
                      }
                      disabled={!settings.Chunking?.Enable}
                      style={{ width: "100%" }}
                    />
                  </Form.Item>

                  <Form.Item label="Overlap Strategy">
                    <Select
                      options={OVERLAP_STRATEGY_OPTIONS}
                      value={
                        settings.Chunking?.OverlapStrategy ?? "SlidingWindow"
                      }
                      onChange={(value) =>
                        updateChunking({
                          OverlapStrategy: value as OverlapStrategyEnum,
                        })
                      }
                      disabled={!settings.Chunking?.Enable}
                    />
                  </Form.Item>
                </>
              ),
            },
          ]}
        />
      </Form>
    </Modal>
  );
};

export default ProcessorSettingsModal;
