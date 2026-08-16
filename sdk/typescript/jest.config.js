// Add any custom config to be passed to Jest
const config = {
  transform: {
    "^.+\\.[cm]?[jt]sx?$": "babel-jest",
  },
  transformIgnorePatterns: [
    "node_modules/(?!(msw|@mswjs|@open-draft|headers-polyfill|outvariant|strict-event-emitter|is-node-process|rettime|until-async)/)",
  ],
  coverageProvider: "v8",
  moduleFileExtensions: ["js", "mjs", "cjs", "ts", "json"],
  collectCoverageFrom: [
    "src/**/*.ts",
    "!src/**/*.d.ts",
    "!**/mocks/**",
    "!**/lib/**",
    "!**/data/**",
    "!**/tests/**",
    "!**/src/types.ts/**",
  ],
};

module.exports = config;
