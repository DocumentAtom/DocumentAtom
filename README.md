<img src="https://github.com/jchristn/DocumentAtom/blob/main/assets/icon.png" width="256" height="256">

# DocumentAtom

[![NuGet Version](https://img.shields.io/nuget/v/DocumentAtom.svg?style=flat)](https://www.nuget.org/packages/DocumentAtom/) [![NuGet](https://img.shields.io/nuget/dt/DocumentAtom.svg)](https://www.nuget.org/packages/DocumentAtom) 

DocumentAtom provides a light, fast library for breaking input documents into constituent parts (atoms), useful for text processing and analysis.

## New in v1.0.x

- Initial release

## Bugs, Feedback, or Enhancement Requests

Please feel free to start an issue or a discussion!

## Types Supported

DocumentAtom supports the following input file types:
- Text
- Markdown
- Microsoft Word (.docx)
- Microsoft Excel (.xlsx)
- Microsoft PowerPoint (.pptx)
- PNG images
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

## Version History

Please refer to ```CHANGELOG.md``` for version history.

## Thanks

Special thanks to iconduck.com and the content authors for producing this [icon](https://iconduck.com/icons/27054/atom).
