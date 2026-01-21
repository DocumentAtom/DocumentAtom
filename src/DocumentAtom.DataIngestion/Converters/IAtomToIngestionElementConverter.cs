namespace DocumentAtom.DataIngestion.Converters
{
    using System;
    using System.Collections.Generic;
    using DocumentAtom.Core.Atoms;

    /// <summary>
    /// Interface for converting Atoms to IngestionDocumentElements.
    /// </summary>
    public interface IAtomToIngestionElementConverter
    {
        /// <summary>
        /// Convert an Atom to an IngestionDocumentElement.
        /// </summary>
        /// <param name="atom">The atom to convert.</param>
        /// <param name="hierarchyMap">Map of GUID to Atom for hierarchy lookups.</param>
        /// <returns>The converted element, or null if the atom should be skipped.</returns>
        IngestionDocumentElement? Convert(Atom atom, Dictionary<Guid, Atom>? hierarchyMap = null);
    }
}
