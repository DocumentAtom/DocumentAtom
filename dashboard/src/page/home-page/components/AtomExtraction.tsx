"use client";
import React, { useRef, useState } from "react";
import { message } from "antd";
import { UploadOutlined } from "@ant-design/icons";
import DocuAtomButton from "#/components/base/button/Button";
import DocuAtomUpload from "#/components/base/upload/Upload";
import DocuAtomTitle from "#/components/base/typograpghy/Title";
import DocuAtomDivider from "#/components/base/divider/Divider";
import DocuAtomSelect from "#/components/base/select/Select";
import JSONEditor from "#/components/base/json-editor/JSONEditor";
import {
  useExtarctExcelsMutation,
  useExtractHtmlMutation,
  useExtractMarkdownMutation,
  useOcrMutation,
  useExtractPdfsMutation,
  useExtractPngsMutation,
  useExtractPptsMutation,
  useExtractRtfMutation,
  useExtractTxtMutation,
  useExtractWordDocsMutation,
} from "#/store/slice/sdkSlice";
import {
  FileExtractionType,
  FILE_EXTRACTION_OPTIONS,
  FILE_TYPE_ACCEPT_MAP,
} from "#/page/home-page/components/utils";

import { uuid } from "#/utils/stringUtils";
import DocuAtomFlex from "#/components/base/flex/Flex";

//eslint-disable-next-line max-lines-per-function
const FileExtraction = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [fileType, setFileType] = useState<FileExtractionType | null>(null);
  const [responseData, setResponseData] = useState<any>(null);
  const uniqueKey = useRef<string>(uuid());

  // Initialize all mutation hooks
  const [extractExcel, { isLoading: isExcelLoading }] =
    useExtarctExcelsMutation();
  const [extractHtml, { isLoading: isHtmlLoading }] = useExtractHtmlMutation();
  const [extractMarkdown, { isLoading: isMarkdownLoading }] =
    useExtractMarkdownMutation();
  const [extractOcr, { isLoading: isOcrLoading }] = useOcrMutation();
  const [extractPdf, { isLoading: isPdfLoading }] = useExtractPdfsMutation();
  const [extractPng, { isLoading: isPngLoading }] = useExtractPngsMutation();
  const [extractPpt, { isLoading: isPptLoading }] = useExtractPptsMutation();
  const [extractRtf, { isLoading: isRtfLoading }] = useExtractRtfMutation();
  const [extractTxt, { isLoading: isTxtLoading }] = useExtractTxtMutation();
  const [extractWordDoc, { isLoading: isWordDocLoading }] =
    useExtractWordDocsMutation();

  const isLoading =
    isExcelLoading ||
    isHtmlLoading ||
    isMarkdownLoading ||
    isOcrLoading ||
    isPdfLoading ||
    isPngLoading ||
    isPptLoading ||
    isRtfLoading ||
    isTxtLoading ||
    isWordDocLoading;

  const handleFileChange = (file: File) => {
    setSelectedFile(file);
    return false; // Prevent auto upload
  };

  const getExtractionMutation = (type: FileExtractionType) => {
    switch (type) {
      case FileExtractionType.EXCEL:
        return extractExcel;
      case FileExtractionType.HTML:
        return extractHtml;
      case FileExtractionType.MARKDOWN:
        return extractMarkdown;
      case FileExtractionType.OCR:
        return extractOcr;
      case FileExtractionType.PDF:
        return extractPdf;
      case FileExtractionType.PNG:
        return extractPng;
      case FileExtractionType.PPT:
        return extractPpt;
      case FileExtractionType.RTF:
        return extractRtf;
      case FileExtractionType.TEXT:
        return extractTxt;
      case FileExtractionType.WORD_DOC:
        return extractWordDoc;
      default:
        return null;
    }
  };

  const handleSubmit = async () => {
    if (!fileType) {
      message.warning("Please select a file type");
      return;
    }

    if (!selectedFile) {
      message.warning("Please select a file first");
      return;
    }

    try {
      const mutationFn = getExtractionMutation(fileType);
      if (!mutationFn) {
        message.error("Invalid file type selected");
        return;
      }

      // Call the mutation
      const response = await mutationFn(selectedFile).unwrap();

      // Store the response
      setResponseData(response);
      uniqueKey.current = uuid();

      // Log the response
      message.success("File extraction completed! Check console for results.");
    } catch (error: any) {
      console.error("File Extraction Error:", error);
      message.error(
        error?.Description || error?.Message || "Failed to extract file"
      );
      setResponseData(null);
    }
  };

  const acceptedFileTypes = fileType ? FILE_TYPE_ACCEPT_MAP[fileType] : "*";

  return (
    <>
      <DocuAtomFlex gap="20px" align="center">
        <DocuAtomFlex gap="20px" align="center">
          <label
            style={{
              display: "block",
              marginBottom: "8px",
              whiteSpace: "nowrap",
            }}
          >
            File Type
          </label>
          <DocuAtomSelect
            options={FILE_EXTRACTION_OPTIONS}
            placeholder="Select file type"
            style={{ width: "150px" }}
            value={fileType}
            onChange={(value: any) => {
              setFileType(value as FileExtractionType);
              setSelectedFile(null); // Reset file when type changes
              setResponseData(null);
              uniqueKey.current = uuid();
            }}
          />
        </DocuAtomFlex>
        <DocuAtomDivider type="vertical" />
        <DocuAtomUpload
          beforeUpload={handleFileChange}
          maxCount={1}
          onRemove={() => {
            setSelectedFile(null);
            setResponseData(null);
            uniqueKey.current = uuid();
          }}
          accept={acceptedFileTypes}
          disabled={!fileType}
        >
          <DocuAtomButton icon={<UploadOutlined />} disabled={!fileType}>
            Select File
          </DocuAtomButton>
        </DocuAtomUpload>

        <DocuAtomDivider type="vertical" />
        <DocuAtomButton
          type="primary"
          onClick={handleSubmit}
          loading={isLoading}
          disabled={!selectedFile || !fileType}
        >
          Submit
        </DocuAtomButton>
      </DocuAtomFlex>

      {responseData && (
        <>
          <DocuAtomDivider />
          <DocuAtomTitle level={5}>Response</DocuAtomTitle>
          <JSONEditor
            value={responseData}
            onChange={() => {}}
            mode="tree"
            uniqueKey={uniqueKey.current}
            expandOnStart={true}
          />
        </>
      )}
    </>
  );
};

export default FileExtraction;
