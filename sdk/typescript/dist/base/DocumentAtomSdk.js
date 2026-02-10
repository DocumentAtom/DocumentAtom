"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
var _superagent = _interopRequireDefault(require("superagent"));
var _SdkConfiguration = require("./SdkConfiguration");
var _SeverityEnum = require("../enums/SeverityEnum");
var _Logger = _interopRequireDefault(require("../utils/Logger"));
var _TypeDetection = _interopRequireDefault(require("../sdks/TypeDetection"));
var _ExtractAtom = _interopRequireDefault(require("../sdks/ExtractAtom"));
var _Helper = _interopRequireDefault(require("../utils/Helper"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
/**
 * Document Atom SDK class.
 * Extends the SdkBase class.
 * @module  DocumentAtomSdk
 * @extends SdkBase
 */
class DocumentAtomSdk {
  /**
   * Instantiate the SDK.
   * @param {string} endpoint - The endpoint URL.
   */

  constructor(endpoint = 'http://localhost:8000/') {
    _defineProperty(this, "config", void 0);
    _defineProperty(this, "TypeDetection", void 0);
    _defineProperty(this, "ExtractAtom", void 0);
    _defineProperty(this, "Helper", void 0);
    const config = new _SdkConfiguration.SdkConfiguration(endpoint);
    this.config = config;
    this.TypeDetection = new _TypeDetection.default(config);
    this.ExtractAtom = new _ExtractAtom.default(config);
    this.Helper = new _Helper.default(config);
  }

  /**
   * Validates API connectivity using a HEAD request.
   * @param {AbortController} [cancellationToken] - Optional cancellation token for cancelling the request.
   * @return {Promise<boolean>} Resolves to true if the connection is successful.
   * @throws {Error} Rejects with the error in case of failure.
   */
  validateConnectivity(cancellationToken) {
    return new Promise((resolve, reject) => {
      const request = _superagent.default.head(this.config.endpoint).timeout({
        response: this.config.timeoutMs
      });
      // If a cancelToken is provided, attach the abort method
      if (cancellationToken) {
        cancellationToken.abort = () => {
          request.abort();
          _Logger.default.log(_SeverityEnum.SeverityEnum.Debug, `Request aborted.`);
        };
      }
      request.then(res => {
        _Logger.default.log(_SeverityEnum.SeverityEnum.Debug, `Success reported from ${this.config.endpoint}`);
        resolve(res.ok);
      }).catch(err => {
        _Logger.default.log(_SeverityEnum.SeverityEnum.Warn, `Failed to retrieve object from ${this.config.endpoint}: ${err.message}`);
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
exports.default = DocumentAtomSdk;
//# sourceMappingURL=DocumentAtomSdk.js.map