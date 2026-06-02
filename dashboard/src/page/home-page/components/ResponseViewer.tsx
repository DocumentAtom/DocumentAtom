"use client";

import React, { useEffect, useMemo, useRef, useState } from "react";
import { Button, Tooltip } from "antd";
import {
  ApartmentOutlined,
  CheckOutlined,
  CodeOutlined,
  CopyOutlined,
} from "@ant-design/icons";
import DocuAtomTitle from "#/components/base/typograpghy/Title";
import JSONEditor from "#/components/base/json-editor/JSONEditor";
import styles from "./ResponseViewer.module.scss";

interface ResponseViewerProps {
  value: any;
  uniqueKey: string;
}

const copyTextToClipboard = async (text: string) => {
  if (navigator.clipboard?.writeText) {
    try {
      await navigator.clipboard.writeText(text);
      return;
    } catch {
      // Fall back for insecure HTTP origins and browser clipboard restrictions.
    }
  }

  const textArea = document.createElement("textarea");
  textArea.value = text;
  textArea.setAttribute("readonly", "");
  textArea.style.position = "fixed";
  textArea.style.top = "-9999px";
  textArea.style.left = "-9999px";
  document.body.appendChild(textArea);
  textArea.focus();
  textArea.select();

  const copied = document.execCommand("copy");
  document.body.removeChild(textArea);

  if (!copied) {
    throw new Error("Copy command was rejected");
  }
};

const ResponseViewer = ({ value, uniqueKey }: ResponseViewerProps) => {
  const [showRawJson, setShowRawJson] = useState(false);
  const [copied, setCopied] = useState(false);
  const resetCopyStateTimer = useRef<ReturnType<typeof setTimeout> | null>(null);
  const jsonText = useMemo(() => JSON.stringify(value, null, 2), [value]);

  useEffect(() => {
    return () => {
      if (resetCopyStateTimer.current) {
        clearTimeout(resetCopyStateTimer.current);
      }
    };
  }, []);

  const handleCopy = async () => {
    try {
      await copyTextToClipboard(jsonText);
      setCopied(true);
    } catch (error) {
      console.error("Failed to copy response JSON:", error);
      return;
    }

    if (resetCopyStateTimer.current) {
      clearTimeout(resetCopyStateTimer.current);
    }

    resetCopyStateTimer.current = setTimeout(() => {
      setCopied(false);
    }, 1200);
  };

  return (
    <div className={styles.responseViewer}>
      <div className={styles.responseHeader}>
        <DocuAtomTitle level={5} className={styles.responseTitle}>
          Response
        </DocuAtomTitle>
        <div className={styles.responseActions}>
          <Tooltip title="Copy JSON">
            <Button
              aria-label="Copy JSON response"
              icon={copied ? <CheckOutlined /> : <CopyOutlined />}
              onClick={handleCopy}
              type="text"
            />
          </Tooltip>
          <Tooltip title={showRawJson ? "View formatted response" : "View raw JSON"}>
            <Button
              aria-label={
                showRawJson ? "View formatted response" : "View raw JSON"
              }
              icon={showRawJson ? <ApartmentOutlined /> : <CodeOutlined />}
              onClick={() => setShowRawJson((current) => !current)}
              type="text"
            />
          </Tooltip>
        </div>
      </div>

      {showRawJson ? (
        <pre className={styles.jsonBox}>{jsonText}</pre>
      ) : (
        <div className={styles.editorBox}>
          <JSONEditor
            value={value}
            onChange={() => {}}
            mode="tree"
            uniqueKey={uniqueKey}
            expandOnStart={true}
          />
        </div>
      )}
    </div>
  );
};

export default ResponseViewer;
