#!/usr/bin/env sh
# Installs DocumentAtom MCP server integration from WSL 1. WSL 1 counterpart of
# scripts/linux/install.sh, which does the work once the environment is fixed: Windows PATH
# entries are dropped so the Linux .NET SDK/runtime is used, and the configs written are the
# ones inside this distro (~/.claude.json, ~/.codex/config.toml, ~/.cursor/mcp.json, ...).
# WSL 1 shares localhost with Windows, so a server running on Windows answers at localhost as is.
#
# Usage: ./install.sh [--targets=all|<csv>] [--endpoint=<url>]   (same options as scripts/linux)
set -eu
SCRIPT_DIR="$(CDPATH='' cd -- "$(dirname -- "$0")" && pwd)"
REPO_ROOT="$(CDPATH='' cd -- "$SCRIPT_DIR/../.." && pwd)"
WSL_FLAVOR=wsl    # "wsl2" in scripts/wsl2
# shellcheck source-path=SCRIPTDIR source=_wsl-common.sh
. "$SCRIPT_DIR/_wsl-common.sh"

case " $* " in *" -h "*|*" --help "*) wsl_run "$SCRIPT_DIR/../linux/install.sh" --help ;; esac

wsl_require
wsl_sanitize_path
wsl_check_repo "$REPO_ROOT"
wsl_require_cmd dotnet "Install the .NET 8 (or later) SDK inside WSL: https://learn.microsoft.com/dotnet/core/install/linux"

# Rebuild the argument list with the endpoint resolved for WSL networking.
endpoint="http://localhost:8200/rpc"
n=$#
i=0
want_value=0
while [ "$i" -lt "$n" ]; do
    a="$1"; shift; i=$((i + 1))
    if [ "$want_value" = 1 ]; then
        endpoint="$a"; want_value=0; continue
    fi
    case "$a" in
        --endpoint=*) endpoint="${a#*=}" ;;
        --endpoint) want_value=1 ;;
        *) set -- "$@" "$a" ;;
    esac
done
[ "$want_value" = 0 ] || wsl_die "--endpoint requires a value"
resolved="$(wsl_resolve_local_url "$endpoint")"
if [ "$resolved" != "$endpoint" ]; then
    wsl_log "Note: the Windows host address can change when Windows restarts; rerun this script"
    wsl_log "(or reinstall.sh) if the connection stops working."
fi

wsl_log "Registering DocumentAtom with the tools inside this WSL distro (endpoint: $resolved)."
wsl_log "For tools that run on Windows (Claude Code or Cursor on Windows, for example), run"
wsl_log "scripts\\windows\\install.cmd from Windows as well."
wsl_run "$SCRIPT_DIR/../linux/install.sh" "$@" "--endpoint=$resolved"
