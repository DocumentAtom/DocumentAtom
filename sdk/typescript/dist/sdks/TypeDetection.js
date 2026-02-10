"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
var _SdkBase = _interopRequireDefault(require("../base/SdkBase"));
var _GenericExceptionHandlers = _interopRequireDefault(require("../exception/GenericExceptionHandlers"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
/**
 * Type Detection SDK class.
 * Extends the SdkBase class.
 * @module  TypeDetection
 * @extends SdkBase
 */
class TypeDetection extends _SdkBase.default {
  constructor(config) {
    super(config);
  }

  /**
   * Detects the type of the data.
   * @param {Buffer | File} fileBinary - The binary of the file to detect the type of.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<TypeDetectionResponse>} The type of the data.
   */
  detectType(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}typedetect`;
    return this.upload(url, fileBinary, cancellationToken);
  }
}
exports.default = TypeDetection;
//# sourceMappingURL=TypeDetection.js.map