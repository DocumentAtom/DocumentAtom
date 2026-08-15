namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Text;
    using DocumentAtom.Core.TypeDetection;
    using Touchstone.Core;

    /// <summary>
    /// Suite covering <see cref="TypeDetector"/> signature- and content-based document type detection.
    /// </summary>
    internal static class TypeDetectionSuites
    {
        internal static TestSuiteDescriptor Build()
        {
            return new SuiteBuilder("Core.TypeDetection")
                .Case("TypeResult.Defaults", "A new TypeResult defaults to Unknown", () =>
                {
                    TypeResult tr = new TypeResult();
                    Check.Equal(DocumentTypeEnum.Unknown, tr.Type);
                })
                .Case("Png", "PNG signature bytes are detected as Png", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(SampleData.PngBytes());
                    Check.Equal(DocumentTypeEnum.Png, tr.Type);
                    Check.Equal("image/png", tr.MimeType);
                })
                .Case("Pdf", "PDF signature bytes are detected as Pdf", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(SampleData.PdfBytes());
                    Check.Equal(DocumentTypeEnum.Pdf, tr.Type);
                })
                .Case("Gif", "GIF signature bytes are detected as Gif", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(SampleData.GifBytes());
                    Check.Equal(DocumentTypeEnum.Gif, tr.Type);
                })
                .Case("PlainText", "Printable text is detected as Text", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(SampleData.TextBytes());
                    Check.Equal(DocumentTypeEnum.Text, tr.Type);
                })
                .Case("Json", "A JSON payload is detected as Json", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(Encoding.UTF8.GetBytes(SampleData.Json));
                    Check.Equal(DocumentTypeEnum.Json, tr.Type);
                })
                .Case("Xml", "An XML payload is detected as Xml", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(Encoding.UTF8.GetBytes(SampleData.Xml));
                    Check.Equal(DocumentTypeEnum.Xml, tr.Type);
                })
                .Case("Html", "An HTML payload is detected as Html", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(Encoding.UTF8.GetBytes(SampleData.Html));
                    Check.Equal(DocumentTypeEnum.Html, tr.Type);
                })
                .Case("Csv.ByContentType", "A CSV payload is detected as Csv when the content type is supplied", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    TypeResult tr = d.Process(Encoding.UTF8.GetBytes(SampleData.Csv), "text/csv");
                    Check.Equal(DocumentTypeEnum.Csv, tr.Type);
                })
                .Case("Validation.NullBytes", "Null bytes throw ArgumentException", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    Check.Throws<ArgumentException>(() => d.Process((byte[])null!));
                })
                .Case("Validation.EmptyBytes", "Empty bytes throw ArgumentException", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    Check.Throws<ArgumentException>(() => d.Process(Array.Empty<byte>()));
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using TypeDetector d = new TypeDetector();
                    Check.Throws<ArgumentNullException>(() => d.Process((string)null!));
                })
                .Build("Core: TypeDetection");
        }
    }
}
