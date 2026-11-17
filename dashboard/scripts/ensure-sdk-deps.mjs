// The dashboard consumes the local TypeScript SDK (file:../sdk/typescript) and
// webpack resolves the SDK's own imports (superagent, msw, ...) from the SDK's
// node_modules. npm does not install a file: dependency's dependencies, so this
// postinstall step installs the SDK's runtime dependencies when any are missing.
// It does nothing when they are already present, so an existing full SDK
// install (including dev dependencies) is left untouched.

import { spawnSync } from "child_process";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const sdkDir = path.resolve(
  path.dirname(fileURLToPath(import.meta.url)),
  "../../sdk/typescript"
);

const pkg = JSON.parse(
  fs.readFileSync(path.join(sdkDir, "package.json"), "utf8")
);

const missing = Object.keys(pkg.dependencies || {}).filter(
  (name) =>
    !fs.existsSync(path.join(sdkDir, "node_modules", name, "package.json"))
);

if (missing.length === 0) {
  process.exit(0);
}

console.log(
  `Installing SDK runtime dependencies in ${sdkDir} (missing: ${missing.join(", ")})`
);

const result = spawnSync(
  "npm",
  ["ci", "--omit=dev", "--no-audit", "--no-fund"],
  { cwd: sdkDir, stdio: "inherit", shell: process.platform === "win32" }
);

process.exit(result.status ?? 1);
