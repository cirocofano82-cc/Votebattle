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
        display: ["Bricolage Grotesque", "system-ui", "sans-serif"],
        sans: ["Plus Jakarta Sans", "system-ui", "-apple-system", "Segoe UI", "Roboto", "sans-serif"],
      },
      borderRadius: {
        card: "22px",
      },
      boxShadow: {
        card: "0 2px 4px rgba(90,70,170,.05), 0 22px 48px -26px rgba(90,70,170,.45)",
      },
    },
  },
  plugins: [],
};
