namespace DocumentAtom.TextTools
{
    using System;

    /// <summary>
    /// String splitter.
    /// </summary>
    public class StringSplitter : IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

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

        private bool _Disposed = false;

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
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _SplitCharacters = null;
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

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
