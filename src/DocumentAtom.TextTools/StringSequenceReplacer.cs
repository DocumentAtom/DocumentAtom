namespace DocumentAtom.TextTools
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    /// <summary>
    /// String sequence replacer.
    /// </summary>
    public class StringSequenceReplacer
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8603 // Possible null reference return.

        #region Public-Members

        /// <summary>
        /// Replacer sequences.  Dictionary where the key is the match value, and the value is the value to use for in-place replacement.
        /// The default value for this dictionary includes whitespace, newline characters, braces, dashes, apostrophes and quotes, 
        /// mathematical operators, and other common non-word-forming characters.
        /// Each key-value pair is evaluated any instances of the keys found in the supplied input will be replaced with the specified values.
        /// Replacements are evaluated in the order in which they are added to the dictionary.
        /// </summary>
        public Dictionary<string, string> Replacements
        {
            get
            {
                return _Replacements;
            }
            set
            {
                if (value == null) _Replacements = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
                else _Replacements = value;
            }
        }

        #endregion

        #region Private-Members

        private Dictionary<string, string> _Replacements = new Dictionary<string, string>
        {
            { "\\", " " },
            { "/", " " },
            { "'", " " },
            { "\"", " " },
            { "'s", " " },
            { ",", " " },
            { ".", " " },
            { ":", " " },
            { ";", " " },
            { "[", " " },
            { "]", " " },
            { "(", " " },
            { ")", " " },
            { "<", " " },
            { ">", " " },
            { "*", " " },
            { "&", " " },
            { "^", " " },
            { "%", " " },
            { "$", " " },
            { "#", " " },
            { "@", " " },
            { "!", " " },
            { "`", " " },
            { "~", " " },
            { "-", " " },
            { "_", " " },
            { "=", " " },
            { "+", " " },
            { "\r", " " },
            { "\n", " " },
            { "\t", " " }
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// String sequence replacer.  This class is used to replace specific substrings with replacement values, and is often useful for
        /// removing punctuation, reducing whitespace, and removal of special characters.
        /// </summary>
        /// <param name="replacements">
        /// Replacer sequences.  Dictionary where the key is the match value, and the value is the value to use for in-place replacement.
        /// The default value for this dictionary includes whitespace, newline characters, braces, dashes, apostrophes and quotes, 
        /// mathematical operators, and other common non-word-forming characters.
        /// Each key-value pair is evaluated any instances of the keys found in the supplied input will be replaced with the specified values.
        /// </param>
        public StringSequenceReplacer(Dictionary<string, string> replacements = null)
        {
            if (replacements != null) _Replacements = replacements;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Process a string of text.
        /// </summary>
        /// <param name="str">String.</param>
        /// <param name="reduceWhitespace">True to reduce whitespace in the returned string.</param>
        /// <returns>Input string with replacements as specified in the replacements dictionary.</returns>
        public string Process(string str, bool reduceWhitespace = true)
        {
            if (String.IsNullOrEmpty(str)) return String.Empty;
            string ret = str.ToLower().Trim();

            foreach (KeyValuePair<string, string> entry in _Replacements)
            {
                string key = entry.Key.ToLower();
                while (ret.Contains(key)) ret = ret.Replace(key, entry.Value);
            }

            if (reduceWhitespace)
            {
                ret = ret
                   .Replace("\r", " ")
                   .Replace("\n", " ")
                   .Replace("\t", " ");

                while (ret.Contains("  ")) ret = ret.Replace("  ", " ");
            }

            return ret.Trim();
        }

        /// <summary>
        /// Process an array of text strings.
        /// </summary>
        /// <param name="strings">Strings.</param>
        /// <param name="removeEmptyEntries">True to remove empty entries after processing.  If set to true, the dimensions of the returned array may be different than the input array.</param>
        /// <param name="reduceWhitespace">True to reduce whitespace in the returned string.</param>
        /// <returns>Input string with replacements as specified in the replacements dictionary.</returns>
        public string[] Process(string[] strings, bool removeEmptyEntries = false, bool reduceWhitespace = true)
        {
            if (strings == null || strings.Length < 1) return strings;

            List<string> ret = new List<string>();

            foreach (string str in strings)
            {
                string val = Process(str, reduceWhitespace);
                if (!removeEmptyEntries) ret.Add(val);
                else if (!String.IsNullOrEmpty(val)) ret.Add(val);
            }

            return ret.ToArray();
        }

        #endregion

        #region Private-Methods

        #endregion

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
