import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: false,
  transpilePackages: [
    "documentatom-sdk",
    "uuid",
    "msw",
    "@mswjs/interceptors",
    "@open-draft/deferred-promise",
    "@open-draft/logger",
    "headers-polyfill",
    "is-node-process",
    "outvariant",
    "rettime",
    "strict-event-emitter",
    "until-async",
  ],
  webpack: (config) => {
    config.resolve.alias["documentatom-sdk"] = path.resolve(
      __dirname,
      "../sdk/typescript"
    );
    return config;
  },
  // eslint: {
  //   ignoreDuringBuilds: true,
  // },
  // typescript: {
  //   ignoreBuildErrors: true,
  // },
};

export default nextConfig;
