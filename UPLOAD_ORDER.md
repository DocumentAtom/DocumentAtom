# NuGet Package Upload Order

This document describes the correct order for uploading DocumentAtom packages to NuGet.

## Why Order Matters

NuGet validates package dependencies during upload. If you attempt to upload a package that references another DocumentAtom package that doesn't exist on NuGet yet, the upload will fail validation. Each package must be able to resolve all its dependencies at upload time.

## Dependency Graph

```
DocumentAtom.Core (no dependencies)
├── DocumentAtom.Text
├── DocumentAtom.Documents
├── DocumentAtom.Sdk
├── DocumentAtom (metapackage)
└── DocumentAtom.DataIngestion
```

## Upload Sequence

### Step 1: Upload Core Package

Upload the foundation package with no dependencies:

- `DocumentAtom.Core`

**Wait 5-15 minutes** for NuGet indexing to complete before proceeding to Step 2.

### Step 2: Upload Text, Document Processors, and SDK

Upload packages that depend only on Core:

- `DocumentAtom.Text`
- `DocumentAtom.Documents`
- `DocumentAtom.Sdk`

**Wait 5-15 minutes** for NuGet indexing to complete before proceeding to Step 3.

### Step 3: Upload Metapackage and Data Ingestion

Upload packages that depend on Core, Text, and Documents:

- `DocumentAtom` (metapackage)
- `DocumentAtom.DataIngestion`

## Verification

After uploading each batch, verify packages are indexed and available before proceeding:

### Option 1: Search on NuGet.org
Visit https://www.nuget.org/packages and search for the package name.

### Option 2: Test Installation
```bash
dotnet add package DocumentAtom.Core --version <your-version>
```

### Option 3: Use NuGet CLI
```bash
nuget list DocumentAtom.Core
```

## Timeline

Total time for complete upload sequence: **10-30 minutes**

- Step 1: Upload + 5-15 min wait
- Step 2: Upload + 5-15 min wait
- Step 3: Upload

## Package Dependencies Reference

### DocumentAtom.Core
- **Dependencies:** None (only external NuGet packages)
- **Package References:** SerializableDataTable, SerializationHelper

### DocumentAtom.Text
- **Dependencies:** DocumentAtom.Core
- **Package References:** CsvHelper, HtmlAgilityPack

### DocumentAtom.Documents
- **Dependencies:** DocumentAtom.Core
- **Package References:** Tesseract, SixLabors.ImageSharp, DocumentFormat.OpenXml, PdfPig, Tabula

### DocumentAtom.DataIngestion
- **Dependencies:** DocumentAtom.Core, DocumentAtom.Text, DocumentAtom.Documents
- **Package References:** Microsoft.Extensions.AI packages

### DocumentAtom (Metapackage)
- **Dependencies:** DocumentAtom.Core, DocumentAtom.Text, DocumentAtom.Documents
- **Package References:** None (metapackage)

### DocumentAtom.Sdk
- **Dependencies:** DocumentAtom.Core
- **Package References:** RestWrapper, System.Text.Json

## Important Notes

1. **Do not rush** - Wait for each tier to be fully indexed before proceeding
2. **Verify availability** - Always check that packages are searchable before uploading dependent packages
3. **Version consistency** - Ensure all packages use the same version number for the release
4. **Indexing delays** - NuGet indexing typically takes 5-15 minutes but can occasionally take longer
5. **Validation failures** - If upload fails due to missing dependencies, wait longer for previous packages to index

## Troubleshooting

### "Package dependencies could not be resolved" error
- **Cause:** Dependent package not yet indexed on NuGet
- **Solution:** Wait longer for previous tier to complete indexing

### Package not appearing in search
- **Cause:** Indexing in progress
- **Solution:** Wait 5-15 more minutes and try again

### Version conflict errors
- **Cause:** Trying to upload different versions in same batch
- **Solution:** Ensure all packages have consistent version numbers
