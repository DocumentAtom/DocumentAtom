"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.SdkConfiguration = void 0;
var _GenericExceptionHandlers = _interopRequireDefault(require("../exception/GenericExceptionHandlers"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
class SdkConfiguration {
  /**
   * Creates an instance of SdkBase.
   * @param {string} endpoint - The API endpoint base URL.
   * @throws {Error} Throws an error if the endpoint is null or empty.
   */
  constructor(endpoint) {
    _defineProperty(this, "_endpoint", void 0);
    _defineProperty(this, "_timeoutMs", void 0);
    _defineProperty(this, "defaultHeaders", void 0);
    if (!endpoint) {
      _GenericExceptionHandlers.default.ArgumentNullException('Endpoint');
    }
    this.endpoint = endpoint.endsWith('/') ? endpoint : endpoint + '/';
    this.timeoutMs = 300000;
    this.defaultHeaders = {};
  }

  /**
   * Getter for the API endpoint.
   * @return {string} The endpoint URL.
   */
  get endpoint() {
    return this._endpoint;
  }

  /**
   * Setter for the API endpoint.
   * @param {string} value - The endpoint URL.
   * @throws {Error} Throws an error if the endpoint is null or empty.
   */
  set endpoint(value) {
    if (!value) {
      _GenericExceptionHandlers.default.ArgumentNullException('Endpoint');
    }
    this._endpoint = value.endsWith('/') ? value : value + '/';
  }

  /**
   * Getter for the timeout in milliseconds.
   * @return {number} The timeout in milliseconds.
   */
  get timeoutMs() {
    return this._timeoutMs;
  }

  /**
   * Setter for the timeout in milliseconds.
   * @param {number} value - Timeout value in milliseconds.
   * @throws {Error} Throws an error if the timeout is less than 1.
   */
  set timeoutMs(value) {
    if (value < 1) {
      _GenericExceptionHandlers.default.GenericException('TimeoutMs must be greater than 0.');
    }
    this._timeoutMs = value;
  }
}
exports.SdkConfiguration = SdkConfiguration;
//# sourceMappingURL=SdkConfiguration.js.map