namespace DocumentAtom.TextTools
{
    using System;

    /// <summary>
    /// Token extractor.
    /// </summary>
    public class TokenExtractor : IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// String sequence replacer.
        /// </summary>
        public StringSequenceReplacer StringSequenceReplacer { get; set; } = new StringSequenceReplacer();

        /// <summary>
        /// Word remover.
        /// </summary>
        public WordRemover WordRemover { get; set; } = new WordRemover();

        /// <summary>
        /// String splitter.
        /// </summary>
        public StringSplitter StringSplitter
        {
            get
            {
                return _StringSplitter;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(StringSplitter));
                _StringSplitter = value;
            }
        }

        /// <summary>
        /// Lemmatizer.
        /// </summary>
        public Lemmatizer Lemmatizer { get; set; } = new Lemmatizer();

        /// <summary>
        /// Minimum token length.
        /// </summary>
        public int MinimumTokenLength
        {
            get
            {
                return _MinimumTokenLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MinimumTokenLength));
                _MinimumTokenLength = value;
            }
        }

        /// <summary>
        /// Maximum token length.
        /// </summary>
        public int MaximumTokenLength
        {
            get
            {
                return _MaximumTokenLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(MaximumTokenLength));
                _MaximumTokenLength = value;
            }
        }

        #endregion

        #region Private-Members

        private StringSplitter _StringSplitter = new StringSplitter();
        private int _MinimumTokenLength = 3;
        private int _MaximumTokenLength = 64;

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Token extractor.
        /// </summary>
        public TokenExtractor()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _StringSplitter?.Dispose();
                    _StringSplitter = null;
                }

                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Extract tokens from an input string.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <returns>Enumerable string.</returns>
        public IEnumerable<string> Process(string str)
        {
            if (String.IsNullOrEmpty(str)) yield break;
            if (_MaximumTokenLength < _MinimumTokenLength) throw new ArgumentException("The specified maximum token length must be greater than the minimum token length.");

            string ret = str;

            if (WordRemover != null) ret = WordRemover.Process(ret);
            if (StringSequenceReplacer != null) ret = StringSequenceReplacer.Process(ret);

            foreach (string curr in _StringSplitter.Process(ret))
            {
                string val = curr;

                if (Lemmatizer != null) val = Lemmatizer.Process(curr);
                if (!String.IsNullOrEmpty(val) 
                    && val.Length >= _MinimumTokenLength 
                    && val.Length <= _MaximumTokenLength) yield return val;
            }
        }

        /// <summary>
        /// Break an input string into chunks given the specified constraints.
        /// </summary>
        /// <param name="str">Input string.</param>
        /// <param name="maxTokenCount">Maximum number of tokens per chunk.  Minimum is 1.</param>
        /// <param name="maxChunkLength">Maximum chunk length.  Minimum is 32.</param>
        /// <returns>Enumerable string, where each string contains one or more tokens.</returns>
        public IEnumerable<string> Chunk(string str, int maxTokenCount, int maxChunkLength)
        {
            if (String.IsNullOrEmpty(str)) yield break;
            if (maxTokenCount < 1) throw new ArgumentOutOfRangeException(nameof(maxTokenCount));
            if (maxChunkLength < 32) throw new ArgumentOutOfRangeException(nameof(maxChunkLength));

            string chunk = "";
            int tokenCount = 0;

            foreach (string token in Process(str))
            {
                if (chunk.Length >= maxChunkLength
                    || tokenCount >= maxTokenCount)
                {
                    yield return chunk;
                    chunk = token + " ";
                    tokenCount = 1;
                }
                else
                { 
                    chunk += token + " ";
                    tokenCount++;
                }
            }

            if (!String.IsNullOrEmpty(chunk) && tokenCount > 0)
            {
                yield return chunk;
            }
        }

        #endregion

        #region Private-Methods

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
