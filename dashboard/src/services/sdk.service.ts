import { apiEndpointURL } from "#/constants/config";
import { DocumentAtomSdk } from "documentatom-sdk";

export const sdk = new DocumentAtomSdk(apiEndpointURL);

export const updateSdkEndPoint = (endpoint: string) => {
  sdk.config.endpoint = endpoint;
};
