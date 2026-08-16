namespace DocumentAtom.Testing.Shared.Suites
{
    using System;
    using System.Text.Json;
    using DocumentAtom.McpServer.Classes;
    using Touchstone.Core;
    using Voltaic.Core;

    internal static class McpServerSuites
    {
        internal static TestSuiteDescriptor Parameters()
        {
            return new SuiteBuilder("mcpserver.parameters")
                .Case("object-parameters-to-json-element", "MCP parameters expose object properties", () =>
                {
                    RpcParameters parameters = RpcParameters.FromObject(new
                    {
                        data = "YWJj",
                        extractOcr = true
                    });

                    JsonElement element = parameters.ToJsonElement();

                    Check.Equal(JsonValueKind.Object, element.ValueKind);
                    Check.True(element.TryGetProperty("data", out JsonElement data));
                    Check.Equal("YWJj", data.GetString());
                    Check.True(element.TryGetProperty("extractOcr", out JsonElement extractOcr));
                    Check.True(extractOcr.GetBoolean());
                })
                .Case("missing-parameters-throw", "MCP parameters reject missing values", () =>
                {
                    ArgumentException ex = Check.Throws<ArgumentException>(() => new RpcParameters(null).ToJsonElement());

                    Check.Contains("Parameters required", ex.Message);
                })
                .Case("non-object-parameters-throw", "MCP parameters reject non-object values", () =>
                {
                    ArgumentException ex = Check.Throws<ArgumentException>(() => RpcParameters.FromObject(new[] { "YWJj" }).ToJsonElement());

                    Check.Contains("Parameters must be a JSON object", ex.Message);
                })
                .Build("MCP server parameters");
        }
    }
}
