# Changelog

## Current Version

v1.0.0

- Initial release of DocumentAtom SDK
- C# SDK for interacting with DocumentAtom server instances
- Document processing capabilities
  - Support for multiple document formats (PDF, Word, Excel, PowerPoint, HTML, Markdown, JSON, XML, CSV, RTF, Text, PNG)
  - OCR text extraction from images
  - Hierarchical document atomization
- Document type detection
  - Automatic document type identification
  - Content-based type detection
  - MIME type and extension detection
- Health monitoring
  - Server health checks
  - Status monitoring
- HTTP client functionality
  - GET, POST request support
  - Binary data handling
  - Chunked transfer encoding support
  - Configurable timeouts and logging
- Authentication support
  - Optional access key authentication
  - Bearer token authorization
- JSON serialization
  - Built-in JSON serialization with customizable options
  - Support for enum string conversion
  - Case-insensitive property matching
- Logging and debugging
  - Request/response logging
  - Custom logger support with severity levels
  - Debug mode for detailed output
- Dependencies
  - DocumentAtom (1.1.0) - Core document processing
  - DocumentAtom.TypeDetection (1.0.37) - Type detection
  - RestWrapper (3.1.6) - HTTP client wrapper
  - System.Text.Json (8.0.5) - JSON serialization
- Platform support
  - .NET 8.0 minimum requirement
  - Cross-platform compatibility

## Previous Versions

Notes from previous versions will be placed here.
