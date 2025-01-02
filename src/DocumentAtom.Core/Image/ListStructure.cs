namespace DocumentAtom.Core.Image
{
    using System;
    using System.Drawing;

    /// <summary>
    /// List structure.
    /// </summary>
    public class ListStructure
    {
        #region Public-Members

        /// <summary>
        /// List items.
        /// </summary>
        public List<string> Items { get; set; }

        /// <summary>
        /// Boolean indicating if the list is ordered.
        /// </summary>
        public bool IsOrdered { get; set; }

        /// <summary>
        /// Bounds for the list structure.
        /// </summary>
        public Rectangle Bounds { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// List structure.
        /// </summary>
        public ListStructure()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
