export default function VersusBar({ pa = 0, empty = false }) {
  // No votes yet: show a neutral, empty track instead of a full side-B bar.
  if (empty) {
    return <div className="vs-bar" role="img" aria-label="No votes yet" />;
  }
  const a = Math.max(0, Math.min(100, pa));
  const b = 100 - a;
  return (
    <div className="vs-bar" role="img" aria-label={`${a.toFixed(1)}% versus ${b.toFixed(1)}%`}>
      <div className="fa" style={{ width: `${a}%` }} />
      <div className="fb" style={{ width: `${b}%` }} />
    </div>
  );
}
