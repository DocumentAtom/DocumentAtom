#!/usr/bin/env bash
# Register the DocumentAtom MCP server (HTTP JSON-RPC transport) as "documentatom" in the per-user
# configuration of Claude Code, OpenAI Codex, Gemini CLI, Cursor and mux. Running it again replaces
# the documentatom entry; other entries in each config file are kept.
# This builds src/DocumentAtom.McpServer (Release, net10.0 with a .NET 10 SDK, otherwise net8.0)
# when the .NET SDK is installed, then runs its --install command, which edits the config files.
# Without an SDK it uses an existing build (from an earlier run) that the installed runtime can
# run. Nothing is installed as a dotnet tool.
# Usage: ./scripts/linux/install.sh [--targets=all|<csv>] [--endpoint=<url>]
#   --targets   claude, codex, gemini, cursor, mux or all (default: all); "--targets <csv>" also works
#   --endpoint  HTTP JSON-RPC endpoint to register (default: http://localhost:8200/rpc)
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../.." && pwd)"
die() { echo "Error: $*" >&2; exit 1; }
# Prints the header comment above as usage text.
usage() { awk 'NR > 1 && /^#/ { sub(/^# ?/, ""); print; next } NR > 1 { exit }' "$0"; }

if [ "$(uname -s)" != Linux ]; then
    die "This script is for Linux. Use scripts/macos on macOS or scripts\\windows on Windows."
fi
# A Microsoft kernel alone is not WSL: Docker Desktop containers have one too, but no WSL interop.
if grep -qi microsoft /proc/version 2>/dev/null \
    && { [ -n "${WSL_DISTRO_NAME:-}" ] || [ -d /run/WSL ] || ls /proc/sys/fs/binfmt_misc/WSLInterop* >/dev/null 2>&1; }; then
    die "This is WSL. Use the same script in scripts/wsl2 (WSL 2) or scripts/wsl (WSL 1); they handle the Windows PATH, line endings and the Windows host address."
fi

# Native-library checks and installs: the scripts/wsl*/_wsl-common.sh "Native libraries" section
# with native_ names (generated from it, same wording).
native_log() { printf '%s\n' "$*" >&2; }
native_die() { printf 'Error: %s\n' "$*" >&2; exit 1; }

# --- Native libraries -------------------------------------------------------------------------------
# Minimal and server installs often lack these libraries. Building never loads them, so
# without a check the failure only shows when the app starts, as "Unable to load shared library ...".

# Libraries a .NET GUI app built on Avalonia 12 loads on Linux: the sonames inside Avalonia.X11.dll
# (12.1.x) plus SkiaSharp's fontconfig. libGL, GTK 3 and Vulkan are optional (software rendering and
# managed dialogs are the fallbacks). Each entry is soname:Debian/Ubuntu package:Fedora package.
# shellcheck disable=SC2034 # used by the scripts that source this file
NATIVE_AVALONIA_LIBS="libX11.so.6:libx11-6:libX11 libICE.so.6:libice6:libICE libSM.so.6:libsm6:libSM libXext.so.6:libxext6:libXext libXrandr.so.2:libxrandr2:libXrandr libXi.so.6:libxi6:libXi libXcursor.so.1:libxcursor1:libXcursor libXfixes.so.3:libxfixes3:libXfixes libfontconfig.so.1:libfontconfig1:fontconfig"

# Prints the ldconfig cache (ldconfig -p), or nothing when there is no ldconfig to ask.
# NATIVE_LDCONFIG overrides the ldconfig command (used by tests).
native_ldconfig_cache() {
    _wl_ldc="${NATIVE_LDCONFIG:-}"
    if [ -z "$_wl_ldc" ]; then
        for _wl_c in ldconfig /sbin/ldconfig /usr/sbin/ldconfig; do
            if command -v "$_wl_c" >/dev/null 2>&1; then _wl_ldc="$_wl_c"; break; fi
        done
    fi
    [ -n "$_wl_ldc" ] || return 0
    "$_wl_ldc" -p 2>/dev/null || true
}

# True when the dynamic linker has <soname> (exact name, or a prefix ending in a dot such as
# "libicuuc.so.") for this machine's architecture. Usage: native_have_lib <cache> <soname>
native_have_lib() {
    case "$(uname -m)" in
        x86_64|amd64) _wl_arch=x86-64 ;;
        aarch64|arm64) _wl_arch=AArch64 ;;
        *) _wl_arch="" ;;
    esac
    printf '%s\n' "$1" | awk -v so="$2" -v arch="$_wl_arch" '
        { name = $1 }
        (name == so || (substr(so, length(so)) == "." && index(name, so) == 1)) && (arch == "" || index($0, arch)) { found = 1 }
        END { exit !found }'
}

# Prints the install command for the given Debian and Fedora package lists.
# Usage: native_install_hint "<deb packages>" "<rpm packages>"
native_install_hint() {
    if command -v apt-get >/dev/null 2>&1; then
        printf '  sudo apt install %s\n' "$1"
    elif command -v dnf >/dev/null 2>&1; then
        printf '  sudo dnf install %s\n' "$2"
    else
        printf '  Debian/Ubuntu: sudo apt install %s\n  Fedora:        sudo dnf install %s\n' "$1" "$2"
    fi
}


# .NET needs ICU (libicu) for globalization: the SDK itself and every app that does not run with
# invariant globalization. dotnet-install.sh and self-contained apps do not bring it. Exits with the
# install command when it is missing; does nothing when DOTNET_SYSTEM_GLOBALIZATION_INVARIANT is set
# to 1/true, or when there is no ldconfig to ask.
native_check_icu() {
    case "${DOTNET_SYSTEM_GLOBALIZATION_INVARIANT:-}" in 1|true|TRUE|True) return 0 ;; esac
    _wl_cache="$(native_ldconfig_cache)"
    [ -n "$_wl_cache" ] || return 0
    native_have_lib "$_wl_cache" "libicuuc.so." && return 0
    _wl_icu=""
    if command -v apt-cache >/dev/null 2>&1; then
        _wl_icu="$(apt-cache pkgnames libicu 2>/dev/null | awk '/^libicu[0-9]+(t64)?$/' | sort -V | tail -n 1 || true)"
    fi
    native_log "Error: .NET needs ICU (libicu), and this system does not have it. Without it dotnet fails with"
    native_log "\"Couldn't find a valid ICU package installed on the system\". Install it with:"
    native_log "$(native_install_hint "${_wl_icu:-libicu-dev}" libicu)"
    exit 1
}


# Installs the system packages an app needs, when they are missing: install
# scripts call it so the app does not fail later at startup. Uses apt-get (Debian/Ubuntu) or dnf
# (Fedora), through sudo unless running as root. INSTALL_SYSTEM_PACKAGES=0 turns it into a report.
# Usage: native_install_deps "<app name>" [icu] [gui] [clipboard] [browser]
#   icu        ICU for .NET (required)                 gui      the Avalonia libraries (required)
#   clipboard  xclip when no clipboard tool exists    browser  wslu + xdg-utils, to open links in Windows
# Required packages that cannot be installed are fatal; optional ones only warn.
native_install_deps() {
    _wd_app="$1"; shift
    _wd_cache="$(native_ldconfig_cache)"
    _wd_req_deb=""; _wd_req_rpm=""; _wd_opt_deb=""; _wd_opt_rpm=""; _wd_apt_icu=0
    for _wd_want in "$@"; do
        case "$_wd_want" in
            icu)
                case "${DOTNET_SYSTEM_GLOBALIZATION_INVARIANT:-}" in 1|true|TRUE|True) continue ;; esac
                if [ -n "$_wd_cache" ] && ! native_have_lib "$_wd_cache" "libicuuc.so."; then
                    _wd_apt_icu=1; _wd_req_rpm="$_wd_req_rpm libicu"
                fi ;;
            gui)
                [ -n "$_wd_cache" ] || continue
                for _wd_e in $NATIVE_AVALONIA_LIBS; do
                    native_have_lib "$_wd_cache" "${_wd_e%%:*}" && continue
                    _wd_r="${_wd_e#*:}"
                    _wd_req_deb="$_wd_req_deb ${_wd_r%%:*}"; _wd_req_rpm="$_wd_req_rpm ${_wd_r#*:}"
                done ;;
            clipboard)
                if ! command -v wl-copy >/dev/null 2>&1 && ! command -v xclip >/dev/null 2>&1 && ! command -v xsel >/dev/null 2>&1; then
                    _wd_opt_deb="$_wd_opt_deb xclip"; _wd_opt_rpm="$_wd_opt_rpm xclip"
                fi ;;
            browser)
                if ! command -v wslview >/dev/null 2>&1; then _wd_opt_deb="$_wd_opt_deb wslu"; _wd_opt_rpm="$_wd_opt_rpm wslu"; fi
                if ! command -v xdg-open >/dev/null 2>&1; then _wd_opt_deb="$_wd_opt_deb xdg-utils"; _wd_opt_rpm="$_wd_opt_rpm xdg-utils"; fi ;;
            *) native_die "native_install_deps: unknown package group: $_wd_want" ;;
        esac
    done
    [ -n "$_wd_req_deb$_wd_req_rpm$_wd_opt_deb$_wd_opt_rpm" ] || [ "$_wd_apt_icu" = 1 ] || return 0

    if command -v apt-get >/dev/null 2>&1; then _wd_pm=apt; elif command -v dnf >/dev/null 2>&1; then _wd_pm=dnf; else _wd_pm=""; fi
    if [ "$(id -u)" = 0 ]; then _wd_sudo=""; elif command -v sudo >/dev/null 2>&1; then _wd_sudo=sudo; else _wd_sudo=none; fi
    if [ "${INSTALL_SYSTEM_PACKAGES:-1}" = 0 ] || [ -z "$_wd_pm" ] || [ "$_wd_sudo" = none ]; then
        _wd_icu_deb=""; [ "$_wd_apt_icu" = 1 ] && _wd_icu_deb=" libicu-dev"
        native_log "$_wd_app needs system packages that are not installed, and they were not installed automatically"
        native_log "(INSTALL_SYSTEM_PACKAGES=0, no apt-get/dnf, or no sudo). Install them with:"
        native_log "$(native_install_hint "${_wd_req_deb# }$_wd_icu_deb$_wd_opt_deb" "${_wd_req_rpm# }$_wd_opt_rpm")"
        if [ -n "$_wd_req_deb$_wd_req_rpm" ] || [ "$_wd_apt_icu" = 1 ]; then exit 1; fi
        return 0
    fi

    if [ "$_wd_pm" = apt ]; then
        native_log "Installing the system packages $_wd_app needs (apt-get; sudo may ask for your password)..."
        # shellcheck disable=SC2086 # empty or "sudo"
        $_wd_sudo apt-get update -qq || native_die "apt-get update failed."
        if [ "$_wd_apt_icu" = 1 ]; then
            _wd_icu="$(apt-cache pkgnames libicu 2>/dev/null | awk '/^libicu[0-9]+(t64)?$/' | sort -V | tail -n 1 || true)"
            _wd_req_deb="$_wd_req_deb ${_wd_icu:-libicu-dev}"
        fi
        if [ -n "$_wd_req_deb" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            DEBIAN_FRONTEND=noninteractive $_wd_sudo apt-get install -y -q $_wd_req_deb \
                || native_die "Could not install the required packages:$_wd_req_deb"
        fi
        if [ -n "$_wd_opt_deb" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            DEBIAN_FRONTEND=noninteractive $_wd_sudo apt-get install -y -q $_wd_opt_deb \
                || native_log "Warning: could not install the optional packages:$_wd_opt_deb (copy to clipboard or opening links may not work)."
        fi
    else
        native_log "Installing the system packages $_wd_app needs (dnf; sudo may ask for your password)..."
        if [ -n "$_wd_req_rpm" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            $_wd_sudo dnf install -y $_wd_req_rpm || native_die "Could not install the required packages:$_wd_req_rpm"
        fi
        if [ -n "$_wd_opt_rpm" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            $_wd_sudo dnf install -y $_wd_opt_rpm \
                || native_log "Warning: could not install the optional packages:$_wd_opt_rpm (copy to clipboard or opening links may not work)."
        fi
    fi
    native_log "System packages for $_wd_app are installed."
}

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
        *) die "Unexpected argument: $1 (run scripts/linux/install.sh --help for usage)" ;;
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

echo "DocumentAtom MCP install (Linux)"
echo "[1/3] Checking for .NET..."
# .NET (the SDK and the server) needs ICU. Install it now if it is missing (INSTALL_SYSTEM_PACKAGES=0
# only reports it), before the dotnet check, whose ICU check would stop the script first.
native_install_deps "DocumentAtom.McpServer" icu
command -v dotnet >/dev/null 2>&1 || die "dotnet was not found on PATH. Install .NET 8 or later: https://learn.microsoft.com/dotnet/core/install/linux"
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
