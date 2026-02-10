import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));

/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: false,
  transpilePackages: ["documentatom-sdk"],
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
