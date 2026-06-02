"use client";

import React, { useRef, useState } from "react";
import { App } from "antd";
import { UploadOutlined } from "@ant-design/icons";
import DocuAtomButton from "#/components/base/button/Button";
import DocuAtomUpload from "#/components/base/upload/Upload";
import { useTypeDetectionMutation } from "#/store/slice/sdkSlice";
import DocuAtomDivider from "#/components/base/divider/Divider";
import { uuid } from "#/utils/stringUtils";
import DocuAtomFlex from "#/components/base/flex/Flex";
import ResponseViewer from "#/page/home-page/components/ResponseViewer";

const TypeDetection = () => {
  const { message } = App.useApp();
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [responseData, setResponseData] = useState<any>(null);
  const [detectType, { isLoading }] = useTypeDetectionMutation();
  const uniqueKey = useRef<string>(uuid());

  const handleFileChange = (file: File) => {
    setSelectedFile(file);
    return false; // Prevent auto upload
  };

  const handleSubmit = async () => {
    if (!selectedFile) {
      message.warning("Please select a file first");
      return;
    }

    try {
      // Call the mutation
      console.log("selectedFile", selectedFile);
      const response = await detectType(selectedFile).unwrap();

      // Store the response
      setResponseData(response);
      uniqueKey.current = uuid();
      // Log the response
      message.success("Type detection completed! Check console for results.");
    } catch (error) {
      console.error("Type Detection Error:", error);
      message.error("Failed to detect file type");
      setResponseData(null);
    }
  };

  return (
    <>
      <DocuAtomFlex gap="20px" align="center">
        <DocuAtomUpload
          beforeUpload={handleFileChange}
          maxCount={1}
          onRemove={() => setSelectedFile(null)}
        >
          <DocuAtomButton icon={<UploadOutlined />}>Select File</DocuAtomButton>
        </DocuAtomUpload>

        <DocuAtomButton
          type="primary"
          onClick={handleSubmit}
          loading={isLoading}
          disabled={!selectedFile}
        >
          Submit
        </DocuAtomButton>
      </DocuAtomFlex>

      {responseData && (
        <>
          <DocuAtomDivider />
          <ResponseViewer value={responseData} uniqueKey={uniqueKey.current} />
        </>
      )}
    </>
  );
};

export default TypeDetection;
