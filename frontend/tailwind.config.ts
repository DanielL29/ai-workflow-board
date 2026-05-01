import type { Config } from "tailwindcss";

const config: Config = {
  content: [
    "./src/app/**/*.{ts,tsx}",
    "./src/components/**/*.{ts,tsx}",
    "./src/lib/**/*.{ts,tsx}",
    "./src/store/**/*.{ts,tsx}",
  ],
  theme: {
    extend: {
      colors: {
        sand: "#f4efe4",
        ink: "#1f1e1a",
        teal: "#0f766e",
        coral: "#ef7c57",
        mustard: "#f1c95a",
        slateblue: "#335c67",
      },
      fontFamily: {
        sans: ["'Sora'", "ui-sans-serif", "system-ui", "sans-serif"],
      },
      boxShadow: {
        card: "0 18px 60px rgba(28, 31, 35, 0.10)",
      },
    },
  },
  plugins: [],
};

export default config;
