# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

DocumentAtom is a .NET 8 library for breaking input documents into constituent parts ("atoms") for AI, machine learning, processing, analytics, and analysis. The library supports multiple document formats including Word, Excel, PowerPoint, PDF, images, text, markdown, and HTML.

## Build Commands

### Building the Solution
```bash
dotnet build DocumentAtom.sln
```

### Building Individual Projects
```bash
dotnet build DocumentAtom.Core/DocumentAtom.Core.csproj
dotnet build DocumentAtom.Server/DocumentAtom.Server.csproj
```

### Running Tests
```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test Test.TextProcessor/Test.TextProcessor.csproj
dotnet test Test.MarkdownProcessor/Test.MarkdownProcessor.csproj
# (etc. for other Test.* projects)
```

### Running the Server
```bash
cd DocumentAtom.Server
dotnet run
```

### Docker Commands
```bash
# Build Docker image (from DocumentAtom.Server directory)
docker build -t documentatom .

# Using the provided build script
DocumentAtom.Server/Dockerbuild.bat v1.0.0 0
```

## Architecture

### Core Structure
- **DocumentAtom.Core**: Base classes and interfaces (`ProcessorBase`, `ProcessorSettingsBase`, `ChunkingSettings`)
- **DocumentAtom.Server**: HTTP server implementation using Watson WebServer
- **DocumentAtom.TypeDetection**: File type detection including iWork files (Pages, Numbers, Keynote)

### Document Processors
Each document type has its own processor library:
- **DocumentAtom.Text**: Plain text processing
- **DocumentAtom.Markdown**: Markdown document processing
- **DocumentAtom.Word**: DOCX file processing
- **DocumentAtom.Excel**: XLSX file processing
- **DocumentAtom.PowerPoint**: PPTX file processing
- **DocumentAtom.Pdf**: PDF document processing
- **DocumentAtom.Image**: Image file processing
- **DocumentAtom.Html**: HTML document processing
- **DocumentAtom.Ocr**: OCR processing capabilities
- **DocumentAtom.TextTools**: Text processing utilities

### Test Projects
Each processor has a corresponding test project (Test.*) for validation.

### Key Dependencies
- .NET 8.0 target framework
- SerializableDataTable for data handling
- SerializationHelper for JSON/XML operations
- Watson WebServer for HTTP server functionality
- SyslogLogging for logging capabilities
- Tesseract libraries for OCR functionality

### Processor Pattern
All document processors inherit from `ProcessorBase` and follow a consistent pattern:
- Settings configuration via `ProcessorSettingsBase`
- Logging support via configurable logger
- Serialization support for atoms/chunks
- Dispose pattern implementation

### Document Type Detection
The TypeDetection module includes sophisticated logic for identifying file types, particularly Apple iWork files (Pages, Numbers, Keynote) which are ZIP archives with specific internal structures.

## NuGet Packaging
The Core project is configured for NuGet package generation with version 1.0.35. Package builds include documentation files, icons, and license information.

## CRITICAL CODING STANDARDS

All code files in this repository MUST strictly follow these rules for consistency and maintainability:

### Code Organization
- Namespace declaration at the top, using statements INSIDE the namespace block
- Microsoft/system usings first (alphabetical), then other usings (alphabetical)
- One class or one enum per file - no nesting multiple types in a single file

### Documentation
- All public members, constructors, and public methods MUST have XML documentation
- No documentation on private members or private methods
- Document default values, minimum/maximum values, and their effects
- Document exceptions using `/// <exception>` tags
- Document thread safety guarantees
- Document nullability in XML comments

### Variable and Property Conventions
- No `var` keyword - use explicit types
- Private class members start with underscore then PascalCase (`_FooBar`, not `_fooBar`)
- Public members with explicit getters/setters using backing variables for validation
- Avoid constants - use configurable public members with reasonable defaults

### Async/Threading
- Async methods MUST accept `CancellationToken` parameter (unless class has token member)
- Use `.ConfigureAwait(false)` where appropriate
- Check cancellation at appropriate places in async methods
- Both sync and async variants for methods returning `IEnumerable`

### Exception Handling
- Use specific exception types, not generic `Exception`
- Include meaningful error messages with context
- Consider custom exception types for domain-specific errors
- Use exception filters: `catch (SqlException ex) when (ex.Number == 2601)`

### Resource Management
- Implement `IDisposable`/`IAsyncDisposable` for unmanaged resources
- Use `using` statements/declarations for disposable objects
- Follow full Dispose pattern with `protected virtual void Dispose(bool disposing)`
- Always call `base.Dispose()` in derived classes

### Nullable Reference Types
- Enable `<Nullable>enable</Nullable>` in project files
- Validate input with guard clauses at method start
- Use `ArgumentNullException.ThrowIfNull()` (.NET 6+) or manual null checks
- Proactively eliminate null-caused exceptions

### LINQ and Collections
- Prefer LINQ when readability isn't compromised
- Use `.Any()` instead of `.Count() > 0` for existence checks
- Use `.FirstOrDefault()` with null checks, not `.First()`
- Be aware of multiple enumeration - use `.ToList()` when needed

### Concurrency
- Use `Interlocked` operations for simple atomic operations
- Prefer `ReaderWriterLockSlim` over `lock` for read-heavy scenarios

### Absolute Prohibitions
- **NO TUPLES** unless absolutely necessary
- Do not assume class members/methods exist on opaque classes - ask for implementation
- Do not modify manually prepared SQL strings without discussion