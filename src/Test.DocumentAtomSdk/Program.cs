namespace Test.DocumentAtomSdk
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text.Json;
    using System.Threading.Tasks;
    using DocumentAtom.Sdk;  
    using DocumentAtom.Core.Enums;
    using DocumentAtom.TypeDetection;
    using GetSomeInput;
    using DocumentAtom.Core.Atoms;

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

            // Initialize SDK
            InitializeSdk();

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

        private static void InitializeSdk()
        {
            try
            {
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
            await TestDocumentProcessing("CSV", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessCsv(data, extractOcr));
        }

        private static async Task TestExcelProcessing()
        {
            await TestDocumentProcessing("Excel", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessExcel(data, extractOcr));
        }

        private static async Task TestHtmlProcessing()
        {
            await TestDocumentProcessing("HTML", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessHtml(data));
        }

        private static async Task TestJsonProcessing()
        {
            await TestDocumentProcessing("JSON", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessJson(data));
        }

        private static async Task TestMarkdownProcessing()
        {
            await TestDocumentProcessing("Markdown", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessMarkdown(data));
        }

        private static async Task TestOcrProcessing()
        {
            await TestDocumentProcessing("OCR", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessOcr(data));
        }

        private static async Task TestPdfProcessing()
        {
            await TestDocumentProcessing("PDF", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessPdf(data, extractOcr));
        }

        private static async Task TestPngProcessing()
        {
            await TestDocumentProcessing("PNG", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessPng(data));
        }

        private static async Task TestPowerPointProcessing()
        {
            await TestDocumentProcessing("PowerPoint", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessPowerPoint(data, extractOcr));
        }

        private static async Task TestRtfProcessing()
        {
            await TestDocumentProcessing("RTF", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessRtf(data, extractOcr));
        }

        private static async Task TestTextProcessing()
        {
            await TestDocumentProcessing("Text", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessText(data));
        }

        private static async Task TestWordProcessing()
        {
            await TestDocumentProcessing("Word", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessWord(data, extractOcr));
        }

        private static async Task TestXmlProcessing()
        {
            await TestDocumentProcessing("XML", async (data, extractOcr) =>
                await _Sdk!.Atom.ProcessXml(data));
        }

        private static async Task TestDocumentProcessing(string documentType, Func<byte[], bool, Task<List<Atom>?>> processor)
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

                bool extractOcr = false;
                if (documentType == "PDF" || documentType == "Word" || documentType == "Excel" ||
                    documentType == "PowerPoint" || documentType == "RTF")
                {
                    extractOcr = Inputty.GetBoolean("Extract OCR from images?", false);
                }

                byte[] data = await File.ReadAllBytesAsync(filename);
                Console.WriteLine($"Processing {documentType} file: {filename} ({data.Length} bytes)...");

                DateTime startTime = DateTime.UtcNow;
                List<Atom>? atoms = await processor(data, extractOcr);
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

                        Console.WriteLine($"Atom: {atom.Type} - {content}...");
                    }

                    if (atoms.Count > 5)
                    {
                        Console.WriteLine($"... and {atoms.Count - 5} more atoms");
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
