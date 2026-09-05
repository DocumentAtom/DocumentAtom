namespace DocumentAtom.McpServer.Classes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Text.Json;
    using System.Text.Json.Nodes;

    /// <summary>
    /// Installs or removes DocumentAtom MCP server integration into supported AI coding tools
    /// (Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux). All operations target the
    /// per-user (global) configuration for each tool. The DocumentAtom MCP server exposes an
    /// HTTP JSON-RPC transport, so the registered entry points each client at the HTTP endpoint.
    /// </summary>
    internal static class McpClientInstaller
    {
        #region Public-Members

        /// <summary>
        /// Name used as the MCP server key/identifier in each client's configuration.
        /// </summary>
        public static string ServerName = "documentatom";

        /// <summary>
        /// Default HTTP JSON-RPC endpoint registered when no endpoint is supplied.
        /// </summary>
        public static string DefaultEndpoint = "http://localhost:8200/rpc";

        /// <summary>
        /// Supported client targets.
        /// </summary>
        public static string[] SupportedTargets = new string[] { "claude", "codex", "gemini", "cursor", "mux" };

        #endregion

        #region Public-Methods

        /// <summary>
        /// Run the installer or uninstaller against one or more clients.
        /// </summary>
        /// <param name="uninstall">True to remove the integration, false to install it.</param>
        /// <param name="targetsCsv">Comma-separated list of targets, or "all"/empty for every supported target.</param>
        /// <param name="endpoint">HTTP JSON-RPC endpoint to register. If empty, the default endpoint is used.</param>
        /// <returns>Zero on success, non-zero if one or more targets failed.</returns>
        public static int Run(bool uninstall, string targetsCsv, string endpoint)
        {
            if (String.IsNullOrWhiteSpace(endpoint)) endpoint = DefaultEndpoint;

            List<string> targets = ResolveTargets(targetsCsv);
            if (targets.Count == 0)
            {
                Console.WriteLine("No valid targets specified. Valid targets: " + String.Join(", ", SupportedTargets) + ", all");
                return 1;
            }

            Console.WriteLine((uninstall ? "Uninstalling" : "Installing") + " DocumentAtom MCP integration");
            Console.WriteLine("| Server name : " + ServerName);
            if (!uninstall) Console.WriteLine("| Endpoint    : " + endpoint);
            Console.WriteLine("| Targets     : " + String.Join(", ", targets));
            Console.WriteLine();

            int failures = 0;

            foreach (string target in targets)
            {
                try
                {
                    switch (target)
                    {
                        case "claude":
                            InstallClaude(uninstall, endpoint);
                            break;
                        case "codex":
                            InstallCodex(uninstall, endpoint);
                            break;
                        case "gemini":
                            InstallGemini(uninstall, endpoint);
                            break;
                        case "cursor":
                            InstallCursor(uninstall, endpoint);
                            break;
                        case "mux":
                            InstallMux(uninstall, endpoint);
                            break;
                    }
                }
                catch (Exception e)
                {
                    failures++;
                    Console.WriteLine("[" + target + "] ERROR: " + e.Message);
                }
            }

            Console.WriteLine();
            Console.WriteLine(failures == 0 ? "Done." : "Completed with " + failures + " error(s).");
            return failures == 0 ? 0 : 1;
        }

        #endregion

        #region Private-Methods

        private static List<string> ResolveTargets(string targetsCsv)
        {
            List<string> resolved = new List<string>();

            if (String.IsNullOrWhiteSpace(targetsCsv) || targetsCsv.Trim().Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                resolved.AddRange(SupportedTargets);
                return resolved;
            }

            string[] parts = targetsCsv.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string part in parts)
            {
                string candidate = part.Trim().ToLowerInvariant();
                if (candidate.Equals("all"))
                {
                    resolved.Clear();
                    resolved.AddRange(SupportedTargets);
                    return resolved;
                }

                if (Array.IndexOf(SupportedTargets, candidate) >= 0)
                {
                    if (!resolved.Contains(candidate)) resolved.Add(candidate);
                }
                else
                {
                    Console.WriteLine("Ignoring unknown target '" + candidate + "'");
                }
            }

            return resolved;
        }

        private static void InstallClaude(bool uninstall, string endpoint)
        {
            string path = Path.Combine(HomeDirectory(), ".claude.json");
            JsonObject value = new JsonObject
            {
                ["type"] = "http",
                ["url"] = endpoint
            };
            UpsertMcpServersObject("claude", path, value, uninstall, true);
        }

        private static void InstallGemini(bool uninstall, string endpoint)
        {
            string path = Path.Combine(HomeDirectory(), ".gemini", "settings.json");
            JsonObject value = new JsonObject
            {
                ["httpUrl"] = endpoint
            };
            UpsertMcpServersObject("gemini", path, value, uninstall, true);
        }

        private static void InstallCursor(bool uninstall, string endpoint)
        {
            string path = Path.Combine(HomeDirectory(), ".cursor", "mcp.json");
            JsonObject value = new JsonObject
            {
                ["url"] = endpoint
            };
            UpsertMcpServersObject("cursor", path, value, uninstall, true);
        }

        private static void InstallMux(bool uninstall, string endpoint)
        {
            string configDir = Environment.GetEnvironmentVariable("MUX_CONFIG_DIR");
            if (String.IsNullOrWhiteSpace(configDir)) configDir = Path.Combine(HomeDirectory(), ".mux");
            string path = Path.Combine(configDir, "mcp-servers.json");

            string baseUrl;
            string mcpPath;
            SplitUrl(endpoint, out baseUrl, out mcpPath);

            JsonObject root;
            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                JsonNode? parsed = String.IsNullOrWhiteSpace(existing) ? null : JsonNode.Parse(existing, null, ParseDocumentOptions());
                root = parsed as JsonObject ?? new JsonObject();
            }
            else
            {
                if (uninstall)
                {
                    Console.WriteLine("[mux] not configured (no file at " + path + ")");
                    return;
                }
                root = new JsonObject();
            }

            JsonArray servers = root["servers"] as JsonArray ?? new JsonArray();

            JsonArray filtered = new JsonArray();
            bool found = false;
            foreach (JsonNode? item in servers)
            {
                if (item is JsonObject obj && ReadString(obj, "name").Equals(ServerName, StringComparison.OrdinalIgnoreCase))
                {
                    found = true;
                    continue;
                }
                filtered.Add(item == null ? null : item.DeepClone());
            }

            if (uninstall)
            {
                if (!found)
                {
                    Console.WriteLine("[mux] not configured (" + path + ")");
                    return;
                }
                root["servers"] = filtered;
                WriteJson(path, root);
                Console.WriteLine("[mux] removed '" + ServerName + "' from " + path);
                return;
            }

            filtered.Add(new JsonObject
            {
                ["name"] = ServerName,
                ["transport"] = "http",
                ["url"] = baseUrl,
                ["mcpPath"] = mcpPath
            });
            root["servers"] = filtered;
            EnsureDirectory(path);
            WriteJson(path, root);
            Console.WriteLine("[mux] configured '" + ServerName + "' -> " + baseUrl + mcpPath + " (" + path + ")");
        }

        private static void InstallCodex(bool uninstall, string endpoint)
        {
            string path = Path.Combine(HomeDirectory(), ".codex", "config.toml");
            string header = "[mcp_servers." + ServerName + "]";

            string content = File.Exists(path) ? File.ReadAllText(path) : String.Empty;
            string withoutSection = RemoveTomlSection(content, header);
            bool existed = !String.Equals(withoutSection, content, StringComparison.Ordinal);

            if (uninstall)
            {
                if (!File.Exists(path))
                {
                    Console.WriteLine("[codex] not configured (no file at " + path + ")");
                    return;
                }
                if (!existed)
                {
                    Console.WriteLine("[codex] not configured (" + path + ")");
                    return;
                }
                File.WriteAllText(path, withoutSection.TrimEnd() + Environment.NewLine);
                Console.WriteLine("[codex] removed '" + ServerName + "' from " + path);
                return;
            }

            StringBuilder sb = new StringBuilder();
            string trimmedExisting = withoutSection.TrimEnd();
            if (trimmedExisting.Length > 0)
            {
                sb.Append(trimmedExisting);
                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }
            sb.Append(header);
            sb.Append(Environment.NewLine);
            sb.Append("url = \"");
            sb.Append(endpoint);
            sb.Append("\"");
            sb.Append(Environment.NewLine);

            EnsureDirectory(path);
            File.WriteAllText(path, sb.ToString());
            Console.WriteLine("[codex] configured '" + ServerName + "' -> " + endpoint + " (" + path + ")");
        }

        private static void UpsertMcpServersObject(string label, string path, JsonObject value, bool uninstall, bool createIfMissing)
        {
            JsonObject root;

            if (File.Exists(path))
            {
                string existing = File.ReadAllText(path);
                JsonNode? parsed = String.IsNullOrWhiteSpace(existing) ? null : JsonNode.Parse(existing, null, ParseDocumentOptions());
                root = parsed as JsonObject ?? new JsonObject();
            }
            else
            {
                if (uninstall)
                {
                    Console.WriteLine("[" + label + "] not configured (no file at " + path + ")");
                    return;
                }
                if (!createIfMissing)
                {
                    Console.WriteLine("[" + label + "] skipped (no file at " + path + ")");
                    return;
                }
                root = new JsonObject();
            }

            JsonObject? servers = root["mcpServers"] as JsonObject;

            if (uninstall)
            {
                if (servers == null || !servers.ContainsKey(ServerName))
                {
                    Console.WriteLine("[" + label + "] not configured (" + path + ")");
                    return;
                }
                servers.Remove(ServerName);
                WriteJson(path, root);
                Console.WriteLine("[" + label + "] removed '" + ServerName + "' from " + path);
                return;
            }

            if (servers == null)
            {
                servers = new JsonObject();
                root["mcpServers"] = servers;
            }

            servers[ServerName] = value;
            EnsureDirectory(path);
            WriteJson(path, root);
            Console.WriteLine("[" + label + "] configured '" + ServerName + "' (" + path + ")");
        }

        private static string RemoveTomlSection(string content, string header)
        {
            if (String.IsNullOrEmpty(content)) return content;

            string[] lines = content.Replace("\r\n", "\n").Split('\n');
            StringBuilder sb = new StringBuilder();
            bool skipping = false;

            foreach (string line in lines)
            {
                string trimmed = line.Trim();

                if (!skipping && trimmed.Equals(header, StringComparison.Ordinal))
                {
                    skipping = true;
                    continue;
                }

                if (skipping)
                {
                    if (trimmed.StartsWith("[", StringComparison.Ordinal))
                    {
                        skipping = false;
                    }
                    else
                    {
                        continue;
                    }
                }

                sb.Append(line);
                sb.Append('\n');
            }

            return sb.ToString();
        }

        private static void SplitUrl(string url, out string baseUrl, out string path)
        {
            baseUrl = url;
            path = "/rpc";

            try
            {
                Uri uri = new Uri(url);
                baseUrl = uri.GetLeftPart(UriPartial.Authority);
                path = String.IsNullOrEmpty(uri.AbsolutePath) || uri.AbsolutePath.Equals("/") ? "/rpc" : uri.AbsolutePath;
            }
            catch (UriFormatException)
            {
                // Leave defaults in place for non-absolute inputs.
            }
        }

        private static string ReadString(JsonObject obj, string key)
        {
            JsonNode? node = obj[key];
            if (node == null) return String.Empty;
            try
            {
                return node.GetValue<string>() ?? String.Empty;
            }
            catch (Exception)
            {
                return String.Empty;
            }
        }

        private static JsonDocumentOptions ParseDocumentOptions()
        {
            return new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            };
        }

        private static void WriteJson(string path, JsonObject root)
        {
            File.WriteAllText(path, root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
        }

        private static void EnsureDirectory(string path)
        {
            string? directory = Path.GetDirectoryName(path);
            if (!String.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static string HomeDirectory()
        {
            string home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (String.IsNullOrEmpty(home)) home = Environment.GetEnvironmentVariable("HOME") ?? ".";
            return home;
        }

        #endregion
    }
}
