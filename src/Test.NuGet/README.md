# Test.NuGet - DocumentAtom Package Tester

This console application provides a comprehensive test harness for the DocumentAtom metapackage. It allows you to interactively test all document processors and type detection functionality.

## Purpose

This project serves two main purposes:

1. **Local Development Testing**: Test changes to DocumentAtom packages during development using project references
2. **Published Package Testing**: Verify published NuGet packages work correctly after release

## Features

- Interactive console menu for easy testing
- Type detection for unknown files
- Document atomization for all supported formats:
  - **Text formats**: Plain text, CSV, JSON, XML, HTML, Markdown
  - **Office documents**: Word (.docx), Excel (.xlsx), PowerPoint (.pptx)
  - **Other formats**: PDF, RTF, Images (PNG/JPG), OCR
- JSON serialization of results for easy inspection
- Logging output from processors

## Usage

### Building and Running

```bash
cd src/Test.NuGet
dotnet build
dotnet run
```

### Interactive Menu

Once running, you'll see a menu:

```
DocumentAtom NuGet Package Test Application

Command [? for menu] >
```

Type `?` to see all available commands:

**General:**
- **?** (or **help**, **menu**) - Display menu
- **cls** - Clear console
- **q** (or **quit**, **exit**) - Quit

**Type Detection:**
- **td** (or **detect**, **type**) - Detect file type

**Text Processors:**
- **text** (or **txt**) - Plain text
- **csv** - CSV files
- **json** - JSON files
- **xml** - XML files
- **html** (or **htm**) - HTML files
- **markdown** (or **md**) - Markdown files

**Document Processors:**
- **word** (or **docx**) - Word documents
- **excel** (or **xlsx**) - Excel spreadsheets
- **powerpoint** (or **pptx**, **ppt**) - PowerPoint presentations
- **pdf** - PDF documents
- **rtf** - Rich Text Format
- **image** (or **img**, **png**, **jpg**, **jpeg**) - Images
- **ocr** - OCR extraction

### Example Session

```
Command [? for menu] > td
=== Type Detection ===
File path > sample.pdf
Result:
{
  "MimeType": "application/pdf",
  "Extension": "pdf"
}

Command [? for menu] > pdf
=== PDF Processor ===
File path > sample.pdf
Atoms: 42
Results:
[
  {
    "Type": "Text",
    "Text": "Document content here...",
    ...
  }
]

Command [? for menu] > excel
=== Excel Processor ===
File path > data.xlsx
Atoms: 15
Results:
[
  {
    "Type": "Table",
    "Table": { ... },
    ...
  }
]
```

## Switching Between Local and NuGet Package Testing

### For Local Development (Default)

The project is pre-configured with `ProjectReference` to test local changes:

```xml
<ProjectReference Include="..\DocumentAtom\DocumentAtom.csproj" />
```

### For Testing Published NuGet Packages

After publishing to NuGet, edit `Test.NuGet.csproj`:

1. **Comment out** the ProjectReference:
   ```xml
   <!-- <ProjectReference Include="..\DocumentAtom\DocumentAtom.csproj" /> -->
   ```

2. **Uncomment** the PackageReference and update version:
   ```xml
   <PackageReference Include="DocumentAtom" Version="2.0.0" />
   ```

3. Clear NuGet cache and restore:
   ```bash
   dotnet nuget locals all --clear
   dotnet restore
   dotnet build
   ```

## OCR Testing

OCR functionality requires Tesseract data files. The application will automatically search for a `tessdata` directory in common locations:

- `./tessdata`
- `../tessdata`
- In the application base directory

If tessdata is not found, OCR testing will display an error message.

## Output Format

All results are serialized to JSON for easy inspection. For atom processors, you'll see:

- Count of atoms extracted
- Full JSON representation of each atom including:
  - Type (Text, Table, List, etc.)
  - Content
  - Metadata
  - Hierarchy information (if applicable)

## Tips

- Use the type detection (option 1) first if you're unsure of a file's format
- Check the processor log messages for debugging information
- Results can be copied from the console for further analysis
- Use `cls` command to clear the console between tests

## Supported File Types

- **Plain Text**: .txt
- **CSV**: .csv
- **JSON**: .json
- **XML**: .xml
- **HTML**: .html, .htm
- **Markdown**: .md
- **Word**: .docx
- **Excel**: .xlsx
- **PowerPoint**: .pptx
- **PDF**: .pdf
- **RTF**: .rtf
- **Images**: .png, .jpg, .jpeg (with OCR)

## Troubleshooting

### Build Errors

If you encounter build errors:
1. Ensure all dependencies are restored: `dotnet restore`
2. Clean and rebuild: `dotnet clean && dotnet build`

### File Not Found

Ensure you provide the full or relative path to test files. The application validates file existence before processing.

### OCR Errors

If OCR testing fails:
1. Verify tessdata directory exists and contains language files (eng.traineddata, etc.)
2. Check that you have the correct Tesseract binaries for your platform

## Architecture

The project demonstrates proper usage of the DocumentAtom metapackage:

- References the `DocumentAtom` metapackage which includes:
  - `DocumentAtom.Core` - Base classes and type detection
  - `DocumentAtom.Text` - Text format processors
  - `DocumentAtom.Documents` - Binary document processors
- Uses `SerializationHelper` for JSON output
- Follows DocumentAtom coding standards and patterns
