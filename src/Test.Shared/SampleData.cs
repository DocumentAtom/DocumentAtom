namespace DocumentAtom.Testing.Shared
{
    using System.Collections.Generic;
    using System.Text;

    /// <summary>
    /// Deterministic, in-memory sample payloads used across the test suites. Text-based samples
    /// are written to disk on demand via <see cref="Workspace"/>; binary signatures are used for
    /// type-detection tests.
    /// </summary>
    public static class SampleData
    {
        /// <summary>Plain text with paragraph breaks (two paragraphs separated by a blank line).</summary>
        public const string PlainText =
            "The quick brown fox jumps over the lazy dog. It was a bright cold day in April.\r\n\r\n" +
            "Sentences form the second paragraph. Chunking should split these appropriately.";

        /// <summary>A CSV document with a header row and three data rows.</summary>
        public const string Csv =
            "Name,Age,City\r\n" +
            "Alice,30,Seattle\r\n" +
            "Bob,25,Portland\r\n" +
            "Carol,41,Denver\r\n";

        /// <summary>A JSON object with nested structure.</summary>
        public const string Json =
            "{ \"name\": \"Alice\", \"age\": 30, \"address\": { \"city\": \"Seattle\", \"zip\": \"98101\" }, " +
            "\"roles\": [\"admin\", \"user\"] }";

        /// <summary>A JSON array of objects.</summary>
        public const string JsonArray =
            "[ { \"id\": 1, \"name\": \"one\" }, { \"id\": 2, \"name\": \"two\" } ]";

        /// <summary>Malformed JSON (unterminated object).</summary>
        public const string InvalidJson = "{ \"name\": \"Alice\", ";

        /// <summary>A well-formed XML document.</summary>
        public const string Xml =
            "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n" +
            "<catalog>\r\n" +
            "  <book id=\"b1\"><title>First</title><author>Alice</author></book>\r\n" +
            "  <book id=\"b2\"><title>Second</title><author>Bob</author></book>\r\n" +
            "</catalog>";

        /// <summary>Malformed XML (mismatched tags).</summary>
        public const string InvalidXml = "<root><child></root>";

        /// <summary>A Markdown document exercising headers, lists, and a table.</summary>
        public const string Markdown =
            "# Title\r\n\r\n" +
            "Some introductory paragraph text goes here.\r\n\r\n" +
            "## Section\r\n\r\n" +
            "- first item\r\n- second item\r\n- third item\r\n\r\n" +
            "1. step one\r\n2. step two\r\n\r\n" +
            "| Name | Age |\r\n| --- | --- |\r\n| Alice | 30 |\r\n| Bob | 25 |\r\n";

        /// <summary>An HTML document exercising headings, paragraphs, lists, tables, links, images, and code.</summary>
        public const string Html =
            "<!DOCTYPE html><html><head><title>Doc</title></head><body>" +
            "<h1>Heading One</h1>" +
            "<p>A paragraph of text.</p>" +
            "<ul><li>alpha</li><li>beta</li></ul>" +
            "<ol><li>one</li><li>two</li></ol>" +
            "<table><tr><th>Name</th><th>Age</th></tr><tr><td>Alice</td><td>30</td></tr></table>" +
            "<a href=\"https://example.com\">link text</a>" +
            "<img src=\"pic.png\" alt=\"a picture\" />" +
            "<pre><code class=\"language-csharp\">int x = 1;</code></pre>" +
            "</body></html>";

        /// <summary>A minimal but valid RTF document containing plain text.</summary>
        public const string Rtf =
            "{\\rtf1\\ansi\\ansicpg1252\\deff0 {\\fonttbl {\\f0 Times New Roman;}}" +
            "\\viewkind4\\uc1\\pard\\f0\\fs24 " +
            "Hello world from RTF. This is a paragraph of text.\\par " +
            "A second paragraph follows here.\\par}";

        /// <summary>
        /// Returns the 8-byte PNG signature followed by a minimal IHDR-like padding.
        /// Sufficient for signature-based type detection.
        /// </summary>
        public static byte[] PngBytes()
        {
            List<byte> bytes = new List<byte> { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            for (int i = 0; i < 32; i++) bytes.Add(0x00);
            return bytes.ToArray();
        }

        /// <summary>Returns bytes beginning with the %PDF- signature.</summary>
        public static byte[] PdfBytes()
        {
            return Encoding.ASCII.GetBytes("%PDF-1.4\r\n1 0 obj\r\n<<>>\r\nendobj\r\n%%EOF");
        }

        /// <summary>Returns bytes beginning with the GIF89a signature.</summary>
        public static byte[] GifBytes()
        {
            List<byte> bytes = new List<byte>(Encoding.ASCII.GetBytes("GIF89a"));
            for (int i = 0; i < 16; i++) bytes.Add(0x00);
            return bytes.ToArray();
        }

        /// <summary>Returns UTF-8 bytes for the plain-text sample.</summary>
        public static byte[] TextBytes()
        {
            return Encoding.UTF8.GetBytes(PlainText);
        }
    }
}
