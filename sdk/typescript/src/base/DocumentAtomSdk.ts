import superagent from 'superagent';
import { SdkConfiguration } from './SdkConfiguration';
import { SeverityEnum } from '../enums/SeverityEnum';
import Logger from '../utils/Logger';
import TypeDetection from '../sdks/TypeDetection';
import ExtractAtom from '../sdks/ExtractAtom';
import Helper from '../utils/Helper';
/**
 * Document Atom SDK class.
 * Extends the SdkBase class.
 * @module  DocumentAtomSdk
 * @extends SdkBase
 */
export default class DocumentAtomSdk {
  public config: SdkConfiguration;
  public TypeDetection: TypeDetection;
  public ExtractAtom: ExtractAtom;
  public Helper: Helper;
  /**
   * Instantiate the SDK.
   * @param {string} endpoint - The endpoint URL.
   */

  constructor(endpoint: string = 'http://localhost:8000/') {
    const config = new SdkConfiguration(endpoint);
    this.config = config;
    this.TypeDetection = new TypeDetection(config);
    this.ExtractAtom = new ExtractAtom(config);
    this.Helper = new Helper(config);
  }

  /**
   * Validates API connectivity using a HEAD request.
   * @param {AbortController} [cancellationToken] - Optional cancellation token for cancelling the request.
   * @return {Promise<boolean>} Resolves to true if the connection is successful.
   * @throws {Error} Rejects with the error in case of failure.
   */
  validateConnectivity(cancellationToken?: AbortController): Promise<boolean> {
    return new Promise((resolve, reject) => {
      const request = superagent.head(this.config.endpoint).timeout({ response: this.config.timeoutMs });
      // If a cancelToken is provided, attach the abort method
      if (cancellationToken) {
        cancellationToken.abort = () => {
          request.abort();
          Logger.log(SeverityEnum.Debug, `Request aborted.`);
        };
      }
      request
        .then((res) => {
          Logger.log(SeverityEnum.Debug, `Success reported from ${this.config.endpoint}`);
          resolve(res.ok);
        })
        .catch((err) => {
          Logger.log(SeverityEnum.Warn, `Failed to retrieve object from ${this.config.endpoint}: ${err.message}`);
          const errorResponse = err?.response?.body || null;
          if (errorResponse) {
            reject(errorResponse);
          } else {
            reject(err.message ? err.message : err);
          }
        });
    });
  }
}
