namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Word;
    using Touchstone.Core;
    using UglyToad.PdfPig.Core;
    using UglyToad.PdfPig.Fonts.Standard14Fonts;
    using UglyToad.PdfPig.Writer;

    /// <summary>
    /// Suite covering the binary document processors. Because the Office/PDF/image formats require
    /// native fixtures, these cases exercise construction and input-validation contracts that are
    /// deterministic in any environment, plus a positive extraction for the managed RTF parser.
    /// </summary>
    internal static class DocumentProcessorSuites
    {
        /// <summary>
        /// Builds a minimal single-page PDF containing one line of text using PdfPig's own writer.
        /// This gives the PDF extraction path a deterministic fixture without checking a binary into
        /// source control, and exercises the same PdfPig version the processor reads with.
        /// </summary>
        private static byte[] BuildTextPdf(string text)
        {
            PdfDocumentBuilder builder = new PdfDocumentBuilder();
            PdfDocumentBuilder.AddedFont font = builder.AddStandard14Font(Standard14Font.Helvetica);
            PdfPageBuilder page = builder.AddPage(595, 842);
            page.AddText(text, 12, new PdfPoint(50, 750), font);
            return builder.Build();
        }

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
                .Case("Pdf.MissingFile", "The PDF processor throws for a missing file on enumeration", () =>
                {
                    // PdfPig surfaces a missing file as InvalidOperationException ("No file exists at ..."),
                    // unlike the text-family processors which throw FileNotFoundException.
                    using PdfProcessor p = new PdfProcessor();
                    Check.Throws<InvalidOperationException>(() => p.Extract(Workspace.NonExistentPath("pdf")).ToList());
                })
                .Case("Pdf.Extract", "The PDF processor extracts a text atom with a bounding box from a generated document", () =>
                {
                    string path = Workspace.WriteBytes("pdf", BuildTextPdf("Hello DocumentAtom PDF extraction."));
                    using PdfProcessor p = new PdfProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);

                    Atom? textAtom = atoms.FirstOrDefault(a => a.Type == AtomTypeEnum.Text && !string.IsNullOrEmpty(a.Text));
                    Check.NotNull(textAtom);
                    Check.Contains("DocumentAtom", textAtom!.Text);
                    Check.NotNull(textAtom.BoundingBox);
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
