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
   * Extracts the atoms from the document.
   * @param {Buffer | File} fileBinary - The binary of the Excel file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the document.
   */
  excel(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/excel`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from an HTML document.
   * @param {Buffer | File} fileBinary - The binary of the HTML file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms from the HTML document.
   */
  html(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/html`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a Markdown document.
   * @param {Buffer | File} fileBinary - The binary of the Markdown file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Markdown file.
   */
  markdown(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/markdown`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from an OCR (image-based) document.
   * @param {Buffer | File} fileBinary - The binary of the OCR file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the OCR file.
   */
  ocr(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/ocr`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a PDF document.
   * @param {Buffer | File} fileBinary - The binary of the PDF file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PDF file.
   */
  pdf(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/pdf`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a PNG image document.
   * @param {Buffer | File} fileBinary - The binary of the PNG file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PNG file.
   */
  png(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/png`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a PowerPoint (PPT or PPTX) document.
   * @param {Buffer | File} fileBinary - The binary of the PowerPoint file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PowerPoint file.
   */
  powerpoint(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/powerpoint?ocr`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from an RTF (Rich Text Format) document.
   * @param {Buffer | File} fileBinary - The binary of the RTF file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the RTF file.
   */
  rtf(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/rtf`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a plain text (TXT) document.
   * @param {Buffer | File} fileBinary - The binary of the TXT file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the TXT file.
   */
  text(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('fileBinary');
    }
    const url = `${this.config.endpoint}atom/text`;
    return this.upload(url, fileBinary, cancellationToken);
  }

  /**
   * Extracts the atoms from a Word document (DOC or DOCX).
   * @param {Buffer | File} fileBinary - The binary of the Word file.
   * @param {AbortController} [cancellationToken] - The cancellation token.
   * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Word file.
   */
  word(fileBinary, cancellationToken) {
    if (!fileBinary) {
      _GenericExceptionHandlers.default.ArgumentNullException('filePath');
    }
    const url = `${this.config.endpoint}atom/word`;
    return this.upload(url, fileBinary, cancellationToken);
  }
}
exports.default = ExtractAtom;
//# sourceMappingURL=ExtractAtom.js.map