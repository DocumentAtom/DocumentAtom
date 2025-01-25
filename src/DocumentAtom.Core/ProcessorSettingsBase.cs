namespace DocumentAtom.Core
{
    /// <summary>
    /// Processor settings base class.
    /// </summary>
    public class ProcessorSettingsBase
    {
        #region Public-Members

        /// <summary>
        /// True to trim any text output.
        /// </summary>
        public bool TrimText { get; set; } = true;

        /// <summary>
        /// True to remove binary data from input text data.
        /// </summary>
        public bool RemoveBinaryFromText { get; set; } = true;

        /// <summary>
        /// True to extract atoms from images using OCR.
        /// </summary>
        public bool ExtractAtomsFromImages { get; set; } = true;

        /// <summary>
        /// Directory information for the temporary directory.
        /// </summary>
        public DirectoryInfo TempDirectoryInfo
        {
            get
            {
                return new DirectoryInfo(TempDirectory);
            }
        }

        /// <summary>
        /// Temporary directory.  Default is ./temp/.
        /// This directory will be created automatically.
        /// </summary>
        public string TempDirectory
        {
            get
            {
                return _TempDirectory;
            }
            set
            {
                if (String.IsNullOrEmpty(value)) throw new ArgumentNullException(nameof(TempDirectory));

                value = value.Replace("\\", "/");
                if (!value.EndsWith("/")) value += "/";
                _TempDirectory = value;

                if (!Directory.Exists(_TempDirectory)) Directory.CreateDirectory(_TempDirectory);
            }
        }

        /// <summary>
        /// Buffer size to use when reading to or writing from streams.
        /// Default is 8192 bytes.
        /// Value must be greater than zero.
        /// </summary>
        public int StreamBufferSize
        {
            get
            {
                return _StreamBufferSize;
            }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException(nameof(StreamBufferSize));
                _StreamBufferSize = value;
            }
        }

        /// <summary>
        /// Chunking settings, that is, breaking atoms into smaller, more manageable quarks.
        /// </summary>
        public ChunkingSettings Chunking
        {
            get
            {
                return _Chunking;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Chunking));
                _Chunking = value;
            }
        }

        #endregion

        #region Private-Members

        private string _TempDirectory = "./temp/";
        private int _StreamBufferSize = 8192;
        private ChunkingSettings _Chunking = new ChunkingSettings();

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Processor settings base class.
        /// </summary>
        public ProcessorSettingsBase()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
