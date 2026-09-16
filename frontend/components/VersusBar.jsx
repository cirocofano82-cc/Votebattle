export default function VersusBar({ pa = 0 }) {
  const a = Math.max(0, Math.min(100, pa));
  const b = 100 - a;
  return (
    <div className="vs-bar" role="img" aria-label={`${a.toFixed(1)}% versus ${b.toFixed(1)}%`}>
      <div className="fa" style={{ width: `${a}%` }} />
      <div className="fb" style={{ width: `${b}%` }} />
    </div>
  );
}
