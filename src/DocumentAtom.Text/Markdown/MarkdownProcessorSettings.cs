namespace DocumentAtom.Text.Markdown
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for markdown processor.
    /// </summary>
    public class MarkdownProcessorSettings : ProcessorSettingsBase
    {
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

        /// <summary>
        /// Enable or disable hierarchical structure building.
        /// When true, atoms will be organized in a tree structure based on header levels.
        /// When false, atoms will be returned as a flat list.
        /// Default is true.
        /// </summary>
        public bool BuildHierarchy { get; set; } = true;

        private List<string> _Delimiters = new List<string>
        {
            "\r\n\r\n",
            "\n\n",
            "¶"
        };

        /// <summary>
        /// Settings for markdown processor.
        /// </summary>
        public MarkdownProcessorSettings() 
        { 

        }
    }
}
