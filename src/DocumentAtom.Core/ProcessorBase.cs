namespace DocumentAtom.Core
{
    using System.Reflection.Metadata.Ecma335;
    using System.Runtime;
    using DocumentAtom.Core.Enums;
    using SerializationHelper;

    /// <summary>
    /// Processor base class.  Do not use directly.
    /// </summary>
    public class ProcessorBase
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

        #endregion

        #region Private-Methods

        #endregion
    }
}
