namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;

    public class WholeListChunkerTests
    {
        [Fact]
        public void Chunk_NullItems_ReturnsEmpty()
        {
            List<string> result = WholeListChunker.Chunk(null!, true);
            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_EmptyItems_ReturnsEmpty()
        {
            List<string> result = WholeListChunker.Chunk(new List<string>(), false);
            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_Ordered_ProducesNumberedList()
        {
            List<string> items = new List<string> { "First", "Second", "Third" };
            List<string> result = WholeListChunker.Chunk(items, ordered: true);

            Assert.Single(result);
            Assert.Contains("1. First", result[0]);
            Assert.Contains("2. Second", result[0]);
            Assert.Contains("3. Third", result[0]);
        }

        [Fact]
        public void Chunk_Unordered_ProducesBulletedList()
        {
            List<string> items = new List<string> { "Apple", "Banana", "Cherry" };
            List<string> result = WholeListChunker.Chunk(items, ordered: false);

            Assert.Single(result);
            Assert.Contains("- Apple", result[0]);
            Assert.Contains("- Banana", result[0]);
            Assert.Contains("- Cherry", result[0]);
        }

        [Fact]
        public void Chunk_ReturnsSingleChunk()
        {
            List<string> items = new List<string> { "A", "B", "C", "D", "E" };
            List<string> result = WholeListChunker.Chunk(items, true);

            Assert.Single(result);
        }

        [Fact]
        public void Chunk_JoinedByNewlines()
        {
            List<string> items = new List<string> { "A", "B" };
            List<string> result = WholeListChunker.Chunk(items, false);

            Assert.Contains("\n", result[0]);
        }

        [Fact]
        public void Chunk_SingleItem_Ordered()
        {
            List<string> items = new List<string> { "Only" };
            List<string> result = WholeListChunker.Chunk(items, ordered: true);

            Assert.Single(result);
            Assert.Equal("1. Only", result[0]);
        }

        [Fact]
        public void Chunk_SingleItem_Unordered()
        {
            List<string> items = new List<string> { "Only" };
            List<string> result = WholeListChunker.Chunk(items, ordered: false);

            Assert.Single(result);
            Assert.Equal("- Only", result[0]);
        }
    }
}
