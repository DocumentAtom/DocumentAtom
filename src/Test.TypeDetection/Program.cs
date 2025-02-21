namespace Test.TypeDetection
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Text;
    using DocumentAtom.TypeDetection;
    using GetSomeInput;
    using SerializationHelper;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();

        public static void Main(string[] args)
        {
            TypeDetector typeDetector = new TypeDetector();

            while (true)
            {
                string filename =    Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                Console.WriteLine("Content-type is required for detection of CSV files, in such cases use text/csv");
                string contentType = Inputty.GetString("Content type           :", null, true);

                TypeResult tr = typeDetector.Process(filename, contentType);
                Console.WriteLine(_Serializer.SerializeJson(tr, true));
                Console.WriteLine("");
            }

            Console.WriteLine("");
        }
    }
}