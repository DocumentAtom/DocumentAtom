import superagent from 'superagent';
import { SeverityEnum } from '../enums/SeverityEnum';
import GenericExceptionHandlers from '../exception/GenericExceptionHandlers';
import Serializer from '../utils/Serializer';
import { SdkConfiguration } from './SdkConfiguration';
import Logger from '../utils/Logger';

/**
 * SDK Base class for making API calls with logging and timeout functionality.
 * @module SdkBase
 */
export default class SdkBase {
  private logger: (severity: SeverityEnum, message: string) => void;
  protected config: SdkConfiguration;
  /**
   * Creates an instance of SdkBase.
   * @param {SdkConfiguration} config - The SDK configuration.
   * @throws {Error} Throws an error if the config is null.
   */
  constructor(config: SdkConfiguration) {
    if (!config) {
      GenericExceptionHandlers.ArgumentNullException('config');
    }
    this.config = config;
    this.logger = Logger.log;
  }

  /**
   * Logs a message with a severity level.
   * @param {string} sev - The severity level (e.g., SeverityEnum.Debug, 'warn').
   * @param {string} msg - The message to log.
   */
  protected log(sev: SeverityEnum, msg: string) {
    if (!msg) return;
    if (this.logger) this.logger(sev, msg);
  }

  /**
   * Submits data using a POST request to a given URL.
   * @param {string} url - The URL to post data to.
   * @param {Buffer} data - The data to send in the POST request.
   * @param {AbortController} [cancellationToken] - Optional cancellation token for cancelling the request.
   * @return {Promise<Object>} Resolves with the response data.
   * @throws {Error | ApiErrorResponse} Rejects if the URL or data is invalid or if the request fails.
   */
  protected upload<T>(url: string, data: Buffer | File, cancellationToken?: AbortController): Promise<T> {
    return new Promise((resolve, reject) => {
      if (!url) return reject(new Error('URL cannot be null or empty.'));
      const request = superagent
        .post(url)
        .set(this.config.defaultHeaders)
        .send(data)
        .timeout({ response: this.config.timeoutMs });
      // If a cancelToken is provided, attach the abort method
      if (cancellationToken) {
        cancellationToken.abort = () => {
          request.abort();
          this.log(SeverityEnum.Debug, `Request aborted to ${request.method}: ${url}.`);
        };
      }
      request
        .then((res) => {
          this.log(SeverityEnum.Debug, `Success reported from ${request.method}: ${url}: ${res.status}`);
          resolve(Serializer.deserializeJson(res.text || '{}'));
        })
        .catch((err) => {
          this.log(SeverityEnum.Warn, `Failed to retrieve object from ${request.method}: ${url}: ${err.message}`);
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
