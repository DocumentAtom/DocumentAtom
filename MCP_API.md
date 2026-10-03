# DocumentAtom MCP API

The **DocumentAtom MCP Server** (`DocumentAtom.McpServer`) exposes DocumentAtom's document
processing and atomization capabilities to AI agents and tools through the
[Model Context Protocol](https://modelcontextprotocol.io) (MCP).

It is a thin MCP front-end over the DocumentAtom REST server (`DocumentAtom.Server`): each MCP
tool call is translated into a call against the REST API via the DocumentAtom SDK, and the
resulting **atoms** are returned as JSON.

---

## Table of contents

- [Architecture](#architecture)
- [Transports and endpoints](#transports-and-endpoints)
- [Running the server](#running-the-server)
- [Configuration](#configuration)
- [Installing into AI coding tools](#installing-into-ai-coding-tools)
- [Protocol usage](#protocol-usage)
- [Tool catalog](#tool-catalog)
- [Response format](#response-format)
- [Errors](#errors)
- [Compatibility notes and caveats](#compatibility-notes-and-caveats)

---

## Architecture

```
+-------------------+       MCP (JSON-RPC 2.0)        +----------------------+     REST/HTTP      +----------------------+
|  MCP client       |  HTTP / TCP / WebSocket         |  DocumentAtom        |  DocumentAtom SDK  |  DocumentAtom.Server |
|  (Claude, Codex,  | ------------------------------> |  MCP Server          | -----------------> |  (REST API)          |
|   Gemini, Cursor, |                                 |  (DocumentAtom.      |                    |  default :8000       |
|   mux, ...)       | <------------------------------ |   McpServer)         | <----------------- |                      |
+-------------------+     atoms (JSON)                +----------------------+                    +----------------------+
```

> **The MCP server requires a running `DocumentAtom.Server` REST endpoint.** By default it
> connects to `http://localhost:8000`. If the endpoint is not reachable/configured, the MCP
> server fails to start. See [Configuration](#configuration).

---

## Transports and endpoints

The MCP server starts **three transports simultaneously**, all serving the same set of tools:

| Transport | Default bind          | Path      | URL example                     |
|-----------|-----------------------|-----------|---------------------------------|
| HTTP      | `localhost:8200`      | `/rpc`    | `http://localhost:8200/rpc`     |
| HTTP SSE  | `localhost:8200`      | `/events` | `http://localhost:8200/events`  |
| TCP       | `127.0.0.1:8201`      | —         | `tcp://127.0.0.1:8201`          |
| WebSocket | `localhost:8202`      | `/mcp`    | `ws://localhost:8202/mcp`       |

- **HTTP** is a JSON-RPC 2.0 endpoint at `/rpc`; server-sent events are delivered on `/events`.
- **TCP** exchanges newline / framed JSON-RPC messages over a raw socket.
- **WebSocket** exchanges JSON-RPC messages as WebSocket text frames on `/mcp`.

The MCP protocol methods (`initialize`, `tools/list`, `tools/call`, `ping`, …) are available on
all three transports. `ping` returns an empty result (`{}`), as the MCP specification requires.

- On **HTTP**, the DocumentAtom operations are MCP tools. `tools/list` returns exactly the 15
  DocumentAtom tools (no diagnostic tools such as `echo`, `getTime`, or `getSessions`), and a tool is
  invoked only through `tools/call`. Calling a tool name as a bare JSON-RPC method (for example
  `"method": "csv_process"`) returns `-32601` (method not found).
- On **TCP** and **WebSocket**, the DocumentAtom operations are registered as JSON-RPC methods and
  are invoked by name (for example `"method": "csv_process"`).

> **There is no stdio transport.** Clients that only support locally-spawned stdio MCP servers
> cannot launch this server directly; connect over HTTP or WebSocket instead. See
> [Compatibility notes and caveats](#compatibility-notes-and-caveats).

---

## Running the server

### From source

```bash
cd src/DocumentAtom.McpServer
dotnet run
```

### From a build

```bash
dotnet DocumentAtom.McpServer.dll
```

### Docker

```bash
docker run --rm \
  -p 8200:8200 -p 8201:8201 -p 8202:8202 \
  -e DOCUMENTATOM_ENDPOINT=http://host.docker.internal:8000 \
  jchristn77/documentatom-mcp:latest
```

### Command-line options

| Option                  | Description                                                             |
|-------------------------|-------------------------------------------------------------------------|
| `--config=<file>`       | Settings file path (default `./documentatom.json`).                     |
| `--showconfig`          | Print the effective configuration and exit.                             |
| `--install[=targets]`   | Register this server with AI coding tools and exit (see below).         |
| `--uninstall[=targets]` | Remove this server from AI coding tools and exit.                       |
| `--endpoint=<url>`      | HTTP endpoint to register during `--install` (default `http://localhost:8200/rpc`). |
| `--help`, `-h`          | Show help.                                                              |

---

## Configuration

Settings are read from a JSON file (default `./documentatom.json`, created with defaults on first
run) and can be overridden by environment variables.

### Key settings

| Setting                  | Default                  | Description                                             |
|--------------------------|--------------------------|---------------------------------------------------------|
| `DocumentAtom.Endpoint`  | `http://localhost:8000`  | REST endpoint of `DocumentAtom.Server` (**required**).  |
| `DocumentAtom.AccessKey` | *(empty)*                | Access key for the REST API (optional).                 |
| `Http.Hostname` / `Http.Port` | `localhost` / `8200` | HTTP JSON-RPC bind address.                             |
| `Tcp.Address` / `Tcp.Port`    | `127.0.0.1` / `8201` | TCP bind address.                                       |
| `WebSocket.Hostname` / `WebSocket.Port` | `localhost` / `8202` | WebSocket bind address.                     |

### Environment variables

| Variable                        | Overrides                              |
|---------------------------------|----------------------------------------|
| `DOCUMENTATOM_ENDPOINT`         | `DocumentAtom.Endpoint`                |
| `DOCUMENTATOM_ACCESS_KEY`       | `DocumentAtom.AccessKey`               |
| `MCP_HTTP_HOSTNAME`             | `Http.Hostname`                        |
| `MCP_HTTP_PORT`                 | `Http.Port`                            |
| `MCP_TCP_ADDRESS`               | `Tcp.Address`                          |
| `MCP_TCP_PORT`                  | `Tcp.Port`                             |
| `MCP_WS_HOSTNAME`               | `WebSocket.Hostname`                   |
| `MCP_WS_PORT`                   | `WebSocket.Port`                       |
| `MCP_CONSOLE_LOGGING`           | Console logging on/off (`1`/`0`).      |
| `DOCUMENTATOM_TELEMETRY_ENABLE` | OpenTelemetry export on/off.           |
| `DOCUMENTATOM_OTLP_ENDPOINT`    | OTLP exporter endpoint.                |
| `DOCUMENTATOM_OTLP_PROTOCOL`    | OTLP protocol (`grpc` or `http`).      |

---

## Installing into AI coding tools

The server can register itself with common AI coding tools' **per-user (global)** configuration,
pointing each tool at the HTTP endpoint (`http://localhost:8200/rpc` by default). The MCP server
is registered under the name **`documentatom`**.

### Using the built-in command

```bash
# Register with every supported tool
dotnet DocumentAtom.McpServer.dll --install

# Register with specific tools
dotnet DocumentAtom.McpServer.dll --install=claude,cursor

# Register with a custom endpoint
dotnet DocumentAtom.McpServer.dll --install=mux --endpoint=http://localhost:8200/rpc

# Remove the integration
dotnet DocumentAtom.McpServer.dll --uninstall=all
```

`targets` is a comma-separated list of: `claude`, `codex`, `gemini`, `cursor`, `mux` (or `all`).

### Using the platform scripts

See [`scripts/`](scripts/README.md) for one-command install/uninstall wrappers for Windows,
macOS, and Linux.

### What gets written

| Tool         | Config file (per user)                     | Entry written                                                                 |
|--------------|--------------------------------------------|-------------------------------------------------------------------------------|
| Claude Code  | `~/.claude.json`                           | `mcpServers.documentatom = { "type": "http", "url": "<endpoint>" }`           |
| OpenAI Codex | `~/.codex/config.toml`                     | `[mcp_servers.documentatom]` with `url = "<endpoint>"`                        |
| Gemini CLI   | `~/.gemini/settings.json`                  | `mcpServers.documentatom = { "httpUrl": "<endpoint>" }`                       |
| Cursor       | `~/.cursor/mcp.json`                        | `mcpServers.documentatom = { "url": "<endpoint>" }`                           |
| mux          | `~/.mux/mcp-servers.json` (or `$MUX_CONFIG_DIR`) | `servers[] += { "name": "documentatom", "transport": "http", "url": "<base>", "mcpPath": "<path>" }` |

Existing entries in these files are preserved; only the `documentatom` entry is added or removed.

---

## Protocol usage

All examples use the HTTP transport with JSON-RPC 2.0. Replace the base URL for other transports.

### 1. Initialize

```bash
curl -s http://localhost:8200/rpc \
  -H 'Content-Type: application/json' \
  -d '{
    "jsonrpc": "2.0",
    "id": 1,
    "method": "initialize",
    "params": {
      "protocolVersion": "2024-11-05",
      "capabilities": {},
      "clientInfo": { "name": "example-client", "version": "1.0.0" }
    }
  }'
```

### 2. List tools

```bash
curl -s http://localhost:8200/rpc \
  -H 'Content-Type: application/json' \
  -d '{ "jsonrpc": "2.0", "id": 2, "method": "tools/list" }'
```

### 3. Call a tool

Tool inputs take a **base64-encoded** file payload in the `data` field. For example, to atomize
the text `Hello, world!` (`SGVsbG8sIHdvcmxkIQ==`):

```bash
curl -s http://localhost:8200/rpc \
  -H 'Content-Type: application/json' \
  -d '{
    "jsonrpc": "2.0",
    "id": 3,
    "method": "tools/call",
    "params": {
      "name": "text_process",
      "arguments": { "data": "SGVsbG8sIHdvcmxkIQ==" }
    }
  }'
```

The tool result is a standard MCP tool result whose text content is the JSON serialization of the
extracted atoms (see [Response format](#response-format)).

---

## Tool catalog

All tools accept a JSON object argument. `data` is always a **base64-encoded** representation of
the file bytes and is required. Tools that can rasterize/scan embedded images accept an optional
`extractOcr` boolean (default `false`).

| Tool                    | Description                                         | Arguments                              |
|-------------------------|-----------------------------------------------------|----------------------------------------|
| `text_process`          | Process a text file and extract atoms               | `data`                                 |
| `markdown_process`      | Process a Markdown file and extract atoms           | `data`                                 |
| `csv_process`           | Process a CSV file and extract atoms                | `data`, `extractOcr?`                  |
| `json_process`          | Process a JSON file and extract atoms               | `data`                                 |
| `xml_process`           | Process an XML document and extract atoms           | `data`                                 |
| `html_process`          | Process an HTML file and extract atoms              | `data`                                 |
| `excel_process`         | Process an Excel (.xlsx) file and extract atoms     | `data`, `extractOcr?`                  |
| `powerpoint_process`    | Process a PowerPoint (.pptx) file and extract atoms | `data`, `extractOcr?`                  |
| `word_process`          | Process a Word (.docx) document and extract atoms   | `data`, `extractOcr?`                  |
| `pdf_process`           | Process a PDF document and extract atoms            | `data`, `extractOcr?`                  |
| `richtext_process`      | Process a Rich Text (.rtf) file and extract atoms   | `data`, `extractOcr?`                  |
| `image_process`         | Process an image file and extract atoms             | `data`                                 |
| `image_ocr`             | Extract text from an image using OCR                | `data`                                 |
| `ocr_process`           | Process an image (PNG) and extract text using OCR   | `data`                                 |
| `typedetection_detect`  | Detect a document's type from its content           | `data`, `contentType?`                 |

### Argument reference

| Argument      | Type    | Required | Notes                                                             |
|---------------|---------|----------|-------------------------------------------------------------------|
| `data`        | string  | yes      | Base64-encoded file bytes.                                        |
| `extractOcr`  | boolean | no       | When `true`, also extracts atoms from images via OCR (default `false`). |
| `contentType` | string  | no       | Optional MIME/content-type hint for `typedetection_detect`.       |

---

## Response format

### `*_process`, `image_*`, `ocr_process`

These tools return a JSON **array of `Atom` objects**. An atom captures one constituent part of a
document (a paragraph, list, table, image, etc.). Key fields:

| Field             | Type          | Description                                                        |
|-------------------|---------------|-------------------------------------------------------------------|
| `GUID`            | guid          | Unique atom identifier.                                            |
| `ParentGUID`      | guid \| null  | Parent atom identifier, when nested.                              |
| `Type`            | enum          | Atom type (e.g. `Text`, `Table`, `Image`, `List`, `Binary`).      |
| `Text`            | string        | Extracted text content, when applicable.                          |
| `Title` / `Subtitle` | string     | Section title / subtitle, when applicable.                        |
| `OrderedList` / `UnorderedList` | string[] | List items, when the atom is a list.                    |
| `Table`           | object        | Tabular data, when the atom is a table.                           |
| `SheetName` / `CellIdentifier` | string | Spreadsheet location, for Excel atoms.                    |
| `Binary`          | base64        | Raw bytes, when the atom is binary/image content.                 |
| `MD5Hash` / `SHA1Hash` / `SHA256Hash` | base64 | Content hashes.                                  |
| `BoundingBox`     | object        | Position on the page, when available.                             |
| `Formatting`      | enum \| null  | Markdown formatting hint.                                          |
| `Quarks`          | Atom[]        | Sub-atoms (finer-grained decomposition).                          |
| `Chunks`          | Chunk[]       | Chunked representations of the atom's text.                       |

Example (abridged):

```json
[
  {
    "GUID": "b6b1f0f8-3d2a-4c9e-9f6b-6a2f0c1d4e77",
    "ParentGUID": null,
    "Type": "Text",
    "Text": "Hello, world!",
    "MD5Hash": "6cd3556deb0da54bca060b4c39479839",
    "SHA256Hash": "315f5bdb76d078c43b8ac0064e4a0164612b1fce77c869345bfc94c75894edd3"
  }
]
```

### `typedetection_detect`

Returns a single `TypeResult` object:

```json
{
  "MimeType": "text/plain",
  "Extension": "txt",
  "Type": "Text"
}
```

| Field       | Type   | Description                                  |
|-------------|--------|----------------------------------------------|
| `MimeType`  | string | Detected MIME type.                          |
| `Extension` | string | Detected file extension.                     |
| `Type`      | enum   | Detected document type (`DocumentTypeEnum`). |

---

## Errors

Protocol-level problems (malformed JSON-RPC, an unknown method, a request before `initialize`) are
returned as JSON-RPC 2.0 error objects, for example:

```json
{ "jsonrpc": "2.0", "id": 3, "error": { "code": -32601, "message": "Method not found" } }
```

Tool failures are returned as a normal MCP tool result with `isError: true`, as the MCP
specification prescribes, so the calling model can see and correct them:

```json
{ "jsonrpc": "2.0", "id": 3, "result": { "isError": true, "content": [ { "type": "text", "text": "..." } ] } }
```

Over HTTP, `tools/call` arguments are validated against each tool's input schema before the tool
runs. A missing required argument, or an argument of the wrong type (including `null` for an
optional string such as `contentType`; omit the argument instead), returns an `isError` tool result
whose text names the argument. An exception thrown inside a tool (for example invalid base64 or an
unreachable REST server) is also returned as an `isError` result; its details are written to the
MCP server log.

Common causes:

- **Missing `data`** — the required `data` argument was not supplied.
- **Invalid base64** — `data` was not a valid base64 string.
- **Upstream failure** — the DocumentAtom REST server is unreachable or returned an error.

---

## Compatibility notes and caveats

- **No stdio transport.** This server exposes HTTP, TCP, and WebSocket transports only. Clients
  that can only spawn a local stdio process cannot launch it directly; point them at the HTTP or
  WebSocket endpoint instead.
- **HTTP transport shape.** The HTTP transport serves JSON-RPC on `/rpc` and server-sent events on
  `/events` (two paths), which differs from the single-endpoint "streamable HTTP" shape some MCP
  clients expect. Depending on the client, HTTP/WebSocket connectivity may require the client to
  support a URL-based (remote) MCP server. Verify connectivity after installing.
- **The REST server must be running.** The MCP server proxies to `DocumentAtom.Server`; ensure
  `DocumentAtom.Endpoint` points at a reachable REST endpoint.
- **Tool names use underscores.** Since v3.3.0 the tools are named `<type>_process` (plus
  `image_ocr` and `typedetection_detect`). Earlier releases used slash-separated names such as
  `csv/process`, which current MCP tooling rejects; update any client that hard-codes tool names.
- **Binary payloads are base64.** All file inputs are base64-encoded in `data`, and binary atom
  outputs (`Binary`, hash fields) are base64-encoded in responses.
