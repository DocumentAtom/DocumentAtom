namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;
    using System.Xml;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Text;
    using DocumentAtom.Text.Csv;
    using DocumentAtom.Text.Html;
    using DocumentAtom.Text.Json;
    using DocumentAtom.Text.Markdown;
    using DocumentAtom.Text.Xml;
    using SerializableDataTables;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the text-family document processors: plain text, CSV, JSON, XML, Markdown, HTML.
    /// </summary>
    internal static class TextProcessorSuites
    {
        /// <summary>
        /// Flattens an atom tree, yielding each atom and recursing into its quarks. Processors with
        /// hierarchy building nest child content under headers, so tests search the flattened set.
        /// </summary>
        private static IEnumerable<Atom> Flatten(IEnumerable<Atom> atoms)
        {
            foreach (Atom atom in atoms)
            {
                yield return atom;
                if (atom.Quarks != null)
                {
                    foreach (Atom quark in Flatten(atom.Quarks))
                        yield return quark;
                }
            }
        }

        internal static TestSuiteDescriptor Text()
        {
            return new SuiteBuilder("Text.TextProcessor")
                .Case("Extract.Paragraphs", "A multi-paragraph document produces text atoms", () =>
                {
                    string path = Workspace.WriteText("txt", SampleData.PlainText);
                    using TextProcessor p = new TextProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                    foreach (Atom a in atoms) Check.Equal(AtomTypeEnum.Text, a.Type);
                })
                .Case("Extract.Bytes", "Extraction from a byte array produces text atoms", () =>
                {
                    using TextProcessor p = new TextProcessor();
                    List<Atom> atoms = p.Extract(SampleData.TextBytes()).ToList();
                    Check.NotEmpty(atoms);
                })
                .Case("Extract.EmptyBytes", "Extraction from empty bytes yields no atoms", () =>
                {
                    using TextProcessor p = new TextProcessor();
                    Check.Empty(p.Extract(Array.Empty<byte>()).ToList());
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using TextProcessor p = new TextProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Validation.MissingFile", "A missing file throws FileNotFoundException on enumeration", () =>
                {
                    using TextProcessor p = new TextProcessor();
                    Check.Throws<FileNotFoundException>(() => p.Extract(Workspace.NonExistentPath("txt")).ToList());
                })
                .Case("Settings.Null", "Assigning null settings throws ArgumentNullException", () =>
                {
                    using TextProcessor p = new TextProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Settings = null!);
                })
                .Case("Settings.NullDelimiters", "Null delimiters are rejected", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new TextProcessorSettings().Delimiters = null!);
                })
                .Case("Settings.EmptyDelimiterElement", "A null or empty delimiter element is rejected", () =>
                {
                    Check.Throws<ArgumentException>(() => new TextProcessorSettings().Delimiters = new List<string> { "" });
                })
                .Build("Text: TextProcessor");
        }

        internal static TestSuiteDescriptor Csv()
        {
            return new SuiteBuilder("Text.CsvProcessor")
                .Case("Extract.Table", "A CSV document produces a table atom with the right column count", () =>
                {
                    string path = Workspace.WriteText("csv", SampleData.Csv);
                    using CsvProcessor p = new CsvProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                    Atom table = atoms[0];
                    Check.Equal(AtomTypeEnum.Table, table.Type);
                    Check.Equal(3, table.Columns);
                })
                .Case("Extract.TableContent", "A CSV table atom exposes typed columns and the parsed row values", () =>
                {
                    string path = Workspace.WriteText("csv", SampleData.Csv);
                    using CsvProcessor p = new CsvProcessor();
                    Atom table = p.Extract(path).First();

                    Check.Equal(AtomTypeEnum.Table, table.Type);
                    Check.NotNull(table.Table);

                    // Columns are taken from the CSV header and typed as strings by the processor.
                    List<string> columnNames = table.Table.Columns.Select(c => c.Name).ToList();
                    Check.Equal(3, columnNames.Count);
                    Check.Equal("Name", columnNames[0]);
                    Check.Equal("Age", columnNames[1]);
                    Check.Equal("City", columnNames[2]);
                    foreach (SerializableColumn column in table.Table.Columns)
                        Check.Equal(ColumnValueTypeEnum.String, column.Type);

                    // Rows preserve the parsed cell values in header order.
                    Check.Equal(3, table.Table.Rows.Count);
                    Check.Equal("Alice", table.Table.Rows[0]["Name"].ToString());
                    Check.Equal("30", table.Table.Rows[0]["Age"].ToString());
                    Check.Equal("Seattle", table.Table.Rows[0]["City"].ToString());
                })
                .Case("Extract.RowsPerAtom", "RowsPerAtom of one produces one atom per data row", () =>
                {
                    string path = Workspace.WriteText("csv", SampleData.Csv);
                    CsvProcessorSettings settings = new CsvProcessorSettings();
                    settings.RowsPerAtom = 1;
                    using CsvProcessor p = new CsvProcessor(settings);
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.Equal(3, atoms.Count);
                })
                .Case("Validation.RowsPerAtom", "A negative RowsPerAtom is rejected", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new CsvProcessorSettings().RowsPerAtom = -1);
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using CsvProcessor p = new CsvProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Build("Text: CsvProcessor");
        }

        internal static TestSuiteDescriptor Json()
        {
            return new SuiteBuilder("Text.JsonProcessor")
                .Case("Extract.Object", "A JSON object produces table atoms", () =>
                {
                    string path = Workspace.WriteText("json", SampleData.Json);
                    using JsonProcessor p = new JsonProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                })
                .Case("Extract.Array", "A JSON array produces table atoms", () =>
                {
                    string path = Workspace.WriteText("json", SampleData.JsonArray);
                    using JsonProcessor p = new JsonProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                })
                .Case("Validation.InvalidJson", "Malformed JSON throws a JsonException", () =>
                {
                    string path = Workspace.WriteText("json", SampleData.InvalidJson);
                    using JsonProcessor p = new JsonProcessor();
                    Check.Throws<JsonException>(() => p.Extract(path).ToList());
                })
                .Case("Validation.MaxDepth", "A MaxDepth below one is rejected", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new JsonProcessorSettings().MaxDepth = 0);
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using JsonProcessor p = new JsonProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Build("Text: JsonProcessor");
        }

        internal static TestSuiteDescriptor Xml()
        {
            return new SuiteBuilder("Text.XmlProcessor")
                .Case("Extract.Document", "An XML document produces table atoms", () =>
                {
                    string path = Workspace.WriteText("xml", SampleData.Xml);
                    using XmlProcessor p = new XmlProcessor();
                    List<Atom> atoms = p.Extract(path).ToList();
                    Check.NotEmpty(atoms);
                })
                .Case("Validation.InvalidXml", "Malformed XML throws an XmlException", () =>
                {
                    string path = Workspace.WriteText("xml", SampleData.InvalidXml);
                    using XmlProcessor p = new XmlProcessor();
                    Check.Throws<XmlException>(() => p.Extract(path).ToList());
                })
                .Case("Validation.MaxDepth", "A MaxDepth below one is rejected", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new XmlProcessorSettings().MaxDepth = 0);
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using XmlProcessor p = new XmlProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Build("Text: XmlProcessor");
        }

        internal static TestSuiteDescriptor Markdown()
        {
            return new SuiteBuilder("Text.MarkdownProcessor")
                .Case("Extract.Headers", "A markdown document yields a header atom", () =>
                {
                    string path = Workspace.WriteText("md", SampleData.Markdown);
                    using MarkdownProcessor p = new MarkdownProcessor();
                    List<Atom> atoms = Flatten(p.Extract(path)).ToList();
                    Check.NotEmpty(atoms);
                    Check.True(atoms.Any(a => a.Formatting == MarkdownFormattingEnum.Header), "Expected at least one header atom.");
                })
                .Case("Extract.Table", "A markdown document yields a table atom", () =>
                {
                    string path = Workspace.WriteText("md", SampleData.Markdown);
                    using MarkdownProcessor p = new MarkdownProcessor();
                    List<Atom> atoms = Flatten(p.Extract(path)).ToList();
                    Check.True(atoms.Any(a => a.Formatting == MarkdownFormattingEnum.Table), "Expected at least one table atom.");
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using MarkdownProcessor p = new MarkdownProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Build("Text: MarkdownProcessor");
        }

        internal static TestSuiteDescriptor Html()
        {
            return new SuiteBuilder("Text.HtmlProcessor")
                .Case("Extract.MixedContent", "An HTML document yields table and hyperlink atoms", () =>
                {
                    string path = Workspace.WriteText("html", SampleData.Html);
                    using HtmlProcessor p = new HtmlProcessor();
                    List<Atom> atoms = Flatten(p.Extract(path)).ToList();
                    Check.NotEmpty(atoms);
                    Check.True(atoms.Any(a => a.Type == AtomTypeEnum.Table), "Expected a table atom.");
                    Check.True(atoms.Any(a => a.Type == AtomTypeEnum.Hyperlink), "Expected a hyperlink atom.");
                })
                .Case("Extract.Headings", "An HTML document yields a heading atom with a header level", () =>
                {
                    string path = Workspace.WriteText("html", SampleData.Html);
                    using HtmlProcessor p = new HtmlProcessor();
                    List<Atom> atoms = Flatten(p.Extract(path)).ToList();
                    Check.True(atoms.Any(a => a.HeaderLevel.HasValue && a.HeaderLevel.Value == 1), "Expected an h1 atom.");
                })
                .Case("Validation.NullFilename", "A null filename throws ArgumentNullException", () =>
                {
                    using HtmlProcessor p = new HtmlProcessor();
                    Check.Throws<ArgumentNullException>(() => p.Extract((string)null!).ToList());
                })
                .Case("Validation.MissingFile", "A missing file throws FileNotFoundException", () =>
                {
                    using HtmlProcessor p = new HtmlProcessor();
                    Check.Throws<FileNotFoundException>(() => p.Extract(Workspace.NonExistentPath("html")).ToList());
                })
                .Build("Text: HtmlProcessor");
        }
    }
}
