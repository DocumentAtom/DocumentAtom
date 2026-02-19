namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;

    public class ChunkTests
    {
        [Fact]
        public void FromText_SetsPositionAndLength()
        {
            Chunk chunk = Chunk.FromText("hello world", 3);

            Assert.Equal(3, chunk.Position);
            Assert.Equal(11, chunk.Length);
            Assert.Equal("hello world", chunk.Text);
        }

        [Fact]
        public void FromText_ComputesMD5Hash()
        {
            Chunk chunk = Chunk.FromText("test", 0);

            Assert.NotNull(chunk.MD5Hash);
            Assert.True(chunk.MD5Hash.Length > 0);
        }

        [Fact]
        public void FromText_ComputesSHA1Hash()
        {
            Chunk chunk = Chunk.FromText("test", 0);

            Assert.NotNull(chunk.SHA1Hash);
            Assert.True(chunk.SHA1Hash.Length > 0);
        }

        [Fact]
        public void FromText_ComputesSHA256Hash()
        {
            Chunk chunk = Chunk.FromText("test", 0);

            Assert.NotNull(chunk.SHA256Hash);
            Assert.True(chunk.SHA256Hash.Length > 0);
        }

        [Fact]
        public void FromText_SameInputProducesSameHashes()
        {
            Chunk chunk1 = Chunk.FromText("identical text", 0);
            Chunk chunk2 = Chunk.FromText("identical text", 5);

            Assert.Equal(chunk1.MD5Hash, chunk2.MD5Hash);
            Assert.Equal(chunk1.SHA1Hash, chunk2.SHA1Hash);
            Assert.Equal(chunk1.SHA256Hash, chunk2.SHA256Hash);
        }

        [Fact]
        public void FromText_DifferentInputProducesDifferentHashes()
        {
            Chunk chunk1 = Chunk.FromText("text one", 0);
            Chunk chunk2 = Chunk.FromText("text two", 0);

            Assert.NotEqual(chunk1.MD5Hash, chunk2.MD5Hash);
            Assert.NotEqual(chunk1.SHA256Hash, chunk2.SHA256Hash);
        }

        [Fact]
        public void FromText_NullText_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => Chunk.FromText(null!, 0));
        }

        [Fact]
        public void FromText_NegativePosition_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => Chunk.FromText("text", -1));
        }

        [Fact]
        public void Position_NegativeValue_Throws()
        {
            Chunk chunk = new Chunk();
            Assert.Throws<ArgumentOutOfRangeException>(() => chunk.Position = -1);
        }

        [Fact]
        public void Length_NegativeValue_Throws()
        {
            Chunk chunk = new Chunk();
            Assert.Throws<ArgumentOutOfRangeException>(() => chunk.Length = -1);
        }

        [Fact]
        public void FromText_EmptyString_SetsLengthZero()
        {
            Chunk chunk = Chunk.FromText("", 0);

            Assert.Equal(0, chunk.Length);
            Assert.Equal("", chunk.Text);
        }

        [Fact]
        public void ToString_ContainsPositionAndLength()
        {
            Chunk chunk = Chunk.FromText("hello", 2);
            string result = chunk.ToString();

            Assert.Contains("Position", result);
            Assert.Contains("2", result);
            Assert.Contains("Length", result);
            Assert.Contains("5", result);
        }
    }
}
