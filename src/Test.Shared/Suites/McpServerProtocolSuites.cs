namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Sockets;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using DocumentAtom.McpServer.Registrations;
    using DocumentAtom.Sdk;
    using SerializationHelper;
    using Touchstone.Core;
    using Voltaic.Core;
    using Voltaic.Mcp;

    /// <summary>
    /// End-to-end suites that stand up the DocumentAtom MCP registrations on real Voltaic servers and
    /// verify the Voltaic 2.x protocol surface: only DocumentAtom tools are published, protocol ping
    /// returns an empty result, tools are reachable only through tools/call, and input validation applies.
    /// No DocumentAtom backend is required; cases only exercise paths that fail before the SDK is called.
    /// </summary>
    internal static class McpServerProtocolSuites
    {
        private static readonly string[] _ExpectedTools = new[]
        {
            "image_process", "image_ocr", "csv_process", "excel_process", "html_process", "json_process",
            "markdown_process", "ocr_process", "pdf_process", "powerpoint_process", "richtext_process",
            "text_process", "typedetection_detect", "word_process", "xml_process"
        };

        private static readonly string[] _RemovedDemoTools = new[] { "ping", "echo", "getTime", "getSessions", "getClients" };

        internal static TestSuiteDescriptor Http()
        {
            return new SuiteBuilder("mcpserver.http")
                .CaseAsync("tools-list-publishes-only-documentatom-tools", "tools/list returns exactly the DocumentAtom tools", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        List<string> names = await ListToolNamesAsync(client, ct).ConfigureAwait(false);

                        Check.Count(_ExpectedTools.Length, names);
                        foreach (string expected in _ExpectedTools)
                            Check.True(names.Contains(expected), "tools/list is missing " + expected);
                        foreach (string removed in _RemovedDemoTools)
                            Check.False(names.Contains(removed), "tools/list must not publish Voltaic demo tool " + removed);
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tool-names-are-valid", "every published tool name uses only A-Z, a-z, 0-9, underscore, hyphen and dot", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        List<string> names = await ListToolNamesAsync(client, ct).ConfigureAwait(false);

                        foreach (string name in names)
                        {
                            Check.True(name.Length >= 1 && name.Length <= 128, name + " must be 1 to 128 characters");
                            Check.True(name.All(c => Char.IsAsciiLetterOrDigit(c) || c == '_' || c == '-' || c == '.'), name + " contains an invalid character");
                            Check.False(name.Contains('/'), name + " must not contain a slash");
                        }
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tools-have-input-schemas", "every published tool declares an object input schema requiring data", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync("tools/list", new { }, 10000, ct).ConfigureAwait(false);
                        Check.Null(response.Error);

                        using JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(response.Result));
                        foreach (JsonElement tool in doc.RootElement.GetProperty("tools").EnumerateArray())
                        {
                            string name = tool.GetProperty("name").GetString() ?? String.Empty;
                            JsonElement schema = tool.GetProperty("inputSchema");
                            Check.Equal("object", schema.GetProperty("type").GetString(), name + " schema type");
                            List<string?> required = schema.GetProperty("required").EnumerateArray().Select(r => r.GetString()).ToList();
                            Check.True(required.Contains("data"), name + " must require data");
                        }
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("ping-returns-empty-result", "protocol ping succeeds and returns an empty object", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        await client.PingAsync(0, ct).ConfigureAwait(false);

                        JsonRpcResponse response = await client.CallAsync("ping", null, 10000, ct).ConfigureAwait(false);
                        Check.Null(response.Error);
                        Check.Equal("{}", JsonSerializer.Serialize(response.Result));
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("diagnostic-tools-not-published", "echo, getTime and getSessions cannot be called through tools/call", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        foreach (string removed in new[] { "echo", "getTime", "getSessions" })
                        {
                            JsonRpcResponse response = await client.CallAsync(
                                "tools/call",
                                new { name = removed, arguments = new { message = "hello" } },
                                10000,
                                ct).ConfigureAwait(false);

                            Check.True(IsFailure(response), "tools/call " + removed + " must fail");
                        }
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("bare-tool-call-method-not-found", "calling a tool by its bare name returns -32601", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync("csv_process", new { data = "YSxiCjEsMg==" }, 10000, ct).ConfigureAwait(false);

                        Check.NotNull(response.Error);
                        Check.Equal(-32601, response.Error!.Code);
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tools-call-missing-required-argument", "tools/call without the required data argument returns an isError tool result", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync(
                            "tools/call",
                            new { name = "csv_process", arguments = new { extractOcr = true } },
                            10000,
                            ct).ConfigureAwait(false);

                        Check.Null(response.Error);
                        Check.True(IsToolError(response), "invalid arguments must surface as an isError tool result");
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tools-call-wrong-argument-type", "tools/call with a non-string data argument returns an isError tool result", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync(
                            "tools/call",
                            new { name = "text_process", arguments = new { data = 42 } },
                            10000,
                            ct).ConfigureAwait(false);

                        Check.Null(response.Error);
                        Check.True(IsToolError(response), "invalid arguments must surface as an isError tool result");
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tools-call-null-optional-argument", "tools/call with a null value for an optional string argument returns an isError tool result", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync(
                            "tools/call",
                            new { name = "typedetection_detect", arguments = new { data = "YQ==", contentType = (string?)null } },
                            10000,
                            ct).ConfigureAwait(false);

                        Check.Null(response.Error);
                        Check.True(IsToolError(response), "invalid arguments must surface as an isError tool result");
                        Check.Contains("contentType", GetToolResultText(response));
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("tools-call-invalid-base64-fails", "tools/call routes to the DocumentAtom handler, which rejects invalid base64", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync(
                            "tools/call",
                            new { name = "text_process", arguments = new { data = "!!! not base64 !!!" } },
                            10000,
                            ct).ConfigureAwait(false);

                        Check.True(IsFailure(response), "invalid base64 must surface as a tool failure");
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("unknown-tool-fails", "tools/call for an unknown tool fails", async ct =>
                {
                    await WithHttpServerAsync(async client =>
                    {
                        JsonRpcResponse response = await client.CallAsync(
                            "tools/call",
                            new { name = "nonexistent_tool", arguments = new { data = "YQ==" } },
                            10000,
                            ct).ConfigureAwait(false);

                        Check.True(IsFailure(response), "unknown tool must fail");
                    }, ct).ConfigureAwait(false);
                })
                .Build("MCP server over HTTP (Voltaic 2.x)");
        }

        internal static TestSuiteDescriptor Tcp()
        {
            return new SuiteBuilder("mcpserver.tcp")
                .CaseAsync("ping-succeeds", "protocol ping succeeds over TCP", async ct =>
                {
                    await WithTcpServerAsync(async client =>
                    {
                        object? result = await client.CallAsync<object?>("ping", null, 10000, ct).ConfigureAwait(false);
                        Check.Equal("{}", JsonSerializer.Serialize(result));
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("no-demo-tools-published", "tools/list over TCP publishes no Voltaic demo tools", async ct =>
                {
                    await WithTcpServerAsync(async client =>
                    {
                        object? result = await client.CallAsync<object?>("tools/list", new { }, 10000, ct).ConfigureAwait(false);
                        string json = JsonSerializer.Serialize(result);
                        foreach (string removed in _RemovedDemoTools)
                            Check.DoesNotContain("\"" + removed + "\"", json);
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("bare-method-still-registered", "DocumentAtom JSON-RPC methods remain callable over TCP", async ct =>
                {
                    await WithTcpServerAsync(async client =>
                    {
                        Exception ex = await Check.ThrowsAsync<Exception>(
                            () => client.CallAsync<string>("csv_process", null, 10000, ct)).ConfigureAwait(false);

                        // The handler ran and rejected the missing parameters (internal error), rather than method-not-found
                        Check.Contains("-32603", ex.Message);
                        Check.DoesNotContain("-32601", ex.Message);
                    }, ct).ConfigureAwait(false);
                })
                .CaseAsync("removed-get-clients-method-not-found", "getClients is no longer a TCP method", async ct =>
                {
                    await WithTcpServerAsync(async client =>
                    {
                        Exception ex = await Check.ThrowsAsync<Exception>(
                            () => client.CallAsync<object?>("getClients", null, 10000, ct)).ConfigureAwait(false);

                        Check.Contains("-32601", ex.Message);
                    }, ct).ConfigureAwait(false);
                })
                .Build("MCP server over TCP (Voltaic 2.x)");
        }

        private static async Task WithHttpServerAsync(Func<McpHttpClient, Task> body, CancellationToken token)
        {
            int port = GetFreePort();
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            using DocumentAtomSdk sdk = new DocumentAtomSdk("http://127.0.0.1:1");
            Serializer serializer = new Serializer();

            using McpHttpServer server = new McpHttpServer("localhost", port, "/rpc", "/events");
            RegisterHttpTools(server, sdk, serializer);
            Task serverTask = server.StartAsync(cts.Token);

            using McpHttpClient client = new McpHttpClient();
            try
            {
                bool connected = false;
                for (int attempt = 0; attempt < 20 && !connected; attempt++)
                {
                    connected = await client.ConnectAsync("http://localhost:" + port, "/rpc", "/events", cts.Token).ConfigureAwait(false);
                    if (!connected) await Task.Delay(100, cts.Token).ConfigureAwait(false);
                }

                Check.True(connected, "could not connect to the MCP HTTP server");
                await body(client).ConfigureAwait(false);
            }
            finally
            {
                client.Disconnect();
                cts.Cancel();
                server.Stop();
                try { await serverTask.ConfigureAwait(false); } catch (Exception) { }
            }
        }

        private static async Task WithTcpServerAsync(Func<McpTcpClient, Task> body, CancellationToken token)
        {
            int port = GetFreePort();
            using CancellationTokenSource cts = CancellationTokenSource.CreateLinkedTokenSource(token);
            using DocumentAtomSdk sdk = new DocumentAtomSdk("http://127.0.0.1:1");
            Serializer serializer = new Serializer();

            using McpTcpServer server = new McpTcpServer(IPAddress.Loopback, port);
            RegisterTcpMethods(server, sdk, serializer);
            Task serverTask = server.StartAsync(cts.Token);

            using McpTcpClient client = new McpTcpClient();
            try
            {
                bool connected = false;
                for (int attempt = 0; attempt < 20 && !connected; attempt++)
                {
                    connected = await client.ConnectAsync("127.0.0.1", port, cts.Token).ConfigureAwait(false);
                    if (!connected) await Task.Delay(100, cts.Token).ConfigureAwait(false);
                }

                Check.True(connected, "could not connect to the MCP TCP server");
                await body(client).ConfigureAwait(false);
            }
            finally
            {
                cts.Cancel();
                server.Stop();
                try { await serverTask.ConfigureAwait(false); } catch (Exception) { }
            }
        }

        private static void RegisterHttpTools(McpHttpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            ImageRegistrations.RegisterHttpTools(server, sdk, serializer);
            CsvRegistrations.RegisterHttpTools(server, sdk, serializer);
            ExcelRegistrations.RegisterHttpTools(server, sdk, serializer);
            HtmlRegistrations.RegisterHttpTools(server, sdk, serializer);
            JsonRegistrations.RegisterHttpTools(server, sdk, serializer);
            MarkdownRegistrations.RegisterHttpTools(server, sdk, serializer);
            OcrRegistrations.RegisterHttpTools(server, sdk, serializer);
            PdfRegistrations.RegisterHttpTools(server, sdk, serializer);
            PowerPointRegistrations.RegisterHttpTools(server, sdk, serializer);
            RichTextRegistrations.RegisterHttpTools(server, sdk, serializer);
            TextRegistrations.RegisterHttpTools(server, sdk, serializer);
            TypeDetectionRegistrations.RegisterHttpTools(server, sdk, serializer);
            WordRegistrations.RegisterHttpTools(server, sdk, serializer);
            XmlRegistrations.RegisterHttpTools(server, sdk, serializer);
        }

        private static void RegisterTcpMethods(McpTcpServer server, DocumentAtomSdk sdk, Serializer serializer)
        {
            ImageRegistrations.RegisterTcpMethods(server, sdk, serializer);
            CsvRegistrations.RegisterTcpMethods(server, sdk, serializer);
            ExcelRegistrations.RegisterTcpMethods(server, sdk, serializer);
            HtmlRegistrations.RegisterTcpMethods(server, sdk, serializer);
            JsonRegistrations.RegisterTcpMethods(server, sdk, serializer);
            MarkdownRegistrations.RegisterTcpMethods(server, sdk, serializer);
            OcrRegistrations.RegisterTcpMethods(server, sdk, serializer);
            PdfRegistrations.RegisterTcpMethods(server, sdk, serializer);
            PowerPointRegistrations.RegisterTcpMethods(server, sdk, serializer);
            RichTextRegistrations.RegisterTcpMethods(server, sdk, serializer);
            TextRegistrations.RegisterTcpMethods(server, sdk, serializer);
            TypeDetectionRegistrations.RegisterTcpMethods(server, sdk, serializer);
            WordRegistrations.RegisterTcpMethods(server, sdk, serializer);
            XmlRegistrations.RegisterTcpMethods(server, sdk, serializer);
        }

        private static async Task<List<string>> ListToolNamesAsync(McpHttpClient client, CancellationToken token)
        {
            JsonRpcResponse response = await client.CallAsync("tools/list", new { }, 10000, token).ConfigureAwait(false);
            Check.Null(response.Error);

            using JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(response.Result));
            return doc.RootElement.GetProperty("tools").EnumerateArray()
                .Select(t => t.GetProperty("name").GetString() ?? String.Empty)
                .ToList();
        }

        private static bool IsFailure(JsonRpcResponse response)
        {
            if (response.Error != null) return true;
            if (response.Result == null) return false;

            using JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(response.Result));
            return doc.RootElement.ValueKind == JsonValueKind.Object
                && doc.RootElement.TryGetProperty("isError", out JsonElement isError)
                && isError.ValueKind == JsonValueKind.True;
        }

        private static bool IsToolError(JsonRpcResponse response)
        {
            return response.Error == null && IsFailure(response);
        }

        private static string GetToolResultText(JsonRpcResponse response)
        {
            if (response.Result == null) return String.Empty;

            using JsonDocument doc = JsonDocument.Parse(JsonSerializer.Serialize(response.Result));
            if (!doc.RootElement.TryGetProperty("content", out JsonElement content) || content.ValueKind != JsonValueKind.Array)
                return String.Empty;

            return String.Join(Environment.NewLine, content.EnumerateArray()
                .Where(c => c.TryGetProperty("text", out JsonElement t) && t.ValueKind == JsonValueKind.String)
                .Select(c => c.GetProperty("text").GetString()));
        }

        private static int GetFreePort()
        {
            TcpListener listener = new TcpListener(IPAddress.Loopback, 0);
            listener.Start();
            int port = ((IPEndPoint)listener.LocalEndpoint).Port;
            listener.Stop();
            return port;
        }
    }
}
