namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Word;
    using Touchstone.Core;

    /// <summary>
    /// Suite covering the binary document processors. Because the Office/PDF/image formats require
    /// native fixtures, these cases exercise construction and input-validation contracts that are
    /// deterministic in any environment, plus a positive extraction for the managed RTF parser.
    /// </summary>
    internal static class DocumentProcessorSuites
    {
        internal static TestSuiteDescriptor Build()
        {
            return new SuiteBuilder("Documents.Processors")
                .Case("Docx.Construct", "The Word processor constructs with default settings", () =>
                {
                    using DocxProcessor p = new DocxProcessor();
                    Check.NotNull(p);
                })
                .Case("Docx.NullFilename", "The Word processor rejects a null filename", () =>
                {
                    using DocxProcessor p = new DocxProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Docx.EmptyBytes", "The Word processor yields nothing for empty bytes", () =>
                {
                    using DocxProcessor p = new DocxProcessor();
                    Check.Empty(p.Extract(Array.Empty<byte>()).ToList());
                })
                .Case("Xlsx.Construct", "The Excel processor constructs with default settings", () =>
                {
                    using XlsxProcessor p = new XlsxProcessor();
                    Check.NotNull(p);
                })
                .Case("Xlsx.NullFilename", "The Excel processor rejects a null filename", () =>
                {
                    using XlsxProcessor p = new XlsxProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Pptx.Construct", "The PowerPoint processor constructs with default settings", () =>
                {
                    using PptxProcessor p = new PptxProcessor();
                    Check.NotNull(p);
                })
                .Case("Pptx.NullFilename", "The PowerPoint processor rejects a null filename", () =>
                {
                    using PptxProcessor p = new PptxProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Pdf.Construct", "The PDF processor constructs with default settings", () =>
                {
                    using PdfProcessor p = new PdfProcessor();
                    Check.NotNull(p);
                })
                .Case("Pdf.NullFilename", "The PDF processor rejects a null filename", () =>
                {
                    using PdfProcessor p = new PdfProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Image.Construct", "The image processor constructs with default settings", () =>
                {
                    using ImageProcessor p = new ImageProcessor();
                    Check.NotNull(p);
                })
                .Case("Image.NullFilename", "The image processor rejects a null filename", () =>
                {
                    using ImageProcessor p = new ImageProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Rtf.Construct", "The RTF processor constructs with default settings", () =>
                {
                    using RtfProcessor p = new RtfProcessor();
                    Check.NotNull(p);
                })
                .Case("Rtf.NullFilename", "The RTF processor rejects a null filename", () =>
                {
                    using RtfProcessor p = new RtfProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Rtf.Extract", "The RTF processor extracts text atoms from a valid document", () =>
                {
                    string path = Workspace.WriteText("rtf", SampleData.Rtf);
                    using RtfProcessor p = new RtfProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                    Check.True(atoms.Any(a => !string.IsNullOrEmpty(a.Text)), "Expected at least one atom with text.");
                })
                .Build("Documents: Processors");
        }
    }
}
