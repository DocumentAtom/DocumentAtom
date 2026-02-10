"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
function _defineProperty(e, r, t) { return (r = _toPropertyKey(r)) in e ? Object.defineProperty(e, r, { value: t, enumerable: !0, configurable: !0, writable: !0 }) : e[r] = t, e; }
function _toPropertyKey(t) { var i = _toPrimitive(t, "string"); return "symbol" == typeof i ? i : i + ""; }
function _toPrimitive(t, r) { if ("object" != typeof t || !t) return t; var e = t[Symbol.toPrimitive]; if (void 0 !== e) { var i = e.call(t, r || "default"); if ("object" != typeof i) return i; throw new TypeError("@@toPrimitive must return a primitive value."); } return ("string" === r ? String : Number)(t); }
class GenericExceptionHandlers {}
exports.default = GenericExceptionHandlers;
/**
 * @param {string} argName
 */
_defineProperty(GenericExceptionHandlers, "ArgumentNullException", argName => {
  throw Error(`ArgumentNullException: ${argName} is null or empty`);
});
_defineProperty(GenericExceptionHandlers, "GenericException", message => {
  throw Error(message);
});
//# sourceMappingURL=GenericExceptionHandlers.js.map