namespace DocumentAtom.Documents.Image
{
    using DocumentAtom.Core;

    /// <summary>
    /// Settings for image processor.  Use of this processor requires that Tesseract be installed on the host.
    /// </summary>
    public class ImageProcessorSettings : ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// Tesseract data directory (the tessdata folder).
        /// On Windows, the folder is usually in C:\Program Files\Tesseract-OCR\tessdata or C:\Program Files(x86)\Tesseract-OCR\tessdata.
        /// On Ubuntu, the folder is usually in /usr/share/tesseract-ocr/4.00/tessdata or /usr/local/share/tessdata.
        /// On Mac, the folder is usually in /usr/local/share/tessdata or /opt/homebrew/share/tessdata.
        /// </summary>
        public string TesseractDataDirectory
        {
            get
            {
                return _TesseractDataDirectory;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TesseractDataDirectory));
                if (!Directory.Exists(value)) throw new DirectoryNotFoundException("The specified Tesseract data directory (tessdata) does not exist.");
                _TesseractDataDirectory = value;
            }
        }

        /// <summary>
        /// Tesseract trained language file, i.e. {language}.traineddata in the tessdata folder.
        /// </summary>
        public string TesseractLanguage
        {
            get
            {
                return _TesseractLanguage;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TesseractLanguage));
                _TesseractLanguage = value;
            }
        }

        /// <summary>
        /// The vertical distance threshold (in pixels) used to group text elements into lines.
        /// Default value is 5 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will make the extractor more tolerant of vertical spacing variations.
        /// Decreasing it will enforce stricter line grouping.
        /// </summary>
        public int LineThreshold
        {
            get
            {
                return _LineThreshold;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(LineThreshold));
                _LineThreshold = value;
            }
        }

        /// <summary>
        /// The vertical distance threshold (in pixels) used to identify paragraph breaks.
        /// Default value is 30 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will merge more text blocks into single paragraphs.
        /// Decreasing it will create more paragraph breaks.
        /// </summary>
        public int ParagraphThreshold
        {
            get
            {
                return _ParagraphThreshold;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(ParagraphThreshold));
                _ParagraphThreshold = value;
            }
        }

        /// <summary>
        /// The minimum length (in pixels) required to identify a horizontal line in tables.
        /// Default value is 80 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will require longer lines for table detection.
        /// Decreasing it may detect more table-like structures.
        /// </summary>
        public int HorizontalLineLength
        {
            get
            {
                return _HorizontalLineLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(HorizontalLineLength));
                _HorizontalLineLength = value;
            }
        }

        /// <summary>
        /// The minimum length (in pixels) required to identify a vertical line in tables.
        /// Default value is 40 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will require taller lines for table detection.
        /// Decreasing it may detect more table-like structures.
        /// </summary>
        public int VerticalLineLength
        {
            get
            {
                return _VerticalLineLength;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(VerticalLineLength));
                _VerticalLineLength = value;
            }
        }

        /// <summary>
        /// The minimum area (in square pixels) required for a region to be considered a table.
        /// Default value is 5000 pixels². 
        /// Minimum value is 1 pixel². 
        /// Increasing this value will only detect larger tables.
        /// Decreasing it may detect smaller table-like structures.
        /// </summary>
        public int TableMinArea
        {
            get
            {
                return _TableMinArea;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(TableMinArea));
                _TableMinArea = value;
            }
        }

        /// <summary>
        /// The horizontal pixel tolerance for aligning elements into columns.
        /// Default value is 10 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will be more lenient in column alignment.
        /// Decreasing it will enforce stricter column alignment.
        /// </summary>
        public int ColumnAlignmentTolerance
        {
            get
            {
                return _ColumnAlignmentTolerance;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(ColumnAlignmentTolerance));
                _ColumnAlignmentTolerance = value;
            }
        }

        /// <summary>
        /// The distance threshold (in pixels) used to determine if elements are related.
        /// Default value is 20 pixels. 
        /// Minimum value is 1 pixel. 
        /// Increasing this value will group more distant elements together.
        /// Decreasing it will create more separate groups.
        /// </summary>
        public int ProximityThreshold
        {
            get
            {
                return _ProximityThreshold;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(ProximityThreshold));
                _ProximityThreshold = value;
            }
        }

        /// <summary>
        /// The collection of characters used to identify list items in the text.
        /// Default includes common bullet point and list markers. 
        /// Adding markers will allow detection of additional list styles.
        /// Removing markers will limit list detection.
        /// </summary>
        public HashSet<string> ListMarkers
        {
            get
            {
                return _ListMarkers;
            }
            set
            {
                if (value == null) value = new HashSet<string>();
                _ListMarkers = value;
            }
        }

        /// <summary>
        /// Collection of regular expression patterns used to identify numbered list items.
        /// Default patterns include common formats like "1.", "a)", "(1)", "i.", etc.
        /// Adding patterns will enable detection of additional numbering styles.
        /// Removing patterns will limit detection of those styles.
        /// </summary>
        public HashSet<string> ListNumberingPatterns
        {
            get
            {
                return _ListNumberingPatterns;
            }
            set
            {
                if (value == null) value = new HashSet<string>();
                _ListNumberingPatterns = value;
            }
        }

        #endregion

        #region Private-Members

        private string _TesseractDataDirectory = @"C:\Program Files\Tesseract-OCR\tessdata";
        private string _TesseractLanguage = "eng";
        private int _LineThreshold { get; set; } = 5;
        private int _ParagraphThreshold { get; set; } = 30;
        private int _HorizontalLineLength { get; set; } = 80;
        private int _VerticalLineLength { get; set; } = 40;
        private int _TableMinArea { get; set; } = 5000;
        private int _ColumnAlignmentTolerance { get; set; } = 10;
        private int _ProximityThreshold { get; set; } = 20;
        private HashSet<string> _ListMarkers { get; set; } = new HashSet<string> { "-", "•", "*", "○", "●", "■", "□", "→", "▪", "▫", "♦", "⚫" };
        private HashSet<string> _ListNumberingPatterns { get; set; } = new HashSet<string>
        {
            @"^\d+\.",             // 1.
            @"^[a-z]\)",          // a)
            @"^[A-Z]\)",          // A)
            @"^\(\d+\)",          // (1)
            @"^[ivxlcdm]+\.",     // Roman numerals lowercase
            @"^[IVXLCDM]+\.",     // Roman numerals uppercase
            @"^[a-z]\.",          // a.
            @"^[A-Z]\."           // A.
        };

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Settings for image processor.  Use of this processor requires that Tesseract be installed on the host.
        /// </summary>
        public ImageProcessorSettings() 
        { 

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
