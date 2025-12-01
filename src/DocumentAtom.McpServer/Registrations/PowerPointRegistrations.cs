namespace DocumentAtom.McpServer.Registrations
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using DocumentAtom.Sdk;
    using SerializationHelper;
    using Voltaic;

    /// <summary>
    /// Registration methods for PowerPoint operations.
    /// </summary>
    public static class PowerPointRegistrations
    {
        #region HTTP-Tools

        /// <summary>
        /// Registers PowerPoint tools on HTTP server.
        /// </summary>
        /// <param name="server">HTTP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterTool(
                "powerpoint/process",
                "Process a PowerPoint file and extract atoms",
                new
                {
                    type = "object",
                    properties = new
                    {
                        data = new { type = "string", description = "Base64-encoded PowerPoint file data" },
                        extractOcr = new { type = "boolean", description = "Whether to extract text from images using OCR (default: false)" }
                    },
                    required = new[] { "data" }
                },
                (args) =>
                {
                    if (!args.HasValue) throw new ArgumentException("Parameters required");
                    if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                        throw new ArgumentException("PowerPoint data is required");

                    string base64Data = dataProp.GetString() ?? throw new ArgumentException("PowerPoint data cannot be null");
                    byte[] powerPointData = Convert.FromBase64String(base64Data);
                    bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                    var atoms = sdk.Atom.ProcessPowerPoint(powerPointData, extractOcr).GetAwaiter().GetResult();
                    return serializer.SerializeJson(atoms, true);
                });
        }

        #endregion

        #region TCP-Methods

        /// <summary>
        /// Registers PowerPoint methods on TCP server.
        /// </summary>
        /// <param name="server">TCP server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("powerpoint/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("PowerPoint data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("PowerPoint data cannot be null");
                byte[] powerPointData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                var atoms = sdk.Atom.ProcessPowerPoint(powerPointData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion

        #region WebSocket-Methods

        /// <summary>
        /// Registers PowerPoint methods on WebSocket server.
        /// </summary>
        /// <param name="server">WebSocket server instance.</param>
        /// <param name="sdk">DocumentAtom SDK instance.</param>
        /// <param name="serializer">Serializer instance.</param>
        public static void RegisterWebSocketMethods(McpWebsocketsServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            server.RegisterMethod("powerpoint/process", (args) =>
            {
                if (!args.HasValue) throw new ArgumentException("Parameters required");
                if (!args.Value.TryGetProperty("data", out JsonElement dataProp))
                    throw new ArgumentException("PowerPoint data is required");

                string base64Data = dataProp.GetString() ?? throw new ArgumentException("PowerPoint data cannot be null");
                byte[] powerPointData = Convert.FromBase64String(base64Data);
                bool extractOcr = DocumentAtomMcpServerHelpers.GetBoolOrDefault(args.Value, "extractOcr", false);

                var atoms = sdk.Atom.ProcessPowerPoint(powerPointData, extractOcr).GetAwaiter().GetResult();
                return serializer.SerializeJson(atoms, true);
            });
        }

        #endregion
    }
}

