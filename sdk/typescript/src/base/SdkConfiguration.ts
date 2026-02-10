import GenericExceptionHandlers from '../exception/GenericExceptionHandlers';

export class SdkConfiguration {
  private _endpoint: string;
  private _timeoutMs: number;
  public defaultHeaders: any;

  /**
   * Creates an instance of SdkBase.
   * @param {string} endpoint - The API endpoint base URL.
   * @throws {Error} Throws an error if the endpoint is null or empty.
   */
  constructor(endpoint: string) {
    if (!endpoint) {
      GenericExceptionHandlers.ArgumentNullException('Endpoint');
    }

    this.endpoint = endpoint.endsWith('/') ? endpoint : endpoint + '/';
    this.timeoutMs = 300000;
    this.defaultHeaders = {};
  }

  /**
   * Getter for the API endpoint.
   * @return {string} The endpoint URL.
   */
  get endpoint(): string {
    return this._endpoint;
  }

  /**
   * Setter for the API endpoint.
   * @param {string} value - The endpoint URL.
   * @throws {Error} Throws an error if the endpoint is null or empty.
   */
  set endpoint(value: string) {
    if (!value) {
      GenericExceptionHandlers.ArgumentNullException('Endpoint');
    }
    this._endpoint = value.endsWith('/') ? value : value + '/';
  }

  /**
   * Getter for the timeout in milliseconds.
   * @return {number} The timeout in milliseconds.
   */
  get timeoutMs(): number {
    return this._timeoutMs;
  }

  /**
   * Setter for the timeout in milliseconds.
   * @param {number} value - Timeout value in milliseconds.
   * @throws {Error} Throws an error if the timeout is less than 1.
   */
  set timeoutMs(value: number) {
    if (value < 1) {
      GenericExceptionHandlers.GenericException('TimeoutMs must be greater than 0.');
    }
    this._timeoutMs = value;
  }
}
