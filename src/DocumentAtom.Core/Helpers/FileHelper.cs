namespace DocumentAtom.Core.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// File helpers.
    /// </summary>
    public static class FileHelper
    {
        #region Public-Methods

        /// <summary>
        /// Recursively delete a directory.
        /// </summary>
        /// <param name="baseDir">Base directory.</param>
        /// <param name="isRootDir">True to indicate the supplied directory is the root directory.</param>
        public static void RecursiveDelete(DirectoryInfo baseDir, bool isRootDir)
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

        #endregion
    }
}
