namespace DocumentAtom.Text.Markdown
{
    using DocumentAtom.Core;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using DocumentAtom.Core.Helpers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// Create atoms from markdown documents.
    /// </summary>
    public class MarkdownProcessor : ProcessorBase, IDisposable
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.

        #region Public-Members

        /// <summary>
        /// Markdown processor settings.
        /// </summary>
        public new MarkdownProcessorSettings Settings
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

        private MarkdownProcessorSettings _Settings = new MarkdownProcessorSettings();

        private bool _Disposed = false;

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Create atoms from markdown documents.
        /// </summary>
        public MarkdownProcessor(MarkdownProcessorSettings settings = null)
        {
            if (settings == null) settings = new MarkdownProcessorSettings();

            Header = "[Markdown] ";

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

            // Track the current header at each level (1-6)
            Dictionary<int, Atom> currentHeaders = new Dictionary<int, Atom>();

            // Root-level atoms (top of tree)
            List<Atom> rootAtoms = new List<Atom>();

            foreach (Atom atom in flatAtoms)
            {
                if (atom.HeaderLevel != null && atom.HeaderLevel.Value > 0)
                {
                    // This is a header atom
                    int level = atom.HeaderLevel.Value;

                    // Find parent header (nearest header with level < current level)
                    Atom parent = FindParentHeader(currentHeaders, level);

                    if (parent != null)
                    {
                        // Add this header as a Quark (child) of the parent
                        if (parent.Quarks == null)
                        {
                            parent.Quarks = new List<Atom>();
                        }
                        atom.ParentGUID = parent.GUID;
                        parent.Quarks.Add(atom);
                    }
                    else
                    {
                        // No parent found - this is a root-level header
                        atom.ParentGUID = null;
                        rootAtoms.Add(atom);
                    }

                    // Update current header tracking
                    currentHeaders[level] = atom;

                    // Clear tracking for deeper levels (they're now out of scope)
                    ClearDeeperLevels(currentHeaders, level);
                }
                else
                {
                    // This is a non-header atom (text, list, table, code, etc.)
                    // Add it to the deepest current header, or to root if no headers exist
                    Atom parent = FindDeepestHeader(currentHeaders);

                    if (parent != null)
                    {
                        // Add as Quark to deepest header
                        if (parent.Quarks == null)
                        {
                            parent.Quarks = new List<Atom>();
                        }
                        atom.ParentGUID = parent.GUID;
                        parent.Quarks.Add(atom);
                    }
                    else
                    {
                        // No headers yet - add to root
                        atom.ParentGUID = null;
                        rootAtoms.Add(atom);
                    }
                }
            }

            return rootAtoms;
        }

        /// <summary>
        /// Find the parent header for a given header level.
        /// Returns the nearest header with level less than the specified level.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <param name="level">Header level to find parent for.</param>
        /// <returns>Parent header atom, or null if no parent exists.</returns>
        private Atom FindParentHeader(Dictionary<int, Atom> currentHeaders, int level)
        {
            // Search backwards from level-1 down to 1
            for (int i = level - 1; i >= 1; i--)
            {
                if (currentHeaders.ContainsKey(i))
                {
                    return currentHeaders[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Find the deepest (highest level number) current header.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <returns>Deepest header atom, or null if no headers exist.</returns>
        private Atom FindDeepestHeader(Dictionary<int, Atom> currentHeaders)
        {
            if (currentHeaders.Count == 0)
            {
                return null;
            }

            int deepestLevel = currentHeaders.Keys.Max();
            return currentHeaders[deepestLevel];
        }

        /// <summary>
        /// Clear tracking for header levels deeper than the specified level.
        /// </summary>
        /// <param name="currentHeaders">Dictionary of current headers by level.</param>
        /// <param name="level">Current level.</param>
        private void ClearDeeperLevels(Dictionary<int, Atom> currentHeaders, int level)
        {
            // Remove all levels > current level (max markdown level is 6)
            for (int i = level + 1; i <= 6; i++)
            {
                currentHeaders.Remove(i);
            }
        }

        private IEnumerable<Atom> ProcessFile(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(fs))
                {
                    StringBuilder currentSegment = new StringBuilder();
                    char[] buffer = new char[_Settings.StreamBufferSize];
                    int charsRead;

                    int atomCount = 0;

                    while ((charsRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        string chunk = new string(buffer, 0, charsRead);
                        currentSegment.Append(chunk);

                        int startIndex = 0;
                        while (startIndex < currentSegment.Length)
                        {
                            int nextDelimiterIndex = -1;
                            string matchedDelimiter = null;

                            foreach (string delimiter in Settings.Delimiters)
                            {
                                int index = currentSegment.ToString().IndexOf(delimiter, startIndex);
                                if (index != -1 && (nextDelimiterIndex == -1 || index < nextDelimiterIndex))
                                {
                                    nextDelimiterIndex = index;
                                    matchedDelimiter = delimiter;
                                }
                            }

                            if (nextDelimiterIndex != -1)
                            {
                                string segment = currentSegment.ToString(startIndex, nextDelimiterIndex - startIndex).Trim();
                                if (Settings.RemoveBinaryFromText) segment = StringHelper.RemoveBinaryData(segment);

                                if (!string.IsNullOrEmpty(segment))
                                {
                                    yield return Atom.FromMarkdownContent(segment, atomCount, _Settings.Chunking);
                                    atomCount++;
                                }

                                startIndex = nextDelimiterIndex + matchedDelimiter.Length;
                            }
                            else
                            {
                                // No delimiter found in current buffer
                                if (startIndex > 0)
                                {
                                    // Remove processed text from StringBuilder
                                    currentSegment.Remove(0, startIndex);
                                }

                                break;
                            }
                        }
                    }

                    // Handle any remaining text
                    string finalSegment = currentSegment.ToString().Trim();
                    if (_Settings.RemoveBinaryFromText) finalSegment = StringHelper.RemoveBinaryData(finalSegment);

                    if (!String.IsNullOrEmpty(finalSegment))
                    {
                        yield return Atom.FromMarkdownContent(finalSegment, atomCount, _Settings.Chunking);
                        atomCount++;
                    }
                }
            }
        }

        #endregion

#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
    }
}
