namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;

    public class ChunkingConfigurationTests
    {
        [Fact]
        public void Defaults_AreCorrect()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();

            Assert.False(config.Enable);
            Assert.Equal(ChunkStrategyEnum.FixedTokenCount, config.Strategy);
            Assert.Equal(256, config.FixedTokenCount);
            Assert.Equal(0, config.OverlapCount);
            Assert.Null(config.OverlapPercentage);
            Assert.Equal(OverlapStrategyEnum.SlidingWindow, config.OverlapStrategy);
            Assert.Equal(5, config.RowGroupSize);
            Assert.Null(config.ContextPrefix);
            Assert.Null(config.RegexPattern);
        }

        [Fact]
        public void FixedTokenCount_BelowMinimum_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.FixedTokenCount = 0);
        }

        [Fact]
        public void FixedTokenCount_MinimumIsOne()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            config.FixedTokenCount = 1;
            Assert.Equal(1, config.FixedTokenCount);
        }

        [Fact]
        public void OverlapCount_Negative_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.OverlapCount = -1);
        }

        [Fact]
        public void OverlapCount_Zero_Allowed()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            config.OverlapCount = 0;
            Assert.Equal(0, config.OverlapCount);
        }

        [Fact]
        public void OverlapPercentage_BelowZero_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.OverlapPercentage = -0.1);
        }

        [Fact]
        public void OverlapPercentage_AboveOne_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.OverlapPercentage = 1.1);
        }

        [Fact]
        public void OverlapPercentage_ValidRange_Accepted()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            config.OverlapPercentage = 0.0;
            Assert.Equal(0.0, config.OverlapPercentage);

            config.OverlapPercentage = 0.5;
            Assert.Equal(0.5, config.OverlapPercentage);

            config.OverlapPercentage = 1.0;
            Assert.Equal(1.0, config.OverlapPercentage);
        }

        [Fact]
        public void OverlapPercentage_Null_Allowed()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            config.OverlapPercentage = 0.5;
            config.OverlapPercentage = null;
            Assert.Null(config.OverlapPercentage);
        }

        [Fact]
        public void RowGroupSize_BelowMinimum_Throws()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            Assert.Throws<ArgumentOutOfRangeException>(() => config.RowGroupSize = 0);
        }

        [Fact]
        public void RowGroupSize_MinimumIsOne()
        {
            ChunkingConfiguration config = new ChunkingConfiguration();
            config.RowGroupSize = 1;
            Assert.Equal(1, config.RowGroupSize);
        }
    }
}
