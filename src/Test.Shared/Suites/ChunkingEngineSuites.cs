namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using Touchstone.Core;

    /// <summary>
    /// Suite covering <see cref="ChunkingEngine"/> routing, validation, and position/hash bookkeeping.
    /// Migrated and expanded from the legacy Test.Chunking xUnit suite.
    /// </summary>
    internal static class ChunkingEngineSuites
    {
        internal static TestSuiteDescriptor Build()
        {
            ChunkingEngine engine = new ChunkingEngine();

            return new SuiteBuilder("Core.ChunkingEngine")
                .Case("Route.Text", "Text type produces chunks", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Enable = true, Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 10 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Text, "Hello world this is a test string for chunking", null, null, null, cfg);
                    Check.NotEmpty(result);
                })
                .Case("Route.Code", "Code type routes to the text chunker", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Code, "let x = 1;", null, null, null, cfg);
                    Check.NotEmpty(result);
                    Check.Contains("let x = 1;", result[0].Text!);
                })
                .Case("Route.Hyperlink", "Hyperlink type routes to the text chunker", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Hyperlink, "https://example.com", null, null, null, cfg);
                    Check.NotEmpty(result);
                })
                .Case("Route.List.WholeList.Unordered", "WholeList strategy renders an unordered list into a single chunk", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeList };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "A", "B", "C" }, null, cfg);
                    Check.Single(result);
                    Check.Contains("- A", result[0].Text!);
                })
                .Case("Route.List.ListEntry", "ListEntry strategy yields one chunk per item", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "Apple", "Banana", "Cherry" }, null, cfg);
                    Check.Equal(3, result.Count);
                    Check.Equal("Apple", result[0].Text);
                })
                .Case("Route.List.OrderedPreferred", "An ordered list is preferred over an unordered list", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.WholeList };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, new List<string> { "First", "Second" }, new List<string> { "A", "B" }, null, cfg);
                    Check.Single(result);
                    Check.Contains("1. First", result[0].Text!);
                })
                .Case("Route.Table.Row", "Row strategy produces one chunk per data row", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.Row };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Table, null, null, null, Table(), cfg);
                    Check.Equal(2, result.Count);
                    Check.Equal("Alice 30", result[0].Text);
                })
                .Case("Route.Table.KeyValuePairs", "KeyValuePairs strategy pairs headers with cells", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.KeyValuePairs };
                    List<List<string>> table = new List<List<string>>
                    {
                        new List<string> { "Name", "Age" },
                        new List<string> { "Alice", "30" }
                    };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Table, null, null, null, table, cfg);
                    Check.Single(result);
                    Check.Equal("Name: Alice, Age: 30", result[0].Text);
                })
                .Case("Route.Table.TextFallback", "An unknown table strategy falls back to markdown text chunking", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Table, null, null, null, Table(), cfg);
                    Check.NotEmpty(result);
                    Check.Contains("Name", result[0].Text!);
                })
                .Case("Route.Binary.WithText", "Binary type with text is chunked as text", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Binary, "extracted text from binary", null, null, null, cfg);
                    Check.NotEmpty(result);
                })
                .Case("Route.Binary.NoText", "Binary type with no text yields no chunks", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Binary, null, null, null, null, cfg);
                    Check.Empty(result);
                })
                .Case("Route.Image.WithText", "Image type with OCR text is chunked as text", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.Image, "OCR extracted text", null, null, null, cfg);
                    Check.NotEmpty(result);
                })
                .Case("Validation.NullConfig", "A null configuration throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => engine.Chunk(AtomTypeEnum.Text, "test", null, null, null, null!));
                })
                .Case("ContextPrefix.Prepended", "A context prefix is prepended to every chunk", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry, ContextPrefix = "DOC: " };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "Item1", "Item2" }, null, cfg);
                    Check.Equal(2, result.Count);
                    Check.StartsWith("DOC: ", result[0].Text!);
                    Check.StartsWith("DOC: ", result[1].Text!);
                })
                .Case("ContextPrefix.NotPrepended", "No prefix leaves chunk text unchanged", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry, ContextPrefix = null };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "Item1" }, null, cfg);
                    Check.Equal("Item1", result[0].Text);
                })
                .Case("Position.Sequential", "Chunk positions are assigned sequentially", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "A", "B", "C", "D" }, null, cfg);
                    for (int i = 0; i < result.Count; i++) Check.Equal(i, result[i].Position);
                })
                .Case("Hashes.Populated", "Every produced chunk carries populated hashes and a positive length", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry };
                    List<Chunk> result = engine.Chunk(AtomTypeEnum.List, null, null, new List<string> { "A", "B" }, null, cfg);
                    foreach (Chunk chunk in result)
                    {
                        Check.NotNull(chunk.MD5Hash);
                        Check.NotNull(chunk.SHA1Hash);
                        Check.NotNull(chunk.SHA256Hash);
                        Check.True(chunk.Length > 0);
                    }
                })
                .Case("Empty.NullTable", "A null table yields no chunks", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.Row };
                    Check.Empty(engine.Chunk(AtomTypeEnum.Table, null, null, null, null, cfg));
                })
                .Case("Empty.NullList", "A null list yields no chunks", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.ListEntry };
                    Check.Empty(engine.Chunk(AtomTypeEnum.List, null, null, null, null, cfg));
                })
                .Case("Empty.EmptyText", "Empty text yields no chunks", () =>
                {
                    ChunkingConfiguration cfg = new ChunkingConfiguration { Strategy = ChunkStrategyEnum.FixedTokenCount, FixedTokenCount = 100 };
                    Check.Empty(engine.Chunk(AtomTypeEnum.Text, "", null, null, null, cfg));
                })
                .Case("SerializableDataTableToList.Null", "SerializableDataTableToList returns an empty list for null input", () =>
                {
                    List<List<string>> result = ChunkingEngine.SerializableDataTableToList(null!);
                    Check.NotNull(result);
                    Check.Empty(result);
                })
                .Build("Core: ChunkingEngine");
        }

        private static List<List<string>> Table()
        {
            return new List<List<string>>
            {
                new List<string> { "Name", "Age" },
                new List<string> { "Alice", "30" },
                new List<string> { "Bob", "25" }
            };
        }
    }
}
