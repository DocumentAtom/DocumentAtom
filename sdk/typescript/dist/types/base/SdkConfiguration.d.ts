export declare class SdkConfiguration {
    private _endpoint;
    private _timeoutMs;
    defaultHeaders: any;
    /**
     * Creates an instance of SdkBase.
     * @param {string} endpoint - The API endpoint base URL.
     * @throws {Error} Throws an error if the endpoint is null or empty.
     */
    constructor(endpoint: string);
    /**
     * Getter for the API endpoint.
     * @return {string} The endpoint URL.
     */
    get endpoint(): string;
    /**
     * Setter for the API endpoint.
     * @param {string} value - The endpoint URL.
     * @throws {Error} Throws an error if the endpoint is null or empty.
     */
    set endpoint(value: string);
    /**
     * Getter for the timeout in milliseconds.
     * @return {number} The timeout in milliseconds.
     */
    get timeoutMs(): number;
    /**
     * Setter for the timeout in milliseconds.
     * @param {number} value - Timeout value in milliseconds.
     * @throws {Error} Throws an error if the timeout is less than 1.
     */
    set timeoutMs(value: number);
}
