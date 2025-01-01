namespace DocumentAtom.Text
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for text processor.
    /// </summary>
    public class TextProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

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

        /// <summary>
        /// Settings for text processor.
        /// </summary>
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
