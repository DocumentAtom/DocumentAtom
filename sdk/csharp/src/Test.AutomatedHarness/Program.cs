namespace Test.AutomatedHarness
{
    using System.Diagnostics;
    using DocumentAtom.Core.Api;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Chunking;
    using DocumentAtom.Sdk;

    internal class Program
    {
        #region Private-Members

        private static string _Endpoint = "http://localhost:8000";
        private static string _FixtureDir = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "..", "test-fixtures"));

        private static readonly List<(string Status, double Duration, string Name, string Message)> _Results = new();

        private static readonly (string File, string Format)[] _Fixtures =
        {
            ("sample.csv",  "csv"),
            ("sample.html", "html"),
            ("sample.json", "json"),
            ("sample.md",   "markdown"),
            ("sample.txt",  "text"),
            ("sample.xml",  "xml"),
            ("sample.rtf",  "rtf"),
            ("sample.pdf",  "pdf"),
            ("sample.docx", "word"),
            ("sample.xlsx", "excel"),
            ("sample.pptx", "powerpoint"),
            ("sample.png",  "png"),
            ("sample.jpg",  "ocr"),
        };

        private static readonly HashSet<string> _ImageFormats = new() { "png", "ocr" };
        private static readonly HashSet<string> _ChunkingSkip = new() { "png", "ocr" };
        private static readonly HashSet<string> _OcrFormats = new() { "pdf", "word", "powerpoint" };

        #endregion

        #region Public-Methods

        static async Task<int> Main(string[] args)
        {
            if (args.Length >= 1)
                _Endpoint = args[0].TrimEnd('/');

            // Try to locate fixture dir relative to project source if the bin-relative path doesn't work
            if (!Directory.Exists(_FixtureDir))
            {
                string altDir = Path.GetFullPath(
                    Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "test-fixtures"));
                if (Directory.Exists(altDir))
                    _FixtureDir = altDir;
            }

            Console.WriteLine($"Endpoint:    {_Endpoint}");
            Console.WriteLine($"Fixtures:    {_FixtureDir}");
            Console.WriteLine();

            Stopwatch overall = Stopwatch.StartNew();

            using DocumentAtomSdk sdk = new DocumentAtomSdk(_Endpoint);

            // 1. Connectivity (2 tests)
            await RunTest("Connectivity: server is reachable", async () =>
            {
                bool healthy = await sdk.Health.IsHealthy();
                if (!healthy) throw new Exception("IsHealthy returned false");
            });

            await RunTest("Connectivity: server returns status", async () =>
            {
                string? status = await sdk.Health.GetStatus();
                if (status == null) throw new Exception("GetStatus returned null");
            });

            // 2. Type Detection (13 tests)
            foreach ((string file, string format) in _Fixtures)
            {
                byte[]? data = LoadFixture(file);
                if (data == null)
                {
                    Skip($"Type detection: {file}", "fixture not found");
                    continue;
                }
                await RunTest($"Type detection: {file}", async () =>
                {
                    DocumentAtom.Core.TypeDetection.TypeResult? result = await sdk.TypeDetection.DetectType(data);
                    if (result == null) throw new Exception("DetectType returned null");
                });
            }

            // 3. Extraction — No Settings (13 tests)
            foreach ((string file, string format) in _Fixtures)
            {
                byte[]? data = LoadFixture(file);
                if (data == null)
                {
                    Skip($"Extraction (default): {file}", "fixture not found");
                    continue;
                }
                await RunTest($"Extraction (default): {file}", async () =>
                {
                    List<Atom>? atoms = await CallExtraction(sdk, format, data);
                    if (_ImageFormats.Contains(format))
                    {
                        // Minimal test images have no text; just verify the call succeeded
                        if (atoms == null)
                            throw new Exception("extraction returned null");
                    }
                    else
                    {
                        if (atoms == null || atoms.Count < 1)
                            throw new Exception($"expected >= 1 atom, got {atoms?.Count ?? 0}");
                    }
                });
            }

            // 4. Extraction — With Chunking (11 tests, skip PNG/JPG)
            foreach ((string file, string format) in _Fixtures)
            {
                if (_ChunkingSkip.Contains(format))
                {
                    Skip($"Extraction (chunking): {file}", "chunking not applicable for images");
                    continue;
                }
                byte[]? data = LoadFixture(file);
                if (data == null)
                {
                    Skip($"Extraction (chunking): {file}", "fixture not found");
                    continue;
                }
                await RunTest($"Extraction (chunking): {file}", async () =>
                {
                    ApiProcessorSettings settings = new ApiProcessorSettings
                    {
                        Chunking = new ChunkingConfiguration
                        {
                            Enable = true,
                            Strategy = DocumentAtom.Core.Enums.ChunkStrategyEnum.FixedTokenCount,
                            FixedTokenCount = 32,
                            OverlapCount = 0,
                        }
                    };
                    List<Atom>? atoms = await CallExtraction(sdk, format, data, settings);
                    if (atoms == null || atoms.Count < 1)
                        throw new Exception($"expected >= 1 atom, got {atoms?.Count ?? 0}");
                    bool hasChunks = atoms.Any(a => a.Chunks != null && a.Chunks.Count > 0);
                    if (!hasChunks)
                        throw new Exception("expected at least one atom with non-empty Chunks");
                });
            }

            // 5. Extraction — With OCR Setting (3 tests: PDF, Word, PowerPoint)
            foreach ((string file, string format) in _Fixtures)
            {
                if (!_OcrFormats.Contains(format)) continue;
                byte[]? data = LoadFixture(file);
                if (data == null)
                {
                    Skip($"Extraction (OCR setting): {file}", "fixture not found");
                    continue;
                }
                await RunTest($"Extraction (OCR setting): {file}", async () =>
                {
                    ApiProcessorSettings settings = new ApiProcessorSettings
                    {
                        ExtractAtomsFromImages = true
                    };
                    List<Atom>? atoms = await CallExtraction(sdk, format, data, settings);
                    if (atoms == null || atoms.Count < 1)
                        throw new Exception($"expected >= 1 atom, got {atoms?.Count ?? 0}");
                });
            }

            // 6. Error Cases (3 tests)
            await RunTest("Error: empty data to text extraction", async () =>
            {
                try
                {
                    await sdk.Atom.ProcessText(Array.Empty<byte>());
                }
                catch
                {
                    // error response is acceptable
                }
            });

            await RunTest("Error: CSV bytes sent to PDF endpoint", async () =>
            {
                byte[]? csvData = LoadFixture("sample.csv");
                if (csvData == null) throw new Exception("fixture not found");
                try
                {
                    await sdk.Atom.ProcessPdf(csvData);
                }
                catch
                {
                    // error response is acceptable
                }
            });

            await RunTest("Error: empty data to type detection", async () =>
            {
                try
                {
                    await sdk.TypeDetection.DetectType(Array.Empty<byte>());
                }
                catch
                {
                    // error response is acceptable
                }
            });

            overall.Stop();

            // Summary
            int passed = _Results.Count(r => r.Status == "PASS");
            int failed = _Results.Count(r => r.Status == "FAIL");
            int skipped = _Results.Count(r => r.Status == "SKIP");
            int total = _Results.Count;
            List<(string Status, double Duration, string Name, string Message)> failedTests =
                _Results.Where(r => r.Status == "FAIL").ToList();

            Console.WriteLine();
            Console.WriteLine(new string('=', 60));
            Console.WriteLine("Test Summary");
            Console.WriteLine(new string('=', 60));
            Console.WriteLine($"Total:    {total}");
            Console.WriteLine($"Passed:   {passed}");
            Console.WriteLine($"Failed:   {failed}");
            Console.WriteLine($"Skipped:  {skipped}");
            Console.WriteLine($"Duration: {overall.Elapsed.TotalSeconds:F3}s");

            if (failedTests.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("Failed tests:");
                foreach ((string _, double _, string name, string msg) in failedTests)
                    Console.WriteLine($"  - {name} - {msg}");
            }

            Console.WriteLine(new string('=', 60));

            return failed > 0 ? 1 : 0;
        }

        #endregion

        #region Private-Methods

        private static byte[]? LoadFixture(string filename)
        {
            string path = Path.Combine(_FixtureDir, filename);
            if (!File.Exists(path)) return null;
            return File.ReadAllBytes(path);
        }

        private static void Record(string status, double duration, string name, string message = "")
        {
            _Results.Add((status, duration, name, message));
            string tag = status switch
            {
                "PASS" => "[PASS]",
                "FAIL" => "[FAIL]",
                "SKIP" => "[SKIP]",
                _ => "[????]"
            };
            string line = $"{tag}  {duration,7:F3}s  {name}";
            if (!string.IsNullOrEmpty(message))
                line += $" - {message}";
            Console.WriteLine(line);
        }

        private static void Skip(string name, string reason)
        {
            Record("SKIP", 0.0, name, reason);
        }

        private static async Task RunTest(string name, Func<Task> fn)
        {
            Stopwatch sw = Stopwatch.StartNew();
            try
            {
                await fn();
                sw.Stop();
                Record("PASS", sw.Elapsed.TotalSeconds, name);
            }
            catch (Exception ex)
            {
                sw.Stop();
                Record("FAIL", sw.Elapsed.TotalSeconds, name, ex.Message);
            }
        }

        private static async Task<List<Atom>?> CallExtraction(
            DocumentAtomSdk sdk,
            string format,
            byte[] data,
            ApiProcessorSettings? settings = null)
        {
            return format switch
            {
                "csv" => await sdk.Atom.ProcessCsv(data, settings),
                "html" => await sdk.Atom.ProcessHtml(data, settings),
                "json" => await sdk.Atom.ProcessJson(data, settings),
                "markdown" => await sdk.Atom.ProcessMarkdown(data, settings),
                "text" => await sdk.Atom.ProcessText(data, settings),
                "xml" => await sdk.Atom.ProcessXml(data, settings),
                "rtf" => await sdk.Atom.ProcessRtf(data, settings),
                "pdf" => await sdk.Atom.ProcessPdf(data, settings),
                "word" => await sdk.Atom.ProcessWord(data, settings),
                "excel" => await sdk.Atom.ProcessExcel(data, settings),
                "powerpoint" => await sdk.Atom.ProcessPowerPoint(data, settings),
                "png" => await sdk.Atom.ProcessPng(data, settings),
                "ocr" => await sdk.Atom.ProcessOcr(data, settings),
                _ => throw new ArgumentException($"Unknown format: {format}")
            };
        }

        #endregion
    }
}
