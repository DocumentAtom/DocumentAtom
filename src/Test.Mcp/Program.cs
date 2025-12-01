namespace Test.Mcp
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using GetSomeInput;
    using Voltaic;

    class Program
    {
        static bool _RunForever = true;
        static bool _Debug = false;
        static McpHttpClient? _McpClient = null;
        static string _McpServerUrl = "http://localhost:8200";

        static Task Main(string[] args)
        {
            return MainAsync(args, CancellationToken.None);
        }

        static async Task MainAsync(string[] args, CancellationToken token = default)
        {
            Console.WriteLine("DocumentAtom MCP Server Test Console");
            Console.WriteLine("====================================");
            Console.WriteLine("");

            _McpClient = new McpHttpClient();
            _McpClient.Log += (sender, msg) =>
            {
                if (_Debug)
                {
                    Console.WriteLine("[MCP] " + msg);
                }
            };

            Console.WriteLine($"Connecting to MCP server at {_McpServerUrl}...");
            bool connected = await _McpClient.ConnectAsync(_McpServerUrl, "/rpc", "/events", token).ConfigureAwait(false);

            if (!connected)
            {
                Console.WriteLine($"Failed to connect to MCP server at {_McpServerUrl}");
                Console.WriteLine("Make sure the MCP server is running.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine("Connected successfully!");
            Console.WriteLine("");

            while (_RunForever)
            {
                string userInput = Inputty.GetString("Command [? for help]:", null, false);

                if (userInput.Equals("?")) Menu();
                else if (userInput.Equals("q")) _RunForever = false;
                else if (userInput.Equals("cls")) Console.Clear();
                else if (userInput.Equals("debug")) ToggleDebug();
                else if (userInput.Equals("test-image-process")) await TestImageProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-image-ocr")) await TestImageOcr(token).ConfigureAwait(false);
                else if (userInput.Equals("test-csv-process")) await TestCsvProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-excel-process")) await TestExcelProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-html-process")) await TestHtmlProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-json-process")) await TestJsonProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-markdown-process")) await TestMarkdownProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-ocr-process")) await TestOcrProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-pdf-process")) await TestPdfProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-powerpoint-process")) await TestPowerPointProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-richtext-process")) await TestRichTextProcess(token).ConfigureAwait(false);
                else if (userInput.Equals("test-text-process")) await TestTextProcess(token).ConfigureAwait(false);
                else
                {
                    Console.WriteLine("Unknown command. Type '?' for help.");
                }
            }
        }

        static void Menu()
        {
            Console.WriteLine("");
            Console.WriteLine("Available commands:");
            Console.WriteLine("  ?                              help, this menu");
            Console.WriteLine("  q                              quit");
            Console.WriteLine("  cls                            clear the screen");
            Console.WriteLine("  debug                          enable or disable debug (enabled: " + _Debug + ")");
            Console.WriteLine("");
            Console.WriteLine("  test-image-process             test image processing with image file path");
            Console.WriteLine("  test-image-ocr                 test OCR extraction from image file");
            Console.WriteLine("  test-csv-process               test CSV processing with CSV file path");
            Console.WriteLine("  test-excel-process             test Excel processing with Excel file path");
            Console.WriteLine("  test-html-process              test HTML processing with HTML file path");
            Console.WriteLine("  test-json-process              test JSON processing with JSON file path");
            Console.WriteLine("  test-markdown-process          test Markdown processing with Markdown file path");
            Console.WriteLine("  test-ocr-process               test OCR processing with image file path");
            Console.WriteLine("  test-pdf-process               test PDF processing with PDF file path");
            Console.WriteLine("  test-powerpoint-process        test PowerPoint processing with PowerPoint file path");
            Console.WriteLine("  test-richtext-process          test Rich Text processing with RTF file path");
            Console.WriteLine("  test-text-process              test text processing with text file path");
            Console.WriteLine("");
        }

        static void ToggleDebug()
        {
            _Debug = !_Debug;
            Console.WriteLine("Debug mode: " + (_Debug ? "enabled" : "disabled"));
        }

        static async Task TestImageProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing image processing...");

                // Get image file path from user
                string imagePath = Inputty.GetString("Enter image file path:", null, false);
                if (string.IsNullOrEmpty(imagePath))
                {
                    Console.WriteLine("No image path provided.");
                    return;
                }

                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return;
                }

                // Read the image file
                byte[] imageData = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageData);
                string filename = Path.GetFileName(imagePath);


                Console.WriteLine($"Processing image: {filename} ({imageData.Length} bytes)");
                var request = new
                {
                    data = base64Image
                };

                Console.WriteLine("Sending image processing request...");
                var result = await _McpClient!.CallAsync<string>("image/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("Image processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in image processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestImageOcr(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing OCR extraction...");

                // Get image file path from user
                string imagePath = Inputty.GetString("Enter image file path:", null, false);
                if (string.IsNullOrEmpty(imagePath))
                {
                    Console.WriteLine("No image path provided.");
                    return;
                }

                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return;
                }

                // Read the image file
                byte[] imageData = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageData);
                string filename = Path.GetFileName(imagePath);

                Console.WriteLine($"Extracting OCR from image: {filename} ({imageData.Length} bytes)");
                var request = new
                {
                    data = base64Image
                };

                Console.WriteLine("Sending OCR extraction request...");
                var result = await _McpClient!.CallAsync<string>("image/ocr", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("OCR extraction result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OCR test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestCsvProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing CSV processing...");

                // Get CSV file path from user
                string csvPath = Inputty.GetString("Enter CSV file path:", null, false);
                if (string.IsNullOrEmpty(csvPath))
                {
                    Console.WriteLine("No CSV path provided.");
                    return;
                }

                if (!File.Exists(csvPath))
                {
                    Console.WriteLine($"CSV file not found: {csvPath}");
                    return;
                }

                byte[] csvData = File.ReadAllBytes(csvPath);
                string base64Csv = Convert.ToBase64String(csvData);
                string filename = Path.GetFileName(csvPath);

                bool extractOcr = Inputty.GetBoolean("Extract text from images using OCR?", false);

                var request = new
                {
                    data = base64Csv,
                    extractOcr = extractOcr
                };

                Console.WriteLine($"Processing CSV: {filename} ({csvData.Length} bytes)");
                if (extractOcr)
                {
                    Console.WriteLine("OCR extraction enabled");
                }
                Console.WriteLine("Sending CSV processing request...");
                var result = await _McpClient!.CallAsync<string>("csv/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("CSV processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CSV processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestExcelProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing Excel processing...");

                // Get Excel file path from user
                string excelPath = Inputty.GetString("Enter Excel file path:", null, false);
                if (string.IsNullOrEmpty(excelPath))
                {
                    Console.WriteLine("No Excel path provided.");
                    return;
                }

                if (!File.Exists(excelPath))
                {
                    Console.WriteLine($"Excel file not found: {excelPath}");
                    return;
                }

                // Read the Excel file
                byte[] excelData = File.ReadAllBytes(excelPath);
                string base64Excel = Convert.ToBase64String(excelData);
                string filename = Path.GetFileName(excelPath);

                // Ask if user wants OCR extraction
                bool extractOcr = Inputty.GetBoolean("Extract text from images using OCR?", false);

                var request = new
                {
                    data = base64Excel,
                    extractOcr = extractOcr
                };

                Console.WriteLine($"Processing Excel: {filename} ({excelData.Length} bytes)");
                if (extractOcr)
                {
                    Console.WriteLine("OCR extraction enabled");
                }
                Console.WriteLine("Sending Excel processing request...");
                var result = await _McpClient!.CallAsync<string>("excel/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("Excel processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Excel processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestHtmlProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing HTML processing...");

                // Get HTML file path from user
                string htmlPath = Inputty.GetString("Enter HTML file path:", null, false);
                if (string.IsNullOrEmpty(htmlPath))
                {
                    Console.WriteLine("No HTML path provided.");
                    return;
                }

                if (!File.Exists(htmlPath))
                {
                    Console.WriteLine($"HTML file not found: {htmlPath}");
                    return;
                }

                // Read the HTML file
                byte[] htmlData = File.ReadAllBytes(htmlPath);
                string base64Html = Convert.ToBase64String(htmlData);
                string filename = Path.GetFileName(htmlPath);

                var request = new
                {
                    data = base64Html
                };

                Console.WriteLine($"Processing HTML: {filename} ({htmlData.Length} bytes)");
                Console.WriteLine("Sending HTML processing request...");
                var result = await _McpClient!.CallAsync<string>("html/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("HTML processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in HTML processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestJsonProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing JSON processing...");

                // Get JSON file path from user
                string jsonPath = Inputty.GetString("Enter JSON file path:", null, false);
                if (string.IsNullOrEmpty(jsonPath))
                {
                    Console.WriteLine("No JSON path provided.");
                    return;
                }

                if (!File.Exists(jsonPath))
                {
                    Console.WriteLine($"JSON file not found: {jsonPath}");
                    return;
                }

                // Read the JSON file
                byte[] jsonData = File.ReadAllBytes(jsonPath);
                string base64Json = Convert.ToBase64String(jsonData);
                string filename = Path.GetFileName(jsonPath);

                var request = new
                {
                    data = base64Json
                };

                Console.WriteLine($"Processing JSON: {filename} ({jsonData.Length} bytes)");
                Console.WriteLine("Sending JSON processing request...");
                var result = await _McpClient!.CallAsync<string>("json/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("JSON processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in JSON processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestMarkdownProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing Markdown processing...");

                // Get Markdown file path from user
                string markdownPath = Inputty.GetString("Enter Markdown file path:", null, false);
                if (string.IsNullOrEmpty(markdownPath))
                {
                    Console.WriteLine("No Markdown path provided.");
                    return;
                }

                if (!File.Exists(markdownPath))
                {
                    Console.WriteLine($"Markdown file not found: {markdownPath}");
                    return;
                }

                // Read the Markdown file
                byte[] markdownData = File.ReadAllBytes(markdownPath);
                string base64Markdown = Convert.ToBase64String(markdownData);
                string filename = Path.GetFileName(markdownPath);

                var request = new
                {
                    data = base64Markdown
                };

                Console.WriteLine($"Processing Markdown: {filename} ({markdownData.Length} bytes)");
                Console.WriteLine("Sending Markdown processing request...");
                var result = await _McpClient!.CallAsync<string>("markdown/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("Markdown processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Markdown processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestOcrProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing OCR processing...");

                // Get image file path from user
                string imagePath = Inputty.GetString("Enter image file path:", null, false);
                if (string.IsNullOrEmpty(imagePath))
                {
                    Console.WriteLine("No image path provided.");
                    return;
                }

                if (!File.Exists(imagePath))
                {
                    Console.WriteLine($"Image file not found: {imagePath}");
                    return;
                }

                // Read the image file
                byte[] imageData = File.ReadAllBytes(imagePath);
                string base64Image = Convert.ToBase64String(imageData);
                string filename = Path.GetFileName(imagePath);

                var request = new
                {
                    data = base64Image
                };

                Console.WriteLine($"Processing image with OCR: {filename} ({imageData.Length} bytes)");
                Console.WriteLine("Sending OCR processing request...");
                var result = await _McpClient!.CallAsync<string>("ocr/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("OCR processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OCR processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestPdfProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing PDF processing...");

                // Get PDF file path from user
                string pdfPath = Inputty.GetString("Enter PDF file path:", null, false);
                if (string.IsNullOrEmpty(pdfPath))
                {
                    Console.WriteLine("No PDF path provided.");
                    return;
                }

                if (!File.Exists(pdfPath))
                {
                    Console.WriteLine($"PDF file not found: {pdfPath}");
                    return;
                }

                // Read the PDF file
                byte[] pdfData = File.ReadAllBytes(pdfPath);
                string base64Pdf = Convert.ToBase64String(pdfData);
                string filename = Path.GetFileName(pdfPath);

                // Ask if user wants OCR extraction
                bool extractOcr = Inputty.GetBoolean("Extract text from images using OCR?", false);

                var request = new
                {
                    data = base64Pdf,
                    extractOcr = extractOcr
                };

                Console.WriteLine($"Processing PDF: {filename} ({pdfData.Length} bytes)");
                if (extractOcr)
                {
                    Console.WriteLine("OCR extraction enabled");
                }
                Console.WriteLine("Sending PDF processing request...");
                var result = await _McpClient!.CallAsync<string>("pdf/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("PDF processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PDF processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestPowerPointProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing PowerPoint processing...");

                // Get PowerPoint file path from user
                string powerPointPath = Inputty.GetString("Enter PowerPoint file path:", null, false);
                if (string.IsNullOrEmpty(powerPointPath))
                {
                    Console.WriteLine("No PowerPoint path provided.");
                    return;
                }

                if (!File.Exists(powerPointPath))
                {
                    Console.WriteLine($"PowerPoint file not found: {powerPointPath}");
                    return;
                }

                // Read the PowerPoint file
                byte[] powerPointData = File.ReadAllBytes(powerPointPath);
                string base64PowerPoint = Convert.ToBase64String(powerPointData);
                string filename = Path.GetFileName(powerPointPath);

                // Ask if user wants OCR extraction
                bool extractOcr = Inputty.GetBoolean("Extract text from images using OCR?", false);

                var request = new
                {
                    data = base64PowerPoint,
                    extractOcr = extractOcr
                };

                Console.WriteLine($"Processing PowerPoint: {filename} ({powerPointData.Length} bytes)");
                if (extractOcr)
                {
                    Console.WriteLine("OCR extraction enabled");
                }
                Console.WriteLine("Sending PowerPoint processing request...");
                var result = await _McpClient!.CallAsync<string>("powerpoint/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("PowerPoint processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PowerPoint processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestRichTextProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing Rich Text processing...");

                // Get Rich Text file path from user
                string richTextPath = Inputty.GetString("Enter Rich Text file path:", null, false);
                if (string.IsNullOrEmpty(richTextPath))
                {
                    Console.WriteLine("No Rich Text path provided.");
                    return;
                }

                if (!File.Exists(richTextPath))
                {
                    Console.WriteLine($"Rich Text file not found: {richTextPath}");
                    return;
                }

                // Read the Rich Text file
                byte[] richTextData = File.ReadAllBytes(richTextPath);
                string base64RichText = Convert.ToBase64String(richTextData);
                string filename = Path.GetFileName(richTextPath);

                // Ask if user wants OCR extraction
                bool extractOcr = Inputty.GetBoolean("Extract text from images using OCR?", false);

                var request = new
                {
                    data = base64RichText,
                    extractOcr = extractOcr
                };

                Console.WriteLine($"Processing Rich Text: {filename} ({richTextData.Length} bytes)");
                if (extractOcr)
                {
                    Console.WriteLine("OCR extraction enabled");
                }
                Console.WriteLine("Sending Rich Text processing request...");
                var result = await _McpClient!.CallAsync<string>("richtext/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("Rich Text processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Rich Text processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        static async Task TestTextProcess(CancellationToken token = default)
        {
            try
            {
                Console.WriteLine("Testing Text processing...");

                // Get text file path from user
                string textPath = Inputty.GetString("Enter text file path:", null, false);
                if (string.IsNullOrEmpty(textPath))
                {
                    Console.WriteLine("No text path provided.");
                    return;
                }

                if (!File.Exists(textPath))
                {
                    Console.WriteLine($"Text file not found: {textPath}");
                    return;
                }

                // Read the text file
                byte[] textData = File.ReadAllBytes(textPath);
                string base64Text = Convert.ToBase64String(textData);
                string filename = Path.GetFileName(textPath);

                var request = new
                {
                    data = base64Text
                };

                Console.WriteLine($"Processing text: {filename} ({textData.Length} bytes)");
                Console.WriteLine("Sending text processing request...");
                var result = await _McpClient!.CallAsync<string>("text/process", request, 60000, token).ConfigureAwait(false);

                Console.WriteLine("Text processing result:");
                Console.WriteLine(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in text processing test: {ex.Message}");
                if (_Debug)
                {
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }


    }
}
