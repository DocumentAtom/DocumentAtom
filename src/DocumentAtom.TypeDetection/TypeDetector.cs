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
    public class TypeDetector
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

        private bool IsBinary(byte[] data, int maxNullCount = 1, int maxBytesToRead = 8000)
        {
            int nullCount = 0;

            for (int i = 0; i < data.Length; i++)
            {
                if (data[i] == 0x00) nullCount++;
                if (nullCount >= maxNullCount) return true;
                if (i >= maxBytesToRead) break;
            }

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
