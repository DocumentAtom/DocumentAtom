namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering <see cref="Atom"/> (properties, factories, markdown helpers) and <see cref="BoundingBox"/>.
    /// </summary>
    internal static class AtomSuites
    {
        internal static TestSuiteDescriptor Atom()
        {
            return new SuiteBuilder("Core.Atom")
                .Case("Defaults", "A new atom has a unique GUID and defaults to text", () =>
                {
                    Atom a = new Atom();
                    Atom b = new Atom();
                    Check.NotEqual(a.GUID, b.GUID);
                    Check.Equal(AtomTypeEnum.Text, a.Type);
                    Check.Equal(0, a.Length);
                })
                .Case("Validation.PageNumber", "PageNumber rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().PageNumber = -1);
                })
                .Case("Validation.Position", "Position rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().Position = -1);
                })
                .Case("Validation.Length", "Length rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().Length = -1);
                })
                .Case("Validation.Rows", "Rows rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().Rows = -1);
                })
                .Case("Validation.Columns", "Columns rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().Columns = -1);
                })
                .Case("Validation.HeaderLevel", "HeaderLevel rejects values below one", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new Atom().HeaderLevel = 0);
                })
                .Case("FromTextContent.Basic", "FromTextContent populates text, length, and hashes", () =>
                {
                    Atom a = Core.Atoms.Atom.FromTextContent("hello world", 0, new ChunkingConfiguration());
                    Check.Equal(AtomTypeEnum.Text, a.Type);
                    Check.Equal("hello world", a.Text);
                    Check.Equal(11, a.Length);
                    Check.NotNull(a.MD5Hash);
                    Check.NotNull(a.SHA256Hash);
                })
                .Case("FromTextContent.NullSettings", "FromTextContent tolerates null settings", () =>
                {
                    Atom a = Core.Atoms.Atom.FromTextContent("data", 0, null!);
                    Check.Equal("data", a.Text);
                })
                .Case("FromTextContent.NullText", "FromTextContent throws for null text", () =>
                {
                    Check.Throws<ArgumentNullException>(() => Core.Atoms.Atom.FromTextContent(null!, 0, new ChunkingConfiguration()));
                })
                .Case("FromTextContent.NegativePosition", "FromTextContent throws for negative position", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => Core.Atoms.Atom.FromTextContent("data", -1, new ChunkingConfiguration()));
                })
                .Case("FromMarkdownContent.Header", "FromMarkdownContent detects a header and its level", () =>
                {
                    Atom a = Core.Atoms.Atom.FromMarkdownContent("## Section", 0, new ChunkingConfiguration());
                    Check.Equal(MarkdownFormattingEnum.Header, a.Formatting);
                    Check.Equal(2, a.HeaderLevel);
                })
                .Case("FromMarkdownContent.UnorderedList", "FromMarkdownContent detects an unordered list", () =>
                {
                    Atom a = Core.Atoms.Atom.FromMarkdownContent("- one\n- two", 0, new ChunkingConfiguration());
                    Check.Equal(MarkdownFormattingEnum.UnorderedList, a.Formatting);
                    Check.NotNull(a.UnorderedList);
                })
                .Case("FromMarkdownContent.OrderedList", "FromMarkdownContent detects an ordered list", () =>
                {
                    Atom a = Core.Atoms.Atom.FromMarkdownContent("1. one\n2. two", 0, new ChunkingConfiguration());
                    Check.Equal(MarkdownFormattingEnum.OrderedList, a.Formatting);
                    Check.NotNull(a.OrderedList);
                })
                .Case("FromMarkdownContent.Table", "FromMarkdownContent detects a table", () =>
                {
                    Atom a = Core.Atoms.Atom.FromMarkdownContent("| Name | Age |\n| --- | --- |\n| Alice | 30 |", 0, new ChunkingConfiguration());
                    Check.Equal(MarkdownFormattingEnum.Table, a.Formatting);
                    Check.NotNull(a.Table);
                })
                .Case("FromTableStructure.Null", "FromTableStructure throws for null input", () =>
                {
                    Check.Throws<ArgumentNullException>(() => Core.Atoms.Atom.FromTableStructure(null!));
                })
                .Case("IsMarkdownUnorderedListItem", "Unordered list item detection", () =>
                {
                    Check.True(Core.Atoms.Atom.IsMarkdownUnorderedListItem("- item"));
                    Check.True(Core.Atoms.Atom.IsMarkdownUnorderedListItem("* item"));
                    Check.False(Core.Atoms.Atom.IsMarkdownUnorderedListItem("not a list"));
                    Check.False(Core.Atoms.Atom.IsMarkdownUnorderedListItem(null!));
                })
                .Case("IsMarkdownOrderedListItem", "Ordered list item detection", () =>
                {
                    Check.True(Core.Atoms.Atom.IsMarkdownOrderedListItem("1. item"));
                    Check.False(Core.Atoms.Atom.IsMarkdownOrderedListItem("- item"));
                })
                .Case("IsMarkdownTableItem", "Table item detection", () =>
                {
                    Check.True(Core.Atoms.Atom.IsMarkdownTableItem("| a | b |"));
                    Check.False(Core.Atoms.Atom.IsMarkdownTableItem("plain"));
                })
                .Case("MarkdownTextToList", "MarkdownTextToList strips list markers", () =>
                {
                    List<string> items = Core.Atoms.Atom.MarkdownTextToList("- alpha\n- beta");
                    Check.Equal(2, items.Count);
                    Check.Equal("alpha", items[0]);
                })
                .Case("MarkdownTextToDataTable.Empty", "MarkdownTextToDataTable returns null for empty text", () =>
                {
                    Check.Null(Core.Atoms.Atom.MarkdownTextToDataTable(""));
                })
                .Case("ToString.NullHashesThrows", "ToString on a bare atom throws because hashes are null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new Atom().ToString());
                })
                .Case("ToString.PopulatedAtom", "ToString on a text atom includes its type", () =>
                {
                    Atom a = Core.Atoms.Atom.FromTextContent("data", 0, new ChunkingConfiguration());
                    string s = a.ToString();
                    Check.NotNull(s);
                    Check.Contains("Text", s);
                })
                .Build("Core: Atom");
        }

        internal static TestSuiteDescriptor BoundingBoxSuite()
        {
            return new SuiteBuilder("Core.BoundingBox")
                .Case("Defaults", "Default corners are the origin", () =>
                {
                    BoundingBox box = new BoundingBox();
                    Check.Equal(0, box.UpperLeft.X);
                    Check.Equal(0, box.UpperLeft.Y);
                })
                .Case("Validation.NegativeCoordinate", "Negative coordinates are rejected", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new BoundingBox().UpperLeft = new Point(-1, 0));
                })
                .Case("FromRectangle.Width", "FromRectangle derives width from the rectangle", () =>
                {
                    BoundingBox box = BoundingBox.FromRectangle(new Rectangle(0, 0, 100, 50));
                    Check.Equal(100, box.Width);
                })
                .Case("FromRectangle.Contains", "Contains returns true for an interior point", () =>
                {
                    BoundingBox box = BoundingBox.FromRectangle(new Rectangle(0, 0, 100, 50));
                    Check.True(box.Contains(50, 25));
                    Check.False(box.Contains(200, 25));
                })
                .Case("ToString.ContainsCorners", "ToString lists the four corners", () =>
                {
                    BoundingBox box = BoundingBox.FromRectangle(new Rectangle(0, 0, 10, 10));
                    string s = box.ToString();
                    Check.Contains("UL", s);
                    Check.Contains("LR", s);
                })
                .Build("Core: BoundingBox");
        }
    }
}
