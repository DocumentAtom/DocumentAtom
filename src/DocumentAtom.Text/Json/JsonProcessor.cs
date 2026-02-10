namespace DocumentAtom.Text.Json
{
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using SerializableDataTables;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Text.Json;

    /// <summary>
    /// Create atoms from JSON documents.
    /// </summary>
    public class JsonProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        #region Public-Members

        /// <summary>
        /// JSON processor settings.
        /// </summary>
        public new JsonProcessorSettings Settings
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

        private JsonProcessorSettings _Settings = new JsonProcessorSettings();

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from JSON documents.
        /// </summary>
        public JsonProcessor(JsonProcessorSettings settings = null)
        {
            if (settings == null) settings = new JsonProcessorSettings();

            Header = "[Json] ";

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

            List<Atom> flatAtoms = ProcessFile(filename).ToList();

            if (_Settings.BuildHierarchy)
            {
                return BuildHierarchy(flatAtoms);
            }
            else
            {
                // When hierarchy is disabled, all atoms are root-level
                foreach (Atom atom in flatAtoms)
                {
                    atom.ParentGUID = null;
                }
                return flatAtoms;
            }
        }

        #endregion

        #region Private-Methods

        private IEnumerable<Atom> BuildHierarchy(List<Atom> flatAtoms)
        {
            if (flatAtoms == null || flatAtoms.Count == 0)
            {
                return Enumerable.Empty<Atom>();
            }

            // Build lookup dictionary for atoms by GUID
            Dictionary<Guid, Atom> atomLookup = new Dictionary<Guid, Atom>();
            foreach (Atom atom in flatAtoms)
            {
                atomLookup[atom.GUID] = atom;
            }

            // Root-level atoms (top of tree)
            List<Atom> rootAtoms = new List<Atom>();

            // Build parent-child relationships
            foreach (Atom atom in flatAtoms)
            {
                if (atom.ParentGUID == null)
                {
                    // This is a root-level atom
                    rootAtoms.Add(atom);
                }
                else if (atomLookup.ContainsKey(atom.ParentGUID.Value))
                {
                    // Add this atom as a Quark (child) of the parent
                    Atom parent = atomLookup[atom.ParentGUID.Value];
                    if (parent.Quarks == null)
                    {
                        parent.Quarks = new List<Atom>();
                    }
                    parent.Quarks.Add(atom);
                }
            }

            return rootAtoms;
        }

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            string contents = File.ReadAllText(filename);
            if (String.IsNullOrEmpty(contents))
            {
                yield break;
            }

            JsonDocument doc = JsonDocument.Parse(contents, _Settings.JsonOptions);
            if (doc == null)
            {
                yield break;
            }

            AtomCounter counter = new AtomCounter();
            foreach (Atom atom in ProcessJsonElement(doc.RootElement, null, counter, 0))
            {
                yield return atom;
            }
        }

        private IEnumerable<Atom> ProcessJsonElement(JsonElement element, Guid? parentGuid, AtomCounter counter, int depth)
        {
            if (depth > _Settings.MaxDepth)
            {
                yield break;
            }

            Atom currentAtom = new Atom
            {
                GUID = Guid.NewGuid(),
                ParentGUID = parentGuid,
                Position = counter.Value,
                Type = AtomTypeEnum.Table
            };

            counter.Increment();

            if (element.ValueKind == JsonValueKind.Object)
            {
                #region Object

                // Create table with 1 row, columns = keys
                currentAtom.Table = CreateObjectTable(element, currentAtom.GUID, counter, depth);
                currentAtom.Rows = 1;
                currentAtom.Columns = currentAtom.Table.Columns.Count;

                // Process nested objects/arrays as child atoms
                foreach (JsonProperty prop in element.EnumerateObject())
                {
                    if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                    {
                        foreach (Atom childAtom in ProcessJsonElement(prop.Value, currentAtom.GUID, counter, depth + 1))
                        {
                            yield return childAtom;
                        }
                    }
                }

                #endregion
            }
            else if (element.ValueKind == JsonValueKind.Array)
            {
                #region Array

                // Create table with N rows
                currentAtom.Table = CreateArrayTable(element, currentAtom.GUID, counter, depth);
                currentAtom.Rows = currentAtom.Table.Rows.Count;
                currentAtom.Columns = currentAtom.Table.Columns.Count;

                // Process nested objects/arrays in array elements as child atoms
                // ONLY if they contain nested structures (not just primitives)
                foreach (JsonElement arrayItem in element.EnumerateArray())
                {
                    if (arrayItem.ValueKind == JsonValueKind.Array)
                    {
                        // Always process arrays as child atoms
                        foreach (Atom childAtom in ProcessJsonElement(arrayItem, currentAtom.GUID, counter, depth + 1))
                        {
                            yield return childAtom;
                        }
                    }
                    else if (arrayItem.ValueKind == JsonValueKind.Object && HasNestedStructures(arrayItem))
                    {
                        // Only process objects that have nested structures
                        foreach (Atom childAtom in ProcessJsonElement(arrayItem, currentAtom.GUID, counter, depth + 1))
                        {
                            yield return childAtom;
                        }
                    }
                }

                #endregion
            }

            // Calculate hash based on table data (serialize table to JSON for hashing)
            string tableJson = JsonSerializer.Serialize(currentAtom.Table);
            byte[] tableBytes = Encoding.UTF8.GetBytes(tableJson);
            currentAtom.Length = tableBytes.Length;
            currentAtom.MD5Hash = HashHelper.MD5Hash(tableBytes);
            currentAtom.SHA1Hash = HashHelper.SHA1Hash(tableBytes);
            currentAtom.SHA256Hash = HashHelper.SHA256Hash(tableBytes);

            yield return currentAtom;
        }

        private bool HasNestedStructures(JsonElement element)
        {
            if (element.ValueKind != JsonValueKind.Object) return false;

            foreach (JsonProperty prop in element.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                {
                    return true;
                }
            }

            return false;
        }

        private SerializableDataTable CreateObjectTable(JsonElement obj, Guid atomGuid, AtomCounter counter, int depth)
        {
            SerializableDataTable table = new SerializableDataTable();
            Dictionary<string, object> rowData = new Dictionary<string, object>();

            List<JsonProperty> properties = obj.EnumerateObject().ToList();

            if (properties.Count == 0)
            {
                // Empty object - no columns, one row with NULL
                // Actually, per spec: empty object creates table with no columns and one row of NULL
                // But SerializableDataTable might not support zero columns, so let's test
                table.Rows.Add(rowData);
                return table;
            }

            foreach (JsonProperty prop in properties)
            {
                // Add column
                table.Columns.Add(new SerializableColumn
                {
                    Name = prop.Name,
                    Type = ColumnValueType.String
                });

                // Add value to row
                if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                {
                    // Nested structure - store minified JSON
                    rowData[prop.Name] = JsonSerializer.Serialize(prop.Value, new JsonSerializerOptions { WriteIndented = false });
                }
                else if (prop.Value.ValueKind == JsonValueKind.Null)
                {
                    rowData[prop.Name] = null;
                }
                else
                {
                    rowData[prop.Name] = prop.Value.ToString();
                }
            }

            // Add the single row
            table.Rows.Add(rowData);

            return table;
        }

        private SerializableDataTable CreateArrayTable(JsonElement array, Guid atomGuid, AtomCounter counter, int depth)
        {
            SerializableDataTable table = new SerializableDataTable();
            List<JsonElement> arrayItems = array.EnumerateArray().ToList();

            if (arrayItems.Count == 0)
            {
                // Empty array - create table with "undefined" column, no rows
                table.Columns.Add(new SerializableColumn
                {
                    Name = "undefined",
                    Type = ColumnValueType.String
                });
                return table;
            }

            // Determine all column names
            HashSet<string> allColumns = new HashSet<string>();
            bool hasPrimitives = false;

            foreach (JsonElement item in arrayItems)
            {
                if (item.ValueKind == JsonValueKind.Object)
                {
                    foreach (JsonProperty prop in item.EnumerateObject())
                    {
                        allColumns.Add(prop.Name);
                    }
                }
                else
                {
                    hasPrimitives = true;
                }
            }

            // Add "undefined" column if there are primitives or it's pure primitives
            if (hasPrimitives || allColumns.Count == 0)
            {
                table.Columns.Add(new SerializableColumn
                {
                    Name = "undefined",
                    Type = ColumnValueType.String
                });
            }

            // Add columns for object keys
            foreach (string colName in allColumns.OrderBy(c => c))
            {
                table.Columns.Add(new SerializableColumn
                {
                    Name = colName,
                    Type = ColumnValueType.String
                });
            }

            // Add rows
            foreach (JsonElement item in arrayItems)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();

                if (item.ValueKind == JsonValueKind.Object)
                {
                    // Object row
                    if (hasPrimitives || allColumns.Count == 0)
                    {
                        rowData["undefined"] = null;
                    }

                    foreach (JsonProperty prop in item.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Object || prop.Value.ValueKind == JsonValueKind.Array)
                        {
                            // Nested structure - store minified JSON
                            rowData[prop.Name] = JsonSerializer.Serialize(prop.Value, new JsonSerializerOptions { WriteIndented = false });
                        }
                        else if (prop.Value.ValueKind == JsonValueKind.Null)
                        {
                            rowData[prop.Name] = null;
                        }
                        else
                        {
                            rowData[prop.Name] = prop.Value.ToString();
                        }
                    }

                    // Fill missing columns with null
                    foreach (string colName in allColumns)
                    {
                        if (!rowData.ContainsKey(colName))
                        {
                            rowData[colName] = null;
                        }
                    }
                }
                else if (item.ValueKind == JsonValueKind.Array)
                {
                    // Nested array - store minified JSON in "undefined" column
                    rowData["undefined"] = JsonSerializer.Serialize(item, new JsonSerializerOptions { WriteIndented = false });

                    // Fill object columns with null
                    foreach (string colName in allColumns)
                    {
                        rowData[colName] = null;
                    }
                }
                else
                {
                    // Primitive value
                    if (item.ValueKind == JsonValueKind.Null)
                    {
                        rowData["undefined"] = null;
                    }
                    else
                    {
                        rowData["undefined"] = item.ToString();
                    }

                    // Fill object columns with null
                    foreach (string colName in allColumns)
                    {
                        rowData[colName] = null;
                    }
                }

                table.Rows.Add(rowData);
            }

            return table;
        }

        #endregion

        #region Private-Classes

        private class AtomCounter
        {
            public int Value { get; private set; } = 0;

            public void Increment()
            {
                Value++;
            }
        }

        #endregion

#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
