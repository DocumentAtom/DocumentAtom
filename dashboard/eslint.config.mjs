import nextCoreWebVitals from "eslint-config-next/core-web-vitals";
import nextTypescript from "eslint-config-next/typescript";

const config = [
  ...nextCoreWebVitals,
  ...nextTypescript,
  {
    ignores: [
      ".next/**",
      "out/**",
      "build/**",
      "coverage/**",
      "next-env.d.ts",
      "src/tests/**",
    ],
  },
  {
    rules: {
      "no-debugger": 1,
      "no-use-before-define": 0,
      semi: 2,
      "max-len": [
        2,
        {
          code: 200,
          ignoreStrings: true,
          ignoreTemplateLiterals: true,
        },
      ],
      "no-duplicate-imports": [2, { includeExports: true }],
      "max-lines-per-function": [2, { max: 120 }],
      "no-console": 1,
      "no-invalid-this": 0,
      "no-unused-vars": 0,
      "react/jsx-filename-extension": [
        1,
        { extensions: [".js", ".jsx", ".ts", ".tsx"] },
      ],
      "react/prop-types": 1,
      "@typescript-eslint/no-unused-vars": 0,
      "@typescript-eslint/no-use-before-define": 0,
      "@typescript-eslint/no-explicit-any": 0,
      "@typescript-eslint/explicit-function-return-type": 0,
      "@typescript-eslint/explicit-member-accessibility": 0,
      "react/react-in-jsx-scope": 0,
      "jsx-a11y/anchor-is-valid": 0,
      // Shows typedef errors in the editor, so we control where explicit types are required.
      "@typescript-eslint/typedef": [
        "error",
        {
          arrayDestructuring: false,
          arrowParameter: true,
          memberVariableDeclaration: true,
          objectDestructuring: false,
          parameter: true,
          propertyDeclaration: true,
          variableDeclaration: false,
          variableDeclarationIgnoreFunction: true,
        },
      ],
      "@typescript-eslint/naming-convention": [
        1,
        { selector: "method", format: ["camelCase", "PascalCase"] },
        { selector: "class", format: ["PascalCase"] },
        {
          selector: "property",
          format: ["camelCase", "UPPER_CASE", "snake_case", "PascalCase"],
        },
        { selector: "enumMember", format: ["PascalCase", "UPPER_CASE"] },
        { selector: "enum", format: ["PascalCase"] },
        { selector: "interface", format: ["PascalCase"] },
        {
          selector: "variable",
          format: ["PascalCase", "camelCase", "UPPER_CASE"],
        },
      ],
    },
  },
];

export default config;
