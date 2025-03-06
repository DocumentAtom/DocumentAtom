
namespace DocumentAtom.Core.Office
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    /// <summary>
    /// Office document relationships.
    /// </summary>
    [XmlRoot("Relationships", Namespace = "http://schemas.openxmlformats.org/package/2006/relationships")]
    public class Relationships : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Items.
        /// </summary>
        [XmlElement("Relationship")]
        public List<Relationship> Items
        {
            get
            {
                return _Items;
            }
            set
            {
                if (value == null) value = new List<Relationship>();
                _Items = value;
            }
        }

        #endregion

        #region Private-Members

        private List<Relationship> _Items = new List<Relationship>();

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Office document relationships.
        /// </summary>
        public Relationships()
        {

        }

        /// <summary>
        /// Convert to a dictionary.
        /// </summary>
        /// <param name="relationships">Relationships.</param>
        /// <returns>Dictionary. </returns>
        public static Dictionary<string, string> ToDictionary(Relationships relationships)
        {
            if (relationships == null) throw new ArgumentNullException(nameof(relationships));

            return relationships.ToDictionary();
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                    _Items = null;
                }
                
                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Convert to a dictionary.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> ToDictionary()
        {
            Dictionary<string, string> rels = new Dictionary<string, string>();

            if (Items != null && Items.Count > 0)
            {
                foreach (Relationship rel in Items)
                {
                    rels.Add(rel.Id, rel.Target);
                }
            }

            return rels;
        }

        #endregion

        #region Private-Methods

        #endregion
    }
}
