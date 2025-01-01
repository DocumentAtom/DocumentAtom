namespace DocumentAtom.Core.Helpers
{
    using DocumentAtom.Core.Office;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Xml.Serialization;

    /// <summary>
    /// Office file helper.
    /// </summary>
    public static class OfficeFileHelper
    {
        #region Public-Methods

        /// <summary>
        /// Get relationships from relationships file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Relationships.</returns>
        public static Relationships GetRelationships(string filename)
        {
            if (!File.Exists(filename)) return new Relationships();

            XmlSerializer serializer = new XmlSerializer(typeof(Relationships));

            using (StreamReader reader = new StreamReader(filename))
            {
                return (Relationships)serializer.Deserialize(reader);
            }
        }

        #endregion
    }
}
