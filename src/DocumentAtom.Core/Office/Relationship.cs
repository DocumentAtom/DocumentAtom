namespace DocumentAtom.Core.Office
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    /// <summary>
    /// Relationship.
    /// </summary>
    public class Relationship
    {
        #region Public-Members

        /// <summary>
        /// ID.
        /// </summary>
        [XmlAttribute("Id")]
        public string Id { get; set; }

        /// <summary>
        /// Type.
        /// </summary>
        [XmlAttribute("Type")]
        public string Type { get; set; }

        /// <summary>
        /// Target.
        /// </summary>
        [XmlAttribute("Target")]
        public string Target { get; set; }

        /// <summary>
        /// Target mode.
        /// </summary>
        [XmlAttribute("TargetMode")]
        public string TargetMode { get; set; }

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Relationship.
        /// </summary>
        public Relationship()
        {

        }

        #endregion

        #region Public-Methods

        #endregion

        #region Private-Methods

        #endregion
    }
}
