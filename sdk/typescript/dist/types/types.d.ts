export interface TypeDetectionResponse {
    MimeType: string;
    Extension: string;
    Type: string;
}
export type ExtractAtomResponse = Atom[];
export interface Atom {
    GUID: string;
    Type: string;
    Position: number;
    Length: number;
    MD5Hash: string;
    SHA1Hash: string;
    SHA256Hash: string;
    Formatting: string;
    Text: string;
    HeaderLevel?: number;
    Quarks?: Quark[];
}
export interface Quark {
    ParentGUID: string;
    GUID: string;
    Type: string;
    Position: number;
    Length: number;
    MD5Hash: string;
    SHA1Hash: string;
    SHA256Hash: string;
    Formatting: string;
    Text: string;
    Table?: any;
    HeaderLevel?: number;
    Quarks?: any[];
}
