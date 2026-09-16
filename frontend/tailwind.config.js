/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./app/**/*.{js,jsx}",
    "./components/**/*.{js,jsx}",
  ],
  theme: {
    extend: {
      colors: {
        bg: "var(--bg)",
        surface: "var(--surface)",
        surface2: "var(--surface-2)",
        line: "var(--line)",
        "line-strong": "var(--line-strong)",
        muted: "var(--muted)",
        text: "var(--text)",
        ink: "var(--ink)",
        onink: "var(--on-ink)",
        sidea: { DEFAULT: "var(--a)", soft: "var(--a-soft)" },
        sideb: { DEFAULT: "var(--b)", soft: "var(--b-soft)" },
        gold: { DEFAULT: "var(--gold)", soft: "var(--gold-soft)", ink: "var(--gold-ink)" },
      },
      fontFamily: {
        display: ["Anton", "Arial Narrow", "system-ui", "sans-serif"],
        sans: ["Instrument Sans", "system-ui", "-apple-system", "Segoe UI", "Roboto", "sans-serif"],
      },
      borderRadius: {
        card: "16px",
      },
      boxShadow: {
        card: "0 1px 2px rgba(16,22,40,.04), 0 8px 24px -12px rgba(16,22,40,.18)",
      },
    },
  },
  plugins: [],
};
