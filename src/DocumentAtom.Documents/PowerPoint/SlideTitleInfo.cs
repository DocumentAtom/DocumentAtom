namespace DocumentAtom.Documents.PowerPoint
{
    /// <summary>
    /// Represents title and subtitle information extracted from a slide.
    /// </summary>
    public class SlideTitleInfo
    {
        /// <summary>
        /// Gets or sets the slide title.
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the slide subtitle.
        /// </summary>
        public string Subtitle { get; set; } = string.Empty;

        /// <summary>
        /// Initializes a new instance of the SlideTitleInfo class.
        /// </summary>
        public SlideTitleInfo()
        {
        }

        /// <summary>
        /// Initializes a new instance of the SlideTitleInfo class with specified values.
        /// </summary>
        /// <param name="title">The slide title.</param>
        /// <param name="subtitle">The slide subtitle.</param>
        public SlideTitleInfo(string title, string subtitle)
        {
            Title = title;
            Subtitle = subtitle;
        }
    }
}
