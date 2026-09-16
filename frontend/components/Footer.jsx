import Link from "next/link";

export default function Footer() {
  return (
    <footer className="border-t border-line text-muted text-sm mt-10">
      <div className="container-page flex flex-wrap items-center gap-5 py-6">
        <span className="font-display text-xl text-text">
          Vote<span className="text-sidea">Bat</span><span className="text-sideb">tle</span>
        </span>
        <div className="flex-1" />
        <Link href="/terms" className="no-underline hover:text-text">Terms</Link>
        <Link href="/privacy" className="no-underline hover:text-text">Privacy</Link>
        <Link href="/refunds" className="no-underline hover:text-text">Refunds</Link>
        <Link href="/cookies" className="no-underline hover:text-text">Cookies</Link>
        <Link href="/community-guidelines" className="no-underline hover:text-text">Guidelines</Link>
      </div>
    </footer>
  );
}
