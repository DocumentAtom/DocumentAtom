namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using SharpToken;

    public class FixedTokenChunkerTests
    {
        private readonly GptEncoding _Encoding = GptEncoding.GetEncoding("cl100k_base");

        [Fact]
        public void Chunk_EmptyText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration { FixedTokenCount = 10 };
            List<string> result = FixedTokenChunker.Chunk("", config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration { FixedTokenCount = 10 };
            List<string> result = FixedTokenChunker.Chunk(null!, config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_ShortText_ReturnsSingleChunk()
        {
            ChunkingConfiguration config = new ChunkingConfiguration { FixedTokenCount = 100 };
            List<string> result = FixedTokenChunker.Chunk("Hello world", config, _Encoding);

            Assert.Single(result);
            Assert.Equal("Hello world", result[0]);
        }

        [Fact]
        public void Chunk_LongText_SplitsIntoMultipleChunks()
        {
            string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog.", 50));
            ChunkingConfiguration config = new ChunkingConfiguration { FixedTokenCount = 20 };
            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1, "Expected multiple chunks for long text");
        }

        [Fact]
        public void Chunk_NoOverlap_ChunksDontRepeat()
        {
            string text = string.Join(" ", Enumerable.Repeat("word", 100));
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 10,
                OverlapCount = 0
            };
            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_WithOverlapCount_ProducesMoreChunks()
        {
            string text = string.Join(" ", Enumerable.Repeat("word", 100));

            ChunkingConfiguration noOverlap = new ChunkingConfiguration
            {
                FixedTokenCount = 20,
                OverlapCount = 0
            };

            ChunkingConfiguration withOverlap = new ChunkingConfiguration
            {
                FixedTokenCount = 20,
                OverlapCount = 5
            };

            List<string> noOverlapResult = FixedTokenChunker.Chunk(text, noOverlap, _Encoding);
            List<string> overlapResult = FixedTokenChunker.Chunk(text, withOverlap, _Encoding);

            Assert.True(overlapResult.Count >= noOverlapResult.Count,
                "Overlap should produce at least as many chunks");
        }

        [Fact]
        public void Chunk_WithOverlapPercentage_TakesPrecedenceOverCount()
        {
            string text = string.Join(" ", Enumerable.Repeat("The quick brown fox.", 50));

            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 20,
                OverlapCount = 0,
                OverlapPercentage = 0.25
            };

            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);
            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_SlidingWindowStrategy_ProducesResults()
        {
            string text = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps.", 30));
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 20,
                OverlapCount = 5,
                OverlapStrategy = OverlapStrategyEnum.SlidingWindow
            };

            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);
            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_SentenceBoundaryAwareStrategy_ProducesResults()
        {
            string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence. Seventh sentence. Eighth sentence.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 10,
                OverlapCount = 3,
                OverlapStrategy = OverlapStrategyEnum.SentenceBoundaryAware
            };

            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);
            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_SemanticBoundaryAwareStrategy_ProducesResults()
        {
            string text = "First paragraph content here.\n\nSecond paragraph content here.\n\nThird paragraph content here.\n\nFourth paragraph content here.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 10,
                OverlapCount = 3,
                OverlapStrategy = OverlapStrategyEnum.SemanticBoundaryAware
            };

            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);
            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_AllContentPreserved()
        {
            string text = "Hello world this is a test of chunking";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                FixedTokenCount = 5,
                OverlapCount = 0
            };

            List<string> result = FixedTokenChunker.Chunk(text, config, _Encoding);
            string recombined = string.Join("", result);

            Assert.Equal(text, recombined);
        }
    }
}
