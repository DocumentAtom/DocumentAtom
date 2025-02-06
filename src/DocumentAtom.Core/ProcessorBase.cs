namespace DocumentAtom.Core
{
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using SerializationHelper;

    /// <summary>
    /// Processor base class.  Do not use directly.
    /// </summary>
    public abstract class ProcessorBase
    {
        #region Public-Members

        /// <summary>
        /// Settings.
        /// </summary>
        public ProcessorSettingsBase Settings { get; set; }

        /// <summary>
        /// Logger method.
        /// </summary>
        public Action<SeverityEnum, string> Logger { get; set; }

        /// <summary>
        /// Header to prepend to log messages.
        /// </summary>
        public string Header { get; set; } = "[ProcessorBase] ";

        /// <summary>
        /// Serializer.
        /// </summary>
        public Serializer Serializer { get; } = new Serializer();

        #endregion

        #region Private-Members

        #endregion

        #region Constructors-and-Factories

        /// <summary>
        /// Processor base class.  Do not use directly.
        /// </summary>
        public ProcessorBase()
        {

        }

        #endregion

        #region Public-Methods

        /// <summary>
        /// Emit a log message.
        /// </summary>
        /// <param name="sev">Severity.</param>
        /// <param name="msg">Message.</param>
        public void Log(SeverityEnum sev, string msg)
        {
            if (Logger == null || String.IsNullOrEmpty(msg)) return;
            Logger(sev, msg);
        }

        /// <summary>
        /// Extract atoms from a byte array.
        /// </summary>
        /// <param name="bytes">Byte array.</param>
        /// <returns>Atoms.</returns>
        public IEnumerable<Atom> Extract(byte[] bytes)
        {
            if (bytes == null || bytes.Length < 1) yield break;

            Guid guid = Guid.NewGuid();
            string directory = Path.GetFullPath("./" + guid.ToString() + "/");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string filename = Guid.NewGuid().ToString();

            try
            {
                File.WriteAllBytes(directory + filename, bytes);
                foreach (Atom atom in Extract(directory + filename))
                {
                    yield return atom;
                }
            }
            finally
            {
                Helpers.FileHelper.RecursiveDelete(new DirectoryInfo(directory), true);
            }
        }

        /// <summary>
        /// Extract atoms from a file.
        /// </summary>
        /// <param name="filename">Filename.</param>
        /// <returns>Atoms.</returns>
        public abstract IEnumerable<Atom> Extract(string filename);

        #endregion

        #region Private-Methods

        #endregion
    }
}
