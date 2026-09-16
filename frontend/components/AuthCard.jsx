export default function AuthCard({ title, subtitle, children, footer }) {
  return (
    <div className="container-page py-12">
      <div className="max-w-[420px] mx-auto card p-6">
        <h1 className="font-display text-3xl mb-1">{title}</h1>
        {subtitle && <p className="text-muted mb-5">{subtitle}</p>}
        {children}
        {footer && <div className="text-sm text-muted mt-5 text-center">{footer}</div>}
      </div>
    </div>
  );
}
