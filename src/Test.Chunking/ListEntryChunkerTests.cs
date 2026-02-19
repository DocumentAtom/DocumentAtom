namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;

    public class ListEntryChunkerTests
    {
        [Fact]
        public void Chunk_NullItems_ReturnsEmpty()
        {
            List<string> result = ListEntryChunker.Chunk(null!);
            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_EmptyItems_ReturnsEmpty()
        {
            List<string> result = ListEntryChunker.Chunk(new List<string>());
            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_EachItemBecomesChunk()
        {
            List<string> items = new List<string> { "Apple", "Banana", "Cherry" };
            List<string> result = ListEntryChunker.Chunk(items);

            Assert.Equal(3, result.Count);
            Assert.Equal("Apple", result[0]);
            Assert.Equal("Banana", result[1]);
            Assert.Equal("Cherry", result[2]);
        }

        [Fact]
        public void Chunk_FiltersWhitespaceItems()
        {
            List<string> items = new List<string> { "Apple", "  ", "Cherry", "", "\t" };
            List<string> result = ListEntryChunker.Chunk(items);

            Assert.Equal(2, result.Count);
            Assert.Equal("Apple", result[0]);
            Assert.Equal("Cherry", result[1]);
        }

        [Fact]
        public void Chunk_AllWhitespace_ReturnsEmpty()
        {
            List<string> items = new List<string> { "  ", "", "\t", "\n" };
            List<string> result = ListEntryChunker.Chunk(items);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_SingleItem_ReturnsSingleChunk()
        {
            List<string> items = new List<string> { "Only item" };
            List<string> result = ListEntryChunker.Chunk(items);

            Assert.Single(result);
            Assert.Equal("Only item", result[0]);
        }
    }
}
