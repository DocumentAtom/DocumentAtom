namespace DocumentAtom.McpServer.Registrations
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.Sdk;
    using SerializationHelper;
    using Voltaic;

    /// <summary>
    /// Registration methods for XML operations.
    /// </summary>
    public static class XmlRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers XML tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "xml/process",
                "Process an XML document and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded XML document data" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("XML document data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("XML document data cannot be null");
                    byte[] xmlData = Convert.FromBase64String(base64Data);

                    var atoms = sdk.Atom.ProcessXml(xmlData).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers XML methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("xml/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("XML document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("XML document data cannot be null");
                byte[] xmlData = Convert.FromBase64String(base64Data);

                var atoms = sdk.Atom.ProcessXml(xmlData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers XML methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("xml/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("XML document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("XML document data cannot be null");
                byte[] xmlData = Convert.FromBase64String(base64Data);

                var atoms = sdk.Atom.ProcessXml(xmlData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}
