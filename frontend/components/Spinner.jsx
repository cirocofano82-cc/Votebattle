// Small accessible loading spinner using the design tokens.
export default function Spinner({ size = 28, className = "" }) {
  return (
    <span
      role="status"
      aria-label="Loading"
      className={`inline-block animate-spin rounded-full border-2 border-line border-t-ink ${className}`}
      style={{ width: size, height: size }}
    />
  );
}
