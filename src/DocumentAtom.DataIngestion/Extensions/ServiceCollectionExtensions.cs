namespace DocumentAtom.DataIngestion.Extensions
{
    using System;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Converters;
    using DocumentAtom.DataIngestion.Processors;
    using DocumentAtom.DataIngestion.Readers;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;

    /// <summary>
    /// Extension methods for registering DocumentAtom.DataIngestion services.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add DocumentAtom data ingestion services to the service collection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configureReader">Optional action to configure reader options.</param>
        /// <param name="configureChunker">Optional action to configure chunker options.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDocumentAtomIngestion(
            this IServiceCollection services,
            Action<AtomDocumentReaderOptions>? configureReader = null,
            Action<AtomChunkerOptions>? configureChunker = null)
        {
            if (services == null) throw new ArgumentNullException(nameof(services));

            // Register options
            AtomDocumentReaderOptions readerOptions = new AtomDocumentReaderOptions();
            configureReader?.Invoke(readerOptions);

            AtomChunkerOptions chunkerOptions = new AtomChunkerOptions();
            configureChunker?.Invoke(chunkerOptions);

            services.TryAddSingleton(readerOptions);
            services.TryAddSingleton(chunkerOptions);

            // Register processor options
            AtomDocumentProcessorOptions processorOptions = new AtomDocumentProcessorOptions
            {
                ReaderOptions = readerOptions,
                ChunkerOptions = chunkerOptions
            };
            services.TryAddSingleton(processorOptions);

            // Register services
            services.TryAddSingleton<IProcessorFactory>(sp =>
                new DefaultProcessorFactory(sp.GetRequiredService<AtomDocumentReaderOptions>()));

            services.TryAddSingleton<IAtomToIngestionElementConverter>(sp =>
                new AtomToIngestionElementConverter(sp.GetRequiredService<AtomDocumentReaderOptions>()));

            services.TryAddSingleton<IAtomChunker>(sp =>
            {
                AtomDocumentProcessorOptions opts = sp.GetRequiredService<AtomDocumentProcessorOptions>();
                return opts.UseHierarchyAwareChunking
                    ? new HierarchyAwareChunker(sp.GetRequiredService<AtomChunkerOptions>())
                    : new AtomChunker(sp.GetRequiredService<AtomChunkerOptions>());
            });

            services.TryAddScoped<AtomDocumentReader>(sp =>
                new AtomDocumentReader(
                    sp.GetRequiredService<AtomDocumentReaderOptions>(),
                    sp.GetRequiredService<IProcessorFactory>(),
                    sp.GetRequiredService<IAtomToIngestionElementConverter>()));

            services.TryAddScoped<AtomDocumentProcessor>(sp =>
                new AtomDocumentProcessor(sp.GetRequiredService<AtomDocumentProcessorOptions>()));

            return services;
        }

        /// <summary>
        /// Add DocumentAtom data ingestion services optimized for RAG.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDocumentAtomIngestionForRag(this IServiceCollection services)
        {
            return services.AddDocumentAtomIngestion(
                reader =>
                {
                    reader.PreserveFullAtomData = true;
                    reader.BuildHierarchy = true;
                    reader.EnableOcr = true;
                },
                chunker =>
                {
                    chunker.MaxChunkSize = 500;
                    chunker.ChunkOverlap = 50;
                    chunker.PreserveParagraphs = true;
                    chunker.IncludeHeaderContext = true;
                    chunker.UseQuarksIfAvailable = true;
                });
        }

        /// <summary>
        /// Add DocumentAtom data ingestion services optimized for summarization.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <returns>The service collection.</returns>
        public static IServiceCollection AddDocumentAtomIngestionForSummarization(this IServiceCollection services)
        {
            return services.AddDocumentAtomIngestion(
                reader =>
                {
                    reader.PreserveFullAtomData = false;
                    reader.BuildHierarchy = true;
                    reader.EnableOcr = true;
                },
                chunker =>
                {
                    chunker.MaxChunkSize = 2000;
                    chunker.ChunkOverlap = 200;
                    chunker.PreserveParagraphs = true;
                    chunker.IncludeHeaderContext = true;
                    chunker.UseQuarksIfAvailable = false;
                });
        }
    }
}
