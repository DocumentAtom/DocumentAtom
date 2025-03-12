namespace Test.PptxProcessor
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Image;
    using DocumentAtom.PowerPoint;
    using GetSomeInput;
    using SerializationHelper;
    using Timestamps;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static PptxProcessorSettings _ProcessorSettings = new PptxProcessorSettings();
        private static ImageProcessorSettings _ImageProcessorSettings = null;
        private static bool _ThroughputOnly = false;

        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("q")) _ThroughputOnly = true;

            _ProcessorSettings.Chunking.Enable = true;
            _ProcessorSettings.Chunking.MaximumLength = 512;
            _ProcessorSettings.Chunking.ShiftSize = 384;

            Console.WriteLine("");
            if (Inputty.GetBoolean("Enable OCR for images", true)) EnableOcr();

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                using (Timestamp ts = new Timestamp())
                {
                    ts.Start = DateTime.UtcNow;

                    using (PptxProcessor processor = new PptxProcessor(_ProcessorSettings, _ImageProcessorSettings))
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