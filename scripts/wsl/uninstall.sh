#!/usr/bin/env sh
# Removes DocumentAtom MCP server integration from WSL 1. WSL 1 counterpart of
# scripts/linux/uninstall.sh, which does the work once the environment is fixed (Windows PATH
# entries dropped, Linux .NET used). Only the configs inside this distro are changed; run
# scripts\windows\uninstall.cmd from Windows for tools that run on Windows.
#
# Usage: ./uninstall.sh [--targets=all|<csv>]   (same options as scripts/linux)
set -eu
SCRIPT_DIR="$(CDPATH='' cd -- "$(dirname -- "$0")" && pwd)"
REPO_ROOT="$(CDPATH='' cd -- "$SCRIPT_DIR/../.." && pwd)"
WSL_FLAVOR=wsl    # "wsl2" in scripts/wsl2
# shellcheck source-path=SCRIPTDIR source=_wsl-common.sh
. "$SCRIPT_DIR/_wsl-common.sh"

case " $* " in *" -h "*|*" --help "*) wsl_run "$SCRIPT_DIR/../linux/uninstall.sh" --help ;; esac

wsl_require
wsl_sanitize_path
wsl_check_repo "$REPO_ROOT"
wsl_require_cmd dotnet "Install the .NET 8 (or later) SDK inside WSL: https://learn.microsoft.com/dotnet/core/install/linux"

wsl_log "Removing DocumentAtom from the tools inside this WSL distro."
wsl_log "For tools that run on Windows, run scripts\\windows\\uninstall.cmd from Windows as well."
wsl_run "$SCRIPT_DIR/../linux/uninstall.sh" "$@"
