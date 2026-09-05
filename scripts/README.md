# DocumentAtom MCP install scripts

One-command scripts to register (or remove) the **DocumentAtom MCP server** with common AI coding
tools: **Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux**.

Each script targets the tool's **per-user (global)** configuration and registers the DocumentAtom
MCP server (HTTP JSON-RPC transport) under the name **`documentatom`**.

```
scripts/
├── windows/    install.ps1 · uninstall.ps1 · install.cmd · uninstall.cmd · _common.ps1
├── macos/      install.sh  · uninstall.sh  · _common.sh
└── linux/      install.sh  · uninstall.sh  · _common.sh
```

## How it works

These scripts are thin wrappers around the `DocumentAtom.McpServer --install` / `--uninstall`
command, which performs the actual configuration edits (robust JSON and TOML handling that
preserves your existing settings). The scripts locate a compiled `DocumentAtom.McpServer.dll`,
build it first if the .NET SDK is available (so you never run a stale binary), and then invoke it.

> Doing the config edits inside the (already tested) .NET tool — rather than reimplementing JSON
> and TOML editing separately in PowerShell and Bash — guarantees identical, safe behavior across
> all platforms, including edits to large config files such as Claude Code's `~/.claude.json`.

## Prerequisites

- **.NET SDK** (recommended) — lets the scripts build the server automatically. If only the .NET
  **runtime** is present, a compiled `DocumentAtom.McpServer.dll` must already exist under
  `src/DocumentAtom.McpServer/bin/…`.
- The target tools do **not** need to be installed for their config to be written; the scripts
  create the per-user config file/directory if missing.

## Usage

### Windows (PowerShell)

```powershell
# Install into every supported tool
scripts\windows\install.ps1

# Install into specific tools
scripts\windows\install.ps1 -Targets claude,cursor

# Custom endpoint
scripts\windows\install.ps1 -Targets mux -Endpoint http://localhost:8200/rpc

# Uninstall
scripts\windows\uninstall.ps1
scripts\windows\uninstall.ps1 -Targets cursor
```

From `cmd.exe`, use the `.cmd` wrappers:

```bat
scripts\windows\install.cmd -Targets claude,cursor
scripts\windows\uninstall.cmd
```

> If PowerShell blocks the script, run it via the `.cmd` wrapper (which sets
> `-ExecutionPolicy Bypass`) or start PowerShell with `-ExecutionPolicy Bypass`.

### macOS / Linux (Bash)

```bash
# Make executable once (if needed)
chmod +x scripts/linux/*.sh        # or scripts/macos/*.sh

# Install into every supported tool
scripts/linux/install.sh

# Install into specific tools
scripts/linux/install.sh --targets=claude,cursor

# Custom endpoint
scripts/linux/install.sh --targets=mux --endpoint=http://localhost:8200/rpc

# Uninstall
scripts/linux/uninstall.sh
scripts/linux/uninstall.sh --targets=cursor

# Help
scripts/linux/install.sh --help
```

(Use `scripts/macos/…` on macOS — the scripts are identical.)

## Options

| Option                | Windows                     | macOS / Linux                | Default                      |
|-----------------------|-----------------------------|------------------------------|------------------------------|
| Targets               | `-Targets claude,cursor`    | `--targets=claude,cursor`    | `all`                        |
| Endpoint (install)    | `-Endpoint <url>`           | `--endpoint=<url>`           | `http://localhost:8200/rpc`  |

Valid targets: `claude`, `codex`, `gemini`, `cursor`, `mux`, or `all`.

## What gets written

| Tool         | Config file (per user)                            | Entry                                                                          |
|--------------|---------------------------------------------------|-------------------------------------------------------------------------------|
| Claude Code  | `~/.claude.json`                                  | `mcpServers.documentatom = { "type": "http", "url": "<endpoint>" }`           |
| OpenAI Codex | `~/.codex/config.toml`                            | `[mcp_servers.documentatom]` with `url = "<endpoint>"`                        |
| Gemini CLI   | `~/.gemini/settings.json`                         | `mcpServers.documentatom = { "httpUrl": "<endpoint>" }`                       |
| Cursor       | `~/.cursor/mcp.json`                              | `mcpServers.documentatom = { "url": "<endpoint>" }`                           |
| mux          | `~/.mux/mcp-servers.json` (or `$MUX_CONFIG_DIR`)  | `servers[]` entry `{ "name": "documentatom", "transport": "http", "url": "<base>", "mcpPath": "<path>" }` |

Existing entries are preserved; only the `documentatom` entry is added or removed.

## Notes

- The DocumentAtom MCP server exposes **HTTP/TCP/WebSocket** transports (no stdio). The scripts
  register the **HTTP** endpoint. Depending on the tool's MCP client, remote/URL-based connectivity
  may need to be supported — verify the connection after installing. See
  [`../MCP_API.md`](../MCP_API.md).
- The MCP server proxies to the DocumentAtom REST server (`DocumentAtom.Server`, default
  `http://localhost:8000`) — make sure it is running for the tools to actually work.
- After editing config, restart the target tool so it reloads its MCP server list.
