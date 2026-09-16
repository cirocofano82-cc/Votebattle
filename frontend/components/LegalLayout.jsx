export default function LegalLayout({ title, children }) {
  return (
    <div className="container-page py-10 max-w-[760px] mx-auto">
      <div className="card p-4 mb-6 text-sm text-muted" style={{ borderLeft: "4px solid var(--gold)" }}>
        ⚠️ <strong className="text-text">Placeholder document.</strong> This is a draft for the MVP and
        is <strong className="text-text">not legal advice</strong>. It must be reviewed and completed by a
        qualified legal professional before launch.
      </div>
      <h1 className="font-display text-4xl mb-6">{title}</h1>
      <div className="flex flex-col gap-5 leading-relaxed text-[15px]">{children}</div>
    </div>
  );
}

export function Section({ heading, children }) {
  return (
    <section>
      <h2 className="font-display text-xl mb-1">{heading}</h2>
      <p className="text-muted">{children}</p>
    </section>
  );
}
