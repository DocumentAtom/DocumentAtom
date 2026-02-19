namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using SharpToken;

    public class RegexChunkerTests
    {
        private readonly GptEncoding _Encoding = GptEncoding.GetEncoding("cl100k_base");

        [Fact]
        public void Chunk_EmptyText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"\n"
            };
            List<string> result = RegexChunker.Chunk("", config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullText_ReturnsEmpty()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"\n"
            };
            List<string> result = RegexChunker.Chunk(null!, config, _Encoding);

            Assert.Empty(result);
        }

        [Fact]
        public void Chunk_NullPattern_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = null
            };

            Assert.Throws<ArgumentException>(() => RegexChunker.Chunk("some text", config, _Encoding));
        }

        [Fact]
        public void Chunk_EmptyPattern_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = ""
            };

            Assert.Throws<ArgumentException>(() => RegexChunker.Chunk("some text", config, _Encoding));
        }

        [Fact]
        public void Chunk_SplitsOnNewline()
        {
            string text = "Line one\nLine two\nLine three";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"\n"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            Assert.Equal(3, result.Count);
            Assert.Equal("Line one", result[0]);
            Assert.Equal("Line two", result[1]);
            Assert.Equal("Line three", result[2]);
        }

        [Fact]
        public void Chunk_SplitsOnCustomDelimiter()
        {
            string text = "Section A---Section B---Section C";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"---"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            Assert.Equal(3, result.Count);
            Assert.Equal("Section A", result[0]);
            Assert.Equal("Section B", result[1]);
            Assert.Equal("Section C", result[2]);
        }

        [Fact]
        public void Chunk_FiltersWhitespaceOnlySegments()
        {
            string text = "Content\n\n\n\nMore content";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"\n"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            foreach (string segment in result)
            {
                Assert.False(string.IsNullOrWhiteSpace(segment));
            }
        }

        [Fact]
        public void Chunk_NoMatch_ReturnsWholeText()
        {
            string text = "No delimiters here";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"---"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            Assert.Single(result);
            Assert.Equal("No delimiters here", result[0]);
        }

        [Fact]
        public void Chunk_TrimsSegments()
        {
            string text = "  A  \n  B  \n  C  ";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"\n"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            foreach (string segment in result)
            {
                Assert.Equal(segment.Trim(), segment);
            }
        }

        [Fact]
        public void Chunk_MultilinePattern_Works()
        {
            string text = "Header\n=====\nContent\n=====\nFooter";
            ChunkingConfiguration config = new ChunkingConfiguration
            {
                Strategy = ChunkStrategyEnum.RegexBased,
                RegexPattern = @"=====\n?"
            };
            List<string> result = RegexChunker.Chunk(text, config, _Encoding);

            Assert.True(result.Count >= 2);
        }
    }
}
