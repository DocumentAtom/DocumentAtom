namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using SharpToken;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the individual static chunker implementations directly. This is the migrated
    /// and expanded superset of the legacy Test.Chunking per-chunker xUnit suites.
    /// </summary>
    internal static class ChunkerSuites
    {
        private static readonly GptEncoding _Encoding = GptEncoding.GetEncoding("cl100k_base");

        internal static TestSuiteDescriptor FixedToken()
        {
            return new SuiteBuilder("Core.FixedTokenChunker")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(FixedTokenChunker.Chunk(null!, new ChunkingConfiguration { FixedTokenCount = 10 }, _Encoding));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(FixedTokenChunker.Chunk(string.Empty, new ChunkingConfiguration { FixedTokenCount = 10 }, _Encoding));
                })
                .Case("Single.SmallText", "Text under the token limit yields a single chunk equal to the input", () =>
                {
                    List<string> result = FixedTokenChunker.Chunk("Hello world", new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding);
                    Check.Single(result);
                    Check.Equal("Hello world", result[0]);
                })
                .Case("Multiple.LargeText", "Text over the token limit is split into multiple chunks", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog.", 50));
                    List<string> result = FixedTokenChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 20 }, _Encoding);
                    Check.True(result.Count > 1, "Expected multiple chunks for long text.");
                })
                .Case("Overlap.MoreOrEqualChunks", "Overlap produces at least as many chunks as no overlap", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("word", 100));
                    List<string> none = FixedTokenChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 20, OverlapCount = 0 }, _Encoding);
                    List<string> some = FixedTokenChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 20, OverlapCount = 5 }, _Encoding);
                    Check.True(some.Count >= none.Count, "Overlap should produce at least as many chunks.");
                })
                .Case("Overlap.PercentagePrecedence", "OverlapPercentage drives splitting when set", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox.", 50));
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 20, OverlapCount = 0, OverlapPercentage = 0.25 };
                    Check.True(FixedTokenChunker.Chunk(text, cfg, _Encoding).Count > 1);
                })
                .Case("Overlap.SlidingWindow", "The sliding window strategy produces results", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps.", 30));
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 20, OverlapCount = 5, OverlapStrategy = OverlapStrategyEnum.SlidingWindow };
                    Check.True(FixedTokenChunker.Chunk(text, cfg, _Encoding).Count > 1);
                })
                .Case("Overlap.SentenceBoundaryAware", "The sentence-boundary-aware strategy produces results", () =>
                {
                    string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence. Seventh sentence. Eighth sentence.";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 10, OverlapCount = 3, OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware };
                    Check.True(FixedTokenChunker.Chunk(text, cfg, _Encoding).Count > 1);
                })
                .Case("Overlap.SemanticBoundaryAware", "The semantic-boundary-aware strategy produces results", () =>
                {
                    string text = "First paragraph content here.\n\nSecond paragraph content here.\n\nThird paragraph content here.\n\nFourth paragraph content here.";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 10, OverlapCount = 3, OverlapStrategy = OverlapStrategyEnum.SemanticBoundaryAware };
                    Check.True(FixedTokenChunker.Chunk(text, cfg, _Encoding).Count > 1);
                })
                .Case("Content.Preserved", "With no overlap, recombining chunks reproduces the input", () =>
                {
                    string text = "Hello world this is a test of chunking";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 5, OverlapCount = 0 };
                    List<string> result = FixedTokenChunker.Chunk(text, cfg, _Encoding);
                    Check.Equal(text, string.Join(string.Empty, result));
                })
                .Case("Overlap.NoInfiniteLoop", "Overlap greater than or equal to chunk size still terminates", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 5, OverlapCount = 10 };
                    string text = string.Join(" ", Enumerable.Repeat("token", 50));
                    Check.NotEmpty(FixedTokenChunker.Chunk(text, cfg, _Encoding));
                })
                .Build("Core: FixedTokenChunker");
        }

        internal static TestSuiteDescriptor Sentence()
        {
            return new SuiteBuilder("Core.SentenceChunker")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(SentenceChunker.Chunk(null!, new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(SentenceChunker.Chunk(string.Empty, new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Single.Sentence", "A single sentence yields a single chunk", () =>
                {
                    Check.Single(SentenceChunker.Chunk("This is a single sentence.", new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Multiple.TokenBudget", "Sentences are grouped by the token budget", () =>
                {
                    string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence.";
                    Check.True(SentenceChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 8 }, _Encoding).Count > 1);
                })
                .Case("Overlap.MoreOrEqual", "Overlap produces at least as many chunks", () =>
                {
                    string text = "One. Two. Three. Four. Five. Six. Seven. Eight.";
                    List<string> some = SentenceChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 5, OverlapCount = 1 }, _Encoding);
                    List<string> none = SentenceChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 5, OverlapCount = 0 }, _Encoding);
                    Check.True(some.Count >= none.Count);
                })
                .Case("Overlap.PercentagePrecedence", "OverlapPercentage drives splitting when set", () =>
                {
                    string text = "First. Second. Third. Fourth. Fifth. Sixth. Seventh. Eighth. Ninth. Tenth.";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { FixedTokenCount = 5, OverlapCount = 0, OverlapPercentage = 0.5 };
                    Check.True(SentenceChunker.Chunk(text, cfg, _Encoding).Count > 1);
                })
                .Case("Split.Exclamation", "Exclamation marks split sentences", () =>
                {
                    Check.True(SentenceChunker.Chunk("Wow! Amazing! Incredible!", new ChunkingConfiguration { FixedTokenCount = 2 }, _Encoding).Count >= 2);
                })
                .Case("Split.Question", "Question marks split sentences", () =>
                {
                    Check.True(SentenceChunker.Chunk("What? Why? How? When?", new ChunkingConfiguration { FixedTokenCount = 2 }, _Encoding).Count >= 2);
                })
                .Case("LargeBudget.Single", "A large token budget yields a single chunk", () =>
                {
                    Check.Single(SentenceChunker.Chunk("First sentence. Second sentence. Third sentence.", new ChunkingConfiguration { FixedTokenCount = 1000 }, _Encoding));
                })
                .Build("Core: SentenceChunker");
        }

        internal static TestSuiteDescriptor Paragraph()
        {
            return new SuiteBuilder("Core.ParagraphChunker")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(ParagraphChunker.Chunk(null!, new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(ParagraphChunker.Chunk(string.Empty, new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Single.Paragraph", "A single paragraph yields a single chunk", () =>
                {
                    Check.Single(ParagraphChunker.Chunk("Single paragraph text.", new ChunkingConfiguration { FixedTokenCount = 100 }, _Encoding));
                })
                .Case("Split.DoubleNewline", "Paragraphs split on double newlines", () =>
                {
                    string text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";
                    Check.True(ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 5 }, _Encoding).Count > 1);
                })
                .Case("Split.WindowsLineEndings", "Paragraphs split on Windows-style double newlines", () =>
                {
                    string text = "First paragraph.\r\n\r\nSecond paragraph.\r\n\r\nThird paragraph.";
                    Check.True(ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 5 }, _Encoding).Count > 1);
                })
                .Case("LargeBudget.Single", "A large token budget groups paragraphs into a single chunk", () =>
                {
                    string text = "A.\n\nB.\n\nC.\n\nD.\n\nE.\n\nF.";
                    Check.Single(ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 1000 }, _Encoding));
                })
                .Case("Overlap.MoreOrEqual", "Overlap produces at least as many chunks", () =>
                {
                    string text = "First paragraph here.\n\nSecond paragraph here.\n\nThird paragraph here.\n\nFourth paragraph here.";
                    List<string> some = ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 8, OverlapCount = 1 }, _Encoding);
                    List<string> none = ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 8, OverlapCount = 0 }, _Encoding);
                    Check.True(some.Count >= none.Count);
                })
                .Case("Trims.Whitespace", "Chunks are trimmed of surrounding whitespace", () =>
                {
                    string text = "  First paragraph.  \n\n  Second paragraph.  ";
                    foreach (string chunk in ParagraphChunker.Chunk(text, new ChunkingConfiguration { FixedTokenCount = 5 }, _Encoding))
                        Check.Equal(chunk.Trim(), chunk);
                })
                .Build("Core: ParagraphChunker");
        }

        internal static TestSuiteDescriptor Regex()
        {
            return new SuiteBuilder("Core.RegexChunker")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(RegexChunker.Chunk(null!, new ChunkingConfiguration { RegexPattern = "\\n" }, _Encoding));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(RegexChunker.Chunk(string.Empty, new ChunkingConfiguration { RegexPattern = "\\n" }, _Encoding));
                })
                .Case("Split.Newline", "Text splits on a newline pattern", () =>
                {
                    List<string> result = RegexChunker.Chunk("Line one\nLine two\nLine three", new ChunkingConfiguration { RegexPattern = "\\n" }, _Encoding);
                    Check.Equal(3, result.Count);
                    Check.Equal("Line one", result[0]);
                    Check.Equal("Line three", result[2]);
                })
                .Case("Split.CustomDelimiter", "Text splits on a custom delimiter", () =>
                {
                    List<string> result = RegexChunker.Chunk("Section A---Section B---Section C", new ChunkingConfiguration { RegexPattern = "---" }, _Encoding);
                    Check.Equal(3, result.Count);
                    Check.Equal("Section B", result[1]);
                })
                .Case("Validation.NullPattern", "A null pattern throws ArgumentException", () =>
                {
                    Check.Throws<ArgumentException>(() => RegexChunker.Chunk("text", new ChunkingConfiguration { RegexPattern = null }, _Encoding));
                })
                .Case("Validation.EmptyPattern", "An empty pattern throws ArgumentException", () =>
                {
                    Check.Throws<ArgumentException>(() => RegexChunker.Chunk("text", new ChunkingConfiguration { RegexPattern = string.Empty }, _Encoding));
                })
                .Case("FiltersWhitespace", "Whitespace-only segments are filtered out", () =>
                {
                    List<string> result = RegexChunker.Chunk("Content\n\n\n\nMore content", new ChunkingConfiguration { RegexPattern = "\\n" }, _Encoding);
                    foreach (string segment in result) Check.False(string.IsNullOrWhiteSpace(segment));
                })
                .Case("TrimsSegments", "Segments are trimmed", () =>
                {
                    List<string> result = RegexChunker.Chunk("  A  \n  B  \n  C  ", new ChunkingConfiguration { RegexPattern = "\\n" }, _Encoding);
                    foreach (string segment in result) Check.Equal(segment.Trim(), segment);
                })
                .Case("NoMatch.ReturnsOriginal", "A non-matching pattern returns the original text", () =>
                {
                    List<string> result = RegexChunker.Chunk("No delimiters here", new ChunkingConfiguration { RegexPattern = "---" }, _Encoding);
                    Check.Single(result);
                    Check.Equal("No delimiters here", result[0]);
                })
                .Build("Core: RegexChunker");
        }

        internal static TestSuiteDescriptor ListAndTable()
        {
            return new SuiteBuilder("Core.ListTableChunkers")
                .Case("ListEntry.Null", "ListEntryChunker returns empty for null input", () =>
                {
                    Check.Empty(ListEntryChunker.Chunk(null!));
                })
                .Case("ListEntry.Empty", "ListEntryChunker returns empty for an empty list", () =>
                {
                    Check.Empty(ListEntryChunker.Chunk(new List<string>()));
                })
                .Case("ListEntry.EachItem", "ListEntryChunker yields one chunk per item", () =>
                {
                    List<string> result = ListEntryChunker.Chunk(new List<string> { "Apple", "Banana", "Cherry" });
                    Check.Equal(3, result.Count);
                    Check.Equal("Apple", result[0]);
                    Check.Equal("Cherry", result[2]);
                })
                .Case("ListEntry.DropsWhitespace", "ListEntryChunker drops whitespace-only entries", () =>
                {
                    List<string> result = ListEntryChunker.Chunk(new List<string> { "Apple", "  ", "Cherry", string.Empty, "\t" });
                    Check.Equal(2, result.Count);
                    Check.Equal("Cherry", result[1]);
                })
                .Case("ListEntry.AllWhitespace", "ListEntryChunker returns empty when all entries are whitespace", () =>
                {
                    Check.Empty(ListEntryChunker.Chunk(new List<string> { "  ", string.Empty, "\t", "\n" }));
                })
                .Case("WholeList.Null", "WholeListChunker returns empty for null input", () =>
                {
                    Check.Empty(WholeListChunker.Chunk(null!, true));
                })
                .Case("WholeList.Empty", "WholeListChunker returns empty for an empty list", () =>
                {
                    Check.Empty(WholeListChunker.Chunk(new List<string>(), false));
                })
                .Case("WholeList.Ordered", "WholeListChunker renders an ordered list", () =>
                {
                    List<string> result = WholeListChunker.Chunk(new List<string> { "First", "Second", "Third" }, true);
                    Check.Single(result);
                    Check.Contains("1. First", result[0]);
                    Check.Contains("3. Third", result[0]);
                })
                .Case("WholeList.Unordered", "WholeListChunker renders an unordered list", () =>
                {
                    List<string> result = WholeListChunker.Chunk(new List<string> { "Apple", "Banana" }, false);
                    Check.Single(result);
                    Check.Contains("- Apple", result[0]);
                    Check.Contains("- Banana", result[0]);
                })
                .Case("WholeList.JoinedByNewlines", "WholeListChunker joins items with newlines", () =>
                {
                    Check.Contains("\n", WholeListChunker.Chunk(new List<string> { "A", "B" }, false)[0]);
                })
                .Case("WholeList.SingleOrdered", "A single ordered item renders as a numbered entry", () =>
                {
                    Check.Equal("1. Only", WholeListChunker.Chunk(new List<string> { "Only" }, true)[0]);
                })
                .Case("WholeList.SingleUnordered", "A single unordered item renders as a bullet entry", () =>
                {
                    Check.Equal("- Only", WholeListChunker.Chunk(new List<string> { "Only" }, false)[0]);
                })
                .Case("Table.Empty", "Table chunkers return empty for an empty table", () =>
                {
                    List<List<string>> empty = new List<List<string>>();
                    Check.Empty(TableChunker.ChunkByRow(empty));
                    Check.Empty(TableChunker.ChunkByRowWithHeaders(empty));
                    Check.Empty(TableChunker.ChunkByKeyValuePairs(empty));
                    Check.Empty(TableChunker.ChunkWholeTable(empty));
                })
                .Case("Table.HeaderOnly", "A header-only table yields no data chunks", () =>
                {
                    List<List<string>> headerOnly = new List<List<string>> { new List<string> { "Name", "Age" } };
                    Check.Empty(TableChunker.ChunkByRow(headerOnly));
                    Check.Empty(TableChunker.ChunkByKeyValuePairs(headerOnly));
                })
                .Case("Table.ByRow", "ChunkByRow yields space-separated data rows", () =>
                {
                    List<string> result = TableChunker.ChunkByRow(BigTable());
                    Check.Equal(3, result.Count);
                    Check.Equal("Alice 30 NYC", result[0]);
                    Check.Equal("Charlie 35 Chicago", result[2]);
                })
                .Case("Table.ByRowWithHeaders", "ChunkByRowWithHeaders includes headers and separator in every chunk", () =>
                {
                    List<string> result = TableChunker.ChunkByRowWithHeaders(BigTable());
                    Check.Equal(3, result.Count);
                    Check.Contains("| Name | Age | City |", result[0]);
                    Check.Contains("|---|---|---|", result[0]);
                    foreach (string chunk in result) Check.Contains("Name", chunk);
                })
                .Case("Table.GroupSize2", "ChunkByRowGroupWithHeaders groups rows in twos", () =>
                {
                    List<string> result = TableChunker.ChunkByRowGroupWithHeaders(BigTable(), 2);
                    Check.Equal(2, result.Count);
                    Check.Contains("Alice", result[0]);
                    Check.Contains("Bob", result[0]);
                    Check.Contains("Charlie", result[1]);
                })
                .Case("Table.GroupSize1EqualsRowWithHeaders", "Group size one matches the per-row chunk count", () =>
                {
                    Check.Equal(TableChunker.ChunkByRowWithHeaders(BigTable()).Count, TableChunker.ChunkByRowGroupWithHeaders(BigTable(), 1).Count);
                })
                .Case("Table.GroupLargerThanRows", "A group size larger than the row count yields one chunk", () =>
                {
                    List<string> result = TableChunker.ChunkByRowGroupWithHeaders(BigTable(), 100);
                    Check.Single(result);
                    Check.Contains("Charlie", result[0]);
                })
                .Case("Table.GroupSizeZeroClamps", "A group size of zero clamps to one", () =>
                {
                    Check.Equal(3, TableChunker.ChunkByRowGroupWithHeaders(BigTable(), 0).Count);
                })
                .Case("Table.KeyValuePairs", "ChunkByKeyValuePairs formats each row as key/value pairs", () =>
                {
                    List<string> result = TableChunker.ChunkByKeyValuePairs(BigTable());
                    Check.Equal(3, result.Count);
                    Check.Equal("Name: Alice, Age: 30, City: NYC", result[0]);
                    Check.Equal("Name: Charlie, Age: 35, City: Chicago", result[2]);
                })
                .Case("Table.WholeTable", "ChunkWholeTable renders one markdown table with all rows", () =>
                {
                    List<string> result = TableChunker.ChunkWholeTable(BigTable());
                    Check.Single(result);
                    Check.Contains("| Name | Age | City |", result[0]);
                    Check.Contains("| Alice | 30 | NYC |", result[0]);
                    Check.Contains("| Charlie | 35 | Chicago |", result[0]);
                })
                .Build("Core: List and Table chunkers");
        }

        private static List<List<string>> BigTable()
        {
            return new List<List<string>>
            {
                new List<string> { "Name", "Age", "City" },
                new List<string> { "Alice", "30", "NYC" },
                new List<string> { "Bob", "25", "LA" },
                new List<string> { "Charlie", "35", "Chicago" }
            };
        }
    }
}
