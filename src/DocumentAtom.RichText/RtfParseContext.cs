namespace DocumentAtom.RichText
{
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// RTF parsing context.
    /// </summary>
    internal class RtfParseContext
    {
        /// <summary>
        /// Current parsing state.
        /// </summary>
        public RtfParseState State { get; set; } = RtfParseState.Normal;

        /// <summary>
        /// Current RTF destination.
        /// </summary>
        public RtfDestination CurrentDestination { get; set; } = RtfDestination.Normal;

        /// <summary>
        /// Stack of RTF destinations.
        /// </summary>
        public Stack<RtfDestination> DestinationStack { get; set; } = new Stack<RtfDestination>();

        /// <summary>
        /// Whether text is bold.
        /// </summary>
        public bool Bold { get; set; }

        /// <summary>
        /// Whether text is italic.
        /// </summary>
        public bool Italic { get; set; }

        /// <summary>
        /// Current font size.
        /// </summary>
        public int FontSize { get; set; } = 24;

        /// <summary>
        /// Current font index.
        /// </summary>
        public int FontIndex { get; set; }

        /// <summary>
        /// Current style index.
        /// </summary>
        public int StyleIndex { get; set; }

        /// <summary>
        /// Current outline level.
        /// </summary>
        public int OutlineLevel { get; set; } = -1;

        /// <summary>
        /// Current list style.
        /// </summary>
        public int ListStyle { get; set; }

        /// <summary>
        /// Current list level.
        /// </summary>
        public int ListLevel { get; set; }

        /// <summary>
        /// Whether currently in list text.
        /// </summary>
        public bool InListText { get; set; }

        /// <summary>
        /// Whether currently in list sequence.
        /// </summary>
        public bool InListSequence { get; set; }

        /// <summary>
        /// Whether currently in table.
        /// </summary>
        public bool InTable { get; set; }

        /// <summary>
        /// Whether currently in table cell.
        /// </summary>
        public bool InTableCell { get; set; }

        /// <summary>
        /// Current group level.
        /// </summary>
        public int GroupLevel { get; set; }

        /// <summary>
        /// Current list items.
        /// </summary>
        public List<string> CurrentListItems { get; set; } = new List<string>();

        /// <summary>
        /// Current table being parsed.
        /// </summary>
        public DataTable? CurrentTable { get; set; }

        /// <summary>
        /// Current table row being parsed.
        /// </summary>
        public List<string> CurrentTableRow { get; set; } = new List<string>();

        /// <summary>
        /// Current table cell text.
        /// </summary>
        public StringBuilder CurrentCellText { get; set; } = new StringBuilder();

        /// <summary>
        /// Current text being parsed.
        /// </summary>
        public StringBuilder CurrentText { get; set; } = new StringBuilder();

        /// <summary>
        /// Whether current list is ordered.
        /// </summary>
        public bool IsOrderedList { get; set; }

        /// <summary>
        /// Pending list marker.
        /// </summary>
        public string? PendingListMarker { get; set; }

        /// <summary>
        /// Current image data being parsed.
        /// </summary>
        public StringBuilder CurrentImageData { get; set; } = new StringBuilder();

        /// <summary>
        /// Extracted images.
        /// </summary>
        public List<byte[]> ExtractedImages { get; set; } = new List<byte[]>();

        /// <summary>
        /// Check if we should ignore content in the current destination.
        /// </summary>
        /// <returns>True if content should be ignored.</returns>
        public bool ShouldIgnoreContent()
        {
            return CurrentDestination == RtfDestination.FontTable ||
                   CurrentDestination == RtfDestination.ColorTable ||
                   CurrentDestination == RtfDestination.StyleSheet ||
                   CurrentDestination == RtfDestination.Info ||
                   CurrentDestination == RtfDestination.ListTable ||
                   CurrentDestination == RtfDestination.ListOverrideTable ||
                   CurrentDestination == RtfDestination.IgnoredDestination ||
                   CurrentDestination == RtfDestination.ShapeInst ||
                   CurrentDestination == RtfDestination.ShapeProperty ||
                   CurrentDestination == RtfDestination.UnknownDestination;
        }

        /// <summary>
        /// Clone the context.
        /// </summary>
        /// <returns>Cloned context.</returns>
        public RtfParseContext Clone()
        {
            return new RtfParseContext
            {
                State = State,
                CurrentDestination = CurrentDestination,
                DestinationStack = new Stack<RtfDestination>(DestinationStack.Reverse()),
                Bold = Bold,
                Italic = Italic,
                FontSize = FontSize,
                FontIndex = FontIndex,
                StyleIndex = StyleIndex,
                OutlineLevel = OutlineLevel,
                ListStyle = ListStyle,
                ListLevel = ListLevel,
                InListText = InListText,
                InListSequence = InListSequence,
                InTable = InTable,
                InTableCell = InTableCell,
                GroupLevel = GroupLevel,
                CurrentListItems = new List<string>(CurrentListItems),
                CurrentTable = CurrentTable,
                CurrentTableRow = new List<string>(CurrentTableRow),
                CurrentCellText = new StringBuilder(CurrentCellText.ToString()),
                CurrentText = new StringBuilder(CurrentText.ToString()),
                IsOrderedList = IsOrderedList,
                PendingListMarker = PendingListMarker
            };
        }

        /// <summary>
        /// Reset paragraph-specific properties.
        /// </summary>
        public void ResetParagraph()
        {
            OutlineLevel = -1;
            InListText = false;
            // Don't automatically reset InListSequence here as it might be part of a continuing list
        }
    }
}