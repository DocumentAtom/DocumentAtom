import fs from 'fs';
import GenericExceptionHandlers from '../exception/GenericExceptionHandlers';
import { SdkConfiguration } from '../base/SdkConfiguration';

export default class Helper {
  private config: SdkConfiguration;
  /**
   * Creates an instance of Helper.
   * @param {SdkConfiguration} config - The SDK configuration.
   * @throws {Error} Throws an error if the config is null.
   */
  constructor(config: SdkConfiguration) {
    if (!config) {
      GenericExceptionHandlers.ArgumentNullException('config');
    }
    this.config = config;
  }
  /**
   * Converts a file to binary data.
   * @param {string} filePath - The path to the file.
   * @return {Buffer} The binary data of the file.
   * @throws {Error} Throws an error if the file is not found.
   */
  public convertFileToBinary(filePath: string): Buffer {
    if (!fs.existsSync(filePath)) {
      throw GenericExceptionHandlers.GenericException(`File not found at ${filePath}`);
    }
    const buf = fs.readFileSync(filePath); // Buffer with raw bytes
    return buf;
  }
}
