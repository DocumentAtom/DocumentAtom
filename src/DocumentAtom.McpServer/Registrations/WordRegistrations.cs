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
    /// Registration methods for Word operations.
    /// </summary>
    public static class WordRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers Word tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "word/process",
                "Process a Word document and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded Word document data" },
                        extractOcr = new { type = "boolean", description = "Whether to extract text from images using OCR (default: false)" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("Word document data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("Word document data cannot be null");
                    byte[] wordData = Convert.FromBase64String(base64Data);
                    bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                    List<Atom> atoms = sdk.Atom.ProcessWord(wordData, extractOcr).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers Word methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("word/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Word document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Word document data cannot be null");
                byte[] wordData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                List<Atom> atoms =sdk.Atom.ProcessWord(wordData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers Word methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("word/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Word document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Word document data cannot be null");
                byte[] wordData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                List<Atom> atoms =sdk.Atom.ProcessWord(wordData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}
