namespace Test.OcrProcessor
{
    using System;
    using System.IO;
    using GetSomeInput;
    using DocumentAtom.Ocr;
    using SerializationHelper;
    using Timestamps;

    public static class Program
    {
        private static Serializer _Serializer = new Serializer();
        private static bool _ThroughputOnly = false;

        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("q")) _ThroughputOnly = true;

            while (true)
            {
                string filename = Inputty.GetString("File:", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                using (Timestamp ts = new Timestamp())
                {
                    ts.Start = DateTime.UtcNow;

                    using (ImageContentExtractor ice = new ImageContentExtractor(@"C:\Program Files\Tesseract-OCR\tessdata"))
                    {
                        ExtractionResult result = ice.ExtractContent(File.ReadAllBytes(filename));

                        if (!_ThroughputOnly)
                            Console.WriteLine(_Serializer.SerializeJson(result, true));

                        ts.End = DateTime.UtcNow;
                        Console.WriteLine("End of file: " + ts.TotalMs + "ms");
                        Console.WriteLine("");
                    }
                }
            }
        }
    }
}