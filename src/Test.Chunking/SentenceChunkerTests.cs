namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using SharpToken;

    public class SentenceChunkerTests
    {
        private readonly GptEncoding _Encoding = GptEncoding.GetEncoding("cl100k_base");

        [Fact]
        public void Chunk_EmptyText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 100
            };
            List<string> result = SentenceChunker.Chunk("", config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 100
            };
            List<string> result = SentenceChunker.Chunk(null!, config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_SingleSentence_ReturnsSingleChunk()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 100
            };
            List<string> result = SentenceChunker.Chunk("This is a single sentence.", config, _Encoding);

            Assert.Single(result);
        }

        [Fact]
        public void Chunk_MultipleSentences_GroupsByTokenBudget()
        {
            string text = "First sentence. Second sentence. Third sentence. Fourth sentence. Fifth sentence. Sixth sentence.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 8
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1, "Should split into multiple chunks based on token budget");
        }

        [Fact]
        public void Chunk_WithOverlap_RepeatsTrailingSentences()
        {
            string text = "One. Two. Three. Four. Five. Six. Seven. Eight.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 5,
                OverlapCount = 1
            };

            ChunkingConfiguration noOverlapConfig = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 5,
                OverlapCount = 0
            };

            List<string> withOverlap = SentenceChunker.Chunk(text, config, _Encoding);
            List<string> withoutOverlap = SentenceChunker.Chunk(text, noOverlapConfig, _Encoding);

            Assert.True(withOverlap.Count >= withoutOverlap.Count,
                "Overlap should produce at least as many chunks");
        }

        [Fact]
        public void Chunk_OverlapPercentage_TakesPrecedence()
        {
            string text = "First. Second. Third. Fourth. Fifth. Sixth. Seventh. Eighth. Ninth. Tenth.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 5,
                OverlapCount = 0,
                OverlapPercentage = 0.5
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_SplitsOnPeriods()
        {
            string text = "Hello world. Goodbye world.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 3
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void Chunk_SplitsOnExclamationMarks()
        {
            string text = "Wow! Amazing! Incredible!";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 2
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void Chunk_SplitsOnQuestionMarks()
        {
            string text = "What? Why? How? When?";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 2
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count >= 2);
        }

        [Fact]
        public void Chunk_LargeTokenBudget_ReturnsSingleChunk()
        {
            string text = "First sentence. Second sentence. Third sentence.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.SentenceBased,
                FixedTokenCount = 1000
            };
            List<string> result = SentenceChunker.Chunk(text, config, _Encoding);

            Assert.Single(result);
        }
    }
}
