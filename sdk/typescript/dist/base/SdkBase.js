"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
var _superagent = _interopRequireDefault(require("superagent"));
var _SeverityEnum = require("../enums/SeverityEnum");
var _GenericExceptionHandlers = _interopRequireDefault(require("../exception/GenericExceptionHandlers"));
var _Serializer = _interopRequireDefault(require("../utils/Serializer"));
var _Logger = _interopRequireDefault(require("../utils/Logger"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
/**
 * SDK Base class for making API calls with logging and timeout functionality.
 * @module SdkBase
 */
class SdkBase {
  /**
   * Creates an instance of SdkBase.
   * @param {SdkConfiguration} config - The SDK configuration.
   * @throws {Error} Throws an error if the config is null.
   */
  constructor(config) {
    _defineProperty(this, "logger", void 0);
    _defineProperty(this, "config", void 0);
    if (!config) {
      _GenericExceptionHandlers.default.ArgumentNullException('config');
    }
    this.config = config;
    this.logger = _Logger.default.log;
  }

  /**
   * Logs a message with a severity level.
   * @param {string} sev - The severity level (e.g., SeverityEnum.Debug, 'warn').
   * @param {string} msg - The message to log.
   */
  log(sev, msg) {
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
  upload(url, data, cancellationToken) {
    return new Promise((resolve, reject) => {
      if (!url) return reject(new Error('URL cannot be null or empty.'));
      const request = _superagent.default.post(url).set(this.config.defaultHeaders).send(data).timeout({
        response: this.config.timeoutMs
      });
      // If a cancelToken is provided, attach the abort method
      if (cancellationToken) {
        cancellationToken.abort = () => {
          request.abort();
          this.log(_SeverityEnum.SeverityEnum.Debug, `Request aborted to ${request.method}: ${url}.`);
        };
      }
      request.then(res => {
        this.log(_SeverityEnum.SeverityEnum.Debug, `Success reported from ${request.method}: ${url}: ${res.status}`);
        resolve(_Serializer.default.deserializeJson(res.text || '{}'));
      }).catch(err => {
        this.log(_SeverityEnum.SeverityEnum.Warn, `Failed to retrieve object from ${request.method}: ${url}: ${err.message}`);
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
exports.default = SdkBase;
//# sourceMappingURL=SdkBase.js.map