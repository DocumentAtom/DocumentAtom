namespace DocumentAtom.TextTools
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// String splitter.
    /// </summary>
    public class StringSplitter
    {
        #region Public-Members

        /// <summary>
        /// Split characters.  Input strings will be split using these characters.
        /// </summary>
        public char[] SplitCharacters
        {
            get
            {
                return _SplitCharacters;
            }
            set
            {
                if (value == null) value = new char[0];
                _SplitCharacters = value;
            }
        }

        #endregion

        #region Private-Members

        private char[] _SplitCharacters { get; set; } = new char[]
        {
            ' ',
            ',',
            ';',
            '.',
            '/',
            '\\',
            '!',
            ':',
            '&',
            '*',
            '(',
            ')',
            '=',
            '<',
            '>',
            '{',
            '}',
            '[',
            ']'
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// String splitter.
        /// </summary>
        public StringSplitter()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Process an input string.
        /// </summary>
        /// <param name="str">String.</param>
        /// <returns>Enumerable string.</returns>
        public IEnumerable<string> Process(string str)
        {
            if (String.IsNullOrEmpty(str)) yield break;

            foreach (string token in str.Split(_SplitCharacters, StringSplitOptions.RemoveEmptyEntries))
            {
                string trimmed = token.Trim();
                if (!String.IsNullOrEmpty(trimmed)) yield return trimmed;
            }
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
