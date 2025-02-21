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
            TokenExtractor extractor = new TokenExtractor();

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                string[] lines = File.ReadAllLines(filename);

                foreach (string line in lines)
                {
                    Console.WriteLine(Environment.NewLine + Environment.NewLine + "Line: " + line);
                    foreach (string token in extractor.Process(line))
                        Console.Write(token + " ");
                }

                Console.WriteLine("");
                Console.WriteLine("End of file");
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}