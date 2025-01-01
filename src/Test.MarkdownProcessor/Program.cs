namespace Test.MarkdownProcessor
{
    using System;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Markdown;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static MarkdownProcessorSettings _MarkdownSettings = new MarkdownProcessorSettings();

        public static void Main(string[] args)
        {
            _MarkdownSettings.Chunking.Enable = true;
            _MarkdownSettings.Chunking.MaximumLength = 512;
            _MarkdownSettings.Chunking.ShiftSize = 384;

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                MarkdownProcessor processor = new MarkdownProcessor(_MarkdownSettings);
                foreach (MarkdownAtom atom in processor.Extract(filename))
                    Console.WriteLine(_Serializer.SerializeJson(atom, true));
                
                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}