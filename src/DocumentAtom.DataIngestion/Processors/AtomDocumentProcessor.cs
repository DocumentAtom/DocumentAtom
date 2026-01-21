namespace DocumentAtom.DataIngestion.Processors
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Metadata;
    using DocumentAtom.DataIngestion.Readers;

    /// <summary>
    /// High-level processor that reads documents and produces chunks ready for embedding.
    /// </summary>
    public class AtomDocumentProcessor : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Processor options.
        /// </summary>
        public AtomDocumentProcessorOptions Options
        {
            get
            {
                return _Options;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Options));
                _Options = value;
            }
        }

        /// <summary>
        /// Logger action for debug output.
        /// </summary>
        public Action<string>? Logger { get; set; } = null;

        #endregion

        #region Private-Members

        private AtomDocumentProcessorOptions _Options;
        private readonly AtomDocumentReader _Reader;
        private readonly IAtomChunker _Chunker;
        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// High-level processor that reads documents and produces chunks ready for embedding.
        /// </summary>
        /// <param name="options">Processor options.</param>
        public AtomDocumentProcessor(AtomDocumentProcessorOptions? options = null)
        {
            _Options = options ?? new AtomDocumentProcessorOptions();
            _Reader = new AtomDocumentReader(_Options.ReaderOptions);

            _Chunker = _Options.UseHierarchyAwareChunking
                ? new HierarchyAwareChunker(_Options.ChunkerOptions)
                : new AtomChunker(_Options.ChunkerOptions);
        }

        /// <summary>
        /// High-level processor that reads documents and produces chunks ready for embedding.
        /// </summary>
        /// <param name="options">Processor options.</param>
        /// <param name="reader">Custom document reader.</param>
        /// <param name="chunker">Custom chunker.</param>
        public AtomDocumentProcessor(
            AtomDocumentProcessorOptions options,
            AtomDocumentReader reader,
            IAtomChunker chunker)
        {
            _Options = options ?? throw new ArgumentNullException(nameof(options));
            _Reader = reader ?? throw new ArgumentNullException(nameof(reader));
            _Chunker = chunker ?? throw new ArgumentNullException(nameof(chunker));
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Reader?.Dispose();
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Process a document from a file path and return chunks.
        /// </summary>
        /// <param name="path">Path to the document.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks.</returns>
        public async IAsyncEnumerable<IngestionChunk> ProcessAsync(
            string path,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("File not found.", path);

            Log($"Processing document: {path}");

            IngestionDocument document = await _Reader.ReadAsync(path, cancellationToken).ConfigureAwait(false);

            Log($"Read document with {document.Elements.Count} elements");

            HashSet<string>? seenHashes = _Options.RemoveDuplicates ? new HashSet<string>() : null;
            int duplicatesRemoved = 0;

            await foreach (IngestionChunk chunk in _Chunker.ChunkAsync(document, cancellationToken).ConfigureAwait(false))
            {
                // Skip empty chunks
                if (_Options.SkipEmptyChunks && string.IsNullOrWhiteSpace(chunk.Content))
                {
                    continue;
                }

                // Check minimum length
                if (chunk.Content.Length < _Options.MinimumChunkLength)
                {
                    continue;
                }

                // Check for duplicates
                if (seenHashes != null)
                {
                    string hash = ComputeHash(chunk.Content);
                    chunk.Metadata[AtomMetadataKeys.ComputedSha256] = hash;

                    if (!seenHashes.Add(hash))
                    {
                        duplicatesRemoved++;
                        continue;
                    }
                }

                yield return chunk;
            }

            if (duplicatesRemoved > 0)
            {
                Log($"Removed {duplicatesRemoved} duplicate chunks");
            }
        }

        /// <summary>
        /// Process a document from a byte array and return chunks.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <param name="contentType">Optional content type hint.</param>
        /// <param name="originalFilename">Optional original filename.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks.</returns>
        public async IAsyncEnumerable<IngestionChunk> ProcessAsync(
            byte[] data,
            string? contentType = null,
            string? originalFilename = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));

            Log($"Processing document from byte array ({data.Length} bytes)");

            IngestionDocument document = await _Reader.ReadAsync(data, contentType, originalFilename, cancellationToken).ConfigureAwait(false);

            Log($"Read document with {document.Elements.Count} elements");

            HashSet<string>? seenHashes = _Options.RemoveDuplicates ? new HashSet<string>() : null;
            int duplicatesRemoved = 0;

            await foreach (IngestionChunk chunk in _Chunker.ChunkAsync(document, cancellationToken).ConfigureAwait(false))
            {
                // Skip empty chunks
                if (_Options.SkipEmptyChunks && string.IsNullOrWhiteSpace(chunk.Content))
                {
                    continue;
                }

                // Check minimum length
                if (chunk.Content.Length < _Options.MinimumChunkLength)
                {
                    continue;
                }

                // Check for duplicates
                if (seenHashes != null)
                {
                    string hash = ComputeHash(chunk.Content);
                    chunk.Metadata[AtomMetadataKeys.ComputedSha256] = hash;

                    if (!seenHashes.Add(hash))
                    {
                        duplicatesRemoved++;
                        continue;
                    }
                }

                yield return chunk;
            }

            if (duplicatesRemoved > 0)
            {
                Log($"Removed {duplicatesRemoved} duplicate chunks");
            }
        }

        /// <summary>
        /// Process a document synchronously and return all chunks.
        /// </summary>
        /// <param name="path">Path to the document.</param>
        /// <returns>List of chunks.</returns>
        public List<IngestionChunk> Process(string path)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));
            if (!File.Exists(path)) throw new FileNotFoundException("File not found.", path);

            Log($"Processing document: {path}");

            IngestionDocument document = _Reader.Read(path);

            Log($"Read document with {document.Elements.Count} elements");

            return ProcessDocument(document);
        }

        /// <summary>
        /// Process a document from a byte array synchronously and return all chunks.
        /// </summary>
        /// <param name="data">Document data.</param>
        /// <param name="contentType">Optional content type hint.</param>
        /// <param name="originalFilename">Optional original filename.</param>
        /// <returns>List of chunks.</returns>
        public List<IngestionChunk> Process(byte[] data, string? contentType = null, string? originalFilename = null)
        {
            if (data == null || data.Length == 0) throw new ArgumentNullException(nameof(data));

            Log($"Processing document from byte array ({data.Length} bytes)");

            IngestionDocument document = _Reader.Read(data, contentType, originalFilename);

            Log($"Read document with {document.Elements.Count} elements");

            return ProcessDocument(document);
        }

        /// <summary>
        /// Process multiple documents from file paths.
        /// </summary>
        /// <param name="paths">Paths to documents.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks with source information.</returns>
        public async IAsyncEnumerable<IngestionChunk> ProcessManyAsync(
            IEnumerable<string> paths,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (paths == null) throw new ArgumentNullException(nameof(paths));

            foreach (string path in paths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await foreach (IngestionChunk chunk in ProcessAsync(path, cancellationToken).ConfigureAwait(false))
                {
                    chunk.Metadata[AtomMetadataKeys.SourcePath] = path;
                    yield return chunk;
                }
            }
        }

        /// <summary>
        /// Get the underlying document reader.
        /// </summary>
        /// <returns>The document reader.</returns>
        public AtomDocumentReader GetReader()
        {
            return _Reader;
        }

        /// <summary>
        /// Get the underlying chunker.
        /// </summary>
        /// <returns>The chunker.</returns>
        public IAtomChunker GetChunker()
        {
            return _Chunker;
        }

        #endregion

        #region Private-Methods

        private List<IngestionChunk> ProcessDocument(IngestionDocument document)
        {
            List<IngestionChunk> chunks = new List<IngestionChunk>();
            HashSet<string>? seenHashes = _Options.RemoveDuplicates ? new HashSet<string>() : null;
            int duplicatesRemoved = 0;

            foreach (IngestionChunk chunk in _Chunker.Chunk(document))
            {
                // Skip empty chunks
                if (_Options.SkipEmptyChunks && string.IsNullOrWhiteSpace(chunk.Content))
                {
                    continue;
                }

                // Check minimum length
                if (chunk.Content.Length < _Options.MinimumChunkLength)
                {
                    continue;
                }

                // Check for duplicates
                if (seenHashes != null)
                {
                    string hash = ComputeHash(chunk.Content);
                    chunk.Metadata[AtomMetadataKeys.ComputedSha256] = hash;

                    if (!seenHashes.Add(hash))
                    {
                        duplicatesRemoved++;
                        continue;
                    }
                }

                chunks.Add(chunk);
            }

            if (duplicatesRemoved > 0)
            {
                Log($"Removed {duplicatesRemoved} duplicate chunks");
            }

            Log($"Produced {chunks.Count} chunks");

            return chunks;
        }

        private string ComputeHash(string content)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(content);
            byte[] hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }

        private void Log(string message)
        {
            Logger?.Invoke($"[AtomDocumentProcessor] {message}");
        }

        #endregion
    }
}
