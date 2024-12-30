namespace DocumentAtom.Text
{
    using DocumentAtom.Core;
    using DocumentAtom.Core.Helpers;
    using System.Text;

    /// <summary>
    /// Create atoms from text documents.
    /// </summary>
    public class TextProcessor
    {
        #region Public-Members

        /// <summary>
        /// Settings.
        /// </summary>
        public TextProcessorSettings Settings
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
        public TextProcessor()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public IEnumerable<TextAtom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            string text = File.ReadAllText(filename);
            return ProcessText(text);
        }

        #endregion

        #region Private-Methods

        private IEnumerable<TextAtom> ProcessText(string text)
        {
            if (String.IsNullOrEmpty(text)) yield break;

            int atomCount = 0;
            int startIndex = 0;
            int textLength = text.Length;

            while (startIndex < textLength)
            {
                int nextDelimiterIndex = textLength;
                string matchedDelimiter = null;

                foreach (string delimiter in _Settings.Delimiters)
                {
                    int index = text.IndexOf(delimiter, startIndex);
                    if (index != -1 && index < nextDelimiterIndex)
                    {
                        nextDelimiterIndex = index;
                        matchedDelimiter = delimiter;
                    }
                }

                string segment = text.Substring(startIndex, nextDelimiterIndex - startIndex).Trim();
                if (_Settings.RemoveBinary) segment = StringHelper.RemoveBinaryData(segment);

                if (!string.IsNullOrEmpty(segment))
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(segment);

                    TextAtom atom = new TextAtom
                    {
                        Type = AtomTypeEnum.Text,
                        Position = atomCount,
                        Length = segment.Length,
                        Text = segment,
                        MD5Hash = HashHelper.MD5Hash(bytes),
                        SHA1Hash = HashHelper.SHA1Hash(bytes),
                        SHA256Hash = HashHelper.SHA256Hash(bytes)
                    };

                    yield return atom;
                    atomCount++;
                }

                startIndex = matchedDelimiter != null ?
                    nextDelimiterIndex + matchedDelimiter.Length :
                    textLength;
            }
        }

        #endregion
    }
}
