#!/usr/bin/env bash
#
# Installs DocumentAtom MCP server integration into supported AI coding tools.
#
# Registers the DocumentAtom MCP server (HTTP JSON-RPC transport) with the per-user
# (global) configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux.
#
# This is a thin wrapper around the DocumentAtom.McpServer '--install' command, which
# performs the actual configuration edits. If a compiled binary cannot be found, the
# script builds it first (requires the .NET SDK).
#
# Usage:
#   ./install.sh [--targets=all|<csv>] [--endpoint=<url>]
#
# Examples:
#   ./install.sh
#   ./install.sh --targets=claude,cursor
#   ./install.sh --targets=mux --endpoint=http://localhost:8200/rpc
#
# Valid targets: claude, codex, gemini, cursor, mux (or 'all').

set -euo pipefail

TARGETS="all"
ENDPOINT="http://localhost:8200/rpc"

while [ $# -gt 0 ]; do
  case "$1" in
    --targets=*)  TARGETS="${1#*=}" ;;
    --targets)    TARGETS="${2:?--targets requires a value}"; shift ;;
    --endpoint=*) ENDPOINT="${1#*=}" ;;
    --endpoint)   ENDPOINT="${2:?--endpoint requires a value}"; shift ;;
    -h|--help)
      grep '^#' "$0" | grep -v '^#!' | sed 's/^# \{0,1\}//'
      exit 0 ;;
    *)
      echo "Unknown argument: $1" >&2
      echo "Run '$0 --help' for usage." >&2
      exit 1 ;;
  esac
  shift
done

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=_common.sh
. "$SCRIPT_DIR/_common.sh"

invoke_documentatom_mcp "install" "$TARGETS" "$ENDPOINT"
