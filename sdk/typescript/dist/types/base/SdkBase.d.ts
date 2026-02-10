import { SeverityEnum } from '../enums/SeverityEnum';
import { SdkConfiguration } from './SdkConfiguration';
/**
 * SDK Base class for making API calls with logging and timeout functionality.
 * @module SdkBase
 */
export default class SdkBase {
    private logger;
    protected config: SdkConfiguration;
    /**
     * Creates an instance of SdkBase.
     * @param {SdkConfiguration} config - The SDK configuration.
     * @throws {Error} Throws an error if the config is null.
     */
    constructor(config: SdkConfiguration);
    /**
     * Logs a message with a severity level.
     * @param {string} sev - The severity level (e.g., SeverityEnum.Debug, 'warn').
     * @param {string} msg - The message to log.
     */
    protected log(sev: SeverityEnum, msg: string): void;
    /**
     * Submits data using a POST request to a given URL.
     * @param {string} url - The URL to post data to.
     * @param {Buffer} data - The data to send in the POST request.
     * @param {AbortController} [cancellationToken] - Optional cancellation token for cancelling the request.
     * @return {Promise<Object>} Resolves with the response data.
     * @throws {Error | ApiErrorResponse} Rejects if the URL or data is invalid or if the request fails.
     */
    protected upload<T>(url: string, data: Buffer | File, cancellationToken?: AbortController): Promise<T>;
}
