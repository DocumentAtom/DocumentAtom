import SdkBase from '../base/SdkBase';
import { SdkConfiguration } from '../base/SdkConfiguration';
import { ApiProcessorSettings, ExtractAtomResponse } from '../types';
export default class ExtractAtom extends SdkBase {
    constructor(config: SdkConfiguration);
    /**
     * Extracts the atoms from an Excel document.
     * @param {Buffer} fileBinary - The binary of the Excel file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the document.
     */
    excel(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a CSV document.
     * @param {Buffer} fileBinary - The binary of the CSV file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the CSV document.
     */
    csv(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an HTML document.
     * @param {Buffer} fileBinary - The binary of the HTML file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the HTML document.
     */
    html(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a JSON document.
     * @param {Buffer} fileBinary - The binary of the JSON file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms from the JSON document.
     */
    json(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a Markdown document.
     * @param {Buffer} fileBinary - The binary of the Markdown file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Markdown file.
     */
    markdown(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an OCR (image-based) document.
     * @param {Buffer} fileBinary - The binary of the OCR file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the OCR file.
     */
    ocr(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PDF document.
     * @param {Buffer} fileBinary - The binary of the PDF file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PDF file.
     */
    pdf(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PNG image document.
     * @param {Buffer} fileBinary - The binary of the PNG file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PNG file.
     */
    png(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a PowerPoint (PPT or PPTX) document.
     * @param {Buffer} fileBinary - The binary of the PowerPoint file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the PowerPoint file.
     */
    powerpoint(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an RTF (Rich Text Format) document.
     * @param {Buffer} fileBinary - The binary of the RTF file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the RTF file.
     */
    rtf(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a plain text (TXT) document.
     * @param {Buffer} fileBinary - The binary of the TXT file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the TXT file.
     */
    text(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from a Word document (DOC or DOCX).
     * @param {Buffer} fileBinary - The binary of the Word file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the Word file.
     */
    word(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    /**
     * Extracts the atoms from an XML document.
     * @param {Buffer} fileBinary - The binary of the XML file.
     * @param {ApiProcessorSettings} [settings] - Optional processor settings.
     * @param {AbortController} [cancellationToken] - The cancellation token.
     * @returns {Promise<ExtractAtomResponse>} The atoms extracted from the XML file.
     */
    xml(fileBinary: Buffer | File, settings?: ApiProcessorSettings, cancellationToken?: AbortController): Promise<ExtractAtomResponse>;
    private toBase64;
    private buildAtomRequest;
    private postAtomRequest;
}
