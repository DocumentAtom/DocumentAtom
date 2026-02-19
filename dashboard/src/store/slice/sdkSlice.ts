import { BaseQueryFn, EndpointBuilder } from "@reduxjs/toolkit/query/react";
import sdkSlice, { SdkBaseQueryArgs } from "#/store/rtk/rtkSdkInstance";

export enum SdkSliceTags {
  SDK = "SDK",
}

import { sdk } from "#/services/sdk.service";
import {
  ApiProcessorSettings,
  ExtractAtomResponse,
  TypeDetectionResponse,
} from "documentatom-sdk/dist/types/types";

interface ExtractMutationArg {
  file: File;
  settings?: ApiProcessorSettings;
}

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
    extarctExcels: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.excel(file, settings),
      }),
    }),
    extractHtml: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.html(file, settings),
      }),
    }),
    extractMarkdown: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.markdown(file, settings),
      }),
    }),
    ocr: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.ocr(file, settings),
      }),
    }),
    extractPdfs: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.pdf(file, settings),
      }),
    }),
    extractPngs: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.png(file, settings),
      }),
    }),
    extractPpts: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.powerpoint(file, settings),
      }),
    }),
    extractRtf: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.rtf(file, settings),
      }),
    }),
    extractTxt: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.text(file, settings),
      }),
    }),
    extractWordDocs: build.mutation<ExtractAtomResponse, ExtractMutationArg>({
      query: ({ file, settings }: ExtractMutationArg) => ({
        callback: () => sdk.ExtractAtom.word(file, settings),
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
