namespace DocumentAtom.McpServer.Classes
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;

    /// <summary>
    /// Tesseract settings.
    /// </summary>
    public class TesseractSettings
    {
        /// <summary>
        /// Data directory.
        /// </summary>
        public string? DataDirectory { get; set; }

        /// <summary>
        /// Language file prefix.
        /// </summary>
        public string Language { get; set; } = "eng";

        /// <summary>
        /// Tesseract settings.
        /// </summary>
        public TesseractSettings()
        {
            DataDirectory = GetDefaultTessDataPath();
        }

        private string? GetDefaultTessDataPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                string[] possiblePaths = new[]
                {
                    Path.Combine(programFiles, "Tesseract-OCR", "tessdata"),
                    Path.Combine(programFilesX86, "Tesseract-OCR", "tessdata")
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }

                return Path.Combine(programFiles, "Tesseract-OCR", "tessdata");
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                string[] possiblePaths = new[]
                {
                    "/usr/share/tessdata",
                    "/usr/local/share/tessdata"
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }

                return "/usr/share/tessdata";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                string[] possiblePaths = new[]
                {
                    "/usr/local/share/tessdata",
                    "/opt/homebrew/share/tessdata"
                };

                foreach (string path in possiblePaths)
                {
                    if (Directory.Exists(path))
                    {
                        return path;
                    }
                }

                return "/usr/local/share/tessdata";
            }

            return null;
        }
    }
}
