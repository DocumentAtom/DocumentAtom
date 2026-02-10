namespace DocumentAtom.McpServer.Registrations
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.Core.Atoms;
    using DocumentAtom.Sdk;
    using SerializationHelper;
    using Voltaic;

    /// <summary>
    /// Registration methods for JSON operations.
    /// </summary>
    public static class JsonRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers JSON tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "json/process",
                "Process a JSON file and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded JSON file data" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("JSON data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("JSON data cannot be null");
                    byte[] jsonData = Convert.FromBase64String(base64Data);

                    List<Atom> atoms = sdk.Atom.ProcessJson(jsonData).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers JSON methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("json/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("JSON data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("JSON data cannot be null");
                byte[] jsonData = Convert.FromBase64String(base64Data);

                List<Atom> atoms =sdk.Atom.ProcessJson(jsonData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers JSON methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("json/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("JSON data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("JSON data cannot be null");
                byte[] jsonData = Convert.FromBase64String(base64Data);

                List<Atom> atoms =sdk.Atom.ProcessJson(jsonData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}
