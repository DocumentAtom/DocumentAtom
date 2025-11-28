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
            Console.WriteLine("  ?                    help, this menu");
            Console.WriteLine("  q                    quit");
            Console.WriteLine("  cls                  clear the screen");
            Console.WriteLine("  debug                enable or disable debug (enabled: " + _Debug + ")");
            Console.WriteLine("");
            Console.WriteLine("  test-image-process   test image processing with image file path");
            Console.WriteLine("  test-image-ocr       test OCR extraction from image file");
            Console.WriteLine("  test-csv-process     test CSV processing with CSV file path");
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
    }
}
