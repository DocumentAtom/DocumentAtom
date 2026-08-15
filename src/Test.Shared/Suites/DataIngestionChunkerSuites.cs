namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using DocumentAtom.DataIngestion;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Metadata;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering <see cref="AtomChunker"/> and <see cref="HierarchyAwareChunker"/> over
    /// in-memory ingestion documents.
    /// </summary>
    internal static class DataIngestionChunkerSuites
    {
        private const string LongParagraph =
            "This is a sufficiently long paragraph of content that comfortably exceeds the minimum " +
            "chunk size so that the chunker emits at least one chunk for downstream processing.";

        internal static TestSuiteDescriptor AtomChunkerSuite()
        {
            return new SuiteBuilder("DataIngestion.AtomChunker")
                .Case("Chunk.ProducesChunks", "Chunking a document produces chunks carrying the document id", () =>
                {
                    AtomChunker chunker = new AtomChunker();
                    List<IngestionChunk> chunks = chunker.Chunk(BuildDoc()).ToList();
                    Check.NotEmpty(chunks);
                    foreach (IngestionChunk c in chunks) Check.Equal("doc-1", c.DocumentId);
                })
                .Case("Chunk.HeaderContext", "Header context is prepended to element chunks", () =>
                {
                    AtomChunker chunker = new AtomChunker();
                    List<IngestionChunk> chunks = chunker.Chunk(BuildDoc()).ToList();
                    Check.True(chunks.Any(c => c.Content.Contains("Introduction")), "Expected header context in chunk content.");
                })
                .Case("Chunk.SourceMetadata", "Element chunks are tagged with an element source", () =>
                {
                    AtomChunker chunker = new AtomChunker();
                    List<IngestionChunk> chunks = chunker.Chunk(BuildDoc()).ToList();
                    IngestionChunk first = chunks[0];
                    Check.True(first.Metadata.ContainsKey(AtomMetadataKeys.ChunkSource));
                    Check.Equal("element", first.Metadata[AtomMetadataKeys.ChunkSource]?.ToString());
                })
                .CaseAsync("ChunkAsync.ProducesChunks", "Async chunking produces chunks", async ct =>
                {
                    AtomChunker chunker = new AtomChunker();
                    List<IngestionChunk> chunks = new List<IngestionChunk>();
                    await foreach (IngestionChunk c in chunker.ChunkAsync(BuildDoc(), ct))
                        chunks.Add(c);
                    Check.NotEmpty(chunks);
                })
                .Case("Chunk.NullDocument", "A null document throws ArgumentNullException", () =>
                {
                    AtomChunker chunker = new AtomChunker();
                    Check.Throws<ArgumentNullException>(() => chunker.Chunk(null!).ToList());
                })
                .Case("Options.Null", "Assigning null options throws ArgumentNullException", () =>
                {
                    AtomChunker chunker = new AtomChunker();
                    Check.Throws<ArgumentNullException>(() => chunker.Options = null!);
                })
                .Build("DataIngestion: AtomChunker");
        }

        internal static TestSuiteDescriptor HierarchyChunkerSuite()
        {
            return new SuiteBuilder("DataIngestion.HierarchyAwareChunker")
                .Case("Chunk.ProducesChunks", "Hierarchy-aware chunking produces chunks", () =>
                {
                    HierarchyAwareChunker chunker = new HierarchyAwareChunker();
                    List<IngestionChunk> chunks = chunker.Chunk(BuildDoc()).ToList();
                    Check.NotEmpty(chunks);
                })
                .Case("Chunk.SourceMetadata", "Chunks are tagged with a hierarchy source", () =>
                {
                    HierarchyAwareChunker chunker = new HierarchyAwareChunker();
                    List<IngestionChunk> chunks = chunker.Chunk(BuildDoc()).ToList();
                    Check.True(chunks.Any(c =>
                        c.Metadata.ContainsKey(AtomMetadataKeys.ChunkSource) &&
                        c.Metadata[AtomMetadataKeys.ChunkSource]?.ToString() == "hierarchy"),
                        "Expected a hierarchy-sourced chunk.");
                })
                .Case("Chunk.NullDocument", "A null document throws ArgumentNullException", () =>
                {
                    HierarchyAwareChunker chunker = new HierarchyAwareChunker();
                    Check.Throws<ArgumentNullException>(() => chunker.Chunk(null!).ToList());
                })
                .Build("DataIngestion: HierarchyAwareChunker");
        }

        private static IngestionDocument BuildDoc()
        {
            IngestionDocument doc = new IngestionDocument();
            doc.Id = "doc-1";

            IngestionDocumentElement header = new IngestionDocumentElement();
            header.ElementType = IngestionElementType.Header;
            header.Content = "Introduction";
            header.Metadata[AtomMetadataKeys.AtomHeaderLevel] = 1;
            doc.Elements.Add(header);

            IngestionDocumentElement body = new IngestionDocumentElement();
            body.ElementType = IngestionElementType.Paragraph;
            body.Content = LongParagraph;
            doc.Elements.Add(body);

            return doc;
        }
    }
}
