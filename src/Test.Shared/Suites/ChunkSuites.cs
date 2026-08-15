namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Core.Enums;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering <see cref="Chunk"/> and <see cref="ChunkingConfiguration"/>.
    /// </summary>
    internal static class ChunkSuites
    {
        internal static TestSuiteDescriptor Chunk()
        {
            return new SuiteBuilder("Core.Chunk")
                .Case("FromText.PositionAndLength", "FromText sets position, length, and text", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("hello world", 3);
                    Check.Equal(3, chunk.Position);
                    Check.Equal(11, chunk.Length);
                    Check.Equal("hello world", chunk.Text);
                })
                .Case("FromText.Md5", "FromText computes a 16-byte MD5 hash", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("test", 0);
                    Check.NotNull(chunk.MD5Hash);
                    Check.Equal(16, chunk.MD5Hash!.Length);
                })
                .Case("FromText.Sha1", "FromText computes a 20-byte SHA1 hash", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("test", 0);
                    Check.NotNull(chunk.SHA1Hash);
                    Check.Equal(20, chunk.SHA1Hash!.Length);
                })
                .Case("FromText.Sha256", "FromText computes a 32-byte SHA256 hash", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("test", 0);
                    Check.NotNull(chunk.SHA256Hash);
                    Check.Equal(32, chunk.SHA256Hash!.Length);
                })
                .Case("FromText.SameInputSameHash", "Identical text yields identical hashes regardless of position", () =>
                {
                    Chunk a = Core.Chunking.Chunk.FromText("identical text", 0);
                    Chunk b = Core.Chunking.Chunk.FromText("identical text", 5);
                    Check.True(HashEqual(a.MD5Hash, b.MD5Hash));
                    Check.True(HashEqual(a.SHA1Hash, b.SHA1Hash));
                    Check.True(HashEqual(a.SHA256Hash, b.SHA256Hash));
                })
                .Case("FromText.DifferentInputDifferentHash", "Different text yields different hashes", () =>
                {
                    Chunk a = Core.Chunking.Chunk.FromText("text one", 0);
                    Chunk b = Core.Chunking.Chunk.FromText("text two", 0);
                    Check.False(HashEqual(a.MD5Hash, b.MD5Hash));
                    Check.False(HashEqual(a.SHA256Hash, b.SHA256Hash));
                })
                .Case("FromText.EmptyString", "FromText allows an empty string and sets length zero", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("", 0);
                    Check.Equal(0, chunk.Length);
                    Check.Equal("", chunk.Text);
                    Check.NotNull(chunk.MD5Hash);
                })
                .Case("FromText.NullText", "FromText throws ArgumentNullException for null text", () =>
                {
                    Check.Throws<ArgumentNullException>(() => Core.Chunking.Chunk.FromText(null!, 0));
                })
                .Case("FromText.NegativePosition", "FromText throws ArgumentOutOfRangeException for negative position", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => Core.Chunking.Chunk.FromText("text", -1));
                })
                .Case("Position.Negative", "Position setter rejects negatives", () =>
                {
                    Chunk chunk = new Chunk();
                    Check.Throws<ArgumentOutOfRangeException>(() => chunk.Position = -1);
                })
                .Case("Length.Negative", "Length setter rejects negatives", () =>
                {
                    Chunk chunk = new Chunk();
                    Check.Throws<ArgumentOutOfRangeException>(() => chunk.Length = -1);
                })
                .Case("ToString.ContainsPositionAndLength", "ToString includes position and length labels", () =>
                {
                    Chunk chunk = Core.Chunking.Chunk.FromText("hello", 2);
                    string s = chunk.ToString();
                    Check.Contains("Position", s);
                    Check.Contains("Length", s);
                })
                .Build("Core: Chunk");
        }

        internal static TestSuiteDescriptor Configuration()
        {
            return new SuiteBuilder("Core.ChunkingConfiguration")
                .Case("Defaults", "Default configuration values are correct", () =>
                {
                    ChunkingConfiguration c = new ChunkingConfiguration();
                    Check.False(c.Enable);
                    Check.Equal(ChunkStrategyEnum.FixedTokenCount, c.Strategy);
                    Check.Equal(256, c.FixedTokenCount);
                    Check.Equal(0, c.OverlapCount);
                    Check.Null(c.OverlapPercentage);
                    Check.Equal(OverlapStrategyEnum.SlidingWindow, c.OverlapStrategy);
                    Check.Equal(5, c.RowGroupSize);
                    Check.Null(c.ContextPrefix);
                    Check.Null(c.RegexPattern);
                })
                .Case("FixedTokenCount.Zero", "FixedTokenCount rejects zero", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().FixedTokenCount = 0);
                })
                .Case("FixedTokenCount.Negative", "FixedTokenCount rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().FixedTokenCount = -5);
                })
                .Case("FixedTokenCount.One", "FixedTokenCount accepts the minimum of one", () =>
                {
                    ChunkingConfiguration c = new ChunkingConfiguration();
                    c.FixedTokenCount = 1;
                    Check.Equal(1, c.FixedTokenCount);
                })
                .Case("OverlapCount.Negative", "OverlapCount rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().OverlapCount = -1);
                })
                .Case("OverlapCount.Zero", "OverlapCount accepts zero", () =>
                {
                    ChunkingConfiguration c = new ChunkingConfiguration();
                    c.OverlapCount = 0;
                    Check.Equal(0, c.OverlapCount);
                })
                .Case("OverlapPercentage.BelowRange", "OverlapPercentage rejects values below 0.0", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().OverlapPercentage = -0.1);
                })
                .Case("OverlapPercentage.AboveRange", "OverlapPercentage rejects values above 1.0", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().OverlapPercentage = 1.1);
                })
                .Case("OverlapPercentage.Bounds", "OverlapPercentage accepts 0.0, 1.0, and null", () =>
                {
                    ChunkingConfiguration c = new ChunkingConfiguration();
                    c.OverlapPercentage = 0.0;
                    Check.Equal(0.0, c.OverlapPercentage);
                    c.OverlapPercentage = 1.0;
                    Check.Equal(1.0, c.OverlapPercentage);
                    c.OverlapPercentage = null;
                    Check.Null(c.OverlapPercentage);
                })
                .Case("RowGroupSize.Zero", "RowGroupSize rejects zero", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new ChunkingConfiguration().RowGroupSize = 0);
                })
                .Build("Core: ChunkingConfiguration");
        }

        private static bool HashEqual(byte[]? a, byte[]? b)
        {
            if (a == null || b == null) return a == b;
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
                if (a[i] != b[i]) return false;
            return true;
        }
    }
}
