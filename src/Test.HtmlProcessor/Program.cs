namespace Test.HtmlProcessor
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Html;
    using SerializationHelper;

    class Program
    {
        private static string _Filename = null;
        private static HtmlProcessorSettings _Settings = new HtmlProcessorSettings();
        private static DocumentAtom.Html.HtmlProcessor _Processor = null;
        private static Serializer _Serializer = new Serializer();

        static void Main(string[] args)
        {
            try
            {
                #region Parse-Arguments

                if (args != null && args.Length > 0)
                {
                    foreach (string arg in args)
                    {
                        if (arg.StartsWith("--"))
                        {
                            string key = arg.Substring(2);
                            string val = null;

                            if (key.Contains("="))
                            {
                                string[] parts = key.Split('=');
                                key = parts[0];
                                val = parts[1];
                            }

                            switch (key.ToLower())
                            {
                                case "debug":
                                    _Settings.DebugLogging = true;
                                    break;

                                case "meta":
                                    _Settings.ProcessMetaTags = true;
                                    break;

                                case "scripts":
                                    _Settings.ProcessScripts = true;
                                    break;

                                case "comments":
                                    _Settings.ProcessComments = true;
                                    break;

                                case "data":
                                    _Settings.ExtractDataAttributes = true;
                                    break;

                                case "svg":
                                    _Settings.ProcessSvg = true;
                                    break;

                                case "maxlength":
                                    if (!String.IsNullOrEmpty(val))
                                    {
                                        _Settings.MaxTextLength = Convert.ToInt32(val);
                                    }
                                    break;

                                case "help":
                                    Usage();
                                    return;

                                default:
                                    Console.WriteLine("Unknown argument: " + key);
                                    Usage();
                                    return;
                            }
                        }
                        else
                        {
                            _Filename = arg;
                        }
                    }
                }

                #endregion

                #region Check-for-File

                if (String.IsNullOrEmpty(_Filename))
                {
                    while (true)
                    {
                        Console.Write("Filename: ");
                        _Filename = Console.ReadLine();

                        if (String.IsNullOrEmpty(_Filename))
                        {
                            continue;
                        }

                        if (!File.Exists(_Filename))
                        {
                            Console.WriteLine("File '" + _Filename + "' does not exist");
                            continue;
                        }

                        break;
                    }
                }
                else
                {
                    if (!File.Exists(_Filename))
                    {
                        Console.WriteLine("File '" + _Filename + "' does not exist");
                        return;
                    }
                }

                #endregion

                #region Process-File

                _Processor = new DocumentAtom.Html.HtmlProcessor(_Settings);

                foreach (Atom atom in _Processor.Extract(_Filename))
                {
                    Console.WriteLine(_Serializer.SerializeJson(atom, true));
                }

                #endregion

                #region Summary

                #endregion
            }
            catch (Exception e)
            {
                Console.WriteLine("");
                Console.WriteLine("Exception: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
            finally
            {
                if (_Processor != null)
                {
                    _Processor.Dispose();
                }
            }
        }

        static void Usage()
        {
            Console.WriteLine("");
            Console.WriteLine("Test.HtmlProcessor");
            Console.WriteLine("");
            Console.WriteLine("Usage:");
            Console.WriteLine("  Test.HtmlProcessor [filename] [options]");
            Console.WriteLine("");
            Console.WriteLine("Options:");
            Console.WriteLine("  --debug           Enable debug logging");
            Console.WriteLine("  --meta            Process meta tags");
            Console.WriteLine("  --scripts         Process script content");
            Console.WriteLine("  --comments        Process HTML comments");
            Console.WriteLine("  --data            Extract data attributes");
            Console.WriteLine("  --svg             Process SVG elements");
            Console.WriteLine("  --json            Output as JSON");
            Console.WriteLine("  --maxlength=N     Set maximum text length per atom");
            Console.WriteLine("  --help            Show this help message");
            Console.WriteLine("");
            Console.WriteLine("Examples:");
            Console.WriteLine("  Test.HtmlProcessor sample.html");
            Console.WriteLine("  Test.HtmlProcessor sample.html --debug --meta");
            Console.WriteLine("  Test.HtmlProcessor sample.html --json");
            Console.WriteLine("  Test.HtmlProcessor sample.html --maxlength=500");
            Console.WriteLine("");
        }
    }
}