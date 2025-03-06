namespace DocumentAtom.TextTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Removes stop words and other specified words from text while preserving word boundaries.
    /// </summary>
    public class WordRemover : IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        #region Public-Members

        /// <summary>
        /// Words to remove. When any of these words are encountered as complete words, they will be removed.
        /// </summary>
        public string[] WordsToRemove
        {
            get
            {
                return _WordsToRemove;
            }
            set
            {
                _WordsToRemove = value ?? new string[0];
            }
        }

        /// <summary>
        /// Characters used to separate words during processing.
        /// </summary>
        public char[] WordSeparators
        {
            get
            {
                return _WordSeparators;
            }
            set
            {
                _WordSeparators = value ?? new char[] { ' ' };
            }
        }

        #endregion

        #region Private-Members

        private string[] _WordsToRemove = new string[]
        {
            "a", "about", "above", "after", "again", "against", "all", "am", "an", "and", "any",
            "are", "arent", "aren't", "as", "at", "be", "because", "been", "before", "being",
            "below", "between", "both", "but", "by", "cant", "can't", "cannot", "could",
            "couldnt", "couldn't", "did", "didnt", "didn't", "do", "does", "doesnt", "doesn't",
            "doing", "dont", "don't", "down", "during", "each", "every", "few", "for", "from",
            "further", "had", "hadnt", "hadn't", "has", "hasnt", "hasn't", "have", "havent",
            "haven't", "having", "he", "hed", "he'd", "he'll", "hes", "he's", "her", "here",
            "heres", "here's", "hers", "herself", "him", "himself", "his", "how", "hows",
            "how's", "i", "id", "i'd", "i'll", "im", "i'm", "ive", "i've", "if", "in", "into",
            "is", "isnt", "isn't", "it", "it's", "its", "itself", "lets", "let's", "me",
            "more", "most", "mustnt", "mustn't", "my", "myself", "no", "nor", "not", "of",
            "off", "on", "once", "only", "or", "other", "ought", "our", "ours", "ourselves",
            "out", "over", "own", "same", "shall", "shant", "shan't", "she", "she'd", "she'll",
            "shes", "she's", "should", "shouldnt", "shouldn't", "so", "some", "such", "than",
            "that", "thats", "that's", "the", "their", "theirs", "them", "themselves", "then",
            "there", "theres", "there's", "these", "they", "theyd", "they'd", "theyll",
            "they'll", "theyre", "they're", "theyve", "they've", "this", "those", "through",
            "to", "too", "under", "until", "up", "very", "was", "wasnt", "wasn't", "we",
            "we'd", "well", "we'll", "we're", "weve", "we've", "were", "werent", "weren't",
            "what", "whats", "what's", "when", "whens", "when's", "where", "wheres", "where's",
            "which", "while", "who", "whos", "who's", "whose", "whom", "why", "whys", "why's",
            "with", "wont", "won't", "would", "wouldnt", "wouldn't", "you", "youd", "you'd",
            "youll", "you'll", "youre", "you're", "youve", "you've", "your", "yours", "yourself"
        };

        private char[] _WordSeparators = new char[]
        {
            ' ', '\t', '\r', '\n', '.', ',', ';', ':', '!', '?'
        };

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Initializes a new instance of the WordRemover class.
        /// </summary>
        public WordRemover()
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
                    _WordsToRemove = null;
                    _WordSeparators = null;
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
        /// Process an array of tokens, removing specified words.
        /// </summary>
        /// <param name="tokens">Array of tokens to process.</param>
        /// <returns>Array with specified words removed.</returns>
        public string[] Process(string[] tokens)
        {
            if (tokens == null || tokens.Length == 0) return new string[0];

            return tokens
                .SelectMany(t => ProcessSingleToken(t))
                .Where(token => !string.IsNullOrWhiteSpace(token))
                .ToArray();
        }

        /// <summary>
        /// Process a single string, removing specified words.
        /// </summary>
        /// <param name="input">String to process.</param>
        /// <returns>Input string with specified words removed.</returns>
        public string Process(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return string.Empty;

            string[] words = input.Split(_WordSeparators, StringSplitOptions.RemoveEmptyEntries);
            string[] processed = Process(words);
            return string.Join(" ", processed);
        }

        #endregion

        #region Private-Methods

        private IEnumerable<string> ProcessSingleToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) yield break;

            string[] words = token.Split(_WordSeparators, StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                string trimmedWord = word.Trim().ToLower();
                if (!string.IsNullOrWhiteSpace(trimmedWord) &&
                    !_WordsToRemove.Contains(trimmedWord, StringComparer.OrdinalIgnoreCase))
                {
                    yield return word.Trim();  // Return original case
                }
            }
        }

        #endregion

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}