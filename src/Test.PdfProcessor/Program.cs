namespace Test.PdfProcessor
{
    using System;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Image;
    using DocumentAtom.Pdf;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static PdfProcessorSettings _Settings = new PdfProcessorSettings();
        private static ImageProcessorSettings _ImageSettings = new ImageProcessorSettings();

        public static void Main(string[] args)
        {
            _Settings.Chunking.Enable = true;
            _Settings.Chunking.MaximumLength = 512;
            _Settings.Chunking.ShiftSize = 384;

            _ImageSettings.TesseractDataDirectory = "C:\\Program Files\\Tesseract-OCR\\tessdata";
            _ImageSettings.TesseractLanguage = "eng";

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                PdfProcessor processor = new PdfProcessor(_Settings, _ImageSettings);
                foreach (Atom atom in processor.Extract(filename))
                    Console.WriteLine(_Serializer.SerializeJson(atom, true));

                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}