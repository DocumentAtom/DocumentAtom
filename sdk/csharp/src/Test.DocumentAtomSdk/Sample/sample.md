# DocumentAtom SDK Test Document

This is a sample Markdown document for testing the DocumentAtom SDK capabilities.

## Introduction

The **DocumentAtom SDK** is a comprehensive C# library designed for document processing and atomization. It provides a unified interface for processing various document formats and extracting meaningful content chunks called "atoms".

## Key Features

- ✅ **Multi-format Support**: PDF, Word, Excel, PowerPoint, HTML, JSON, XML, Markdown, RTF, CSV, and plain text
- ✅ **OCR Integration**: Extract text from images within documents
- ✅ **Batch Processing**: Process multiple documents efficiently
- ✅ **Type Safety**: Strongly-typed C# SDK with full IntelliSense support
- ✅ **Async Operations**: Full async/await support for better performance
- ✅ **Error Handling**: Comprehensive error handling and logging

## Supported Document Formats

| Format | Extension | OCR Support | Description |
|--------|-----------|-------------|-------------|
| PDF | `.pdf` | ✅ Yes | Portable Document Format |
| Microsoft Word | `.docx` | ✅ Yes | Word documents |
| Microsoft Excel | `.xlsx` | ✅ Yes | Excel spreadsheets |
| Microsoft PowerPoint | `.pptx` | ✅ Yes | PowerPoint presentations |
| HTML | `.html`, `.htm` | ❌ No | Web pages |
| JSON | `.json` | ❌ No | JavaScript Object Notation |
| XML | `.xml` | ❌ No | eXtensible Markup Language |
| Markdown | `.md` | ❌ No | Markdown documents |
| RTF | `.rtf` | ✅ Yes | Rich Text Format |
| CSV | `.csv` | ❌ No | Comma-Separated Values |
| Plain Text | `.txt` | ❌ No | Plain text files |

## Usage Examples

### Basic Usage

```csharp
// Initialize the SDK
var sdk = new DocumentAtomSdk("http://localhost:8080");

// Process a PDF document
byte[] pdfData = File.ReadAllBytes("document.pdf");
var atoms = await sdk.Atom.ProcessPdf(pdfData, extractOcr: true);

// Display results
foreach (var atom in atoms)
{
    Console.WriteLine($"Type: {atom.Type}, Content: {atom.Content}");
}
```

### Advanced Configuration

```csharp
// Initialize with custom settings
var sdk = new DocumentAtomSdk("http://localhost:8080", "your-access-key")
{
    TimeoutMs = 600000, // 10 minutes
    LogRequests = true,
    LogResponses = true,
    Logger = (severity, message) => Console.WriteLine($"[{severity}] {message}")
};

// Process multiple document types
var tasks = new List<Task<List<Atom>?>>();

tasks.Add(sdk.Atom.ProcessPdf(pdfData, extractOcr: true));
tasks.Add(sdk.Atom.ProcessWord(wordData, extractOcr: false));
tasks.Add(sdk.Atom.ProcessExcel(excelData, extractOcr: true));

var results = await Task.WhenAll(tasks);
```

### Health Checks

```csharp
// Check server health
bool isHealthy = await sdk.Health.IsHealthy();
if (isHealthy)
{
    Console.WriteLine("Server is healthy and ready to process documents.");
}

// Get server status
string? status = await sdk.Health.GetStatus();
Console.WriteLine($"Server status: {status}");
```

### Type Detection

```csharp
// Detect document type
byte[] documentData = File.ReadAllBytes("unknown-document");
var typeResult = await sdk.TypeDetection.DetectType(documentData);

if (typeResult != null)
{
    Console.WriteLine($"Detected type: {typeResult.MimeType}");
    Console.WriteLine($"File extension: {typeResult.Extension}");
    Console.WriteLine($"Confidence: {typeResult.Confidence}");
}
```

## Configuration Options

The SDK supports various configuration options:

- **Endpoint URL**: Server address for DocumentAtom API
- **Access Key**: Optional authentication token
- **Timeout**: Request timeout in milliseconds (default: 300000)
- **Logging**: Enable/disable request and response logging
- **Logger**: Custom logging callback function

## Error Handling

The SDK provides comprehensive error handling:

```csharp
try
{
    var atoms = await sdk.Atom.ProcessPdf(pdfData);
    // Process atoms...
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"Network error: {ex.Message}");
}
catch (TaskCanceledException ex)
{
    Console.WriteLine($"Request timeout: {ex.Message}");
}
catch (Exception ex)
{
    Console.WriteLine($"Unexpected error: {ex.Message}");
}
```

## Best Practices

1. **Use Async/Await**: Always use async methods for better performance
2. **Handle Errors**: Implement proper error handling for network and processing errors
3. **Configure Timeouts**: Set appropriate timeouts based on your document sizes
4. **Enable Logging**: Use logging during development and debugging
5. **Batch Processing**: Use batch operations for processing multiple documents
6. **Resource Management**: Properly dispose of the SDK instance when done

## Conclusion

The DocumentAtom SDK provides a powerful and flexible solution for document processing in C# applications. With support for multiple formats, OCR capabilities, and a clean, type-safe API, it simplifies the integration of document atomization into your projects.

For more information, visit the [DocumentAtom GitHub repository](https://github.com/documentatom/documentatom).

---

*This document was created for testing purposes and demonstrates the Markdown processing capabilities of the DocumentAtom SDK.*
