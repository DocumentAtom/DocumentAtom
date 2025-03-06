namespace DocumentAtom.Core
{
    using System.IO;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Enums;
    using SerializationHelper;

    /// <summary>
    /// Processor base class.  Do not use directly.
    /// </summary>
    public abstract class ProcessorBase : IDisposable
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
        public Serializer Serializer
        {
            get
            {
                return _Serializer;
            }
            set
            {
                if (value == null) throw new ArgumentNullException(nameof(Serializer));
                _Serializer = value;
            }
        }

        #endregion

        #region Private-Members

        private Serializer _Serializer = new Serializer();
        private bool _Disposed = false;

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
                    Header = null;

                    _Serializer = null;
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
                Directory.Delete(directory, true);
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
