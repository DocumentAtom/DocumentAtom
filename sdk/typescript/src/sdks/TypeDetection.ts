import SdkBase from '../base/SdkBase';
import { SdkConfiguration } from '../base/SdkConfiguration';
import GenericExceptionHandlers from '../exception/GenericExceptionHandlers';
import { TypeDetectionResponse } from '../types';

/**
 * Type Detection SDK class.
 * Extends the SdkBase class.
 * @module  TypeDetection
 * @extends SdkBase
 */
export default class TypeDetection extends SdkBase {
  constructor(config: SdkConfiguration) {
    super(config);
  }

  /**
   * Detects the type of the data.
   * @param {Buffer | File} fileBinary - The binary of the file to detect the type of.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<TypeDetectionResponse>} The type of the data.
   */
  detectType(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<TypeDetectionResponse> {
    if (!fileBinary) {
      GenericExceptionHandlers.ArgumentNullException('fileBinary');
    }

    const url = `${this.config.endpoint}typedetect`;

    return this.upload<TypeDetectionResponse>(url, fileBinary, cancellationToken);
  }
}
