# Microsoft.Extensions.DataIngestion Integration Plan

## Executive Summary

This document outlines a comprehensive plan for integrating DocumentAtom with Microsoft.Extensions.DataIngestion, enabling DocumentAtom's document parsing capabilities to participate in standardized .NET AI/RAG pipelines.

**Target Package**: `DocumentAtom.DataIngestion`
**Target Framework**: .NET 8.0+ (matching Microsoft.Extensions.DataIngestion requirements)
**Dependency**: `Microsoft.Extensions.DataIngestion` (prerelease, monitor for GA)

---

## Table of Contents

1. [Goals and Non-Goals](#1-goals-and-non-goals)
2. [Architecture Overview](#2-architecture-overview)
3. [Package Structure](#3-package-structure)
4. [Type Mappings](#4-type-mappings)
5. [Implementation Details](#5-implementation-details)
   - 5.1 [Reader Implementation](#51-reader-implementation)
   - 5.2 [Chunker Implementation](#52-chunker-implementation)
   - 5.3 [Document Processor Implementation](#53-document-processor-implementation)
   - 5.4 [Extension Methods and Factories](#54-extension-methods-and-factories)
6. [Configuration and Options](#6-configuration-and-options)
7. [Metadata Preservation Strategy](#7-metadata-preservation-strategy)
8. [Test Plan](#8-test-plan)
9. [Documentation Requirements](#9-documentation-requirements)
10. [Migration Guide](#10-migration-guide)
11. [Implementation Phases](#11-implementation-phases)
12. [Risk Assessment](#12-risk-assessment)
13. [Future Considerations](#13-future-considerations)

---

## 1. Goals and Non-Goals

### Goals

| ID | Goal | Rationale |
|----|------|-----------|
| G1 | Expose DocumentAtom parsers as `IngestionDocumentReader` implementations | Enable DocumentAtom to read documents into the DataIngestion pipeline |
| G2 | Provide an `IngestionChunker<T>` that leverages Atom/Quark boundaries | Atom boundaries provide higher-quality semantic chunking than token-based strategies |
| G3 | Preserve full Atom metadata through the pipeline | Users should not lose DocumentAtom's rich metadata (hashes, bounding boxes, hierarchy) |
| G4 | Support all existing DocumentAtom formats | PDF, Word, Excel, PowerPoint, HTML, Markdown, JSON, CSV, XML, RTF, Images, Text |
| G5 | Enable seamless integration with VectorStoreWriter and AI enrichers | Primary use case is RAG pipelines |
| G6 | Maintain backward compatibility | Existing DocumentAtom users should not be affected |
| G7 | Provide convenience factories for common pipeline configurations | Reduce boilerplate for typical use cases |
| G8 | Comprehensive test coverage | All adapters must be thoroughly tested |

### Non-Goals

| ID | Non-Goal | Rationale |
|----|----------|-----------|
| NG1 | Replace DocumentAtom's core Atom model | The Atom model is DocumentAtom's differentiator and should be preserved |
| NG2 | Rewrite existing processors | Adapters wrap existing functionality |
| NG3 | Make Microsoft.Extensions.DataIngestion a required dependency | Integration should be opt-in via separate package |
| NG4 | Support DataIngestion features that don't map to DocumentAtom | Focus on document reading and chunking, not unrelated features |
| NG5 | Implement custom vector store writers | Use existing VectorStoreWriter from DataIngestion |

---

## 2. Architecture Overview

### High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        User Application                                      │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                 Microsoft.Extensions.DataIngestion                           │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────┐ │
│  │IngestionPipeline<T>         │  │ Enrichers   │  │ VectorStoreWriter   │ │
│  └─────────────┘  └─────────────┘  └─────────────┘  └─────────────────────┘ │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                    ┌───────────────┼───────────────┐
                    ▼               ▼               ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                    DocumentAtom.DataIngestion (NEW)                          │
│  ┌───────────────────┐  ┌───────────────────┐  ┌──────────────────────────┐ │
│  │ AtomDocumentReader │  │ AtomChunker<T>    │  │ AtomDocumentProcessor    │ │
│  │ (IngestionDoc-     │  │ (IngestionChunker │  │ (IngestionDocument-      │ │
│  │  umentReader)      │  │  <T>)             │  │  Processor)              │ │
│  └───────────────────┘  └───────────────────┘  └──────────────────────────┘ │
│              │                    │                         │                │
│              ▼                    ▼                         ▼                │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                         Atom Converters                                │  │
│  │  AtomToIngestionElement | IngestionChunkFromAtom | MetadataMapper     │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DocumentAtom.Core + Processors                       │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │PdfProcessor │ │DocxProcessor│ │HtmlProcessor│ │    ...      │           │
│  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘           │
│                              │                                               │
│                              ▼                                               │
│                    ┌─────────────────┐                                       │
│                    │      Atom       │                                       │
│                    │  (Core Model)   │                                       │
│                    └─────────────────┘                                       │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Data Flow

```
Input Document (PDF, DOCX, etc.)
         │
         ▼
┌─────────────────────────┐
│  AtomDocumentReader     │
│  ┌───────────────────┐  │
│  │ TypeDetector      │──┼──► Detect document type
│  └───────────────────┘  │
│  ┌───────────────────┐  │
│  │ ProcessorFactory  │──┼──► Create appropriate processor
│  └───────────────────┘  │
│  ┌───────────────────┐  │
│  │ Processor.Extract │──┼──► Extract Atoms
│  └───────────────────┘  │
│  ┌───────────────────┐  │
│  │ AtomConverter     │──┼──► Convert to IngestionDocument
│  └───────────────────┘  │
└─────────────────────────┘
         │
         ▼
IngestionDocument (with Atom metadata preserved)
         │
         ▼
┌─────────────────────────┐
│ AtomDocumentProcessor   │ (optional)
│ - Image OCR enrichment  │
│ - Hierarchy building    │
└─────────────────────────┘
         │
         ▼
┌─────────────────────────┐
│  AtomChunker<T>         │
│  ┌───────────────────┐  │
│  │ Quark extraction  │──┼──► Use pre-computed quarks if available
│  └───────────────────┘  │
│  ┌───────────────────┐  │
│  │ Fallback chunking │──┼──► Apply ChunkingSettings if no quarks
│  └───────────────────┘  │
└─────────────────────────┘
         │
         ▼
IEnumerable<IngestionChunk<T>> (with full Atom metadata)
         │
         ▼
┌─────────────────────────┐
│ DataIngestion Enrichers │ (SummaryEnricher, KeywordEnricher, etc.)
└─────────────────────────┘
         │
         ▼
┌─────────────────────────┐
│ VectorStoreWriter<T>    │
└─────────────────────────┘
         │
         ▼
Vector Database (Qdrant, CosmosDB, SQL Server, etc.)
```

---

## 3. Package Structure

### Project Layout

```
src/
└── DocumentAtom.DataIngestion/
    ├── DocumentAtom.DataIngestion.csproj
    │
    ├── Readers/
    │   ├── AtomDocumentReader.cs
    │   ├── AtomDocumentReaderOptions.cs
    │   └── ProcessorFactory.cs
    │
    ├── Chunkers/
    │   ├── AtomChunker.cs
    │   ├── AtomChunkerOptions.cs
    │   ├── QuarkChunkingStrategy.cs
    │   └── HierarchyAwareChunker.cs
    │
    ├── Processors/
    │   ├── AtomDocumentProcessor.cs
    │   └── HierarchyBuildingProcessor.cs
    │
    ├── Converters/
    │   ├── AtomToIngestionElementConverter.cs
    │   ├── IngestionElementToAtomConverter.cs
    │   ├── AtomToIngestionChunkConverter.cs
    │   └── MetadataMapper.cs
    │
    ├── Extensions/
    │   ├── AtomExtensions.cs
    │   ├── IngestionDocumentExtensions.cs
    │   ├── IngestionChunkExtensions.cs
    │   └── ServiceCollectionExtensions.cs
    │
    ├── Factories/
    │   ├── DocumentAtomIngestionFactory.cs
    │   └── PipelineBuilderExtensions.cs
    │
    ├── Metadata/
    │   ├── AtomMetadataKeys.cs
    │   └── MetadataSerializer.cs
    │
    └── Internals/
        ├── TypeDetectorWrapper.cs
        └── ProcessorRegistry.cs

test/
└── DocumentAtom.DataIngestion.Test/
    ├── DocumentAtom.DataIngestion.Test.csproj
    │
    ├── Readers/
    │   ├── AtomDocumentReaderTests.cs
    │   ├── AtomDocumentReaderPdfTests.cs
    │   ├── AtomDocumentReaderDocxTests.cs
    │   ├── AtomDocumentReaderXlsxTests.cs
    │   ├── AtomDocumentReaderHtmlTests.cs
    │   ├── AtomDocumentReaderMarkdownTests.cs
    │   ├── AtomDocumentReaderJsonTests.cs
    │   ├── AtomDocumentReaderCsvTests.cs
    │   ├── AtomDocumentReaderXmlTests.cs
    │   ├── AtomDocumentReaderRtfTests.cs
    │   ├── AtomDocumentReaderImageTests.cs
    │   ├── AtomDocumentReaderTextTests.cs
    │   └── AtomDocumentReaderPptxTests.cs
    │
    ├── Chunkers/
    │   ├── AtomChunkerTests.cs
    │   ├── QuarkChunkingStrategyTests.cs
    │   ├── HierarchyAwareChunkerTests.cs
    │   ├── ChunkOverlapTests.cs
    │   └── ChunkMetadataPreservationTests.cs
    │
    ├── Converters/
    │   ├── AtomToIngestionElementConverterTests.cs
    │   ├── IngestionElementToAtomConverterTests.cs
    │   ├── MetadataMapperTests.cs
    │   └── RoundTripConversionTests.cs
    │
    ├── Integration/
    │   ├── FullPipelineTests.cs
    │   ├── VectorStoreIntegrationTests.cs
    │   ├── EnricherIntegrationTests.cs
    │   └── MultiDocumentPipelineTests.cs
    │
    ├── Factories/
    │   ├── DocumentAtomIngestionFactoryTests.cs
    │   └── PipelineBuilderTests.cs
    │
    ├── TestData/
    │   ├── sample.pdf
    │   ├── sample.docx
    │   ├── sample.xlsx
    │   ├── sample.pptx
    │   ├── sample.html
    │   ├── sample.md
    │   ├── sample.json
    │   ├── sample.csv
    │   ├── sample.xml
    │   ├── sample.rtf
    │   ├── sample.png
    │   └── sample.txt
    │
    └── Utilities/
        ├── TestHelpers.cs
        ├── MockProcessorFactory.cs
        └── InMemoryVectorStore.cs
```

### Project File

```xml
<!-- DocumentAtom.DataIngestion.csproj -->
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <LangVersion>latest</LangVersion>

    <PackageId>DocumentAtom.DataIngestion</PackageId>
    <Version>1.0.0</Version>
    <Authors>DocumentAtom Contributors</Authors>
    <Description>Microsoft.Extensions.DataIngestion integration for DocumentAtom document parsing library</Description>
    <PackageTags>document;parsing;ingestion;rag;ai;vector;embedding;chunking</PackageTags>
    <PackageReadmeFile>README.md</PackageReadmeFile>
    <PackageLicenseExpression>MIT</PackageLicenseExpression>
    <RepositoryUrl>https://github.com/DocumentAtom/DocumentAtom</RepositoryUrl>
  </PropertyGroup>

  <ItemGroup>
    <!-- Core DocumentAtom dependency -->
    <ProjectReference Include="..\DocumentAtom.Core\DocumentAtom.Core.csproj" />

    <!-- All processor dependencies -->
    <ProjectReference Include="..\DocumentAtom.Pdf\DocumentAtom.Pdf.csproj" />
    <ProjectReference Include="..\DocumentAtom.Word\DocumentAtom.Word.csproj" />
    <ProjectReference Include="..\DocumentAtom.Excel\DocumentAtom.Excel.csproj" />
    <ProjectReference Include="..\DocumentAtom.PowerPoint\DocumentAtom.PowerPoint.csproj" />
    <ProjectReference Include="..\DocumentAtom.Html\DocumentAtom.Html.csproj" />
    <ProjectReference Include="..\DocumentAtom.Markdown\DocumentAtom.Markdown.csproj" />
    <ProjectReference Include="..\DocumentAtom.Json\DocumentAtom.Json.csproj" />
    <ProjectReference Include="..\DocumentAtom.Csv\DocumentAtom.Csv.csproj" />
    <ProjectReference Include="..\DocumentAtom.Xml\DocumentAtom.Xml.csproj" />
    <ProjectReference Include="..\DocumentAtom.RichText\DocumentAtom.RichText.csproj" />
    <ProjectReference Include="..\DocumentAtom.Image\DocumentAtom.Image.csproj" />
    <ProjectReference Include="..\DocumentAtom.Text\DocumentAtom.Text.csproj" />
    <ProjectReference Include="..\DocumentAtom.TypeDetection\DocumentAtom.TypeDetection.csproj" />

    <!-- Microsoft DataIngestion -->
    <PackageReference Include="Microsoft.Extensions.DataIngestion" Version="9.0.0-preview.*" />
    <PackageReference Include="Microsoft.Extensions.DataIngestion.Abstractions" Version="9.0.0-preview.*" />
  </ItemGroup>

  <ItemGroup>
    <None Include="README.md" Pack="true" PackagePath="\" />
  </ItemGroup>

</Project>
```

---

## 4. Type Mappings

### Atom to IngestionDocumentElement Mapping

| AtomTypeEnum | IngestionDocumentElement Type | Notes |
|--------------|-------------------------------|-------|
| `Text` | `IngestionDocumentParagraph` | Direct content mapping |
| `List` (Ordered) | `IngestionDocumentParagraph` | Render as numbered markdown list |
| `List` (Unordered) | `IngestionDocumentParagraph` | Render as bulleted markdown list |
| `Table` | `IngestionDocumentTable` | Map SerializableDataTable to table structure or convert to DataTable and map |
| `Image` | `IngestionDocumentImage` | Map binary data and bounding box |
| `Binary` | `IngestionDocumentParagraph` | Base64 encode or skip based on config |
| `Hyperlink` | `IngestionDocumentParagraph` | Render as markdown link |
| `Code` | `IngestionDocumentParagraph` | Render as markdown code block |
| `Meta` | (Metadata only) | Attach to document metadata, no element |
| `Unknown` | `IngestionDocumentParagraph` | Best-effort text extraction |

### Atom Properties to IngestionDocument Metadata Mapping

| Atom Property | Metadata Key | Type | Notes |
|---------------|--------------|------|-------|
| `GUID` | `atom:guid` | `string` | Unique identifier |
| `ParentGUID` | `atom:parent_guid` | `string?` | Hierarchy reference |
| `Type` | `atom:type` | `string` | AtomTypeEnum name |
| `PageNumber` | `atom:page_number` | `int?` | Source page |
| `Position` | `atom:position` | `int` | Ordinal position |
| `Length` | `atom:length` | `int` | Content length |
| `MD5Hash` | `atom:md5` | `string?` | Content hash |
| `SHA1Hash` | `atom:sha1` | `string?` | Content hash |
| `SHA256Hash` | `atom:sha256` | `string?` | Content hash |
| `HeaderLevel` | `atom:header_level` | `int?` | For header atoms |
| `Title` | `atom:title` | `string?` | Atom title |
| `Subtitle` | `atom:subtitle` | `string?` | Atom subtitle |
| `Formatting` | `atom:formatting` | `string` | MarkdownFormattingEnum name |
| `BoundingBox` | `atom:bounding_box` | `string` | JSON serialized |
| `SheetName` | `atom:sheet_name` | `string?` | For Excel |
| `CellIdentifier` | `atom:cell_id` | `string?` | For Excel |
| `Rows` | `atom:rows` | `int?` | For tables |
| `Columns` | `atom:columns` | `int?` | For tables |
| `Quarks` | `atom:has_quarks` | `bool` | Indicates pre-chunked |
| `QuarkCount` | `atom:quark_count` | `int` | Number of quarks |

### IngestionChunk Metadata Mapping

| Metadata Key | Source | Notes |
|--------------|--------|-------|
| `atom:guid` | Atom.GUID or Quark.GUID | Chunk identifier |
| `atom:parent_guid` | Parent Atom GUID | For quarks, points to parent atom |
| `atom:document_guid` | Root Atom GUID | Document-level identifier |
| `atom:type` | Atom.Type | Content type |
| `atom:page_number` | Atom.PageNumber | Source location |
| `atom:position` | Atom.Position | Ordinal in document |
| `atom:chunk_index` | Computed | Index within atom's quarks |
| `atom:total_chunks` | Computed | Total quarks in parent |
| `atom:sha256` | Atom.SHA256Hash | Content integrity |
| `atom:bounding_box` | Atom.BoundingBox | Spatial information |
| `atom:header_context` | Computed | Ancestor headers for context |

---

## 5. Implementation Details

### 5.1 Reader Implementation

#### AtomDocumentReader.cs

```csharp
using DocumentAtom.Core;
using DocumentAtom.TypeDetection;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.Logging;

namespace DocumentAtom.DataIngestion.Readers;

/// <summary>
/// Reads documents using DocumentAtom processors and converts them to IngestionDocument format.
/// </summary>
public class AtomDocumentReader : IngestionDocumentReader
{
    private readonly AtomDocumentReaderOptions _options;
    private readonly IProcessorFactory _processorFactory;
    private readonly IAtomToIngestionElementConverter _converter;
    private readonly ILogger<AtomDocumentReader>? _logger;

    public AtomDocumentReader(
        AtomDocumentReaderOptions? options = null,
        IProcessorFactory? processorFactory = null,
        IAtomToIngestionElementConverter? converter = null,
        ILogger<AtomDocumentReader>? logger = null)
    {
        _options = options ?? new AtomDocumentReaderOptions();
        _processorFactory = processorFactory ?? new DefaultProcessorFactory(_options);
        _converter = converter ?? new AtomToIngestionElementConverter(_options);
        _logger = logger;
    }

    /// <summary>
    /// Reads a document from the specified path.
    /// </summary>
    public override async Task<IngestionDocument> ReadAsync(
        string path,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);

        _logger?.LogDebug("Reading document from {Path}", path);

        // Detect document type
        var typeResult = TypeDetector.DetectType(path);
        _logger?.LogDebug("Detected document type: {Type}", typeResult.DocumentType);

        // Get appropriate processor
        using var processor = _processorFactory.CreateProcessor(typeResult.DocumentType, path);
        if (processor == null)
        {
            throw new NotSupportedException(
                $"Document type '{typeResult.DocumentType}' is not supported.");
        }

        // Configure processor logging
        if (_logger != null)
        {
            processor.Logger = (severity, message) =>
            {
                var logLevel = severity switch
                {
                    SeverityEnum.Debug => LogLevel.Debug,
                    SeverityEnum.Info => LogLevel.Information,
                    SeverityEnum.Warn => LogLevel.Warning,
                    SeverityEnum.Error => LogLevel.Error,
                    _ => LogLevel.Information
                };
                _logger.Log(logLevel, "{ProcessorMessage}", message);
            };
        }

        // Extract atoms (potentially async for large files)
        var atoms = await Task.Run(
            () => processor.Extract(path).ToList(),
            cancellationToken);

        _logger?.LogDebug("Extracted {Count} atoms from document", atoms.Count);

        // Convert to IngestionDocument
        var document = ConvertToIngestionDocument(atoms, path, typeResult);

        _logger?.LogInformation(
            "Successfully read document {Path} with {ElementCount} elements",
            path,
            document.Elements.Count);

        return document;
    }

    /// <summary>
    /// Reads a document from a byte array.
    /// </summary>
    public override async Task<IngestionDocument> ReadAsync(
        byte[] data,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(data);

        // Detect type from bytes
        var typeResult = TypeDetector.DetectType(data);

        // Create temp file for processing
        var tempPath = Path.Combine(
            _options.TempDirectory,
            $"{Guid.NewGuid()}{GetExtension(typeResult.DocumentType)}");

        try
        {
            Directory.CreateDirectory(_options.TempDirectory);
            await File.WriteAllBytesAsync(tempPath, data, cancellationToken);

            var document = await ReadAsync(tempPath, cancellationToken);

            // Update source info
            document.Metadata["source:original_filename"] = fileName ?? "unknown";
            document.Metadata["source:was_byte_array"] = true;

            return document;
        }
        finally
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
    }

    /// <summary>
    /// Reads a document from a stream.
    /// </summary>
    public override async Task<IngestionDocument> ReadAsync(
        Stream stream,
        string? fileName = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);

        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream, cancellationToken);
        return await ReadAsync(memoryStream.ToArray(), fileName, cancellationToken);
    }

    private IngestionDocument ConvertToIngestionDocument(
        List<Atom> atoms,
        string sourcePath,
        TypeResult typeResult)
    {
        var document = new IngestionDocument
        {
            Id = Guid.NewGuid().ToString(),
            SourcePath = sourcePath
        };

        // Document-level metadata
        document.Metadata["source:path"] = sourcePath;
        document.Metadata["source:filename"] = Path.GetFileName(sourcePath);
        document.Metadata["source:extension"] = Path.GetExtension(sourcePath);
        document.Metadata["source:document_type"] = typeResult.DocumentType.ToString();
        document.Metadata["source:mime_type"] = typeResult.MimeType ?? "application/octet-stream";
        document.Metadata["atom:total_atoms"] = atoms.Count;
        document.Metadata["atom:extraction_timestamp"] = DateTimeOffset.UtcNow.ToString("O");

        // Build hierarchy map for context
        var hierarchyMap = BuildHierarchyMap(atoms);

        // Group atoms by page/section
        var sections = GroupAtomsIntoSections(atoms);

        foreach (var section in sections)
        {
            var ingestionSection = new IngestionDocumentSection
            {
                PageNumber = section.Key
            };

            foreach (var atom in section.Value)
            {
                var element = _converter.Convert(atom, hierarchyMap);
                if (element != null)
                {
                    // Store original atom for chunker access
                    if (_options.PreserveFullAtomData)
                    {
                        element.Metadata["atom:serialized"] =
                            System.Text.Json.JsonSerializer.Serialize(atom);
                    }

                    ingestionSection.Elements.Add(element);
                }
            }

            if (ingestionSection.Elements.Count > 0)
            {
                document.Sections.Add(ingestionSection);
            }
        }

        // Flatten elements for compatibility
        foreach (var section in document.Sections)
        {
            foreach (var element in section.Elements)
            {
                document.Elements.Add(element);
            }
        }

        return document;
    }

    private Dictionary<Guid, Atom> BuildHierarchyMap(List<Atom> atoms)
    {
        return atoms.ToDictionary(a => a.GUID, a => a);
    }

    private IEnumerable<IGrouping<int?, Atom>> GroupAtomsIntoSections(List<Atom> atoms)
    {
        return atoms.GroupBy(a => a.PageNumber ?? 0);
    }

    private string GetExtension(DocumentTypeEnum type)
    {
        return type switch
        {
            DocumentTypeEnum.Pdf => ".pdf",
            DocumentTypeEnum.Docx => ".docx",
            DocumentTypeEnum.Xlsx => ".xlsx",
            DocumentTypeEnum.Pptx => ".pptx",
            DocumentTypeEnum.Html => ".html",
            DocumentTypeEnum.Markdown => ".md",
            DocumentTypeEnum.Json => ".json",
            DocumentTypeEnum.Csv => ".csv",
            DocumentTypeEnum.Xml => ".xml",
            DocumentTypeEnum.Rtf => ".rtf",
            DocumentTypeEnum.Png => ".png",
            DocumentTypeEnum.Jpeg => ".jpg",
            DocumentTypeEnum.Txt => ".txt",
            _ => ".bin"
        };
    }
}
```

#### AtomDocumentReaderOptions.cs

```csharp
namespace DocumentAtom.DataIngestion.Readers;

/// <summary>
/// Configuration options for AtomDocumentReader.
/// </summary>
public class AtomDocumentReaderOptions
{
    /// <summary>
    /// Directory for temporary files during processing.
    /// </summary>
    public string TempDirectory { get; set; } = Path.Combine(Path.GetTempPath(), "DocumentAtom");

    /// <summary>
    /// Whether to preserve full serialized Atom data in element metadata.
    /// Enables lossless round-trip and advanced chunking.
    /// </summary>
    public bool PreserveFullAtomData { get; set; } = true;

    /// <summary>
    /// Whether to extract text from images using OCR.
    /// </summary>
    public bool EnableOcr { get; set; } = true;

    /// <summary>
    /// Whether to build document hierarchy from heading structure.
    /// </summary>
    public bool BuildHierarchy { get; set; } = true;

    /// <summary>
    /// Whether to include binary content (images, attachments) in output.
    /// </summary>
    public bool IncludeBinaryContent { get; set; } = true;

    /// <summary>
    /// Maximum file size in bytes to process. 0 = unlimited.
    /// </summary>
    public long MaxFileSizeBytes { get; set; } = 0;

    /// <summary>
    /// Settings for chunking during extraction.
    /// </summary>
    public ChunkingSettings? ChunkingSettings { get; set; }

    /// <summary>
    /// Custom processor settings by document type.
    /// </summary>
    public Dictionary<DocumentTypeEnum, ProcessorSettingsBase> ProcessorSettings { get; set; } = new();

    /// <summary>
    /// Metadata keys to exclude from output.
    /// </summary>
    public HashSet<string> ExcludedMetadataKeys { get; set; } = new();
}
```

#### ProcessorFactory.cs

```csharp
namespace DocumentAtom.DataIngestion.Readers;

/// <summary>
/// Factory for creating DocumentAtom processors based on document type.
/// </summary>
public interface IProcessorFactory
{
    ProcessorBase? CreateProcessor(DocumentTypeEnum documentType, string path);
}

/// <summary>
/// Default implementation of processor factory.
/// </summary>
public class DefaultProcessorFactory : IProcessorFactory
{
    private readonly AtomDocumentReaderOptions _options;

    public DefaultProcessorFactory(AtomDocumentReaderOptions options)
    {
        _options = options;
    }

    public ProcessorBase? CreateProcessor(DocumentTypeEnum documentType, string path)
    {
        // Get custom settings if provided
        _options.ProcessorSettings.TryGetValue(documentType, out var customSettings);

        return documentType switch
        {
            DocumentTypeEnum.Pdf => CreatePdfProcessor(customSettings),
            DocumentTypeEnum.Docx => CreateDocxProcessor(customSettings),
            DocumentTypeEnum.Xlsx => CreateXlsxProcessor(customSettings),
            DocumentTypeEnum.Pptx => CreatePptxProcessor(customSettings),
            DocumentTypeEnum.Html => CreateHtmlProcessor(customSettings),
            DocumentTypeEnum.Markdown => CreateMarkdownProcessor(customSettings),
            DocumentTypeEnum.Json => CreateJsonProcessor(customSettings),
            DocumentTypeEnum.Csv => CreateCsvProcessor(customSettings),
            DocumentTypeEnum.Xml => CreateXmlProcessor(customSettings),
            DocumentTypeEnum.Rtf => CreateRtfProcessor(customSettings),
            DocumentTypeEnum.Png or DocumentTypeEnum.Jpeg or
            DocumentTypeEnum.Gif or DocumentTypeEnum.Tiff or
            DocumentTypeEnum.Bmp or DocumentTypeEnum.WebP => CreateImageProcessor(customSettings),
            DocumentTypeEnum.Txt or DocumentTypeEnum.Unknown => CreateTextProcessor(customSettings),
            _ => null
        };
    }

    private PdfProcessor CreatePdfProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as PdfProcessorSettings ?? new PdfProcessorSettings();
        ApplyCommonSettings(settings);
        return new PdfProcessor(settings);
    }

    private DocxProcessor CreateDocxProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as DocxProcessorSettings ?? new DocxProcessorSettings();
        ApplyCommonSettings(settings);
        settings.BuildHierarchy = _options.BuildHierarchy;
        return new DocxProcessor(settings);
    }

    private XlsxProcessor CreateXlsxProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as XlsxProcessorSettings ?? new XlsxProcessorSettings();
        ApplyCommonSettings(settings);
        settings.BuildHierarchy = _options.BuildHierarchy;
        return new XlsxProcessor(settings);
    }

    private PptxProcessor CreatePptxProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as PptxProcessorSettings ?? new PptxProcessorSettings();
        ApplyCommonSettings(settings);
        return new PptxProcessor(settings);
    }

    private HtmlProcessor CreateHtmlProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as HtmlProcessorSettings ?? new HtmlProcessorSettings();
        ApplyCommonSettings(settings);
        return new HtmlProcessor(settings);
    }

    private MarkdownProcessor CreateMarkdownProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as MarkdownProcessorSettings ?? new MarkdownProcessorSettings();
        ApplyCommonSettings(settings);
        return new MarkdownProcessor(settings);
    }

    private JsonProcessor CreateJsonProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as JsonProcessorSettings ?? new JsonProcessorSettings();
        ApplyCommonSettings(settings);
        return new JsonProcessor(settings);
    }

    private CsvProcessor CreateCsvProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as CsvProcessorSettings ?? new CsvProcessorSettings();
        ApplyCommonSettings(settings);
        return new CsvProcessor(settings);
    }

    private XmlProcessor CreateXmlProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as XmlProcessorSettings ?? new XmlProcessorSettings();
        ApplyCommonSettings(settings);
        return new XmlProcessor(settings);
    }

    private RtfProcessor CreateRtfProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as RtfProcessorSettings ?? new RtfProcessorSettings();
        ApplyCommonSettings(settings);
        return new RtfProcessor(settings);
    }

    private ImageProcessor CreateImageProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as ImageProcessorSettings ?? new ImageProcessorSettings();
        ApplyCommonSettings(settings);
        settings.EnableOcr = _options.EnableOcr;
        return new ImageProcessor(settings);
    }

    private TextProcessor CreateTextProcessor(ProcessorSettingsBase? custom)
    {
        var settings = custom as TextProcessorSettings ?? new TextProcessorSettings();
        ApplyCommonSettings(settings);
        return new TextProcessor(settings);
    }

    private void ApplyCommonSettings(ProcessorSettingsBase settings)
    {
        settings.TempDirectory = _options.TempDirectory;
        settings.ExtractAtomsFromImages = _options.EnableOcr;

        if (_options.ChunkingSettings != null)
        {
            settings.Chunking = _options.ChunkingSettings;
        }
    }
}
```

### 5.2 Chunker Implementation

#### AtomChunker.cs

```csharp
using Microsoft.Extensions.DataIngestion;
using Microsoft.ML.Tokenizers;

namespace DocumentAtom.DataIngestion.Chunkers;

/// <summary>
/// Chunks IngestionDocuments using DocumentAtom's semantic boundaries.
/// Leverages pre-computed Quarks when available, falls back to token-based chunking.
/// </summary>
public class AtomChunker : IngestionChunker<string>
{
    private readonly AtomChunkerOptions _atomOptions;
    private readonly IAtomToIngestionChunkConverter _chunkConverter;

    public AtomChunker(
        IngestionChunkerOptions options,
        AtomChunkerOptions? atomOptions = null,
        IAtomToIngestionChunkConverter? chunkConverter = null)
        : base(options)
    {
        _atomOptions = atomOptions ?? new AtomChunkerOptions();
        _chunkConverter = chunkConverter ?? new AtomToIngestionChunkConverter();
    }

    /// <summary>
    /// Chunks the document using Atom boundaries.
    /// </summary>
    public override IEnumerable<IngestionChunk<string>> Chunk(IngestionDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var chunkIndex = 0;
        var headerContext = new Stack<string>();

        foreach (var element in document.Elements)
        {
            // Track header context for downstream use
            if (element is IngestionDocumentHeader header)
            {
                UpdateHeaderContext(headerContext, header);
            }

            // Try to get original Atom data
            if (_atomOptions.UseAtomBoundaries &&
                element.Metadata.TryGetValue("atom:serialized", out var atomJson) &&
                atomJson is string json)
            {
                var atom = System.Text.Json.JsonSerializer.Deserialize<Atom>(json);
                if (atom != null)
                {
                    foreach (var chunk in ChunkFromAtom(atom, document, headerContext, ref chunkIndex))
                    {
                        yield return chunk;
                    }
                    continue;
                }
            }

            // Fallback: treat element as single chunk or apply token-based chunking
            foreach (var chunk in ChunkFromElement(element, document, headerContext, ref chunkIndex))
            {
                yield return chunk;
            }
        }
    }

    private IEnumerable<IngestionChunk<string>> ChunkFromAtom(
        Atom atom,
        IngestionDocument document,
        Stack<string> headerContext,
        ref int chunkIndex)
    {
        // If atom has pre-computed quarks, use them
        if (_atomOptions.UseQuarks && atom.Quarks?.Count > 0)
        {
            var totalQuarks = atom.Quarks.Count;
            var quarkIndex = 0;

            foreach (var quark in atom.Quarks)
            {
                var chunk = _chunkConverter.Convert(
                    quark,
                    document,
                    chunkIndex++,
                    GetHeaderContextString(headerContext));

                // Add quark-specific metadata
                chunk.Metadata["atom:quark_index"] = quarkIndex;
                chunk.Metadata["atom:total_quarks"] = totalQuarks;
                chunk.Metadata["atom:parent_atom_guid"] = atom.GUID.ToString();

                quarkIndex++;
                yield return chunk;
            }
        }
        else
        {
            // Single atom as chunk
            var content = GetAtomContent(atom);

            // Check if content exceeds max tokens and needs splitting
            if (_atomOptions.RespectMaxTokens &&
                Options.Tokenizer != null &&
                Options.Tokenizer.CountTokens(content) > Options.MaxTokensPerChunk)
            {
                // Apply token-based splitting
                foreach (var chunk in SplitByTokens(content, atom, document, headerContext, ref chunkIndex))
                {
                    yield return chunk;
                }
            }
            else
            {
                yield return _chunkConverter.Convert(
                    atom,
                    document,
                    chunkIndex++,
                    GetHeaderContextString(headerContext));
            }
        }
    }

    private IEnumerable<IngestionChunk<string>> ChunkFromElement(
        IngestionDocumentElement element,
        IngestionDocument document,
        Stack<string> headerContext,
        ref int chunkIndex)
    {
        var content = element.Content ?? string.Empty;

        if (string.IsNullOrWhiteSpace(content))
        {
            yield break;
        }

        // Check if needs splitting
        if (Options.Tokenizer != null &&
            Options.Tokenizer.CountTokens(content) > Options.MaxTokensPerChunk)
        {
            foreach (var chunk in SplitByTokens(content, null, document, headerContext, ref chunkIndex))
            {
                // Copy element metadata
                foreach (var kvp in element.Metadata)
                {
                    if (!chunk.Metadata.ContainsKey(kvp.Key))
                    {
                        chunk.Metadata[kvp.Key] = kvp.Value;
                    }
                }
                yield return chunk;
            }
        }
        else
        {
            yield return new IngestionChunk<string>
            {
                Content = content,
                DocumentId = document.Id,
                ChunkIndex = chunkIndex++,
                Metadata = new Dictionary<string, object>(element.Metadata)
                {
                    ["chunk:header_context"] = GetHeaderContextString(headerContext),
                    ["chunk:source"] = "element"
                }
            };
        }
    }

    private IEnumerable<IngestionChunk<string>> SplitByTokens(
        string content,
        Atom? sourceAtom,
        IngestionDocument document,
        Stack<string> headerContext,
        ref int chunkIndex)
    {
        var tokenizer = Options.Tokenizer!;
        var maxTokens = Options.MaxTokensPerChunk;
        var overlap = Options.OverlapTokens;

        // Use StringHelper-style word-boundary splitting for better results
        var words = content.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var currentChunk = new List<string>();
        var currentTokens = 0;
        var splitIndex = 0;

        foreach (var word in words)
        {
            var wordTokens = tokenizer.CountTokens(word + " ");

            if (currentTokens + wordTokens > maxTokens && currentChunk.Count > 0)
            {
                // Emit current chunk
                var chunkContent = string.Join(" ", currentChunk);
                yield return CreateSplitChunk(
                    chunkContent,
                    sourceAtom,
                    document,
                    headerContext,
                    ref chunkIndex,
                    splitIndex++);

                // Handle overlap
                if (overlap > 0)
                {
                    var overlapWords = new List<string>();
                    var overlapTokens = 0;

                    for (int i = currentChunk.Count - 1; i >= 0 && overlapTokens < overlap; i--)
                    {
                        var wt = tokenizer.CountTokens(currentChunk[i] + " ");
                        if (overlapTokens + wt <= overlap)
                        {
                            overlapWords.Insert(0, currentChunk[i]);
                            overlapTokens += wt;
                        }
                        else break;
                    }

                    currentChunk = overlapWords;
                    currentTokens = overlapTokens;
                }
                else
                {
                    currentChunk.Clear();
                    currentTokens = 0;
                }
            }

            currentChunk.Add(word);
            currentTokens += wordTokens;
        }

        // Emit final chunk
        if (currentChunk.Count > 0)
        {
            var chunkContent = string.Join(" ", currentChunk);
            yield return CreateSplitChunk(
                chunkContent,
                sourceAtom,
                document,
                headerContext,
                ref chunkIndex,
                splitIndex);
        }
    }

    private IngestionChunk<string> CreateSplitChunk(
        string content,
        Atom? sourceAtom,
        IngestionDocument document,
        Stack<string> headerContext,
        ref int chunkIndex,
        int splitIndex)
    {
        var chunk = new IngestionChunk<string>
        {
            Content = content,
            DocumentId = document.Id,
            ChunkIndex = chunkIndex++,
            Metadata = new Dictionary<string, object>
            {
                ["chunk:header_context"] = GetHeaderContextString(headerContext),
                ["chunk:split_index"] = splitIndex,
                ["chunk:source"] = "token_split"
            }
        };

        if (sourceAtom != null)
        {
            chunk.Metadata["atom:guid"] = sourceAtom.GUID.ToString();
            chunk.Metadata["atom:type"] = sourceAtom.Type.ToString();
            chunk.Metadata["atom:page_number"] = sourceAtom.PageNumber;
        }

        return chunk;
    }

    private string GetAtomContent(Atom atom)
    {
        if (!string.IsNullOrEmpty(atom.Text))
            return atom.Text;

        if (atom.OrderedList?.Count > 0)
            return string.Join("\n", atom.OrderedList.Select((item, i) => $"{i + 1}. {item}"));

        if (atom.UnorderedList?.Count > 0)
            return string.Join("\n", atom.UnorderedList.Select(item => $"- {item}"));

        if (atom.Table != null)
            return RenderTableAsMarkdown(atom.Table);

        return string.Empty;
    }

    private string RenderTableAsMarkdown(SerializableDataTable table)
    {
        var sb = new StringBuilder();

        // Header row
        if (table.Columns?.Count > 0)
        {
            sb.AppendLine("| " + string.Join(" | ", table.Columns) + " |");
            sb.AppendLine("| " + string.Join(" | ", table.Columns.Select(_ => "---")) + " |");
        }

        // Data rows
        if (table.Rows != null)
        {
            foreach (var row in table.Rows)
            {
                sb.AppendLine("| " + string.Join(" | ", row) + " |");
            }
        }

        return sb.ToString();
    }

    private void UpdateHeaderContext(Stack<string> context, IngestionDocumentHeader header)
    {
        var level = header.Level;

        // Pop headers at same or lower level
        while (context.Count >= level)
        {
            context.Pop();
        }

        context.Push(header.Content ?? string.Empty);
    }

    private string GetHeaderContextString(Stack<string> context)
    {
        if (context.Count == 0)
            return string.Empty;

        return string.Join(" > ", context.Reverse());
    }
}
```

#### AtomChunkerOptions.cs

```csharp
namespace DocumentAtom.DataIngestion.Chunkers;

/// <summary>
/// Options specific to AtomChunker behavior.
/// </summary>
public class AtomChunkerOptions
{
    /// <summary>
    /// Whether to use Atom boundaries for chunking (vs. pure token-based).
    /// Default: true
    /// </summary>
    public bool UseAtomBoundaries { get; set; } = true;

    /// <summary>
    /// Whether to use pre-computed Quarks when available.
    /// Default: true
    /// </summary>
    public bool UseQuarks { get; set; } = true;

    /// <summary>
    /// Whether to respect MaxTokensPerChunk even for atoms/quarks.
    /// If true, large atoms will be split; if false, they pass through as-is.
    /// Default: true
    /// </summary>
    public bool RespectMaxTokens { get; set; } = true;

    /// <summary>
    /// Whether to include header context in chunk metadata.
    /// Default: true
    /// </summary>
    public bool IncludeHeaderContext { get; set; } = true;

    /// <summary>
    /// Whether to preserve parent-child relationships in metadata.
    /// Default: true
    /// </summary>
    public bool PreserveHierarchy { get; set; } = true;

    /// <summary>
    /// Minimum content length for a chunk to be emitted.
    /// Chunks shorter than this are merged with adjacent chunks.
    /// Default: 0 (no minimum)
    /// </summary>
    public int MinChunkLength { get; set; } = 0;

    /// <summary>
    /// Whether to include table content in chunks.
    /// Default: true
    /// </summary>
    public bool IncludeTables { get; set; } = true;

    /// <summary>
    /// Whether to include image descriptions/OCR text in chunks.
    /// Default: true
    /// </summary>
    public bool IncludeImageText { get; set; } = true;
}
```

#### HierarchyAwareChunker.cs

```csharp
namespace DocumentAtom.DataIngestion.Chunkers;

/// <summary>
/// Chunker that preserves and utilizes document hierarchy.
/// Groups related content under parent headings.
/// </summary>
public class HierarchyAwareChunker : AtomChunker
{
    private readonly HierarchyChunkerOptions _hierarchyOptions;

    public HierarchyAwareChunker(
        IngestionChunkerOptions options,
        HierarchyChunkerOptions? hierarchyOptions = null)
        : base(options)
    {
        _hierarchyOptions = hierarchyOptions ?? new HierarchyChunkerOptions();
    }

    public override IEnumerable<IngestionChunk<string>> Chunk(IngestionDocument document)
    {
        if (!_hierarchyOptions.GroupBySection)
        {
            // Fall back to base behavior
            foreach (var chunk in base.Chunk(document))
            {
                yield return chunk;
            }
            yield break;
        }

        // Group atoms by their parent hierarchy
        var sections = GroupByHierarchy(document);

        var chunkIndex = 0;
        foreach (var section in sections)
        {
            var sectionContent = new StringBuilder();
            var sectionMetadata = new Dictionary<string, object>
            {
                ["section:title"] = section.Title,
                ["section:level"] = section.Level,
                ["section:atom_count"] = section.Atoms.Count
            };

            // Combine section content
            foreach (var atom in section.Atoms)
            {
                var content = GetAtomContent(atom);
                if (!string.IsNullOrWhiteSpace(content))
                {
                    sectionContent.AppendLine(content);
                    sectionContent.AppendLine();
                }
            }

            var fullContent = sectionContent.ToString().Trim();

            if (string.IsNullOrWhiteSpace(fullContent))
                continue;

            // Check if section needs to be split
            if (Options.Tokenizer != null &&
                Options.Tokenizer.CountTokens(fullContent) > Options.MaxTokensPerChunk)
            {
                // Split large sections
                foreach (var chunk in SplitSection(fullContent, section, document, ref chunkIndex))
                {
                    yield return chunk;
                }
            }
            else
            {
                yield return new IngestionChunk<string>
                {
                    Content = fullContent,
                    DocumentId = document.Id,
                    ChunkIndex = chunkIndex++,
                    Metadata = sectionMetadata
                };
            }
        }
    }

    private List<DocumentSection> GroupByHierarchy(IngestionDocument document)
    {
        var sections = new List<DocumentSection>();
        DocumentSection? currentSection = null;

        foreach (var element in document.Elements)
        {
            if (element is IngestionDocumentHeader header)
            {
                // Start new section
                if (currentSection != null)
                {
                    sections.Add(currentSection);
                }

                currentSection = new DocumentSection
                {
                    Title = header.Content ?? "Untitled",
                    Level = header.Level
                };
            }
            else if (currentSection != null)
            {
                // Try to recover Atom
                if (element.Metadata.TryGetValue("atom:serialized", out var json) &&
                    json is string jsonStr)
                {
                    var atom = System.Text.Json.JsonSerializer.Deserialize<Atom>(jsonStr);
                    if (atom != null)
                    {
                        currentSection.Atoms.Add(atom);
                    }
                }
            }
        }

        if (currentSection != null)
        {
            sections.Add(currentSection);
        }

        return sections;
    }

    private IEnumerable<IngestionChunk<string>> SplitSection(
        string content,
        DocumentSection section,
        IngestionDocument document,
        ref int chunkIndex)
    {
        // Reuse base splitting logic
        var splitIndex = 0;
        // ... token-based splitting implementation
        yield break; // Placeholder
    }

    private class DocumentSection
    {
        public string Title { get; set; } = string.Empty;
        public int Level { get; set; }
        public List<Atom> Atoms { get; set; } = new();
    }
}

public class HierarchyChunkerOptions
{
    /// <summary>
    /// Whether to group content by document sections/headers.
    /// </summary>
    public bool GroupBySection { get; set; } = true;

    /// <summary>
    /// Maximum section level to group by (1 = H1 only, 2 = H1+H2, etc.)
    /// </summary>
    public int MaxSectionLevel { get; set; } = 3;

    /// <summary>
    /// Whether to include parent section titles in child chunks.
    /// </summary>
    public bool IncludeParentTitles { get; set; } = true;
}
```

### 5.3 Document Processor Implementation

#### AtomDocumentProcessor.cs

```csharp
namespace DocumentAtom.DataIngestion.Processors;

/// <summary>
/// Document processor that applies DocumentAtom-specific transformations.
/// </summary>
public class AtomDocumentProcessor : IngestionDocumentProcessor
{
    private readonly AtomDocumentProcessorOptions _options;

    public AtomDocumentProcessor(AtomDocumentProcessorOptions? options = null)
    {
        _options = options ?? new AtomDocumentProcessorOptions();
    }

    public override async Task<IngestionDocument> ProcessAsync(
        IngestionDocument document,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(document);

        // Apply hierarchy building if requested
        if (_options.BuildHierarchy)
        {
            BuildHierarchy(document);
        }

        // Apply deduplication if requested
        if (_options.DeduplicateContent)
        {
            DeduplicateElements(document);
        }

        // Compute additional hashes if requested
        if (_options.ComputeHashes)
        {
            await ComputeHashesAsync(document, cancellationToken);
        }

        return document;
    }

    private void BuildHierarchy(IngestionDocument document)
    {
        var headerStack = new Stack<(int Level, string Id)>();

        foreach (var element in document.Elements)
        {
            if (element is IngestionDocumentHeader header)
            {
                // Pop headers at same or lower level
                while (headerStack.Count > 0 && headerStack.Peek().Level >= header.Level)
                {
                    headerStack.Pop();
                }

                // Record parent
                if (headerStack.Count > 0)
                {
                    element.Metadata["hierarchy:parent_id"] = headerStack.Peek().Id;
                }

                // Push current header
                var headerId = element.Metadata.TryGetValue("atom:guid", out var guid)
                    ? guid.ToString()!
                    : Guid.NewGuid().ToString();

                headerStack.Push((header.Level, headerId));
                element.Metadata["hierarchy:id"] = headerId;
            }
            else if (headerStack.Count > 0)
            {
                // Assign to current section
                element.Metadata["hierarchy:parent_id"] = headerStack.Peek().Id;
            }
        }
    }

    private void DeduplicateElements(IngestionDocument document)
    {
        var seen = new HashSet<string>();
        var toRemove = new List<IngestionDocumentElement>();

        foreach (var element in document.Elements)
        {
            // Use SHA256 if available, otherwise compute from content
            string hash;
            if (element.Metadata.TryGetValue("atom:sha256", out var sha256) && sha256 != null)
            {
                hash = sha256.ToString()!;
            }
            else
            {
                hash = ComputeContentHash(element.Content ?? string.Empty);
            }

            if (seen.Contains(hash))
            {
                toRemove.Add(element);
            }
            else
            {
                seen.Add(hash);
            }
        }

        foreach (var element in toRemove)
        {
            document.Elements.Remove(element);
        }

        document.Metadata["processor:duplicates_removed"] = toRemove.Count;
    }

    private async Task ComputeHashesAsync(
        IngestionDocument document,
        CancellationToken cancellationToken)
    {
        await Task.Run(() =>
        {
            foreach (var element in document.Elements)
            {
                if (!element.Metadata.ContainsKey("atom:sha256") &&
                    !string.IsNullOrEmpty(element.Content))
                {
                    element.Metadata["computed:sha256"] = ComputeContentHash(element.Content);
                }
            }
        }, cancellationToken);
    }

    private string ComputeContentHash(string content)
    {
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}

public class AtomDocumentProcessorOptions
{
    /// <summary>
    /// Whether to build document hierarchy from headers.
    /// </summary>
    public bool BuildHierarchy { get; set; } = true;

    /// <summary>
    /// Whether to remove duplicate content based on hash.
    /// </summary>
    public bool DeduplicateContent { get; set; } = false;

    /// <summary>
    /// Whether to compute hashes for elements that don't have them.
    /// </summary>
    public bool ComputeHashes { get; set; } = false;
}
```

### 5.4 Extension Methods and Factories

#### ServiceCollectionExtensions.cs

```csharp
using Microsoft.Extensions.DependencyInjection;

namespace DocumentAtom.DataIngestion.Extensions;

/// <summary>
/// Extension methods for registering DocumentAtom.DataIngestion services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds DocumentAtom data ingestion services to the service collection.
    /// </summary>
    public static IServiceCollection AddDocumentAtomIngestion(
        this IServiceCollection services,
        Action<AtomDocumentReaderOptions>? configureReader = null,
        Action<AtomChunkerOptions>? configureChunker = null)
    {
        // Configure options
        if (configureReader != null)
        {
            services.Configure(configureReader);
        }

        if (configureChunker != null)
        {
            services.Configure(configureChunker);
        }

        // Register services
        services.AddSingleton<IProcessorFactory, DefaultProcessorFactory>();
        services.AddSingleton<IAtomToIngestionElementConverter, AtomToIngestionElementConverter>();
        services.AddSingleton<IAtomToIngestionChunkConverter, AtomToIngestionChunkConverter>();

        // Register reader and chunker
        services.AddTransient<IngestionDocumentReader, AtomDocumentReader>();
        services.AddTransient<IngestionChunker<string>, AtomChunker>();

        return services;
    }

    /// <summary>
    /// Adds a pre-configured DocumentAtom ingestion pipeline.
    /// </summary>
    public static IServiceCollection AddDocumentAtomPipeline<TVectorStore>(
        this IServiceCollection services,
        Action<DocumentAtomPipelineOptions>? configure = null)
        where TVectorStore : class
    {
        var options = new DocumentAtomPipelineOptions();
        configure?.Invoke(options);

        services.AddDocumentAtomIngestion(
            reader => { /* apply from options */ },
            chunker => { /* apply from options */ });

        // Register pipeline
        services.AddTransient(sp =>
        {
            var reader = sp.GetRequiredService<IngestionDocumentReader>();
            var chunker = sp.GetRequiredService<IngestionChunker<string>>();
            var writer = sp.GetRequiredService<IngestionChunkWriter<string>>();

            return new IngestionPipeline<string>(reader, chunker, writer);
        });

        return services;
    }
}

public class DocumentAtomPipelineOptions
{
    public AtomDocumentReaderOptions ReaderOptions { get; set; } = new();
    public AtomChunkerOptions ChunkerOptions { get; set; } = new();
    public IngestionChunkerOptions TokenizerOptions { get; set; } = new();
}
```

#### DocumentAtomIngestionFactory.cs

```csharp
namespace DocumentAtom.DataIngestion.Factories;

/// <summary>
/// Factory for creating DocumentAtom-based ingestion components.
/// </summary>
public static class DocumentAtomIngestionFactory
{
    /// <summary>
    /// Creates an AtomDocumentReader with default settings.
    /// </summary>
    public static AtomDocumentReader CreateReader(
        AtomDocumentReaderOptions? options = null)
    {
        return new AtomDocumentReader(options);
    }

    /// <summary>
    /// Creates an AtomChunker with the specified tokenizer.
    /// </summary>
    public static AtomChunker CreateChunker(
        Tokenizer? tokenizer = null,
        int maxTokensPerChunk = 512,
        int overlapTokens = 50,
        AtomChunkerOptions? atomOptions = null)
    {
        var options = new IngestionChunkerOptions(tokenizer ?? TiktokenTokenizer.CreateForModel("gpt-4"))
        {
            MaxTokensPerChunk = maxTokensPerChunk,
            OverlapTokens = overlapTokens
        };

        return new AtomChunker(options, atomOptions);
    }

    /// <summary>
    /// Creates a complete ingestion pipeline with DocumentAtom components.
    /// </summary>
    public static IngestionPipeline<string> CreatePipeline(
        IngestionChunkWriter<string> writer,
        AtomDocumentReaderOptions? readerOptions = null,
        AtomChunkerOptions? chunkerOptions = null,
        Tokenizer? tokenizer = null,
        int maxTokensPerChunk = 512,
        int overlapTokens = 50)
    {
        var reader = CreateReader(readerOptions);
        var chunker = CreateChunker(tokenizer, maxTokensPerChunk, overlapTokens, chunkerOptions);

        return new IngestionPipeline<string>(reader, chunker, writer);
    }

    /// <summary>
    /// Creates a pipeline configured for vector store output.
    /// </summary>
    public static IngestionPipeline<string> CreateVectorStorePipeline(
        IVectorStore vectorStore,
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator,
        int dimensionCount,
        AtomDocumentReaderOptions? readerOptions = null,
        AtomChunkerOptions? chunkerOptions = null,
        Tokenizer? tokenizer = null,
        int maxTokensPerChunk = 512)
    {
        var writer = new VectorStoreWriter<string>(
            vectorStore,
            new VectorStoreWriterOptions
            {
                DimensionCount = dimensionCount,
                EmbeddingGenerator = embeddingGenerator
            });

        return CreatePipeline(
            writer,
            readerOptions,
            chunkerOptions,
            tokenizer,
            maxTokensPerChunk);
    }
}
```

---

## 6. Configuration and Options

### Configuration Hierarchy

```
DocumentAtomPipelineOptions
├── AtomDocumentReaderOptions
│   ├── TempDirectory
│   ├── PreserveFullAtomData
│   ├── EnableOcr
│   ├── BuildHierarchy
│   ├── IncludeBinaryContent
│   ├── MaxFileSizeBytes
│   ├── ChunkingSettings (DocumentAtom.Core)
│   │   ├── Enable
│   │   ├── MaximumLength
│   │   ├── ShiftSize
│   │   └── MaximumWords
│   └── ProcessorSettings (per document type)
│       ├── PdfProcessorSettings
│       ├── DocxProcessorSettings
│       └── ...
│
├── AtomChunkerOptions
│   ├── UseAtomBoundaries
│   ├── UseQuarks
│   ├── RespectMaxTokens
│   ├── IncludeHeaderContext
│   ├── PreserveHierarchy
│   ├── MinChunkLength
│   ├── IncludeTables
│   └── IncludeImageText
│
└── IngestionChunkerOptions (Microsoft)
    ├── Tokenizer
    ├── MaxTokensPerChunk
    └── OverlapTokens
```

### Configuration Examples

#### Basic Configuration

```csharp
var reader = DocumentAtomIngestionFactory.CreateReader(new AtomDocumentReaderOptions
{
    EnableOcr = true,
    BuildHierarchy = true,
    ChunkingSettings = new ChunkingSettings
    {
        Enable = true,
        MaximumLength = 1024,
        ShiftSize = 512  // 50% overlap
    }
});
```

#### Advanced Configuration with DI

```csharp
services.AddDocumentAtomIngestion(
    reader =>
    {
        reader.EnableOcr = true;
        reader.PreserveFullAtomData = true;
        reader.ProcessorSettings[DocumentTypeEnum.Pdf] = new PdfProcessorSettings
        {
            ExtractTables = true,
            UseTabula = true
        };
    },
    chunker =>
    {
        chunker.UseQuarks = true;
        chunker.IncludeHeaderContext = true;
        chunker.MinChunkLength = 100;
    });
```

---

## 7. Metadata Preservation Strategy

### Metadata Key Namespaces

| Prefix | Purpose | Example |
|--------|---------|---------|
| `atom:` | DocumentAtom-specific metadata | `atom:guid`, `atom:type` |
| `source:` | Source document information | `source:path`, `source:mime_type` |
| `chunk:` | Chunk-specific metadata | `chunk:index`, `chunk:header_context` |
| `hierarchy:` | Document structure | `hierarchy:parent_id`, `hierarchy:level` |
| `computed:` | Computed during processing | `computed:sha256` |
| `processor:` | Processing statistics | `processor:duplicates_removed` |
| `section:` | Section grouping info | `section:title`, `section:level` |

### Metadata Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           Original Atom                                      │
│  GUID, ParentGUID, Type, PageNumber, Position, Length, MD5Hash, SHA1Hash,   │
│  SHA256Hash, HeaderLevel, Title, Subtitle, Formatting, BoundingBox,         │
│  SheetName, CellIdentifier, Rows, Columns, Quarks                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                        IngestionDocumentElement                              │
│  Metadata:                                                                   │
│    atom:guid, atom:parent_guid, atom:type, atom:page_number,                │
│    atom:position, atom:length, atom:md5, atom:sha1, atom:sha256,            │
│    atom:header_level, atom:title, atom:subtitle, atom:formatting,           │
│    atom:bounding_box (JSON), atom:sheet_name, atom:cell_id,                 │
│    atom:rows, atom:columns, atom:has_quarks, atom:quark_count,              │
│    atom:serialized (full JSON - optional)                                   │
└─────────────────────────────────────────────────────────────────────────────┘
                                    │
                                    ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          IngestionChunk<string>                              │
│  Metadata (inherited + added):                                               │
│    All atom:* metadata from source element                                   │
│    chunk:index, chunk:header_context, chunk:source,                         │
│    chunk:split_index (if token-split),                                      │
│    atom:quark_index, atom:total_quarks, atom:parent_atom_guid (if quark)    │
└─────────────────────────────────────────────────────────────────────────────┘
```

### Metadata Serialization

```csharp
public static class MetadataSerializer
{
    /// <summary>
    /// Serializes complex metadata values to JSON strings.
    /// </summary>
    public static Dictionary<string, object> SerializeMetadata(Atom atom)
    {
        var metadata = new Dictionary<string, object>();

        // Simple values
        metadata["atom:guid"] = atom.GUID.ToString();
        metadata["atom:type"] = atom.Type.ToString();

        // Nullable values
        if (atom.ParentGUID.HasValue)
            metadata["atom:parent_guid"] = atom.ParentGUID.Value.ToString();

        if (atom.PageNumber.HasValue)
            metadata["atom:page_number"] = atom.PageNumber.Value;

        // Complex values (serialize to JSON)
        if (atom.BoundingBox != null)
        {
            metadata["atom:bounding_box"] = JsonSerializer.Serialize(atom.BoundingBox);
        }

        // Quark info
        metadata["atom:has_quarks"] = atom.Quarks?.Count > 0;
        if (atom.Quarks?.Count > 0)
        {
            metadata["atom:quark_count"] = atom.Quarks.Count;
        }

        return metadata;
    }

    /// <summary>
    /// Deserializes metadata back to Atom properties.
    /// </summary>
    public static Atom? DeserializeAtom(Dictionary<string, object> metadata)
    {
        if (!metadata.TryGetValue("atom:serialized", out var json) || json is not string jsonStr)
            return null;

        return JsonSerializer.Deserialize<Atom>(jsonStr);
    }
}
```

---

## 8. Test Plan

### 8.1 Unit Tests

#### Reader Tests

| Test Class | Test Method | Description |
|------------|-------------|-------------|
| `AtomDocumentReaderTests` | `ReadAsync_WithValidPath_ReturnsIngestionDocument` | Basic read functionality |
| | `ReadAsync_WithInvalidPath_ThrowsFileNotFoundException` | Error handling |
| | `ReadAsync_WithNullPath_ThrowsArgumentException` | Input validation |
| | `ReadAsync_WithByteArray_CreatesValidDocument` | Byte array input |
| | `ReadAsync_WithStream_CreatesValidDocument` | Stream input |
| | `ReadAsync_PreservesDocumentMetadata` | Metadata preservation |
| | `ReadAsync_DetectsDocumentType_Correctly` | Type detection |
| | `ReadAsync_WithCustomSettings_AppliesSettings` | Settings application |
| `AtomDocumentReaderPdfTests` | `ReadAsync_Pdf_ExtractsText` | PDF text extraction |
| | `ReadAsync_Pdf_ExtractsTables` | PDF table extraction |
| | `ReadAsync_Pdf_ExtractsImages` | PDF image extraction |
| | `ReadAsync_Pdf_PreservesBoundingBox` | Spatial metadata |
| | `ReadAsync_Pdf_MultiPage_GroupsBySection` | Multi-page handling |
| `AtomDocumentReaderDocxTests` | `ReadAsync_Docx_ExtractsText` | Word text extraction |
| | `ReadAsync_Docx_ExtractsHeaders` | Header detection |
| | `ReadAsync_Docx_ExtractsTables` | Table extraction |
| | `ReadAsync_Docx_ExtractsImages` | Image extraction |
| | `ReadAsync_Docx_BuildsHierarchy` | Hierarchy building |
| `AtomDocumentReaderXlsxTests` | `ReadAsync_Xlsx_ExtractsSheets` | Sheet extraction |
| | `ReadAsync_Xlsx_ExtractsTableData` | Data extraction |
| | `ReadAsync_Xlsx_PreservesSheetNames` | Sheet metadata |
| `AtomDocumentReaderHtmlTests` | `ReadAsync_Html_ExtractsText` | HTML text |
| | `ReadAsync_Html_ExtractsHeaders` | HTML headers |
| | `ReadAsync_Html_ExtractsTables` | HTML tables |
| | `ReadAsync_Html_ExtractsLinks` | Hyperlinks |
| `AtomDocumentReaderMarkdownTests` | `ReadAsync_Md_ExtractsHeaders` | Markdown headers |
| | `ReadAsync_Md_ExtractsLists` | Lists |
| | `ReadAsync_Md_ExtractsCodeBlocks` | Code blocks |
| | `ReadAsync_Md_ExtractsTables` | Markdown tables |
| `AtomDocumentReaderJsonTests` | `ReadAsync_Json_ExtractsKeyValues` | JSON parsing |
| | `ReadAsync_Json_HandlesNested` | Nested structures |
| `AtomDocumentReaderCsvTests` | `ReadAsync_Csv_ExtractsRows` | CSV parsing |
| | `ReadAsync_Csv_DetectsHeaders` | Header detection |
| `AtomDocumentReaderXmlTests` | `ReadAsync_Xml_ExtractsElements` | XML parsing |
| `AtomDocumentReaderRtfTests` | `ReadAsync_Rtf_ExtractsText` | RTF parsing |
| | `ReadAsync_Rtf_PreservesFormatting` | Formatting preservation |
| `AtomDocumentReaderImageTests` | `ReadAsync_Image_PerformsOcr` | OCR functionality |
| | `ReadAsync_Image_DetectsTables` | Table detection |
| | `ReadAsync_Image_WithOcrDisabled_ReturnsEmptyText` | OCR toggle |
| `AtomDocumentReaderTextTests` | `ReadAsync_Text_ExtractsContent` | Plain text |
| | `ReadAsync_Text_SplitsByDelimiter` | Delimiter handling |
| `AtomDocumentReaderPptxTests` | `ReadAsync_Pptx_ExtractsSlides` | Slide extraction |
| | `ReadAsync_Pptx_ExtractsText` | Text from slides |

#### Chunker Tests

| Test Class | Test Method | Description |
|------------|-------------|-------------|
| `AtomChunkerTests` | `Chunk_WithQuarks_UsesQuarkBoundaries` | Quark-based chunking |
| | `Chunk_WithoutQuarks_UsesAtomBoundaries` | Atom-based chunking |
| | `Chunk_ExceedsMaxTokens_SplitsContent` | Token limit handling |
| | `Chunk_PreservesMetadata` | Metadata preservation |
| | `Chunk_WithHeaderContext_IncludesContext` | Header context |
| | `Chunk_EmptyDocument_ReturnsEmpty` | Empty input handling |
| | `Chunk_SingleElement_ReturnsSingleChunk` | Single element |
| | `Chunk_MultipleElements_ReturnsMultipleChunks` | Multiple elements |
| `QuarkChunkingStrategyTests` | `Chunk_WithPrecomputedQuarks_ReturnsQuarks` | Quark extraction |
| | `Chunk_QuarkMetadata_IncludesParentReference` | Parent reference |
| | `Chunk_QuarkIndex_IsCorrect` | Index tracking |
| `HierarchyAwareChunkerTests` | `Chunk_GroupsBySection_CombinesContent` | Section grouping |
| | `Chunk_LargeSection_Splits` | Large section handling |
| | `Chunk_PreservesHeaderHierarchy` | Hierarchy preservation |
| `ChunkOverlapTests` | `Chunk_WithOverlap_CreatesOverlappingChunks` | Overlap calculation |
| | `Chunk_OverlapTokenCount_IsCorrect` | Overlap token count |
| | `Chunk_ZeroOverlap_NoOverlap` | No overlap case |
| `ChunkMetadataPreservationTests` | `Chunk_PreservesAtomGuid` | GUID preservation |
| | `Chunk_PreservesPageNumber` | Page number |
| | `Chunk_PreservesBoundingBox` | Bounding box |
| | `Chunk_PreservesHash` | Hash values |

#### Converter Tests

| Test Class | Test Method | Description |
|------------|-------------|-------------|
| `AtomToIngestionElementConverterTests` | `Convert_TextAtom_ReturnsParagraph` | Text conversion |
| | `Convert_TableAtom_ReturnsTable` | Table conversion |
| | `Convert_ImageAtom_ReturnsImage` | Image conversion |
| | `Convert_ListAtom_ReturnsParagraphWithList` | List conversion |
| | `Convert_CodeAtom_ReturnsParagraphWithCode` | Code conversion |
| | `Convert_HyperlinkAtom_ReturnsParagraphWithLink` | Hyperlink conversion |
| | `Convert_MetaAtom_ReturnsNull` | Meta handling |
| | `Convert_PreservesAllMetadata` | Metadata preservation |
| `IngestionElementToAtomConverterTests` | `Convert_Paragraph_ReturnsTextAtom` | Reverse conversion |
| | `Convert_Table_ReturnsTableAtom` | Table reverse |
| | `Convert_Image_ReturnsImageAtom` | Image reverse |
| `MetadataMapperTests` | `Map_AllAtomProperties_ToMetadata` | Full property mapping |
| | `Map_NullProperties_Excluded` | Null handling |
| | `Map_ComplexProperties_Serialized` | Complex value serialization |
| `RoundTripConversionTests` | `RoundTrip_TextAtom_PreservesAllData` | Round-trip text |
| | `RoundTrip_TableAtom_PreservesAllData` | Round-trip table |
| | `RoundTrip_ImageAtom_PreservesAllData` | Round-trip image |
| | `RoundTrip_WithQuarks_PreservesQuarks` | Round-trip quarks |

### 8.2 Integration Tests

| Test Class | Test Method | Description |
|------------|-------------|-------------|
| `FullPipelineTests` | `Pipeline_PdfToChunks_Succeeds` | End-to-end PDF |
| | `Pipeline_DocxToChunks_Succeeds` | End-to-end Word |
| | `Pipeline_HtmlToChunks_Succeeds` | End-to-end HTML |
| | `Pipeline_MultiFormat_Succeeds` | Mixed formats |
| | `Pipeline_WithEnrichers_AppliesEnrichment` | Enricher integration |
| | `Pipeline_PartialFailure_ContinuesProcessing` | Error resilience |
| | `Pipeline_LargeDocument_HandlesMemory` | Memory handling |
| `VectorStoreIntegrationTests` | `Pipeline_ToInMemoryStore_StoresChunks` | In-memory store |
| | `Pipeline_ChunkEmbeddings_Generated` | Embedding generation |
| | `Pipeline_MetadataStored_Correctly` | Metadata in store |
| `EnricherIntegrationTests` | `Pipeline_WithSummaryEnricher_AddsSummaries` | Summary enricher |
| | `Pipeline_WithKeywordEnricher_AddsKeywords` | Keyword enricher |
| | `Pipeline_WithMultipleEnrichers_AppliesAll` | Multiple enrichers |
| `MultiDocumentPipelineTests` | `Pipeline_DirectoryBatch_ProcessesAll` | Batch processing |
| | `Pipeline_MixedFormats_HandlesAll` | Mixed format batch |
| | `Pipeline_PartialErrors_ReportsResults` | Error reporting |

### 8.3 Performance Tests

| Test | Description | Target |
|------|-------------|--------|
| `Benchmark_PdfExtraction_1MB` | PDF extraction speed | < 5s |
| `Benchmark_DocxExtraction_1MB` | Word extraction speed | < 3s |
| `Benchmark_Chunking_10000Atoms` | Chunking throughput | < 1s |
| `Benchmark_FullPipeline_10Documents` | Pipeline throughput | < 30s |
| `Memory_LargeDocument_50MB` | Memory usage | < 500MB peak |

### 8.4 Test Data Requirements

| File | Purpose | Content |
|------|---------|---------|
| `sample.pdf` | PDF tests | Multi-page with text, tables, images |
| `sample.docx` | Word tests | Headers, tables, images, formatting |
| `sample.xlsx` | Excel tests | Multiple sheets, formulas, formatting |
| `sample.pptx` | PowerPoint tests | Multiple slides with content |
| `sample.html` | HTML tests | Semantic HTML with tables, links |
| `sample.md` | Markdown tests | Headers, lists, code, tables |
| `sample.json` | JSON tests | Nested structure |
| `sample.csv` | CSV tests | Header row, data rows |
| `sample.xml` | XML tests | Nested elements, attributes |
| `sample.rtf` | RTF tests | Formatted text |
| `sample.png` | Image/OCR tests | Text-containing image |
| `sample.txt` | Text tests | Plain text with paragraphs |
| `large.pdf` | Performance tests | 50+ pages |
| `complex.docx` | Edge cases | Complex formatting, embedded objects |

---

## 9. Documentation Requirements

### 9.1 README.md (Package)

```markdown
# DocumentAtom.DataIngestion

Microsoft.Extensions.DataIngestion integration for DocumentAtom.

## Installation

```bash
dotnet add package DocumentAtom.DataIngestion
```

## Quick Start

```csharp
// Create a simple pipeline
var pipeline = DocumentAtomIngestionFactory.CreateVectorStorePipeline(
    vectorStore,
    embeddingGenerator,
    dimensionCount: 1536);

// Process documents
await foreach (var result in pipeline.ProcessAsync(
    new DirectoryInfo("./documents"),
    "*.pdf"))
{
    Console.WriteLine($"{result.DocumentId}: {result.Succeeded}");
}
```

## Features

- Full document type support (PDF, Word, Excel, HTML, etc.)
- Semantic chunking using Atom boundaries
- Pre-computed Quarks for optimal chunk sizes
- Full metadata preservation
- Hierarchy-aware chunking
- Integration with Microsoft AI enrichers
```

### 9.2 API Documentation

- XML documentation comments on all public types and members
- IntelliSense-friendly summaries
- Parameter documentation
- Exception documentation
- Code examples in remarks

### 9.3 Samples

| Sample | Description |
|--------|-------------|
| `BasicPipeline` | Simple document-to-vector-store pipeline |
| `CustomChunking` | Custom chunking configuration |
| `WithEnrichers` | Using AI enrichers |
| `BatchProcessing` | Processing multiple documents |
| `DependencyInjection` | ASP.NET Core integration |
| `HierarchyAware` | Using hierarchy-aware chunking |

---

## 10. Migration Guide

### For Existing DocumentAtom Users

```csharp
// Before: Direct DocumentAtom usage
var processor = new PdfProcessor(settings);
var atoms = processor.Extract("document.pdf").ToList();

// Process atoms manually...
foreach (var atom in atoms)
{
    var embedding = await GenerateEmbedding(atom.Text);
    await vectorStore.Insert(atom.GUID, embedding, atom);
}

// After: With DataIngestion
var pipeline = DocumentAtomIngestionFactory.CreateVectorStorePipeline(
    vectorStore, embeddingGenerator, 1536,
    new AtomDocumentReaderOptions
    {
        ProcessorSettings = { [DocumentTypeEnum.Pdf] = settings }
    });

await foreach (var result in pipeline.ProcessAsync("document.pdf"))
{
    // Automatic extraction, chunking, embedding, storage
}
```

### Key Differences

| Aspect | Direct API | DataIngestion API |
|--------|------------|-------------------|
| Document reading | Manual processor selection | Automatic type detection |
| Chunking | Manual or via Quarks | Configurable via AtomChunker |
| Embeddings | Manual | Automatic via writer |
| Storage | Manual | VectorStoreWriter |
| Enrichment | N/A | Built-in enrichers |
| Error handling | Manual | Pipeline handles |

---

## 11. Implementation Phases

### Phase 1: Core Infrastructure (Week 1-2)

| Task | Description | Deliverable |
|------|-------------|-------------|
| 1.1 | Create project structure | `DocumentAtom.DataIngestion.csproj` |
| 1.2 | Implement `AtomDocumentReader` | Reader with type detection |
| 1.3 | Implement `AtomToIngestionElementConverter` | Type mapping |
| 1.4 | Implement `MetadataMapper` | Metadata preservation |
| 1.5 | Unit tests for reader | Reader test coverage |

### Phase 2: Chunking (Week 3-4)

| Task | Description | Deliverable |
|------|-------------|-------------|
| 2.1 | Implement `AtomChunker` | Basic chunker |
| 2.2 | Implement quark-based chunking | Quark support |
| 2.3 | Implement token-based fallback | Token splitting |
| 2.4 | Implement `HierarchyAwareChunker` | Section grouping |
| 2.5 | Unit tests for chunkers | Chunker test coverage |

### Phase 3: Integration (Week 5-6)

| Task | Description | Deliverable |
|------|-------------|-------------|
| 3.1 | Implement `AtomDocumentProcessor` | Document processing |
| 3.2 | Implement factory methods | Convenience factories |
| 3.3 | Implement DI extensions | Service registration |
| 3.4 | Integration tests | End-to-end tests |
| 3.5 | Performance tests | Benchmarks |

### Phase 4: Documentation & Release (Week 7-8)

| Task | Description | Deliverable |
|------|-------------|-------------|
| 4.1 | Write README | Package documentation |
| 4.2 | Write API docs | XML comments |
| 4.3 | Create samples | Sample projects |
| 4.4 | Write migration guide | User documentation |
| 4.5 | Package and publish | NuGet package |

---

## 12. Risk Assessment

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| DataIngestion API changes before GA | High | Medium | Isolate adapter code, version pin, monitor releases |
| Performance regression vs direct API | Medium | Medium | Benchmark early, optimize converters |
| Metadata loss during conversion | Low | High | Comprehensive round-trip tests |
| Memory issues with large documents | Medium | High | Streaming where possible, document limits |
| Type detection failures | Low | Medium | Fallback to text processor, logging |
| OCR dependency issues | Medium | Low | Make OCR optional, graceful degradation |

---

## 13. Future Considerations

### Potential Enhancements

| Enhancement | Description | Priority |
|-------------|-------------|----------|
| Async streaming | Stream atoms without full materialization | Medium |
| Custom enrichers | DocumentAtom-specific enrichers | Low |
| Caching | Cache parsed documents | Low |
| Parallel processing | Multi-threaded document processing | Medium |
| Progress reporting | IProgress<T> support | Low |

### Ecosystem Integration

| Integration | Description |
|-------------|-------------|
| Semantic Kernel | SK document loader |
| LangChain.NET | LangChain document loader |
| Azure AI Search | Direct Azure Search integration |
| OpenAI Assistants | File upload support |

### Version Roadmap

| Version | Features |
|---------|----------|
| 1.0.0 | Core reader, chunker, basic factory |
| 1.1.0 | Hierarchy-aware chunking, DI extensions |
| 1.2.0 | Performance optimizations, streaming |
| 2.0.0 | Breaking changes for DataIngestion GA alignment |

---

## Appendix A: Complete Type Mapping Reference

### AtomTypeEnum → IngestionDocumentElement

```csharp
public static IngestionDocumentElement? ConvertAtomType(Atom atom)
{
    return atom.Type switch
    {
        AtomTypeEnum.Text => new IngestionDocumentParagraph
        {
            Content = atom.Text
        },

        AtomTypeEnum.List => new IngestionDocumentParagraph
        {
            Content = atom.OrderedList?.Count > 0
                ? string.Join("\n", atom.OrderedList.Select((x, i) => $"{i + 1}. {x}"))
                : string.Join("\n", atom.UnorderedList?.Select(x => $"- {x}") ?? [])
        },

        AtomTypeEnum.Table => new IngestionDocumentTable
        {
            // Map SerializableDataTable to table structure
            Rows = MapTableRows(atom.Table),
            Columns = atom.Table?.Columns?.ToList()
        },

        AtomTypeEnum.Image => new IngestionDocumentImage
        {
            Data = atom.Binary,
            AlternativeText = atom.Text,
            Width = (int?)(atom.BoundingBox?.Width),
            Height = (int?)(atom.BoundingBox?.Height)
        },

        AtomTypeEnum.Hyperlink => new IngestionDocumentParagraph
        {
            Content = $"[{atom.Title ?? atom.Text}]({atom.Text})"
        },

        AtomTypeEnum.Code => new IngestionDocumentParagraph
        {
            Content = $"```\n{atom.Text}\n```"
        },

        AtomTypeEnum.Binary => null, // Skip or base64 encode based on config

        AtomTypeEnum.Meta => null, // Metadata only, no element

        AtomTypeEnum.Unknown => new IngestionDocumentParagraph
        {
            Content = atom.Text ?? string.Empty
        },

        _ => null
    };
}
```

### Header Detection

```csharp
public static bool IsHeader(Atom atom, out int level)
{
    level = 0;

    if (atom.HeaderLevel.HasValue && atom.HeaderLevel.Value > 0)
    {
        level = atom.HeaderLevel.Value;
        return true;
    }

    if (atom.Formatting == MarkdownFormattingEnum.Header)
    {
        // Infer level from content
        var text = atom.Text ?? string.Empty;
        var match = Regex.Match(text, @"^(#{1,6})\s");
        if (match.Success)
        {
            level = match.Groups[1].Value.Length;
            return true;
        }
        level = 1; // Default
        return true;
    }

    return false;
}
```

---

## Appendix B: Sample Integration Code

### Complete RAG Pipeline Example

```csharp
using DocumentAtom.DataIngestion;
using Microsoft.Extensions.DataIngestion;
using Microsoft.Extensions.VectorData;
using Microsoft.Extensions.AI;
using OpenAI;

// Setup clients
var openAIClient = new OpenAIClient(Environment.GetEnvironmentVariable("OPENAI_API_KEY"));
var embeddingClient = openAIClient.GetEmbeddingClient("text-embedding-3-small");
var chatClient = openAIClient.GetChatClient("gpt-4");

// Setup vector store (example: Qdrant)
var qdrantClient = new QdrantClient("localhost", 6334);
var vectorStore = new QdrantVectorStore(qdrantClient);

// Create DocumentAtom pipeline
using var pipeline = DocumentAtomIngestionFactory.CreateVectorStorePipeline(
    vectorStore,
    embeddingClient.AsIEmbeddingGenerator(),
    dimensionCount: 1536,
    readerOptions: new AtomDocumentReaderOptions
    {
        EnableOcr = true,
        BuildHierarchy = true,
        ChunkingSettings = new ChunkingSettings
        {
            Enable = true,
            MaximumLength = 1024,
            ShiftSize = 512
        }
    },
    chunkerOptions: new AtomChunkerOptions
    {
        UseQuarks = true,
        IncludeHeaderContext = true
    },
    maxTokensPerChunk: 500
);

// Add enrichers
pipeline.ChunkProcessors.Add(new SummaryEnricher(chatClient.AsIChatClient()));
pipeline.ChunkProcessors.Add(new KeywordEnricher(chatClient.AsIChatClient()));

// Process documents
var documentsPath = new DirectoryInfo("./documents");
var results = new List<IngestionResult>();

await foreach (var result in pipeline.ProcessAsync(documentsPath, "*.pdf"))
{
    results.Add(result);

    if (result.Succeeded)
    {
        Console.WriteLine($"✓ {result.DocumentId}");
    }
    else
    {
        Console.WriteLine($"✗ {result.DocumentId}: {result.Error}");
    }
}

Console.WriteLine($"\nProcessed {results.Count} documents");
Console.WriteLine($"Succeeded: {results.Count(r => r.Succeeded)}");
Console.WriteLine($"Failed: {results.Count(r => !r.Succeeded)}");
```

---

*Document Version: 1.0*
*Last Updated: 2026-01-21*
*Author: DocumentAtom Team*
