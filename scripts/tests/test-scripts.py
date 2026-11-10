"""Exercise the install/uninstall/reinstall scripts in scripts/{linux,macos,wsl,wsl2,windows}.

Run with: python3 scripts/tests/test-scripts.py

The Unix scripts run in a temporary copy of the repository against fake tools (dotnet, uname, curl,
ip, cmd.exe, wslpath) that log their arguments, so nothing real is built or configured. The WSL
scripts run with WSL_SKIP_CHECK=1. When pwsh is installed, the Windows .ps1 scripts run against the
same fakes; the .cmd wrappers are checked statically. When a real dotnet and a compiled
src/DocumentAtom.McpServer Release build exist, a final pass lets the fake dotnet hand the server
calls to the real server, so the config files written under a temporary HOME are checked too.
"""
import json
import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

root = Path(__file__).resolve().parents[2]
UNIX = ['linux', 'macos', 'wsl', 'wsl2']
ACTIONS = ['install', 'uninstall', 'reinstall']
CSPROJ_REL = 'src/DocumentAtom.McpServer/DocumentAtom.McpServer.csproj'
DEFAULT_ENDPOINT = 'http://localhost:8200/rpc'

FAKE = '''#!/usr/bin/env python3
import json, os, pathlib, sys
name = pathlib.Path(sys.argv[0]).name
args = sys.argv[1:]
if name == 'uname':
    if args == ['-s']:
        print(os.environ['MOCK_UNAME'])
    elif args == ['-m']:
        print('x86_64')
    else:
        print('5.15.0-microsoft-standard-WSL2' if os.environ.get('MOCK_WSL2') else '6.0.0-generic')
    sys.exit(0)
if name == 'ldconfig':
    print(os.environ.get('MOCK_LDCACHE', '')); sys.exit(0)
if name == 'ip':
    print('default via 172.20.0.1 dev eth0'); sys.exit(0)
if name == 'curl':
    sys.exit(0 if any('172.20.0.1' in a for a in args) else 7)
if name == 'cmd.exe':
    print('C:\\\\Users\\\\test\\\\AppData\\\\Roaming\\r'); sys.exit(0)
if name == 'wslpath':
    print(os.environ['MOCK_APPDATA'] if args[0] == '-u' else 'C:\\\\fake\\\\' + pathlib.Path(args[1]).name); sys.exit(0)
with open(os.environ['CALL_LOG'], 'a') as f:
    f.write(json.dumps([name, *args]) + '\\n')
if name == 'dotnet':
    if args == ['--list-sdks']:
        print(os.environ.get('MOCK_SDKS', '8.0.425 [mock]\\n10.0.401 [mock]')); sys.exit(0)
    if args == ['--list-runtimes']:
        print(os.environ.get('MOCK_RUNTIMES', 'Microsoft.NETCore.App 8.0.31 [mock]\\nMicrosoft.NETCore.App 10.0.14 [mock]')); sys.exit(0)
    if args[:1] == ['build']:
        if os.environ.get('MOCK_BUILD_FAIL'):
            sys.exit(1)
        out = pathlib.Path(args[1]).parent / 'bin' / 'Release' / args[args.index('-f') + 1]
        out.mkdir(parents=True, exist_ok=True)
        (out / 'DocumentAtom.McpServer.dll').write_text('fake')
        sys.exit(0)
    if args and args[0].endswith('DocumentAtom.McpServer.dll'):
        if os.environ.get('REAL_DOTNET'):
            os.execv(os.environ['REAL_DOTNET'], [os.environ['REAL_DOTNET'], os.environ['REAL_SERVER'], *args[1:]])
        action = args[1].split('=')[0]
        sys.exit(1 if os.environ.get('MOCK_FAIL') == action else 0)
'''

count = 0


def check(cond, *info):
    global count
    assert cond, info
    count += 1


# --- Static checks against the real tree -------------------------------------------------------

# Same set of user-facing scripts everywhere (underscore files are sourced libraries).
names = {p: sorted(f.stem for f in (root / 'scripts' / p).glob('*.sh') if not f.name.startswith('_')) for p in UNIX}
for p in UNIX:
    check(names[p] == sorted(ACTIONS), p, names[p])
check(sorted(f.stem for f in (root / 'scripts/windows').glob('*.cmd')) == sorted(ACTIONS))
check(sorted(f.stem for f in (root / 'scripts/windows').glob('*.ps1')) == sorted(ACTIONS))
for p in UNIX + ['windows']:
    extra = sorted(f.name for f in (root / 'scripts' / p).iterdir()
                   if f.is_file() and f.stem not in ACTIONS and f.name != '_wsl-common.sh')
    check(not extra, p, extra)
for p in ['wsl', 'wsl2']:
    check((root / f'scripts/{p}/_wsl-common.sh').is_file(), p)
check((root / 'scripts/wsl/_wsl-common.sh').read_bytes() == (root / 'scripts/wsl2/_wsl-common.sh').read_bytes())

# Self-contained: no script runs another script (sourcing _wsl-common.sh is the only include).
for p in UNIX:
    for a in ACTIONS:
        text = (root / f'scripts/{p}/{a}.sh').read_text()
        code = [line for line in text.splitlines() if not line.lstrip().startswith('#')]
        for line in code:
            check(not re.search(r'\.\./linux|wsl_run|(^|[\s;(])(bash|sh|exec|source|\.)\s+"?\$SCRIPT_DIR/(?!_wsl-common\.sh)', line),
                  p, a, line)
        check(text.startswith('#!/usr/bin/env bash\n') and 'set -euo pipefail' in text and 'die() {' in text, p, a)
        if p.startswith('wsl'):
            check(f'WSL_FLAVOR={p}\n' in text and 'wsl_require\nwsl_sanitize_path\nwsl_check_repo "$REPO_ROOT"' in text, p, a)
            check('wsl_require_cmd dotnet' in text, p, a)
        check(b'\r' not in (root / f'scripts/{p}/{a}.sh').read_bytes(), p, a)
        check(os.access(root / f'scripts/{p}/{a}.sh', os.X_OK), p, a, 'not executable')
for a in ACTIONS:
    ps1 = (root / f'scripts/windows/{a}.ps1').read_text()
    check(not re.search(r'^\s*[.&]\s*\(?\s*(Join-Path|"?\$PSScriptRoot).*', ps1, re.M), a, 'ps1 runs or dot-sources another script')
    cmd = (root / f'scripts/windows/{a}.cmd').read_bytes()
    check(cmd.count(b'\n') == cmd.count(b'\r\n'), a, 'cmd must be CRLF')
    cmd = cmd.decode()
    check(cmd.startswith('@echo off\r\n') and '\r\nsetlocal\r\n' in cmd and 'launch shim' in cmd, a)
    check('$IsWindows' in ps1 and 'SCRIPTS_SKIP_OS_CHECK' in ps1, a)
    check(f'powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0{a}.ps1" %*\r\nset "RC=%ERRORLEVEL%"\r\nendlocal & exit /b %RC%\r\n' in cmd, a)

attributes = (root / '.gitattributes').read_text()
for rule in ['*.sh text eol=lf', '*.bat text eol=crlf', '*.cmd text eol=crlf']:
    check(rule in attributes, rule)

# Every script path a message mentions exists.
for f in list(root.glob('scripts/*/*.sh')) + list(root.glob('scripts/windows/*.ps1')):
    for ref in re.findall(r'scripts[/\\\\]+(?:linux|macos|wsl2?|windows)[/\\\\]+[\w-]+\.(?:sh|cmd|ps1)', f.read_text()):
        check((root / ref.replace('\\', '/').replace('//', '/')).is_file(), f, ref)

# --- Runs against fakes ------------------------------------------------------------------------

with tempfile.TemporaryDirectory(prefix='documentatom scripts ') as temp:
    temp = Path(temp)
    repo = temp / 'repo'
    shutil.copytree(root / 'scripts', repo / 'scripts', ignore=shutil.ignore_patterns('__pycache__', 'tests'))
    csproj = repo / CSPROJ_REL
    csproj.parent.mkdir(parents=True)
    csproj.write_text('<Project />')
    bin_dir = csproj.parent / 'bin'
    fakes = temp / 'bin'
    fakes.mkdir()
    for name in ['dotnet', 'uname', 'curl', 'ip', 'cmd.exe', 'wslpath']:
        (fakes / name).write_text(FAKE)
        (fakes / name).chmod(0o755)
    no_dotnet = temp / 'bin-no-dotnet'
    no_dotnet.mkdir()
    for name in ['uname', 'curl', 'ip']:
        (no_dotnet / name).write_text(FAKE)
        (no_dotnet / name).chmod(0o755)
    log = temp / 'calls.jsonl'
    home = temp / 'home'
    home.mkdir()
    base_env = dict(os.environ, PATH=f'{fakes}:/usr/bin:/bin', CALL_LOG=str(log), HOME=str(home),
                    WSL_SKIP_CHECK='1', WSL_DISTRO_NAME='Test', MOCK_APPDATA=str(temp / 'appdata'))
    for key in ['DOTNET_ROOT', 'WSL_NO_HOST_REWRITE', 'WSL_KEEP_WINDOWS_PATH', 'MUX_CONFIG_DIR']:
        base_env.pop(key, None)

    def calls():
        entries = [json.loads(x) for x in log.read_text().splitlines()] if log.exists() else []
        return [c[1:] for c in entries if c[0] == 'dotnet' and c[1] not in ('--list-sdks', '--list-runtimes')]

    def run(platform, action, *args, **env):
        log.unlink(missing_ok=True)
        e = dict(base_env, MOCK_UNAME='Darwin' if platform == 'macos' else 'Linux')
        for k, v in env.items():
            if v is None:
                e.pop(k, None)
            else:
                e[k] = v
        p = subprocess.run(['bash', str(repo / f'scripts/{platform}/{action}.sh'), *args], env=e,
                           capture_output=True, text=True)
        return p, calls()

    def server(c):
        return c[0].endswith('DocumentAtom.McpServer.dll')

    def reset_bin():
        shutil.rmtree(bin_dir, ignore_errors=True)

    # OS guards.
    for platform, wrong, label in [('linux', 'Darwin', 'for Linux'), ('macos', 'Linux', 'for macOS')]:
        for a in ACTIONS:
            p, c = run(platform, a, MOCK_UNAME=wrong)
            check(p.returncode == 1 and f'This script is {label}' in p.stderr and not c, platform, a, p.stderr)

    for platform in UNIX:
        for a in ACTIONS:
            reset_bin()
            # Argument handling: help, unexpected arguments, bad targets and endpoints change nothing.
            p, c = run(platform, a, '--help')
            check(p.returncode == 0 and f'Usage: ./scripts/{platform}/{a}.sh' in p.stdout and not c, platform, a, p.stdout)
            p, c = run(platform, a, '-h')
            check(p.returncode == 0 and 'Usage:' in p.stdout, platform, a)
            bad = [['extra'], ['--targets=bogus'], ['--targets=claude,nope'], ['--targets='], ['--targets'], ['--targets=,'],
                   ['--endpoint=http://x/rpc'] if a == 'uninstall' else ['--endpoint=ftp://x'],
                   ['--endpoint'] if a != 'uninstall' else ['--force']]
            for args in bad:
                p, c = run(platform, a, *args)
                check(p.returncode == 1 and p.stderr.startswith('Error: ') and not c, platform, a, args, p.stderr)

            # Default run: builds net10.0 (a .NET 10 SDK is present), then calls the server.
            p, c = run(platform, a)
            check(p.returncode == 0, platform, a, p.stderr)
            check(c[0] == ['build', str(csproj), '-c', 'Release', '-f', 'net10.0', '--nologo', '-v', 'quiet'], platform, a, c)
            dll = str(bin_dir / 'Release/net10.0/DocumentAtom.McpServer.dll')
            expected = {'install': [[dll, '--install=all', f'--endpoint={DEFAULT_ENDPOINT}']],
                        'uninstall': [[dll, '--uninstall=all']],
                        'reinstall': [[dll, '--uninstall=all'], [dll, '--install=all', f'--endpoint={DEFAULT_ENDPOINT}']]}[a]
            check(c[1:] == expected, platform, a, c)
            label = {'linux': 'Linux', 'macos': 'macOS', 'wsl': 'WSL 1', 'wsl2': 'WSL 2'}[platform]
            check(p.stdout.startswith(f'DocumentAtom MCP {a} ({label})\n[1/'), platform, a, p.stdout)
            windows_hint = f'scripts\\windows\\{a}.cmd'
            check((windows_hint in p.stdout) == platform.startswith('wsl'), platform, a, p.stdout)

            # Only a .NET 8 SDK: builds net8.0 only.
            reset_bin()
            p, c = run(platform, a, MOCK_SDKS='8.0.425 [mock]')
            check(p.returncode == 0 and c[0][4:6] == ['-f', 'net8.0'] and c[1][0].endswith('Release/net8.0/DocumentAtom.McpServer.dll'), platform, a, c)

            # Targets are normalized and passed through; --targets <value> works too.
            args = ['--targets', 'Claude,\tCursor '] + (['--endpoint', 'http://host:9/rpc'] if a != 'uninstall' else [])
            p, c = run(platform, a, *args)
            check(p.returncode == 0, platform, a, p.stderr)
            srv = [x for x in c if server(x)]
            check(all(x[1].endswith('=claude,cursor') for x in srv), platform, a, srv)
            if a != 'uninstall':
                check(srv[-1][2] == '--endpoint=http://host:9/rpc', platform, a, srv)
                # The scheme check is case-insensitive, as in PowerShell.
                p, c = run(platform, a, '--endpoint=HTTPS://Host:9/rpc')
                check(p.returncode == 0 and c[-1][2] == '--endpoint=HTTPS://Host:9/rpc', platform, a, p.stderr)

            # No SDK: no build; uses an existing build the runtime can run (net8.0 Debug here).
            reset_bin()
            (bin_dir / 'Debug/net8.0').mkdir(parents=True)
            (bin_dir / 'Debug/net8.0/DocumentAtom.McpServer.dll').write_text('fake')
            (bin_dir / 'Release/net10.0').mkdir(parents=True)
            (bin_dir / 'Release/net10.0/DocumentAtom.McpServer.dll').write_text('fake')
            p, c = run(platform, a, MOCK_SDKS='', MOCK_RUNTIMES='Microsoft.NETCore.App 8.0.31 [mock]')
            check(p.returncode == 0 and not any(x[0] == 'build' for x in c), platform, a, c)
            check(all(x[0] == str(bin_dir / 'Debug/net8.0/DocumentAtom.McpServer.dll') for x in c), platform, a, c)
            # A failed build is fatal for install and reinstall; uninstall falls back to an existing
            # build (with a warning), so removing an entry never depends on a working build.
            p, c = run(platform, a, MOCK_BUILD_FAIL='1')
            if a == 'uninstall':
                check(p.returncode == 0 and 'removing with an existing build' in p.stderr
                      and [x for x in c if server(x)] == [[str(bin_dir / 'Release/net10.0/DocumentAtom.McpServer.dll'), '--uninstall=all']], platform, a, p.stderr, c)
            else:
                check(p.returncode == 1 and 'The build failed' in p.stderr and not any(server(x) for x in c), platform, a, p.stderr)
            reset_bin()
            p, c = run(platform, a, MOCK_BUILD_FAIL='1')
            check(p.returncode == 1 and 'The build failed' in p.stderr and not any(server(x) for x in c), platform, a, p.stderr)
            # No SDK and no usable build: fails without calling the server.
            reset_bin()
            p, c = run(platform, a, MOCK_SDKS='')
            check(p.returncode == 1 and 'Install the .NET SDK' in p.stderr and not c, platform, a, p.stderr)
            # dotnet missing.
            p, c = run(platform, a, PATH=f'{no_dotnet}:/usr/bin:/bin')
            check(p.returncode == 1 and "dotnet" in p.stderr and 'was not found' in p.stderr, platform, a, p.stderr)

            # Server failures are reported with exit code 1; reinstall stops before registering.
            reset_bin()
            p, c = run(platform, a, MOCK_FAIL='--uninstall' if a != 'install' else '--install')
            check(p.returncode == 1 and p.stderr.strip().splitlines()[-1].startswith('Error: '), platform, a, p.stderr)
            if a == 'reinstall':
                check(not any(x[1].startswith('--install') for x in c if server(x)), platform, a, c)
                p, c = run(platform, a, MOCK_FAIL='--install')
                check(p.returncode == 1 and [x[1].split('=')[0] for x in c if server(x)] == ['--uninstall', '--install'], c)

    # WSL: the default localhost endpoint is rewritten to the Windows host only when nothing answers
    # inside WSL 2; an explicit endpoint is never rewritten.
    # WSL 1 shares the Windows network, so scripts/wsl never rewrites.
    for platform in ['wsl', 'wsl2']:
        for a in ['install', 'reinstall']:
            p, c = run(platform, a, MOCK_WSL2='1')
            rewritten = 'http://172.20.0.1:8200/rpc' if platform == 'wsl2' else DEFAULT_ENDPOINT
            check(p.returncode == 0 and c[-1][2] == f'--endpoint={rewritten}', platform, a, c)
            check(('can change when Windows restarts' in p.stderr) == (platform == 'wsl2'), platform, a, p.stderr)
            check(('NAT' in (repo / f'scripts/{platform}/{a}.sh').read_text()) == (platform == 'wsl2'), platform, a)
            p, c = run(platform, a, MOCK_WSL2='1', WSL_NO_HOST_REWRITE='1')
            check(c[-1][2] == f'--endpoint={DEFAULT_ENDPOINT}', platform, a, c)
            p, c = run(platform, a, f'--endpoint={DEFAULT_ENDPOINT}', MOCK_WSL2='1')
            check(c[-1][2] == f'--endpoint={DEFAULT_ENDPOINT}', platform, a, c)
            p, c = run(platform, a)  # not WSL 2: unchanged
            check(c[-1][2] == f'--endpoint={DEFAULT_ENDPOINT}', platform, a, c)
    # ICU. A fake ldconfig (off PATH, so every other run here has no ldconfig and skips the checks)
    # reports MOCK_LDCACHE until the fake apt-get has "installed" something, then a cache with
    # libicuuc. Fake sudo, apt-get and apt-cache live in their own directory, added to PATH only here.
    (temp / 'ldc').mkdir()
    (temp / 'apt').mkdir()
    have_icu = '\tlibicuuc.so.74 (libc6,x86-64) => /usr/lib/x86_64-linux-gnu/libicuuc.so.74'
    i386_icu = '\tlibicuuc.so.74 (libc6) => /usr/lib/i386-linux-gnu/libicuuc.so.74'
    no_icu = '\tlibc.so.6 (libc6,x86-64) => /lib/libc.so.6'
    apt_done = temp / 'apt-installed'
    for d, name, body in [('ldc', 'ldconfig', f'#!/bin/sh\nif [ -f "{apt_done}" ]; then printf "%s\\n" "{have_icu}"; else printf "%s\\n" "$MOCK_LDCACHE"; fi\n'),
                          ('apt', 'apt-get', f'#!/bin/sh\necho "[\\"apt-get\\", \\"$*\\"]" >> "$CALL_LOG"\ncase " $* " in *" install "*) touch "{apt_done}" ;; esac\nexit 0\n'),
                          ('apt', 'apt-cache', '#!/bin/sh\nprintf "libicu-dev\\nlibicu74\\nlibicu72\\n"\n'),
                          ('apt', 'sudo', '#!/bin/sh\necho "[\\"sudo\\", \\"$1\\"]" >> "$CALL_LOG"\n[ "$1" = -v ] && exit 0\nexec "$@"\n')]:
        (temp / d / name).write_text(body)
        (temp / d / name).chmod(0o755)
    ldconfig = str(temp / 'ldc/ldconfig')
    apt_path = f'{temp / "apt"}:{fakes}:/usr/bin:/bin'

    def apt_ok(apt):
        # One update and one install of the latest libicuNN, both non-interactive with a lock timeout.
        return (len(apt) == 2 and all('-o DPkg::Lock::Timeout=300' in x[1] for x in apt)
                and apt[0][1].endswith(' update') and apt[1][1].endswith(' install -y libicu74'))

    def run_icu(platform, a, *args, **env):
        apt_done.unlink(missing_ok=True)
        var = 'NATIVE_LDCONFIG' if platform == 'linux' else 'WSL_LDCONFIG'
        log.unlink(missing_ok=True)
        p, _ = run(platform, a, *args, **{var: ldconfig, **env})
        entries = [json.loads(x) for x in log.read_text().splitlines()] if log.exists() else []
        return p, [e for e in entries if e[0] == 'apt-get'], [e for e in entries if e[0] == 'dotnet' and e[1] == 'build']

    # The installer wraps apt-get in "timeout --foreground 300" when timeout supports it; cover that
    # path with a fake timeout too (the machine running the tests may or may not have one).
    (temp / 'tmo').mkdir()
    (temp / 'tmo/timeout').write_text('#!/bin/sh\n[ "$1" = --foreground ] && shift\nshift\nexec "$@"\n')
    (temp / 'tmo/timeout').chmod(0o755)
    for platform in ['linux', 'wsl', 'wsl2']:
        p, apt, build = run_icu(platform, 'install', MOCK_LDCACHE=no_icu, PATH=f'{temp / "tmo"}:{apt_path}')
        check(p.returncode == 0 and apt_ok(apt) and build, platform, 'timeout', p.stderr, apt)
        run(platform, 'uninstall')

    for platform in ['linux', 'wsl', 'wsl2']:
        for a in ACTIONS:
            for cache in [no_icu, i386_icu]:
                # Install-type scripts install ICU (the latest libicuNN from apt-cache), then build.
                p, apt, build = run_icu(platform, a, MOCK_LDCACHE=cache, PATH=apt_path)
                if a == 'uninstall':
                    # uninstall installs nothing: it only checks, and stops before building.
                    check(p.returncode == 1 and 'needs ICU (libicu)' in p.stderr and "Couldn't find a valid ICU package" in p.stderr
                          and 'sudo apt install libicu74' in p.stderr and not apt and not build, platform, a, p.stderr)
                else:
                    check(p.returncode == 0 and apt_ok(apt) and build,
                          platform, a, p.stderr, apt)
            if a != 'uninstall':
                # INSTALL_SYSTEM_PACKAGES=0 reports and exits; so does a system without apt-get or dnf.
                for env in [dict(INSTALL_SYSTEM_PACKAGES='0', PATH=apt_path), {}]:
                    p, apt, build = run_icu(platform, a, MOCK_LDCACHE=no_icu, **env)
                    check(p.returncode == 1 and 'needs system packages that are not installed' in p.stderr
                          and 'libicu-dev' in p.stderr and not apt and not build, platform, a, env, p.stderr)
                # --help installs nothing.
                p, apt, build = run_icu(platform, a, '--help', MOCK_LDCACHE=no_icu, PATH=apt_path)
                check(p.returncode == 0 and not apt and not build, platform, a)
            # Nothing missing: nothing is installed; invariant globalization skips the check.
            for env in [dict(MOCK_LDCACHE=have_icu), dict(MOCK_LDCACHE=no_icu, DOTNET_SYSTEM_GLOBALIZATION_INVARIANT='1')]:
                p, apt, build = run_icu(platform, a, PATH=apt_path, **env)
                check(p.returncode == 0 and not apt and build, platform, a, env, p.stderr)

    # WSL: CRLF checkouts fail clearly before anything runs.
    crlf = repo / 'scripts/linux/crlf-probe.sh'
    crlf.write_bytes(b'#!/bin/sh\r\necho hi\r\n')
    for platform in ['wsl', 'wsl2']:
        p, c = run(platform, 'install')
        check(p.returncode == 1 and 'CRLF' in p.stderr and not c, platform, p.stderr)
    crlf.unlink()

    # Windows: run the .ps1 scripts under pwsh against the same fakes.
    pwsh = shutil.which('pwsh')
    real_dotnet = shutil.which('dotnet', path=os.environ.get('PATH', ''))
    pwsh_cmd = [pwsh]
    if pwsh and real_dotnet:
        # The pwsh .NET global tool shim starts 'dotnet' from PATH, which is the fake here; run its
        # pwsh.dll with the real dotnet instead.
        shim = sorted((Path(pwsh).resolve().parent / '.store/powershell').glob('*/powershell/*/tools/*/any/unix/pwsh.dll'))
        if shim:
            pwsh_cmd = [real_dotnet, str(shim[-1])]
    if pwsh:
        def run_ps(action, *args, **env):
            log.unlink(missing_ok=True)
            e = dict(base_env, SCRIPTS_SKIP_OS_CHECK='1'); e.update(env)
            p = subprocess.run([*pwsh_cmd, '-NoProfile', '-NonInteractive', '-File', str(repo / f'scripts/windows/{action}.ps1'), *args],
                               env=e, capture_output=True, text=True)
            return p, calls()
        for a in ACTIONS:
            reset_bin()
            for args in [['-Help'], ['--help'], ['-h']]:
                p, c = run_ps(a, *args)
                check(p.returncode == 0 and f'Usage: scripts\\windows\\{a}.cmd' in p.stdout and not c, a, args, p.stdout, p.stderr)
            for args in [['extra'], ['--targets=claude'], ['-Targets', 'bogus'], ['-Targets', ',']] + \
                        ([['-Endpoint', 'ftp://x'], ['-Endpoint', '']] if a != 'uninstall' else [['-Endpoint', 'http://x']]):
                p, c = run_ps(a, *args)
                check(p.returncode == 1 and not c, a, args, p.stdout, p.stderr)
            p, c = run_ps(a)
            check(p.returncode == 0, a, p.stdout, p.stderr)
            dll = str(bin_dir / 'Release/net10.0/DocumentAtom.McpServer.dll')
            check(c[0] == ['build', str(csproj), '-c', 'Release', '-f', 'net10.0', '--nologo', '-v', 'quiet'], a, c)
            expected = {'install': [[dll, '--install=all', f'--endpoint={DEFAULT_ENDPOINT}']],
                        'uninstall': [[dll, '--uninstall=all']],
                        'reinstall': [[dll, '--uninstall=all'], [dll, '--install=all', f'--endpoint={DEFAULT_ENDPOINT}']]}[a]
            check(c[1:] == expected, a, c)
            check(p.stdout.startswith(f'DocumentAtom MCP {a} (Windows)\n[1/'), a, p.stdout)
            check(f'scripts/wsl2/{a}.sh' in p.stdout, a, p.stdout)
            p, c = run_ps(a, '-Targets', 'Claude,\tcursor ', MOCK_SDKS='8.0.425 [mock]')
            check(p.returncode == 0 and c[0][4:6] == ['-f', 'net8.0'] and all(x[1].endswith('=claude,cursor') for x in c[1:]), a, c)
            if a == 'uninstall':
                p, c = run_ps(a, MOCK_BUILD_FAIL='1')
                check(p.returncode == 0 and 'removing with an existing build' in p.stderr and [x[1] for x in c if server(x)] == ['--uninstall=all'], a, p.stderr, c)
            else:
                p, c = run_ps(a, '-Endpoint', 'HTTPS://Host:9/rpc')
                check(p.returncode == 0 and c[-1][2] == '--endpoint=HTTPS://Host:9/rpc', a, p.stderr)
            reset_bin()
            p, c = run_ps(a, MOCK_BUILD_FAIL='1')
            check(p.returncode == 1 and 'The build failed' in p.stderr and not any(server(x) for x in c), a, p.stderr)
            # OS guard: refuses outside Windows unless SCRIPTS_SKIP_OS_CHECK=1.
            p, c = run_ps(a, SCRIPTS_SKIP_OS_CHECK='0')
            check(p.returncode == 1 and 'This script is for Windows' in p.stderr and not c, a, p.stderr)
            p, c = run_ps(a, MOCK_SDKS='')
            check(p.returncode == 1 and 'Install the .NET SDK' in p.stderr and not c, a, p.stderr)
            p, c = run_ps(a, MOCK_FAIL='--uninstall' if a != 'install' else '--install')
            check(p.returncode == 1 and 'Error: ' in p.stderr, a, p.stderr)
            if a == 'reinstall':
                check(not any(x[1].startswith('--install') for x in c if server(x)), c)
    else:
        print('pwsh not found: the Windows .ps1 scripts were only checked statically.')

    # Real config files, when a real dotnet and a compiled server exist.
    real_server = next((p for p in [root / 'src/DocumentAtom.McpServer/bin/Release/net10.0/DocumentAtom.McpServer.dll',
                                    root / 'src/DocumentAtom.McpServer/bin/Release/net8.0/DocumentAtom.McpServer.dll'] if p.is_file()), None)
    if real_dotnet and real_server:
        real = dict(REAL_DOTNET=real_dotnet, REAL_SERVER=str(real_server), DOTNET_CLI_HOME=str(temp / 'cli-home'))
        for platform in UNIX + (['windows'] if pwsh else []):
            shutil.rmtree(home)
            home.mkdir()
            (home / '.claude.json').write_text(json.dumps({'keep': 1, 'mcpServers': {'other': {'url': 'x'}}}))
            (home / '.codex').mkdir()
            (home / '.codex/config.toml').write_text('[mcp_servers.other]\nurl = "x"\n')

            def go(a, *args):
                if platform == 'windows':
                    args = [y for x in args for y in (['-Targets', x.split('=', 1)[1]] if x.startswith('--targets=') else ['-Endpoint', x.split('=', 1)[1]] if x.startswith('--endpoint=') else [x])]
                    return run_ps(a, *args, **real)
                return run(platform, a, *args, **real)
            p, _ = go('install', '--endpoint=http://h:1/rpc')
            check(p.returncode == 0, platform, p.stdout, p.stderr)
            claude = json.loads((home / '.claude.json').read_text())
            check(claude['keep'] == 1 and claude['mcpServers'] == {'other': {'url': 'x'}, 'documentatom': {'type': 'http', 'url': 'http://h:1/rpc'}}, platform, claude)
            check('[mcp_servers.other]' in (home / '.codex/config.toml').read_text() and 'url = "http://h:1/rpc"' in (home / '.codex/config.toml').read_text(), platform)
            check(json.loads((home / '.gemini/settings.json').read_text())['mcpServers']['documentatom'] == {'httpUrl': 'http://h:1/rpc'}, platform)
            check(json.loads((home / '.cursor/mcp.json').read_text())['mcpServers']['documentatom'] == {'url': 'http://h:1/rpc'}, platform)
            check(json.loads((home / '.mux/mcp-servers.json').read_text())['servers'] == [{'name': 'documentatom', 'transport': 'http', 'url': 'http://h:1', 'mcpPath': '/rpc'}], platform)
            p, _ = go('reinstall', '--targets=cursor', '--endpoint=http://h:2/mcp')
            check(p.returncode == 0 and json.loads((home / '.cursor/mcp.json').read_text())['mcpServers']['documentatom'] == {'url': 'http://h:2/mcp'}, platform, p.stderr)
            p, _ = go('uninstall')
            check(p.returncode == 0, platform, p.stderr)
            check(json.loads((home / '.claude.json').read_text()) == {'keep': 1, 'mcpServers': {'other': {'url': 'x'}}}, platform)
            check('documentatom' not in (home / '.codex/config.toml').read_text() and '[mcp_servers.other]' in (home / '.codex/config.toml').read_text(), platform)
            for f in ['.gemini/settings.json', '.cursor/mcp.json', '.mux/mcp-servers.json']:
                check('documentatom' not in (home / f).read_text(), platform, f)
            p, _ = go('uninstall')
            check(p.returncode == 0 and 'not configured' in p.stdout, platform, p.stdout)
    else:
        print('No real dotnet or compiled DocumentAtom.McpServer Release build: config files were not checked.')

# Shell syntax for every script.
for path in root.glob('scripts/**/*.sh'):
    subprocess.run(['bash', '-n', str(path)], check=True)
print(f'{count} checks passed: same-set rule, self-contained scripts, OS guards, arguments, build and '
      'server calls, install/reinstall/uninstall semantics, WSL endpoint rewrite, Windows scripts and config files.')
