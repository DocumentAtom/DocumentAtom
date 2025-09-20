namespace DocumentAtom.RichText
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Image;
    using RtfPipe;
    using SerializableDataTables;

    /// <summary>
    /// Create atoms from Rich Text Format .rtf documents.
    /// </summary>
    public class RtfProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8604 // Possible null reference argument.

        #region Public-Members

        /// <summary>
        /// Rtf processor settings.
        /// </summary>
        public new RtfProcessorSettings Settings
        {
            get
            {
                return _ProcessorSettings;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Settings));
                _ProcessorSettings = value;
            }
        }

        #endregion

        #region Private-Members

        private RtfProcessorSettings _ProcessorSettings = new RtfProcessorSettings();
        private ImageProcessorSettings _ImageProcessorSettings = null;
        private ImageProcessor _ImageProcessor = null;

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected new void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _ImageProcessor?.Dispose();
                    _ImageProcessor = null;
                }

                base.Dispose(disposing);
                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Create atoms from text documents.
        /// </summary>
        /// <param name="settings">Processor settings.</param>
        /// <param name="imageSettings">Image processor settings.</param>
        public RtfProcessor(RtfProcessorSettings settings = null, ImageProcessorSettings imageSettings = null)
        {
            if (settings == null) settings = new RtfProcessorSettings();

            Header = "[Rtf] ";

            _ProcessorSettings = settings;
            _ImageProcessorSettings = imageSettings;

            if (_ImageProcessorSettings != null) _ImageProcessor = new ImageProcessor(_ImageProcessorSettings);
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public override IEnumerable<Atom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return ProcessFile(filename);
        }

        /// <summary>
        /// Retrieve metadata from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Dictionary.</returns>
        public Dictionary<string, string> GetMetadata(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            Dictionary<string, string> ret = new Dictionary<string, string>();

            try
            {
                string rtfContent = File.ReadAllText(filename);

                // Extract basic RTF metadata from RTF control words
                if (rtfContent.Contains(@"\title"))
                {
                    string title = ExtractControlWord(rtfContent, @"\title");
                    if (!String.IsNullOrEmpty(title)) ret.Add("title", title);
                }

                if (rtfContent.Contains(@"\author"))
                {
                    string author = ExtractControlWord(rtfContent, @"\author");
                    if (!String.IsNullOrEmpty(author)) ret.Add("author", author);
                }

                if (rtfContent.Contains(@"\subject"))
                {
                    string subject = ExtractControlWord(rtfContent, @"\subject");
                    if (!String.IsNullOrEmpty(subject)) ret.Add("subject", subject);
                }

                if (rtfContent.Contains(@"\creatim"))
                {
                    string createTime = ExtractControlWord(rtfContent, @"\creatim");
                    if (!String.IsNullOrEmpty(createTime)) ret.Add("createTime", createTime);
                }

                return ret;
            }
            catch
            {
                return ret;
            }
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));

            string rtfContent = File.ReadAllText(filename);
            List<Atom> atoms = new List<Atom>();

            // Parse RTF structure to extract individual document elements
            atoms.AddRange(ParseRtfStructure(rtfContent));

            // Post-process atoms to fix any broken list patterns
            atoms = PostProcessAtoms(atoms);

            // Extract embedded images from RTF content
            try
            {
                List<byte[]> images = ExtractImagesFromRtf(rtfContent);

                // Select best format for each image (prefer PNG over legacy formats)
                List<byte[]> uniqueImages = new List<byte[]>();
                bool hasPng = false;
                bool hasOtherFormats = false;
                byte[] pngImage = null;
                byte[] otherImage = null;

                foreach (byte[] imageBytes in images)
                {
                    if (IsValidImageData(imageBytes))
                    {
                        // Check if it's PNG (modern format)
                        if (imageBytes.Length >= 4 && imageBytes[0] == 0x89 && imageBytes[1] == 0x50 && imageBytes[2] == 0x4E && imageBytes[3] == 0x47)
                        {
                            if (!hasPng)
                            {
                                pngImage = imageBytes;
                                hasPng = true;
                            }
                        }
                        else
                        {
                            if (!hasOtherFormats)
                            {
                                otherImage = imageBytes;
                                hasOtherFormats = true;
                            }
                        }
                    }
                }

                // Prefer PNG if available, otherwise use the other format
                if (hasPng)
                {
                    uniqueImages.Add(pngImage);
                }
                else if (hasOtherFormats)
                {
                    uniqueImages.Add(otherImage);
                }

                // Create atoms for unique images
                foreach (byte[] imageBytes in uniqueImages)
                {
                    byte[] imageHashBytes = HashHelper.MD5Hash(imageBytes);
                    string imageHashString = Convert.ToBase64String(imageHashBytes);

                    Atom atom = new Atom
                    {
                        Type = AtomTypeEnum.Image,
                        Binary = imageBytes,
                        MD5Hash = imageHashBytes,
                        SHA1Hash = HashHelper.SHA1Hash(imageBytes),
                        SHA256Hash = HashHelper.SHA256Hash(imageBytes),
                        Length = imageBytes.Length
                    };

                    if (_ImageProcessor != null) atom.Quarks = _ImageProcessor.Extract(imageBytes).ToList();

                    atoms.Add(atom);
                }
            }
            catch (Exception ex)
            {
                Logger?.Invoke(SeverityEnum.Warn, $"Image extraction failed: {ex.Message}");
            }

            return atoms;
        }

        private IEnumerable<Atom> ParseRtfStructure(string rtfContent)
        {
            // Parse RTF using proper tokenization
            var tokens = TokenizeRtf(rtfContent);
            var document = ParseRtfDocument(tokens);

            // Convert document structure to atoms
            foreach (var atom in ConvertDocumentToAtoms(document))
            {
                yield return atom;
            }
        }

        private List<RtfToken> TokenizeRtf(string rtfContent)
        {
            var tokens = new List<RtfToken>();
            int pos = 0;

            while (pos < rtfContent.Length)
            {
                char c = rtfContent[pos];

                if (c == '\\')
                {
                    // Control word or control symbol
                    var token = ParseControlWord(rtfContent, ref pos);
                    if (token != null) tokens.Add(token);
                }
                else if (c == '{')
                {
                    tokens.Add(new RtfToken { Type = RtfTokenType.GroupStart, Text = "{" });
                    pos++;
                }
                else if (c == '}')
                {
                    tokens.Add(new RtfToken { Type = RtfTokenType.GroupEnd, Text = "}" });
                    pos++;
                }
                else if (char.IsWhiteSpace(c))
                {
                    pos++;
                }
                else
                {
                    // Text content
                    var token = ParseText(rtfContent, ref pos);
                    if (token != null) tokens.Add(token);
                }
            }

            return tokens;
        }

        private RtfToken ParseControlWord(string rtf, ref int pos)
        {
            int start = pos;
            pos++; // Skip the backslash

            if (pos >= rtf.Length) return null;

            // Check for control symbol (single character after \)
            char c = rtf[pos];
            if (!char.IsLetter(c))
            {
                pos++;
                return new RtfToken
                {
                    Type = RtfTokenType.ControlSymbol,
                    Text = rtf.Substring(start, pos - start)
                };
            }

            // Parse control word
            while (pos < rtf.Length && char.IsLetter(rtf[pos]))
            {
                pos++;
            }

            string controlWord = rtf.Substring(start + 1, pos - start - 1);

            // Parse optional parameter
            int parameter = 0;
            bool hasParameter = false;
            if (pos < rtf.Length && (char.IsDigit(rtf[pos]) || rtf[pos] == '-'))
            {
                hasParameter = true;
                bool negative = rtf[pos] == '-';
                if (negative) pos++;

                while (pos < rtf.Length && char.IsDigit(rtf[pos]))
                {
                    parameter = parameter * 10 + (rtf[pos] - '0');
                    pos++;
                }

                if (negative) parameter = -parameter;
            }

            // Skip optional delimiter space
            if (pos < rtf.Length && rtf[pos] == ' ')
            {
                pos++;
            }

            return new RtfToken
            {
                Type = RtfTokenType.ControlWord,
                Text = controlWord,
                Parameter = hasParameter ? parameter : null
            };
        }

        private RtfToken ParseText(string rtf, ref int pos)
        {
            int start = pos;

            while (pos < rtf.Length && rtf[pos] != '\\' && rtf[pos] != '{' && rtf[pos] != '}')
            {
                pos++;
            }

            if (pos > start)
            {
                return new RtfToken
                {
                    Type = RtfTokenType.Text,
                    Text = rtf.Substring(start, pos - start)
                };
            }

            return null;
        }

        private RtfDocument ParseRtfDocument(List<RtfToken> tokens)
        {
            var document = new RtfDocument();
            var context = new RtfParseContext();
            int pos = 0;

            // Skip RTF header and font table
            int initialPos = pos;
            SkipToContent(tokens, ref pos);

            // Parse document content - continue until we reach the end
            while (pos < tokens.Count)
            {
                ParseGroup(tokens, ref pos, document, context);
                // After a group ends, we need to advance past the group end token
                if (pos < tokens.Count && tokens[pos].Type == RtfTokenType.GroupEnd)
                {
                    pos++;
                }
            }

            // Finalize any remaining elements
            document.EndParagraph();
            document.FinalizeTables(); // Finalize any pending tables

            return document;
        }

        private void SkipToContent(List<RtfToken> tokens, ref int pos)
        {
            // Skip RTF header sections and find document content

            while (pos < tokens.Count)
            {
                var token = tokens[pos];

                // Look for \pard\plain pattern which typically indicates document content start
                if (token.Type == RtfTokenType.ControlWord && token.Text == "pard")
                {
                    // Check for \plain and document formatting
                    int lookAhead = pos + 1;
                    bool foundPlain = false;
                    bool foundContentIndicators = false;

                    // Look ahead a few tokens to see if this looks like real content
                    for (int i = 0; i < 10 && lookAhead < tokens.Count; i++, lookAhead++)
                    {
                        var nextToken = tokens[lookAhead];
                        if (nextToken.Type == RtfTokenType.ControlWord)
                        {
                            if (nextToken.Text == "plain") foundPlain = true;
                            // Look for content-related formatting
                            if (nextToken.Text == "ltrpar" || nextToken.Text == "s1" ||
                                nextToken.Text == "s2" || nextToken.Text == "ql" ||
                                nextToken.Text.StartsWith("outlinelevel"))
                            {
                                foundContentIndicators = true;
                            }
                        }
                        else if (nextToken.Type == RtfTokenType.Text &&
                                 !string.IsNullOrWhiteSpace(nextToken.Text) &&
                                 nextToken.Text.Length > 1)
                        {
                            // Found substantial text content
                            foundContentIndicators = true;
                            break;
                        }
                    }

                    if (foundPlain && foundContentIndicators)
                    {
                                    break;
                    }
                }

                pos++;
            }

        }

        private void ParseGroup(List<RtfToken> tokens, ref int pos, RtfDocument document, RtfParseContext context)
        {
            int textTokens = 0;
            int controlWords = 0;
            while (pos < tokens.Count)
            {
                var token = tokens[pos];
                switch (token.Type)
                {
                    case RtfTokenType.ControlWord:
                        controlWords++;
                        HandleControlWord(token, document, context);
                        break;
                    case RtfTokenType.ControlSymbol:
                        // Handle control symbols like \'b7 (bullet markers)
                        HandleControlSymbol(token, document, context);
                        break;
                    case RtfTokenType.Text:
                        if (!string.IsNullOrWhiteSpace(token.Text))
                        {
                            textTokens++;
                            document.AddText(token.Text, context);
                        }
                        break;
                    case RtfTokenType.GroupStart:
                        pos++;
                        var subContext = context.Clone();
                        ParseGroup(tokens, ref pos, document, subContext);
                        continue;
                    case RtfTokenType.GroupEnd:
                        // If we have text in this group, finalize any pending elements
                        if (textTokens > 0)
                        {
                            document.EndParagraph();
                        }
                        return;
                }
                pos++;
            }
        }

        private void HandleControlWord(RtfToken token, RtfDocument document, RtfParseContext context)
        {
            switch (token.Text)
            {
                case "par":
                    document.EndParagraph();
                    // Paragraph break - reset list text flag
                    context.InListText = false;
                    break;
                case "pard":
                    // Paragraph marker - don't finalize tables here (pard can occur between table rows)
                    context.ResetParagraph();
                    break;
                case "b":
                    context.Bold = token.Parameter != 0;
                    break;
                case "i":
                    context.Italic = token.Parameter != 0;
                    break;
                case "fs":
                    context.FontSize = token.Parameter ?? 24;
                    break;
                case "f":
                    context.FontIndex = token.Parameter ?? 0;
                    break;
                case "s":
                    context.StyleIndex = token.Parameter ?? 0;
                    break;
                case "outlinelevel":
                    context.OutlineLevel = token.Parameter ?? 0;
                    break;
                case "listtext":
                    context.InListText = true;
                    context.InListSequence = true;
                    break;
                case "ls":
                    context.ListStyle = token.Parameter ?? 0;
                    break;
                case "ilvl":
                    context.ListLevel = token.Parameter ?? 0;
                    break;
                case "tab":
                    // Tab in list context often separates marker from content
                    if (context.InListText)
                    {
                        context.InListText = false;
                    }
                    break;
                case "cell":
                    document.AddTableCell(context);
                    break;
                case "row":
                    document.EndTableRow();
                    break;
                case "intbl":
                    // Just mark that we're in table context, but don't start collecting yet
                    break;
                case "trowd":
                    // Table row definition - start table collection
                    context.InTable = true;
                    break;
                case "sect":
                case "page":
                case "column":
                    // Section/page/column breaks - outside table context
                    if (context.InTable)
                    {
                        context.InTable = false;
                        document.FinalizeTables();
                    }
                    break;
            }
        }

        private void HandleControlSymbol(RtfToken token, RtfDocument document, RtfParseContext context)
        {
            // Handle RTF control symbols like \'b7
            if (token.Text == "\\'b7" || token.Text.Contains("b7"))
            {
                // This is a bullet marker - convert to plain "b7" for list processing
                document.AddText("b7", context);
            }
            // Debug: Add all control symbols as text for now to see what we're missing
            else if (!string.IsNullOrWhiteSpace(token.Text))
            {
                // Don't add unknown symbols as text
            }
        }

        private IEnumerable<Atom> ConvertDocumentToAtoms(RtfDocument document)
        {
            foreach (var element in document.Elements)
            {
                switch (element.Type)
                {
                    case RtfElementType.Paragraph:
                    case RtfElementType.Header1:
                    case RtfElementType.Header2:
                    case RtfElementType.Header3:
                        if (!string.IsNullOrWhiteSpace(element.Text))
                        {
                            yield return new Atom
                            {
                                Type = DocumentAtom.Core.Enums.AtomTypeEnum.Text,
                                Text = element.Text.Trim(),
                                MD5Hash = HashHelper.MD5Hash(element.Text.Trim()),
                                SHA1Hash = HashHelper.SHA1Hash(element.Text.Trim()),
                                SHA256Hash = HashHelper.SHA256Hash(element.Text.Trim()),
                                Length = element.Text.Trim().Length
                            };
                        }
                        break;

                    case RtfElementType.List:
                        if (element.ListItems != null && element.ListItems.Count > 0)
                        {
                            // Post-process list items to combine markers with content
                            var processedItems = PostProcessListItems(element.ListItems);

                            yield return new Atom
                            {
                                Type = DocumentAtom.Core.Enums.AtomTypeEnum.List,
                                OrderedList = element.IsOrderedList ? processedItems : null,
                                UnorderedList = !element.IsOrderedList ? processedItems : null,
                                MD5Hash = HashHelper.MD5Hash(processedItems),
                                SHA1Hash = HashHelper.SHA1Hash(processedItems),
                                SHA256Hash = HashHelper.SHA256Hash(processedItems),
                                Length = processedItems.Sum(s => s.Length)
                            };
                        }
                        break;

                    case RtfElementType.Table:
                        if (element.TableData != null)
                        {
                            yield return new Atom
                            {
                                Type = DocumentAtom.Core.Enums.AtomTypeEnum.Table,
                                Table = SerializableDataTable.FromDataTable(element.TableData),
                                MD5Hash = HashHelper.MD5Hash(element.TableData),
                                SHA1Hash = HashHelper.SHA1Hash(element.TableData),
                                SHA256Hash = HashHelper.SHA256Hash(element.TableData),
                                Length = DataTableHelper.GetLength(element.TableData)
                            };
                        }
                        break;
                }
            }
        }

        private int FindContentStart(string rtfContent)
        {
            // Skip font table, then find first content
            int fontTableEnd = rtfContent.IndexOf("}");
            if (fontTableEnd == -1) return 0;

            // Look for content after the closing brace
            for (int i = fontTableEnd + 1; i < rtfContent.Length; i++)
            {
                char c = rtfContent[i];
                if (c == '\\' && i + 1 < rtfContent.Length)
                {
                    // Found a control word - this could be content
                    return i;
                }
                else if (char.IsLetter(c))
                {
                    // Found plain text content
                    return i;
                }
            }

            int result = 0;
            return result;
        }

        private List<string> PostProcessListItems(List<string> originalItems)
        {
            var processedItems = new List<string>();
            string pendingMarker = null;

            for (int i = 0; i < originalItems.Count; i++)
            {
                string item = originalItems[i].Trim();

                // Check if this item is a list marker
                if (IsListMarker(item))
                {
                    // Store the marker and wait for content
                    pendingMarker = item;
                }
                else if (!string.IsNullOrEmpty(pendingMarker))
                {
                    // Combine pending marker with this content
                    processedItems.Add($"{pendingMarker} {item}".Trim());
                    pendingMarker = null;
                }
                else
                {
                    // Regular item (no pending marker)
                    processedItems.Add(item);
                }
            }

            // If we have a pending marker at the end, add it as-is
            if (!string.IsNullOrEmpty(pendingMarker))
            {
                processedItems.Add(pendingMarker);
            }

            return processedItems;
        }

        private List<Atom> PostProcessAtoms(List<Atom> atoms)
        {
            var processedAtoms = new List<Atom>();

            for (int i = 0; i < atoms.Count; i++)
            {
                var atom = atoms[i];

                // Only process List atoms
                if (atom.Type == DocumentAtom.Core.Enums.AtomTypeEnum.List)
                {
                    // Process UnorderedList
                    if (atom.UnorderedList != null && atom.UnorderedList.Count > 0)
                    {
                        var fixedItems = FixListMarkers(atom.UnorderedList);
                        atom.UnorderedList = fixedItems;
                        atom.Length = fixedItems.Sum(s => s.Length);
                        atom.MD5Hash = HashHelper.MD5Hash(fixedItems);
                        atom.SHA1Hash = HashHelper.SHA1Hash(fixedItems);
                        atom.SHA256Hash = HashHelper.SHA256Hash(fixedItems);
                    }

                    // Process OrderedList (though this should already be working)
                    if (atom.OrderedList != null && atom.OrderedList.Count > 0)
                    {
                        var fixedItems = FixListMarkers(atom.OrderedList);
                        atom.OrderedList = fixedItems;
                        atom.Length = fixedItems.Sum(s => s.Length);
                        atom.MD5Hash = HashHelper.MD5Hash(fixedItems);
                        atom.SHA1Hash = HashHelper.SHA1Hash(fixedItems);
                        atom.SHA256Hash = HashHelper.SHA256Hash(fixedItems);
                    }
                }

                processedAtoms.Add(atom);
            }

            return processedAtoms;
        }

        private List<string> FixListMarkers(List<string> items)
        {
            var fixedItems = new List<string>();

            for (int i = 0; i < items.Count; i++)
            {
                string item = items[i].Trim();

                // Clean up RTF bullet markers (b7) from unordered list items
                if (item.StartsWith("b7 "))
                {
                    string cleanedItem = item.Substring(3).Trim(); // Remove "b7 " prefix
                    fixedItems.Add(cleanedItem);
                }
                else
                {
                    // Keep ordered list items and other items as-is
                    fixedItems.Add(item);
                }
            }

            return fixedItems;
        }

        private List<RtfDocumentElement> ParseRtfDocumentElements(string content)
        {
            List<RtfDocumentElement> elements = new List<RtfDocumentElement>();

            // Parse table data first to handle structured content
            List<RtfTableData> tables = ExtractTables(content);

            // Split by paragraph markers and analyze each section
            string[] paragraphSections = content.Split(new[] { "\\par" }, StringSplitOptions.RemoveEmptyEntries);

            // Track list context to handle sections that follow list markers
            bool previousSectionWasListMarker = false;
            bool isInListContext = false;

            for (int i = 0; i < paragraphSections.Length; i++)
            {
                string section = paragraphSections[i];

                // Skip sections that are clearly metadata or style definitions
                if (ShouldSkipSection(section)) continue;

                string cleanText = ExtractCleanTextFromSection(section);
                if (!String.IsNullOrWhiteSpace(cleanText) && IsValidContentText(cleanText))
                {
                    RtfElementType elementType = DetermineElementType(section);
                    bool isOrderedList = DetermineListType(section);

                    // Check if this section contains list markers
                    bool hasListMarkers = section.Contains("\\listtext") ||
                                        (section.Contains("\\ls1") && section.Contains("\\li720")) ||
                                        section.Contains("\\fi-360\\li720");

                    // If previous section had list markers but no text, and this section has text,
                    // then this section is likely a list item
                    if (previousSectionWasListMarker && elementType == RtfElementType.Paragraph)
                    {
                        elementType = RtfElementType.ListItem;
                        isInListContext = true;
                    }
                    // If we're in list context and this looks like list formatting
                    else if (isInListContext && (section.Contains("\\ls1") || section.Contains("\\li720")))
                    {
                        elementType = RtfElementType.ListItem;
                    }
                    // Reset list context if we encounter non-list content
                    else if (elementType != RtfElementType.ListItem)
                    {
                        isInListContext = false;
                    }

                    // Additional cleanup for specific element types
                    if (elementType == RtfElementType.ListItem)
                    {
                        // Clean up list item text
                        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"^-\s*", "");
                        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"^\s*\u2022\s*", ""); // bullet character
                        cleanText = System.Text.RegularExpressions.Regex.Replace(cleanText, @"arsid\d+", ""); // Extra arsid cleanup
                        cleanText = cleanText.Trim();
                        isInListContext = true;
                    }


                    RtfDocumentElement element = new RtfDocumentElement
                    {
                        Type = elementType,
                        Text = cleanText,
                        RawRtf = section,
                        IsOrderedList = isOrderedList
                    };

                    elements.Add(element);

                    // Track if this section had list markers but no significant text
                    previousSectionWasListMarker = hasListMarkers && cleanText.Length < 5;
                }
                else
                {
                    // Check if this is a section with just list markers
                    previousSectionWasListMarker = section.Contains("\\listtext");
                }
            }

            // Add table elements
            foreach (RtfTableData table in tables)
            {
                elements.Add(new RtfDocumentElement
                {
                    Type = RtfElementType.Table,
                    TableData = table.DataTable,
                    Text = String.Empty,
                    RawRtf = table.RawRtf
                });
            }

            return elements;
        }

        private bool ShouldSkipSection(string section)
        {
            // Skip sections that contain primarily style definitions or metadata
            string[] skipPatterns = {
                "\\stylesheet",
                "\\fonttbl",
                "\\colortbl",
                "\\info",
                "\\generator",
                "\\*\\pgdsctbl",
                "\\*\\pgdscnxt",
                "Normal;heading",
                "Table Grid",
                "Default Paragraph Font",
                "\\viewkind",
                "\\uc1",
                "\\pntxta",
                "\\pntxtb",
                "\\intbl",  // Skip table content that will be processed separately
                "\\cell",   // Skip cell content that will be processed as tables
                "\\trowd"   // Skip table row definitions
            };

            foreach (string pattern in skipPatterns)
            {
                if (section.Contains(pattern)) return true;
            }

            return false;
        }

        private bool ContainsJunkData(string text)
        {
            if (String.IsNullOrEmpty(text)) return false;

            // RTF shape and picture metadata patterns
            if (text.Contains("shapeType") || text.Contains("fFlipH") || text.Contains("fFlipV") ||
                text.Contains("pictureFormat") || text.Contains("pictureFrame") || text.Contains("pngblip") ||
                text.Contains("blipw") || text.Contains("bliph") || text.Contains("blipuid")) return true;

            // Check for long hex strings (anything with 30+ consecutive hex chars)
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"[0-9a-fA-F]{30,}")) return true;

            // Check for revision tracking numbers (7+ digits)
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d{7,}\s")) return true;

            // Check if text starts with numbers and contains mostly hex-like content
            if (text.Length > 50 && System.Text.RegularExpressions.Regex.IsMatch(text, @"^[\d\w]*\d{4,}") &&
                text.Count(c => "0123456789abcdefABCDEF".Contains(c)) > text.Length * 0.8) return true;

            // Common RTF metadata patterns
            if (text.StartsWith("*shapeType")) return true;

            return false;
        }

        private bool IsValidContentText(string text)
        {
            // Filter out obviously non-content text
            if (text.Length < 1) return false;
            if (text.Length > 1000) return false; // Very long strings are likely metadata

            // Skip text that looks like style names or encoded data
            if (text.Contains(";") && text.Split(';').Length > 5) return false;
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^[0-9a-fA-F]{20,}$")) return false;

            // Skip single characters that are likely artifacts
            if (text.Length == 1 && !char.IsLetterOrDigit(text[0])) return false;

            return true;
        }

        private string ExtractCleanTextFromSection(string section)
        {
            StringBuilder sb = new StringBuilder();
            bool insideControlWord = false;
            bool insideControlParameter = false;
            int groupLevel = 0;
            bool skipContent = false;
            int skipLevel = -1;

            for (int i = 0; i < section.Length; i++)
            {
                char c = section[i];

                if (c == '\\')
                {
                    insideControlWord = true;
                    insideControlParameter = false;
                    continue;
                }

                if (insideControlWord)
                {
                    if (char.IsLetter(c))
                    {
                        // Still in the control word name
                        continue;
                    }
                    else if (char.IsDigit(c) || c == '-')
                    {
                        // Now in the control word parameter
                        insideControlParameter = true;
                        continue;
                    }
                    else
                    {
                        // End of control word (and parameter if any)
                        insideControlWord = false;
                        insideControlParameter = false;
                        if (c == ' ') continue;
                    }
                }

                if (insideControlParameter)
                {
                    if (char.IsDigit(c))
                    {
                        // Still in parameter
                        continue;
                    }
                    else
                    {
                        // End of parameter
                        insideControlParameter = false;
                        insideControlWord = false;
                        if (c == ' ') continue;
                    }
                }

                if (c == '{')
                {
                    groupLevel++;

                    // Check if this is start of a picture section to skip
                    if (!skipContent && i + 10 < section.Length)
                    {
                        string next = section.Substring(i, Math.Min(20, section.Length - i));
                        if (next.Contains("\\pict") || next.Contains("\\shppict") || next.Contains("\\blip"))
                        {
                            skipContent = true;
                            skipLevel = groupLevel;
                        }
                    }
                    continue;
                }

                if (c == '}')
                {
                    if (skipContent && groupLevel == skipLevel)
                    {
                        skipContent = false;
                        skipLevel = -1;
                    }
                    groupLevel--;
                    continue;
                }

                // Add printable characters (but not if we're inside a control word or its parameter)
                if (!skipContent && !insideControlWord && !insideControlParameter)
                {
                    if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ')
                    {
                        sb.Append(c);
                    }
                }
            }

            // Clean up the text
            string result = sb.ToString();
            string beforeCleanup = result;

            // Remove revision tracking patterns and control word fragments
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(par|)arsid\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(ins|char)rsid\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\btblrsid\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"arsid\d+", "");

            // Remove table positioning markers and cell markers
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bx\d+\b", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bcellx\d+\b", "");

            // Remove other RTF control word fragments that might leak through
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(rtlch|ltrch|fcs|af|afs|alang|fs|cf|lang|langfe|kerning|loch|hich|dbch|cgrid|langnp|langfenp)\d*\b", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b(f|s)\d+\b", "");

            // Remove list markers and bullet characters
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\\u\d+", ""); // Unicode markers like \u0027
            result = System.Text.RegularExpressions.Regex.Replace(result, @"[\u2022\u2023\u25E6\u2043\u2219]", ""); // Various bullet characters
            result = result.Replace("'b7", ""); // RTF bullet marker
            result = result.Replace("·", ""); // Middle dot

            // Remove ordered list prefixes like "1.", "2.", etc. when they appear standalone
            result = System.Text.RegularExpressions.Regex.Replace(result, @"^\d+\.\s*", "");

            // Fix common artifacts
            result = result.Replace("dParagraph", "Paragraph");

            // Remove list markers like "-\tab" or bullet points
            result = System.Text.RegularExpressions.Regex.Replace(result, @"^-\s*", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bd-\b", "");

            // Remove excessive whitespace
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");

            return result.Trim();
        }

        private void PostProcessListItems(List<RtfDocumentElement> elements)
        {
            // First pass: merge list markers with following text
            for (int i = elements.Count - 1; i >= 0; i--)
            {
                var element = elements[i];

                // Look for list marker elements (bullets, numbers)
                if (element.Text != null)
                {
                    string text = element.Text.Trim();
                    bool isListMarker = text.Contains("'b7") ||
                                       System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.$") ||
                                       text == "d1." || text == "d2." || text == "d3.";

                    if (isListMarker && i + 1 < elements.Count)
                    {
                        var nextElement = elements[i + 1];
                        if (nextElement.Type == RtfElementType.Paragraph && !String.IsNullOrWhiteSpace(nextElement.Text))
                        {
                            // Merge marker with next text element
                            bool isOrdered = System.Text.RegularExpressions.Regex.IsMatch(text, @"\d+\.$");
                            nextElement.Type = RtfElementType.ListItem;
                            nextElement.IsOrderedList = isOrdered;

                            // Remove the marker element
                            elements.RemoveAt(i);
                        }
                    }
                }
            }

            // Second pass: identify remaining list items by content patterns
            for (int i = 0; i < elements.Count; i++)
            {
                var element = elements[i];

                if (element.Type == RtfElementType.Paragraph && !String.IsNullOrWhiteSpace(element.Text))
                {
                    string text = element.Text.Trim();

                    // Pattern: List item content (detect by bullet markers and numbered patterns)
                    if (text.StartsWith("b7 ") || // RTF bullet marker
                        System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.\s*\w") || // "1. Item"
                        (text.StartsWith("•") && text.Length > 2) ||
                        (text.StartsWith("-") && text.Length > 2) ||
                        (text.StartsWith("*") && text.Length > 2))
                    {
                        element.Type = RtfElementType.ListItem;
                        element.IsOrderedList = System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.");
                    }
                }
            }
        }

        private RtfElementType DetermineElementType(string section)
        {
            // Check for list items FIRST - prioritize over header detection
            if (section.Contains("\\listtext") || section.Contains("\\tab}") ||
                (section.Contains("\\ls1") && section.Contains("\\li720")) ||
                section.Contains("\\fi-360\\li720"))
                return RtfElementType.ListItem;

            // Check for table cells
            if (section.Contains("\\cell") || section.Contains("\\intbl"))
                return RtfElementType.TableCell;

            // Check for heading styles
            if (section.Contains("\\s1") || section.Contains("outlinelevel0"))
                return RtfElementType.Header1;
            if (section.Contains("\\s2") || section.Contains("outlinelevel1"))
                return RtfElementType.Header2;
            if (section.Contains("\\s3") || section.Contains("outlinelevel2"))
                return RtfElementType.Header3;

            // Default to paragraph
            return RtfElementType.Paragraph;
        }


        private string ExtractTextFromHtml(string html)
        {
            if (String.IsNullOrEmpty(html)) return String.Empty;

            StringBuilder sb = new StringBuilder();
            bool insideTag = false;
            bool insideScript = false;
            bool insideStyle = false;

            for (int i = 0; i < html.Length; i++)
            {
                char c = html[i];

                if (c == '<')
                {
                    insideTag = true;

                    // Check for script or style tags to skip their content
                    if (i + 7 < html.Length && html.Substring(i, 7).ToLower() == "<script")
                        insideScript = true;
                    else if (i + 6 < html.Length && html.Substring(i, 6).ToLower() == "<style")
                        insideStyle = true;
                    else if (i + 8 < html.Length && html.Substring(i, 9).ToLower() == "</script>")
                        insideScript = false;
                    else if (i + 7 < html.Length && html.Substring(i, 8).ToLower() == "</style>")
                        insideStyle = false;
                }
                else if (c == '>')
                {
                    insideTag = false;

                    // Add line breaks for block elements
                    if (i >= 3 && html.Substring(Math.Max(0, i - 3), 4).ToLower().Contains("p>"))
                        sb.AppendLine();
                    else if (i >= 3 && html.Substring(Math.Max(0, i - 3), 4).ToLower().Contains("br"))
                        sb.AppendLine();
                    else if (i >= 4 && html.Substring(Math.Max(0, i - 4), 5).ToLower().Contains("div>"))
                        sb.AppendLine();
                }
                else if (!insideTag && !insideScript && !insideStyle)
                {
                    sb.Append(c);
                }
            }

            // Clean up HTML entities
            string text = sb.ToString();
            text = text.Replace("&amp;", "&");
            text = text.Replace("&lt;", "<");
            text = text.Replace("&gt;", ">");
            text = text.Replace("&quot;", "\"");
            text = text.Replace("&apos;", "'");
            text = text.Replace("&nbsp;", " ");
            text = text.Replace("&ndash;", "–");
            text = text.Replace("&mdash;", "—");
            text = text.Replace("&hellip;", "…");

            // Clean up whitespace
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\r\n|\r|\n", "\n");
            text = System.Text.RegularExpressions.Regex.Replace(text, @"\n\s*\n+", "\n\n");

            return text.Trim();
        }

        private string ExtractTextFromRtf(string rtfContent)
        {
            if (String.IsNullOrEmpty(rtfContent)) return String.Empty;

            // Find document content after header sections
            string[] contentMarkers = {
                "\\pard", // paragraph definition - usually indicates start of content
                "\\viewkind", // view kind - often precedes content
                "\\uc1", // unicode character handling
                "\\lang" // language setting in content
            };

            int contentStart = -1;
            foreach (string marker in contentMarkers)
            {
                int pos = rtfContent.IndexOf(marker);
                if (pos > 1000) // Past header sections
                {
                    contentStart = pos;
                    break;
                }
            }

            if (contentStart == -1)
            {
                // Fallback: skip first 30% (usually headers)
                contentStart = Math.Min(rtfContent.Length / 3, 10000);
            }

            // Extract text from the content portion only
            string contentPortion = rtfContent.Substring(contentStart);

            StringBuilder sb = new StringBuilder();
            bool insideControlWord = false;
            int groupLevel = 0;
            int skipLevel = -1;
            bool skipContent = false;
            bool foundTextContent = false;

            for (int i = 0; i < contentPortion.Length; i++)
            {
                char c = contentPortion[i];

                if (c == '\\')
                {
                    // Start of control word
                    insideControlWord = true;

                    // Check for paragraph break
                    if (i + 4 < contentPortion.Length && contentPortion.Substring(i, 4) == "\\par")
                    {
                        if (!skipContent && foundTextContent)
                            sb.AppendLine();
                        i += 3; // Skip "par", the loop will increment i
                        insideControlWord = false;
                        continue;
                    }

                    continue;
                }

                if (insideControlWord)
                {
                    // Skip characters until we hit space, brace, or non-alphanumeric
                    if (c == ' ' || c == '{' || c == '}' || (!char.IsLetterOrDigit(c) && c != '-'))
                    {
                        insideControlWord = false;
                        if (c == ' ') continue; // Skip the space after control word
                        // Let other characters be processed normally
                    }
                    else
                    {
                        continue; // Skip control word characters
                    }
                }

                if (c == '{')
                {
                    groupLevel++;

                    // Check if this is a group we should skip entirely
                    if (i + 1 < contentPortion.Length && contentPortion[i + 1] == '\\')
                    {
                        // Extract the control word following the opening brace
                        string controlWord = ExtractControlWordAt(contentPortion, i + 2);

                        // Skip known non-content groups (even in content section)
                        if (controlWord == "pict" || controlWord == "objdata" ||
                            controlWord == "bkmkstart" || controlWord == "bkmkend" ||
                            controlWord == "listtext" || controlWord.StartsWith("*") ||
                            controlWord == "object" || controlWord == "nonshppict" ||
                            controlWord == "shppict" || controlWord == "bin" ||
                            controlWord == "data" || controlWord == "theme" ||
                            controlWord == "colorschememapping" || controlWord == "latentstyles")
                        {
                            skipContent = true;
                            skipLevel = groupLevel;
                        }
                    }
                    continue;
                }

                if (c == '}')
                {
                    if (skipContent && groupLevel == skipLevel)
                    {
                        skipContent = false;
                        skipLevel = -1;
                    }
                    groupLevel--;
                    continue;
                }

                // Add character if we're in the document content and not skipping
                if (groupLevel >= 0 && !skipContent && !insideControlWord)
                {
                    // Only include printable characters
                    if (char.IsControl(c))
                    {
                        if (c == '\r' || c == '\n' || c == '\t')
                        {
                            if (foundTextContent) sb.Append(' '); // Convert to space
                        }
                        // Skip other control characters
                    }
                    else if (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' ')
                    {
                        sb.Append(c);
                        foundTextContent = true;
                    }
                }
            }

            // Clean up the extracted text
            string result = sb.ToString();

            // Remove hex data patterns (sequences of hex digits)
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b[0-9a-fA-F]{32,}\b", " ");

            // Remove strings that look like encoded data
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b[A-Za-z0-9+/]{20,}={0,2}\b", " ");

            // Remove standalone single characters that are likely artifacts
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b[a-zA-Z]\s+(?=[a-zA-Z]\s+)", "");

            // Remove "d arsid" patterns which are RTF revision tracking
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bd\s+arsid\d+\s+", " ");

            // Remove remaining single 'd' characters which are revision artifacts
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\bd\s+", " ");

            // Remove long lists of style names and table styles (common RTF metadata)
            string[] stylePatterns = {
                @"Normal;heading \d+;.*?Smart Link;",
                @"\*\s*Normal;.*?Smart Link;",
                @"heading \d+;.*?List Table \d+.*?;",
                @"Table.*?Accent \d+;.*?Table.*?;"
            };

            foreach (string pattern in stylePatterns)
            {
                result = System.Text.RegularExpressions.Regex.Replace(result, pattern, " ", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            }

            // Remove standalone asterisks and short hex patterns
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\*\s+", " ");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\b[0-9a-fA-F]{8,}\b", " ");

            // Remove sequences of repeated characters (likely artifacts)
            result = System.Text.RegularExpressions.Regex.Replace(result, @"(\*\s*){3,}", " ");

            // Remove excessive whitespace
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");
            result = result.Trim();

            return result;
        }

        private string ExtractControlWordAt(string rtfContent, int startIndex)
        {
            if (startIndex >= rtfContent.Length) return "";

            StringBuilder word = new StringBuilder();
            int i = startIndex;

            while (i < rtfContent.Length && char.IsLetter(rtfContent[i]))
            {
                word.Append(rtfContent[i]);
                i++;
            }

            return word.ToString();
        }

        private bool ShouldSkipGroup(string controlWord)
        {
            // Skip groups that don't contain document text
            return controlWord switch
            {
                "fonttbl" => true,     // Font table
                "colortbl" => true,    // Color table
                "stylesheet" => true,  // Stylesheet
                "info" => true,        // Document info
                "pict" => false,       // Pictures - we handle these separately
                "object" => true,      // Embedded objects
                "field" => true,       // Field definitions
                _ => false
            };
        }

        private string ExtractControlWord(string rtfContent, string controlWord)
        {
            int startIndex = rtfContent.IndexOf(controlWord);
            if (startIndex == -1) return String.Empty;

            startIndex += controlWord.Length;
            if (startIndex >= rtfContent.Length) return String.Empty;

            // Skip whitespace
            while (startIndex < rtfContent.Length && Char.IsWhiteSpace(rtfContent[startIndex]))
            {
                startIndex++;
            }

            StringBuilder sb = new StringBuilder();
            int groupLevel = 0;

            for (int i = startIndex; i < rtfContent.Length; i++)
            {
                char c = rtfContent[i];

                if (c == '{') groupLevel++;
                if (c == '}')
                {
                    groupLevel--;
                    if (groupLevel < 0) break;
                }
                if (c == '\\' && groupLevel == 0) break;

                if (groupLevel == 0 && c != '{' && c != '}')
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Trim();
        }

        private List<byte[]> ExtractImagesFromRtf(string rtfContent)
        {
            List<byte[]> images = new List<byte[]>();

            if (String.IsNullOrEmpty(rtfContent)) return images;

            try
            {
                // Look for \pict control word which indicates the start of picture data
                // Also check for \shppict which can contain \pict sections
                List<string> pictPatterns = new List<string> { @"\pict", @"\shppict" };


                foreach (string pattern in pictPatterns)
                {
                    int pictIndex = 0;
                    while ((pictIndex = rtfContent.IndexOf(pattern, pictIndex)) != -1)
                    {
                        // For \shppict, look for nested \pict
                        if (pattern == @"\shppict")
                        {
                            int nestedPictIndex = rtfContent.IndexOf(@"\pict", pictIndex);
                            if (nestedPictIndex != -1 && nestedPictIndex < pictIndex + 1000) // Reasonable proximity
                            {
                                pictIndex = nestedPictIndex;
                            }
                            else
                            {
                                pictIndex += pattern.Length;
                                continue;
                            }
                        }

                        // Find the start of the hex data (after the \pict and any parameters)
                        int hexStart = pictIndex + 5; // Start after "\pict"

                    // Skip any control words and parameters that might follow \pict
                    while (hexStart < rtfContent.Length)
                    {
                        char c = rtfContent[hexStart];
                        if (c == '\\')
                        {
                            // Skip control word - look for word ending
                            hexStart++;
                            while (hexStart < rtfContent.Length && Char.IsLetter(rtfContent[hexStart]))
                            {
                                hexStart++;
                            }
                            // Skip any numeric parameter
                            if (hexStart < rtfContent.Length && rtfContent[hexStart] == '-')
                                hexStart++; // negative parameter
                            while (hexStart < rtfContent.Length && Char.IsDigit(rtfContent[hexStart]))
                            {
                                hexStart++;
                            }
                            // Skip any delimiter space
                            if (hexStart < rtfContent.Length && rtfContent[hexStart] == ' ')
                                hexStart++;
                        }
                        else if (c == '{')
                        {
                            // Skip group - find matching closing brace
                            int braceLevel = 1;
                            hexStart++;
                            while (hexStart < rtfContent.Length && braceLevel > 0)
                            {
                                if (rtfContent[hexStart] == '{') braceLevel++;
                                else if (rtfContent[hexStart] == '}') braceLevel--;
                                hexStart++;
                            }
                        }
                        else if (Char.IsWhiteSpace(c))
                        {
                            hexStart++;
                        }
                        else if (Char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F'))
                        {
                            // Found the start of hex data
                            break;
                        }
                        else
                        {
                            // Invalid character, skip this picture
                            break;
                        }
                    }

                    if (hexStart >= rtfContent.Length) break;

                    // Extract hex data until we hit a closing brace or non-hex character
                    StringBuilder hexData = new StringBuilder();
                    int pos = hexStart;

                    while (pos < rtfContent.Length)
                    {
                        char c = rtfContent[pos];
                        if (c == '}' || c == '\\')
                        {
                            break;
                        }
                        if (!Char.IsDigit(c) && (c < 'a' || c > 'f') && (c < 'A' || c > 'F') && !Char.IsWhiteSpace(c))
                        {
                            break;
                        }
                        if (!Char.IsWhiteSpace(c))
                        {
                            hexData.Append(c);
                        }
                        pos++;
                    }

                    // Convert hex string to byte array
                    string hexString = hexData.ToString();

                    if (hexString.Length > 0 && hexString.Length % 2 == 0)
                    {
                        try
                        {
                            byte[] imageBytes = new byte[hexString.Length / 2];
                            for (int i = 0; i < hexString.Length; i += 2)
                            {
                                imageBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                            }

                            // Validate that this looks like image data (basic check for common image headers)
                            if (IsValidImageData(imageBytes))
                            {
                                images.Add(imageBytes);
                            }
                        }
                        catch
                        {
                            // Invalid hex data, skip this image
                        }
                    }

                        pictIndex = pos;
                    }
                }
            }
            catch
            {
                // If anything goes wrong, return whatever images we've found so far
            }

            return images;
        }

        private bool IsValidImageData(byte[] data)
        {
            if (data == null || data.Length < 4) return false;

            // Check for common image file signatures
            // PNG: 89 50 4E 47
            if (data.Length >= 4 && data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
                return true;

            // JPEG: FF D8 FF
            if (data.Length >= 3 && data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                return true;

            // GIF: 47 49 46 38
            if (data.Length >= 4 && data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x38)
                return true;

            // BMP: 42 4D
            if (data.Length >= 2 && data[0] == 0x42 && data[1] == 0x4D)
                return true;

            // Windows Enhanced Metafile: 01 00 00 00 or similar WMF/EMF signatures
            if (data.Length >= 4 && data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)
                return true;

            // Windows Metafile: D7 CD C6 9A
            if (data.Length >= 4 && data[0] == 0xD7 && data[1] == 0xCD && data[2] == 0xC6 && data[3] == 0x9A)
                return true;

            // TIFF: 49 49 2A 00 (little endian) or 4D 4D 00 2A (big endian)
            if (data.Length >= 4 &&
                ((data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A && data[3] == 0x00) ||
                 (data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00 && data[3] == 0x2A)))
                return true;

            // Be more lenient - if we have a reasonable amount of data that could be an image, allow it
            if (data.Length >= 50)
            {
                // Check if it looks like binary data (not mostly text)
                int nonPrintableCount = 0;
                int sampleSize = Math.Min(50, data.Length);
                for (int i = 0; i < sampleSize; i++)
                {
                    if (data[i] < 32 || data[i] > 126)
                        nonPrintableCount++;
                }
                // If more than 30% is non-printable, it's likely binary data
                if (nonPrintableCount > sampleSize * 0.3)
                    return true;
            }

            return false;
        }

        private bool DetermineListType(string section)
        {
            // Check for ordered list markers (numbers, letters)
            // RTF ordered lists often have numbering patterns
            if (section.Contains("\\pnlvlbody") || section.Contains("\\pnf") || section.Contains("\\pnstart"))
                return true;

            // Check for bullet markers which indicate unordered lists
            if (section.Contains("\\pnlvlblt") || section.Contains("-\\tab") || section.Contains("\\listtext"))
                return false;

            // Default to unordered for safety
            return false;
        }

        private List<RtfTableData> ExtractTables(string content)
        {
            List<RtfTableData> tables = new List<RtfTableData>();

            // Find table structures in RTF content
            // Tables in RTF are marked with \trowd (table row definition) and \cell markers
            int searchStart = 0;
            while (true)
            {
                int tableStart = content.IndexOf("\\trowd", searchStart);
                if (tableStart == -1) break;

                // Find the end of this table - look for next table or end of content
                int tableEnd = content.IndexOf("\\trowd", tableStart + 1);
                if (tableEnd == -1)
                {
                    // Look for end of table markers
                    int endMarker = content.IndexOf("\\pard", tableStart);
                    tableEnd = endMarker != -1 ? endMarker : content.Length;
                }

                string tableSection = content.Substring(tableStart, tableEnd - tableStart);

                // Extract table data
                DataTable dt = ParseTableFromRtf(tableSection);
                if (dt.Rows.Count > 0)
                {
                    tables.Add(new RtfTableData
                    {
                        DataTable = dt,
                        RawRtf = tableSection
                    });
                }

                searchStart = tableEnd;
            }

            return tables;
        }

        private DataTable ParseTableFromRtf(string tableSection)
        {
            DataTable dt = new DataTable();

            // Find cell content more precisely
            List<List<string>> rows = new List<List<string>>();

            // Split by row markers first
            string[] rowSections = tableSection.Split(new[] { "\\row" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (string rowSection in rowSections)
            {
                if (!rowSection.Contains("\\cell")) continue;

                List<string> rowCells = new List<string>();

                // Extract cells from this row
                string[] cellParts = rowSection.Split(new[] { "\\cell" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (string cellPart in cellParts)
                {
                    // Look for actual text content, not just control sequences
                    string cellText = ExtractCellText(cellPart);
                    if (!String.IsNullOrWhiteSpace(cellText))
                    {
                        rowCells.Add(cellText.Trim());
                    }
                }

                if (rowCells.Count > 0)
                {
                    rows.Add(rowCells);
                }
            }

            if (rows.Count == 0) return dt;

            // Determine number of columns
            int maxColumns = rows.Max(r => r.Count);

            // Create columns
            for (int i = 0; i < maxColumns; i++)
            {
                dt.Columns.Add($"Column{i + 1}");
            }

            // Add rows to DataTable
            foreach (List<string> row in rows)
            {
                DataRow dr = dt.NewRow();
                for (int i = 0; i < Math.Min(row.Count, maxColumns); i++)
                {
                    dr[i] = row[i];
                }
                dt.Rows.Add(dr);
            }

            return dt;
        }

        private string ExtractCellText(string cellContent)
        {
            // Remove RTF control words but keep text content
            StringBuilder sb = new StringBuilder();
            bool insideControlWord = false;
            int braceLevel = 0;

            for (int i = 0; i < cellContent.Length; i++)
            {
                char c = cellContent[i];

                if (c == '\\')
                {
                    insideControlWord = true;
                    continue;
                }

                if (insideControlWord)
                {
                    if (c == ' ' || c == '{' || c == '}' || (!char.IsLetterOrDigit(c) && c != '-'))
                    {
                        insideControlWord = false;
                        if (c == ' ') continue;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (c == '{')
                {
                    braceLevel++;
                    continue;
                }

                if (c == '}')
                {
                    braceLevel--;
                    continue;
                }

                // Add text content
                if (!insideControlWord && (char.IsLetterOrDigit(c) || char.IsPunctuation(c) || char.IsSymbol(c) || c == ' '))
                {
                    sb.Append(c);
                }
            }

            string result = sb.ToString();

            // Clean up cell-specific artifacts - more aggressive
            result = System.Text.RegularExpressions.Regex.Replace(result, @"x\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"arsid\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"insrsid\d+", "");
            result = System.Text.RegularExpressions.Regex.Replace(result, @"\s+", " ");

            return result.Trim();
        }

        private bool IsListMarker(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            // Check for bullet markers (both unicode and RTF encoded)
            if (text == "b7" || text == "•" || text == "◦" || text == "‣") return true;

            // Check for numbered list markers
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.$")) return true;

            // Check for lettered list markers
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z]\.$")) return true;

            return false;
        }

#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        #endregion
    }
}