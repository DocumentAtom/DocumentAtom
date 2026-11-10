#!/usr/bin/env bash
# Register the DocumentAtom MCP server (HTTP JSON-RPC transport) as "documentatom" in the per-user
# configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor and mux. Running it again replaces
# the documentatom entry; other entries in each config file are kept.
# This builds src/DocumentAtom.McpServer (Release, net10.0 with a .NET 10 SDK, otherwise net8.0)
# when the .NET SDK is installed, then runs its --install command, which edits the config files.
# Without an SDK it uses an existing build (from an earlier run) that the installed runtime can
# run. Nothing is installed as a dotnet tool.
# Usage: ./scripts/macos/install.sh [--targets=all|<csv>] [--endpoint=<url>]
#   --targets   claude, codex, gemini, cursor, mux or all (default: all); "--targets <csv>" also works
#   --endpoint  HTTP JSON-RPC endpoint to register (default: http://localhost:8200/rpc)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
die() { echo "Error: $*" >&2; exit 1; }
# Prints the header comment above as usage text.
usage() { awk 'NR > 1 && /^#/ { sub(/^# ?/, ""); print; next } NR > 1 { exit }' "$0"; }

if [ "$(uname -s)" != Darwin ]; then
    die "This script is for macOS. Use scripts/linux on Linux, scripts/wsl2 or scripts/wsl under WSL, or scripts\\windows on Windows."
fi

DEFAULT_ENDPOINT="http://localhost:8200/rpc"
ENDPOINT=""
TARGETS=all
while [ "$#" -gt 0 ]; do
    case "$1" in
        -h|--help) usage; exit 0 ;;
        --targets=*) TARGETS="${1#*=}" ;;
        --targets) [ "$#" -ge 2 ] || die "--targets requires a value."; TARGETS="$2"; shift ;;
        --endpoint=*) ENDPOINT="${1#*=}"; [ -n "$ENDPOINT" ] || die "--endpoint requires a value." ;;
        --endpoint) [ "$#" -ge 2 ] || die "--endpoint requires a value."; ENDPOINT="$2"; shift ;;
        *) die "Unexpected argument: $1 (run scripts/macos/install.sh --help for usage)" ;;
    esac
    shift
done

# Validate the targets before anything is built or changed (the server would skip unknown names).
TARGETS="$(printf '%s' "$TARGETS" | tr '[:upper:]' '[:lower:]' | tr -d '[:space:]')"
[ -n "$TARGETS" ] || die "--targets requires a value."
TARGET_COUNT=0
IFS=, read -r -a TARGET_LIST <<< "$TARGETS,"
for target in "${TARGET_LIST[@]}"; do
    case "$target" in
        '') ;;
        claude|codex|gemini|cursor|mux|all) TARGET_COUNT=$((TARGET_COUNT + 1)) ;;
        *) die "Unknown target '$target'. Valid targets: claude, codex, gemini, cursor, mux, all." ;;
    esac
done
[ "$TARGET_COUNT" -gt 0 ] || die "--targets requires at least one target."
if [ -n "$ENDPOINT" ]; then
    # Case-insensitive, as in the Windows script.
    case "$(printf '%s' "$ENDPOINT" | tr '[:upper:]' '[:lower:]')" in
        http://*|https://*) ;;
        *) die "--endpoint must be an http:// or https:// URL (got '$ENDPOINT')." ;;
    esac
else
    ENDPOINT="$DEFAULT_ENDPOINT"
fi

CSPROJ="$REPO_ROOT/src/DocumentAtom.McpServer/DocumentAtom.McpServer.csproj"
BIN_DIR="$REPO_ROOT/src/DocumentAtom.McpServer/bin"

echo "DocumentAtom MCP install (macOS)"
echo "[1/3] Checking for .NET..."
command -v dotnet >/dev/null 2>&1 || die "dotnet was not found on PATH. Install .NET 8 or later: https://learn.microsoft.com/dotnet/core/install/macos"
# Capture first: under 'set -o pipefail', 'dotnet --list-sdks | grep -q' can fail with SIGPIPE.
SDKS="$(dotnet --list-sdks 2>/dev/null || true)"
RUNTIMES="$(dotnet --list-runtimes 2>/dev/null || true)"

# Prints an existing DocumentAtom.McpServer.dll that the installed runtime can run: net10.0 before
# net8.0, Release before Debug. Fails when there is none.
find_server() {
    local frameworks="" framework config
    case "
$RUNTIMES" in *"
Microsoft.NETCore.App 10."*) frameworks="net10.0" ;; esac
    case "
$RUNTIMES" in *"
Microsoft.NETCore.App 8."*) frameworks="$frameworks net8.0" ;; esac
    for framework in $frameworks; do
        for config in Release Debug; do
            if [ -f "$BIN_DIR/$config/$framework/DocumentAtom.McpServer.dll" ]; then
                printf '%s\n' "$BIN_DIR/$config/$framework/DocumentAtom.McpServer.dll"
                return 0
            fi
        done
    done
    return 1
}

if [ -n "$SDKS" ]; then
    # Build one framework only: building both needs both SDKs.
    case "
$SDKS" in
        *"
1"[0-9]"."*) FRAMEWORK=net10.0 ;;
        *) FRAMEWORK=net8.0 ;;
    esac
    echo "[2/3] Building DocumentAtom.McpServer (Release, $FRAMEWORK)..."
    if dotnet build "$CSPROJ" -c Release -f "$FRAMEWORK" --nologo -v quiet; then
        SERVER="$BIN_DIR/Release/$FRAMEWORK/DocumentAtom.McpServer.dll"
        [ -f "$SERVER" ] || die "The build succeeded, but $SERVER was not found."
    else
        die "The build failed (see above). Nothing was changed."
    fi
else
    echo "[2/3] No .NET SDK found, so nothing is rebuilt; looking for an existing build..."
    SERVER="$(find_server)" || die "No DocumentAtom.McpServer.dll that this .NET runtime can run was found under $BIN_DIR. Install the .NET SDK so this script can build it."
fi
echo "Using $SERVER"
echo "[3/3] Registering 'documentatom' ($ENDPOINT) with: $TARGETS"
dotnet "$SERVER" "--install=$TARGETS" "--endpoint=$ENDPOINT" || die "Registration failed for one or more tools (see above)."
echo "Restart the tools so they reload their MCP servers. The MCP server must be running at $ENDPOINT."
