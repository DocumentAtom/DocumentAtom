# Migration Guide: v1.x to v2.0

This guide helps you migrate from DocumentAtom v1.x (17+ packages) to v2.0 (4 consolidated packages).

## Overview

DocumentAtom v2.0 consolidates the library from 17+ individual packages into 4 main packages:

| v2.0 Package | Description |
|--------------|-------------|
| `DocumentAtom.Core` | Core classes, TextTools (merged) |
| `DocumentAtom.Text` | Text-based processors (Text, CSV, JSON, XML, HTML, Markdown, TypeDetection) |
| `DocumentAtom.Documents` | Binary document processors (Word, Excel, PowerPoint, PDF, Image, OCR, RichText) |
| `DocumentAtom` | Metapackage that references all above packages |
| `DocumentAtom.DataIngestion` | Microsoft.Extensions.AI integration (kept separate) |

## Package Changes

### Packages Merged into DocumentAtom.Core

| Old Package | New Location |
|-------------|--------------|
| `DocumentAtom.TextTools` | `DocumentAtom.Core.TextTools` namespace |

### Packages Merged into DocumentAtom.Text

| Old Package | New Namespace |
|-------------|---------------|
| `DocumentAtom.Text` | `DocumentAtom.Text` (unchanged) |
| `DocumentAtom.Csv` | `DocumentAtom.Text.Csv` |
| `DocumentAtom.Json` | `DocumentAtom.Text.Json` |
| `DocumentAtom.Xml` | `DocumentAtom.Text.Xml` |
| `DocumentAtom.Html` | `DocumentAtom.Text.Html` |
| `DocumentAtom.Markdown` | `DocumentAtom.Text.Markdown` |
| `DocumentAtom.TypeDetection` | `DocumentAtom.Text.TypeDetection` |

### Packages Merged into DocumentAtom.Documents

| Old Package | New Namespace |
|-------------|---------------|
| `DocumentAtom.Ocr` | `DocumentAtom.Documents.Ocr` |
| `DocumentAtom.Image` | `DocumentAtom.Documents.Image` |
| `DocumentAtom.Excel` | `DocumentAtom.Documents.Excel` |
| `DocumentAtom.Word` | `DocumentAtom.Documents.Word` |
| `DocumentAtom.PowerPoint` | `DocumentAtom.Documents.PowerPoint` |
| `DocumentAtom.Pdf` | `DocumentAtom.Documents.Pdf` |
| `DocumentAtom.RichText` | `DocumentAtom.Documents.RichText` |

## Migration Steps

### 1. Update Package References

**Old (.csproj):**
```xml
<ItemGroup>
  <PackageReference Include="DocumentAtom.Csv" Version="1.x.x" />
  <PackageReference Include="DocumentAtom.Word" Version="1.x.x" />
  <PackageReference Include="DocumentAtom.Pdf" Version="1.x.x" />
  <PackageReference Include="DocumentAtom.TextTools" Version="1.x.x" />
</ItemGroup>
```

**New (.csproj):**
```xml
<ItemGroup>
  <!-- Option 1: Reference individual packages -->
  <PackageReference Include="DocumentAtom.Core" Version="2.0.0" />
  <PackageReference Include="DocumentAtom.Text" Version="2.0.0" />
  <PackageReference Include="DocumentAtom.Documents" Version="2.0.0" />

  <!-- Option 2: Use the metapackage (includes all) -->
  <PackageReference Include="DocumentAtom" Version="2.0.0" />
</ItemGroup>
```

### 2. Update Using Statements

Replace the old using statements with the new namespaces:

```csharp
// OLD
using DocumentAtom.Csv;
using DocumentAtom.Word;
using DocumentAtom.Pdf;
using DocumentAtom.TextTools;
using DocumentAtom.TypeDetection;

// NEW
using DocumentAtom.Text.Csv;
using DocumentAtom.Documents.Word;
using DocumentAtom.Documents.Pdf;
using DocumentAtom.Core.TextTools;
using DocumentAtom.Text.TypeDetection;
```

### 3. Complete Namespace Mapping

Here's the full mapping of old to new namespaces:

```csharp
// Text processors
using DocumentAtom.Csv;           // -> using DocumentAtom.Text.Csv;
using DocumentAtom.Json;          // -> using DocumentAtom.Text.Json;
using DocumentAtom.Xml;           // -> using DocumentAtom.Text.Xml;
using DocumentAtom.Html;          // -> using DocumentAtom.Text.Html;
using DocumentAtom.Markdown;      // -> using DocumentAtom.Text.Markdown;
using DocumentAtom.TypeDetection; // -> using DocumentAtom.Text.TypeDetection;

// Document processors
using DocumentAtom.Word;          // -> using DocumentAtom.Documents.Word;
using DocumentAtom.Excel;         // -> using DocumentAtom.Documents.Excel;
using DocumentAtom.PowerPoint;    // -> using DocumentAtom.Documents.PowerPoint;
using DocumentAtom.Pdf;           // -> using DocumentAtom.Documents.Pdf;
using DocumentAtom.Image;         // -> using DocumentAtom.Documents.Image;
using DocumentAtom.Ocr;           // -> using DocumentAtom.Documents.Ocr;
using DocumentAtom.RichText;      // -> using DocumentAtom.Documents.RichText;

// Text tools (merged into Core)
using DocumentAtom.TextTools;     // -> using DocumentAtom.Core.TextTools;
```

## Code Examples

### Processing a CSV File (v1.x vs v2.0)

**v1.x:**
```csharp
using DocumentAtom.Csv;

var processor = new CsvProcessor(new CsvProcessorSettings());
var atoms = processor.Extract("data.csv");
```

**v2.0:**
```csharp
using DocumentAtom.Text.Csv;

var processor = new CsvProcessor(new CsvProcessorSettings());
var atoms = processor.Extract("data.csv");
```

### Processing a Word Document (v1.x vs v2.0)

**v1.x:**
```csharp
using DocumentAtom.Word;

var processor = new DocxProcessor(new DocxProcessorSettings());
var atoms = processor.Extract("document.docx");
```

**v2.0:**
```csharp
using DocumentAtom.Documents.Word;

var processor = new DocxProcessor(new DocxProcessorSettings());
var atoms = processor.Extract("document.docx");
```

### Using Text Tools (v1.x vs v2.0)

**v1.x:**
```csharp
using DocumentAtom.TextTools;

var lemmatizer = new Lemmatizer();
var tokenExtractor = new TokenExtractor();
```

**v2.0:**
```csharp
using DocumentAtom.Core.TextTools;

var lemmatizer = new Lemmatizer();
var tokenExtractor = new TokenExtractor();
```

## Breaking Changes

1. **Namespace Changes**: All processor namespaces have changed. See the mapping table above.

2. **Package References**: Individual packages (e.g., `DocumentAtom.Csv`) are no longer published. Use `DocumentAtom.Text` or the metapackage instead.

3. **TextTools Location**: The `DocumentAtom.TextTools` namespace is now `DocumentAtom.Core.TextTools`.

## Benefits of v2.0

- **Simpler Dependencies**: Only 4 packages to reference instead of 17+
- **Unified Versioning**: All packages share the same version number
- **Smaller Footprint**: Single native DLL location for Tesseract
- **Consistent API**: All processors follow the same patterns
- **Monorepo SDKs**: TypeScript, Python, and C# SDKs are now in the main repository

## Getting Help

If you encounter issues during migration:
- Check the [GitHub Issues](https://github.com/jchristn/DocumentAtom/issues)
- Review the [README.md](README.md) for updated examples
- Open a new issue if you find a bug
