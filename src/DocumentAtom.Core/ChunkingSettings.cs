namespace DocumentAtom.Core
{
    /// <summary>
    /// Chunking settings, that is, breaking atoms into quarks.
    /// </summary>
    public class ChunkingSettings
    {
        #region Public-Members

        /// <summary>
        /// Enable or disable chunking, that is, breaking atoms into quarks.
        /// </summary>
        public bool Enable { get; set; } = false;

        /// <summary>
        /// Maximum quark content length.  Minimum is 256 and maximum is 16384.  Default is 512.
        /// </summary>
        public int MaximumLength
        {
            get
            {
                return _MaximumLength;
            }
            set
            {
                if (value < 256 || value > 16384) throw new ArgumentOutOfRangeException(nameof(MaximumLength));
                _MaximumLength = value;
            }
        }

        /// <summary>
        /// Shift size, used to determine overlap amongst neighboring quarks.
        /// When set to the same value as the maximum quark content length, no overlap will exist amongst neighboring quarks.
        /// When set to a smaller amount than the maximum quark content length, overlap will exist amongst neighboring quarks.
        /// This value must be equal to or less than the maximum quark content length.
        /// </summary>
        public int ShiftSize
        {
            get
            {
                return _ShiftSize;
            }
            set
            {
                if (value > _MaximumLength) throw new ArgumentException("ShiftSize must be equal to or less than MaximumLength.");
                _ShiftSize = value;
            }
        }

        #endregion

        #region Private-Members

        private int _MaximumLength = 512;
        private int _ShiftSize = 512;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Chunking settings, that is, breaking atoms into quarks.
        /// </summary>
        public ChunkingSettings()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
