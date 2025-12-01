namespace DocumentAtom.McpServer.Registrations
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.Sdk;
    using SerializationHelper;
    using Voltaic;

    /// <summary>
    /// Registration methods for Rich Text operations.
    /// </summary>
    public static class RichTextRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers Rich Text tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "richtext/process",
                "Process a Rich Text file and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded Rich Text file data" },
                        extractOcr = new { type = "boolean", description = "Whether to extract text from images using OCR (default: false)" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("Rich Text data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("Rich Text data cannot be null");
                    byte[] richTextData = Convert.FromBase64String(base64Data);
                    bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                    var atoms = sdk.Atom.ProcessRtf(richTextData, extractOcr).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers Rich Text methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("richtext/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Rich Text data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Rich Text data cannot be null");
                byte[] richTextData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                var atoms = sdk.Atom.ProcessRtf(richTextData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers Rich Text methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("richtext/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Rich Text data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Rich Text data cannot be null");
                byte[] richTextData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                var atoms = sdk.Atom.ProcessRtf(richTextData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}

