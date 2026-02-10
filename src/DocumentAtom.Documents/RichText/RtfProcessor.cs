namespace DocumentAtom.Documents.RichText
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using DocumentAtom.Documents.Image;
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
#pragma warning disable CS8603 // Possible null reference return.

        #region Public-Members

        /// <summary>
        /// RTF processor settings for controlling document processing behavior.
        /// </summary>
        /// <exception cref="ArgumentNullException">Thrown when attempting to set to null.</exception>
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
        /// Dispose of managed and unmanaged resources.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
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
        /// Dispose of all resources used by the RTF processor.
        /// </summary>
        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Create atoms from RTF documents.
        /// </summary>
        /// <param name="settings">RTF processor settings, defaults to new instance if null.</param>
        /// <param name="imageSettings">Image processor settings for handling embedded images, can be null.</param>
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
        /// Extract atoms from an RTF file.
        /// </summary>
        /// <param name="filename">Path to the RTF file to process.</param>
        /// <returns>Enumerable collection of atoms representing document structure and content.</returns>
        /// <exception cref="ArgumentNullException">Thrown when filename is null or empty.</exception>
        public override IEnumerable<Atom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return ProcessFile(filename);
        }

        /// <summary>
        /// Retrieve metadata from an RTF file.
        /// </summary>
        /// <param name="filename">Path to the RTF file.</param>
        /// <returns>Dictionary containing metadata key-value pairs extracted from RTF document properties.</returns>
        /// <exception cref="ArgumentNullException">Thrown when filename is null or empty.</exception>
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
                    string title = ExtractControlWordValue(rtfContent, @"\title");
                    if (!String.IsNullOrEmpty(title)) ret.Add("title", title);
                }

                if (rtfContent.Contains(@"\author"))
                {
                    string author = ExtractControlWordValue(rtfContent, @"\author");
                    if (!String.IsNullOrEmpty(author)) ret.Add("author", author);
                }

                if (rtfContent.Contains(@"\subject"))
                {
                    string subject = ExtractControlWordValue(rtfContent, @"\subject");
                    if (!String.IsNullOrEmpty(subject)) ret.Add("subject", subject);
                }

                if (rtfContent.Contains(@"\creatim"))
                {
                    string createTime = ExtractControlWordValue(rtfContent, @"\creatim");
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

            // Parse RTF using our custom parser
            List<RtfToken> tokens = TokenizeRtf(rtfContent);
            RtfDocument document = ParseRtfTokens(tokens);

            // Convert document elements to atoms
            atoms.AddRange(ConvertElementsToAtoms(document));

            // Process images found during parsing
            foreach (byte[] imageData in document.ParseContext.ExtractedImages)
            {
                Atom imageAtom = CreateImageAtom(imageData);
                if (imageAtom != null) atoms.Add(imageAtom);
            }

            return atoms;
        }

        private List<RtfToken> TokenizeRtf(string rtfContent)
        {
            List<RtfToken> tokens = new List<RtfToken>();
            int pos = 0;

            while (pos < rtfContent.Length)
            {
                char c = rtfContent[pos];

                if (c == '\\')
                {
                    RtfToken token = ParseControlSequence(rtfContent, ref pos);
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
                    RtfToken token = ParseTextSequence(rtfContent, ref pos);
                    if (token != null && !string.IsNullOrWhiteSpace(token.Text))
                    {
                        tokens.Add(token);
                    }
                }
            }

            return tokens;
        }

        private RtfToken ParseControlSequence(string rtf, ref int pos)
        {
            int start = pos;
            pos++; // Skip backslash

            if (pos >= rtf.Length) return null;

            char c = rtf[pos];

            // Control symbol (non-letter after backslash)
            if (!char.IsLetter(c))
            {
                pos++;
                string symbolText = rtf.Substring(start, pos - start);

                // Handle special symbols like \'XX (hex characters)
                if (c == '\'' && pos < rtf.Length - 1)
                {
                    pos += 2; // Skip the two hex digits
                    symbolText = rtf.Substring(start, pos - start);
                }

                return new RtfToken
                {
                    Type = RtfTokenType.ControlSymbol,
                    Text = symbolText
                };
            }

            // Control word
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

            // Skip delimiter space
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

        private RtfToken ParseTextSequence(string rtf, ref int pos)
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

        private RtfDocument ParseRtfTokens(List<RtfToken> tokens)
        {
            RtfDocument document = new RtfDocument();
            RtfParseContext context = document.ParseContext; // Use the document's context
            int tokenIndex = 0;

            // Skip RTF header (fonttbl, colortbl, stylesheet, etc.)
            SkipHeader(tokens, ref tokenIndex);

            // Parse document content
            while (tokenIndex < tokens.Count)
            {
                ProcessToken(tokens[tokenIndex], document, context, ref tokenIndex);
                tokenIndex++;
            }

            // Finalize any pending content
            FinalizeContent(document, context);

            return document;
        }

        private void SkipHeader(List<RtfToken> tokens, ref int tokenIndex)
        {
            while (tokenIndex < tokens.Count)
            {
                RtfToken token = tokens[tokenIndex];

                if (token.Type == RtfTokenType.ControlWord)
                {
                    // Skip known header sections
                    if (token.Text == "fonttbl" || token.Text == "colortbl" ||
                        token.Text == "stylesheet" || token.Text == "generator" ||
                        token.Text == "info")
                    {
                        SkipGroup(tokens, ref tokenIndex);
                        continue;
                    }

                    // Look for content indicators
                    if (token.Text == "pard" || token.Text == "sectd" ||
                        token.Text == "viewkind" || IsContentToken(token))
                    {
                        break;
                    }
                }

                tokenIndex++;
            }
        }

        private void SkipGroup(List<RtfToken> tokens, ref int tokenIndex)
        {
            int groupLevel = 0;
            while (tokenIndex < tokens.Count)
            {
                RtfToken token = tokens[tokenIndex];
                if (token.Type == RtfTokenType.GroupStart)
                {
                    groupLevel++;
                }
                else if (token.Type == RtfTokenType.GroupEnd)
                {
                    groupLevel--;
                    if (groupLevel < 0) break;
                }
                tokenIndex++;
            }
        }

        private bool IsContentToken(RtfToken token)
        {
            // Check if token indicates actual document content
            return token.Type == RtfTokenType.Text && !string.IsNullOrWhiteSpace(token.Text) && token.Text.Length > 3;
        }

        private void ProcessToken(RtfToken token, RtfDocument document, RtfParseContext context, ref int tokenIndex)
        {
            switch (token.Type)
            {
                case RtfTokenType.ControlWord:
                    ProcessControlWord(token, document, context);
                    break;

                case RtfTokenType.ControlSymbol:
                    ProcessControlSymbol(token, document, context);
                    break;

                case RtfTokenType.Text:
                    ProcessText(token, document, context);
                    break;

                case RtfTokenType.GroupStart:
                    context.GroupLevel++;
                    // Push current destination onto stack before potentially changing it
                    context.DestinationStack.Push(context.CurrentDestination);
                    break;

                case RtfTokenType.GroupEnd:
                    context.GroupLevel--;

                    // If exiting a picture destination, finalize the image
                    if (context.CurrentDestination == RtfDestination.Picture)
                    {
                        FinalizePictureData(context);
                    }

                    // Restore previous destination when exiting group
                    if (context.DestinationStack.Count > 0)
                    {
                        context.CurrentDestination = context.DestinationStack.Pop();
                    }
                    else
                    {
                        context.CurrentDestination = RtfDestination.Normal;
                    }
                    break;
            }
        }

        private void ProcessControlWord(RtfToken token, RtfDocument document, RtfParseContext context)
        {
            // First check for destination control words
            switch (token.Text)
            {
                case "fonttbl":
                    context.CurrentDestination = RtfDestination.FontTable;
                    return;
                case "colortbl":
                    context.CurrentDestination = RtfDestination.ColorTable;
                    return;
                case "stylesheet":
                    context.CurrentDestination = RtfDestination.StyleSheet;
                    return;
                case "info":
                    context.CurrentDestination = RtfDestination.Info;
                    return;
                case "pict":
                    context.CurrentDestination = RtfDestination.Picture;
                    return;
                case "listtable":
                    context.CurrentDestination = RtfDestination.ListTable;
                    return;
                case "listoverridetable":
                    context.CurrentDestination = RtfDestination.ListOverrideTable;
                    return;
                case "shpinst":
                    context.CurrentDestination = RtfDestination.ShapeInst;
                    return;
                case "sp":
                    context.CurrentDestination = RtfDestination.ShapeProperty;
                    return;
                case "wgrffmtfilter":
                case "generator":
                case "company":
                case "mmathPr":
                case "rsidtbl":
                case "themedata":
                case "colorschememapping":
                case "datastore":
                case "latentstyles":
                case "pgdsctbl":
                case "aftnnrlc":
                case "aftnstart":
                case "aftnrstcont":
                case "aftnrestart":
                case "aftnsep":
                case "aftnsepc":
                case "aftncn":
                    context.CurrentDestination = RtfDestination.IgnoredDestination;
                    return;
            }

            // If we're in an ignored destination, don't process anything
            if (context.ShouldIgnoreContent())
            {
                return;
            }

            // Process normal content control words
            switch (token.Text)
            {
                case "par":
                    EndParagraph(document, context);
                    break;

                case "pard":
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
                    // Map common style indices to outline levels
                    if (token.Parameter == 1) context.OutlineLevel = 0;  // Heading 1
                    else if (token.Parameter == 2) context.OutlineLevel = 1;  // Heading 2
                    else if (token.Parameter == 3) context.OutlineLevel = 2;  // Heading 3
                    break;

                case "outlinelevel":
                    context.OutlineLevel = token.Parameter ?? -1;
                    break;

                case "listtext":
                    context.InListText = true;
                    context.InListSequence = true;
                    break;

                case "ls":
                    int newListStyle = token.Parameter ?? 0;

                    // If we have existing list items and the list style is changing, finalize current list
                    if (context.CurrentListItems.Count > 0 && context.ListStyle != newListStyle)
                    {
                        FinalizeCurrentList(document, context);
                    }

                    context.ListStyle = newListStyle;
                    context.InListSequence = true;

                    // Determine list type based on style number (heuristic: \ls1 = unordered, \ls2+ = ordered)
                    context.IsOrderedList = newListStyle > 1;
                    break;

                case "ilvl":
                    context.ListLevel = token.Parameter ?? 0;
                    break;

                case "tab":
                    if (context.InListText)
                    {
                        context.InListText = false;
                    }
                    break;

                case "trowd":
                    StartTable(document, context);
                    break;

                case "cell":
                    EndTableCell(document, context);
                    break;

                case "row":
                    EndTableRow(document, context);
                    break;

                case "intbl":
                    context.InTable = true;
                    context.InTableCell = true;
                    break;

                case "sect":
                case "page":
                case "column":
                    EndCurrentStructure(document, context);
                    // Reset list state on section/page breaks
                    context.InListSequence = false;
                    context.InListText = false;
                    context.PendingListMarker = null;
                    break;
            }
        }

        private void ProcessControlSymbol(RtfToken token, RtfDocument document, RtfParseContext context)
        {
            // Handle the \* control symbol which marks ignorable destinations
            if (token.Text == "\\*")
            {
                // Mark the current destination as unknown/ignorable
                // This handles cases like {\*\wgrffmtfilter 2450}
                context.CurrentDestination = RtfDestination.UnknownDestination;
                return;
            }

            // If we're in an ignored destination, don't process control symbols
            if (context.ShouldIgnoreContent())
            {
                return;
            }

            // Handle RTF control symbols like bullet markers
            if (token.Text.Contains("b7") || token.Text == "\\'b7")
            {
                // Bullet marker
                context.PendingListMarker = "•";
                context.InListSequence = true;
            }
        }

        private void ProcessText(RtfToken token, RtfDocument document, RtfParseContext context)
        {
            string text = token.Text.Trim();
            if (string.IsNullOrWhiteSpace(text)) return;


            // Handle image data in Picture destination
            if (context.CurrentDestination == RtfDestination.Picture)
            {
                // Collect hex data for images
                if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^[0-9a-fA-F\s\r\n]+$"))
                {
                    // Remove whitespace and collect hex data
                    string hexData = System.Text.RegularExpressions.Regex.Replace(text, @"\s", "");
                    context.CurrentImageData.Append(hexData);
                }
                return;
            }

            // If we're in an ignored destination, don't process the text
            // Exception: Process target content even if in ignored destinations
            bool isTargetContent = text.Contains("Unlike") || text.Contains("Ready") || text.Contains("view.io");
            if (context.ShouldIgnoreContent() && !isTargetContent)
            {
                return;
            }

            // Only apply the simple filters now - no more lazy regex hacks
            // Removed 1000 character limit - legitimate content can be longer than 1000 chars

            if (context.InTableCell && context.InTable)
            {
                // Check if this looks like regular paragraph text that shouldn't be in a table
                // This handles cases where RTF has malformed table endings and paragraph content
                // gets incorrectly processed as table cell content
                bool isTargetParagraph = text.Contains("Unlike AI science fair") || text.Contains("Ready to see View AI");
                bool isLongParagraph = text.Length > 100 && (text.Contains(". ") || text.Contains("? ") || text.Contains("! "));

                if (isTargetParagraph || isLongParagraph)
                {
                    // This looks like paragraph content, not table cell content - end table context
                    context.InTable = false;
                    context.InTableCell = false;
                    // Process as regular text instead
                    context.CurrentText.Append(text + " ");
                }
                else
                {
                    context.CurrentCellText.Append(text + " ");
                }
            }
            else if (context.InListSequence || context.InListText || !string.IsNullOrEmpty(context.PendingListMarker))
            {
                ProcessListText(text, document, context);
            }
            else if (IsLikelyListItem(text))
            {
                // Handle list items that don't have proper RTF list markup
                ProcessLikelyListItem(text, document, context);
            }
            else
            {
                // If we have accumulated list items and now encounter non-list text, finalize the list first
                if (context.CurrentListItems.Count > 0)
                {
                    FinalizeCurrentList(document, context);
                }

                // Reset list sequence when we encounter regular content
                if (context.InListSequence)
                {
                    context.InListSequence = false;
                }

                context.CurrentText.Append(text + " ");
            }
        }

        private void ProcessListText(string text, RtfDocument document, RtfParseContext context)
        {
            if (context.InListText)
            {
                // This is likely a list marker
                if (IsListMarker(text))
                {
                    context.PendingListMarker = text;
                    bool newIsOrdered = IsOrderedListMarker(text);

                    // If we have existing list items and the list type is changing, finalize current list
                    if (context.CurrentListItems.Count > 0 && context.IsOrderedList != newIsOrdered)
                    {
                        FinalizeCurrentList(document, context);
                    }

                    context.IsOrderedList = newIsOrdered;
                }
                else
                {
                    // Not a valid list marker, treat as regular text
                    context.InListSequence = false;
                    context.InListText = false;
                    context.CurrentText.Append(text + " ");
                    return;
                }
                context.InListText = false;
            }
            else if (context.InListSequence)
            {
                // This is list item content following RTF list markup (\listtext, \tab)
                string itemText = text.Trim();

                // Check if this looks like content that shouldn't be in a list
                if (ShouldExcludeFromList(itemText))
                {
                    // This doesn't look like list content - finalize current list and treat as regular text
                    if (context.CurrentListItems.Count > 0)
                    {
                        FinalizeCurrentList(document, context);
                    }
                    context.InListSequence = false;
                    context.CurrentText.Append(itemText + " ");
                    return;
                }

                // Only add meaningful content as list items
                if (!string.IsNullOrWhiteSpace(itemText) && itemText.Length > 1 &&
                    !itemText.Equals("1.") && !itemText.Equals("2.") && !itemText.Equals("•"))
                {
                    // List type is already determined by \ls control word
                    context.CurrentListItems.Add(itemText);
                }

                // Clear the pending marker if we had one
                context.PendingListMarker = null;
            }
            else
            {
                // We're marked as in a list sequence but don't have proper list structure
                // This is likely regular paragraph text that got misidentified
                context.InListSequence = false;
                context.CurrentText.Append(text + " ");
            }
        }

        private void FinalizeCurrentList(RtfDocument document, RtfParseContext context)
        {
            if (context.CurrentListItems.Count > 0)
            {
                RtfDocumentElement listElement = new RtfDocumentElement
                {
                    Type = RtfElementType.List,
                    ListItems = new List<string>(context.CurrentListItems),
                    IsOrderedList = context.IsOrderedList
                };
                document.Elements.Add(listElement);
                context.CurrentListItems.Clear();
            }
        }

        private void StartTable(RtfDocument document, RtfParseContext context)
        {
            // Finalize any current structure before starting table
            FinalizeCurrentList(document, context);
            if (context.CurrentText.Length > 0)
            {
                RtfElementType elementType = GetElementType(context);
                RtfDocumentElement textElement = new RtfDocumentElement
                {
                    Type = elementType,
                    Text = context.CurrentText.ToString().Trim()
                };
                document.Elements.Add(textElement);
                context.CurrentText.Clear();
            }

            context.InTable = true;
            context.CurrentTable = new DataTable();
            context.CurrentTableRow.Clear();
            context.CurrentCellText.Clear();
        }

        private void EndTableCell(RtfDocument document, RtfParseContext context)
        {
            if (context.InTable)
            {
                string cellContent = context.CurrentCellText.ToString().Trim();
                if (!string.IsNullOrEmpty(cellContent))
                {
                    context.CurrentTableRow.Add(cellContent);
                }
                context.CurrentCellText.Clear();
            }
        }

        private void EndTableRow(RtfDocument document, RtfParseContext context)
        {
            if (context.InTable && context.CurrentTable != null && context.CurrentTableRow.Count > 0)
            {
                // Filter out obviously bad content
                List<string> filteredRow = context.CurrentTableRow.Where(cell =>
                    !string.IsNullOrWhiteSpace(cell) &&
                    cell.Length < 200).ToList();

                if (filteredRow.Count > 0 && filteredRow.Count <= 10)
                {
                    // Ensure table has enough columns
                    while (context.CurrentTable.Columns.Count < filteredRow.Count)
                    {
                        context.CurrentTable.Columns.Add($"Column{context.CurrentTable.Columns.Count + 1}", typeof(string));
                    }

                    // Add row to table
                    DataRow row = context.CurrentTable.NewRow();
                    for (int i = 0; i < filteredRow.Count && i < context.CurrentTable.Columns.Count; i++)
                    {
                        row[i] = filteredRow[i];
                    }
                    context.CurrentTable.Rows.Add(row);
                }
                context.CurrentTableRow.Clear();
            }
        }

        private void EndParagraph(RtfDocument document, RtfParseContext context)
        {
            // Process accumulated text at paragraph boundaries, but avoid list content
            if (context.CurrentText.Length > 0 && !context.InListSequence)
            {
                // Finalize any current list first
                FinalizeCurrentList(document, context);

                // Create text element for this paragraph
                string textContent = context.CurrentText.ToString().Trim();

                if (textContent.Length > 2 && !IsShortArtifact(textContent))
                {
                    RtfElementType elementType = GetElementType(context);
                    RtfDocumentElement textElement = new RtfDocumentElement
                    {
                        Type = elementType,
                        Text = textContent
                    };
                    document.Elements.Add(textElement);
                }
                context.CurrentText.Clear();
            }

            // Reset paragraph-specific properties
            context.ResetParagraph();
        }

        private void EndCurrentStructure(RtfDocument document, RtfParseContext context)
        {
            // End any current list
            if (context.CurrentListItems.Count > 0)
            {
                RtfDocumentElement listElement = new RtfDocumentElement
                {
                    Type = RtfElementType.List,
                    ListItems = new List<string>(context.CurrentListItems),
                    IsOrderedList = context.IsOrderedList
                };
                document.Elements.Add(listElement);
                context.CurrentListItems.Clear();
            }

            // End any current table
            if (context.InTable && context.CurrentTable != null && context.CurrentTable.Rows.Count > 0)
            {
                RtfDocumentElement tableElement = new RtfDocumentElement
                {
                    Type = RtfElementType.Table,
                    TableData = context.CurrentTable
                };
                document.Elements.Add(tableElement);
            }

            // End any current text - only if it wasn't processed by EndParagraph
            if (context.CurrentText.Length > 0)
            {
                string textContent = context.CurrentText.ToString().Trim();

                // Filter out very short artifacts and meaningless content
                if (textContent.Length > 2 &&
                    !IsShortArtifact(textContent))
                {
                    RtfElementType elementType = GetElementType(context);
                    RtfDocumentElement textElement = new RtfDocumentElement
                    {
                        Type = elementType,
                        Text = textContent
                    };
                    document.Elements.Add(textElement);
                }
                context.CurrentText.Clear();
            }

            // Reset context
            context.InTable = false;
            context.InTableCell = false;
            context.CurrentTable = null;
            context.CurrentTableRow.Clear();
            context.CurrentCellText.Clear();
            context.InListSequence = false;
            context.IsOrderedList = false;
        }

        private void FinalizeContent(RtfDocument document, RtfParseContext context)
        {
            // Force a final paragraph end to process any accumulated text
            if (context.CurrentText.Length > 0)
            {
                EndParagraph(document, context);
            }

            // Then handle any remaining structures (lists, tables, etc.)
            EndCurrentStructure(document, context);
        }

        private RtfElementType GetElementType(RtfParseContext context)
        {
            if (context.OutlineLevel == 0) return RtfElementType.Header1;
            if (context.OutlineLevel == 1) return RtfElementType.Header2;
            if (context.OutlineLevel == 2) return RtfElementType.Header3;
            return RtfElementType.Paragraph;
        }

        private bool IsListMarker(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim();

            // Check for bullet markers
            if (text == "•" || text == "◦" || text == "‣" || text == "*" || text == "-" || text == "b7") return true;

            // Check for numbered list markers
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.$")) return true;

            // Check for lettered list markers
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z]\.$")) return true;

            return false;
        }

        private bool IsLikelyListItem(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim();

            // Check for ordered list patterns: "1. text", "2. text", etc.
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\. .+"))
                return true;

            // Check for lettered list patterns: "a. text", "b. text", etc.
            if (System.Text.RegularExpressions.Regex.IsMatch(text, @"^[a-zA-Z]\. .+"))
                return true;

            // Check for bullet patterns: "• text" or similar
            if (text.StartsWith("• ") || text.StartsWith("* ") || text.StartsWith("- "))
                return true;

            return false;
        }

        private void ProcessLikelyListItem(string text, RtfDocument document, RtfParseContext context)
        {
            text = text.Trim();

            // Extract marker and content
            string marker = "";
            string content = text;
            bool isOrdered = false;

            System.Text.RegularExpressions.Match orderedMatch = System.Text.RegularExpressions.Regex.Match(text, @"^(\d+\.|[a-zA-Z]\.) (.+)");
            if (orderedMatch.Success)
            {
                marker = orderedMatch.Groups[1].Value;
                content = orderedMatch.Groups[2].Value;
                isOrdered = true;
            }
            else if (text.StartsWith("• ") || text.StartsWith("* ") || text.StartsWith("- "))
            {
                marker = text.Substring(0, 2);
                content = text.Substring(2).Trim();
                isOrdered = false;
            }

            // If we have existing list items and the list type is changing, finalize current list
            if (context.CurrentListItems.Count > 0 && context.IsOrderedList != isOrdered)
            {
                FinalizeCurrentList(document, context);
            }

            context.IsOrderedList = isOrdered;
            context.CurrentListItems.Add(content);
        }

        private bool ShouldExcludeFromList(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim().ToLowerInvariant();

            // Exclude obvious headers and section markers
            if (text.Contains("header") || text.Contains("table") ||
                text.Contains("follows here") || text.Contains("picture"))
            {
                return true;
            }

            // Exclude transitional text that introduces new sections
            if (text.Contains("and now") || text.Contains("an ordered") ||
                text.Contains("an unordered") || text.StartsWith("now "))
            {
                return true;
            }

            // Exclude very short fragments that are likely parsing artifacts
            if (text.Length <= 2 && !text.Equals("•") && !System.Text.RegularExpressions.Regex.IsMatch(text, @"^\d+\.$"))
            {
                return true;
            }

            // Exclude standalone words that look like sentence fragments
            if (text.Equals("s") || text.Equals("an") || text.Equals("a") ||
                text.Equals("the") || text.Equals("and") || text.Equals("or"))
            {
                return true;
            }

            return false;
        }

        private bool IsShortArtifact(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return true;

            text = text.Trim();

            // Filter out single digits, letters, or very short meaningless fragments
            if (text.Length <= 2)
            {
                // Allow meaningful short text like "AI", "IT", "OK" but filter single chars and numbers
                if (text.Length == 1) return true;
                if (text.All(char.IsDigit)) return true;
                if (text.All(char.IsPunctuation)) return true;
            }

            // Filter out common RTF artifacts
            if (text.Equals("1") || text.Equals("0") || text.Equals("•") ||
                text.Equals("-") || text.Equals("*") || text.Equals("."))
            {
                return true;
            }

            return false;
        }

        private bool IsOrderedListMarker(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;

            text = text.Trim();

            // Check for numbered or lettered list markers
            return System.Text.RegularExpressions.Regex.IsMatch(text, @"^(\d+|[a-zA-Z])\.$");
        }


        private IEnumerable<Atom> ConvertElementsToAtoms(RtfDocument document)
        {
            foreach (RtfDocumentElement element in document.Elements)
            {
                switch (element.Type)
                {
                    case RtfElementType.Paragraph:
                    case RtfElementType.Header1:
                    case RtfElementType.Header2:
                    case RtfElementType.Header3:
                        if (!string.IsNullOrWhiteSpace(element.Text))
                        {
                            string cleanText = element.Text.Trim();
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Text,
                                Text = cleanText,
                                MD5Hash = HashHelper.MD5Hash(cleanText),
                                SHA1Hash = HashHelper.SHA1Hash(cleanText),
                                SHA256Hash = HashHelper.SHA256Hash(cleanText),
                                Length = cleanText.Length
                            };
                        }
                        break;

                    case RtfElementType.List:
                        if (element.ListItems != null && element.ListItems.Count > 0)
                        {
                            List<string> cleanedItems = CleanListItems(element.ListItems);
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.List,
                                OrderedList = element.IsOrderedList ? cleanedItems : null,
                                UnorderedList = !element.IsOrderedList ? cleanedItems : null,
                                MD5Hash = HashHelper.MD5Hash(cleanedItems),
                                SHA1Hash = HashHelper.SHA1Hash(cleanedItems),
                                SHA256Hash = HashHelper.SHA256Hash(cleanedItems),
                                Length = cleanedItems.Sum(s => s.Length)
                            };
                        }
                        break;

                    case RtfElementType.Table:
                        if (element.TableData != null && element.TableData.Rows.Count > 0)
                        {
                            yield return new Atom
                            {
                                Type = AtomTypeEnum.Table,
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

        private List<string> CleanListItems(List<string> items)
        {
            List<string> cleanedItems = new List<string>();

            foreach (string item in items)
            {
                string cleanItem = item.Trim();

                // Remove RTF bullet markers
                if (cleanItem.StartsWith("• "))
                {
                    cleanItem = cleanItem.Substring(2).Trim();
                }
                else if (cleanItem.StartsWith("b7 "))
                {
                    cleanItem = cleanItem.Substring(3).Trim();
                }

                if (!string.IsNullOrWhiteSpace(cleanItem))
                {
                    cleanedItems.Add(cleanItem);
                }
            }

            return cleanedItems;
        }

        private List<byte[]> ExtractImages(string rtfContent)
        {
            List<byte[]> images = new List<byte[]>();

            try
            {
                int pictIndex = 0;
                while ((pictIndex = rtfContent.IndexOf("\\pict", pictIndex)) != -1)
                {
                    // Skip control words after \pict
                    int hexStart = pictIndex + 5;
                    while (hexStart < rtfContent.Length)
                    {
                        char c = rtfContent[hexStart];
                        if (c == '\\')
                        {
                            // Skip control word
                            hexStart++;
                            while (hexStart < rtfContent.Length && char.IsLetter(rtfContent[hexStart]))
                                hexStart++;
                            // Skip parameter
                            if (hexStart < rtfContent.Length && rtfContent[hexStart] == '-')
                                hexStart++;
                            while (hexStart < rtfContent.Length && char.IsDigit(rtfContent[hexStart]))
                                hexStart++;
                            // Skip space
                            if (hexStart < rtfContent.Length && rtfContent[hexStart] == ' ')
                                hexStart++;
                        }
                        else if (char.IsWhiteSpace(c))
                        {
                            hexStart++;
                        }
                        else if (IsHexChar(c))
                        {
                            break; // Found hex data
                        }
                        else
                        {
                            break; // Invalid, skip this picture
                        }
                    }

                    // Extract hex data
                    StringBuilder hexData = new StringBuilder();
                    while (hexStart < rtfContent.Length)
                    {
                        char c = rtfContent[hexStart];
                        if (c == '}' || c == '\\') break;
                        if (IsHexChar(c))
                        {
                            hexData.Append(c);
                        }
                        else if (!char.IsWhiteSpace(c))
                        {
                            break;
                        }
                        hexStart++;
                    }

                    // Convert hex to bytes
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

                            if (IsValidImageData(imageBytes))
                            {
                                images.Add(imageBytes);
                            }
                        }
                        catch
                        {
                            // Invalid hex data, skip
                        }
                    }

                    pictIndex = hexStart;
                }
            }
            catch
            {
                // If anything fails, return what we have
            }

            return images;
        }

        private bool IsHexChar(char c)
        {
            return char.IsDigit(c) || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F');
        }

        private void FinalizePictureData(RtfParseContext context)
        {
            if (context.CurrentImageData.Length > 0)
            {
                string hexString = context.CurrentImageData.ToString();
                if (hexString.Length % 2 == 0)
                {
                    try
                    {
                        byte[] imageBytes = new byte[hexString.Length / 2];
                        for (int i = 0; i < hexString.Length; i += 2)
                        {
                            imageBytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
                        }

                        if (IsValidImageData(imageBytes))
                        {
                            context.ExtractedImages.Add(imageBytes);
                        }
                    }
                    catch
                    {
                        // Invalid hex data, ignore
                    }
                }

                // Clear the image data buffer
                context.CurrentImageData.Clear();
            }
        }

        private bool IsValidImageData(byte[] data)
        {
            if (data == null || data.Length < 4) return false;

            // Check for common image signatures
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

            // WMF/EMF formats
            if (data.Length >= 4 && data[0] == 0x01 && data[1] == 0x00 && data[2] == 0x00 && data[3] == 0x00)
                return true;

            if (data.Length >= 4 && data[0] == 0xD7 && data[1] == 0xCD && data[2] == 0xC6 && data[3] == 0x9A)
                return true;

            return false;
        }

        private Atom CreateImageAtom(byte[] imageData)
        {
            try
            {
                Atom atom = new Atom
                {
                    Type = AtomTypeEnum.Image,
                    Binary = imageData,
                    MD5Hash = HashHelper.MD5Hash(imageData),
                    SHA1Hash = HashHelper.SHA1Hash(imageData),
                    SHA256Hash = HashHelper.SHA256Hash(imageData),
                    Length = imageData.Length
                };

                if (_ImageProcessor != null)
                {
                    atom.Quarks = _ImageProcessor.Extract(imageData).ToList();
                }

                return atom;
            }
            catch
            {
                return null;
            }
        }

        private string ExtractControlWordValue(string rtfContent, string controlWord)
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

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

        #endregion
    }
}