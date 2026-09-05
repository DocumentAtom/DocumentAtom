#!/usr/bin/env bash
# Shared helpers for the DocumentAtom MCP install/uninstall scripts.
# Sourced by install.sh and uninstall.sh.

set -euo pipefail

repo_root() {
  # scripts/<os>/_common.sh -> repo root is two levels up.
  local script_dir
  script_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
  (cd "$script_dir/../.." && pwd)
}

has_dotnet_sdk() {
  command -v dotnet >/dev/null 2>&1 || return 1
  local sdks
  sdks="$(dotnet --list-sdks 2>/dev/null || true)"
  [ -n "$sdks" ]
}

find_mcp_dll() {
  local repo="$1"
  local bin_dir="$repo/src/DocumentAtom.McpServer/bin"
  [ -d "$bin_dir" ] || return 0

  local candidates
  candidates="$(find "$bin_dir" -name 'DocumentAtom.McpServer.dll' -type f 2>/dev/null || true)"
  [ -z "$candidates" ] && return 0

  # Preference order: Release before Debug; net8.0 (LTS, broadest runtime availability)
  # before net10.0.
  local pat match
  for pat in 'Release/net8' 'Release/net10' 'Debug/net8' 'Debug/net10'; do
    match="$(printf '%s\n' "$candidates" | grep "/$pat" | head -n1 || true)"
    if [ -n "$match" ]; then
      printf '%s\n' "$match"
      return 0
    fi
  done

  printf '%s\n' "$candidates" | head -n1
}

resolve_mcp_dll() {
  local repo="$1"

  # When the SDK is available, build (incrementally) so we never run a stale binary.
  if has_dotnet_sdk; then
    echo "Building DocumentAtom.McpServer (Release)..." >&2
    if ! dotnet build "$repo/src/DocumentAtom.McpServer/DocumentAtom.McpServer.csproj" -c Release --nologo >&2; then
      echo "WARNING: Build failed; attempting to use an existing binary if one is present." >&2
    fi
  fi

  local dll
  dll="$(find_mcp_dll "$repo")"
  if [ -z "$dll" ]; then
    echo "ERROR: Could not locate a compiled DocumentAtom.McpServer.dll." >&2
    echo "       Install the .NET SDK and build the project, then retry." >&2
    return 1
  fi
  printf '%s\n' "$dll"
}

# invoke_documentatom_mcp <install|uninstall> <targets> [endpoint]
invoke_documentatom_mcp() {
  local action="$1"
  local targets="$2"
  local endpoint="${3:-}"

  if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: The .NET runtime ('dotnet') was not found on PATH." >&2
    echo "       Install the .NET 8 (or later) runtime to run the MCP server tooling." >&2
    return 1
  fi

  local repo dll
  repo="$(repo_root)"
  dll="$(resolve_mcp_dll "$repo")"

  if [ "$action" = "install" ] && [ -n "$endpoint" ]; then
    exec dotnet "$dll" "--install=$targets" "--endpoint=$endpoint"
  else
    exec dotnet "$dll" "--$action=$targets"
  fi
}
