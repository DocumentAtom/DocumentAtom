import { createApi } from "@reduxjs/toolkit/query/react";

import { keepUnusedDataFor } from "#/constants/config";
import { BaseQueryFn } from "@reduxjs/toolkit/query";

export interface SdkBaseQueryArgs {
  callback: () => Promise<any>;
}

export const sdkBaseQuery: BaseQueryFn<
  SdkBaseQueryArgs,
  unknown,
  unknown
> = async ({ callback }: { callback: <T>() => Promise<T> }) => {
  try {
    const result = await callback();
    return { data: result as ReturnType<typeof callback> };
  } catch (error) {
    return { error };
  }
};

const sdkSlice = createApi({
  reducerPath: "sdk",
  baseQuery: sdkBaseQuery,
  tagTypes: [],
  endpoints: () => ({}),
  keepUnusedDataFor: keepUnusedDataFor,
});

export default sdkSlice;
