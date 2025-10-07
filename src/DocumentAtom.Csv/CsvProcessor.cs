namespace DocumentAtom.Csv
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Create atoms from CSV documents.
    /// </summary>
    public class CsvProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// CSV processor settings.
        /// </summary>
        public new CsvProcessorSettings Settings
        {
            get
            {
                return _Settings;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Settings));
                _Settings = value;
            }
        }

        #endregion

        #region Private-Members

        private CsvProcessorSettings _Settings = new CsvProcessorSettings();

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from CSV documents.
        /// </summary>
        public CsvProcessor(CsvProcessorSettings settings = null)
        {
            if (settings == null) settings = new CsvProcessorSettings();

            Header = "[Csv] ";

            _Settings = settings;
        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Dispose.
        /// </summary>
        /// <param name="disposing">Disposing.</param>
        protected new void Dispose(bool disposing)
        {
            if (!_Disposed)
            {
                if (disposing)
                {
                }

                base.Dispose(disposing);
                _Disposed = true;
            }
        }

        /// <summary>
        /// Dispose.
        /// </summary>
        public new void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public override IEnumerable<Atom> Extract(string filename)
        {
            if (String.IsNullOrEmpty(filename)) throw new ArgumentNullException(nameof(filename));
            return ProcessFile(filename);
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            byte[] data = File.ReadAllBytes(filename);
            if (data == null || data.Length == 0)
            {
                yield break;
            }

            CsvConfiguration config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = _Settings.ColumnDelimiter.ToString(),
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null,
                HasHeaderRecord = _Settings.HasHeaderRow
            };

            DataTable table = new DataTable();
            string[] headerNames = null;
            List<string[]> dataRows = new List<string[]>();

            using (MemoryStream ms = new MemoryStream(data))
            using (StreamReader reader = new StreamReader(ms))
            using (CsvParser parser = new CsvParser(reader, config))
            {
                int rowIndex = 0;

                while (parser.Read())
                {
                    if (parser.Record != null && parser.Record.Length > 0)
                    {
                        if (_Settings.HasHeaderRow && rowIndex == 0)
                        {
                            // Header row
                            headerNames = parser.Record;

                            // Create DataTable columns
                            foreach (string header in headerNames)
                            {
                                table.Columns.Add(header, typeof(string));
                            }
                        }
                        else
                        {
                            // Data row
                            dataRows.Add(parser.Record);
                        }

                        rowIndex++;
                    }
                }
            }

            // If no header row, create default column names
            if (!_Settings.HasHeaderRow && dataRows.Count > 0)
            {
                int columnCount = dataRows[0].Length;
                for (int i = 0; i < columnCount; i++)
                {
                    table.Columns.Add($"Column{i + 1}", typeof(string));
                }
            }

            // Add data rows to table
            foreach (string[] rowData in dataRows)
            {
                DataRow dataRow = table.NewRow();
                for (int i = 0; i < Math.Min(rowData.Length, table.Columns.Count); i++)
                {
                    dataRow[i] = rowData[i];
                }
                table.Rows.Add(dataRow);
            }

            // Generate atoms based on RowsPerAtom setting
            int atomCount = 0;

            if (_Settings.RowsPerAtom == 0)
            {
                // Entire table as one atom
                foreach (Atom atom in CreateTableAtom(table, atomCount))
                {
                    yield return atom;
                    atomCount++;
                }
            }
            else
            {
                // Split into multiple atoms
                int currentRow = 0;
                while (currentRow < table.Rows.Count)
                {
                    int rowsToTake = Math.Min(_Settings.RowsPerAtom, table.Rows.Count - currentRow);
                    DataTable subTable = CreateSubTable(table, currentRow, rowsToTake);

                    foreach (Atom atom in CreateTableAtom(subTable, atomCount))
                    {
                        yield return atom;
                        atomCount++;
                    }

                    currentRow += rowsToTake;
                }
            }
        }

        private DataTable CreateSubTable(DataTable sourceTable, int startRow, int rowCount)
        {
            DataTable subTable = sourceTable.Clone();

            for (int i = 0; i < rowCount && (startRow + i) < sourceTable.Rows.Count; i++)
            {
                subTable.ImportRow(sourceTable.Rows[startRow + i]);
            }

            return subTable;
        }

        private IEnumerable<Atom> CreateTableAtom(DataTable table, int position)
        {
            if (table == null || table.Rows.Count == 0)
            {
                yield break;
            }

            // Convert DataTable to SerializableDataTable
            SerializableDataTable serializableTable = new SerializableDataTable();

            // Add columns
            foreach (DataColumn col in table.Columns)
            {
                serializableTable.Columns.Add(new SerializableColumn
                {
                    Name = col.ColumnName,
                    Type = ColumnValueType.String
                });
            }

            // Add rows
            foreach (DataRow row in table.Rows)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();
                foreach (DataColumn col in table.Columns)
                {
                    rowData[col.ColumnName] = row[col]?.ToString() ?? "";
                }
                serializableTable.Rows.Add(rowData);
            }

            // Calculate hash based on table data
            byte[] tableBytes = HashHelper.MD5Hash(table);

            Atom atom = new Atom
            {
                GUID = Guid.NewGuid(),
                ParentGUID = null,
                Position = position,
                Type = AtomTypeEnum.Table,
                Text = null,
                Length = tableBytes.Length,
                Rows = table.Rows.Count,
                Columns = table.Columns.Count,
                Table = serializableTable,
                MD5Hash = HashHelper.MD5Hash(table),
                SHA1Hash = HashHelper.SHA1Hash(table),
                SHA256Hash = HashHelper.SHA256Hash(table)
            };

            yield return atom;
        }

        private IEnumerable<Atom> ChunkAtom(Atom atom, ChunkingSettings settings)
        {
            if (atom == null || String.IsNullOrEmpty(atom.Text)) yield break;
            if (settings == null || !settings.Enable) yield break;

            string text = atom.Text;
            int maxLength = settings.MaximumLength;
            int shiftSize = settings.ShiftSize;

            if (text.Length <= maxLength)
            {
                yield break;
            }

            int chunkPosition = 0;
            int currentPosition = 0;

            while (currentPosition < text.Length)
            {
                int chunkLength = Math.Min(maxLength, text.Length - currentPosition);
                string chunkText = text.Substring(currentPosition, chunkLength);

                byte[] chunkBytes = Encoding.UTF8.GetBytes(chunkText);

                Atom chunk = new Atom
                {
                    GUID = Guid.NewGuid(),
                    ParentGUID = atom.GUID,
                    Position = chunkPosition,
                    Type = AtomTypeEnum.Text,
                    Text = chunkText,
                    Length = chunkBytes.Length,
                    MD5Hash = HashHelper.MD5Hash(chunkBytes),
                    SHA1Hash = HashHelper.SHA1Hash(chunkBytes),
                    SHA256Hash = HashHelper.SHA256Hash(chunkBytes)
                };

                yield return chunk;

                chunkPosition++;
                currentPosition += shiftSize;

                if (currentPosition >= text.Length)
                {
                    break;
                }
            }
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
