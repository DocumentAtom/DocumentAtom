#!/usr/bin/env sh
# Reinstalls DocumentAtom MCP server integration from WSL 2: runs uninstall.sh, then install.sh
# from this directory. WSL 2 counterpart of scripts/linux/reinstall.sh. A failed uninstall (for
# example, nothing registered yet) is reported and the install still runs.
#
# Usage: ./reinstall.sh [--targets=all|<csv>] [--endpoint=<url>]
set -eu
SCRIPT_DIR="$(CDPATH='' cd -- "$(dirname -- "$0")" && pwd)"
REPO_ROOT="$(CDPATH='' cd -- "$SCRIPT_DIR/../.." && pwd)"
WSL_FLAVOR=wsl2   # "wsl" in scripts/wsl
# shellcheck source-path=SCRIPTDIR source=_wsl-common.sh
. "$SCRIPT_DIR/_wsl-common.sh"

case " $* " in *" -h "*|*" --help "*) wsl_run "$SCRIPT_DIR/../linux/reinstall.sh" --help ;; esac

wsl_require
wsl_sanitize_path
wsl_check_repo "$REPO_ROOT"

# Only --targets applies to uninstall; validate everything before changing anything.
targets=""
want=""
for a in "$@"; do
    if [ -n "$want" ]; then
        [ "$want" = targets ] && targets="--targets=$a"
        want=""; continue
    fi
    case "$a" in
        --targets=*) targets="$a" ;;
        --targets) want=targets ;;
        --endpoint=*) ;;
        --endpoint) want=endpoint ;;
        *) wsl_die "Unknown argument: $a (run '$0 --help' for usage)" ;;
    esac
done
[ -z "$want" ] || wsl_die "--$want requires a value"

wsl_log "==> Uninstalling DocumentAtom MCP integration"
if [ -n "$targets" ]; then
    sh "$SCRIPT_DIR/uninstall.sh" "$targets" || wsl_log "WARNING: Uninstall reported an error; continuing with install."
else
    sh "$SCRIPT_DIR/uninstall.sh" || wsl_log "WARNING: Uninstall reported an error; continuing with install."
fi

wsl_log "==> Installing DocumentAtom MCP integration"
exec sh "$SCRIPT_DIR/install.sh" "$@"
