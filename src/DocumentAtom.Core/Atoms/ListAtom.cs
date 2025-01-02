using SerializableDataTables;
using static System.Net.Mime.MediaTypeNames;
using System.Text;

namespace DocumentAtom.Core.Atoms
{
    /// <summary>
    /// A list atom is an atom that contains a list of some type, such as a bulleted or numbered list.
    /// </summary>
    public class ListAtom : AtomBase<ListAtom>
    {
        #region Public-Members

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// A list atom is an atom that contains a list of some type, such as a bulleted or numbered list.
        /// </summary>
        public ListAtom()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Produce a human-readable string of this object.
        /// </summary>
        /// <returns>String.</returns>
        public override string ToString()
        {
            return base.ToString();
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
