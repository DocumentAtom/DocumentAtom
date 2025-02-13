namespace DocumentAtom.TypeDetection
{
    using System.Runtime.Serialization;
    using System.Text.Json.Serialization;

    /// <summary>
    /// Data type associated with an input object or file.
    /// </summary>
    public enum DocumentTypeEnum
    {
        /// <summary>
        /// Unknown.
        /// </summary>
        [EnumMember(Value = "Unknown")]
        Unknown,
        /// <summary>
        /// CSV.
        /// </summary>
        [EnumMember(Value = "Csv")]
        Csv,
        /// <summary>
        /// DataTable.
        /// </summary>
        [EnumMember(Value = "DataTable")]
        DataTable,
        /// <summary>
        /// DOCX, Word document.
        /// </summary>
        [EnumMember(Value = "Docx")]
        Docx,
        /// <summary>
        /// HTML.
        /// </summary>
        [EnumMember(Value = "Html")]
        Html,
        /// <summary>
        /// JSON.
        /// </summary>
        [EnumMember(Value = "Json")]
        Json,
        /// <summary>
        /// Keynote.
        /// </summary>
        [EnumMember(Value = "Keynote")]
        Keynote,
        /// <summary>
        /// Markdown.
        /// </summary>
        [EnumMember(Value = "Markdown")]
        Markdown,
        /// <summary>
        /// Numbers.
        /// </summary>
        [EnumMember(Value = "Numbers")]
        Numbers,
        /// <summary>
        /// Pages.
        /// </summary>
        [EnumMember(Value = "Pages")]
        Pages,
        /// <summary>
        /// Parquet.
        /// </summary>
        [EnumMember(Value = "Parquet")]
        Parquet,
        /// <summary>
        /// PDF.
        /// </summary>
        [EnumMember(Value = "Pdf")]
        Pdf,
        /// <summary>
        /// PNG.
        /// </summary>
        [EnumMember(Value = "Png")]
        Png,
        /// <summary>
        /// PostScript.
        /// </summary>
        [EnumMember(Value = "PostScript")]
        PostScript,
        /// <summary>
        /// PPTX, PowerPoint presentation.
        /// </summary>
        [EnumMember(Value = "Pptx")]
        Pptx,
        /// <summary>
        /// Sqlite database file.
        /// </summary>
        [EnumMember(Value = "Sqlite")]
        Sqlite,
        /// <summary>
        /// Text.
        /// </summary>
        [EnumMember(Value = "Text")]
        Text,
        /// <summary>
        /// XLSX, Excel spreadsheet.
        /// </summary> 
        [EnumMember(Value = "Xlsx")]
        Xlsx,
        /// <summary>
        /// XML.
        /// </summary>
        [EnumMember(Value = "Xml")]
        Xml
    }
}
