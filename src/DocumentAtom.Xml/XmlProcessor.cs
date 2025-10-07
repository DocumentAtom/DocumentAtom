namespace DocumentAtom.Xml
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
    using System.Xml.Linq;

    /// <summary>
    /// Create atoms from XML documents.
    /// </summary>
    public class XmlProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.

        #region Public-Members

        /// <summary>
        /// XML processor settings.
        /// </summary>
        public new XmlProcessorSettings Settings
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

        private XmlProcessorSettings _Settings = new XmlProcessorSettings();

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from XML documents.
        /// </summary>
        public XmlProcessor(XmlProcessorSettings settings = null)
        {
            if (settings == null) settings = new XmlProcessorSettings();

            Header = "[Xml] ";

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

        /// <summary>
        /// Build hierarchical structure from flat list of atoms.
        /// </summary>
        /// <param name="flatAtoms">Flat list of atoms.</param>
        /// <returns>Root-level atoms with hierarchical structure.</returns>
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

            XDocument doc = XDocument.Parse(contents);
            if (doc == null || doc.Root == null)
            {
                yield break;
            }

            AtomCounter counter = new AtomCounter();
            foreach (Atom atom in ProcessXmlElement(doc.Root, null, counter, 0))
            {
                yield return atom;
            }
        }

        private IEnumerable<Atom> ProcessXmlElement(XElement element, Guid? parentGuid, AtomCounter counter, int depth)
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

            if (IsKeyValuePair(element))
            {
                #region Leaf-Element

                // Leaf element - create table with element value + attributes
                currentAtom.Table = CreateLeafElementTable(element);
                currentAtom.Rows = 1;
                currentAtom.Columns = currentAtom.Table.Columns.Count;

                #endregion
            }
            else if (IsArray(element))
            {
                #region Array-Elements

                // Array of repeating elements - create table with rows
                currentAtom.Table = CreateArrayElementTable(element, currentAtom.GUID, counter, depth);
                currentAtom.Rows = currentAtom.Table.Rows.Count;
                currentAtom.Columns = currentAtom.Table.Columns.Count;

                // Process nested structures in array items as child atoms
                // ONLY if they contain nested structures (not just leaf elements)
                foreach (XElement arrayItem in element.Elements())
                {
                    if (!IsKeyValuePair(arrayItem) && HasNestedStructures(arrayItem))
                    {
                        foreach (Atom childAtom in ProcessXmlElement(arrayItem, currentAtom.GUID, counter, depth + 1))
                        {
                            yield return childAtom;
                        }
                    }
                }

                #endregion
            }
            else
            {
                #region Object-Elements

                // Object element - create table with child element names as columns
                currentAtom.Table = CreateObjectElementTable(element, currentAtom.GUID, counter, depth);
                currentAtom.Rows = 1;
                currentAtom.Columns = currentAtom.Table.Columns.Count;

                // Process nested structures as child atoms
                foreach (XElement child in element.Elements())
                {
                    if (!IsKeyValuePair(child))
                    {
                        foreach (Atom childAtom in ProcessXmlElement(child, currentAtom.GUID, counter, depth + 1))
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

        private bool IsKeyValuePair(XElement element)
        {
            return !element.HasElements;
        }

        private bool IsArray(XElement element)
        {
            List<XElement> childElements = element.Elements().ToList();
            return childElements.Count > 0 && childElements.All(e => e.Name == childElements[0].Name);
        }

        private bool HasNestedStructures(XElement element)
        {
            if (!element.HasElements) return false;

            foreach (XElement child in element.Elements())
            {
                if (child.HasElements)
                {
                    return true;
                }
            }

            return false;
        }

        private SerializableDataTable CreateLeafElementTable(XElement element)
        {
            SerializableDataTable table = new SerializableDataTable();
            Dictionary<string, object> rowData = new Dictionary<string, object>();

            string elementName = element.Name.LocalName;

            // Add column for element value
            table.Columns.Add(new SerializableColumn
            {
                Name = elementName,
                Type = ColumnValueType.String
            });

            string value = _Settings.PreserveWhitespace ? element.Value : element.Value.Trim();
            rowData[elementName] = String.IsNullOrEmpty(value) ? null : value;

            // Add columns for attributes (with .attr suffix)
            if (_Settings.IncludeAttributes && element.HasAttributes)
            {
                foreach (XAttribute attr in element.Attributes())
                {
                    string attrColumnName = $"{elementName}.{attr.Name.LocalName}";
                    table.Columns.Add(new SerializableColumn
                    {
                        Name = attrColumnName,
                        Type = ColumnValueType.String
                    });
                    rowData[attrColumnName] = attr.Value;
                }
            }

            table.Rows.Add(rowData);
            return table;
        }

        private SerializableDataTable CreateObjectElementTable(XElement element, Guid atomGuid, AtomCounter counter, int depth)
        {
            SerializableDataTable table = new SerializableDataTable();
            Dictionary<string, object> rowData = new Dictionary<string, object>();

            // Add columns for parent element attributes first
            if (_Settings.IncludeAttributes && element.HasAttributes)
            {
                foreach (XAttribute attr in element.Attributes())
                {
                    string attrColumnName = $"{element.Name.LocalName}.{attr.Name.LocalName}";
                    table.Columns.Add(new SerializableColumn
                    {
                        Name = attrColumnName,
                        Type = ColumnValueType.String
                    });
                    rowData[attrColumnName] = attr.Value;
                }
            }

            // Add columns for child elements
            foreach (XElement child in element.Elements())
            {
                string childName = child.Name.LocalName;

                // Add column for child element
                if (!table.Columns.Any(c => c.Name == childName))
                {
                    table.Columns.Add(new SerializableColumn
                    {
                        Name = childName,
                        Type = ColumnValueType.String
                    });
                }

                // Add value
                if (IsKeyValuePair(child))
                {
                    // Leaf element - just the value
                    string value = _Settings.PreserveWhitespace ? child.Value : child.Value.Trim();
                    rowData[childName] = String.IsNullOrEmpty(value) ? null : value;
                }
                else
                {
                    // Nested structure - store minified XML
                    rowData[childName] = child.ToString(SaveOptions.DisableFormatting);
                }

                // Add columns for child element attributes
                if (_Settings.IncludeAttributes && child.HasAttributes)
                {
                    foreach (XAttribute attr in child.Attributes())
                    {
                        string attrColumnName = $"{childName}.{attr.Name.LocalName}";
                        if (!table.Columns.Any(c => c.Name == attrColumnName))
                        {
                            table.Columns.Add(new SerializableColumn
                            {
                                Name = attrColumnName,
                                Type = ColumnValueType.String
                            });
                        }
                        rowData[attrColumnName] = attr.Value;
                    }
                }
            }

            table.Rows.Add(rowData);
            return table;
        }

        private SerializableDataTable CreateArrayElementTable(XElement element, Guid atomGuid, AtomCounter counter, int depth)
        {
            SerializableDataTable table = new SerializableDataTable();
            List<XElement> arrayItems = element.Elements().ToList();

            if (arrayItems.Count == 0)
            {
                return table;
            }

            // Determine all column names from all array items
            HashSet<string> allColumns = new HashSet<string>();

            // Get element name (since all items have same name in an array)
            string itemName = arrayItems[0].Name.LocalName;

            foreach (XElement item in arrayItems)
            {
                if (IsKeyValuePair(item))
                {
                    // Leaf element
                    allColumns.Add(itemName);

                    // Add attribute columns
                    if (_Settings.IncludeAttributes && item.HasAttributes)
                    {
                        foreach (XAttribute attr in item.Attributes())
                        {
                            allColumns.Add($"{itemName}.{attr.Name.LocalName}");
                        }
                    }
                }
                else
                {
                    // Object element - get all child element names
                    foreach (XElement child in item.Elements())
                    {
                        allColumns.Add(child.Name.LocalName);

                        // Add attribute columns for child
                        if (_Settings.IncludeAttributes && child.HasAttributes)
                        {
                            foreach (XAttribute attr in child.Attributes())
                            {
                                allColumns.Add($"{child.Name.LocalName}.{attr.Name.LocalName}");
                            }
                        }
                    }

                    // Add attribute columns for item itself
                    if (_Settings.IncludeAttributes && item.HasAttributes)
                    {
                        foreach (XAttribute attr in item.Attributes())
                        {
                            allColumns.Add($"{itemName}.{attr.Name.LocalName}");
                        }
                    }
                }
            }

            // Add all columns
            foreach (string colName in allColumns.OrderBy(c => c))
            {
                table.Columns.Add(new SerializableColumn
                {
                    Name = colName,
                    Type = ColumnValueType.String
                });
            }

            // Add rows
            foreach (XElement item in arrayItems)
            {
                Dictionary<string, object> rowData = new Dictionary<string, object>();

                if (IsKeyValuePair(item))
                {
                    // Leaf element
                    string value = _Settings.PreserveWhitespace ? item.Value : item.Value.Trim();
                    rowData[itemName] = String.IsNullOrEmpty(value) ? null : value;

                    // Add attributes
                    if (_Settings.IncludeAttributes && item.HasAttributes)
                    {
                        foreach (XAttribute attr in item.Attributes())
                        {
                            rowData[$"{itemName}.{attr.Name.LocalName}"] = attr.Value;
                        }
                    }
                }
                else
                {
                    // Object element
                    foreach (XElement child in item.Elements())
                    {
                        string childName = child.Name.LocalName;

                        if (IsKeyValuePair(child))
                        {
                            string value = _Settings.PreserveWhitespace ? child.Value : child.Value.Trim();
                            rowData[childName] = String.IsNullOrEmpty(value) ? null : value;
                        }
                        else
                        {
                            // Nested structure - store minified XML
                            rowData[childName] = child.ToString(SaveOptions.DisableFormatting);
                        }

                        // Add attributes
                        if (_Settings.IncludeAttributes && child.HasAttributes)
                        {
                            foreach (XAttribute attr in child.Attributes())
                            {
                                rowData[$"{childName}.{attr.Name.LocalName}"] = attr.Value;
                            }
                        }
                    }

                    // Add item-level attributes
                    if (_Settings.IncludeAttributes && item.HasAttributes)
                    {
                        foreach (XAttribute attr in item.Attributes())
                        {
                            rowData[$"{itemName}.{attr.Name.LocalName}"] = attr.Value;
                        }
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

#pragma warning restore CS8601 // Possible null reference assignment.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
