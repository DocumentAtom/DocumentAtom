namespace Test.PdfProcessor
{
    using System;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Pdf;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static PdfProcessorSettings _Settings = new PdfProcessorSettings();

        public static void Main(string[] args)
        {
            _Settings.Chunking.Enable = true;
            _Settings.Chunking.MaximumLength = 512;
            _Settings.Chunking.ShiftSize = 384;

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                PdfProcessor processor = new PdfProcessor(_Settings);
                foreach (PdfAtom atom in processor.Extract(filename))
                    Console.WriteLine(_Serializer.SerializeJson(atom, true));

                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}