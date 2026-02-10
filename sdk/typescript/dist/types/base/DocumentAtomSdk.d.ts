import { SdkConfiguration } from './SdkConfiguration';
import TypeDetection from '../sdks/TypeDetection';
import ExtractAtom from '../sdks/ExtractAtom';
import Helper from '../utils/Helper';
/**
 * Document Atom SDK class.
 * Extends the SdkBase class.
 * @module  DocumentAtomSdk
 * @extends SdkBase
 */
export default class DocumentAtomSdk {
    config: SdkConfiguration;
    TypeDetection: TypeDetection;
    ExtractAtom: ExtractAtom;
    Helper: Helper;
    /**
     * Instantiate the SDK.
     * @param {string} endpoint - The endpoint URL.
     */
    constructor(endpoint?: string);
    /**
     * Validates API connectivity using a HEAD request.
     * @param {AbortController} [cancellationToken] - Optional cancellation token for cancelling the request.
     * @return {Promise<boolean>} Resolves to true if the connection is successful.
     * @throws {Error} Rejects with the error in case of failure.
     */
    validateConnectivity(cancellationToken?: AbortController): Promise<boolean>;
}
