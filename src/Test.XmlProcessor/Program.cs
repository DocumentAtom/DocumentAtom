namespace Test.XmlProcessor
{
    using System;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Xml;
    using GetSomeInput;
    using SerializationHelper;
    using Timestamps;

    public static class Program
    {
        private static Serializer _Serializer = new SerializationHelper.Serializer();
        private static XmlProcessorSettings _Settings = new XmlProcessorSettings();
        private static bool _ThroughputOnly = false;

        public static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("q")) _ThroughputOnly = true;

            _Settings.Chunking.Enable = true;
            _Settings.Chunking.MaximumLength = 512;
            _Settings.Chunking.ShiftSize = 384;

            while (true)
            {
                string filename = Inputty.GetString("Filename (ENTER to end):", null, true);
                if (String.IsNullOrEmpty(filename)) break;

                string buildHierarchy = Inputty.GetString("Build hierarchy? (y/n):", "y", false);
                _Settings.BuildHierarchy = buildHierarchy.Equals("y", StringComparison.OrdinalIgnoreCase);

                using (Timestamp ts = new Timestamp())
                {
                    ts.Start = DateTime.UtcNow;

                    using (XmlProcessor processor = new XmlProcessor(_Settings))
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
    }
}
