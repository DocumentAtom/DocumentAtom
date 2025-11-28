namespace DocumentAtom.McpServer.Classes
{
    using System;
    using System.Text.Json;

    /// <summary>
    /// Helper methods for DocumentAtom MCP Server.
    /// </summary>
    internal static class DocumentAtomMcpServerHelpers
    {
        /// <summary>
        /// Gets a GUID from JSON element, throwing if not present.
        /// </summary>
        public static Guid GetGuidRequired(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement prop))
                throw new ArgumentException($"Required parameter '{propertyName}' is missing");

            string? guidStr = prop.GetString();
            if (string.IsNullOrEmpty(guidStr) || !Guid.TryParse(guidStr, out Guid guid))
                throw new ArgumentException($"Invalid GUID format for '{propertyName}'");

            return guid;
        }

        /// <summary>
        /// Gets an optional GUID from JSON element, returning null if not present or invalid.
        /// </summary>
        public static Guid? GetGuidOptional(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out JsonElement prop))
                return null;

            string? guidStr = prop.GetString();
            if (string.IsNullOrEmpty(guidStr) || !Guid.TryParse(guidStr, out Guid guid))
                return null;

            return guid;
        }

        /// <summary>
        /// Gets a boolean from JSON element, returning default if not present.
        /// </summary>
        public static bool GetBoolOrDefault(JsonElement element, string propertyName, bool defaultValue = false)
        {
            if (element.TryGetProperty(propertyName, out JsonElement prop))
                return prop.GetBoolean();
            return defaultValue;
        }

        /// <summary>
        /// Gets an integer from JSON element, returning default if not present.
        /// </summary>
        public static int GetIntOrDefault(JsonElement element, string propertyName, int defaultValue = 0)
        {
            if (element.TryGetProperty(propertyName, out JsonElement prop))
                return prop.GetInt32();
            return defaultValue;
        }

        /// <summary>
        /// Gets a string from JSON element, returning default if not present.
        /// </summary>
        public static string GetStringOrDefault(JsonElement element, string propertyName, string defaultValue = "")
        {
            if (element.TryGetProperty(propertyName, out JsonElement prop))
                return prop.GetString() ?? defaultValue;
            return defaultValue;
        }
    }
}
