namespace DocumentAtom.Core
{
    using System.Diagnostics;
    using System.IO;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Core.Diagnostics;
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

            long startTicks = Stopwatch.GetTimestamp();
            long atomCount = 0;
            string outcome = "ok";
            string processorName = GetType().Name;

            Activity? activity = DocumentAtomDiagnostics.CoreActivitySource.StartActivity(
                "documentatom.processor.extract",
                ActivityKind.Internal);

            activity?.SetTag("documentatom.processor", processorName);
            activity?.SetTag("documentatom.input.kind", "bytes");
            activity?.SetTag("documentatom.input.size", bytes.LongLength);

            Guid guid = Guid.NewGuid();
            string directory = Path.GetFullPath("./" + guid.ToString() + "/");
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            string filename = Guid.NewGuid().ToString();
            string path = directory + filename;

            try
            {
                File.WriteAllBytes(path, bytes);
            }
            catch (Exception e)
            {
                outcome = "error";
                DocumentAtomDiagnostics.RecordException(activity, e);
                DocumentAtomDiagnostics.RecordProcessorExtraction(
                    processorName,
                    "bytes",
                    outcome,
                    atomCount,
                    DocumentAtomDiagnostics.GetElapsedSeconds(startTicks));
                activity?.Dispose();

                Helpers.FileHelper.RecursiveDelete(new DirectoryInfo(directory), true);
                Directory.Delete(directory, true);
                throw;
            }

            try
            {
                using (IEnumerator<Atom> enumerator = Extract(path).GetEnumerator())
                {
                    while (true)
                    {
                        Atom atom;

                        try
                        {
                            if (!enumerator.MoveNext()) break;
                            atom = enumerator.Current;
                        }
                        catch (Exception e)
                        {
                            outcome = "error";
                            DocumentAtomDiagnostics.RecordException(activity, e);
                            throw;
                        }

                        atomCount++;
                        yield return atom;
                    }
                }

                activity?.SetStatus(ActivityStatusCode.Ok);
            }
            finally
            {
                Helpers.FileHelper.RecursiveDelete(new DirectoryInfo(directory), true);
                Directory.Delete(directory, true);

                DocumentAtomDiagnostics.RecordProcessorExtraction(
                    processorName,
                    "bytes",
                    outcome,
                    atomCount,
                    DocumentAtomDiagnostics.GetElapsedSeconds(startTicks));

                activity?.Dispose();
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
