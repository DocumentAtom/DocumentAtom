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
    /// Registration methods for Image operations.
    /// </summary>
    public static class ImageRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers image tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "image/process",
                "Process an image file and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded image data" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("Image data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                    byte[] imageData = Convert.FromBase64String(base64Data);

                    List<Atom> atoms = sdk.Atom.ProcessPng(imageData).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });

            server.RegisterTool(
                "image/ocr",
                "Extract text from an image using OCR",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded image data" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("Image data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                    byte[] imageData = Convert.FromBase64String(base64Data);

                    string filename = DocumentAtomMcpServerHelpers.GetStringOrDefault(args.Value, "filename", "");

                List<Atom> atoms = sdk.Atom.ProcessOcr(imageData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers image methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("image/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Image data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                byte[] imageData = Convert.FromBase64String(base64Data);

                List<Atom> atoms =sdk.Atom.ProcessPng(imageData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });

            server.RegisterMethod("image/ocr", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Image data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                byte[] imageData = Convert.FromBase64String(base64Data);

                List<Atom> atoms = sdk.Atom.ProcessOcr(imageData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers image methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("image/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Image data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                byte[] imageData = Convert.FromBase64String(base64Data);

                List<Atom> atoms =sdk.Atom.ProcessPng(imageData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });

            server.RegisterMethod("image/ocr", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("Image data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("Image data cannot be null");
                byte[] imageData = Convert.FromBase64String(base64Data);

                List<Atom> atoms = sdk.Atom.ProcessOcr(imageData).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}
