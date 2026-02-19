"use strict";

Object.defineProperty(exports, "__esModule", {
  value: true
});
exports.default = void 0;
var _SdkBase = _interopRequireDefault(require("../base/SdkBase"));
var _GenericExceptionHandlers = _interopRequireDefault(require("../exception/GenericExceptionHandlers"));
function _interopRequireDefault(e) { return e && e.__esModule ? e : { default: e }; }
class ExtractAtom extends _SdkBase.default {
  constructor(config) {
    super(config);
  }

  /**
   * Extracts the atoms from an Excel document.
   * @param {Buffer} fileBinary - The binary of the Excel file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the document.
   */
  excel(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/excel', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a CSV document.
   * @param {Buffer} fileBinary - The binary of the CSV file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the CSV document.
   */
  csv(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/csv', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from an HTML document.
   * @param {Buffer} fileBinary - The binary of the HTML file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the HTML document.
   */
  html(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/html', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a JSON document.
   * @param {Buffer} fileBinary - The binary of the JSON file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the JSON document.
   */
  json(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/json', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a Markdown document.
   * @param {Buffer} fileBinary - The binary of the Markdown file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Markdown file.
   */
  markdown(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/markdown', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from an OCR (image-based) document.
   * @param {Buffer} fileBinary - The binary of the OCR file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the OCR file.
   */
  ocr(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/ocr', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a PDF document.
   * @param {Buffer} fileBinary - The binary of the PDF file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PDF file.
   */
  pdf(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/pdf', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a PNG image document.
   * @param {Buffer} fileBinary - The binary of the PNG file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PNG file.
   */
  png(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/png', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a PowerPoint (PPT or PPTX) document.
   * @param {Buffer} fileBinary - The binary of the PowerPoint file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PowerPoint file.
   */
  powerpoint(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/powerpoint', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from an RTF (Rich Text Format) document.
   * @param {Buffer} fileBinary - The binary of the RTF file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the RTF file.
   */
  rtf(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/rtf', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a plain text (TXT) document.
   * @param {Buffer} fileBinary - The binary of the TXT file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the TXT file.
   */
  text(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/text', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from a Word document (DOC or DOCX).
   * @param {Buffer} fileBinary - The binary of the Word file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Word file.
   */
  word(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/word', fileBinary, settings, cancellationToken);
  }

  /**
   * Extracts the atoms from an XML document.
   * @param {Buffer} fileBinary - The binary of the XML file.
   * @param {ApiProcessorSettings} [settings] - Optional processor settings.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the XML file.
   */
  xml(fileBinary, settings, cancellationToken) {
    return this.postAtomRequest('atom/xml', fileBinary, settings, cancellationToken);
  }
  async toBase64(fileBinary) {
    if (fileBinary instanceof File) {
      const arrayBuffer = await fileBinary.arrayBuffer();
      const bytes = new Uint8Array(arrayBuffer);
      let binary = '';
      for (let i = 0; i < bytes.length; i++) {
        binary += String.fromCharCode(bytes[i]);
      }
      return btoa(binary);
    }
    return fileBinary.toString('base64');
  }
  async buildAtomRequest(fileBinary, settings) {
    const base64Data = await this.toBase64(fileBinary);
    return {
      Data: base64Data,
      Settings: settings
    };
  }
  async postAtomRequest(route, fileBinary, settings, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}${route}`;
    const body = await this.buildAtomRequest(fileBinary, settings);
    return this.postJson(url, body, cancellationToken);
  }
}
exports.default = ExtractAtom;
//# sourceMappingURL=ExtractAtom.js.map