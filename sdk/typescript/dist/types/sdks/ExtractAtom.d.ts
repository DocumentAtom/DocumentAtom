import SdkBase from '../base/SdkBase';
import { SdkConfiguration } from '../base/SdkConfiguration';
import { ExtractAtomResponse } from '../types';
export default class ExtractAtom extends SdkBase {
    constructor(config: SdkConfiguration);
    /**
     * Extracts the atoms from the document.
     * @param {Buffer | File} fileBinary - The binary of the Excel file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the document.
     */
    excel(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an HTML document.
     * @param {Buffer | File} fileBinary - The binary of the HTML file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the HTML document.
     */
    html(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a Markdown document.
     * @param {Buffer | File} fileBinary - The binary of the Markdown file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Markdown file.
     */
    markdown(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an OCR (image-based) document.
     * @param {Buffer | File} fileBinary - The binary of the OCR file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the OCR file.
     */
    ocr(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PDF document.
     * @param {Buffer | File} fileBinary - The binary of the PDF file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PDF file.
     */
    pdf(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PNG image document.
     * @param {Buffer | File} fileBinary - The binary of the PNG file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PNG file.
     */
    png(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PowerPoint (PPT or PPTX) document.
     * @param {Buffer | File} fileBinary - The binary of the PowerPoint file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PowerPoint file.
     */
    powerpoint(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an RTF (Rich Text Format) document.
     * @param {Buffer | File} fileBinary - The binary of the RTF file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the RTF file.
     */
    rtf(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a plain text (TXT) document.
     * @param {Buffer | File} fileBinary - The binary of the TXT file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the TXT file.
     */
    text(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a Word document (DOC or DOCX).
     * @param {Buffer | File} fileBinary - The binary of the Word file.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Word file.
     */
    word(fileBinary: Buffer | File, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
}
