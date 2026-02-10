"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
var _fs = _interopRequireDefault(require("fs"));
var _GenericExceptionHandlers = _interopRequireDefault(require("../exception/GenericExceptionHandlers"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
class Helper {
  /**
   * Creates an instance of Helper.
   * @param {SdkConfiguration} config - The SDK configuration.
   * @throws {Error} Throws an error if the config is null.
   */
  constructor(config) {
    _defineProperty(this, "config", void 0);
    if (!config) {
      _GenericExceptionHandlers.default.ArgumentNullException('config');
    }
    this.config = config;
  }
  /**
   * Converts a file to binary data.
   * @param {string} filePath - The path to the file.
   * @return {Buffer} The binary data of the file.
   * @throws {Error} Throws an error if the file is not found.
   */
  convertFileToBinary(filePath) {
    if (!_fs.default.existsSync(filePath)) {
      throw _GenericExceptionHandlers.default.GenericException(`File not found at ${filePath}`);
    }
    const buf = _fs.default.readFileSync(filePath); // Buffer with raw bytes
    return buf;
  }
}
exports.default = Helper;
//# sourceMappingURL=Helper.js.map