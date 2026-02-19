namespace Test.Chunking
{
    using DocumentAtom.Core.Chunking;

    public class TableChunkerTests
    {
        private List<List<string>> CreateSampleTable()
        {
            return new List<List<string>>
            {
                new List<string> { "Name", "Age", "City" },
                new List<string> { "Alice", "30", "NYC" },
                new List<string> { "Bob", "25", "LA" },
                new List<string> { "Charlie", "35", "Chicago" }
            };
        }

        #region ChunkByRow

        [Fact]
        public void ChunkByRow_EmptyTable_ReturnsEmpty()
        {
            List<string> result = TableChunker.ChunkByRow(new List<List<string>>());
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByRow_HeaderOnly_ReturnsEmpty()
        {
            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" }
            };
            List<string> result = TableChunker.ChunkByRow(table);
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByRow_ReturnsSpaceSeparatedValues()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRow(table);

            Assert.Equal(3, result.Count);
            Assert.Equal("Alice 30 NYC", result[0]);
            Assert.Equal("Bob 25 LA", result[1]);
            Assert.Equal("Charlie 35 Chicago", result[2]);
        }

        #endregion

        #region ChunkByRowWithHeaders

        [Fact]
        public void ChunkByRowWithHeaders_EmptyTable_ReturnsEmpty()
        {
            List<string> result = TableChunker.ChunkByRowWithHeaders(new List<List<string>>());
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByRowWithHeaders_HeaderOnly_ReturnsEmpty()
        {
            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" }
            };
            List<string> result = TableChunker.ChunkByRowWithHeaders(table);
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByRowWithHeaders_ReturnsMarkdownTable()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRowWithHeaders(table);

            Assert.Equal(3, result.Count);

            Assert.Contains("| Name | Age | City |", result[0]);
            Assert.Contains("|---|---|---|", result[0]);
            Assert.Contains("| Alice | 30 | NYC |", result[0]);
        }

        [Fact]
        public void ChunkByRowWithHeaders_EachChunkHasHeaders()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRowWithHeaders(table);

            foreach (string chunk in result)
            {
                Assert.Contains("Name", chunk);
                Assert.Contains("Age", chunk);
                Assert.Contains("City", chunk);
            }
        }

        #endregion

        #region ChunkByRowGroupWithHeaders

        [Fact]
        public void ChunkByRowGroupWithHeaders_EmptyTable_ReturnsEmpty()
        {
            List<string> result = TableChunker.ChunkByRowGroupWithHeaders(new List<List<string>>(), 2);
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByRowGroupWithHeaders_GroupSize2_GroupsCorrectly()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRowGroupWithHeaders(table, 2);

            Assert.Equal(2, result.Count);
            Assert.Contains("Alice", result[0]);
            Assert.Contains("Bob", result[0]);
            Assert.Contains("Charlie", result[1]);
        }

        [Fact]
        public void ChunkByRowGroupWithHeaders_GroupSize1_SameAsRowWithHeaders()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> byGroup = TableChunker.ChunkByRowGroupWithHeaders(table, 1);
            List<string> byRow = TableChunker.ChunkByRowWithHeaders(table);

            Assert.Equal(byRow.Count, byGroup.Count);
        }

        [Fact]
        public void ChunkByRowGroupWithHeaders_GroupSizeLargerThanRows_SingleChunk()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRowGroupWithHeaders(table, 100);

            Assert.Single(result);
            Assert.Contains("Alice", result[0]);
            Assert.Contains("Bob", result[0]);
            Assert.Contains("Charlie", result[0]);
        }

        [Fact]
        public void ChunkByRowGroupWithHeaders_GroupSizeZero_ClampsToOne()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByRowGroupWithHeaders(table, 0);

            Assert.Equal(3, result.Count);
        }

        #endregion

        #region ChunkByKeyValuePairs

        [Fact]
        public void ChunkByKeyValuePairs_EmptyTable_ReturnsEmpty()
        {
            List<string> result = TableChunker.ChunkByKeyValuePairs(new List<List<string>>());
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByKeyValuePairs_HeaderOnly_ReturnsEmpty()
        {
            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" }
            };
            List<string> result = TableChunker.ChunkByKeyValuePairs(table);
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkByKeyValuePairs_FormatsAsKeyValue()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkByKeyValuePairs(table);

            Assert.Equal(3, result.Count);
            Assert.Equal("Name: Alice, Age: 30, City: NYC", result[0]);
            Assert.Equal("Name: Bob, Age: 25, City: LA", result[1]);
            Assert.Equal("Name: Charlie, Age: 35, City: Chicago", result[2]);
        }

        #endregion

        #region ChunkWholeTable

        [Fact]
        public void ChunkWholeTable_EmptyTable_ReturnsEmpty()
        {
            List<string> result = TableChunker.ChunkWholeTable(new List<List<string>>());
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkWholeTable_HeaderOnly_ReturnsEmpty()
        {
            List<List<string>> table = new List<List<string>>
            {
                new List<string> { "Name", "Age" }
            };
            List<string> result = TableChunker.ChunkWholeTable(table);
            Assert.Empty(result);
        }

        [Fact]
        public void ChunkWholeTable_ReturnsSingleMarkdownTable()
        {
            List<List<string>> table = CreateSampleTable();
            List<string> result = TableChunker.ChunkWholeTable(table);

            Assert.Single(result);
            string markdown = result[0];
            Assert.Contains("| Name | Age | City |", markdown);
            Assert.Contains("|---|---|---|", markdown);
            Assert.Contains("| Alice | 30 | NYC |", markdown);
            Assert.Contains("| Bob | 25 | LA |", markdown);
            Assert.Contains("| Charlie | 35 | Chicago |", markdown);
        }

        #endregion
    }
}
