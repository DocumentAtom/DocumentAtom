<img src="https://raw.githubusercontent.com/jchristn/DocumentAtom/refs/heads/main/assets/icon.png" width="256" height="256">

# DocumentAtom

DocumentAtom provides a light, fast library for breaking input documents into constituent parts (atoms), useful for text processing, analysis, and artificial intelligence.

DocumentAtom requires that Tesseract v5.0 be installed on the host.  This is required as certain document types can have embedded images which are parsed using OCR via Tesseract.

| Package | Version | Downloads |
|---------|---------|-----------|
| DocumentAtom.Csv | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Csv.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Csv/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Csv.svg)](https://www.nuget.org/packages/DocumentAtom.Csv)  |
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

## New in v1.1.x

- Hierarchical atomization (see `BuildHierarchy` in settings) - heading-based for markdown/HTML/Word, page-based for PowerPoint
- Support for CSV, JSON, and XML documents
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

## RESTful API and Docker

Run the `DocumentAtom.Server` project to start a RESTful server listening on `localhost:8000`.  Modify the `documentatom.json` file to change the webserver, logging, or Tesseract settings.  Alternatively, you can pull `jchristn/documentatom` from [Docker Hub](https://hub.docker.com/repository/docker/jchristn/documentatom/general).  Refer to the `Docker` directory in the project for assets for running in Docker.

Refer to the Postman collection for examples exercising the APIs.

## Version History

Please refer to ```CHANGELOG.md``` for version history.

## Thanks

Special thanks to iconduck.com and the content authors for producing this [icon](https://iconduck.com/icons/27054/atom).
