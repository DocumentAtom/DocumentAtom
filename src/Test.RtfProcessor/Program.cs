namespace Test.RtfProcessor
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Image;
    using DocumentAtom.RichText;
    using GetSomeInput;
    using SerializationHelper;
    using Timestamps;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static RtfProcessorSettings _ProcessorSettings = new RtfProcessorSettings();
        private static ImageProcessorSettings _ImageProcessorSettings = null;
        private static bool _ThroughputOnly = false;

        public static void Main(string[] args)
        {
            bool skipOcr = false;
            if (args != null && args.Length > 0)
            {
                if (args[0].Equals("q")) _ThroughputOnly = true;
                if (args.Any(a => a.Equals("noocr"))) skipOcr = true;
            }

            _ProcessorSettings.Chunking.Enable = true;
            _ProcessorSettings.Chunking.MaximumLength = 512;
            _ProcessorSettings.Chunking.ShiftSize = 384;

            Console.WriteLine("");
            if (!skipOcr && Inputty.GetBoolean("Enable OCR for images", true)) EnableOcr();

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                using (Timestamp ts = new Timestamp())
                {
                    ts.Start = DateTime.UtcNow;

                    using (RtfProcessor processor = new RtfProcessor(_ProcessorSettings, _ImageProcessorSettings))
                    {
                        foreach (Atom atom in processor.Extract(filename))
                        {
                            if (!_ThroughputOnly)
                                Console.WriteLine(_Serializer.SerializeJson(atom, true));
                        }
                    }

                    ts.End = DateTime.UtcNow;
                    Console.WriteLine("End of file: " + ts.TotalMs + "ms");
                    Console.WriteLine("");
                }
            }

            Console.WriteLine("");
        }

        private static void EnableOcr()
        {
            string defaultDirectory = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                defaultDirectory = Path.Combine(@"C:\Program Files\Tesseract-OCR", "tessdata");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                defaultDirectory = "/usr/share/tessdata";
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                defaultDirectory = "/usr/local/share/tessdata";

            string tessData = Inputty.GetString("Tesseract data directory :", defaultDirectory, false);
            string language = Inputty.GetString("Tesseract language       :", "eng", false);

            _ImageProcessorSettings = new ImageProcessorSettings
            {
                TesseractDataDirectory = tessData,
                TesseractLanguage = language
            };
        }
    }
}
