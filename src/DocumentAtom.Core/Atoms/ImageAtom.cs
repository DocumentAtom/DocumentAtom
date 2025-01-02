namespace DocumentAtom.Core.Atoms
{
    using System.Data;
    using System.Text;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;

    /// <summary>
    /// An image atom is a self-contained unit of information from a .png file.
    /// </summary>
    public class ImageAtom : AtomBase<MarkdownAtom>
    {
        #region Public-Members

        /// <summary>
        /// Bounding box.
        /// </summary>
        public BoundingBox BoundingBox { get; set; } = new BoundingBox();

        /// <summary>
        /// Text content.
        /// </summary>
        public string Text { get; set; } = null;

        /// <summary>
        /// Unordered list elements.
        /// </summary>
        public List<string> UnorderedList { get; set; } = null;

        /// <summary>
        /// Ordered list elements.
        /// </summary>
        public List<string> OrderedList { get; set; } = null;

        /// <summary>
        /// Data table.
        /// </summary>
        public SerializableDataTable Table { get; set; } = null;

        /// <summary>
        /// Binary data.
        /// </summary>
        public byte[] Binary { get; set; } = null;

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// An image atom is a self-contained unit of information from a .png file.
        /// </summary>
        public ImageAtom()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
