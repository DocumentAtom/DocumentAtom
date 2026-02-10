// Add any custom config to be passed to Jest
const config = {
  transform: {
    '^.+\\.tsx?$': 'babel-jest', // 👈 allows TypeScript files
    '^.+\\.jsx?$': 'babel-jest', // 👈 allows JS/JSX files
  },
  coverageProvider: 'v8',
  moduleFileExtensions: ['js', 'ts', 'json'],
  collectCoverageFrom: [
    'src/**/*.ts',
    '!src/**/*.d.ts',
    '!**/mocks/**',
    '!**/lib/**',
    '!**/data/**',
    '!**/tests/**',
    '!**/src/types.ts/**',
  ],
};

// createJestConfig is exported this way to ensure that next/jest can load the Next.js config which is async
module.exports = config;
