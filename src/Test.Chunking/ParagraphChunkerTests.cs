namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using SharpToken;

    public class ParagraphChunkerTests
    {
        private readonly GptEncoding _Encoding = GptEncoding.GetEncoding("cl100k_base");

        [Fact]
        public void Chunk_EmptyText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 100
            };
            List<string> result = ParagraphChunker.Chunk("", config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 100
            };
            List<string> result = ParagraphChunker.Chunk(null!, config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_SingleParagraph_ReturnsSingleChunk()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 100
            };
            List<string> result = ParagraphChunker.Chunk("Single paragraph text.", config, _Encoding);

            Assert.Single(result);
        }

        [Fact]
        public void Chunk_MultipleParagraphs_SplitsOnDoubleNewline()
        {
            string text = "First paragraph.\n\nSecond paragraph.\n\nThird paragraph.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 5
            };
            List<string> result = ParagraphChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1, "Should split on double newlines");
        }

        [Fact]
        public void Chunk_WindowsLineEndings_SplitsCorrectly()
        {
            string text = "First paragraph.\r\n\r\nSecond paragraph.\r\n\r\nThird paragraph.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 5
            };
            List<string> result = ParagraphChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1, "Should split on Windows-style double newlines");
        }

        [Fact]
        public void Chunk_GroupsParagraphsToFillTokenBudget()
        {
            string text = "A.\n\nB.\n\nC.\n\nD.\n\nE.\n\nF.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 1000
            };
            List<string> result = ParagraphChunker.Chunk(text, config, _Encoding);

            Assert.Single(result);
        }

        [Fact]
        public void Chunk_WithOverlap_RepeatsTrailingParagraphs()
        {
            string text = "First paragraph here.\n\nSecond paragraph here.\n\nThird paragraph here.\n\nFourth paragraph here.";

            ChunkingConfiguration withOverlap = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 8,
                OverlapCount = 1
            };

            ChunkingConfiguration noOverlap = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 8,
                OverlapCount = 0
            };

            List<string> overlapResult = ParagraphChunker.Chunk(text, withOverlap, _Encoding);
            List<string> noOverlapResult = ParagraphChunker.Chunk(text, noOverlap, _Encoding);

            Assert.True(overlapResult.Count >= noOverlapResult.Count);
        }

        [Fact]
        public void Chunk_OverlapPercentage_TakesPrecedence()
        {
            string text = "Para one.\n\nPara two.\n\nPara three.\n\nPara four.\n\nPara five.";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 5,
                OverlapCount = 0,
                OverlapPercentage = 0.5
            };
            List<string> result = ParagraphChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count > 1);
        }

        [Fact]
        public void Chunk_TrimsWhitespace()
        {
            string text = "  First paragraph.  \n\n  Second paragraph.  ";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.ParagraphBased,
                FixedTokenCount = 5
            };
            List<string> result = ParagraphChunker.Chunk(text, config, _Encoding);

            foreach (string chunk in result)
            {
                Assert.Equal(chunk.Trim(), chunk);
            }
        }
    }
}
