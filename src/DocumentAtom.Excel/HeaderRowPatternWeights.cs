namespace DocumentAtom.Excel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Header row pattern weights.
    /// </summary>
    public class HeaderRowPatternWeights
    {
        #region Public-Members

        /// <summary>
        /// Sequential first column.
        /// </summary>
        public double SequentialFirstColumn { get; set; } = 2.0;

        /// <summary>
        /// Higher text ratio.
        /// </summary>
        public double HigherTextRatio { get; set; } = 1.0;

        /// <summary>
        /// Distinct format.
        /// </summary>
        public double DistinctFormat { get; set; } = 1.0;

        /// <summary>
        /// Consistent data columns.
        /// </summary>
        public double ConsistentDataColumns { get; set; } = 1.0;

        /// <summary>
        /// Header terms.
        /// </summary>
        public double HeaderTerms { get; set; } = 1.5;

        /// <summary>
        /// Column headers.
        /// </summary>
        public double ColumnHeaders { get; set; } = 3.0;

        /// <summary>
        /// Row one differs.
        /// </summary>
        public double Row1Differs { get; set; } = 2.0;

        /// <summary>
        /// All rows similar.
        /// </summary>
        public double AllRowsSimilar { get; set; } = -3.0;

        /// <summary>
        /// Column numbers.
        /// </summary>
        public double ColumnNumbers { get; set; } = 2.0;

        /// <summary>
        /// Numeric sequence.
        /// </summary>
        public double NumericSequence { get; set; } = -3.0;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Header row pattern weights.
        /// </summary>
        public HeaderRowPatternWeights()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
