namespace DocumentAtom.Documents.RichText
{
    using System.Data;

    /// <summary>
    /// Represents table data extracted from RTF.
    /// </summary>
    internal class RtfTableData
    {
        /// <summary>
        /// DataTable containing the extracted table data.
        /// </summary>
        public DataTable DataTable { get; set; } = new DataTable();

        /// <summary>
        /// Raw RTF content for this table.
        /// </summary>
        public string RawRtf { get; set; } = String.Empty;
    }
}