<img src="https://raw.githubusercontent.com/jchristn/DocumentAtom/refs/heads/main/assets/icon.png" width="256" height="256">

# DocumentAtom

DocumentAtom provides a light, fast library for breaking input documents into constituent parts (atoms), useful for text processing, analysis, and artificial intelligence.

DocumentAtom requires that Tesseract v5.0 be installed on the host.  This is required as certain document types can have embedded images which are parsed using OCR via Tesseract.

| Package | Version | Downloads |
|---------|---------|-----------|
| DocumentAtom.Csv | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Csv.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Csv/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Csv.svg)](https://www.nuget.org/packages/DocumentAtom.Csv)  |
| DocumentAtom.DataIngestion | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.DataIngestion.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.DataIngestion/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.DataIngestion.svg)](https://www.nuget.org/packages/DocumentAtom.DataIngestion)  |
| DocumentAtom.Excel | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Excel.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Excel/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Excel.svg)](https://www.nuget.org/packages/DocumentAtom.Excel)  |
| DocumentAtom.Html | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Html.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Html/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Html.svg)](https://www.nuget.org/packages/DocumentAtom.Html)  |
| DocumentAtom.Image | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Image.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Image/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Image.svg)](https://www.nuget.org/packages/DocumentAtom.Image)  |
| DocumentAtom.Json | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Json.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Json/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Json.svg)](https://www.nuget.org/packages/DocumentAtom.Json)  |
| DocumentAtom.Markdown | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Markdown.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Markdown/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Markdown.svg)](https://www.nuget.org/packages/DocumentAtom.Markdown)  |
| DocumentAtom.Pdf | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Pdf.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Pdf/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Pdf.svg)](https://www.nuget.org/packages/DocumentAtom.Pdf)  |
| DocumentAtom.PowerPoint | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.PowerPoint.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.PowerPoint/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.PowerPoint.svg)](https://www.nuget.org/packages/DocumentAtom.PowerPoint)  |
| DocumentAtom.Ocr | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Ocr.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Ocr/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Ocr.svg)](https://www.nuget.org/packages/DocumentAtom.Ocr)  |
| DocumentAtom.RichText | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.RichText.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.RichText/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.RichText.svg)](https://www.nuget.org/packages/DocumentAtom.RichText)  |
| DocumentAtom.Text | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Text.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Text/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Text.svg)](https://www.nuget.org/packages/DocumentAtom.Text)  |
| DocumentAtom.TypeDetection | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.TypeDetection.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.TypeDetection/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.TypeDetection.svg)](https://www.nuget.org/packages/DocumentAtom.TypeDetection)  |
| DocumentAtom.Word | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Word.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Word/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Word.svg)](https://www.nuget.org/packages/DocumentAtom.Word)  |
| DocumentAtom.Xml | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Xml.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Xml/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Xml.svg)](https://www.nuget.org/packages/DocumentAtom.Xml)  |

## New in v1.2.x

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
using DocumentAtom.Markdown;

MarkdownProcessorSettings settings = new MarkdownProcessorSettings();
MarkdownProcessor processor = new MarkdownProcessor(_Settings);
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
- `Quarks` - sub-atomic particles created from the `Atom` content, for instance, when chunking text

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
        chunker.MaxChunkSize = 500;
        chunker.ChunkOverlap = 50;
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

| Method | Best For |
|--------|----------|
| `AtomDocumentProcessorOptions.ForRag()` | Vector database ingestion, semantic search |
| `AtomDocumentProcessorOptions.ForSummarization()` | Document summarization, analysis |
| `AtomChunkerOptions.ForLargeContext()` | Large context window models |

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
