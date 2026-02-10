namespace Test.NuGet
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using DocumentAtom.Core.TypeDetection;
    using DocumentAtom.Text;
    using DocumentAtom.Text.Csv;
    using DocumentAtom.Text.Json;
    using DocumentAtom.Text.Xml;
    using DocumentAtom.Text.Html;
    using DocumentAtom.Text.Markdown;
    using DocumentAtom.Documents.Word;
    using DocumentAtom.Documents.Excel;
    using DocumentAtom.Documents.PowerPoint;
    using DocumentAtom.Documents.Pdf;
    using DocumentAtom.Documents.RichText;
    using DocumentAtom.Documents.Image;
    using DocumentAtom.Documents.Ocr;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using SerializationHelper;

    /// <summary>
    /// Test application for DocumentAtom NuGet package.
    /// </summary>
    internal class Program
    {
        #region Private-Members

        private static bool _RunForever = true;
        private static Serializer _Serializer = new Serializer();

        #endregion

        #region Public-Methods

        /// <summary>
        /// Main entry point.
        /// </summary>
        /// <param name="args">Command line arguments.</param>
        public static void Main(string[] args)
        {
            Console.WriteLine("");
            Console.WriteLine("DocumentAtom NuGet Package Test Application");
            Console.WriteLine("");

            while (_RunForever)
            {
                string userInput = GetUserInput();
                if (string.IsNullOrEmpty(userInput)) continue;

                switch (userInput.ToLower())
                {
                    case "cls":
                        Console.Clear();
                        break;

                    case "q":
                    case "quit":
                    case "exit":
                        _RunForever = false;
                        break;

                    case "?":
                    case "help":
                    case "menu":
                        Menu();
                        break;

                    case "detect":
                    case "type":
                    case "td":
                        DetectType();
                        break;

                    case "text":
                    case "txt":
                        ProcessText();
                        break;

                    case "csv":
                        ProcessCsv();
                        break;

                    case "json":
                        ProcessJson();
                        break;

                    case "xml":
                        ProcessXml();
                        break;

                    case "html":
                    case "htm":
                        ProcessHtml();
                        break;

                    case "markdown":
                    case "md":
                        ProcessMarkdown();
                        break;

                    case "word":
                    case "docx":
                        ProcessWord();
                        break;

                    case "excel":
                    case "xlsx":
                        ProcessExcel();
                        break;

                    case "powerpoint":
                    case "pptx":
                    case "ppt":
                        ProcessPowerPoint();
                        break;

                    case "pdf":
                        ProcessPdf();
                        break;

                    case "rtf":
                        ProcessRtf();
                        break;

                    case "image":
                    case "img":
                    case "png":
                    case "jpg":
                    case "jpeg":
                        ProcessImage();
                        break;

                    case "ocr":
                        ProcessOcr();
                        break;

                    default:
                        Console.WriteLine("Unknown command. Type '?' for menu.");
                        break;
                }
            }

            Console.WriteLine("");
            Console.WriteLine("Goodbye!");
            Console.WriteLine("");
        }

        #endregion

        #region Private-Methods

        private static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available Commands:");
            Console.WriteLine("  ?               Display this menu (also: help, menu)");
            Console.WriteLine("  cls             Clear console");
            Console.WriteLine("  q               Quit (also: quit, exit)");
            Console.WriteLine("");
            Console.WriteLine("Type Detection:");
            Console.WriteLine("  td              Detect file type (also: detect, type)");
            Console.WriteLine("");
            Console.WriteLine("Text Processors:");
            Console.WriteLine("  text            Process plain text (also: txt)");
            Console.WriteLine("  csv             Process CSV");
            Console.WriteLine("  json            Process JSON");
            Console.WriteLine("  xml             Process XML");
            Console.WriteLine("  html            Process HTML (also: htm)");
            Console.WriteLine("  markdown        Process Markdown (also: md)");
            Console.WriteLine("");
            Console.WriteLine("Document Processors:");
            Console.WriteLine("  word            Process Word (also: docx)");
            Console.WriteLine("  excel           Process Excel (also: xlsx)");
            Console.WriteLine("  powerpoint      Process PowerPoint (also: pptx, ppt)");
            Console.WriteLine("  pdf             Process PDF");
            Console.WriteLine("  rtf             Process RTF");
            Console.WriteLine("  image           Process Image (also: img, png, jpg, jpeg)");
            Console.WriteLine("  ocr             Process OCR");
            Console.WriteLine("");
        }

        private static string GetUserInput()
        {
            Console.Write("Command [? for menu] > ");
            string userInput = Console.ReadLine();
            if (string.IsNullOrEmpty(userInput)) return null;
            return userInput.Trim();
        }

        private static string GetFilePath()
        {
            Console.Write("File path > ");
            string filePath = Console.ReadLine();
            if (string.IsNullOrEmpty(filePath)) return null;
            filePath = filePath.Trim();

            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found: " + filePath);
                return null;
            }

            return filePath;
        }

        private static void DetectType()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Type Detection ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                TypeDetector detector = new TypeDetector();
                detector.Logger = (msg) => Console.WriteLine("[TypeDetector] " + msg);

                TypeResult result = detector.Process(data);
                if (result != null)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Result:");
                    Console.WriteLine(_Serializer.SerializeJson(result, true));
                }
                else
                {
                    Console.WriteLine("Type detection returned null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessText()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Text Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                TextProcessor processor = new TextProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessCsv()
        {
            Console.WriteLine("");
            Console.WriteLine("=== CSV Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                CsvProcessor processor = new CsvProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessJson()
        {
            Console.WriteLine("");
            Console.WriteLine("=== JSON Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                JsonProcessor processor = new JsonProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessXml()
        {
            Console.WriteLine("");
            Console.WriteLine("=== XML Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                XmlProcessor processor = new XmlProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessHtml()
        {
            Console.WriteLine("");
            Console.WriteLine("=== HTML Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                HtmlProcessor processor = new HtmlProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessMarkdown()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Markdown Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                MarkdownProcessor processor = new MarkdownProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessWord()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Word Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                DocxProcessor processor = new DocxProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessExcel()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Excel Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                XlsxProcessor processor = new XlsxProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessPowerPoint()
        {
            Console.WriteLine("");
            Console.WriteLine("=== PowerPoint Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                PptxProcessor processor = new PptxProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessPdf()
        {
            Console.WriteLine("");
            Console.WriteLine("=== PDF Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                PdfProcessor processor = new PdfProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessRtf()
        {
            Console.WriteLine("");
            Console.WriteLine("=== RTF Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                RtfProcessor processor = new RtfProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessImage()
        {
            Console.WriteLine("");
            Console.WriteLine("=== Image Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);
                ImageProcessor processor = new ImageProcessor();
                processor.Logger = (sev, msg) => Console.WriteLine($"[{sev}] {msg}");

                List<Atom> atoms = processor.Extract(data).ToList();
                DisplayResults(atoms);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static void ProcessOcr()
        {
            Console.WriteLine("");
            Console.WriteLine("=== OCR Processor ===");
            string filePath = GetFilePath();
            if (string.IsNullOrEmpty(filePath)) return;

            try
            {
                byte[] data = File.ReadAllBytes(filePath);

                // Find tessdata directory
                string tessdataPath = FindTessdataPath();
                if (string.IsNullOrEmpty(tessdataPath))
                {
                    Console.WriteLine("Error: Could not find tessdata directory.");
                    Console.WriteLine("Please ensure Tesseract data files are available.");
                    return;
                }

                ImageContentExtractor extractor = new ImageContentExtractor(tessdataPath);
                ExtractionResult result = extractor.ExtractContent(data);

                if (result != null)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Result:");
                    Console.WriteLine(_Serializer.SerializeJson(result, true));
                }
                else
                {
                    Console.WriteLine("OCR extraction returned null.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
                Console.WriteLine(ex.StackTrace);
            }
        }

        private static string FindTessdataPath()
        {
            // Common tessdata locations
            string[] possiblePaths = new string[]
            {
                "./tessdata",
                "../tessdata",
                "../../tessdata",
                "../../../tessdata",
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../tessdata"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../tessdata")
            };

            foreach (string path in possiblePaths)
            {
                string fullPath = Path.GetFullPath(path);
                if (Directory.Exists(fullPath))
                {
                    Console.WriteLine($"Found tessdata at: {fullPath}");
                    return fullPath;
                }
            }

            return null;
        }

        private static void DisplayResults(List<Atom> atoms)
        {
            if (atoms == null || atoms.Count == 0)
            {
                Console.WriteLine("No atoms returned.");
                return;
            }

            Console.WriteLine("");
            Console.WriteLine("Atoms: " + atoms.Count);
            Console.WriteLine("");
            Console.WriteLine("Results:");
            Console.WriteLine(_Serializer.SerializeJson(atoms, true));
        }

        #endregion
    }
}
