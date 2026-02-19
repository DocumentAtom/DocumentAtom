namespace Test.DocumentAtomSdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.Sdk;
    using GetSomeInput;

    /// <summary>
    /// Test application for DocumentAtom SDK demonstrating all available methods.
    /// </summary>
    public static class Program
    {
        #region Private-Members

        private static bool _RunForever = true;
        private static bool _Debug = false;
        private static DocumentAtomSdk? _Sdk = null;
        private static string _Endpoint = "http://localhost:8000";
        private static string? _AccessKey = null;
        private static readonly JsonSerializerOptions _JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };

        #endregion

        #region Main-Entry

        /// <summary>
        /// Main entry point for the test application.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static async Task Main(string[] args)
        {
            Console.WriteLine("DocumentAtom SDK Test Application");
            Console.WriteLine("=================================");
            Console.WriteLine();

            bool endpointFromArgs = false;
            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
            {
                _Endpoint = args[0];
                endpointFromArgs = true;
            }

            // Initialize SDK
            InitializeSdk(endpointFromArgs);

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);

                if (userInput.Equals("?")) ShowMenu();
                else if (userInput.Equals("q")) _RunForever = false;
                else if (userInput.Equals("cls")) Console.Clear();
                else if (userInput.Equals("debug")) ToggleDebug();
                else if (userInput.Equals("endpoint")) SetEndpoint();
                else if (userInput.Equals("key")) SetAccessKey();
                else if (userInput.Equals("health")) await TestHealth();
                else if (userInput.Equals("status")) await TestStatus();
                else if (userInput.Equals("detect")) await TestTypeDetection();
                else if (userInput.Equals("csv")) await TestCsvProcessing();
                else if (userInput.Equals("excel")) await TestExcelProcessing();
                else if (userInput.Equals("html")) await TestHtmlProcessing();
                else if (userInput.Equals("json")) await TestJsonProcessing();
                else if (userInput.Equals("markdown")) await TestMarkdownProcessing();
                else if (userInput.Equals("ocr")) await TestOcrProcessing();
                else if (userInput.Equals("pdf")) await TestPdfProcessing();
                else if (userInput.Equals("png")) await TestPngProcessing();
                else if (userInput.Equals("powerpoint")) await TestPowerPointProcessing();
                else if (userInput.Equals("rtf")) await TestRtfProcessing();
                else if (userInput.Equals("text")) await TestTextProcessing();
                else if (userInput.Equals("word")) await TestWordProcessing();
                else if (userInput.Equals("xml")) await TestXmlProcessing();
                else
                {
                    Console.WriteLine("Unknown command. Type '?' for help.");
                }
            }

            // Cleanup
            _Sdk?.Dispose();
            Console.WriteLine("Goodbye!");
        }

        #endregion

        #region Private-Methods

        private static void InitializeSdk(bool skipPrompt = false)
        {
            try
            {
                if (!skipPrompt)
                    _Endpoint = Inputty.GetString("Endpoint:", _Endpoint, false);

                _Sdk = new DocumentAtomSdk(_Endpoint, _AccessKey);
                _Sdk.LogRequests = _Debug;
                _Sdk.LogResponses = _Debug;
                _Sdk.Logger = (severity, message) =>
                {
                    if (_Debug || severity >= SeverityEnum.Warn)
                    {
                        Console.WriteLine($"[{severity}] {message}");
                    }
                };

                Console.WriteLine($"SDK initialized with endpoint: {_Endpoint}");
                if (!string.IsNullOrEmpty(_AccessKey))
                    Console.WriteLine($"Access key configured: {_AccessKey.Substring(0, Math.Min(8, _AccessKey.Length))}...");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing SDK: {ex.Message}");
            }
        }

        private static void ShowMenu()
        {
            Console.WriteLine();
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?               help, this menu");
            Console.WriteLine("  q               quit");
            Console.WriteLine("  cls             clear the screen");
            Console.WriteLine("  debug           enable or disable debug (enabled: " + _Debug + ")");
            Console.WriteLine("  endpoint        set the DocumentAtom server endpoint (currently: " + _Endpoint + ")");
            Console.WriteLine("  key             set the access key for authentication");
            Console.WriteLine();
            Console.WriteLine("Health & Status:");
            Console.WriteLine("  health          check if server is healthy");
            Console.WriteLine("  status          get server status");
            Console.WriteLine("  detect          test type detection");
            Console.WriteLine();
            Console.WriteLine("Document Processing:");
            Console.WriteLine("  csv             process CSV document");
            Console.WriteLine("  excel           process Excel document");
            Console.WriteLine("  html            process HTML document");
            Console.WriteLine("  json            process JSON document");
            Console.WriteLine("  markdown        process Markdown document");
            Console.WriteLine("  ocr             process image with OCR");
            Console.WriteLine("  pdf             process PDF document");
            Console.WriteLine("  png             process PNG image");
            Console.WriteLine("  powerpoint      process PowerPoint document");
            Console.WriteLine("  rtf             process RTF document");
            Console.WriteLine("  text            process text document");
            Console.WriteLine("  word            process Word document");
            Console.WriteLine("  xml             process XML document");
            Console.WriteLine();
        }

        private static void ToggleDebug()
        {
            _Debug = !_Debug;
            if (_Sdk != null)
            {
                _Sdk.LogRequests = _Debug;
                _Sdk.LogResponses = _Debug;
            }
            Console.WriteLine("Debug mode: " + (_Debug ? "enabled" : "disabled"));
        }

        private static void SetEndpoint()
        {
            string newEndpoint = Inputty.GetString("DocumentAtom server endpoint:", _Endpoint, false);
            if (!string.IsNullOrEmpty(newEndpoint))
            {
                _Endpoint = newEndpoint;
                InitializeSdk();
            }
        }

        private static void SetAccessKey()
        {
            string newKey = Inputty.GetString("Access key (ENTER to clear):", _AccessKey ?? "", true);
            _AccessKey = string.IsNullOrEmpty(newKey) ? null : newKey;
            InitializeSdk();
        }

        private static async Task TestHealth()
        {
            if (_Sdk == null)
            {
                Console.WriteLine("SDK not initialized.");
                return;
            }

            try
            {
                Console.WriteLine("Checking server health...");
                bool isHealthy = await _Sdk.Health.IsHealthy();
                Console.WriteLine($"Server is {(isHealthy ? "healthy" : "unhealthy")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error checking health: {ex.Message}");
            }
        }

        private static async Task TestStatus()
        {
            if (_Sdk == null)
            {
                Console.WriteLine("SDK not initialized.");
                return;
            }

            try
            {
                Console.WriteLine("Getting server status...");
                string? status = await _Sdk.Health.GetStatus();
                Console.WriteLine($"Server status: {status ?? "No response"}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting status: {ex.Message}");
            }
        }

        private static async Task TestTypeDetection()
        {
            if (_Sdk == null)
            {
                Console.WriteLine("SDK not initialized.");
                return;
            }

            try
            {
                string filename = Inputty.GetString("File path for type detection:", null, false);
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                string? contentType = Inputty.GetString("Content-type:", null, true);
                if (string.IsNullOrEmpty(contentType))
                {
                    contentType = null;
                }

                byte[] data = await File.ReadAllBytesAsync(filename);
                Console.WriteLine($"Detecting type for {filename} ({data.Length} bytes)...");
                if (!string.IsNullOrEmpty(contentType))
                {
                    Console.WriteLine($"Using content type hint: {contentType}");
                }

                TypeResult? result = await _Sdk.TypeDetection.DetectType(data, contentType);
                if (result != null)
                {
                    Console.WriteLine("Type detection result:");
                    Console.WriteLine($"  MIME Type: {result.MimeType ?? "Unknown"}");
                    Console.WriteLine($"  Extension: {result.Extension ?? "Unknown"}");
                    Console.WriteLine($"  Document Type: {result.Type}");
                    Console.WriteLine();
                    Console.WriteLine("Full JSON response:");
                    Console.WriteLine(JsonSerializer.Serialize(result, _JsonOptions));
                }
                else
                {
                    Console.WriteLine("No type detection result received.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in type detection: {ex.Message}");
                Console.WriteLine("This might be due to a JSON deserialization issue with the server response.");
            }
        }

        private static async Task TestCsvProcessing()
        {
            await TestDocumentProcessing("CSV", async (data, settings) =>
                await _Sdk!.Atom.ProcessCsv(data, settings));
        }

        private static async Task TestExcelProcessing()
        {
            await TestDocumentProcessing("Excel", async (data, settings) =>
                await _Sdk!.Atom.ProcessExcel(data, settings));
        }

        private static async Task TestHtmlProcessing()
        {
            await TestDocumentProcessing("HTML", async (data, settings) =>
                await _Sdk!.Atom.ProcessHtml(data, settings));
        }

        private static async Task TestJsonProcessing()
        {
            await TestDocumentProcessing("JSON", async (data, settings) =>
                await _Sdk!.Atom.ProcessJson(data, settings));
        }

        private static async Task TestMarkdownProcessing()
        {
            await TestDocumentProcessing("Markdown", async (data, settings) =>
                await _Sdk!.Atom.ProcessMarkdown(data, settings));
        }

        private static async Task TestOcrProcessing()
        {
            await TestDocumentProcessing("OCR", async (data, settings) =>
                await _Sdk!.Atom.ProcessOcr(data, settings));
        }

        private static async Task TestPdfProcessing()
        {
            await TestDocumentProcessing("PDF", async (data, settings) =>
                await _Sdk!.Atom.ProcessPdf(data, settings));
        }

        private static async Task TestPngProcessing()
        {
            await TestDocumentProcessing("PNG", async (data, settings) =>
                await _Sdk!.Atom.ProcessPng(data, settings));
        }

        private static async Task TestPowerPointProcessing()
        {
            await TestDocumentProcessing("PowerPoint", async (data, settings) =>
                await _Sdk!.Atom.ProcessPowerPoint(data, settings));
        }

        private static async Task TestRtfProcessing()
        {
            await TestDocumentProcessing("RTF", async (data, settings) =>
                await _Sdk!.Atom.ProcessRtf(data, settings));
        }

        private static async Task TestTextProcessing()
        {
            await TestDocumentProcessing("Text", async (data, settings) =>
                await _Sdk!.Atom.ProcessText(data, settings));
        }

        private static async Task TestWordProcessing()
        {
            await TestDocumentProcessing("Word", async (data, settings) =>
                await _Sdk!.Atom.ProcessWord(data, settings));
        }

        private static async Task TestXmlProcessing()
        {
            await TestDocumentProcessing("XML", async (data, settings) =>
                await _Sdk!.Atom.ProcessXml(data, settings));
        }

        private static async Task TestDocumentProcessing(string documentType, Func<byte[], ApiProcessorSettings?, Task<List<Atom>?>> processor)
        {
            if (_Sdk == null)
            {
                Console.WriteLine("SDK not initialized.");
                return;
            }

            try
            {
                string filename = Inputty.GetString($"File path for {documentType} processing:", null, false);
                if (string.IsNullOrEmpty(filename) || !File.Exists(filename))
                {
                    Console.WriteLine("File not found.");
                    return;
                }

                ApiProcessorSettings? settings = null;
                bool configureSettings = Inputty.GetBoolean("Configure processor settings?", false);
                if (configureSettings)
                {
                    settings = new ApiProcessorSettings();

                    if (documentType == "PDF" || documentType == "Word" || documentType == "Excel" ||
                        documentType == "PowerPoint" || documentType == "RTF")
                    {
                        bool extractOcr = Inputty.GetBoolean("Extract atoms from images (OCR)?", false);
                        if (extractOcr) settings.ExtractAtomsFromImages = true;
                    }

                    bool enableChunking = Inputty.GetBoolean("Enable chunking?", false);
                    if (enableChunking)
                    {
                        settings.Chunking = new DocumentAtom.Core.Chunking.ChunkingConfiguration
                        {
                            Enable = true
                        };

                        string strategy = Inputty.GetString(
                            "Chunking strategy [FixedTokenCount/SentenceBased/ParagraphBased/RegexBased/WholeList/ListEntry/Row/RowWithHeaders/RowGroupWithHeaders/KeyValuePairs/WholeTable]:",
                            "FixedTokenCount",
                            false);

                        if (Enum.TryParse<DocumentAtom.Core.Enums.ChunkStrategyEnum>(strategy, true, out DocumentAtom.Core.Enums.ChunkStrategyEnum parsedStrategy))
                            settings.Chunking.Strategy = parsedStrategy;

                        int fixedTokenCount = Inputty.GetInteger("Fixed token count:", 128, true, true);
                        settings.Chunking.FixedTokenCount = fixedTokenCount;

                        string overlapStrategy = Inputty.GetString(
                            "Overlap strategy [SlidingWindow/SentenceBoundaryAware/SemanticBoundaryAware]:",
                            "SlidingWindow",
                            false);

                        if (Enum.TryParse<DocumentAtom.Core.Enums.OverlapStrategyEnum>(overlapStrategy, true, out DocumentAtom.Core.Enums.OverlapStrategyEnum parsedOverlap))
                            settings.Chunking.OverlapStrategy = parsedOverlap;

                        int overlapCount = Inputty.GetInteger("Overlap count:", 0, true, true);
                        settings.Chunking.OverlapCount = overlapCount;
                    }
                }

                byte[] data = await File.ReadAllBytesAsync(filename);
                Console.WriteLine($"Processing {documentType} file: {filename} ({data.Length} bytes)...");

                DateTime startTime = DateTime.UtcNow;
                List<Atom>? atoms = await processor(data, settings);
                DateTime endTime = DateTime.UtcNow;

                if (atoms != null)
                {
                    Console.WriteLine($"Processing completed in {(endTime - startTime).TotalMilliseconds:F2}ms");
                    Console.WriteLine($"Extracted {atoms.Count} atoms:");
                    Console.WriteLine();

                    foreach (Atom atom in atoms.Take(5)) // Show first 5 atoms
                    {
                        string content = "";
                        if (atom.Type == AtomTypeEnum.Table)
                        {
                            content = $"Table with {atom.Rows} rows, {atom.Columns} columns";
                        }
                        else if (!string.IsNullOrEmpty(atom.Text))
                        {
                            content = atom.Text.Substring(0, Math.Min(300, atom.Text.Length));
                        }
                        else if (atom.UnorderedList != null && atom.UnorderedList.Count > 0)
                        {
                            content = $"Unordered list with {atom.UnorderedList.Count} items";
                        }
                        else if (atom.OrderedList != null && atom.OrderedList.Count > 0)
                        {
                            content = $"Ordered list with {atom.OrderedList.Count} items";
                        }
                        else
                        {
                            content = "No text content";
                        }

                        Console.WriteLine($"  Atom [{atom.Position}]: {atom.Type} - {content}");

                        if (atom.Chunks != null && atom.Chunks.Count > 0)
                        {
                            Console.WriteLine($"    Chunks: {atom.Chunks.Count}");
                            foreach (DocumentAtom.Core.Chunking.Chunk chunk in atom.Chunks.Take(3))
                            {
                                string chunkPreview = chunk.Text != null
                                    ? chunk.Text.Substring(0, Math.Min(100, chunk.Text.Length))
                                    : "No text";
                                Console.WriteLine($"      Chunk [{chunk.Position}]: {chunk.Length} chars - {chunkPreview}...");
                            }
                            if (atom.Chunks.Count > 3)
                                Console.WriteLine($"      ... and {atom.Chunks.Count - 3} more chunks");
                        }
                    }

                    if (atoms.Count > 5)
                    {
                        Console.WriteLine($"... and {atoms.Count - 5} more atoms");
                    }

                    // Summary of chunking results
                    int atomsWithChunks = atoms.Count(a => a.Chunks != null && a.Chunks.Count > 0);
                    int totalChunks = atoms.Where(a => a.Chunks != null).Sum(a => a.Chunks.Count);
                    if (atomsWithChunks > 0)
                    {
                        Console.WriteLine();
                        Console.WriteLine($"Chunking summary: {atomsWithChunks} atoms with chunks, {totalChunks} total chunks");
                    }

                    Console.WriteLine();
                    Console.WriteLine("Full result:");
                    Console.WriteLine(JsonSerializer.Serialize(atoms, _JsonOptions));
                }
                else
                {
                    Console.WriteLine("No atoms extracted.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing {documentType}: {ex.Message}");
            }
        }

        #endregion
    }
}
