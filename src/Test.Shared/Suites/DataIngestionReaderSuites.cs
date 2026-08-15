namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DocumentAtom.Core;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.DataIngestion;
    using DocumentAtom.DataIngestion.Extensions;
    using DocumentAtom.DataIngestion.Processors;
    using DocumentAtom.DataIngestion.Readers;
    using Microsoft.Extensions.DependencyInjection;
    using Touchstone.Core;

    /// <summary>
    /// Suites covering the reader, processor, processor factory, and DI registration end-to-end
    /// against generated text documents.
    /// </summary>
    internal static class DataIngestionReaderSuites
    {
        internal static TestSuiteDescriptor Reader()
        {
            return new SuiteBuilder("DataIngestion.Reader")
                .Case("GetSupportedTypes", "The supported type list contains the expected members", () =>
                {
                    IReadOnlyList<DocumentTypeEnum> types = AtomDocumentReader.GetSupportedTypes();
                    Check.Equal(17, types.Count);
                    Check.True(types.Contains(DocumentTypeEnum.Pdf));
                    Check.True(types.Contains(DocumentTypeEnum.Text));
                    Check.True(types.Contains(DocumentTypeEnum.Markdown));
                })
                .CaseAsync("ReadAsync.TextDocument", "Reading a text document yields elements and source metadata", async ct =>
                {
                    string path = Workspace.WriteText("txt", SampleData.PlainText);
                    using AtomDocumentReader reader = new AtomDocumentReader();
                    IngestionDocument doc = await reader.ReadAsync(path, ct);
                    Check.NotEmpty(doc.Elements);
                    Check.NotEmpty(doc.Metadata);
                })
                .CaseAsync("ReadAsync.Null", "Reading a null path throws ArgumentNullException", async ct =>
                {
                    using AtomDocumentReader reader = new AtomDocumentReader();
                    await Check.ThrowsAsync<ArgumentNullException>(async () => await reader.ReadAsync((string)null!, ct));
                })
                .CaseAsync("ReadAsync.Missing", "Reading a missing file throws FileNotFoundException", async ct =>
                {
                    using AtomDocumentReader reader = new AtomDocumentReader();
                    await Check.ThrowsAsync<FileNotFoundException>(async () => await reader.ReadAsync(Workspace.NonExistentPath("txt"), ct));
                })
                .Case("Options.Null", "Assigning null options throws ArgumentNullException", () =>
                {
                    using AtomDocumentReader reader = new AtomDocumentReader();
                    Check.Throws<ArgumentNullException>(() => reader.Options = null!);
                })
                .Build("DataIngestion: Reader");
        }

        internal static TestSuiteDescriptor Processor()
        {
            return new SuiteBuilder("DataIngestion.Processor")
                .Case("Process.TextDocument", "Processing a text document produces chunks", () =>
                {
                    string path = Workspace.WriteText("txt", SampleData.PlainText);
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    List<IngestionChunk> chunks = processor.Process(path);
                    Check.NotEmpty(chunks);
                })
                .CaseAsync("ProcessAsync.TextDocument", "Async processing produces chunks", async ct =>
                {
                    string path = Workspace.WriteText("txt", SampleData.PlainText);
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    List<IngestionChunk> chunks = new List<IngestionChunk>();
                    await foreach (IngestionChunk c in processor.ProcessAsync(path, ct))
                        chunks.Add(c);
                    Check.NotEmpty(chunks);
                })
                .Case("Process.Null", "Processing a null path throws ArgumentNullException", () =>
                {
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    Check.Throws<ArgumentNullException>(() => processor.Process((string)null!));
                })
                .Case("Process.Missing", "Processing a missing file throws FileNotFoundException", () =>
                {
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    Check.Throws<FileNotFoundException>(() => processor.Process(Workspace.NonExistentPath("txt")));
                })
                .CaseAsync("ProcessAsync.NullBytes", "Async processing of null bytes throws ArgumentNullException", async ct =>
                {
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    await Check.ThrowsAsync<ArgumentNullException>(async () =>
                    {
                        await foreach (IngestionChunk _ in processor.ProcessAsync((byte[])null!, null, null, ct)) { }
                    });
                })
                .Case("Options.Null", "Assigning null options throws ArgumentNullException", () =>
                {
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    Check.Throws<ArgumentNullException>(() => processor.Options = null!);
                })
                .Case("Accessors", "GetReader and GetChunker return non-null instances", () =>
                {
                    using AtomDocumentProcessor processor = new AtomDocumentProcessor();
                    Check.NotNull(processor.GetReader());
                    Check.NotNull(processor.GetChunker());
                })
                .Build("DataIngestion: Processor");
        }

        internal static TestSuiteDescriptor Factory()
        {
            return new SuiteBuilder("DataIngestion.ProcessorFactory")
                .Case("Ctor.Null", "Constructing with null options throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => new DefaultProcessorFactory(null!));
                })
                .Case("Create.Text", "A Text type maps to a processor", () =>
                {
                    DefaultProcessorFactory f = new DefaultProcessorFactory(new AtomDocumentReaderOptions());
                    ProcessorBase? p = f.CreateProcessor(DocumentTypeEnum.Text);
                    Check.NotNull(p);
                    p?.Dispose();
                })
                .Case("Create.Markdown", "A Markdown type maps to a processor", () =>
                {
                    DefaultProcessorFactory f = new DefaultProcessorFactory(new AtomDocumentReaderOptions());
                    ProcessorBase? p = f.CreateProcessor(DocumentTypeEnum.Markdown);
                    Check.NotNull(p);
                    p?.Dispose();
                })
                .Case("Create.Unsupported", "An unsupported type maps to null", () =>
                {
                    DefaultProcessorFactory f = new DefaultProcessorFactory(new AtomDocumentReaderOptions());
                    Check.Null(f.CreateProcessor(DocumentTypeEnum.Mp3));
                })
                .Build("DataIngestion: ProcessorFactory");
        }

        internal static TestSuiteDescriptor DependencyInjection()
        {
            return new SuiteBuilder("DataIngestion.DependencyInjection")
                .Case("Register.Resolves", "The processor resolves from a configured service provider", () =>
                {
                    ServiceCollection services = new ServiceCollection();
                    services.AddDocumentAtomIngestion();
                    using ServiceProvider provider = services.BuildServiceProvider();
                    using IServiceScope scope = provider.CreateScope();
                    AtomDocumentProcessor processor = scope.ServiceProvider.GetRequiredService<AtomDocumentProcessor>();
                    Check.NotNull(processor);
                })
                .Case("Register.ForRag", "The RAG preset registers a resolvable processor", () =>
                {
                    ServiceCollection services = new ServiceCollection();
                    services.AddDocumentAtomIngestionForRag();
                    using ServiceProvider provider = services.BuildServiceProvider();
                    using IServiceScope scope = provider.CreateScope();
                    Check.NotNull(scope.ServiceProvider.GetRequiredService<AtomDocumentProcessor>());
                })
                .Case("Register.NullServices", "Registering on a null service collection throws ArgumentNullException", () =>
                {
                    Check.Throws<ArgumentNullException>(() => ServiceCollectionExtensions.AddDocumentAtomIngestion(null!));
                })
                .Build("DataIngestion: DependencyInjection");
        }
    }
}
