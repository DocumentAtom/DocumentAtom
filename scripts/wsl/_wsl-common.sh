#!/bin/sh
# Shared helpers for the scripts in scripts/wsl and scripts/wsl2. Source it; don't run it.
# Set WSL_FLAVOR to "wsl" (WSL 1) or "wsl2" before sourcing. The helpers fix the usual reasons
# scripts/linux fails under WSL:
#   - WSL appends the Windows PATH, so names such as claude, codex, gemini, node, npm and code can
#     resolve to Windows shims under /mnt/c. Windows entries are dropped; set WSL_KEEP_WINDOWS_PATH=1
#     to keep them.
#   - ~/.dotnet and ~/.dotnet/tools (user-local .NET installs and global tools) are often not on PATH.
#   - A repository cloned with Windows Git (core.autocrlf=true) has CRLF scripts that fail with
#     "bash\r: No such file or directory".
#   - A repository on /mnt/<drive> (DrvFs) has no executable bits, is slow, and can be locked by
#     Windows processes.
#   - systemd and desktop autostart entries do nothing at Windows login, so start-at-login uses a
#     hidden launcher in the Windows Startup folder that runs the command through wsl.exe.
#   - Under WSL 2 NAT networking, 127.0.0.1 inside the distro is not the Windows host.

WSL_ORIGINAL_PATH="$PATH"
# shellcheck disable=SC2034 # read by the scripts that source this file
WSL_ORIGINAL_DOTNET_ROOT="${DOTNET_ROOT:-}"

# Prints 1 for WSL 1, 2 for WSL 2, or 0 when not running under WSL. A Microsoft kernel alone is not
# enough: Docker Desktop containers on the WSL 2 backend have one too, but no WSL interop.
wsl_version() {
    if [ -z "${WSL_DISTRO_NAME:-}" ] && [ ! -d /run/WSL ] && ! ls /proc/sys/fs/binfmt_misc/WSLInterop* >/dev/null 2>&1; then
        echo 0; return 0
    fi
    _wsl_rel="$(uname -r 2>/dev/null || true)"
    case "$_wsl_rel" in
        *microsoft-standard*|*WSL2*) echo 2; return 0 ;;
        *Microsoft*) echo 1; return 0 ;;
    esac
    if grep -qi 'wsl2\|microsoft-standard' /proc/version 2>/dev/null; then echo 2
    elif grep -qi microsoft /proc/version 2>/dev/null; then echo 1
    else echo 0
    fi
}

wsl_log() { printf '%s\n' "$*" >&2; }
wsl_die() { printf 'Error: %s\n' "$*" >&2; exit 1; }

# Fails outside WSL, and when the WSL version doesn't match this directory (scripts/wsl is WSL 1,
# scripts/wsl2 is WSL 2). WSL_SKIP_CHECK=1 skips both checks.
wsl_require() {
    [ "${WSL_SKIP_CHECK:-0}" = 1 ] && return 0
    _wsl_v="$(wsl_version)"
    [ "$_wsl_v" != 0 ] || wsl_die "This script is for WSL. Use scripts/linux, scripts/macos or scripts\\windows instead."
    case "${WSL_FLAVOR:-wsl2}" in
        wsl) [ "$_wsl_v" = 1 ] || wsl_die "This distro runs WSL 2. Use the same script in scripts/wsl2 instead." ;;
        wsl2) [ "$_wsl_v" = 2 ] || wsl_die "This distro runs WSL 1. Use the same script in scripts/wsl instead." ;;
    esac
}

# Drops Windows (/mnt/<drive>/...) PATH entries and adds user-local .NET and tool directories.
wsl_sanitize_path() {
    if [ "${WSL_KEEP_WINDOWS_PATH:-0}" != 1 ]; then
        _wsl_new=""
        _wsl_ifs="$IFS"; IFS=:
        for _wsl_p in $PATH; do
            case "$_wsl_p" in
                /mnt/[a-zA-Z]/*|/mnt/[a-zA-Z]) continue ;;
            esac
            _wsl_new="${_wsl_new:+$_wsl_new:}$_wsl_p"
        done
        IFS="$_wsl_ifs"
        PATH="$_wsl_new"
    fi
    for _wsl_p in "$HOME/.local/bin" "$HOME/.dotnet/tools"; do
        [ -d "$_wsl_p" ] || continue
        case ":$PATH:" in *":$_wsl_p:"*) ;; *) PATH="$PATH:$_wsl_p" ;; esac
    done
    # A user-local .NET (dotnet-install.sh) is used only when no other Linux dotnet is on PATH.
    if ! command -v dotnet >/dev/null 2>&1 && [ -x "$HOME/.dotnet/dotnet" ]; then
        PATH="$HOME/.dotnet:$PATH"
        if [ -z "${DOTNET_ROOT:-}" ]; then DOTNET_ROOT="$HOME/.dotnet"; export DOTNET_ROOT; fi
    fi
    export PATH
}

# Fails with a helpful message when a Linux command is missing (after wsl_sanitize_path, Windows
# copies no longer count). Usage: wsl_require_cmd <name> [install hint]
wsl_require_cmd() {
    if command -v "$1" >/dev/null 2>&1; then
        if [ "$1" = dotnet ]; then wsl_check_icu; fi
        return 0
    fi
    _wsl_hint="${2:-Install it inside this WSL distro.}"
    if PATH="$WSL_ORIGINAL_PATH" command -v "$1" >/dev/null 2>&1 || PATH="$WSL_ORIGINAL_PATH" command -v "$1.exe" >/dev/null 2>&1; then
        wsl_die "'$1' is only installed on the Windows side. $_wsl_hint (Or run the scripts/windows version from Windows.)"
    fi
    wsl_die "'$1' was not found. $_wsl_hint"
}

# Warns about DrvFs checkouts and fails on CRLF scripts. Usage: wsl_check_repo <repo root>
wsl_check_repo() {
    case "$1" in
        /mnt/[a-zA-Z]/*)
            wsl_log "Warning: $1 is on a Windows drive (DrvFs). Builds are much slower there and"
            wsl_log "file locks from Windows processes can break them. Clone into the Linux file"
            wsl_log "system (for example ~/src) for best results." ;;
    esac
    _wsl_cr="$(printf '\r')"
    _wsl_bad="$(find "$1/scripts" -name '*.sh' -type f -exec grep -l "$_wsl_cr" {} + 2>/dev/null | head -n 5 || true)"
    if [ -n "$_wsl_bad" ]; then
        wsl_log "These scripts have Windows (CRLF) line endings and will not run under WSL:"
        wsl_log "$_wsl_bad"
        wsl_log "Commit or stash your changes first (the commands below discard uncommitted edits), then run:"
        wsl_die "git config core.autocrlf input && git rm -r --cached -q . && git reset --hard"
    fi
}

# Runs a script with the interpreter from its shebang, so missing executable bits (DrvFs) don't matter.
# Usage: wsl_run <script> [args...]
wsl_run() {
    _wsl_script="$1"; shift
    [ -f "$_wsl_script" ] || wsl_die "Missing script: $_wsl_script"
    case "$(head -n 1 "$_wsl_script")" in
        *bash*) exec bash "$_wsl_script" "$@" ;;
        *) exec sh "$_wsl_script" "$@" ;;
    esac
}

# True when systemd is PID 1 and a user manager is reachable (WSL 2 with [boot] systemd=true).
wsl_has_systemd() {
    [ "$(ps -p 1 -o comm= 2>/dev/null)" = systemd ] && command -v systemctl >/dev/null 2>&1 \
        && systemctl --user show-environment >/dev/null 2>&1
}

# Finds a Windows executable (cmd.exe, wsl.exe, ...) even after the PATH is sanitized.
wsl_win_exe() {
    _wsl_found="$(PATH="$WSL_ORIGINAL_PATH" command -v "$1" 2>/dev/null || true)"
    if [ -z "$_wsl_found" ]; then
        for _wsl_d in /mnt/c/Windows/System32 /mnt/c/Windows; do
            [ -x "$_wsl_d/$1" ] && { _wsl_found="$_wsl_d/$1"; break; }
        done
    fi
    [ -n "$_wsl_found" ] || wsl_die "Cannot find $1. Windows interop must be enabled ([interop] enabled=true in /etc/wsl.conf)."
    printf '%s\n' "$_wsl_found"
}

# Prints the Windows host's address as seen from WSL 2 NAT networking.
wsl_windows_host() {
    _wsl_gw="$(ip route show default 2>/dev/null | awk '/default/ {print $3; exit}' || true)"
    [ -n "$_wsl_gw" ] || _wsl_gw="$(awk '/^nameserver/ {print $2; exit}' /etc/resolv.conf 2>/dev/null || true)"
    printf '%s\n' "$_wsl_gw"
}

# Under WSL 2 NAT networking, rewrites a 127.0.0.1/localhost URL to the Windows host when nothing
# answers inside the distro but the Windows host does (a server running on Windows). Leaves the URL
# alone on WSL 1, with mirrored networking, when curl is missing, or when WSL_NO_HOST_REWRITE=1.
# Usage: url="$(wsl_resolve_local_url "$url")"
wsl_resolve_local_url() {
    _wsl_url="$1"
    [ "${WSL_NO_HOST_REWRITE:-0}" = 1 ] && { printf '%s\n' "$_wsl_url"; return 0; }
    [ "$(wsl_version)" = 2 ] || { printf '%s\n' "$_wsl_url"; return 0; }
    command -v curl >/dev/null 2>&1 || { printf '%s\n' "$_wsl_url"; return 0; }
    case "$_wsl_url" in
        *://127.0.0.1|*://127.0.0.1:*|*://127.0.0.1/*|*://localhost|*://localhost:*|*://localhost/*) ;;
        *) printf '%s\n' "$_wsl_url"; return 0 ;;
    esac
    if curl -s -o /dev/null --max-time 2 "$_wsl_url" 2>/dev/null; then printf '%s\n' "$_wsl_url"; return 0; fi
    _wsl_host="$(wsl_windows_host)"
    [ -n "$_wsl_host" ] || { printf '%s\n' "$_wsl_url"; return 0; }
    _wsl_alt="$(printf '%s\n' "$_wsl_url" | sed -e "s#://127\.0\.0\.1#://$_wsl_host#" -e "s#://localhost#://$_wsl_host#")"
    if curl -s -o /dev/null --max-time 2 "$_wsl_alt" 2>/dev/null; then
        wsl_log "Nothing answers at $_wsl_url inside WSL, but the Windows host does; using $_wsl_alt."
        wsl_log "(Set WSL_NO_HOST_REWRITE=1 to keep the original, or enable networkingMode=mirrored in .wslconfig.)"
        printf '%s\n' "$_wsl_alt"; return 0
    fi
    printf '%s\n' "$_wsl_url"
}

# Prints the launcher file name for <name> in this distro, so several distros can register the same
# command without replacing each other's launcher. Usage: wsl_launcher_file <name>
wsl_launcher_file() {
    printf '%s-wsl-%s.vbs\n' "$1" "$(printf '%s' "${WSL_DISTRO_NAME:-default}" | tr -c 'A-Za-z0-9._-' '_')"
}

# Removes a launcher written by an older version of these helpers (<name>-wsl.vbs, one name for every
# distro) when it belongs to this distro. Returns 0 when one was removed. Usage: wsl_startup_remove_legacy <dir> <name>
wsl_startup_remove_legacy() {
    _wsl_legacy="$1/$2-wsl.vbs"
    [ -f "$_wsl_legacy" ] || return 1
    grep -qF -- "-d \"\"${WSL_DISTRO_NAME:-}\"\"" "$_wsl_legacy" 2>/dev/null || return 1
    rm -f "$_wsl_legacy"
    wsl_log "Removed the older Windows Startup launcher: $_wsl_legacy"
}

# Prints the Windows Startup folder as a WSL path.
wsl_startup_dir() {
    _wsl_cmd="$(wsl_win_exe cmd.exe)"
    _wsl_appdata="$(cd /mnt/c 2>/dev/null || true; "$_wsl_cmd" /c 'echo %APPDATA%' 2>/dev/null | tr -d '\r' || true)"
    [ -n "$_wsl_appdata" ] || wsl_die "Could not read %APPDATA% from Windows."
    printf '%s/Microsoft/Windows/Start Menu/Programs/Startup\n' "$(wslpath -u "$_wsl_appdata")"
}

# Registers a command to start, hidden, at Windows login, inside this distro. The launcher keeps
# wsl.exe attached so the distro stays up while the command runs. No admin rights needed.
# Usage: wsl_startup_install <name> <command> [args...]   (name: letters, digits, dashes)
wsl_startup_install() {
    _wsl_name="$1"; shift
    [ -n "${WSL_DISTRO_NAME:-}" ] || wsl_die "WSL_DISTRO_NAME is not set; run this from a WSL shell."
    _wsl_dir="$(wsl_startup_dir)" || exit 1
    mkdir -p "$_wsl_dir"
    wsl_startup_remove_legacy "$_wsl_dir" "$_wsl_name" || true
    _wsl_args=""
    for _wsl_a in "$@"; do
        _wsl_q="$(printf '%s' "$_wsl_a" | sed "s/'/'\\\\''/g")"
        _wsl_args="$_wsl_args '$_wsl_q'"
    done
    # VBScript doubles quotes inside string literals.
    _wsl_line="wsl.exe -d \"\"$WSL_DISTRO_NAME\"\" --cd ~ --exec /bin/sh -lc \"\"exec$_wsl_args\"\""
    printf 'CreateObject("WScript.Shell").Run "%s", 0, False\r\n' "$_wsl_line" > "$_wsl_dir/$(wsl_launcher_file "$_wsl_name")"
    wsl_log "Installed the Windows Startup launcher: $_wsl_dir/$(wsl_launcher_file "$_wsl_name")"
}

# Usage: wsl_startup_remove <name>   (returns 1 when nothing was registered; exits when the Startup
# folder cannot be found, rather than reporting "nothing registered")
wsl_startup_remove() {
    _wsl_dir="$(wsl_startup_dir)" || exit 1
    _wsl_file="$_wsl_dir/$(wsl_launcher_file "$1")"
    _wsl_removed=1
    if wsl_startup_remove_legacy "$_wsl_dir" "$1"; then _wsl_removed=0; fi
    [ -f "$_wsl_file" ] || return "$_wsl_removed"
    rm -f "$_wsl_file"
    wsl_log "Removed the Windows Startup launcher: $_wsl_file"
}

# Runs a launcher installed by wsl_startup_install now, the same way Windows runs it at login, so the
# command keeps running after this terminal closes. Usage: wsl_startup_run <name>
wsl_startup_run() {
    _wsl_dir="$(wsl_startup_dir)" || exit 1
    _wsl_file="$_wsl_dir/$(wsl_launcher_file "$1")"
    [ -f "$_wsl_file" ] || wsl_die "No Windows Startup launcher named $1 is installed."
    _wsl_wscript="$(wsl_win_exe wscript.exe)"
    _wsl_winfile="$(wslpath -w "$_wsl_file")"
    (cd /mnt/c 2>/dev/null || true; "$_wsl_wscript" "$_wsl_winfile")
}

# True when a launcher installed by wsl_startup_install exists. Usage: wsl_startup_exists <name>
wsl_startup_exists() {
    _wsl_dir="$( (wsl_startup_dir) 2>/dev/null || true)"
    [ -n "$_wsl_dir" ] || return 1
    [ -f "$_wsl_dir/$(wsl_launcher_file "$1")" ] && return 0
    grep -qF -- "-d \"\"${WSL_DISTRO_NAME:-}\"\"" "$_wsl_dir/$1-wsl.vbs" 2>/dev/null
}

# --- Native libraries -------------------------------------------------------------------------------
# WSLg supplies only the display server. The distro still needs its own client libraries, and the
# stock WSL images (like minimal and server images) often lack them. Building never loads them, so
# without a check the failure only shows when the app starts, as "Unable to load shared library ...".

# Libraries a .NET GUI app built on Avalonia 12 loads on Linux: the sonames inside Avalonia.X11.dll
# (12.1.x) plus SkiaSharp's fontconfig. libGL, GTK 3 and Vulkan are optional (software rendering and
# managed dialogs are the fallbacks). Each entry is soname:Debian/Ubuntu package:Fedora package.
# shellcheck disable=SC2034 # used by the scripts that source this file
WSL_AVALONIA_LIBS="libX11.so.6:libx11-6:libX11 libICE.so.6:libice6:libICE libSM.so.6:libsm6:libSM libXext.so.6:libxext6:libXext libXrandr.so.2:libxrandr2:libXrandr libXi.so.6:libxi6:libXi libXcursor.so.1:libxcursor1:libXcursor libXfixes.so.3:libxfixes3:libXfixes libfontconfig.so.1:libfontconfig1:fontconfig"

# Prints the ldconfig cache (ldconfig -p), or nothing when there is no ldconfig to ask.
# WSL_LDCONFIG overrides the ldconfig command (used by tests).
wsl_ldconfig_cache() {
    _wl_ldc="${WSL_LDCONFIG:-}"
    if [ -z "$_wl_ldc" ]; then
        for _wl_c in ldconfig /sbin/ldconfig /usr/sbin/ldconfig; do
            if command -v "$_wl_c" >/dev/null 2>&1; then _wl_ldc="$_wl_c"; break; fi
        done
    fi
    [ -n "$_wl_ldc" ] || return 0
    "$_wl_ldc" -p 2>/dev/null || true
}

# True when the dynamic linker has <soname> (exact name, or a prefix ending in a dot such as
# "libicuuc.so.") for this machine's architecture. Usage: wsl_have_lib <cache> <soname>
wsl_have_lib() {
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
# Usage: wsl_install_hint "<deb packages>" "<rpm packages>"
wsl_install_hint() {
    if command -v apt-get >/dev/null 2>&1; then
        printf '  sudo apt install %s\n' "$1"
    elif command -v dnf >/dev/null 2>&1; then
        printf '  sudo dnf install %s\n' "$2"
    else
        printf '  Debian/Ubuntu: sudo apt install %s\n  Fedora:        sudo dnf install %s\n' "$1" "$2"
    fi
}

# Reports libraries from <list> that are missing, with the command that installs them. Does nothing
# when they are all present or when there is no ldconfig to ask (it never guesses).
# Usage: wsl_check_libs error|warning "<app name>" "<soname:deb:rpm list>"
#   error   exits (use before launching the app)
#   warning returns (use after installing it)
wsl_check_libs() {
    _wl_cache="$(wsl_ldconfig_cache)"
    [ -n "$_wl_cache" ] || return 0
    _wl_missing=""; _wl_deb=""; _wl_rpm=""
    for _wl_e in $3; do
        _wl_so="${_wl_e%%:*}"
        wsl_have_lib "$_wl_cache" "$_wl_so" && continue
        _wl_r="${_wl_e#*:}"
        _wl_missing="$_wl_missing $_wl_so"; _wl_deb="$_wl_deb ${_wl_r%%:*}"; _wl_rpm="$_wl_rpm ${_wl_r#*:}"
    done
    [ -n "$_wl_missing" ] || return 0
    if [ "$1" = error ]; then
        wsl_log "Error: $2 cannot start without these shared libraries:$_wl_missing"
    else
        wsl_log "Warning: $2 is installed, but it cannot start until these shared libraries are installed:$_wl_missing"
    fi
    wsl_log "WSLg provides only the display; the distro needs its own copies. Without them the app fails with"
    wsl_log "\"Unable to load shared library '...'\". Install them inside this distro with:"
    wsl_log "$(wsl_install_hint "${_wl_deb# }" "${_wl_rpm# }")"
    if [ "$1" = error ]; then exit 1; fi
    return 0
}

# .NET needs ICU (libicu) for globalization: the SDK itself and every app that does not run with
# invariant globalization. dotnet-install.sh and self-contained apps do not bring it. Exits with the
# install command when it is missing; does nothing when DOTNET_SYSTEM_GLOBALIZATION_INVARIANT is set
# to 1/true, or when there is no ldconfig to ask.
wsl_check_icu() {
    case "${DOTNET_SYSTEM_GLOBALIZATION_INVARIANT:-}" in 1|true|TRUE|True) return 0 ;; esac
    _wl_cache="$(wsl_ldconfig_cache)"
    [ -n "$_wl_cache" ] || return 0
    wsl_have_lib "$_wl_cache" "libicuuc.so." && return 0
    _wl_icu=""
    if command -v apt-cache >/dev/null 2>&1; then
        _wl_icu="$(apt-cache pkgnames libicu 2>/dev/null | awk '/^libicu[0-9]+(t64)?$/' | sort -V | tail -n 1 || true)"
    fi
    wsl_log "Error: .NET needs ICU (libicu), and this distro does not have it. Without it dotnet fails with"
    wsl_log "\"Couldn't find a valid ICU package installed on the system\". Install it inside this distro with:"
    wsl_log "$(wsl_install_hint "${_wl_icu:-libicu-dev}" libicu)"
    exit 1
}

# Warns when no clipboard tool is installed: terminal apps (TUIKit and the apps built on it) copy by
# running wl-copy, xclip or xsel, and none ships with the stock WSL images. Usage: wsl_check_clipboard "<app name>"
wsl_check_clipboard() {
    for _wl_t in wl-copy xclip xsel; do
        command -v "$_wl_t" >/dev/null 2>&1 && return 0
    done
    wsl_log "Note: copy to clipboard will not work in $1 until a clipboard tool is installed:"
    wsl_log "$(wsl_install_hint xclip xclip)"
}

# Installs the system packages an app needs, inside this distro, when they are missing: install
# scripts call it so the app does not fail later at startup. Uses apt-get (Debian/Ubuntu) or dnf
# (Fedora), through sudo unless running as root. INSTALL_SYSTEM_PACKAGES=0 turns it into a report.
# Usage: wsl_install_deps "<app name>" [icu] [gui] [clipboard] [browser]
#   icu        ICU for .NET (required)                 gui      the Avalonia libraries (required)
#   clipboard  xclip when no clipboard tool exists    browser  wslu + xdg-utils, to open links in Windows
# Required packages that cannot be installed are fatal; optional ones only warn.
wsl_install_deps() {
    _wd_app="$1"; shift
    _wd_cache="$(wsl_ldconfig_cache)"
    _wd_req_deb=""; _wd_req_rpm=""; _wd_opt_deb=""; _wd_opt_rpm=""; _wd_apt_icu=0
    for _wd_want in "$@"; do
        case "$_wd_want" in
            icu)
                case "${DOTNET_SYSTEM_GLOBALIZATION_INVARIANT:-}" in 1|true|TRUE|True) continue ;; esac
                if [ -n "$_wd_cache" ] && ! wsl_have_lib "$_wd_cache" "libicuuc.so."; then
                    _wd_apt_icu=1; _wd_req_rpm="$_wd_req_rpm libicu"
                fi ;;
            gui)
                [ -n "$_wd_cache" ] || continue
                for _wd_e in $WSL_AVALONIA_LIBS; do
                    wsl_have_lib "$_wd_cache" "${_wd_e%%:*}" && continue
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
            *) wsl_die "wsl_install_deps: unknown package group: $_wd_want" ;;
        esac
    done
    [ -n "$_wd_req_deb$_wd_req_rpm$_wd_opt_deb$_wd_opt_rpm" ] || [ "$_wd_apt_icu" = 1 ] || return 0

    if command -v apt-get >/dev/null 2>&1; then _wd_pm=apt; elif command -v dnf >/dev/null 2>&1; then _wd_pm=dnf; else _wd_pm=""; fi
    if [ "$(id -u)" = 0 ]; then _wd_sudo=""; elif command -v sudo >/dev/null 2>&1; then _wd_sudo=sudo; else _wd_sudo=none; fi
    if [ "${INSTALL_SYSTEM_PACKAGES:-1}" = 0 ] || [ -z "$_wd_pm" ] || [ "$_wd_sudo" = none ]; then
        _wd_icu_deb=""; [ "$_wd_apt_icu" = 1 ] && _wd_icu_deb=" libicu-dev"
        wsl_log "$_wd_app needs system packages that are not installed, and they were not installed automatically"
        wsl_log "(INSTALL_SYSTEM_PACKAGES=0, no apt-get/dnf, or no sudo). Install them with:"
        wsl_log "$(wsl_install_hint "${_wd_req_deb# }$_wd_icu_deb$_wd_opt_deb" "${_wd_req_rpm# }$_wd_opt_rpm")"
        if [ -n "$_wd_req_deb$_wd_req_rpm" ] || [ "$_wd_apt_icu" = 1 ]; then exit 1; fi
        return 0
    fi

    if [ "$_wd_pm" = apt ]; then
        wsl_log "Installing the system packages $_wd_app needs (apt-get; sudo may ask for your password)..."
        # shellcheck disable=SC2086 # empty or "sudo"
        $_wd_sudo apt-get update -qq || wsl_die "apt-get update failed."
        if [ "$_wd_apt_icu" = 1 ]; then
            _wd_icu="$(apt-cache pkgnames libicu 2>/dev/null | awk '/^libicu[0-9]+(t64)?$/' | sort -V | tail -n 1 || true)"
            _wd_req_deb="$_wd_req_deb ${_wd_icu:-libicu-dev}"
        fi
        if [ -n "$_wd_req_deb" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            DEBIAN_FRONTEND=noninteractive $_wd_sudo apt-get install -y -q $_wd_req_deb \
                || wsl_die "Could not install the required packages:$_wd_req_deb"
        fi
        if [ -n "$_wd_opt_deb" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            DEBIAN_FRONTEND=noninteractive $_wd_sudo apt-get install -y -q $_wd_opt_deb \
                || wsl_log "Warning: could not install the optional packages:$_wd_opt_deb (copy to clipboard or opening links may not work)."
        fi
    else
        wsl_log "Installing the system packages $_wd_app needs (dnf; sudo may ask for your password)..."
        if [ -n "$_wd_req_rpm" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            $_wd_sudo dnf install -y $_wd_req_rpm || wsl_die "Could not install the required packages:$_wd_req_rpm"
        fi
        if [ -n "$_wd_opt_rpm" ]; then
            # shellcheck disable=SC2086 # word-split package lists on purpose
            $_wd_sudo dnf install -y $_wd_opt_rpm \
                || wsl_log "Warning: could not install the optional packages:$_wd_opt_rpm (copy to clipboard or opening links may not work)."
        fi
    fi
    wsl_log "System packages for $_wd_app are installed."
}

# Prints the PATH to give an interactive app started from these scripts: the cleaned Linux PATH
# first, then the Windows entries the cleanup removed, so clip.exe, explorer.exe, wslview and the
# Windows browser stay reachable from inside the app (builds still use the cleaned PATH).
wsl_app_path() {
    _wl_path="$PATH"
    _wl_ifs="$IFS"; IFS=:
    for _wl_p in $WSL_ORIGINAL_PATH; do
        case "$_wl_p" in
            /mnt/[a-zA-Z]/*|/mnt/[a-zA-Z])
                case ":$_wl_path:" in *":$_wl_p:"*) ;; *) _wl_path="$_wl_path:$_wl_p" ;; esac ;;
        esac
    done
    IFS="$_wl_ifs"
    printf '%s\n' "$_wl_path"
}

# Exports what an interactive app needs from Windows: the PATH from wsl_app_path, and, when wslview
# (from the wslu package) exists and BROWSER is unset, BROWSER=wslview so xdg-open and the apps open
# links in the Windows browser. Call it right before starting the app.
wsl_prepare_app_env() {
    PATH="$(wsl_app_path)"; export PATH
    if [ -z "${BROWSER:-}" ] && command -v wslview >/dev/null 2>&1; then
        BROWSER=wslview; export BROWSER
    fi
    # Under WSL the dependable way to open a link is wslview (it hands the URL to Windows); xdg-open
    # alone usually finds no Linux browser.
    if ! command -v wslview >/dev/null 2>&1; then
        wsl_log "Note: links may not open from the app until wslview (and xdg-open) are installed:"
        wsl_log "$(wsl_install_hint "wslu xdg-utils" "wslu xdg-utils")"
    fi
}

