import eslint from "@eslint/js";
import prettier from "eslint-config-prettier";
import { defineConfig } from "eslint/config";
import tseslint from "typescript-eslint";

const noNetwork =
  "Player scripts make no network calls (CODING_STANDARDS.md, section 11).";

export default defineConfig(
  { ignores: ["dist/"] },
  eslint.configs.recommended,
  tseslint.configs.strictTypeChecked,
  tseslint.configs.stylisticTypeChecked,
  {
    languageOptions: {
      parserOptions: {
        projectService: true,
        tsconfigRootDir: import.meta.dirname,
      },
    },
    rules: {
      "no-eval": "error",
      "no-new-func": "error",
      "no-restricted-globals": [
        "error",
        { name: "fetch", message: noNetwork },
        { name: "XMLHttpRequest", message: noNetwork },
        { name: "WebSocket", message: noNetwork },
        { name: "EventSource", message: noNetwork },
      ],
    },
  },
  {
    files: ["**/*.mjs"],
    extends: [tseslint.configs.disableTypeChecked],
  },
  prettier,
);
