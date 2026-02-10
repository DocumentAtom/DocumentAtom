namespace DocumentAtom.Core.TypeDetection
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text.Json;
    using System.Text;
    using System.Xml;

    /// <summary>
    /// DocumentAtom type detector.
    /// </summary>
    public class TypeDetector : IDisposable
    {
        #region Public-Members

        /// <summary>
        /// Method to invoke to emit log messages.
        /// </summary>
        public Action<string> Logger { get; set; } = null;

        #endregion

        #region Private-Members

        private string _Header = "[TypeDetector] ";
        private string _TempDirectory = "./temp/";
        private DirectoryInfo _DirInfo = null;

        private string _BmpMimeType = "image/bmp";
        private string _BmpExtension = "bmp";

        private string _CsvMimeType = "text/csv";
        private string _CsvExtension = "csv";

        private string _DocMimeType = "application/msword";
        private string _DocExtension = "doc";

        private string _DocxMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        private string _DocxExtension = "docx";

        private string _EpubMimeType = "application/epub+zip";
        private string _EpubExtension = "epub";

        private string _GifMimeType = "image/gif";
        private string _GifExtension = "gif";

        private string _GpxMimeType = "application/gpx+xml";
        private string _GpxExtension = "gpx";

        private string _GzipMimeType = "application/gzip";
        private string _GzipExtension = "gz";

        private string _HtmlMimeType = "application/html";
        private string _HtmlExtension = "html";

        private string _IcoMimeType = "image/x-icon";
        private string _IcoExtension = "ico";

        private string _JpegMimeType = "image/jpeg";
        private string _JpegExtension = "jpg";

        private string _JsonMimeType = "application/json";
        private string _JsonExtension = "json";

        private string _KeynoteMimeType = "application/vnd.apple.keynote";
        private string _KeynoteExtension = "key";

        private string _MarkdownMimeType = "text/markdown";
        private string _MarkdownExtension = "md";

        private string _MovMimeType = "video/quicktime";
        private string _MovExtension = "mov";

        private string _Mp3MimeType = "audio/mpeg";
        private string _Mp3Extension = "mp3";

        private string _Mp4MimeType = "video/mp4";
        private string _Mp4Extension = "mp4";

        private string _NumbersMimeType = "application/vnd.apple.numbers";
        private string _NumbersExtension = "numbers";

        private string _OdpMimeType = "application/vnd.oasis.opendocument.presentation";
        private string _OdpExtension = "odp";

        private string _OdsMimeType = "application/vnd.oasis.opendocument.spreadsheet";
        private string _OdsExtension = "ods";

        private string _OdtMimeType = "application/vnd.oasis.opendocument.text";
        private string _OdtExtension = "odt";

        private string _PagesMimeType = "application/vnd.apple.pages";
        private string _PagesExtension = "pages";

        private string _ParquetMimeType = "application/vnd.apache.parquet";
        private string _ParquetExtension = "parquet";

        private string _PdfMimeType = "application/pdf";
        private string _PdfExtension = "pdf";

        private string _PngMimeType = "image/png";
        private string _PngExtension = "png";

        private string _PostScriptMimeType = "application/postscript";
        private string _PostScriptExtension = "ps";

        private string _PptMimeType = "application/vnd.ms-powerpoint";
        private string _PptExtension = "ppt";

        private string _PptxMimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        private string _PptxExtension = "pptx";

        private string _RarMimeType = "application/vnd.rar";
        private string _RarExtension = "rar";

        private string _RtfMimeType = "application/rtf";
        private string _RtfExtension = "rtf";

        private string _SevenZMimeType = "application/x-7z-compressed";
        private string _SevenZExtension = "7z";

        private string _SqliteMimeType = "application/vnd.sqlite3";
        private string _SqliteExtension = "db";

        private string _SvgMimeType = "image/svg+xml";
        private string _SvgExtension = "svg";

        private string _TarMimeType = "application/x-tar";
        private string _TarExtension = "tar";

        private string _TextMimeType = "text/plain";
        private string _TextExtension = "txt";

        private string _TiffMimeType = "image/tiff";
        private string _TiffExtension = "tiff";

        private string _TsvMimeType = "text/tab-separated-values";
        private string _TsvExtension = "tsv";

        private string _WebPMimeType = "image/webp";
        private string _WebPExtension = "webp";

        private string _XlsMimeType = "application/vnd.ms-excel";
        private string _XlsExtension = "xls";

        private string _XlsxMimeType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        private string _XlsxExtension = "xlsx";

        private string _XmlMimeType = "text/xml";
        private string _XmlExtension = "xml";

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// DocumentAtom type detector.
        /// </summary>
        /// <param name="tempDirectory">Temporary directory.</param>
        public TypeDetector(string tempDirectory = "./temp/")
        {
            if (!String.IsNullOrEmpty(tempDirectory)) _TempDirectory = tempDirectory;
            if (!Directory.Exists(_TempDirectory)) Directory.CreateDirectory(_TempDirectory);
            _DirInfo = new DirectoryInfo(Path.GetFullPath(_TempDirectory));
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
                    Logger = null;

                    _DirInfo = null;
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
        /// Determine the type of a given file.  CSV file types are inferred from the supplied content type header.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <param name="contentType">Content-type header value, if any.
        /// The content-type header is used to infer the content-type for certain difficult types, including CSV.
        /// </param>
        /// <returns>TypeResult.</returns>
        public TypeResult Process(string filename, string contentType = null)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return Process(File.ReadAllBytes(filename), contentType);
        }

        /// <summary>
        /// Determine the type of supplied data.  CSV file types are inferred from the supplied content type header.
        /// </summary>
        /// <param name="data">Data.</param>
        /// <param name="contentType">
        /// Content-type header value, if any.
        /// The content-type header is used to infer the content-type for certain difficult types, including CSV.
        /// </param>
        /// <returns>TypeResult.</returns>
        public TypeResult Process(byte[] data, string contentType = null)
        {
            if (data == null || data.Length < 1) throw new ArgumentException("No input data supplied.");

            TypeResult tr = new TypeResult
            {
                MimeType = "application/octet-stream",
                Extension = null,
                Type = DocumentTypeEnum.Unknown
            };

            try
            {
                bool isBinary = IsBinary(data);

                if (isBinary)
                {
                    #region Binary

                    // Images
                    if (IsPng(data))
                    {
                        tr.MimeType = _PngMimeType;
                        tr.Extension = _PngExtension;
                        tr.Type = DocumentTypeEnum.Png;
                        return tr;
                    }
                    else if (IsJpeg(data))
                    {
                        tr.MimeType = _JpegMimeType;
                        tr.Extension = _JpegExtension;
                        tr.Type = DocumentTypeEnum.Jpeg;
                        return tr;
                    }
                    else if (IsGif(data))
                    {
                        tr.MimeType = _GifMimeType;
                        tr.Extension = _GifExtension;
                        tr.Type = DocumentTypeEnum.Gif;
                        return tr;
                    }
                    else if (IsTiff(data))
                    {
                        tr.MimeType = _TiffMimeType;
                        tr.Extension = _TiffExtension;
                        tr.Type = DocumentTypeEnum.Tiff;
                        return tr;
                    }
                    else if (IsBmp(data))
                    {
                        tr.MimeType = _BmpMimeType;
                        tr.Extension = _BmpExtension;
                        tr.Type = DocumentTypeEnum.Bmp;
                        return tr;
                    }
                    else if (IsWebP(data))
                    {
                        tr.MimeType = _WebPMimeType;
                        tr.Extension = _WebPExtension;
                        tr.Type = DocumentTypeEnum.WebP;
                        return tr;
                    }
                    else if (IsIco(data))
                    {
                        tr.MimeType = _IcoMimeType;
                        tr.Extension = _IcoExtension;
                        tr.Type = DocumentTypeEnum.Ico;
                        return tr;
                    }
                    // Documents and Data
                    else if (IsParquet(data))
                    {
                        tr.MimeType = _ParquetMimeType;
                        tr.Extension = _ParquetExtension;
                        tr.Type = DocumentTypeEnum.Parquet;
                        return tr;
                    }
                    else if (IsPdf(data))
                    {
                        tr.MimeType = _PdfMimeType;
                        tr.Extension = _PdfExtension;
                        tr.Type = DocumentTypeEnum.Pdf;
                        return tr;
                    }
                    else if (IsPostScript(data))
                    {
                        tr.MimeType = _PostScriptMimeType;
                        tr.Extension = _PostScriptExtension;
                        tr.Type = DocumentTypeEnum.PostScript;
                        return tr;
                    }
                    else if (IsSqlite(data))
                    {
                        tr.MimeType = _SqliteMimeType;
                        tr.Extension = _SqliteExtension;
                        tr.Type = DocumentTypeEnum.Sqlite;
                        return tr;
                    }
                    // Legacy Office formats
                    else if (IsDoc(data))
                    {
                        tr.MimeType = _DocMimeType;
                        tr.Extension = _DocExtension;
                        tr.Type = DocumentTypeEnum.Doc;
                        return tr;
                    }
                    else if (IsXls(data))
                    {
                        tr.MimeType = _XlsMimeType;
                        tr.Extension = _XlsExtension;
                        tr.Type = DocumentTypeEnum.Xls;
                        return tr;
                    }
                    else if (IsPpt(data))
                    {
                        tr.MimeType = _PptMimeType;
                        tr.Extension = _PptExtension;
                        tr.Type = DocumentTypeEnum.Ppt;
                        return tr;
                    }
                    // Multimedia
                    else if (IsMp3(data))
                    {
                        tr.MimeType = _Mp3MimeType;
                        tr.Extension = _Mp3Extension;
                        tr.Type = DocumentTypeEnum.Mp3;
                        return tr;
                    }
                    else if (IsMp4OrMov(data))
                    {
                        // Need to distinguish between MP4 and MOV
                        // Check for quicktime specific markers
                        try
                        {
                            string content = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 100));
                            if (content.Contains("qt") || content.Contains("moov"))
                            {
                                tr.MimeType = _MovMimeType;
                                tr.Extension = _MovExtension;
                                tr.Type = DocumentTypeEnum.Mov;
                            }
                            else
                            {
                                tr.MimeType = _Mp4MimeType;
                                tr.Extension = _Mp4Extension;
                                tr.Type = DocumentTypeEnum.Mp4;
                            }
                        }
                        catch
                        {
                            tr.MimeType = _Mp4MimeType;
                            tr.Extension = _Mp4Extension;
                            tr.Type = DocumentTypeEnum.Mp4;
                        }
                        return tr;
                    }
                    // Archives
                    else if (IsSevenZ(data))
                    {
                        tr.MimeType = _SevenZMimeType;
                        tr.Extension = _SevenZExtension;
                        tr.Type = DocumentTypeEnum.SevenZip;
                        return tr;
                    }
                    else if (IsRar(data))
                    {
                        tr.MimeType = _RarMimeType;
                        tr.Extension = _RarExtension;
                        tr.Type = DocumentTypeEnum.Rar;
                        return tr;
                    }
                    else if (IsTar(data))
                    {
                        tr.MimeType = _TarMimeType;
                        tr.Extension = _TarExtension;
                        tr.Type = DocumentTypeEnum.Tar;
                        return tr;
                    }
                    else if (IsGzip(data))
                    {
                        tr.MimeType = _GzipMimeType;
                        tr.Extension = _GzipExtension;
                        tr.Type = DocumentTypeEnum.Gzip;
                        return tr;
                    }
                    else if (IsZipArchive(data))
                    {
                        #region Zip-Archive

                        // Word, Excel, PowerPoint

                        string guid = Guid.NewGuid().ToString();

                        DirectoryInfo dir = UnpackArchive(data, guid, Path.GetFullPath(Path.Combine(_TempDirectory, (guid + "/"))));

                        if (dir == null) return tr;

                        try
                        {
                            if (IsEpub(dir.FullName))
                            {
                                tr.MimeType = _EpubMimeType;
                                tr.Extension = _EpubExtension;
                                tr.Type = DocumentTypeEnum.Epub;
                                return tr;
                            }
                            else if (IsOdt(dir.FullName))
                            {
                                tr.MimeType = _OdtMimeType;
                                tr.Extension = _OdtExtension;
                                tr.Type = DocumentTypeEnum.Odt;
                                return tr;
                            }
                            else if (IsOds(dir.FullName))
                            {
                                tr.MimeType = _OdsMimeType;
                                tr.Extension = _OdsExtension;
                                tr.Type = DocumentTypeEnum.Ods;
                                return tr;
                            }
                            else if (IsOdp(dir.FullName))
                            {
                                tr.MimeType = _OdpMimeType;
                                tr.Extension = _OdpExtension;
                                tr.Type = DocumentTypeEnum.Odp;
                                return tr;
                            }
                            else if (IsIworkArchive(dir.FullName))
                            {
                                if (IsKeynoteArchive(dir.FullName))
                                {
                                    tr.MimeType = _KeynoteMimeType;
                                    tr.Extension = _KeynoteExtension;
                                    tr.Type = DocumentTypeEnum.Keynote;
                                    return tr;
                                }
                                else if (IsNumbersArchive(dir.FullName))
                                {
                                    tr.MimeType = _NumbersMimeType;
                                    tr.Extension = _NumbersExtension;
                                    tr.Type = DocumentTypeEnum.Numbers;
                                    return tr;
                                }
                                else if (IsPagesArchive(dir.FullName))
                                {
                                    tr.MimeType = _PagesMimeType;
                                    tr.Extension = _PagesExtension;
                                    tr.Type = DocumentTypeEnum.Pages;
                                    return tr;
                                }
                                else
                                {
                                    return tr;
                                }
                            }
                            else if (IsExcelArchive(dir.FullName))
                            {
                                tr.MimeType = _XlsxMimeType;
                                tr.Extension = _XlsxExtension;
                                tr.Type = DocumentTypeEnum.Xlsx;
                                return tr;
                            }
                            else if (IsPowerPointArchive(dir.FullName))
                            {
                                tr.MimeType = _PptxMimeType;
                                tr.Extension = _PptxExtension;
                                tr.Type = DocumentTypeEnum.Pptx;
                                return tr;
                            }
                            else if (IsWordArchive(dir.FullName))
                            {
                                tr.MimeType = _DocxMimeType;
                                tr.Extension = _DocxExtension;
                                tr.Type = DocumentTypeEnum.Docx;
                                return tr;
                            }
                            else
                            {
                                return tr;
                            }
                        }
                        catch (Exception eInner)
                        {
                            Logger?.Invoke(_Header + "exception while unpacking archive" + Environment.NewLine + eInner.ToString());
                            return tr;
                        }
                        finally
                        {
                            RecursiveDelete(dir, true);
                        }

                        #endregion
                    }
                    else
                    {
                        Logger?.Invoke(_Header + "unable to discern type of supplied binary data");
                        return tr;
                    }

                    #endregion
                }
                else
                {
                    #region Text

                    // RTF is technically text but starts with specific signature
                    if (IsRtf(data))
                    {
                        tr.MimeType = _RtfMimeType;
                        tr.Extension = _RtfExtension;
                        tr.Type = DocumentTypeEnum.Rtf;
                        return tr;
                    }
                    // Content-type specific formats
                    else if (IsMarkdown(contentType))
                    {
                        tr.MimeType = _MarkdownMimeType;
                        tr.Extension = _MarkdownExtension;
                        tr.Type = DocumentTypeEnum.Markdown;
                        return tr;
                    }
                    else if (IsCsv(contentType))
                    {
                        tr.MimeType = _CsvMimeType;
                        tr.Extension = _CsvExtension;
                        tr.Type = DocumentTypeEnum.Csv;
                        return tr;
                    }
                    else if (IsTsv(contentType))
                    {
                        tr.MimeType = _TsvMimeType;
                        tr.Extension = _TsvExtension;
                        tr.Type = DocumentTypeEnum.Tsv;
                        return tr;
                    }
                    // Structured data formats
                    else if (IsJson(data))
                    {
                        tr.MimeType = _JsonMimeType;
                        tr.Extension = _JsonExtension;
                        tr.Type = DocumentTypeEnum.Json;
                        return tr;
                    }
                    else if (IsXml(data))
                    {
                        // Check for specific XML-based formats first
                        if (IsSvg(data))
                        {
                            tr.MimeType = _SvgMimeType;
                            tr.Extension = _SvgExtension;
                            tr.Type = DocumentTypeEnum.Svg;
                            return tr;
                        }
                        else if (IsGpx(data))
                        {
                            tr.MimeType = _GpxMimeType;
                            tr.Extension = _GpxExtension;
                            tr.Type = DocumentTypeEnum.Gpx;
                            return tr;
                        }
                        else if (IsHtml(data))
                        {
                            tr.MimeType = _HtmlMimeType;
                            tr.Extension = _HtmlExtension;
                            tr.Type = DocumentTypeEnum.Html;
                            return tr;
                        }
                        else
                        {
                            tr.MimeType = _XmlMimeType;
                            tr.Extension = _XmlExtension;
                            tr.Type = DocumentTypeEnum.Xml;
                            return tr;
                        }
                    }
                    // HTML (non-XML compliant)
                    else if (IsHtml(data))
                    {
                        tr.MimeType = _HtmlMimeType;
                        tr.Extension = _HtmlExtension;
                        tr.Type = DocumentTypeEnum.Html;
                        return tr;
                    }
                    // Default to plain text
                    else
                    {
                        tr.MimeType = _TextMimeType;
                        tr.Extension = _TextExtension;
                        tr.Type = DocumentTypeEnum.Text;
                        return tr;
                    }

                    #endregion
                }
            }
            catch (Exception e)
            {
                Logger?.Invoke(_Header + "exception encountered while detecting content type: " + Environment.NewLine + e.ToString());
                return tr;
            }
        }

        #endregion

        #region Private-Methods

        private bool IsBinary(byte[] data, int maxBytesToRead = 8000)
        {
            // Check for known binary file signatures first
            if (data.Length >= 8)
            {
                // PostScript signature: %!PS or just %!
                if (data.Length >= 3 && data[0] == 0x25 && data[1] == 0x21)
                {
                    // Check for complete "%!PS" signature
                    if (data.Length >= 4 && data[2] == 0x50 && data[3] == 0x53)
                        return true;

                    // For PostScript files that might start with just "%!"
                    // Scan a bit further for "PostScript" or "EPSF" markers
                    if (data.Length >= 20)
                    {
                        string header = System.Text.Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 100));
                        if (header.IndexOf("PostScript", StringComparison.OrdinalIgnoreCase) >= 0 ||
                            header.IndexOf("EPSF", StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }

                // Some EPS files might have the PS signature slightly offset
                // Scan the first few hundred bytes
                int scanLength = Math.Min(data.Length, 500);
                for (int i = 0; i < scanLength - 3; i++)
                {
                    if (data[i] == 0x25 && data[i + 1] == 0x21 &&
                        data[i + 2] == 0x50 && data[i + 3] == 0x53)
                        return true;
                }

                // PDF signature: %PDF
                if (data[0] == 0x25 && data[1] == 0x50 && data[2] == 0x44 && data[3] == 0x46)
                    return true;

                // PNG signature: 89 50 4E 47 0D 0A 1A 0A
                if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47 &&
                    data[4] == 0x0D && data[5] == 0x0A && data[6] == 0x1A && data[7] == 0x0A)
                    return true;

                // JPEG signatures: FF D8 FF
                if (data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF)
                    return true;

                // GIF signatures: GIF87a or GIF89a
                if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 &&
                    data[3] == 0x38 && (data[4] == 0x37 || data[4] == 0x39) && data[5] == 0x61)
                    return true;

                // ZIP signatures (also used by docx, xlsx, jar, etc.): PK
                if (data[0] == 0x50 && data[1] == 0x4B &&
                    ((data[2] == 0x03 && data[3] == 0x04) || (data[2] == 0x05 && data[3] == 0x06)))
                    return true;

                // RAR signature: Rar!
                if (data[0] == 0x52 && data[1] == 0x61 && data[2] == 0x72 && data[3] == 0x21)
                    return true;

                // TIFF signature: 49 49 2A 00 or 4D 4D 00 2A
                if ((data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A && data[3] == 0x00) ||
                    (data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00 && data[3] == 0x2A))
                    return true;

                // BMP signature: BM
                if (data[0] == 0x42 && data[1] == 0x4D)
                    return true;

                // EXE/DLL signature: MZ
                if (data[0] == 0x4D && data[1] == 0x5A)
                    return true;

                // MP3 signature: ID3 or starting with 0xFF 0xFB
                if ((data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33) ||
                    (data[0] == 0xFF && (data[1] == 0xFB || data[1] == 0xF3 || data[1] == 0xF2)))
                    return true;

                // MP4/MOV/QuickTime signature: ftyp or moov
                if ((data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70) ||
                    (data[4] == 0x6D && data[5] == 0x6F && data[6] == 0x6F && data[7] == 0x76))
                    return true;

                // WebP signature: RIFF + ????WEBP
                if (data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                    data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50 && data.Length >= 12)
                    return true;

                // 7z signature: 7z
                if (data[0] == 0x37 && data[1] == 0x7A && data[2] == 0xBC && data[3] == 0xAF &&
                    data[4] == 0x27 && data[5] == 0x1C)
                    return true;

                // SQL Lite Database: SQLite format 3
                if (data[0] == 0x53 && data[1] == 0x51 && data[2] == 0x4C && data[3] == 0x69 &&
                    data[4] == 0x74 && data[5] == 0x65)
                    return true;
            }

            // Limit the number of bytes to check
            int bytesToCheck = Math.Min(data.Length, maxBytesToRead);

            // Count of characters that are likely to appear in binary files
            int binaryCharCount = 0;
            int nullByteCount = 0;

            for (int i = 0; i < bytesToCheck; i++)
            {
                byte b = data[i];

                // Null bytes are definitely binary
                if (b == 0x00)
                {
                    nullByteCount++;
                    if (nullByteCount >= 3) // Allow a couple of null bytes in text files
                        return true;
                }

                // Control characters (except common text file characters like CR, LF, tab)
                if (b < 0x08 || (b > 0x0D && b < 0x20 && b != 0x1B)) // Excluding ESC character (0x1B) which can appear in text terminals
                    binaryCharCount++;

                // If we've seen enough binary characters, consider it binary
                if (binaryCharCount > 10) // Threshold can be adjusted
                    return true;
            }

            // For UTF-16 / UTF-32 detection (common patterns with null bytes alternating with text)
            if (data.Length >= 4)
            {
                // UTF-16 BOM check
                if ((data[0] == 0xFE && data[1] == 0xFF) || (data[0] == 0xFF && data[1] == 0xFE))
                    return true;

                // UTF-32 BOM check
                if ((data[0] == 0x00 && data[1] == 0x00 && data[2] == 0xFE && data[3] == 0xFF) ||
                    (data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x00 && data[3] == 0x00))
                    return true;
            }

            // Final check for high concentration of bytes outside ASCII printable range
            int nonPrintableCount = 0;
            for (int i = 0; i < bytesToCheck; i++)
            {
                if ((data[i] < 0x20 || data[i] > 0x7E)
                    && (data[i] != 0x09 && data[i] != 0x0A && data[i] != 0x0D))
                {
                    nonPrintableCount++;
                }
            }

            // If more than 30% of content is non-printable, likely binary
            if (bytesToCheck > 0 && (double)nonPrintableCount / bytesToCheck > 0.3)
                return true;

            return false;
        }

        private bool IsPng(byte[] data)
        {
            // PNG signature: 89 50 4E 47 0D 0A 1A 0A
            if (data.Length < 8) return false;

            return data[0] == 0x89 &&
                   data[1] == 0x50 &&
                   data[2] == 0x4E &&
                   data[3] == 0x47 &&
                   data[4] == 0x0D &&
                   data[5] == 0x0A &&
                   data[6] == 0x1A &&
                   data[7] == 0x0A;
        }

        private bool IsJpeg(byte[] data)
        {
            // JPEG signature: FF D8 FF
            if (data.Length < 3) return false;

            return data[0] == 0xFF && data[1] == 0xD8 && data[2] == 0xFF;
        }

        private bool IsGif(byte[] data)
        {
            // GIF signatures: GIF87a or GIF89a
            if (data.Length < 6) return false;

            return data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46 &&
                   data[3] == 0x38 && (data[4] == 0x37 || data[4] == 0x39) && data[5] == 0x61;
        }

        private bool IsTiff(byte[] data)
        {
            // TIFF signature: 49 49 2A 00 (little endian) or 4D 4D 00 2A (big endian)
            if (data.Length < 4) return false;

            return (data[0] == 0x49 && data[1] == 0x49 && data[2] == 0x2A && data[3] == 0x00) ||
                   (data[0] == 0x4D && data[1] == 0x4D && data[2] == 0x00 && data[3] == 0x2A);
        }

        private bool IsBmp(byte[] data)
        {
            // BMP signature: BM
            if (data.Length < 2) return false;

            return data[0] == 0x42 && data[1] == 0x4D;
        }

        private bool IsWebP(byte[] data)
        {
            // WebP signature: RIFF + ????WEBP
            if (data.Length < 12) return false;

            return data[0] == 0x52 && data[1] == 0x49 && data[2] == 0x46 && data[3] == 0x46 &&
                   data[8] == 0x57 && data[9] == 0x45 && data[10] == 0x42 && data[11] == 0x50;
        }

        private bool IsIco(byte[] data)
        {
            // ICO signature: 00 00 01 00 (icon) or 00 00 02 00 (cursor)
            if (data.Length < 4) return false;

            return data[0] == 0x00 && data[1] == 0x00 &&
                   (data[2] == 0x01 || data[2] == 0x02) && data[3] == 0x00;
        }

        private bool IsSvg(byte[] data)
        {
            try
            {
                string text = GetTextWithBestEncoding(data).Trim();
                return text.StartsWith("<?xml", StringComparison.OrdinalIgnoreCase) &&
                       text.Contains("<svg", StringComparison.OrdinalIgnoreCase) ||
                       text.StartsWith("<svg", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private string GetTextWithBestEncoding(byte[] data)
        {
            // Check for BOM first
            if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
                return Encoding.UTF8.GetString(data, 3, data.Length - 3);

            if (data.Length >= 2 && data[0] == 0xFF && data[1] == 0xFE)
                return Encoding.Unicode.GetString(data, 2, data.Length - 2);

            if (data.Length >= 2 && data[0] == 0xFE && data[1] == 0xFF)
                return Encoding.BigEndianUnicode.GetString(data, 2, data.Length - 2);

            if (data.Length >= 4 && data[0] == 0xFF && data[1] == 0xFE && data[2] == 0x00 && data[3] == 0x00)
                return Encoding.UTF32.GetString(data, 4, data.Length - 4);

            if (data.Length >= 4 && data[0] == 0x00 && data[1] == 0x00 && data[2] == 0xFE && data[3] == 0xFF)
                return Encoding.UTF32.GetString(data, 4, data.Length - 4);

            // Try UTF-8 first, fall back to others if needed
            try
            {
                return Encoding.UTF8.GetString(data);
            }
            catch
            {
                try
                {
                    return Encoding.ASCII.GetString(data);
                }
                catch
                {
                    return Encoding.Default.GetString(data);
                }
            }
        }

        private bool IsPostScript(byte[] data)
        {
            if (data == null || data.Length < 4) return false;

            return (data[0] == 0x25 && data[1] == 0x21 && data[2] == 0x50 && data[3] == 0x53) || // %!PS
                   (data[0] == 0xC5 && data[1] == 0xD0 && data[2] == 0xD3 && data[3] == 0xC6);   // DOS EPS Binary
        }

        private bool IsParquet(byte[] data)
        {
            if (data.Length >= 4)
            {
                if (data[0] == 0x50
                    && data[1] == 0x41
                    && data[2] == 0x52
                    && data[3] == 0x31)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsSqlite(byte[] data)
        {
            byte[] expectedMagicNumber = new byte[]
            {
                0x53, 0x51, 0x4C, 0x69, 0x74, 0x65, 0x20, 0x66,
                0x6F, 0x72, 0x6D, 0x61, 0x74, 0x20, 0x33, 0x00
            };

            return data.Take(16).SequenceEqual(expectedMagicNumber);
        }

        private bool IsPdf(byte[] data)
        {
            if (data.Length >= 5)
            {
                if (data[0] == 0x25
                    && data[1] == 0x50
                    && data[2] == 0x44
                    && data[3] == 0x46
                    && data[4] == 0x2D)
                {
                    return true;
                }
            }

            return false;
        }

        private bool IsZipArchive(byte[] data)
        {
            if (data.Length >= 4)
            {
                if (data[0] == 0x50 && data[1] == 0x4B)
                {
                    if (data[2] == 0x03)
                    {
                        return (data[3] == 0x04);
                    }
                    else if (data[2] == 0x05)
                    {
                        return (data[3] == 0x06);
                    }
                    else if (data[2] == 0x07)
                    {
                        return (data[3] == 0x08);
                    }
                }
            }

            return false;
        }

        private DirectoryInfo UnpackArchive(byte[] data, string guid, string directory)
        {
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            try
            {
                string tempFile = Path.GetFullPath(Path.Combine(directory, guid));

                File.WriteAllBytes(tempFile, data);

                using (ZipArchive archive = ZipFile.OpenRead(tempFile))
                {
                    archive.ExtractToDirectory(directory);
                }

                File.Delete(tempFile);

                return new DirectoryInfo(directory);
            }
            catch (Exception e)
            {
                Logger?.Invoke(_Header + "unable to unpack archive:" + Environment.NewLine + e.ToString());
                return null;
            }
        }

        private void RecursiveDelete(DirectoryInfo baseDir, bool isRootDir)
        {
            if (!baseDir.Exists) return;
            foreach (DirectoryInfo dir in baseDir.EnumerateDirectories()) RecursiveDelete(dir, false);

            foreach (FileInfo file in baseDir.GetFiles())
            {
                file.IsReadOnly = false;
                file.Delete();
            }
            if (!isRootDir)
            {
                baseDir.Delete();
            }
        }

        private bool IsExcelArchive(string directory)
        {
            if (Directory.Exists(Path.Combine(directory + "_rels/"))
                && Directory.Exists(Path.Combine(directory + "docProps/"))
                && Directory.Exists(Path.Combine(directory + "xl"))
                && File.Exists(Path.Combine(directory + "[Content_Types].xml")))
            {
                return true;
            }

            return false;
        }

        private bool IsPowerPointArchive(string directory)
        {
            if (Directory.Exists(Path.Combine(directory + "_rels/"))
                && Directory.Exists(Path.Combine(directory + "docProps/"))
                && Directory.Exists(Path.Combine(directory + "ppt"))
                && File.Exists(Path.Combine(directory + "[Content_Types].xml")))
            {
                return true;
            }

            return false;
        }

        private bool IsWordArchive(string directory)
        {
            if (Directory.Exists(Path.Combine(directory + "_rels/"))
                && Directory.Exists(Path.Combine(directory + "docProps/"))
                && Directory.Exists(Path.Combine(directory + "word"))
                && File.Exists(Path.Combine(directory + "[Content_Types].xml")))
            {
                return true;
            }

            return false;
        }

        private bool IsIworkArchive(string directory)
        {
            if (Directory.Exists(Path.Combine(directory + "Data"))
                && Directory.Exists(Path.Combine(directory + "Index"))
                && Directory.Exists(Path.Combine(directory + "Metadata")))
            {
                return Directory.EnumerateFiles(Path.Combine(directory + "Index"), "*.iwa").Any();
            }

            return false;
        }

        private bool IsKeynoteArchive(string directory)
        {
            return Directory.EnumerateFiles(Path.Combine(directory + "Index"), "Slide*.iwa").Any();
        }

        private bool IsNumbersArchive(string directory)
        {
            return Directory.EnumerateFiles(Path.Combine(directory + "Index"), "AnnotationAuthorStorage-*.iwa").Any()
                && Directory.EnumerateFiles(Path.Combine(directory + "Index"), "CalculationEngine-*.iwa").Any();
        }

        private bool IsPagesArchive(string directory)
        {
            return Directory.EnumerateFiles(Path.Combine(directory + "Index"), "AnnotationAuthorStorage.iwa").Any()
                && Directory.EnumerateFiles(Path.Combine(directory + "Index"), "CalculationEngine.iwa").Any();
        }

        private bool IsJson(byte[] data)
        {
            try
            {
                JsonDocumentOptions options = new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip
                };

                using (JsonDocument doc = JsonDocument.Parse(data, options))
                {
                    // dispose any created doc
                }

                return true;
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private bool IsXml(byte[] data)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(Encoding.UTF8.GetString(data));
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool IsCsv(NameValueCollection headers)
        {
            if (headers != null && headers.Count > 0)
            {
                string contentType = headers.Get("content-type");
                if (!String.IsNullOrEmpty(contentType)) return IsCsv(contentType);
            }

            return false;
        }

        private bool IsMarkdown(string contentType)
        {
            if (!String.IsNullOrEmpty(contentType) && contentType.ToLower().Contains("/markdown")) return true;
            return false;
        }

        private bool IsCsv(string contentType)
        {
            if (!String.IsNullOrEmpty(contentType) && contentType.ToLower().Contains("/csv")) return true;
            return false;
        }

        private bool IsHtml(byte[] data)
        {
            string text = Encoding.UTF8.GetString(data).Trim().ToLower();

            if (text.StartsWith("<!doctype html>")) return true;

            if (text.Contains("<head")
                && text.Contains("</head")
                && text.Contains("<body")
                && text.Contains("</body"))
            {
                return true;
            }

            return false;
        }

        private bool IsRtf(byte[] data)
        {
            try
            {
                string text = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 20)).Trim();
                return text.StartsWith("{\\rtf", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }

        private bool IsEpub(string directory)
        {
            // EPUB is a ZIP archive with specific structure
            if (File.Exists(Path.Combine(directory, "mimetype")))
            {
                try
                {
                    string mimeContent = File.ReadAllText(Path.Combine(directory, "mimetype"));
                    return mimeContent.Trim() == "application/epub+zip";
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }

        private bool IsDoc(byte[] data)
        {
            // DOC files use OLE2 compound document format
            // OLE2 signature: D0 CF 11 E0 A1 B1 1A E1
            if (data.Length < 8) return false;

            bool isOle2 = data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
                         data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1;

            if (!isOle2) return false;

            // Additional check for Word document indicators
            // Look for Word-specific OLE streams in the first 2KB
            try
            {
                string content = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 2048));
                return content.Contains("Microsoft Office Word") ||
                       content.Contains("Word.Document") ||
                       content.Contains("WordDocument");
            }
            catch
            {
                return false;
            }
        }

        private bool IsXls(byte[] data)
        {
            // XLS files use OLE2 compound document format
            // OLE2 signature: D0 CF 11 E0 A1 B1 1A E1
            if (data.Length < 8) return false;

            bool isOle2 = data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
                         data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1;

            if (!isOle2) return false;

            // Additional check for Excel document indicators
            try
            {
                string content = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 2048));
                return content.Contains("Microsoft Office Excel") ||
                       content.Contains("Excel.Sheet") ||
                       content.Contains("Workbook");
            }
            catch
            {
                return false;
            }
        }

        private bool IsPpt(byte[] data)
        {
            // PPT files use OLE2 compound document format
            // OLE2 signature: D0 CF 11 E0 A1 B1 1A E1
            if (data.Length < 8) return false;

            bool isOle2 = data[0] == 0xD0 && data[1] == 0xCF && data[2] == 0x11 && data[3] == 0xE0 &&
                         data[4] == 0xA1 && data[5] == 0xB1 && data[6] == 0x1A && data[7] == 0xE1;

            if (!isOle2) return false;

            // Additional check for PowerPoint document indicators
            try
            {
                string content = Encoding.ASCII.GetString(data, 0, Math.Min(data.Length, 2048));
                return content.Contains("Microsoft Office PowerPoint") ||
                       content.Contains("PowerPoint Document") ||
                       content.Contains("PP") || // Common PowerPoint indicator
                       content.Contains("Current User");
            }
            catch
            {
                return false;
            }
        }

        private bool IsOpenDocument(string directory)
        {
            // OpenDocument formats are ZIP archives with specific structure
            return File.Exists(Path.Combine(directory, "META-INF", "manifest.xml")) &&
                   File.Exists(Path.Combine(directory, "content.xml"));
        }

        private bool IsOdt(string directory)
        {
            if (!IsOpenDocument(directory)) return false;

            try
            {
                string manifestPath = Path.Combine(directory, "META-INF", "manifest.xml");
                string manifestContent = File.ReadAllText(manifestPath);
                return manifestContent.Contains("application/vnd.oasis.opendocument.text");
            }
            catch
            {
                return false;
            }
        }

        private bool IsOds(string directory)
        {
            if (!IsOpenDocument(directory)) return false;

            try
            {
                string manifestPath = Path.Combine(directory, "META-INF", "manifest.xml");
                string manifestContent = File.ReadAllText(manifestPath);
                return manifestContent.Contains("application/vnd.oasis.opendocument.spreadsheet");
            }
            catch
            {
                return false;
            }
        }

        private bool IsOdp(string directory)
        {
            if (!IsOpenDocument(directory)) return false;

            try
            {
                string manifestPath = Path.Combine(directory, "META-INF", "manifest.xml");
                string manifestContent = File.ReadAllText(manifestPath);
                return manifestContent.Contains("application/vnd.oasis.opendocument.presentation");
            }
            catch
            {
                return false;
            }
        }

        private bool IsSevenZ(byte[] data)
        {
            // 7z signature: 37 7A BC AF 27 1C
            if (data.Length < 6) return false;

            return data[0] == 0x37 && data[1] == 0x7A && data[2] == 0xBC &&
                   data[3] == 0xAF && data[4] == 0x27 && data[5] == 0x1C;
        }

        private bool IsRar(byte[] data)
        {
            // RAR signature: Rar! or Rar!\x1A\x07\x00 (newer)
            if (data.Length < 4) return false;

            return data[0] == 0x52 && data[1] == 0x61 && data[2] == 0x72 && data[3] == 0x21;
        }

        private bool IsTar(byte[] data)
        {
            // TAR files have a header at offset 257 with "ustar" signature
            if (data.Length < 262) return false;

            return data[257] == 0x75 && data[258] == 0x73 && data[259] == 0x74 &&
                   data[260] == 0x61 && data[261] == 0x72;
        }

        private bool IsGzip(byte[] data)
        {
            // GZIP signature: 1F 8B
            if (data.Length < 2) return false;

            return data[0] == 0x1F && data[1] == 0x8B;
        }

        private bool IsMp3(byte[] data)
        {
            // MP3 signature: ID3 or starting with 0xFF 0xFB
            if (data.Length < 3) return false;

            return (data[0] == 0x49 && data[1] == 0x44 && data[2] == 0x33) ||
                   (data[0] == 0xFF && (data[1] == 0xFB || data[1] == 0xF3 || data[1] == 0xF2));
        }

        private bool IsMp4OrMov(byte[] data)
        {
            // MP4/MOV/QuickTime signature: ftyp or moov at offset 4
            if (data.Length < 8) return false;

            return (data[4] == 0x66 && data[5] == 0x74 && data[6] == 0x79 && data[7] == 0x70) ||
                   (data[4] == 0x6D && data[5] == 0x6F && data[6] == 0x6F && data[7] == 0x76);
        }

        private bool IsGpx(byte[] data)
        {
            try
            {
                string text = GetTextWithBestEncoding(data).Trim();
                return text.Contains("<gpx") && text.Contains("</gpx>") ||
                       text.Contains("<?xml") && text.Contains("gpx");
            }
            catch
            {
                return false;
            }
        }

        private bool IsTsv(string contentType)
        {
            if (!String.IsNullOrEmpty(contentType) && contentType.ToLower().Contains("/tab-separated-values")) return true;
            return false;
        }

        #endregion
    }
}
