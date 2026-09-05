#!/usr/bin/env bash
#
# Removes DocumentAtom MCP server integration from supported AI coding tools.
#
# Removes the DocumentAtom MCP server entry from the per-user (global) configuration of
# Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux. Other entries are preserved.
#
# This is a thin wrapper around the DocumentAtom.McpServer '--uninstall' command. If a
# compiled binary cannot be found, the script builds it first (requires the .NET SDK).
#
# Usage:
#   ./uninstall.sh [--targets=all|<csv>]
#
# Examples:
#   ./uninstall.sh
#   ./uninstall.sh --targets=cursor
#
# Valid targets: claude, codex, gemini, cursor, mux (or 'all').

set -euo pipefail

TARGETS="all"

while [ $# -gt 0 ]; do
  case "$1" in
    --targets=*) TARGETS="${1#*=}" ;;
    --targets)   TARGETS="${2:?--targets requires a value}"; shift ;;
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

invoke_documentatom_mcp "uninstall" "$TARGETS"
