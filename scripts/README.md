# DocumentAtom MCP install scripts

One-command scripts to register (or remove) the **DocumentAtom MCP server** with common AI coding
tools: **Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux**.

Each script targets the tool's **per-user (global)** configuration and registers the DocumentAtom
MCP server (HTTP JSON-RPC transport) under the name **`documentatom`**.

```
scripts/
├── windows/    install.ps1 · uninstall.ps1 · reinstall.ps1 · install.cmd · uninstall.cmd · reinstall.cmd
├── macos/      install.sh  · uninstall.sh  · reinstall.sh
├── linux/      install.sh  · uninstall.sh  · reinstall.sh
├── wsl/        install.sh  · uninstall.sh  · reinstall.sh  · _wsl-common.sh   (WSL 1)
└── wsl2/       install.sh  · uninstall.sh  · reinstall.sh  · _wsl-common.sh   (WSL 2)
```

Every script is self-contained and does the same steps on every OS: check for .NET, build the
server, then register or remove the entry. `install` adds the `documentatom` entry (running it again
replaces it), `uninstall` removes it and succeeds when nothing is registered, and `reinstall` removes
the entry and adds it again with the given options. Unknown options, unknown targets and a
non-`http(s)://` endpoint are rejected before anything is built or changed; `-h`/`--help` prints
usage. Each script refuses to run on the wrong OS. The Windows `.cmd` files are launch shims that run the
same-name `.ps1` next to them with the same arguments. Nothing is
installed as a dotnet tool. `scripts/tests/test-scripts.py` checks all of this.

## How it works

The scripts run the `DocumentAtom.McpServer --install` / `--uninstall` command, which performs the
actual configuration edits (robust JSON and TOML handling that preserves your existing settings).
With the .NET SDK installed they first build the server (Release; `net10.0` with a .NET 10 SDK,
otherwise `net8.0`) and run that build. A failed build stops `install` and `reinstall`;
`uninstall` warns and removes the entry with an existing build instead. Without an SDK they run an
existing build under `src/DocumentAtom.McpServer/bin` that the installed runtime can run.

> Doing the config edits inside the (already tested) .NET tool — rather than reimplementing JSON
> and TOML editing separately in PowerShell and Bash — guarantees identical, safe behavior across
> all platforms, including edits to large config files such as Claude Code's `~/.claude.json`.

## Prerequisites

- **.NET 8 or 10 SDK** (recommended) — lets the scripts build the server. If only the .NET 8 or 10
  **runtime** is present, a compiled `DocumentAtom.McpServer.dll` for that runtime must already
  exist under `src/DocumentAtom.McpServer/bin/<Release|Debug>/<net8.0|net10.0>/`.
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

(Use `scripts/macos/…` on macOS; the scripts take the same options and refuse to run on the wrong OS.)

### WSL / WSL 2

Use `scripts/wsl2/…` on WSL 2 and `scripts/wsl/…` on WSL 1, with the same options as Linux
(`install.sh`, `uninstall.sh`, `reinstall.sh`); each refuses to run on the other WSL version. They
handle the usual WSL problems: Windows PATH entries are dropped so the Linux `dotnet` is used (a
Windows-only `dotnet` is reported), `~/.dotnet` is used when no other Linux .NET is installed, and
CRLF checkouts are caught with a clear error. They configure the tools **inside the distro**
(`~/.claude.json`, `~/.cursor/mcp.json`, ... in your WSL home); for tools that run on Windows, also
run `scripts\windows\install.cmd` from Windows. On WSL 2 (NAT networking), when no `--endpoint` is
given, the default `localhost` endpoint is rewritten to the Windows host address if only Windows
answers there; set `WSL_NO_HOST_REWRITE=1` to keep it. An endpoint you pass is used as given. WSL 1
shares the Windows network, so nothing is rewritten there. For best performance clone the repository into the
Linux file system (for example `~/src`) rather than `/mnt/c`.

## Options

| Option                | Windows                     | macOS / Linux                | Default                      |
|-----------------------|-----------------------------|------------------------------|------------------------------|
| Targets               | `-Targets claude,cursor`    | `--targets=claude,cursor`    | `all`                        |
| Endpoint (install, reinstall) | `-Endpoint <url>`           | `--endpoint=<url>`           | `http://localhost:8200/rpc`  |

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
