# Change Log

## Current Version

v3.2.1

### Changes
- Upgraded the `TextChunker` dependency from `0.1.0` to `0.2.0`

### Packaging
- Bumped NuGet package versions from `3.2.0` to `3.2.1`

## Previous Version

v3.2.0

### New Features
- Migrated all chunking onto the **TextChunker** engine (NuGet `TextChunker` 0.1.0), replacing the in-house `SharpToken`-based strategy implementations with a capability-superset engine; the public `ChunkingEngine`, `ChunkingConfiguration`, and `Chunk` contracts are preserved
- Added the `Recursive` chunking strategy (descending separator-ladder splitting), bringing the total to 12 strategies
- Added selectable tokenizer families to `ChunkingConfiguration` via `TokenizerKind` (`Auto`, `Cl100kBase`, `O200kBase`, `BertWordPiece`) with `ModelId`-based resolution
- Added character-based overlap (`OverlapCharacters`), hierarchy-aware chunking (`HierarchyAware`, `ContextualizeHeaders`, `HeaderContextSeparator`), recursive-format selection (`Format`, `Separators`), small-chunk handling (`SmallChunkMode`, `MinChunkTokens`), and per-run toggles (`TrimWhitespace`, `ComputeTokenCounts`, `ComputeOffsets`, `ComputeHashes`)
- Enriched the `Chunk` output model with `GUID`, `ParentGUID`, `TokenCount`, `StartOffset`, `EndOffset`, and `HeaderContext`
- Reworked the DataIngestion `HierarchyAwareChunker` onto TextChunker's native hierarchy-aware recursive chunking
- Added `--install` / `--uninstall` commands to `DocumentAtom.McpServer` to register (or remove) the MCP server in the per-user configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux; supports `--endpoint=<url>` and per-tool targeting (`--install=claude,cursor`), preserving existing entries
- Added cross-platform install/uninstall scripts under `scripts/` (`windows/` PowerShell + CMD, `macos/` and `linux/` Bash) that wrap the install command
- Added `MCP_API.md` documenting the MCP server transports, tool catalog, request/response schemas, and usage examples

### Fixes
- Fixed fixed-token chunks occasionally exceeding the token budget, and normalized the percentage-overlap unit count, by delegating to TextChunker's strict token slicing
- Table cells containing `|` are now escaped so serialized markdown tables remain valid
- The REST atom routes now populate each chunk's `HeaderContext` from the atom hierarchy (`ParentGUID` / `HeaderLevel`) that the processor builds, so header breadcrumbs (for example `Authentication Requirements > AAA Model`) appear on chunks from the markdown/Word/etc. routes and not only via the DataIngestion pipeline; previously this field was always null on those routes because documents are atomized per element before chunking

### Documentation
- Documented the MCP install command, scripts, and `MCP_API.md` in `README.md`
- Corrected the MCP server environment variable names in `README.md` (`MCP_WS_HOSTNAME`, `MCP_WS_PORT`, `MCP_CONSOLE_LOGGING`) and added the telemetry override variables
- Updated `docker/compose.yaml` and README Docker image tags to `v3.2.0`

### Packaging
- Bumped NuGet package versions from `3.1.2` to `3.2.0`
- Replaced the `SharpToken` dependency with `TextChunker` 0.1.0 in `DocumentAtom.Core`

## Previous Versions

v3.1.2

### New Features
- Added `DocumentAtom.Telemetry` host that wires the existing `Meter`/`ActivitySource` instruments into an OpenTelemetry export pipeline (OTLP metrics and traces) for the REST server and MCP server
- Added `Telemetry` settings to the server and MCP configurations (OTLP endpoint/protocol/headers, sampling ratio, export interval, in-process Prometheus, runtime and process metrics); enabled by default to `http://localhost:4317`
- Added `DOCUMENTATOM_TELEMETRY_ENABLE`, `DOCUMENTATOM_OTLP_ENDPOINT`, and `DOCUMENTATOM_OTLP_PROTOCOL` environment overrides for the MCP server
- Added process (working set, uptime, thread count) and .NET runtime metrics
- Added a bundled observability stack to `docker/compose.yaml` (OpenTelemetry Collector, Prometheus, Tempo, Loki, Grafana on host port 3001) with provisioned datasources and Grafana dashboards (Overview, HTTP, MCP, Processing, SDK, Runtime) in a top-level `DocumentAtom` folder
- Added an **Observability** tab to the dashboard UI with cards linking out to Grafana, Prometheus, Tempo, and Loki (name, default credentials, and URL)
- Added `TELEMETRY.md` documenting the metrics/traces catalog, configuration, and integration with external observability stacks

### Packaging
- Bumped NuGet package versions from `3.1.1` to `3.1.2`

## Previous Versions

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

## Older Versions

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
