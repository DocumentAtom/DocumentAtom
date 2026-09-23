namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering each chunking strategy end-to-end through <see cref="ChunkingEngine"/>, which
    /// delegates to the TextChunker engine. These replace the legacy per-static-chunker suites now that
    /// the strategy implementations live in TextChunker.
    /// </summary>
    internal static class ChunkerSuites
    {
        private static readonly ChunkingEngine _Engine = new ChunkingEngine();

        private static List<string> TextChunks(string text, ChunkingConfiguration config)
        {
            return _Engine.Chunk(AtomTypeEnum.Text, text, null, null, null, config)
                .Select(c => c.Text!)
                .ToList();
        }

        private static List<string> ListChunks(List<string>? ordered, List<string>? unordered, ChunkingConfiguration config)
        {
            return _Engine.Chunk(AtomTypeEnum.List, null, ordered, unordered, null, config)
                .Select(c => c.Text!)
                .ToList();
        }

        private static List<string> TableChunks(List<List<string>> table, ChunkingConfiguration config)
        {
            return _Engine.Chunk(AtomTypeEnum.Table, null, null, null, table, config)
                .Select(c => c.Text!)
                .ToList();
        }

        internal static TestSuiteDescriptor FixedToken()
        {
            return new SuiteBuilder("Core.FixedTokenCount")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(null!, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 10 }));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(string.Empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 10 }));
                })
                .Case("Single.SmallText", "Text under the token limit yields a single chunk equal to the input", () =>
                {
                    List<string> result = TextChunks("Hello world", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 });
                    Check.Single(result);
                    Check.Equal("Hello world", result[0]);
                })
                .Case("Multiple.LargeText", "Text over the token limit is split into multiple chunks", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog.", 50));
                    List<string> result = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 20 });
                    Check.True(result.Count > 1, "Expected multiple chunks for long text.");
                })
                .Case("Overlap.MoreOrEqualChunks", "Overlap produces at least as many chunks as no overlap", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("word", 100));
                    List<string> none = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 20, OverlapCount = 0 });
                    List<string> some = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 20, OverlapCount = 5 });
                    Check.True(some.Count >= none.Count, "Overlap should produce at least as many chunks.");
                })
                .Case("Overlap.PercentagePrecedence", "OverlapPercentage drives splitting when set", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox.", 50));
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 20, OverlapCount = 0, OverlapPercentage = 0.25 };
                    Check.True(TextChunks(text, cfg).Count > 1);
                })
                .Case("Overlap.SlidingWindow", "The sliding window strategy produces results", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps.", 30));
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 20, OverlapCount = 5, OverlapStrategy = OverlapStrategyEnum.SlidingWindow };
                    Check.True(TextChunks(text, cfg).Count > 1);
                })
                .Case("Overlap.SentenceBoundaryAware", "The sentence-boundary-aware strategy produces results", () =>
                {
                    string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence. Seventh sentence. Eighth sentence.";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 10, OverlapCount = 3, OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware };
                    Check.True(TextChunks(text, cfg).Count > 1);
                })
                .Case("Overlap.SemanticBoundaryAware", "The semantic-boundary-aware strategy produces results", () =>
                {
                    string text = "First paragraph content here.\n\nSecond paragraph content here.\n\nThird paragraph content here.\n\nFourth paragraph content here.";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 10, OverlapCount = 3, OverlapStrategy = OverlapStrategyEnum.SemanticBoundaryAware };
                    Check.True(TextChunks(text, cfg).Count > 1);
                })
                .Case("Content.Preserved", "With no overlap and no trimming, recombining chunks reproduces the input", () =>
                {
                    string text = "Hello world this is a test of chunking";
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 5, OverlapCount = 0, TrimWhitespace = false };
                    List<string> result = TextChunks(text, cfg);
                    Check.Equal(text, string.Join(string.Empty, result));
                })
                .Case("Overlap.NoInfiniteLoop", "Overlap greater than or equal to chunk size still terminates", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 5, OverlapCount = 10 };
                    string text = string.Join(" ", Enumerable.Repeat("token", 50));
                    Check.NotEmpty(TextChunks(text, cfg));
                })
                .Case("BudgetRespected", "No chunk exceeds the token budget", () =>
                {
                    string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog.", 40));
                    List<Chunk> result = _Engine.Chunk(AtomTypeEnum.Text, text, null, null, null, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 25 });
                    foreach (Chunk chunk in result) Check.True(chunk.TokenCount <= 25, "A fixed-token chunk exceeded the budget.");
                })
                .Build("Core: FixedTokenCount strategy");
        }

        internal static TestSuiteDescriptor Sentence()
        {
            return new SuiteBuilder("Core.SentenceBased")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(null!, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 100 }));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(string.Empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 100 }));
                })
                .Case("Single.Sentence", "A single sentence yields a single chunk", () =>
                {
                    Check.Single(TextChunks("This is a single sentence.", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 100 }));
                })
                .Case("Multiple.TokenBudget", "Sentences are grouped by the token budget", () =>
                {
                    string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence.";
                    Check.True(TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 8 }).Count > 1);
                })
                .Case("Overlap.MoreOrEqual", "Overlap produces at least as many chunks", () =>
                {
                    string text = "One. Two. Three. Four. Five. Six. Seven. Eight.";
                    List<string> some = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 5, OverlapCount = 1 });
                    List<string> none = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 5, OverlapCount = 0 });
                    Check.True(some.Count >= none.Count);
                })
                .Case("Split.Exclamation", "Exclamation marks split sentences", () =>
                {
                    Check.True(TextChunks("Wow! Amazing! Incredible!", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 2 }).Count >= 2);
                })
                .Case("Split.Question", "Question marks split sentences", () =>
                {
                    Check.True(TextChunks("What? Why? How? When?", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 2 }).Count >= 2);
                })
                .Case("LargeBudget.Single", "A large token budget yields a single chunk", () =>
                {
                    Check.Single(TextChunks("First sentence. Second sentence. Third sentence.", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.SentenceBased, FixedTokenCount = 1000 }));
                })
                .Build("Core: SentenceBased strategy");
        }

        internal static TestSuiteDescriptor Paragraph()
        {
            return new SuiteBuilder("Core.ParagraphBased")
                .Case("Empty.Null", "Null text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(null!, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 100 }));
                })
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(string.Empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 100 }));
                })
                .Case("Single.Paragraph", "A single paragraph yields a single chunk", () =>
                {
                    Check.Single(TextChunks("Single paragraph text.", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 100 }));
                })
                .Case("Split.DoubleNewline", "Paragraphs split on double newlines", () =>
                {
                    string text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";
                    Check.True(TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 5 }).Count > 1);
                })
                .Case("Split.WindowsLineEndings", "Paragraphs split on Windows-style double newlines", () =>
                {
                    string text = "First paragraph.\r\n\r\nSecond paragraph.\r\n\r\nThird paragraph.";
                    Check.True(TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 5 }).Count > 1);
                })
                .Case("LargeBudget.Single", "A large token budget groups paragraphs into a single chunk", () =>
                {
                    string text = "A.\n\nB.\n\nC.\n\nD.\n\nE.\n\nF.";
                    Check.Single(TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 1000 }));
                })
                .Case("Overlap.MoreOrEqual", "Overlap produces at least as many chunks", () =>
                {
                    string text = "First paragraph here.\n\nSecond paragraph here.\n\nThird paragraph here.\n\nFourth paragraph here.";
                    List<string> some = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 8, OverlapCount = 1 });
                    List<string> none = TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 8, OverlapCount = 0 });
                    Check.True(some.Count >= none.Count);
                })
                .Case("Trims.Whitespace", "Chunks are trimmed of surrounding whitespace by default", () =>
                {
                    string text = "  First paragraph.  \n\n  Second paragraph.  ";
                    foreach (string chunk in TextChunks(text, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ParagraphBased, FixedTokenCount = 5 }))
                        Check.Equal(chunk.Trim(), chunk);
                })
                .Build("Core: ParagraphBased strategy");
        }

        internal static TestSuiteDescriptor Regex()
        {
            return new SuiteBuilder("Core.RegexBased")
                .Case("Empty.Blank", "Empty text yields an empty list", () =>
                {
                    Check.Empty(TextChunks(string.Empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "\\n" }));
                })
                .Case("Split.Newline", "Text splits on a newline pattern", () =>
                {
                    List<string> result = TextChunks("Line one\nLine two\nLine three", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "\\n" });
                    Check.Equal(3, result.Count);
                    Check.Equal("Line one", result[0]);
                    Check.Equal("Line three", result[2]);
                })
                .Case("Split.CustomDelimiter", "Text splits on a custom delimiter", () =>
                {
                    List<string> result = TextChunks("Section A---Section B---Section C", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "---" });
                    Check.Equal(3, result.Count);
                    Check.Equal("Section B", result[1]);
                })
                .Case("FiltersWhitespace", "Whitespace-only segments are filtered out", () =>
                {
                    List<string> result = TextChunks("Content\n\n\n\nMore content", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "\\n" });
                    foreach (string segment in result) Check.False(string.IsNullOrWhiteSpace(segment));
                })
                .Case("TrimsSegments", "Segments are trimmed", () =>
                {
                    List<string> result = TextChunks("  A  \n  B  \n  C  ", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "\\n" });
                    foreach (string segment in result) Check.Equal(segment.Trim(), segment);
                })
                .Case("NoMatch.ReturnsOriginal", "A non-matching pattern returns the original text", () =>
                {
                    List<string> result = TextChunks("No delimiters here", new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RegexBased, RegexPattern = "---" });
                    Check.Single(result);
                    Check.Equal("No delimiters here", result[0]);
                })
                .Build("Core: RegexBased strategy");
        }

        internal static TestSuiteDescriptor ListAndTable()
        {
            return new SuiteBuilder("Core.ListTableStrategies")
                .Case("ListEntry.Empty", "ListEntry returns empty for an empty list", () =>
                {
                    Check.Empty(ListChunks(null, new List<string>(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry }));
                })
                .Case("ListEntry.EachItem", "ListEntry yields one chunk per item", () =>
                {
                    List<string> result = ListChunks(null, new List<string> { "Apple", "Banana", "Cherry" }, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry });
                    Check.Equal(3, result.Count);
                    Check.Equal("Apple", result[0]);
                    Check.Equal("Cherry", result[2]);
                })
                .Case("WholeList.Empty", "WholeList returns empty for an empty list", () =>
                {
                    Check.Empty(ListChunks(null, new List<string>(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeList }));
                })
                .Case("WholeList.Ordered", "WholeList renders an ordered list into a single chunk", () =>
                {
                    List<string> result = ListChunks(new List<string> { "First", "Second", "Third" }, null, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeList });
                    Check.Single(result);
                    Check.Contains("1. First", result[0]);
                    Check.Contains("3. Third", result[0]);
                })
                .Case("WholeList.Unordered", "WholeList renders an unordered list into a single chunk", () =>
                {
                    List<string> result = ListChunks(null, new List<string> { "Apple", "Banana" }, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeList });
                    Check.Single(result);
                    Check.Contains("- Apple", result[0]);
                    Check.Contains("- Banana", result[0]);
                })
                .Case("Table.Empty", "Table strategies return empty for an empty table", () =>
                {
                    List<List<string>> empty = new List<List<string>>();
                    Check.Empty(TableChunks(empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.Row }));
                    Check.Empty(TableChunks(empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.KeyValuePairs }));
                    Check.Empty(TableChunks(empty, new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeTable }));
                })
                .Case("Table.ByRow", "Row yields space-separated data rows", () =>
                {
                    List<string> result = TableChunks(BigTable(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.Row });
                    Check.Equal(3, result.Count);
                    Check.Equal("Alice 30 NYC", result[0]);
                    Check.Equal("Charlie 35 Chicago", result[2]);
                })
                .Case("Table.ByRowWithHeaders", "RowWithHeaders includes headers and separator in every chunk", () =>
                {
                    List<string> result = TableChunks(BigTable(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RowWithHeaders });
                    Check.Equal(3, result.Count);
                    Check.Contains("| Name | Age | City |", result[0]);
                    Check.Contains("|---|---|---|", result[0]);
                    foreach (string chunk in result) Check.Contains("Name", chunk);
                })
                .Case("Table.GroupSize2", "RowGroupWithHeaders groups rows in twos", () =>
                {
                    List<string> result = TableChunks(BigTable(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.RowGroupWithHeaders, RowGroupSize = 2 });
                    Check.Equal(2, result.Count);
                    Check.Contains("Alice", result[0]);
                    Check.Contains("Bob", result[0]);
                    Check.Contains("Charlie", result[1]);
                })
                .Case("Table.KeyValuePairs", "KeyValuePairs formats each row as key/value pairs", () =>
                {
                    List<string> result = TableChunks(BigTable(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.KeyValuePairs });
                    Check.Equal(3, result.Count);
                    Check.Equal("Name: Alice, Age: 30, City: NYC", result[0]);
                    Check.Equal("Name: Charlie, Age: 35, City: Chicago", result[2]);
                })
                .Case("Table.WholeTable", "WholeTable renders one markdown table with all rows", () =>
                {
                    List<string> result = TableChunks(BigTable(), new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeTable });
                    Check.Single(result);
                    Check.Contains("| Name | Age | City |", result[0]);
                    Check.Contains("| Alice | 30 | NYC |", result[0]);
                    Check.Contains("| Charlie | 35 | Chicago |", result[0]);
                })
                .Build("Core: List and Table strategies");
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
