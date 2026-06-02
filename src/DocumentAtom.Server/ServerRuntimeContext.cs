namespace DocumentAtom.Server
{
    using System.Text.Json;
    using SerializationHelper;
    using SyslogLogging;

    internal sealed class ServerRuntimeContext
    {
        public ServerRuntimeContext(
            string header,
            Serializer serializer,
            ServerSettings settings,
            LoggingModule logging,
            JsonSerializerOptions jsonOptions)
        {
            Header = header;
            Serializer = serializer;
            Settings = settings;
            Logging = logging;
            JsonOptions = jsonOptions;
        }

        public string Header { get; }

        public Serializer Serializer { get; }

        public ServerSettings Settings { get; }

        public LoggingModule Logging { get; }

        public JsonSerializerOptions JsonOptions { get; }
    }
}
