<img src="https://raw.githubusercontent.com/jchristn/DocumentAtom/refs/heads/main/assets/icon.png" width="256" height="256">

# DocumentAtom

DocumentAtom provides a light, fast library for breaking input documents into constituent parts (atoms), useful for text processing, analysis, and artificial intelligence.

DocumentAtom requires that Tesseract v5.0 be installed on the host.  This is required as certain document types can have embedded images which are parsed using OCR via Tesseract.

## Packages

| Package | Description | Version |
|---------|-------------|---------|
| `DocumentAtom` | Metapackage (includes Core, Text, Documents) | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom/) |
| `DocumentAtom.Core` | Core classes and TextTools | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Core.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Core/) |
| `DocumentAtom.Text` | Text processors (CSV, JSON, XML, HTML, Markdown) | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Text.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Text/) |
| `DocumentAtom.Documents` | Document processors (Word, Excel, PowerPoint, PDF, Image, OCR) | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Documents.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Documents/) |
| `DocumentAtom.DataIngestion` | Microsoft.Extensions.AI integration for RAG | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.DataIngestion.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.DataIngestion/) |

## SDKs

SDKs are available for multiple languages in the `sdk/` directory:

| SDK | Location | Description |
|-----|----------|-------------|
| TypeScript/JavaScript | `sdk/typescript/` | Full-featured SDK for Node.js and browser |
| Python | `sdk/python/` | Python SDK for data science workflows |
| C# | `sdk/csharp/` | .NET SDK client library |

## New in v3.0.0 (Breaking)

v3.0 replaces the raw-binary POST API with a structured **JSON envelope**. Every extraction request now carries an optional `Settings` object alongside the document data, giving callers per-request control over parsing, processing, and chunking — without any server-side configuration changes.

### Why a JSON Envelope?

In v2, callers uploaded raw bytes and got back atoms produced with whatever defaults the server happened to have. If you needed OCR, you appended `?ocr=true`. If you needed different CSV delimiters, paragraph grouping, or chunk sizes, you had to change server configuration and redeploy.

v3 puts the caller in control:

- **Tune parsing per request.** Tell the CSV processor that your file uses `|` delimiters and has no header row — in the same request that uploads the file. Process one HTML page with script extraction enabled and another without.
- **Enable and configure chunking on the fly.** Choose from 11 chunking strategies, set token budgets, pick an overlap mode, and get back pre-chunked content ready for embedding or retrieval — all without touching the server.
- **Keep sensible defaults.** Every setting is optional. Send `"Settings": null` and the server behaves exactly like v2 defaults. Override only what you need.
- **One API surface, any language.** The same JSON envelope works identically across the C#, TypeScript, and Python SDKs, the REST API, and the dashboard.

### Breaking Changes

- **JSON envelope required**: All `/atom/*` endpoints now accept `application/json` with base64-encoded document data instead of raw binary upload
- **`?ocr` query parameter removed**: Use `Settings.ExtractAtomsFromImages` in the JSON body instead
- **SDK method signatures changed**: All SDK methods now accept an optional settings object instead of `bool extractOcr`

### JSON Envelope Format

All atom extraction requests use this format:

```json
{
  "Settings": {
    "TrimText": true,
    "ExtractAtomsFromImages": true,
    "Chunking": {
      "Enable": true,
      "Strategy": "SentenceBased",
      "FixedTokenCount": 256,
      "OverlapCount": 2,
      "OverlapStrategy": "SentenceBoundaryAware"
    }
  },
  "Data": "<base64-encoded-document>"
}
```

When `Settings` is `null`, the server uses default processor settings for the target document type. Any field you omit from `Settings` retains its server default — you only specify the values you want to override.

### Per-Request Processing Settings

Every setting in the `Settings` object is optional. Omitted fields keep server defaults.

**Common settings** (available on all processor types except HTML):

| Setting | Type | Description |
|---------|------|-------------|
| `TrimText` | `bool` | Trim whitespace from extracted text |
| `RemoveBinaryFromText` | `bool` | Strip binary data from text output |
| `ExtractAtomsFromImages` | `bool` | Enable OCR on embedded images (requires Tesseract on host) |
| `Chunking` | `object` | Chunking configuration (see below) |

**Type-specific settings:**

| Setting | Type | Applies To | Description |
|---------|------|------------|-------------|
| `RowDelimiter` | `string` | CSV | Row delimiter string |
| `ColumnDelimiter` | `char` | CSV | Column delimiter character |
| `HasHeaderRow` | `bool` | CSV | Whether first row is a header |
| `RowsPerAtom` | `int` | CSV | Number of rows per atom |
| `BuildHierarchy` | `bool` | Excel, HTML, JSON, Markdown, Word, PowerPoint, XML | Build hierarchical atom structure from headings/sections |
| `Delimiters` | `string[]` | Markdown, Text | Custom content delimiters |
| `MaxDepth` | `int` | JSON, XML | Maximum nesting depth to process |
| `IncludeAttributes` | `bool` | XML | Include XML attributes in atoms |
| `PreserveWhitespace` | `bool` | HTML, XML | Preserve whitespace in output |
| `HeaderRowScoreThreshold` | `int` | Excel | Threshold score for header row detection |
| `ProcessInlineStyles` | `bool` | HTML | Process inline CSS styles |
| `ProcessMetaTags` | `bool` | HTML | Include meta tag content |
| `ProcessScripts` | `bool` | HTML | Include script content |
| `ProcessComments` | `bool` | HTML | Include HTML comments |
| `MaxTextLength` | `int` | HTML | Maximum text length to process |
| `ProcessSvg` | `bool` | HTML | Process SVG elements |
| `ExtractDataAttributes` | `bool` | HTML | Extract `data-*` attributes |
| `LineThreshold` | `int` | OCR, PNG | Line detection threshold |
| `ParagraphThreshold` | `int` | OCR, PNG | Paragraph detection threshold |
| `HorizontalLineLength` | `int` | OCR, PNG | Minimum horizontal line length |
| `VerticalLineLength` | `int` | OCR, PNG | Minimum vertical line length |
| `TableMinArea` | `int` | OCR, PNG | Minimum area for table detection |
| `ColumnAlignmentTolerance` | `int` | OCR, PNG | Column alignment tolerance |
| `ProximityThreshold` | `int` | OCR, PNG | Element proximity threshold |

### Server-Side Chunking

v3.0 introduces server-side chunking with 11 strategies. When enabled, each `Atom` in the response includes a `Chunks` array of content fragments — ready for embedding, vector storage, or retrieval without any client-side post-processing.

**Chunking Strategies:**

| Strategy | Description | Use Case |
|----------|-------------|----------|
| `FixedTokenCount` | Splits text into fixed token-count windows using cl100k_base | Embedding models with fixed context windows |
| `SentenceBased` | Groups sentences up to a token budget | RAG pipelines, semantic search |
| `ParagraphBased` | Groups paragraphs up to a token budget | Summarization, longer context |
| `RegexBased` | Splits on a user-supplied regex pattern | Domain-specific delimiters |
| `WholeList` | Serializes an entire list atom as one chunk | Short lists that shouldn't be split |
| `ListEntry` | Each list item becomes its own chunk | FAQ lists, bullet-point extraction |
| `Row` | Each table row becomes a chunk (no headers) | Simple tabular data |
| `RowWithHeaders` | Each row becomes a chunk prefixed with column headers | Tabular data needing context |
| `RowGroupWithHeaders` | Groups N rows together with headers | Large tables, batch processing |
| `KeyValuePairs` | Each row becomes `Header: Value` pairs | Structured data extraction |
| `WholeTable` | Entire table serialized as one chunk | Small tables, preserving structure |

**Overlap Strategies** (for text-based chunking):

| Strategy | Description |
|----------|-------------|
| `SlidingWindow` | Overlaps by raw token/sentence/paragraph count |
| `SentenceBoundaryAware` | Overlap snaps to sentence boundaries |
| `SemanticBoundaryAware` | Overlap snaps to paragraph boundaries |

**ChunkingConfiguration Fields:**

| Field | Type | Default | Description |
|-------|------|---------|-------------|
| `Enable` | `bool` | `false` | Enable/disable chunking |
| `Strategy` | `string` | `FixedTokenCount` | One of the 11 strategies above |
| `FixedTokenCount` | `int` | `256` | Token budget per chunk (min: 1) |
| `OverlapCount` | `int` | `0` | Number of overlap units (min: 0) |
| `OverlapPercentage` | `double` | `null` | Overlap as percentage (0.0-1.0); when set, takes precedence over `OverlapCount` |
| `OverlapStrategy` | `string` | `SlidingWindow` | One of the 3 overlap strategies |
| `RowGroupSize` | `int` | `5` | Rows per group for `RowGroupWithHeaders` (min: 1) |
| `ContextPrefix` | `string` | `null` | Text prepended to each chunk |
| `RegexPattern` | `string` | `null` | Split pattern for `RegexBased` strategy |

### Chunks vs Quarks

- **Quarks** are structural sub-atoms produced during extraction. They represent the document's inherent hierarchy and parent-child relationships — for example, cells within a table row, items within a list, or paragraphs within a heading section.
- **Chunks** are fragments of an atom's content produced by the chunking engine based on the selected chunking strategy. They are designed for downstream consumption such as embedding, vector storage, and retrieval.

An atom can have both quarks (structure, hierarchy, parent-child relationships) and chunks (fragments of atom data based on chunking strategy).

### SDK Examples

**C#:**
```csharp
using DocumentAtom.Sdk;
using DocumentAtom.Core.Api;

var sdk = new DocumentAtomSdk("http://localhost:8000");
byte[] data = File.ReadAllBytes("document.pdf");

// Without settings (server defaults)
List<Atom>? atoms = await sdk.Atom.ProcessPdf(data);

// With settings: enable OCR and sentence-based chunking
var settings = new ApiProcessorSettings
{
    ExtractAtomsFromImages = true,
    Chunking = new ChunkingConfiguration
    {
        Enable = true,
        Strategy = ChunkStrategyEnum.SentenceBased,
        FixedTokenCount = 256,
        OverlapCount = 2,
        OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware
    }
};
List<Atom>? atoms = await sdk.Atom.ProcessPdf(data, settings);
```

**TypeScript:**
```typescript
import DocumentAtomSdk from 'document-atom-sdk';

const sdk = new DocumentAtomSdk({ endpoint: 'http://localhost:8000' });
const fileBuffer = fs.readFileSync('document.pdf');

// Without settings
const atoms = await sdk.extractAtom.pdf(fileBuffer);

// With settings: enable OCR and sentence-based chunking
const atoms = await sdk.extractAtom.pdf(fileBuffer, {
  ExtractAtomsFromImages: true,
  Chunking: {
    Enable: true,
    Strategy: 'SentenceBased',
    FixedTokenCount: 256,
    OverlapCount: 2,
    OverlapStrategy: 'SentenceBoundaryAware',
  },
});
```

**Python:**
```python
from document_atom_sdk import DocumentAtomSdk, ApiProcessorSettingsModel, ChunkingConfigurationModel

sdk = DocumentAtomSdk(endpoint="http://localhost:8000")

with open("document.pdf", "rb") as f:
    data = f.read()

# Without settings
atoms = sdk.atom.extract_atoms_pdf(data)

# With settings: enable OCR and sentence-based chunking
settings = ApiProcessorSettingsModel(
    extract_atoms_from_images=True,
    chunking=ChunkingConfigurationModel(
        enable=True,
        strategy="SentenceBased",
        fixed_token_count=256,
        overlap_count=2,
        overlap_strategy="SentenceBoundaryAware",
    ),
)
atoms = sdk.atom.extract_atoms_pdf(data, settings=settings)
```

### v2 to v3 Migration Guide

**API calls**: Replace raw binary POST with JSON envelope:
```
# v2 (no longer supported)
POST /atom/pdf
Content-Type: application/octet-stream
Body: <raw bytes>

# v3
POST /atom/pdf
Content-Type: application/json
Body: { "Settings": null, "Data": "<base64>" }
```

**OCR extraction**: Replace `?ocr=true` query parameter:
```
# v2 (no longer supported)
POST /atom/pdf?ocr=true

# v3
POST /atom/pdf
Body: { "Settings": { "ExtractAtomsFromImages": true }, "Data": "<base64>" }
```

**C# SDK**: Replace `bool extractOcr` parameter with `ApiProcessorSettings?`:
```csharp
// v2
var atoms = await sdk.Atom.ProcessPdf(data, extractOcr: true);

// v3
var settings = new ApiProcessorSettings { ExtractAtomsFromImages = true };
var atoms = await sdk.Atom.ProcessPdf(data, settings);
```

**TypeScript SDK**: Replace individual parameters with settings object:
```typescript
// v2
const atoms = await sdk.extractAtom.pdf(fileBuffer);

// v3 (same for no settings, but now accepts optional settings)
const atoms = await sdk.extractAtom.pdf(fileBuffer, { ExtractAtomsFromImages: true });
```

**Python SDK**: Replace `ocr` parameter with `settings`:
```python
# v2
atoms = sdk.atom.extract_atoms_pdf(data, ocr=True)

# v3
settings = ApiProcessorSettingsModel(extract_atoms_from_images=True)
atoms = sdk.atom.extract_atoms_pdf(data, settings=settings)
```

## Previous Releases

### v2.0.0

- **Consolidated Packages**: Reduced from 17+ packages to 4 main packages
- **Monorepo Structure**: All SDKs (TypeScript, Python, C#) are now part of the main repository
- **Unified Versioning**: All packages share the same version number via `Directory.Build.props`
- **Simplified Dependencies**: Single Tesseract native DLL location in DocumentAtom.Documents
- **Migration Guide**: See [MIGRATION.md](MIGRATION.md) for upgrading from v1.x

### v1.2.x

- **Data Ingestion Module** (`DocumentAtom.DataIngestion`) for RAG/AI pipeline integration
  - Unified document reader with automatic type detection
  - Intelligent chunking with hierarchy preservation
  - Configurable options for RAG, summarization, and large context windows
  - Dependency injection support with `Microsoft.Extensions.DependencyInjection`
  - Preserves full metadata from atoms for rich filtering in vector databases

## New in v1.1.x

- Hierarchical atomization (see `BuildHierarchy` in settings) - heading-based for markdown/HTML/Word, page-based for PowerPoint
- Support for CSV, JSON, and XML documents
- MCP server (`DocumentAtom.McpServer`) for exposing DocumentAtom operations via Model Context Protocol to AI assistants
- Dependency updates and fixes

## Motivation

Parsing documents and extracting constituent parts is one part science and one part black magic.  If you find ways to improve processing and extraction in any way that is horizontally useful, I'd would love your feedback on ways to make this library more accurate, more useful, faster, and overall better.  My goal in building this library is to make it easier to analyze input data assets and make them more consumable by other systems including analytics and artificial intelligence.

## Bugs, Quality, Feedback, or Enhancement Requests

Please feel free to file issues, enhancement requests, or start discussions about use of the library, improvements, or fixes.  

## Types Supported

DocumentAtom supports the following input file types:
- CSV
- HTML
- JSON
- Markdown
- Microsoft Word (.docx)
- Microsoft Excel (.xlsx)
- Microsoft PowerPoint (.pptx)
- PNG images (**requires Tesseract on the host**)
- PDF
- Rich text (.rtf)
- Text
- XML

## Simple Example 

Refer to the various `Test` projects for working examples.

The following example shows processing a markdown (`.md`) file.

```csharp
using DocumentAtom.Core.Atoms;
using DocumentAtom.Text.Markdown;

MarkdownProcessorSettings settings = new MarkdownProcessorSettings();
MarkdownProcessor processor = new MarkdownProcessor(settings);
foreach (Atom atom in processor.Extract(filename))
    Console.WriteLine(atom.ToString());
```

## Atom Types

DocumentAtom parses input data assets into a variety of `Atom` objects.  Each `Atom` includes top-level metadata including:
- `ParentGUID` - globally-unique identifier of the parent atom, or, null
- `GUID` - globally-unique identifier
- `Type` - including `Text`, `Image`, `Binary`, `Table`, and `List`
- `PageNumber` - where available; some document types do not explicitly indicate page numbers, and page numbers are inferred when rendered
- `Position` - the ordinal position of the `Atom`, relative to others
- `Length` - the length of the `Atom`'s content
- `MD5Hash` - the MD5 hash of the `Atom` content
- `SHA1Hash` - the SHA1 hash of the `Atom` content
- `SHA256Hash` - the SHA256 hash of the `Atom` content
- `Quarks` - structural sub-atoms from the document (e.g., cells in a table row, items in a list)
- `Chunks` - content fragments produced by the chunking engine when chunking is enabled via `Settings`

The `AtomBase` class provides the aforementioned metadata, and several type-specific `Atom`s are returned from the various processors, including:
- `BinaryAtom` - includes a `Bytes` property
- `DocxAtom` - includes `Text`, `HeaderLevel`, `UnorderedList`, `OrderedList`, `Table`, and `Binary` properties
- `ImageAtom` - includes `BoundingBox`, `Text`, `UnorderedList`, `OrderedList`, `Table`, and `Binary` properties
- `MarkdownAtom` - includes `Formatting`, `Text`, `UnorderedList`, `OrderedList`, and `Table` properties
- `PdfAtom` - includes `BoundingBox`, `Text`, `UnorderedList`, `OrderedList`, `Table`, and `Binary` properties
- `PptxAtom` - includes `Title`, `Subtitle`, `Text`, `UnorderedList`, `OrderedList`, `Table`, and `Binary` properties
- `TableAtom` - includes `Rows`, `Columns`, `Irregular`, and `Table` properties
- `TextAtom` - includes `Text`
- `XlsxAtom` - includes `SheetName`, `CellIdentifier`, `Text`, `Table`, and `Binary` properties

`Table` objects inside of `Atom` objects are always presented as `SerializableDataTable` objects (see [SerializableDataTable](https://github.com/jchristn/serializabledatatable) for more information) to provide simple serialization and conversion to native `System.Data.DataTable` objects.

## Underlying Libraries

DocumentAtom is built on the shoulders of several libraries, without which, this work would not be possible.

- [CsvHelper](https://github.com/JoshClose/CsvHelper)
- [DocumentFormat.OpenXml](https://github.com/dotnet/Open-XML-SDK)
- [HTML Agility Pack](https://github.com/zzzprojects/html-agility-pack)
- [PdfPig](https://github.com/UglyToad/PdfPig)
- [RtfPipe](github.com/erdomke/RtfPipe)
- [SharpToken](https://github.com/dmitry-brazhenko/SharpToken) - cl100k_base tokenizer for token-aware chunking
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Tabula](https://github.com/BobLd/tabula-sharp)
- [Tesseract](https://github.com/charlesw/tesseract/)

Each of these libraries were integrated as NuGet packages, and no source was included or modified from these packages.

My libraries used within DocumentAtom:

- [SerializableDataTable](https://github.com/jchristn/serializabledatatable)
- [SerializationHelper](https://github.com/jchristn/serializationhelper)

## Data Ingestion for RAG/AI Pipelines

The `DocumentAtom.DataIngestion` package provides a high-level API for processing documents and producing chunks ready for embedding and vector storage. It's designed to integrate seamlessly with RAG (Retrieval-Augmented Generation) applications and AI pipelines.

### Basic Usage

```csharp
using DocumentAtom.DataIngestion;
using DocumentAtom.DataIngestion.Processors;

// Create processor with RAG-optimized settings
AtomDocumentProcessorOptions options = AtomDocumentProcessorOptions.ForRag();
using AtomDocumentProcessor processor = new AtomDocumentProcessor(options);

// Process a document and get chunks
await foreach (IngestionChunk chunk in processor.ProcessAsync("document.pdf"))
{
    Console.WriteLine($"Chunk {chunk.ChunkIndex}: {chunk.Content.Substring(0, 100)}...");

    // Access metadata for filtering
    if (chunk.Metadata.TryGetValue("atom:page_number", out object? page))
        Console.WriteLine($"  Page: {page}");
}
```

### Dependency Injection

```csharp
using DocumentAtom.DataIngestion.Extensions;

// In your service configuration
services.AddDocumentAtomIngestionForRag();

// Or with custom options
services.AddDocumentAtomIngestion(
    reader => {
        reader.EnableOcr = true;
        reader.BuildHierarchy = true;
    },
    chunker => {
        chunker.Chunking = new ChunkingConfiguration
        {
            Enable = true,
            Strategy = ChunkStrategyEnum.SentenceBased,
            FixedTokenCount = 500,
            OverlapCount = 2,
            OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware
        };
    });
```

### Key Features

- **Automatic Type Detection**: Automatically detects document type from content
- **Intelligent Chunking**: Preserves paragraph boundaries and header context
- **Hierarchy-Aware**: Maintains document structure in chunks for better retrieval
- **Metadata Preservation**: All atom metadata is preserved for rich filtering
- **Duplicate Removal**: Optional deduplication based on content hash
- **Multiple Presets**: Optimized configurations for RAG, summarization, and large context windows

### Processing Options

| Method | Strategy | Token Budget | Best For |
|--------|----------|-------------|----------|
| `AtomDocumentProcessorOptions.ForRag()` | SentenceBased | 256 | Vector database ingestion, semantic search |
| `AtomDocumentProcessorOptions.ForSummarization()` | ParagraphBased | 1024 | Document summarization, analysis |
| `AtomDocumentProcessorOptions.ForLargeContext()` | ParagraphBased | 2048 | Large context window models |

## RESTful API and Docker

Run the `DocumentAtom.Server` project to start a RESTful server listening on `localhost:8000`.  Modify the `documentatom.json` file to change the webserver, logging, or Tesseract settings.  Alternatively, you can pull `jchristn77/documentatom` from [Docker Hub](https://hub.docker.com/repository/docker/jchristn77/documentatom/general).  Refer to the `Docker` directory in the project for assets for running in Docker.

Refer to the Postman collection for examples exercising the APIs.

### Running Locally

```bash
cd src/DocumentAtom.Server
dotnet run
```

### Running with Docker

1. Pull the image from Docker Hub:
```bash
docker pull jchristn77/documentatom:v1.1.0
```

2. Create a `documentatom.json` configuration file (see `Docker/documentatom.json` for an example)

3. Run the container:
```bash
# Windows
docker run -p 8000:8000 -v .\documentatom.json:/app/documentatom.json -v .\logs\:/app/logs/ jchristn77/documentatom:v1.1.0

# Linux/macOS
docker run -p 8000:8000 -v ./documentatom.json:/app/documentatom.json -v ./logs/:/app/logs/ jchristn77/documentatom:v1.1.0
```

Alternatively, use the provided scripts in the `Docker` directory:
```bash
# Windows
Dockerrun.bat v1.1.0

# Linux/macOS
IMG_TAG=v1.1.0 ./Dockerrun.sh
```

## MCP Server and Docker

The `DocumentAtom.McpServer` project provides a [Model Context Protocol (MCP)](https://modelcontextprotocol.io/) server that exposes DocumentAtom operations to AI assistants and LLM-based tools. The MCP server acts as a front-end to the `DocumentAtom.Server` RESTful API, enabling AI agents to process documents via standardized MCP tool calls.

The MCP server supports three transport protocols:
- **HTTP**: JSON-RPC over HTTP at `/rpc` (default port 8200)
- **TCP**: Raw TCP socket connection (default port 8201)
- **WebSocket**: WebSocket connection at `/mcp` (default port 8202)

### Prerequisites

The MCP server requires a running `DocumentAtom.Server` instance. Configure the endpoint in `documentatom.json`:

```json
{
  "DocumentAtom": {
    "Endpoint": "http://localhost:8000",
    "AccessKey": null
  }
}
```

### Running Locally

```bash
cd src/DocumentAtom.McpServer
dotnet run
```

Command-line options:
- `--config=<file>` - Specify settings file path (default: `./documentatom.json`)
- `--showconfig` - Display configuration and exit
- `--help`, `-h` - Show help message

### Running with Docker

1. Pull the image from Docker Hub:
```bash
docker pull jchristn77/documentatom-mcp:v1.1.0
```

2. Create a `documentatom.json` configuration file with MCP server settings:

```json
{
  "Logging": {
    "LogDirectory": "./logs/",
    "LogFilename": "documentatom-mcp.log",
    "ConsoleLogging": true,
    "EnableColors": true,
    "MinimumSeverity": 0
  },
  "DocumentAtom": {
    "Endpoint": "http://host.docker.internal:8000",
    "AccessKey": null
  },
  "Http": {
    "Hostname": "0.0.0.0",
    "Port": 8200
  },
  "Tcp": {
    "Address": "0.0.0.0",
    "Port": 8201
  },
  "WebSocket": {
    "Hostname": "0.0.0.0",
    "Port": 8202
  },
  "Storage": {
    "BackupsDirectory": "./backups/",
    "TempDirectory": "./temp/"
  }
}
```

3. Run the container:
```bash
# Windows
docker run -p 8200:8200 -p 8201:8201 -p 8202:8202 ^
  -v .\documentatom.json:/app/documentatom.json ^
  -v .\logs\:/app/logs/ ^
  -v .\temp\:/app/temp/ ^
  -v .\backups\:/app/backups/ ^
  jchristn77/documentatom-mcp:v1.1.0

# Linux/macOS
docker run -p 8200:8200 -p 8201:8201 -p 8202:8202 \
  -v ./documentatom.json:/app/documentatom.json \
  -v ./logs/:/app/logs/ \
  -v ./temp/:/app/temp/ \
  -v ./backups/:/app/backups/ \
  jchristn77/documentatom-mcp:v1.1.0
```

Alternatively, use the provided scripts in `src/DocumentAtom.McpServer`:
```bash
# Windows
Dockerrun.bat v1.0.0

# Linux/macOS
IMG_TAG=v1.0.0 ./Dockerrun.sh
```

### Environment Variables

The MCP server supports the following environment variables to override configuration:

| Variable | Description |
|----------|-------------|
| `DOCUMENTATOM_ENDPOINT` | DocumentAtom server endpoint URL |
| `DOCUMENTATOM_ACCESS_KEY` | Access key for authentication |
| `MCP_HTTP_HOSTNAME` | HTTP server hostname |
| `MCP_HTTP_PORT` | HTTP server port |
| `MCP_TCP_ADDRESS` | TCP server address |
| `MCP_TCP_PORT` | TCP server port |
| `MCP_WEBSOCKET_HOSTNAME` | WebSocket server hostname |
| `MCP_WEBSOCKET_PORT` | WebSocket server port |
| `CONSOLE_LOGGING` | Enable console logging (1 or 0) |

### Building Docker Images

To build the Docker images locally:

```bash
# Build DocumentAtom.Server image
cd Docker
Dockerbuild.bat v1.1.0 0  # 0 = don't push, 1 = push to Docker Hub

# Build DocumentAtom.McpServer image (from src directory)
cd src
docker buildx build -f DocumentAtom.McpServer/Dockerfile --platform linux/amd64,linux/arm64/v8 --tag jchristn77/documentatom-mcp:v1.1.0 --push .
```

## Version History

Please refer to ```CHANGELOG.md``` for version history.

## Thanks

Special thanks to iconduck.com and the content authors for producing this [icon](https://iconduck.com/icons/27054/atom).
