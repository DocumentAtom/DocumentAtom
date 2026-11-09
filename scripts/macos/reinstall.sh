#!/usr/bin/env bash
#
# Reinstalls DocumentAtom MCP server integration: runs uninstall.sh, then install.sh.
#
# Use it to refresh the 'documentatom' entry (for example after changing the endpoint)
# in the per-user configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor, and mux.
# A failed uninstall (for example, nothing registered yet) is reported and the install
# still runs.
#
# Usage:
#   ./reinstall.sh [--targets=all|<csv>] [--endpoint=<url>]
#
# Examples:
#   ./reinstall.sh
#   ./reinstall.sh --targets=claude,cursor --endpoint=http://localhost:8200/rpc
#
# Valid targets: claude, codex, gemini, cursor, mux (or 'all').

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
UNINSTALL_ARGS=()

# Only --targets applies to uninstall; validate everything before changing anything.
args=("$@")
i=0
while [ $i -lt ${#args[@]} ]; do
  case "${args[$i]}" in
    --targets=*)  UNINSTALL_ARGS+=("${args[$i]}") ;;
    --targets)    UNINSTALL_ARGS+=("--targets=${args[$((i + 1))]:?--targets requires a value}"); i=$((i + 1)) ;;
    --endpoint=*) ;;
    --endpoint)   : "${args[$((i + 1))]:?--endpoint requires a value}"; i=$((i + 1)) ;;
    -h|--help)
      grep '^#' "$0" | grep -v '^#!' | sed 's/^# \{0,1\}//'
      exit 0 ;;
    *)
      echo "Unknown argument: ${args[$i]}" >&2
      echo "Run '$0 --help' for usage." >&2
      exit 1 ;;
  esac
  i=$((i + 1))
done

echo "==> Uninstalling DocumentAtom MCP integration"
if ! bash "$SCRIPT_DIR/uninstall.sh" ${UNINSTALL_ARGS[@]+"${UNINSTALL_ARGS[@]}"}; then
  echo "WARNING: Uninstall reported an error; continuing with install." >&2
fi

echo "==> Installing DocumentAtom MCP integration"
exec bash "$SCRIPT_DIR/install.sh" "$@"
