namespace DocumentAtom.RichText
{
    /// <summary>
    /// Context information for RTF parsing.
    /// </summary>
    internal class RtfParseContext
    {
        /// <summary>
        /// Whether text is bold.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Whether text is italic.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Font size.
        /// </summary>
        public int FontSize { get; set; } = 24;

        /// <summary>
        /// Font index.
        /// </summary>
        public int FontIndex { get; set; }

        /// <summary>
        /// Style index.
        /// </summary>
        public int StyleIndex { get; set; }

        /// <summary>
        /// Outline level.
        /// </summary>
        public int OutlineLevel { get; set; } = -1;

        /// <summary>
        /// Whether currently in list text.
        /// </summary>
        public bool InListText { get; set; }

        /// <summary>
        /// List style.
        /// </summary>
        public int ListStyle { get; set; }

        /// <summary>
        /// List level.
        /// </summary>
        public int ListLevel { get; set; }

        /// <summary>
        /// Whether currently in a table.
        /// </summary>
        public bool InTable { get; set; }

        /// <summary>
        /// Whether currently in a list sequence.
        /// </summary>
        public bool InListSequence { get; set; }

        /// <summary>
        /// Create a clone of this context.
        /// </summary>
        /// <returns>Cloned context.</returns>
        public RtfParseContext Clone()
        {
            return new RtfParseContext
            {
                Bold = this.Bold,
                Italic = this.Italic,
                FontSize = this.FontSize,
                FontIndex = this.FontIndex,
                StyleIndex = this.StyleIndex,
                OutlineLevel = this.OutlineLevel,
                InListText = this.InListText,
                ListStyle = this.ListStyle,
                ListLevel = this.ListLevel,
                InTable = this.InTable,
                InListSequence = this.InListSequence
            };
        }

        /// <summary>
        /// Reset paragraph-specific formatting.
        /// </summary>
        public void ResetParagraph()
        {
            Bold = false;
            Italic = false;
            FontSize = 24;
            OutlineLevel = -1;
            InListText = false;
            ListStyle = 0;
            ListLevel = 0;
        }
    }
}