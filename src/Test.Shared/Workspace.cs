namespace DocumentAtom.Testing.Shared
{
    using System;
    using System.IO;
    using System.Text;

    /// <summary>
    /// Provides a per-process temporary workspace directory for tests that need to write
    /// sample files to disk (the document processors read from file paths). Files are created
    /// on demand; the whole workspace is deleted via <see cref="Cleanup"/> in suite teardown.
    /// </summary>
    public static class Workspace
    {
        private static readonly object _Lock = new object();
        private static string? _Root;

        /// <summary>
        /// Gets the root workspace directory, creating it on first access.
        /// </summary>
        public static string Root
        {
            get
            {
                lock (_Lock)
                {
                    if (_Root == null)
                    {
                        _Root = Path.Combine(Path.GetTempPath(), "DocumentAtomTests", Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(_Root);
                    }

                    return _Root;
                }
            }
        }

        /// <summary>
        /// Writes UTF-8 text to a uniquely-named file with the supplied extension and returns its full path.
        /// </summary>
        /// <param name="extension">File extension without a leading dot (for example "txt").</param>
        /// <param name="content">The text content to write.</param>
        /// <returns>The full path to the written file.</returns>
        public static string WriteText(string extension, string content)
        {
            string path = Path.Combine(Root, Guid.NewGuid().ToString("N") + "." + extension);
            File.WriteAllText(path, content, new UTF8Encoding(false));
            return path;
        }

        /// <summary>
        /// Writes raw bytes to a uniquely-named file with the supplied extension and returns its full path.
        /// </summary>
        /// <param name="extension">File extension without a leading dot.</param>
        /// <param name="content">The bytes to write.</param>
        /// <returns>The full path to the written file.</returns>
        public static string WriteBytes(string extension, byte[] content)
        {
            string path = Path.Combine(Root, Guid.NewGuid().ToString("N") + "." + extension);
            File.WriteAllBytes(path, content);
            return path;
        }

        /// <summary>
        /// Returns a path inside the workspace for a file that does not exist (used for negative tests).
        /// </summary>
        /// <param name="extension">File extension without a leading dot.</param>
        /// <returns>A non-existent path inside the workspace.</returns>
        public static string NonExistentPath(string extension)
        {
            return Path.Combine(Root, "missing-" + Guid.NewGuid().ToString("N") + "." + extension);
        }

        /// <summary>
        /// Deletes the workspace directory and all of its contents. Safe to call multiple times.
        /// </summary>
        public static void Cleanup()
        {
            lock (_Lock)
            {
                try
                {
                    if (_Root != null && Directory.Exists(_Root)) Directory.Delete(_Root, true);
                }
                catch (IOException)
                {
                    // Best-effort cleanup; ignore files still locked by the OS.
                }
                catch (UnauthorizedAccessException)
                {
                    // Best-effort cleanup.
                }
            }
        }
    }
}
