﻿namespace Test.OcrProcessor
{
    using System;
    using System.IO;
    using GetSomeInput;
    using DocumentAtom.Ocr;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new Serializer();

        public static void Main(string[] args)
        {
            while (true)
            {
                string filename = Inputty.GetString("File:", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                using (ImageContentExtractor ice = new ImageContentExtractor(@"C:\Program Files\Tesseract-OCR\tessdata"))
                {
                    Console.WriteLine(_Serializer.SerializeJson(ice.ExtractContent(File.ReadAllBytes(filename)), true));
                }
            }
        }
    }
}