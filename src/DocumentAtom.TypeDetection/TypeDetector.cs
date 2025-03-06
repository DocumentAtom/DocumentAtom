namespace DocumentAtom.TypeDetection
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

        private string _Header = "[ViewTypeDetector] ";
        private string _TempDirectory = "./temp/";
        private DirectoryInfo _DirInfo = null;

        private string _CsvMimeType = "text/csv";
        private string _CsvExtension = "csv";

        private string _DocxMimeType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        private string _DocxExtension = "docx";

        private string _HtmlMimeType = "application/html";
        private string _HtmlExtension = "html";

        private string _JsonMimeType = "application/json";
        private string _JsonExtension = "json";

        private string _KeynoteMimeType = "application/vnd.apple.keynote";
        private string _KeynoteExtension = "key";

        private string _MarkdownMimeType = "text/markdown";
        private string _MarkdownExtension = "md";

        private string _NumbersMimeType = "application/vnd.apple.numbers";
        private string _NumbersExtension = "numbers";

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

        private string _PptxMimeType = "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        private string _PptxExtension = "pptx";

        private string _SqliteMimeType = "application/vnd.sqlite3";
        private string _SqliteExtension = "db";

        private string _TextMimeType = "text/plain";
        private string _TextExtension = "txt";

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

                    if (IsPng(data))
                    {
                        tr.MimeType = _PngMimeType;
                        tr.Extension = _PngExtension;
                        tr.Type = DocumentTypeEnum.Png;
                        return tr;
                    }
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
                    else if (IsZipArchive(data))
                    {
                        #region Zip-Archive

                        // Word, Excel, PowerPoint

                        string guid = Guid.NewGuid().ToString();

                        DirectoryInfo dir = UnpackArchive(data, guid, Path.GetFullPath(Path.Combine(_TempDirectory, (guid + "/"))));

                        if (dir == null) return tr;

                        try
                        {
                            if (IsIworkArchive(dir.FullName))
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

                    if (IsMarkdown(contentType))
                    {
                        tr.MimeType = _MarkdownMimeType;
                        tr.Extension = _MarkdownExtension;
                        tr.Type = DocumentTypeEnum.Markdown;
                        return tr;
                    }
                    if (IsCsv(contentType))
                    {
                        tr.MimeType = _CsvMimeType;
                        tr.Extension = _CsvExtension;
                        tr.Type = DocumentTypeEnum.Csv;
                        return tr;
                    }
                    else if (IsJson(data))
                    {
                        tr.MimeType = _JsonMimeType;
                        tr.Extension = _JsonExtension;
                        tr.Type = DocumentTypeEnum.Json;
                        return tr;
                    }
                    else if (IsXml(data))
                    {
                        if (IsHtml(data))
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
                if (data[i] < 0x20 || data[i] > 0x7E)
                    nonPrintableCount++;
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
                var options = new JsonDocumentOptions
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

        #endregion
    }
}
