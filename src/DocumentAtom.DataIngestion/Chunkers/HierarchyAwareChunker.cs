namespace DocumentAtom.DataIngestion.Chunkers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.DataIngestion.Metadata;

    /// <summary>
    /// Chunker that preserves document hierarchy and context.
    /// </summary>
    public class HierarchyAwareChunker : IAtomChunker
    {
        #region Public-Members

        /// <summary>
        /// Chunker options.
        /// </summary>
        public AtomChunkerOptions Options
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

        #endregion

        #region Private-Members

        private AtomChunkerOptions _Options;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Chunker that preserves document hierarchy and context.
        /// </summary>
        /// <param name="options">Chunker options.</param>
        public HierarchyAwareChunker(AtomChunkerOptions? options = null)
        {
            _Options = options ?? new AtomChunkerOptions();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Chunk an ingestion document into smaller pieces.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <returns>Enumerable of chunks.</returns>
        public async IAsyncEnumerable<IngestionChunk> ChunkAsync(
            IngestionDocument document,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));

            List<IngestionChunk> chunks = await Task.Run(() =>
                ChunkInternal(document), cancellationToken).ConfigureAwait(false);

            foreach (IngestionChunk chunk in chunks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return chunk;
            }
        }

        /// <summary>
        /// Chunk an ingestion document into smaller pieces synchronously.
        /// </summary>
        /// <param name="document">The document to chunk.</param>
        /// <returns>Enumerable of chunks.</returns>
        public IEnumerable<IngestionChunk> Chunk(IngestionDocument document)
        {
            if (document == null) throw new ArgumentNullException(nameof(document));
            return ChunkInternal(document);
        }

        #endregion

        #region Private-Methods

        private List<IngestionChunk> ChunkInternal(IngestionDocument document)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();
            List<HierarchyNode> hierarchy = BuildHierarchy(document.Elements);
            int chunkIndex = 0;

            foreach (HierarchyNode node in hierarchy)
            {
                List<IngestionChunk> nodeChunks = ChunkNode(node, document.Id, chunkIndex);
                results.AddRange(nodeChunks);
                chunkIndex += nodeChunks.Count;
            }

            return results;
        }

        private List<HierarchyNode> BuildHierarchy(List<IngestionDocumentElement> elements)
        {
            List<HierarchyNode> roots = new List<HierarchyNode>();
            Stack<HierarchyNode> stack = new Stack<HierarchyNode>();
            HierarchyNode? currentSection = null;

            foreach (IngestionDocumentElement element in elements)
            {
                if (element.ElementType == IngestionElementType.Header)
                {
                    int level = GetHeaderLevel(element);
                    HierarchyNode newNode = new HierarchyNode
                    {
                        HeaderElement = element,
                        Level = level,
                        Title = element.Content ?? "Untitled"
                    };

                    // Pop stack until we find a parent with lower level
                    while (stack.Count > 0 && stack.Peek().Level >= level)
                    {
                        stack.Pop();
                    }

                    if (stack.Count > 0)
                    {
                        HierarchyNode parent = stack.Peek();
                        parent.Children.Add(newNode);
                        newNode.Parent = parent;
                    }
                    else
                    {
                        roots.Add(newNode);
                    }

                    stack.Push(newNode);
                    currentSection = newNode;
                }
                else
                {
                    // Add content to current section or root
                    if (currentSection != null)
                    {
                        currentSection.ContentElements.Add(element);
                    }
                    else
                    {
                        // No header yet, create implicit root
                        if (roots.Count == 0 || roots.Last().HeaderElement != null)
                        {
                            HierarchyNode implicitRoot = new HierarchyNode
                            {
                                Level = 0,
                                Title = "Document"
                            };
                            roots.Add(implicitRoot);
                        }

                        roots.Last().ContentElements.Add(element);
                    }
                }
            }

            return roots;
        }

        private int GetHeaderLevel(IngestionDocumentElement element)
        {
            if (element.Metadata.TryGetValue(AtomMetadataKeys.AtomHeaderLevel, out object? levelObj))
            {
                if (levelObj is int level) return level;
                if (levelObj is long longLevel) return (int)longLevel;
                if (int.TryParse(levelObj?.ToString(), out int parsedLevel)) return parsedLevel;
            }

            return 1; // Default to H1
        }

        private List<IngestionChunk> ChunkNode(HierarchyNode node, string documentId, int startIndex)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();
            int currentIndex = startIndex;

            // Build header breadcrumb
            List<string> breadcrumb = new List<string>();
            HierarchyNode? current = node;

            while (current != null)
            {
                if (!string.IsNullOrEmpty(current.Title) && current.HeaderElement != null)
                {
                    breadcrumb.Insert(0, current.Title);
                }
                current = current.Parent;
            }

            string headerContext = breadcrumb.Count > 0 ? string.Join(" > ", breadcrumb) : string.Empty;

            // Chunk content elements
            StringBuilder contentBuilder = new StringBuilder();

            foreach (IngestionDocumentElement element in node.ContentElements)
            {
                if (!string.IsNullOrEmpty(element.Content))
                {
                    contentBuilder.AppendLine(element.Content);
                }
            }

            string content = contentBuilder.ToString().Trim();

            if (!string.IsNullOrEmpty(content))
            {
                // Check if content fits in one chunk
                string prefix = _Options.IncludeHeaderContext && !string.IsNullOrEmpty(headerContext)
                    ? headerContext + _Options.HeaderContextSeparator
                    : string.Empty;

                if (content.Length + prefix.Length <= _Options.MaxChunkSize)
                {
                    IngestionChunk chunk = new IngestionChunk
                    {
                        DocumentId = documentId,
                        ChunkIndex = currentIndex++,
                        Content = prefix + content
                    };

                    AddHierarchyMetadata(chunk, node, headerContext);
                    results.Add(chunk);
                }
                else
                {
                    // Split content
                    List<IngestionChunk> splitChunks = SplitContent(content, documentId, currentIndex, headerContext, node);
                    results.AddRange(splitChunks);
                    currentIndex += splitChunks.Count;
                }
            }

            // Recursively chunk children
            foreach (HierarchyNode child in node.Children)
            {
                List<IngestionChunk> childChunks = ChunkNode(child, documentId, currentIndex);
                results.AddRange(childChunks);
                currentIndex += childChunks.Count;
            }

            return results;
        }

        private List<IngestionChunk> SplitContent(
            string content,
            string documentId,
            int startIndex,
            string headerContext,
            HierarchyNode node)
        {
            List<IngestionChunk> results = new List<IngestionChunk>();
            int currentIndex = startIndex;

            string prefix = _Options.IncludeHeaderContext && !string.IsNullOrEmpty(headerContext)
                ? headerContext + _Options.HeaderContextSeparator
                : string.Empty;

            int maxContentSize = _Options.MaxChunkSize - prefix.Length;
            List<string> segments = SplitText(content);
            StringBuilder currentChunk = new StringBuilder();
            int splitIndex = 0;

            foreach (string segment in segments)
            {
                if (currentChunk.Length + segment.Length > maxContentSize && currentChunk.Length > 0)
                {
                    string chunkText = currentChunk.ToString().Trim();

                    if (chunkText.Length >= _Options.MinChunkSize)
                    {
                        IngestionChunk chunk = new IngestionChunk
                        {
                            DocumentId = documentId,
                            ChunkIndex = currentIndex++,
                            Content = prefix + chunkText
                        };

                        AddHierarchyMetadata(chunk, node, headerContext);
                        chunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = splitIndex++;
                        results.Add(chunk);
                    }

                    currentChunk.Clear();

                    // Add overlap
                    if (_Options.ChunkOverlap > 0 && chunkText.Length > _Options.ChunkOverlap)
                    {
                        string overlap = chunkText.Substring(chunkText.Length - _Options.ChunkOverlap);
                        currentChunk.Append(overlap);
                    }
                }

                currentChunk.Append(segment);

                if (!segment.EndsWith(" ") && !segment.EndsWith("\n"))
                {
                    currentChunk.Append(" ");
                }
            }

            // Final chunk
            if (currentChunk.Length > 0)
            {
                string finalText = currentChunk.ToString().Trim();

                if (finalText.Length >= _Options.MinChunkSize)
                {
                    IngestionChunk chunk = new IngestionChunk
                    {
                        DocumentId = documentId,
                        ChunkIndex = currentIndex,
                        Content = prefix + finalText
                    };

                    AddHierarchyMetadata(chunk, node, headerContext);
                    chunk.Metadata[AtomMetadataKeys.ChunkSplitIndex] = splitIndex;
                    results.Add(chunk);
                }
            }

            return results;
        }

        private List<string> SplitText(string text)
        {
            List<string> segments = new List<string>();

            foreach (string separator in _Options.SplitSeparators)
            {
                if (text.Contains(separator))
                {
                    string[] parts = text.Split(new[] { separator }, StringSplitOptions.None);

                    for (int i = 0; i < parts.Length; i++)
                    {
                        string part = parts[i];

                        if (i < parts.Length - 1)
                        {
                            part += separator;
                        }

                        if (!string.IsNullOrWhiteSpace(part))
                        {
                            segments.Add(part);
                        }
                    }

                    if (segments.Count > 1)
                    {
                        return segments;
                    }

                    segments.Clear();
                }
            }

            // Fallback to words
            foreach (string word in text.Split(' '))
            {
                if (!string.IsNullOrWhiteSpace(word))
                {
                    segments.Add(word + " ");
                }
            }

            return segments;
        }

        private void AddHierarchyMetadata(IngestionChunk chunk, HierarchyNode node, string headerContext)
        {
            chunk.Metadata[AtomMetadataKeys.ChunkSource] = "hierarchy";

            if (!string.IsNullOrEmpty(headerContext))
            {
                chunk.Metadata[AtomMetadataKeys.ChunkHeaderContext] = headerContext;
            }

            chunk.Metadata[AtomMetadataKeys.HierarchyLevel] = node.Level;

            if (!string.IsNullOrEmpty(node.Title))
            {
                chunk.Metadata[AtomMetadataKeys.SectionTitle] = node.Title;
                chunk.Metadata[AtomMetadataKeys.SectionLevel] = node.Level;
            }

            chunk.Metadata[AtomMetadataKeys.SectionAtomCount] = node.ContentElements.Count;
        }

        #endregion

        #region Private-Classes

        private class HierarchyNode
        {
            public IngestionDocumentElement? HeaderElement { get; set; }
            public int Level { get; set; }
            public string Title { get; set; } = string.Empty;
            public HierarchyNode? Parent { get; set; }
            public List<HierarchyNode> Children { get; set; } = new();
            public List<IngestionDocumentElement> ContentElements { get; set; } = new();
        }

        #endregion
    }
}
