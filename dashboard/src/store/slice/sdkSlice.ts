import { BaseQueryFn, EndpointBuilder } from "@reduxjs/toolkit/query/react";
import sdkSlice, { SdkBaseQueryArgs } from "#/store/rtk/rtkSdkInstance";

export enum SdkSliceTags {
  SDK = "SDK",
}

import { sdk } from "#/services/sdk.service";
import {
  ExtractAtomResponse,
  TypeDetectionResponse,
} from "documentatom-sdk/dist/types/types";
const enhancedSdk = sdkSlice.enhanceEndpoints({
  addTagTypes: [SdkSliceTags.SDK],
});

const sdkSliceInstance = enhancedSdk.injectEndpoints({
  endpoints: (
    build: EndpointBuilder<
      BaseQueryFn<SdkBaseQueryArgs, unknown, unknown>,
      SdkSliceTags,
      "sdk"
    >
  ) => ({
    validateConnectivity: build.mutation<boolean, void>({
      query: () => ({
        callback: () => sdk.validateConnectivity(),
      }),
    }),
    typeDetection: build.mutation<TypeDetectionResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.TypeDetection.detectType(fileBinary),
      }),
    }),
    extarctExcels: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.excel(fileBinary),
      }),
    }),
    extractHtml: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.html(fileBinary),
      }),
    }),
    extractMarkdown: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.markdown(fileBinary),
      }),
    }),
    ocr: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.ocr(fileBinary),
      }),
    }),
    extractPdfs: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.pdf(fileBinary),
      }),
    }),
    extractPngs: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.png(fileBinary),
      }),
    }),
    extractPpts: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.powerpoint(fileBinary),
      }),
    }),
    extractRtf: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.rtf(fileBinary),
      }),
    }),
    extractTxt: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.text(fileBinary),
      }),
    }),
    extractWordDocs: build.mutation<ExtractAtomResponse, File>({
      query: (fileBinary: File) => ({
        callback: () => sdk.ExtractAtom.word(fileBinary),
      }),
    }),
  }),
});

export const {
  useValidateConnectivityMutation,
  useTypeDetectionMutation,
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
} = sdkSliceInstance;
