namespace DocumentAtom.Text
{
    /// <summary>
    /// Settings for text processor.
    /// </summary>
    public class TextProcessorSettings
    {
        #region Public-Members

        /// <summary>
        /// True to trim the output.
        /// </summary>
        public bool Trim { get; set; } = true;

        /// <summary>
        /// True to remove binary data from input data.
        /// </summary>
        public bool RemoveBinary { get; set; } = true;

        /// <summary>
        /// Delimiters that indicate the beginning of a new atom.
        /// </summary>
        public List<string> Delimiters
        {
            get
            {
                return _Delimiters;
            }
            set
            {
                if (value == null || value.Count < 1) throw new ArgumentNullException(nameof(Delimiters));
                foreach (string val in value)
                {
                    if (val == null || val.Length < 1)
                    {
                        throw new ArgumentException("The supplied delimiters must have one or more characters.");
                    }
                }
                _Delimiters = value;
            }
        }

        #endregion

        #region Private-Members

        private List<string> _Delimiters = new List<string>
        {
            "\r\n\r\n",
            "\n\n",
            "¶"
        };

        #endregion

        #region Constructors-and-Factories

        public TextProcessorSettings() 
        { 

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
