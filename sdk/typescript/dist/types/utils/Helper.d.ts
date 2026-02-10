import { SdkConfiguration } from '../base/SdkConfiguration';
export default class Helper {
    private config;
    /**
     * Creates an instance of Helper.
     * @param {SdkConfiguration} config - The SDK configuration.
     * @throws {Error} Throws an error if the config is null.
     */
    constructor(config: SdkConfiguration);
    /**
     * Converts a file to binary data.
     * @param {string} filePath - The path to the file.
     * @return {Buffer} The binary data of the file.
     * @throws {Error} Throws an error if the file is not found.
     */
    convertFileToBinary(filePath: string): Buffer;
}
