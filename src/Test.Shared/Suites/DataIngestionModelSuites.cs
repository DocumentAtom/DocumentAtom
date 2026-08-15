namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.DataIngestion;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Processors;
    using DocumentAtom.DataIngestion.Readers;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the DataIngestion model records and the options/factory classes.
    /// </summary>
    internal static class DataIngestionModelSuites
    {
        internal static TestSuiteDescriptor Models()
        {
            return new SuiteBuilder("DataIngestion.Models")
                .Case("Chunk.Defaults", "A new IngestionChunk has sensible defaults", () =>
                {
                    IngestionChunk c = new IngestionChunk();
                    Check.NotNull(c.Id);
                    Check.Null(c.DocumentId);
                    Check.Equal(0, c.ChunkIndex);
                    Check.Equal(string.Empty, c.Content);
                    Check.NotNull(c.Metadata);
                    Check.Null(c.Embedding);
                })
                .Case("Chunk.Constructor", "The content constructor assigns its arguments", () =>
                {
                    IngestionChunk c = new IngestionChunk("body", "doc-1", 4);
                    Check.Equal("body", c.Content);
                    Check.Equal("doc-1", c.DocumentId);
                    Check.Equal(4, c.ChunkIndex);
                })
                .Case("Chunk.NullContentCoerced", "Null content is coerced to an empty string", () =>
                {
                    IngestionChunk c = new IngestionChunk(null!);
                    Check.Equal(string.Empty, c.Content);
                })
                .Case("Chunk.UniqueIds", "Each chunk receives a distinct identifier", () =>
                {
                    Check.NotEqual(new IngestionChunk().Id, new IngestionChunk().Id);
                })
                .Case("Document.Defaults", "A new IngestionDocument has empty collections", () =>
                {
                    IngestionDocument d = new IngestionDocument();
                    Check.NotNull(d.Id);
                    Check.Empty(d.Sections);
                    Check.Empty(d.Elements);
                    Check.Empty(d.Metadata);
                })
                .Case("Element.Defaults", "A new element defaults to a paragraph with null content", () =>
                {
                    IngestionDocumentElement e = new IngestionDocumentElement();
                    Check.Equal(IngestionElementType.Paragraph, e.ElementType);
                    Check.Null(e.Content);
                    Check.Null(e.BinaryContent);
                })
                .Case("Section.Defaults", "A new section has no page number and empty elements", () =>
                {
                    IngestionDocumentSection s = new IngestionDocumentSection();
                    Check.Null(s.PageNumber);
                    Check.Null(s.Title);
                    Check.Empty(s.Elements);
                })
                .Build("DataIngestion: Models");
        }

        internal static TestSuiteDescriptor ChunkerOptions()
        {
            return new SuiteBuilder("DataIngestion.AtomChunkerOptions")
                .Case("Defaults", "Default chunker options are correct", () =>
                {
                    AtomChunkerOptions o = new AtomChunkerOptions();
                    Check.NotNull(o.Chunking);
                    Check.True(o.Chunking.Enable);
                    Check.Equal(256, o.Chunking.FixedTokenCount);
                    Check.True(o.IncludeHeaderContext);
                    Check.Equal(": ", o.HeaderContextSeparator);
                    Check.True(o.PreserveElementMetadata);
                    Check.True(o.UseQuarksIfAvailable);
                    Check.Equal(50, o.MinChunkSize);
                })
                .Case("Validation.NullChunking", "Chunking rejects null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new AtomChunkerOptions().Chunking = null!);
                })
                .Case("Validation.MinChunkSize", "MinChunkSize rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new AtomChunkerOptions().MinChunkSize = -1);
                })
                .Case("Factory.ForRag", "ForRag configures sentence-based chunking with overlap", () =>
                {
                    AtomChunkerOptions o = AtomChunkerOptions.ForRag();
                    Check.Equal(ChunkStrategyEnum.SentenceBased, o.Chunking.Strategy);
                    Check.Equal(2, o.Chunking.OverlapCount);
                    Check.True(o.UseQuarksIfAvailable);
                })
                .Case("Factory.ForSummarization", "ForSummarization configures paragraph-based chunking", () =>
                {
                    AtomChunkerOptions o = AtomChunkerOptions.ForSummarization();
                    Check.Equal(ChunkStrategyEnum.ParagraphBased, o.Chunking.Strategy);
                    Check.Equal(1024, o.Chunking.FixedTokenCount);
                    Check.False(o.UseQuarksIfAvailable);
                })
                .Case("Factory.ForLargeContext", "ForLargeContext uses a large token window", () =>
                {
                    AtomChunkerOptions o = AtomChunkerOptions.ForLargeContext();
                    Check.Equal(ChunkStrategyEnum.ParagraphBased, o.Chunking.Strategy);
                    Check.Equal(2048, o.Chunking.FixedTokenCount);
                })
                .Build("DataIngestion: AtomChunkerOptions");
        }

        internal static TestSuiteDescriptor ProcessorOptions()
        {
            return new SuiteBuilder("DataIngestion.AtomDocumentProcessorOptions")
                .Case("Defaults", "Default processor options are correct", () =>
                {
                    AtomDocumentProcessorOptions o = new AtomDocumentProcessorOptions();
                    Check.NotNull(o.ReaderOptions);
                    Check.NotNull(o.ChunkerOptions);
                    Check.True(o.UseHierarchyAwareChunking);
                    Check.False(o.RemoveDuplicates);
                    Check.True(o.SkipEmptyChunks);
                    Check.Equal(10, o.MinimumChunkLength);
                })
                .Case("Validation.MinimumChunkLength", "MinimumChunkLength rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new AtomDocumentProcessorOptions().MinimumChunkLength = -1);
                })
                .Case("Factory.ForRag", "ForRag enables duplicate removal", () =>
                {
                    AtomDocumentProcessorOptions o = AtomDocumentProcessorOptions.ForRag();
                    Check.True(o.RemoveDuplicates);
                })
                .Case("Factory.ForLargeContext", "ForLargeContext raises the minimum chunk length", () =>
                {
                    AtomDocumentProcessorOptions o = AtomDocumentProcessorOptions.ForLargeContext();
                    Check.Equal(20, o.MinimumChunkLength);
                })
                .Build("DataIngestion: AtomDocumentProcessorOptions");
        }

        internal static TestSuiteDescriptor ReaderOptions()
        {
            return new SuiteBuilder("DataIngestion.AtomDocumentReaderOptions")
                .Case("Defaults", "Default reader options are correct", () =>
                {
                    AtomDocumentReaderOptions o = new AtomDocumentReaderOptions();
                    Check.NotNull(o.TempDirectory);
                    Check.True(o.PreserveFullAtomData);
                    Check.True(o.EnableOcr);
                    Check.True(o.BuildHierarchy);
                    Check.True(o.IncludeBinaryContent);
                    Check.Equal(0L, o.MaxFileSizeBytes);
                    Check.Null(o.ChunkingSettings);
                    Check.NotNull(o.ProcessorSettings);
                    Check.NotNull(o.ExcludedMetadataKeys);
                })
                .Case("Validation.TempDirectory", "TempDirectory rejects null", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new AtomDocumentReaderOptions().TempDirectory = null!);
                })
                .Case("Validation.MaxFileSizeBytes", "MaxFileSizeBytes rejects negatives", () =>
                {
                    Check.Throws<ArgumentOutOfRangeException>(() => new AtomDocumentReaderOptions().MaxFileSizeBytes = -1);
                })
                .Build("DataIngestion: AtomDocumentReaderOptions");
        }
    }
}
