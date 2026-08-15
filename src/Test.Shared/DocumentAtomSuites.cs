namespace DocumentAtom.Testing.Shared
{
    using System.Collections.Generic;
    using DocumentAtom.Testing.Shared.Suites;
    using Touchstone.Core;

    /// <summary>
    /// The central source of truth for the DocumentAtom test suite. Every host — the Touchstone CLI
    /// runner (Test.Automated), the xUnit adapter (Test.Xunit), and the NUnit adapter (Test.Nunit) —
    /// executes exactly these descriptors. Adding a suite here makes it available to all runners at once.
    /// </summary>
    public static class DocumentAtomSuites
    {
        /// <summary>
        /// Gets every test suite descriptor covering the DocumentAtom platform: Core (chunking, atoms,
        /// text tools, helpers, type detection, settings, API models), the text-family processors, the
        /// DataIngestion pipeline, and the binary document processors.
        /// </summary>
        public static IReadOnlyList<TestSuiteDescriptor> All { get; } = new List<TestSuiteDescriptor>
        {
            // Core: chunking
            ChunkSuites.Chunk(),
            ChunkSuites.Configuration(),
            ChunkerSuites.FixedToken(),
            ChunkerSuites.Sentence(),
            ChunkerSuites.Paragraph(),
            ChunkerSuites.Regex(),
            ChunkerSuites.ListAndTable(),
            ChunkingEngineSuites.Build(),

            // Core: enums, atoms, helpers, text tools, type detection, settings, API
            EnumSuites.Build(),
            AtomSuites.Atom(),
            AtomSuites.BoundingBoxSuite(),
            HashHelperSuites.Build(),
            TextToolsSuites.Lemmatizer(),
            TextToolsSuites.Replacer(),
            TextToolsSuites.Splitter(),
            TextToolsSuites.Remover(),
            TextToolsSuites.Extractor(),
            CoreHelperSuites.StringHelperSuite(),
            CoreHelperSuites.DataTableHelperSuite(),
            CoreHelperSuites.SettingsBaseSuite(),
            CoreHelperSuites.ApiSuite(),
            TypeDetectionSuites.Build(),

            // Text-family processors
            TextProcessorSuites.Text(),
            TextProcessorSuites.Csv(),
            TextProcessorSuites.Json(),
            TextProcessorSuites.Xml(),
            TextProcessorSuites.Markdown(),
            TextProcessorSuites.Html(),

            // DataIngestion pipeline
            DataIngestionModelSuites.Models(),
            DataIngestionModelSuites.ChunkerOptions(),
            DataIngestionModelSuites.ProcessorOptions(),
            DataIngestionModelSuites.ReaderOptions(),
            DataIngestionChunkerSuites.AtomChunkerSuite(),
            DataIngestionChunkerSuites.HierarchyChunkerSuite(),
            DataIngestionConverterSuites.Converter(),
            DataIngestionConverterSuites.Serializer(),
            DataIngestionConverterSuites.Keys(),
            DataIngestionReaderSuites.Reader(),
            DataIngestionReaderSuites.Processor(),
            DataIngestionReaderSuites.Factory(),
            DataIngestionReaderSuites.DependencyInjection(),

            // Binary document processors
            DocumentProcessorSuites.Build(),
        };
    }
}
