namespace DocumentAtom.Text
{
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Helpers;
    using System.Text;

    /// <summary>
    /// Create atoms from text documents.
    /// </summary>
    public class TextProcessor : ProcessorBase
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// Text processor settings.
        /// </summary>
        public new TextProcessorSettings Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Settings));
                _Settings = value;
            }
        }

        #endregion

        #region Private-Members

        private TextProcessorSettings _Settings = new TextProcessorSettings();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from text documents.
        /// </summary>
        public TextProcessor(TextProcessorSettings settings = null)
        {
            if (settings == null) settings = new TextProcessorSettings();

            Header = "[Text] ";

            _Settings = settings;
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

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    StringBuilder currentSegment = new StringBuilder();
                    char[] buffer = new char[_Settings.StreamBufferSize]; 
                    int charsRead;

                    int atomCount = 0;

                    while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string chunk = new string(buffer, 0, charsRead);
                        currentSegment.Append(chunk);

                        int startIndex = 0;
                        while (startIndex < currentSegment.Length)
                        {
                            int nextDelimiterIndex = -1;
                            string matchedDelimiter = null;

                            foreach (string delimiter in Settings.Delimiters)
                            {
                                int index = currentSegment.ToString().IndexOf(delimiter, startIndex);
                                if (index != -1 && (nextDelimiterIndex == -1 || index < nextDelimiterIndex))
                                {
                                    nextDelimiterIndex = index;
                                    matchedDelimiter = delimiter;
                                }
                            }

                            if (nextDelimiterIndex != -1)
                            {
                                string segment = currentSegment.ToString(startIndex, nextDelimiterIndex - startIndex).Trim();
                                if (Settings.RemoveBinaryFromText) segment = StringHelper.RemoveBinaryData(segment);

                                if (!string.IsNullOrEmpty(segment))
                                {
                                    yield return Atom.FromTextContent(segment, atomCount, _Settings.Chunking);
                                    atomCount++;
                                }

                                startIndex = nextDelimiterIndex + matchedDelimiter.Length;
                            }
                            else
                            {
                                // No delimiter found in current buffer
                                if (startIndex > 0)
                                {
                                    // Remove processed text from StringBuilder
                                    currentSegment.Remove(0, startIndex);
                                }

                                break;
                            }
                        }
                    }

                    // Handle any remaining text
                    string finalSegment = currentSegment.ToString().Trim();
                    if (_Settings.RemoveBinaryFromText) finalSegment = StringHelper.RemoveBinaryData(finalSegment);

                    if (!String.IsNullOrEmpty(finalSegment))
                    {
                        yield return Atom.FromTextContent(finalSegment, atomCount, _Settings.Chunking);
                        atomCount++;
                    }
                }
            }
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
