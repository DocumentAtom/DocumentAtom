export interface TypeDetectionResponse {
    MimeType: string;
    Extension: string;
    Type: string;
}
export type ExtractAtomResponse = Atom[];
export type ChunkStrategyEnum = 'FixedTokenCount' | 'SentenceBased' | 'ParagraphBased' | 'RegexBased' | 'WholeList' | 'ListEntry' | 'Row' | 'RowWithHeaders' | 'RowGroupWithHeaders' | 'KeyValuePairs' | 'WholeTable';
export type OverlapStrategyEnum = 'SlidingWindow' | 'SentenceBoundaryAware' | 'SemanticBoundaryAware';
export interface ChunkingConfiguration {
    Enable?: boolean;
    Strategy?: ChunkStrategyEnum;
    FixedTokenCount?: number;
    OverlapCount?: number;
    OverlapPercentage?: number;
    OverlapStrategy?: OverlapStrategyEnum;
    RowGroupSize?: number;
    ContextPrefix?: string;
    RegexPattern?: string;
}
export interface ApiProcessorSettings {
    TrimText?: boolean;
    RemoveBinaryFromText?: boolean;
    ExtractAtomsFromImages?: boolean;
    Chunking?: ChunkingConfiguration;
    ColumnDelimiter?: string;
    HasHeaderRow?: boolean;
    RowsPerAtom?: number;
    BuildHierarchy?: boolean;
    ExtractMetadata?: boolean;
    MaxDepth?: number;
    PropertyHandling?: string;
    PreserveOriginalStructure?: boolean;
    RootElementName?: string;
    IncludeAttributes?: boolean;
    IncludeComments?: boolean;
    SheetNames?: string[];
    IncludeFormulas?: boolean;
    IncludeHiddenSheets?: boolean;
}
export interface AtomRequest {
    Settings?: ApiProcessorSettings;
    Data: string;
}
export interface Chunk {
    Position: number;
    Length: number;
    Text: string;
    MD5Hash: string;
    SHA1Hash: string;
    SHA256Hash: string;
}
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
    Chunks?: Chunk[];
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
