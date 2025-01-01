namespace Test.PptxProcessor
{
    using System;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.PowerPoint;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static PptxProcessorSettings _PptxSettings = new PptxProcessorSettings();

        public static void Main(string[] args)
        {
            _PptxSettings.Chunking.Enable = true;
            _PptxSettings.Chunking.MaximumLength = 512;
            _PptxSettings.Chunking.ShiftSize = 384;

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                PptxProcessor processor = new PptxProcessor(_PptxSettings);
                foreach (PptxAtom atom in processor.Extract(filename))
                    Console.WriteLine(_Serializer.SerializeJson(atom, true));

                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}