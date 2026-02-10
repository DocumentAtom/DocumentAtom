namespace DocumentAtom.McpServer.Registrations
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.Sdk;
    using DocumentAtom.Core.TypeDetection;
    using SerializationHelper;
    using Voltaic;

    /// <summary>
    /// Registration methods for TypeDetection operations.
    /// </summary>
    public static class TypeDetectionRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers TypeDetection tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "typedetection/detect",
                "Detect the type of a document based on its content",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded document data" },
                        contentType = new { type = "string", description = "Optional content type hint" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("Document data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("Document data cannot be null");
                    byte[] documentData = Convert.FromBase64String(base64Data);

                    string? contentType = null;
                    if (args.Value.TryGetProperty("contentType", out JsonElement contentTypeProp))
                    {
                        contentType = contentTypeProp.GetString();
                    }

                    TypeResult result = sdk.TypeDetection.DetectType(documentData, contentType).GetAwaiter().GetResult();
                    return serializer.SerializeJson(result, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers TypeDetection methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("typedetection/detect", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Document data cannot be null");
                byte[] documentData = Convert.FromBase64String(base64Data);

                string? contentType = null;
                if (args.Value.TryGetProperty("contentType", out JsonElement contentTypeProp))
                {
                    contentType = contentTypeProp.GetString();
                }

                TypeResult result =sdk.TypeDetection.DetectType(documentData, contentType).GetAwaiter().GetResult();
                return serializer.SerializeJson(result, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers TypeDetection methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("typedetection/detect", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Document data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Document data cannot be null");
                byte[] documentData = Convert.FromBase64String(base64Data);

                string? contentType = null;
                if (args.Value.TryGetProperty("contentType", out JsonElement contentTypeProp))
                {
                    contentType = contentTypeProp.GetString();
                }

                TypeResult result =sdk.TypeDetection.DetectType(documentData, contentType).GetAwaiter().GetResult();
                return serializer.SerializeJson(result, true);
            });
        }

        #endregion
    }
}
