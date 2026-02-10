namespace DocumentAtom.Core.TypeDetection
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
        /// BMP image.
        /// </summary>
        [EnumMember(Value = "Bmp")]
        Bmp,
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
        /// DOC, legacy Word document.
        /// </summary>
        [EnumMember(Value = "Doc")]
        Doc,
        /// <summary>
        /// DOCX, Word document.
        /// </summary>
        [EnumMember(Value = "Docx")]
        Docx,
        /// <summary>
        /// EPUB e-book.
        /// </summary>
        [EnumMember(Value = "Epub")]
        Epub,
        /// <summary>
        /// GIF image.
        /// </summary>
        [EnumMember(Value = "Gif")]
        Gif,
        /// <summary>
        /// GPX GPS Exchange Format.
        /// </summary>
        [EnumMember(Value = "Gpx")]
        Gpx,
        /// <summary>
        /// GZIP compressed file.
        /// </summary>
        [EnumMember(Value = "Gzip")]
        Gzip,
        /// <summary>
        /// HTML.
        /// </summary>
        [EnumMember(Value = "Html")]
        Html,
        /// <summary>
        /// ICO, Icon file.
        /// </summary>
        [EnumMember(Value = "Ico")]
        Ico,
        /// <summary>
        /// JPEG image.
        /// </summary>
        [EnumMember(Value = "Jpeg")]
        Jpeg,
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
        /// MOV video.
        /// </summary>
        [EnumMember(Value = "Mov")]
        Mov,
        /// <summary>
        /// MP3 audio.
        /// </summary>
        [EnumMember(Value = "Mp3")]
        Mp3,
        /// <summary>
        /// MP4 video.
        /// </summary>
        [EnumMember(Value = "Mp4")]
        Mp4,
        /// <summary>
        /// Numbers.
        /// </summary>
        [EnumMember(Value = "Numbers")]
        Numbers,
        /// <summary>
        /// ODP, OpenDocument Presentation.
        /// </summary>
        [EnumMember(Value = "Odp")]
        Odp,
        /// <summary>
        /// ODS, OpenDocument Spreadsheet.
        /// </summary>
        [EnumMember(Value = "Ods")]
        Ods,
        /// <summary>
        /// ODT, OpenDocument Text.
        /// </summary>
        [EnumMember(Value = "Odt")]
        Odt,
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
        /// PPT, legacy PowerPoint presentation.
        /// </summary>
        [EnumMember(Value = "Ppt")]
        Ppt,
        /// <summary>
        /// PPTX, PowerPoint presentation.
        /// </summary>
        [EnumMember(Value = "Pptx")]
        Pptx,
        /// <summary>
        /// RAR archive.
        /// </summary>
        [EnumMember(Value = "Rar")]
        Rar,
        /// <summary>
        /// RTF document.
        /// </summary>
        [EnumMember(Value = "Rtf")]
        Rtf,
        /// <summary>
        /// 7Z archive.
        /// </summary>
        [EnumMember(Value = "SevenZip")]
        SevenZip,
        /// <summary>
        /// Sqlite database file.
        /// </summary>
        [EnumMember(Value = "Sqlite")]
        Sqlite,
        /// <summary>
        /// SVG, Scalable Vector Graphics.
        /// </summary>
        [EnumMember(Value = "Svg")]
        Svg,
        /// <summary>
        /// TAR archive.
        /// </summary>
        [EnumMember(Value = "Tar")]
        Tar,
        /// <summary>
        /// Text.
        /// </summary>
        [EnumMember(Value = "Text")]
        Text,
        /// <summary>
        /// TIFF image.
        /// </summary>
        [EnumMember(Value = "Tiff")]
        Tiff,
        /// <summary>
        /// TSV, Tab-separated values.
        /// </summary>
        [EnumMember(Value = "Tsv")]
        Tsv,
        /// <summary>
        /// WebP image.
        /// </summary>
        [EnumMember(Value = "WebP")]
        WebP,
        /// <summary>
        /// XLS, legacy Excel spreadsheet.
        /// </summary>
        [EnumMember(Value = "Xls")]
        Xls,
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
