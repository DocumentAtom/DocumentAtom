<img src="https://github.com/jchristn/DocumentAtom/blob/main/assets/icon.png" width="256" height="256">

# DocumentAtom

DocumentAtom provides a light, fast library for breaking input documents into constituent parts (atoms), useful for text processing, analysis, and artificial intelligence.

| Package | Version | Downloads |
|---------|---------|-----------|
| DocumentAtom.Excel | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Excel.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Excel/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Excel.svg)](https://www.nuget.org/packages/DocumentAtom.Excel)  |
| DocumentAtom.Image | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Image.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Image/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Image.svg)](https://www.nuget.org/packages/DocumentAtom.Image)  |
| DocumentAtom.Markdown | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Markdown.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Markdown/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Markdown.svg)](https://www.nuget.org/packages/DocumentAtom.Markdown)  |
| DocumentAtom.Pdf | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Pdf.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Pdf/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Pdf.svg)](https://www.nuget.org/packages/DocumentAtom.Pdf)  |
| DocumentAtom.PowerPoint | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.PowerPoint.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.PowerPoint/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.PowerPoint.svg)](https://www.nuget.org/packages/DocumentAtom.PowerPoint)  |
| DocumentAtom.Text | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Text.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Text/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Text.svg)](https://www.nuget.org/packages/DocumentAtom.Text)  |
| DocumentAtom.Word | [![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.Word.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom.Word/) | [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.Word.svg)](https://www.nuget.org/packages/DocumentAtom.Word)  |

## New in v1.0.x

- Initial release

## Motivation

Parsing documents and extracting constituent parts is one part science and one part black magic.  I make no claims about the accuracy of extraction, but rather, aims for perfection and hopefully fails to excellence.  If you find ways to improve processing and extraction in any way, we would love your feedback so we can make this library more accurate, faster, and overall better.  My goal in building this library is to make it easier to analyze input data assets and make them more consumable by other systems including analytics and artificial intelligence.

## Bugs, Quality, Feedback, or Enhancement Requests

Please feel free to file issues, enhancement requests, or start discussions about use of the library, improvements, or fixes.  

## Types Supported

DocumentAtom supports the following input file types:
- Text
- Markdown
- Microsoft Word (.docx)
- Microsoft Excel (.xlsx)
- Microsoft PowerPoint (.pptx)
- PNG images **requires Tesseract on the host**
- PDF

## Simple Example 

Refer to the various `Test` projects for working examples.

The following example shows processing a markdown (`.md`) file.

```csharp
using DocumentAtom.Core.Atoms;
using DocumentAtom.Markdown;

MarkdownProcessorSettings settings = new MarkdownProcessorSettings();
MarkdownProcessor processor = new MarkdownProcessor(_Settings);
foreach (MarkdownAtom atom in processor.Extract(filename))
    Console.WriteLine(atom.ToString());
```

## Atom Types

DocumentAtom parses input data assets into a variety of `Atom` objects.  Each `Atom` includes top-level metadata including:
- `GUID`
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

- [DocumentFormat.OpenXml](https://github.com/dotnet/Open-XML-SDK)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp)
- [Tesseract](https://github.com/charlesw/tesseract/)
- [PdfPig](https://github.com/UglyToad/PdfPig)
- [Tabula](https://github.com/BobLd/tabula-sharp)

Each of these libraries were integrated as NuGet packages, and no source was included or modified from these packages.

My libraries used within DocumentAtom:

- [SerializableDataTable](https://github.com/jchristn/serializabledatatable)
- [SerializationHelper](https://github.com/jchristn/serializationhelper)

## Version History

Please refer to ```CHANGELOG.md``` for version history.

## Thanks

Special thanks to iconduck.com and the content authors for producing this [icon](https://iconduck.com/icons/27054/atom).
