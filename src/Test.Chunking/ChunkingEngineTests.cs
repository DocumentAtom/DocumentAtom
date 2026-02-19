namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;

    public class ChunkingEngineTests
    {
        private readonly ChunkingEngine _Engine = new ChunkingEngine();

        #region Routing by Type

        [Fact]
        public void Chunk_TextType_ReturnsChunks()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Enable = true,
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 10
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Text,
                "Hello world this is a test string for chunking",
                null, null, null, config);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void Chunk_CodeType_RoutesToTextChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Enable = true,
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Code,
                "var x = 1;",
                null, null, null, config);

            Assert.NotEmpty(result);
            Assert.Contains("var x = 1;", result[0].Text);
        }

        [Fact]
        public void Chunk_HyperlinkType_RoutesToTextChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Hyperlink,
                "https://example.com",
                null, null, null, config);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void Chunk_ListType_WholeList_RoutesToWholeListChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.WholeList
            };

            List<string> items = new List<string> { "A", "B", "C" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            Assert.Single(result);
            Assert.Contains("- A", result[0].Text);
        }

        [Fact]
        public void Chunk_ListType_ListEntry_RoutesToListEntryChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry
            };

            List<string> items = new List<string> { "Apple", "Banana", "Cherry" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            Assert.Equal(3, result.Count);
            Assert.Equal("Apple", result[0].Text);
        }

        [Fact]
        public void Chunk_ListType_OrderedList_Preferred()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.WholeList
            };

            List<string> ordered = new List<string> { "First", "Second" };
            List<string> unordered = new List<string> { "A", "B" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, ordered, unordered, null, config);

            Assert.Single(result);
            Assert.Contains("1. First", result[0].Text);
        }

        [Fact]
        public void Chunk_TableType_Row_RoutesToTableChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.Row
            };

            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" },
                new List<string> { "Alice", "30" },
                new List<string> { "Bob", "25" }
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Table,
                null, null, null, table, config);

            Assert.Equal(2, result.Count);
            Assert.Equal("Alice 30", result[0].Text);
        }

        [Fact]
        public void Chunk_TableType_KeyValuePairs_RoutesToTableChunker()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.KeyValuePairs
            };

            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" },
                new List<string> { "Alice", "30" }
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Table,
                null, null, null, table, config);

            Assert.Single(result);
            Assert.Equal("Name: Alice, Age: 30", result[0].Text);
        }

        [Fact]
        public void Chunk_BinaryType_WithText_ChunksText()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Binary,
                "extracted text from binary",
                null, null, null, config);

            Assert.NotEmpty(result);
        }

        [Fact]
        public void Chunk_BinaryType_NoText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Binary,
                null, null, null, null, config);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_ImageType_WithText_ChunksText()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Image,
                "OCR extracted text",
                null, null, null, config);

            Assert.NotEmpty(result);
        }

        #endregion

        #region Config Validation

        [Fact]
        public void Chunk_NullConfig_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                _Engine.Chunk(AtomTypeEnum.Text, "test", null, null, null, null!));
        }

        #endregion

        #region Context Prefix

        [Fact]
        public void Chunk_ContextPrefix_PrependedToAllChunks()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry,
                ContextPrefix = "DOC: "
            };

            List<string> items = new List<string> { "Item1", "Item2" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            Assert.Equal(2, result.Count);
            Assert.StartsWith("DOC: ", result[0].Text);
            Assert.StartsWith("DOC: ", result[1].Text);
        }

        [Fact]
        public void Chunk_NoContextPrefix_NotPrepended()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry,
                ContextPrefix = null
            };

            List<string> items = new List<string> { "Item1" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            Assert.Equal("Item1", result[0].Text);
        }

        #endregion

        #region Position Tracking

        [Fact]
        public void Chunk_PositionIndices_AreSequential()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry
            };

            List<string> items = new List<string> { "A", "B", "C", "D" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            for (int i = 0; i < result.Count; i++)
            {
                Assert.Equal(i, result[i].Position);
            }
        }

        [Fact]
        public void Chunk_AllChunksHaveHashes()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry
            };

            List<string> items = new List<string> { "A", "B" };
            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, items, null, config);

            foreach (Chunk chunk in result)
            {
                Assert.NotNull(chunk.MD5Hash);
                Assert.NotNull(chunk.SHA1Hash);
                Assert.NotNull(chunk.SHA256Hash);
                Assert.True(chunk.Length > 0);
            }
        }

        #endregion

        #region Table Fallback

        [Fact]
        public void Chunk_TableType_TextStrategy_FallsBackToTextChunking()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" },
                new List<string> { "Alice", "30" }
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Table,
                null, null, null, table, config);

            Assert.NotEmpty(result);
            Assert.Contains("Name", result[0].Text);
        }

        #endregion

        #region Empty Data

        [Fact]
        public void Chunk_NullTable_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.Row
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Table,
                null, null, null, null, config);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullList_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ListEntry
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.List,
                null, null, null, null, config);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_EmptyText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.FixedTokenCount,
                FixedTokenCount = 100
            };

            List<Chunk> result = _Engine.Chunk(
                AtomTypeEnum.Text,
                "", null, null, null, config);

            Assert.Empty(result);
        }

        #endregion
    }
}
