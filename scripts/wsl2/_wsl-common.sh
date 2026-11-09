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

# Prints 1 for WSL 1, 2 for WSL 2, or 0 when not running under WSL.
wsl_version() {
    _wsl_rel="$(uname -r 2>/dev/null)"
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

# Fails outside WSL; warns when the flavor doesn't match this directory. WSL_SKIP_CHECK=1 skips both.
wsl_require() {
    [ "${WSL_SKIP_CHECK:-0}" = 1 ] && return 0
    _wsl_v="$(wsl_version)"
    [ "$_wsl_v" != 0 ] || wsl_die "This script is for WSL. Use scripts/linux, scripts/macos or scripts/windows instead (or set WSL_SKIP_CHECK=1)."
    case "${WSL_FLAVOR:-wsl2}" in
        wsl) [ "$_wsl_v" = 1 ] || wsl_log "Note: this distro runs WSL 2; scripts/wsl2 is the better match. Continuing." ;;
        wsl2) [ "$_wsl_v" = 2 ] || wsl_log "Note: this distro runs WSL 1; scripts/wsl is the better match. Continuing." ;;
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
    for _wsl_p in "$HOME/.local/bin" "$HOME/.dotnet/tools" "$HOME/.dotnet"; do
        [ -d "$_wsl_p" ] || continue
        case ":$PATH:" in *":$_wsl_p:"*) ;; *) PATH="$_wsl_p:$PATH" ;; esac
    done
    if [ -z "${DOTNET_ROOT:-}" ] && [ -x "$HOME/.dotnet/dotnet" ]; then
        DOTNET_ROOT="$HOME/.dotnet"; export DOTNET_ROOT
    fi
    export PATH
}

# Fails with a helpful message when a Linux command is missing (after wsl_sanitize_path, Windows
# copies no longer count). Usage: wsl_require_cmd <name> [install hint]
wsl_require_cmd() {
    command -v "$1" >/dev/null 2>&1 && return 0
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
    _wsl_bad="$(find "$1/scripts" -name '*.sh' -type f -exec grep -l "$_wsl_cr" {} + 2>/dev/null | head -n 5)"
    if [ -n "$_wsl_bad" ]; then
        wsl_log "These scripts have Windows (CRLF) line endings and will not run under WSL:"
        wsl_log "$_wsl_bad"
        wsl_die "Re-checkout with LF endings: git config core.autocrlf input && git rm -r --cached -q . && git reset --hard"
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
    _wsl_gw="$(ip route show default 2>/dev/null | awk '/default/ {print $3; exit}')"
    [ -n "$_wsl_gw" ] || _wsl_gw="$(awk '/^nameserver/ {print $2; exit}' /etc/resolv.conf 2>/dev/null)"
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
        *://127.0.0.1:*|*://127.0.0.1/*|*://localhost:*|*://localhost/*) ;;
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

# Prints the Windows Startup folder as a WSL path.
wsl_startup_dir() {
    _wsl_cmd="$(wsl_win_exe cmd.exe)"
    _wsl_appdata="$(cd /mnt/c 2>/dev/null || true; "$_wsl_cmd" /c 'echo %APPDATA%' 2>/dev/null | tr -d '\r')"
    [ -n "$_wsl_appdata" ] || wsl_die "Could not read %APPDATA% from Windows."
    printf '%s/Microsoft/Windows/Start Menu/Programs/Startup\n' "$(wslpath -u "$_wsl_appdata")"
}

# Registers a command to start, hidden, at Windows login, inside this distro. The launcher keeps
# wsl.exe attached so the distro stays up while the command runs. No admin rights needed.
# Usage: wsl_startup_install <name> <command> [args...]   (name: letters, digits, dashes)
wsl_startup_install() {
    _wsl_name="$1"; shift
    [ -n "${WSL_DISTRO_NAME:-}" ] || wsl_die "WSL_DISTRO_NAME is not set; run this from a WSL shell."
    _wsl_dir="$(wsl_startup_dir)"
    mkdir -p "$_wsl_dir"
    _wsl_args=""
    for _wsl_a in "$@"; do
        _wsl_q="$(printf '%s' "$_wsl_a" | sed "s/'/'\\\\''/g")"
        _wsl_args="$_wsl_args '$_wsl_q'"
    done
    # VBScript doubles quotes inside string literals.
    _wsl_line="wsl.exe -d \"\"$WSL_DISTRO_NAME\"\" --cd ~ --exec /bin/sh -lc \"\"exec$_wsl_args\"\""
    printf 'CreateObject("WScript.Shell").Run "%s", 0, False\r\n' "$_wsl_line" > "$_wsl_dir/$_wsl_name-wsl.vbs"
    wsl_log "Installed the Windows Startup launcher: $_wsl_dir/$_wsl_name-wsl.vbs"
}

# Usage: wsl_startup_remove <name>   (returns 1 when nothing was registered)
wsl_startup_remove() {
    _wsl_file="$(wsl_startup_dir)/$1-wsl.vbs"
    [ -f "$_wsl_file" ] || return 1
    rm -f "$_wsl_file"
    wsl_log "Removed the Windows Startup launcher: $_wsl_file"
}
