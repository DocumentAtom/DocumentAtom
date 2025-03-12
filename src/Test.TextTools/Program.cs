namespace Test.TextTools
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using DocumentAtom.TextTools;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();

        public static void Main(string[] args)
        {
            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                string[] lines = File.ReadAllLines(filename);

                Console.WriteLine("");
                Console.WriteLine("Token extraction");
                foreach (string line in lines)
                {
                    Console.WriteLine(Environment.NewLine + Environment.NewLine + "Line: " + line);

                    using (TokenExtractor extractor = new TokenExtractor())
                    {
                        foreach (string token in extractor.Process(line))
                            Console.Write(token + " ");
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("");
                Console.WriteLine("Chunk extraction (max words 10, max length 60)");
                foreach (string line in lines)
                {
                    Console.WriteLine(Environment.NewLine + Environment.NewLine + "Line: " + line);

                    using (TokenExtractor extractor = new TokenExtractor())
                    {
                        foreach (string chunk in extractor.Chunk(line, 10, 60))
                        {
                            Console.WriteLine(chunk);
                        }
                    }
                }

                Console.WriteLine("");
                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}