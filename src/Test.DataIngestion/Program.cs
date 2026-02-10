namespace Test.DataIngestion
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using DocumentAtom.DataIngestion;
    using DocumentAtom.DataIngestion.Chunkers;
    using DocumentAtom.DataIngestion.Processors;
    using DocumentAtom.DataIngestion.Readers;
    using GetSomeInput;
    using Timestamps;

    /// <summary>
    /// Test application for DocumentAtom.DataIngestion.
    /// </summary>
    public class Program
    {
        private static bool _RunForever = true;
        private static AtomDocumentProcessor? _Processor = null;

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Arguments.</param>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("DocumentAtom.DataIngestion Test Application");
            Console.WriteLine("");

            InitializeProcessor();

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);

                switch (userInput)
                {
                    case "q":
                    case "quit":
                        _RunForever = false;
                        break;

                    case "?":
                        PrintMenu();
                        break;

                    case "cls":
                        Console.Clear();
                        break;

                    case "process":
                        await ProcessDocument();
                        break;

                    case "process-bytes":
                        await ProcessDocumentBytes();
                        break;

                    case "read":
                        await ReadDocument();
                        break;

                    case "chunk":
                        await ChunkDocument();
                        break;

                    case "types":
                        PrintSupportedTypes();
                        break;

                    default:
                        Console.WriteLine("Unknown command. Type ? for help.");
                        break;
                }
            }

            _Processor?.Dispose();
            Console.WriteLine("Goodbye!");
        }

        private static void InitializeProcessor()
        {
            AtomDocumentProcessorOptions options = AtomDocumentProcessorOptions.ForRag();
            options.ReaderOptions.EnableOcr = true;
            options.ReaderOptions.BuildHierarchy = true;
            options.RemoveDuplicates = true;

            _Processor = new AtomDocumentProcessor(options);
            _Processor.Logger = msg => Console.WriteLine($"[LOG] {msg}");

            Console.WriteLine("Processor initialized with RAG-optimized settings.");
            Console.WriteLine("");
        }

        private static void PrintMenu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  q/quit          Exit the application");
            Console.WriteLine("  cls             Clear the screen");
            Console.WriteLine("  process         Process a document and show chunks");
            Console.WriteLine("  process-bytes   Process document from bytes");
            Console.WriteLine("  read            Read a document without chunking");
            Console.WriteLine("  chunk           Chunk an already-read document");
            Console.WriteLine("  types           Show supported document types");
            Console.WriteLine("");
        }

        private static async Task ProcessDocument()
        {
            string filename = Inputty.GetString("Filename:", "Sample/sample.txt", false);

            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine($"Processing: {filename}");
            Console.WriteLine("");

            Timestamp ts = new Timestamp();
            int chunkCount = 0;

            try
            {
                await foreach (IngestionChunk chunk in _Processor!.ProcessAsync(filename))
                {
                    chunkCount++;
                    PrintChunk(chunk);
                }

                Console.WriteLine("");
                Console.WriteLine($"Total chunks: {chunkCount}");
                Console.WriteLine($"Processing time: {ts.TotalMs}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ProcessDocumentBytes()
        {
            string filename = Inputty.GetString("Filename:", "Sample/sample.txt", false);

            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine($"Processing from bytes: {filename}");
            Console.WriteLine("");

            byte[] data = await File.ReadAllBytesAsync(filename);
            Timestamp ts = new Timestamp();
            int chunkCount = 0;

            try
            {
                await foreach (IngestionChunk chunk in _Processor!.ProcessAsync(data, null, Path.GetFileName(filename)))
                {
                    chunkCount++;
                    PrintChunk(chunk);
                }

                Console.WriteLine("");
                Console.WriteLine($"Total chunks: {chunkCount}");
                Console.WriteLine($"Processing time: {ts.TotalMs}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ReadDocument()
        {
            string filename = Inputty.GetString("Filename:", "Sample/sample.txt", false);

            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine($"Reading: {filename}");
            Console.WriteLine("");

            AtomDocumentReader reader = _Processor!.GetReader();
            Timestamp ts = new Timestamp();

            try
            {
                IngestionDocument document = await reader.ReadAsync(filename);

                Console.WriteLine($"Document ID: {document.Id}");
                Console.WriteLine($"Source Path: {document.SourcePath}");
                Console.WriteLine($"Sections: {document.Sections.Count}");
                Console.WriteLine($"Elements: {document.Elements.Count}");
                Console.WriteLine("");

                Console.WriteLine("Metadata:");
                foreach (KeyValuePair<string, object> kvp in document.Metadata)
                {
                    Console.WriteLine($"  {kvp.Key}: {kvp.Value}");
                }

                Console.WriteLine("");
                Console.WriteLine($"Reading time: {ts.TotalMs}ms");

                bool showElements = Inputty.GetBoolean("Show elements?", false);

                if (showElements)
                {
                    Console.WriteLine("");
                    int i = 0;
                    foreach (IngestionDocumentElement element in document.Elements)
                    {
                        Console.WriteLine($"--- Element {i++} ---");
                        Console.WriteLine($"ID: {element.Id}");
                        Console.WriteLine($"Type: {element.ElementType}");
                        Console.WriteLine($"Content: {TruncateText(element.Content, 200)}");
                        Console.WriteLine("");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ChunkDocument()
        {
            string filename = Inputty.GetString("Filename:", "Sample/sample.txt", false);

            if (!File.Exists(filename))
            {
                Console.WriteLine($"File not found: {filename}");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine($"Reading and chunking: {filename}");
            Console.WriteLine("");

            AtomDocumentReader reader = _Processor!.GetReader();
            IAtomChunker chunker = _Processor.GetChunker();
            Timestamp ts = new Timestamp();

            try
            {
                IngestionDocument document = await reader.ReadAsync(filename);
                int chunkCount = 0;

                await foreach (IngestionChunk chunk in chunker.ChunkAsync(document))
                {
                    chunkCount++;
                    PrintChunk(chunk);
                }

                Console.WriteLine("");
                Console.WriteLine($"Total chunks: {chunkCount}");
                Console.WriteLine($"Processing time: {ts.TotalMs}ms");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static void PrintSupportedTypes()
        {
            Console.WriteLine("");
            Console.WriteLine("Supported document types:");

            foreach (DocumentAtom.Core.TypeDetection.DocumentTypeEnum type in AtomDocumentReader.GetSupportedTypes())
            {
                Console.WriteLine($"  - {type}");
            }

            Console.WriteLine("");
        }

        private static void PrintChunk(IngestionChunk chunk)
        {
            Console.WriteLine($"--- Chunk {chunk.ChunkIndex} ---");
            Console.WriteLine($"ID: {chunk.Id}");
            Console.WriteLine($"Document ID: {chunk.DocumentId}");
            Console.WriteLine($"Content ({chunk.Content.Length} chars): {TruncateText(chunk.Content, 150)}");

            if (chunk.Metadata.Count > 0)
            {
                Console.WriteLine("Metadata:");
                foreach (KeyValuePair<string, object> kvp in chunk.Metadata.Take(5))
                {
                    string value = kvp.Value?.ToString() ?? "null";
                    Console.WriteLine($"  {kvp.Key}: {TruncateText(value, 50)}");
                }

                if (chunk.Metadata.Count > 5)
                {
                    Console.WriteLine($"  ... and {chunk.Metadata.Count - 5} more");
                }
            }

            Console.WriteLine("");
        }

        private static string? TruncateText(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text)) return text;

            if (text.Length <= maxLength) return text;

            return text.Substring(0, maxLength) + "...";
        }
    }
}
