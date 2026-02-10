"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = exports.LoggerInstance = void 0;
var _SeverityEnum = require("../enums/SeverityEnum");
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
class Logger {}
exports.default = Logger;
/**
 * @param {string} type
 * @param {string} message
 */
_defineProperty(Logger, "log", (severity, message) => {
  switch (severity) {
    case _SeverityEnum.SeverityEnum.Warn:
      //eslint-disable-next-line no-console
      console.warn(message);
      break;
    case _SeverityEnum.SeverityEnum.Debug:
      //eslint-disable-next-line no-console
      console.debug(message);
      break;
    default:
      //eslint-disable-next-line no-console
      console.log(message);
  }
});
const LoggerInstance = exports.LoggerInstance = new Logger();
//# sourceMappingURL=Logger.js.map