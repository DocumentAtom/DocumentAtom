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
        private static bool _SerializeJson = true;

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

                                case "json":
                                    _SerializeJson = true;
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

                if (!_SerializeJson)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Processing file: " + _Filename);
                    Console.WriteLine("Settings:");
                    Console.WriteLine("  Debug Logging: " + _Settings.DebugLogging);
                    Console.WriteLine("  Process Meta Tags: " + _Settings.ProcessMetaTags);
                    Console.WriteLine("  Process Scripts: " + _Settings.ProcessScripts);
                    Console.WriteLine("  Process Comments: " + _Settings.ProcessComments);
                    Console.WriteLine("  Process SVG: " + _Settings.ProcessSvg);
                    Console.WriteLine("  Extract Data Attributes: " + _Settings.ExtractDataAttributes);
                    Console.WriteLine("  Max Text Length: " + (_Settings.MaxTextLength == 0 ? "Unlimited" : _Settings.MaxTextLength.ToString()));
                    Console.WriteLine("");
                }

                _Processor = new DocumentAtom.Html.HtmlProcessor(_Settings);

                int atomCount = 0;
                int textCount = 0;
                int listCount = 0;
                int tableCount = 0;
                int imageCount = 0;
                int linkCount = 0;
                int codeCount = 0;
                int metaCount = 0;

                if (!_SerializeJson)
                {
                    Console.WriteLine("Atoms:");
                    Console.WriteLine("");
                }

                foreach (Atom atom in _Processor.Extract(_Filename))
                {
                    atomCount++;

                    if (_SerializeJson)
                    {
                        Console.WriteLine(_Serializer.SerializeJson(atom, true));
                    }
                    else
                    {
                        Console.WriteLine("  [" + atom.Position + "] " + atom.Type + " (Page " + atom.PageNumber + ")");

                        switch (atom.Type)
                        {
                            case AtomTypeEnum.Text:
                                textCount++;
                                HtmlAtom htmlAtom = (HtmlAtom)atom;

                                if (!String.IsNullOrEmpty(htmlAtom.Tag))
                                {
                                    Console.Write("    Tag: " + htmlAtom.Tag);
                                }

                                if (htmlAtom.HeaderLevel.HasValue)
                                {
                                    Console.Write(" (H" + htmlAtom.HeaderLevel.Value + ")");
                                }

                                if (!String.IsNullOrEmpty(htmlAtom.Id))
                                {
                                    Console.Write(" ID: " + htmlAtom.Id);
                                }

                                if (!String.IsNullOrEmpty(htmlAtom.Class))
                                {
                                    Console.Write(" Class: " + htmlAtom.Class);
                                }

                                Console.WriteLine("");

                                if (!String.IsNullOrEmpty(htmlAtom.Text))
                                {
                                    string text = htmlAtom.Text;
                                    if (text.Length > 100)
                                    {
                                        text = text.Substring(0, 100) + "...";
                                    }
                                    Console.WriteLine("    Text: " + text);
                                }
                                break;

                            case AtomTypeEnum.List:
                                listCount++;
                                HtmlAtom listAtom = (HtmlAtom)atom;

                                if (listAtom.UnorderedList != null)
                                {
                                    Console.WriteLine("    Unordered List (" + listAtom.UnorderedList.Count + " items)");
                                    for (int i = 0; i < Math.Min(3, listAtom.UnorderedList.Count); i++)
                                    {
                                        Console.WriteLine("      - " + listAtom.UnorderedList[i]);
                                    }
                                    if (listAtom.UnorderedList.Count > 3)
                                    {
                                        Console.WriteLine("      ... and " + (listAtom.UnorderedList.Count - 3) + " more");
                                    }
                                }
                                else if (listAtom.OrderedList != null)
                                {
                                    Console.WriteLine("    Ordered List (" + listAtom.OrderedList.Count + " items)");
                                    for (int i = 0; i < Math.Min(3, listAtom.OrderedList.Count); i++)
                                    {
                                        Console.WriteLine("      " + (i + 1) + ". " + listAtom.OrderedList[i]);
                                    }
                                    if (listAtom.OrderedList.Count > 3)
                                    {
                                        Console.WriteLine("      ... and " + (listAtom.OrderedList.Count - 3) + " more");
                                    }
                                }
                                break;

                            case AtomTypeEnum.Table:
                                tableCount++;
                                HtmlAtom tableAtom = (HtmlAtom)atom;
                                Console.WriteLine("    Table: " + tableAtom.Rows + " rows x " + tableAtom.Columns + " columns");

                                if (tableAtom.Table != null && tableAtom.Table.Columns.Count > 0)
                                {
                                    Console.Write("    Columns: ");
                                    for (int i = 0; i < tableAtom.Table.Columns.Count; i++)
                                    {
                                        if (i > 0) Console.Write(", ");
                                        Console.Write(tableAtom.Table.Columns[i].Name);
                                    }
                                    Console.WriteLine("");
                                }
                                break;

                            case AtomTypeEnum.Image:
                                imageCount++;
                                ImageAtom imageAtom = (ImageAtom)atom;
                                Console.WriteLine("    Source: " + imageAtom.Src);
                                if (!String.IsNullOrEmpty(imageAtom.Alt))
                                {
                                    Console.WriteLine("    Alt: " + imageAtom.Alt);
                                }
                                if (!String.IsNullOrEmpty(imageAtom.Width) && !String.IsNullOrEmpty(imageAtom.Height))
                                {
                                    Console.WriteLine("    Dimensions: " + imageAtom.Width + " x " + imageAtom.Height);
                                }
                                break;

                            case AtomTypeEnum.Hyperlink:
                                linkCount++;
                                HyperlinkAtom linkAtom = (HyperlinkAtom)atom;
                                Console.WriteLine("    Text: " + linkAtom.Text);
                                Console.WriteLine("    Href: " + linkAtom.Href);
                                if (!String.IsNullOrEmpty(linkAtom.Target))
                                {
                                    Console.WriteLine("    Target: " + linkAtom.Target);
                                }
                                break;

                            case AtomTypeEnum.Code:
                                codeCount++;
                                CodeAtom codeAtom = (CodeAtom)atom;
                                if (!String.IsNullOrEmpty(codeAtom.Language))
                                {
                                    Console.WriteLine("    Language: " + codeAtom.Language);
                                }
                                if (codeAtom.IsInline)
                                {
                                    Console.WriteLine("    Type: Inline");
                                }
                                string code = codeAtom.Code;
                                if (code.Length > 100)
                                {
                                    code = code.Substring(0, 100) + "...";
                                }
                                Console.WriteLine("    Code: " + code);
                                break;

                            case AtomTypeEnum.Meta:
                                metaCount++;
                                MetaAtom metaAtom = (MetaAtom)atom;
                                if (!String.IsNullOrEmpty(metaAtom.Name))
                                {
                                    Console.WriteLine("    Name: " + metaAtom.Name);
                                }
                                if (!String.IsNullOrEmpty(metaAtom.Property))
                                {
                                    Console.WriteLine("    Property: " + metaAtom.Property);
                                }
                                if (!String.IsNullOrEmpty(metaAtom.Content))
                                {
                                    Console.WriteLine("    Content: " + metaAtom.Content);
                                }
                                break;
                        }

                        Console.WriteLine("    Length: " + atom.Length + " bytes");
                        Console.WriteLine("");
                    }
                }

                #endregion

                #region Summary

                if (!_SerializeJson)
                {
                    Console.WriteLine("Summary:");
                    Console.WriteLine("  Total Atoms: " + atomCount);
                    Console.WriteLine("  Text Atoms: " + textCount);
                    Console.WriteLine("  List Atoms: " + listCount);
                    Console.WriteLine("  Table Atoms: " + tableCount);
                    Console.WriteLine("  Image Atoms: " + imageCount);
                    Console.WriteLine("  Hyperlink Atoms: " + linkCount);
                    Console.WriteLine("  Code Atoms: " + codeCount);
                    Console.WriteLine("  Meta Atoms: " + metaCount);
                    Console.WriteLine("");
                }

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