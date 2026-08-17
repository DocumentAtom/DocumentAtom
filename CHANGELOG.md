# Change Log

## Current Version

v3.1.1

### New Features
- Added `DocumentAtom.Core.Diagnostics.DocumentAtomDiagnostics` with shared `ActivitySource` and `Meter` instances for DocumentAtom hosts and clients
- Added core processor, type detection, and chunking spans and metrics
- Added REST server route instrumentation for request duration, status code, active request count, and request body size
- Added MCP server JSON-RPC instrumentation for request duration, active request count, request outcome, and connection count
- Added C# SDK outbound HTTP instrumentation for request duration, status code, route, and request body size
- Added DataIngestion processing instrumentation for document duration and emitted chunk counts

### Packaging
- Bumped NuGet package versions from `3.1.0` to `3.1.1`
- Aligned package release notes with the diagnostics release
- Bumped the non-packable MCP server project patch version from `1.0.0` to `1.0.1`
- Removed the unused direct `SixLabors.ImageSharp` dependency from `DocumentAtom.Documents`

## Previous Versions

v3.0.x

### Breaking Changes
- REST API now requires a JSON envelope (`{ "Settings": ..., "Data": "<base64>" }`) for all endpoints; raw binary POST is no longer supported
- The `?ocr` querystring parameter is removed from all endpoints; use `Settings.ExtractAtomsFromImages` instead
- `ChunkingSettings` class deleted and replaced by `ChunkingConfiguration` with strategy-based chunking
- C# SDK: `bool extractOcr` parameter replaced with `ApiProcessorSettings? settings` on all processing methods
- TypeScript SDK: `settings?: ApiProcessorSettings` parameter added to all processing methods
- Python SDK: `ocr: Optional[bool]` parameter replaced with `settings: Optional[Any]` on all processing methods
- DataIngestion `AtomChunkerOptions`: removed `MaxChunkSize`, `ChunkOverlap`, `PreserveParagraphs`, and `SplitSeparators` in favor of `ChunkingConfiguration`

### New Features
- 11 chunking strategies: `FixedTokenCount`, `SentenceBased`, `ParagraphBased`, `RegexBased`, `WholeList`, `ListEntry`, `Row`, `RowWithHeaders`, `RowGroupWithHeaders`, `KeyValuePairs`, `WholeTable`
- 3 overlap strategies: `SlidingWindow`, `SentenceBoundaryAware`, `SemanticBoundaryAware`
- New `ChunkingEngine` dispatcher with SharpToken `cl100k_base` tokenizer for accurate token counting
- New `Chunk` class with `Position`, `Length`, `Text`, and content hashes (`MD5`, `SHA1`, `SHA256`)
- `Atom.Chunks` property for storing chunking results separately from structural `Quarks`
- Per-request processor settings via `ApiProcessorSettings` and `AtomRequest` envelope; security-sensitive server settings are excluded from the API surface
- TypeScript SDK: added missing `csv()` and `xml()` methods
- Python SDK: client-side OCR `ExtractionResult` to `List[AtomModel]` conversion
- Dashboard: new `ProcessorSettingsModal` component for configuring per-request settings including chunking

### Testing
- New `Test.Chunking` project with unit tests for all chunking components (11 test classes)
- New `Test.AutomatedHarness` integration test project with 42 tests across 6 categories
- Shared `sdk/test-fixtures/` directory with sample files for all three SDK test harnesses

### Other
- MCP server registrations updated to use `ApiProcessorSettings` instead of `extractOcr` boolean
- Updated Postman collection to reflect JSON envelope API

v2.0.x

- MCP (Model Context Protocol) server with HTTP, TCP, and WebSocket support for all document types
- MCP Docker build and deployment infrastructure with health checks
- Microsoft.Extensions.AI integration
- C# SDK (`DocumentAtom.Sdk`) for programmatic access to the server
- TypeScript and Python SDKs with support for all document processing endpoints
- Better document format exception handling (wrong type sent to endpoint)
- Dashboard security fixes (Dependabot alerts resolved)
- Docker improvements and fixes

v1.1.x

- Hierarchical atomization (see `BuildHierarchy` in settings) - heading-based for markdown/HTML/Word, page-based for PowerPoint
- Support for CSV, JSON, and XML documents
- Dependency updates and fixes

v1.0.x

- Initial release
