<img src="https://raw.githubusercontent.com/jchristn/DocumentAtom/refs/heads/main/assets/icon.png" width="256" height="256">

# DocumentAtom.Sdk

[![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Sdk.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Sdk/) [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Sdk.svg)](https://www.nuget.org/packages/DocumentAtom.Sdk)

DocumentAtom.Sdk is a C# SDK that provides a simple and efficient way to interact with DocumentAtom server instances. It enables document atomization, type detection, and health monitoring through a clean, async API.

**IMPORTANT** - DocumentAtom.Sdk assumes you have deployed the DocumentAtom REST server. If you are integrating a DocumentAtom library directly into your code, use of this SDK is not necessary.

## Overview

DocumentAtom.Sdk allows you to:
- **Process Documents**: Extract atoms from various document formats (PDF, Word, Excel, PowerPoint, HTML, Markdown, etc.)
- **Detect Document Types**: Automatically identify document types based on content analysis
- **Monitor Health**: Check server status and health
- **Async Operations**: Full async/await support for all operations

## Installation

Install the package via NuGet:

```bash
Install-Package DocumentAtom.Sdk
```

Or via .NET CLI:

```bash
dotnet add package DocumentAtom.Sdk
```

## Quick Start

### Basic Usage

```csharp
using DocumentAtom.Sdk;
using DocumentAtom.Core.Atoms;

// Initialize the SDK
var sdk = new DocumentAtomSdk("http://localhost:8000");

// Check server health
bool isHealthy = await sdk.Health.IsHealthy();
Console.WriteLine($"Server is healthy: {isHealthy}");

// Process a document
byte[] documentData = await File.ReadAllBytesAsync("document.pdf");
List<Atom>? atoms = await sdk.Atom.ProcessPdf(documentData);

if (atoms != null)
{
    foreach (var atom in atoms)
    {
        Console.WriteLine($"Atom Type: {atom.Type}, Content: {atom.Text}");
    }
}
```


### With Logging

```csharp
var sdk = new DocumentAtomSdk("http://localhost:8000");
sdk.LogRequests = true;
sdk.LogResponses = true;
sdk.Logger = (severity, message) => Console.WriteLine($"[{severity}] {message}");
```

## API Reference

### DocumentAtomSdk Class

The main SDK class that provides access to all functionality.

#### Constructor

```csharp
public DocumentAtomSdk(string endpoint)
```

- `endpoint`: DocumentAtom server endpoint URL

#### Properties

- `Endpoint`: Server endpoint URL
- `TimeoutMs`: Request timeout in milliseconds (default: 300000)
- `LogRequests`: Enable request logging
- `LogResponses`: Enable response logging
- `Logger`: Custom logger delegate

#### Main API Groups

- `Atom`: Document processing methods
- `TypeDetection`: Document type detection
- `Health`: Server health and status

### Document Processing (Atom Methods)

Process various document types and extract atoms:

#### Supported Document Types

- **CSV**: `ProcessCsv(byte[] data, bool extractOcr = false)`
- **Excel**: `ProcessExcel(byte[] data, bool extractOcr = false)`
- **HTML**: `ProcessHtml(byte[] data)`
- **JSON**: `ProcessJson(byte[] data)`
- **Markdown**: `ProcessMarkdown(byte[] data)`
- **OCR**: `ProcessOcr(byte[] data)`
- **PDF**: `ProcessPdf(byte[] data, bool extractOcr = false)`
- **PNG**: `ProcessPng(byte[] data)`
- **PowerPoint**: `ProcessPowerPoint(byte[] data, bool extractOcr = false)`
- **RTF**: `ProcessRtf(byte[] data, bool extractOcr = false)`
- **Text**: `ProcessText(byte[] data)`
- **Word**: `ProcessWord(byte[] data, bool extractOcr = false)`
- **XML**: `ProcessXml(byte[] data)`

#### Example: Processing Multiple Document Types

```csharp
// Process a PDF document
byte[] pdfData = await File.ReadAllBytesAsync("document.pdf");
List<Atom>? pdfAtoms = await sdk.Atom.ProcessPdf(pdfData, extractOcr: true);

// Process a Word document
byte[] wordData = await File.ReadAllBytesAsync("document.docx");
List<Atom>? wordAtoms = await sdk.Atom.ProcessWord(wordData, extractOcr: true);

// Process an Excel spreadsheet
byte[] excelData = await File.ReadAllBytesAsync("spreadsheet.xlsx");
List<Atom>? excelAtoms = await sdk.Atom.ProcessExcel(excelData);
```

### Type Detection

Automatically detect document types:

```csharp
// Detect document type
byte[] documentData = await File.ReadAllBytesAsync("unknown-document");
TypeResult? result = await sdk.TypeDetection.DetectType(documentData);

if (result != null)
{
    Console.WriteLine($"MIME Type: {result.MimeType}");
    Console.WriteLine($"Extension: {result.Extension}");
    Console.WriteLine($"Document Type: {result.Type}");
}

// With content type hint
TypeResult? resultWithHint = await sdk.TypeDetection.DetectType(
    documentData, 
    contentType: "application/pdf"
);
```

### Health Monitoring

Check server health and status:

```csharp
// Check if server is healthy
bool isHealthy = await sdk.Health.IsHealthy();

// Get server status
string? status = await sdk.Health.GetStatus();
Console.WriteLine($"Server status: {status}");
```

## Atom Types

The SDK returns `Atom` objects that contain structured document content:

### Atom Properties

- `GUID`: Unique identifier
- `ParentGUID`: Parent atom identifier (for hierarchical structures)
- `Type`: Atom type (Text, Image, Binary, Table, List)
- `PageNumber`: Page number where available
- `Position`: Ordinal position
- `Length`: Content length
- `Text`: Text content
- `MD5Hash`, `SHA1Hash`, `SHA256Hash`: Content hashes
- `Quarks`: Sub-atomic particles (chunks)

### Specialized Atom Types

- `TableAtom`: Contains table data with rows and columns
- `ImageAtom`: Contains image data and OCR text
- `BinaryAtom`: Contains binary data
- `ListAtom`: Contains ordered or unordered lists

## Error Handling

The SDK handles errors gracefully and returns null for failed operations:

```csharp
try
{
    List<Atom>? atoms = await sdk.Atom.ProcessPdf(documentData);
    
    if (atoms == null)
    {
        Console.WriteLine("Failed to process document or no atoms extracted");
    }
    else
    {
        Console.WriteLine($"Successfully extracted {atoms.Count} atoms");
    }
}
catch (Exception ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Configuration

### Timeout Configuration

```csharp
var sdk = new DocumentAtomSdk("http://localhost:8000");
sdk.TimeoutMs = 600000; // 10 minutes
```

### Logging Configuration

```csharp
var sdk = new DocumentAtomSdk("http://localhost:8000");
sdk.LogRequests = true;
sdk.LogResponses = true;
sdk.Logger = (severity, message) => 
{
    // Custom logging implementation
    File.AppendAllText("sdk.log", $"[{DateTime.UtcNow}] [{severity}] {message}\n");
};
```

## Server Requirements

The SDK requires a running DocumentAtom server instance. You can:

1. **Run locally**: Start the DocumentAtom.Server project
2. **Use Docker**: Pull and run the `jchristn/documentatom` Docker image
3. **Deploy**: Deploy to your preferred hosting environment

### Server Setup

```bash
# Using Docker
docker run -p 8000:8000 jchristn/documentatom

# Or run the DocumentAtom.Server project locally
# Default endpoint: http://localhost:8000
```

## Examples

### Complete Example: Document Processing Pipeline

```csharp
using DocumentAtom.Sdk;
using DocumentAtom.Core.Atoms;

public class DocumentProcessor
{
    private readonly DocumentAtomSdk _sdk;
    
    public DocumentProcessor(string endpoint)
    {
        _sdk = new DocumentAtomSdk(endpoint);
        _sdk.Logger = (severity, message) => Console.WriteLine($"[{severity}] {message}");
    }
    
    public async Task ProcessDocumentAsync(string filePath)
    {
        try
        {
            // Check server health
            if (!await _sdk.Health.IsHealthy())
            {
                throw new Exception("DocumentAtom server is not healthy");
            }
            
            // Read document
            byte[] documentData = await File.ReadAllBytesAsync(filePath);
            
            // Detect document type
            TypeResult? typeResult = await _sdk.TypeDetection.DetectType(documentData);
            Console.WriteLine($"Detected type: {typeResult?.Type}");
            
            // Process based on type
            List<Atom>? atoms = typeResult?.Type switch
            {
                DocumentTypeEnum.Pdf => await _sdk.Atom.ProcessPdf(documentData, extractOcr: true),
                DocumentTypeEnum.Word => await _sdk.Atom.ProcessWord(documentData, extractOcr: true),
                DocumentTypeEnum.Excel => await _sdk.Atom.ProcessExcel(documentData),
                DocumentTypeEnum.PowerPoint => await _sdk.Atom.ProcessPowerPoint(documentData, extractOcr: true),
                DocumentTypeEnum.Html => await _sdk.Atom.ProcessHtml(documentData),
                DocumentTypeEnum.Markdown => await _sdk.Atom.ProcessMarkdown(documentData),
                DocumentTypeEnum.Json => await _sdk.Atom.ProcessJson(documentData),
                DocumentTypeEnum.Xml => await _sdk.Atom.ProcessXml(documentData),
                DocumentTypeEnum.Csv => await _sdk.Atom.ProcessCsv(documentData),
                DocumentTypeEnum.Text => await _sdk.Atom.ProcessText(documentData),
                DocumentTypeEnum.Rtf => await _sdk.Atom.ProcessRtf(documentData, extractOcr: true),
                DocumentTypeEnum.Png => await _sdk.Atom.ProcessPng(documentData),
                _ => throw new NotSupportedException($"Document type {typeResult?.Type} is not supported")
            };
            
            if (atoms != null)
            {
                Console.WriteLine($"Extracted {atoms.Count} atoms from {filePath}");
                
                // Process atoms
                foreach (var atom in atoms)
                {
                    ProcessAtom(atom);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error processing {filePath}: {ex.Message}");
        }
    }
    
    private void ProcessAtom(Atom atom)
    {
        Console.WriteLine($"Atom: {atom.Type} - {atom.Text?.Substring(0, Math.Min(100, atom.Text.Length ?? 0))}...");
        
        // Process based on atom type
        switch (atom.Type)
        {
            case AtomTypeEnum.Table:
                Console.WriteLine($"  Table: {atom.Rows} rows, {atom.Columns} columns");
                break;
            case AtomTypeEnum.Image:
                Console.WriteLine($"  Image: {atom.Binary?.Length ?? 0} bytes");
                break;
            case AtomTypeEnum.Text:
                Console.WriteLine($"  Text: {atom.Text?.Length ?? 0} characters");
                break;
        }
    }
    
    public void Dispose()
    {
        _sdk?.Dispose();
    }
}
```

## Dependencies

- **DocumentAtom (1.1.0)**: Core document processing library
- **DocumentAtom.TypeDetection (1.0.37)**: Document type detection
- **RestWrapper (3.1.6)**: HTTP client wrapper
- **System.Text.Json (8.0.5)**: JSON serialization

## Version History

Please refer to [CHANGELOG.md](CHANGELOG.md) for version history.

## Contributing

Contributions are welcome! Please feel free to submit issues, enhancement requests, or pull requests.

## License

This project is licensed under the MIT License - see the LICENSE file for details.

## Support

For support, please:
1. Check the [DocumentAtom documentation](https://github.com/jchristn/DocumentAtom)
2. Review the test projects in the solution
3. File an issue in the repository
