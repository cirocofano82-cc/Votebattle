"use client";

import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import { formatNumber } from "@/lib/format";

export default function Navbar() {
  const { user, loading, logout } = useAuth();

  return (
    <header className="sticky top-0 z-50 border-b border-line bg-surface/90 backdrop-blur">
      <div className="container-page flex items-center gap-4 h-[60px]">
        <Link href="/" className="font-display text-2xl tracking-wide no-underline text-text">
          Vote<span className="text-sidea">Bat</span><span className="text-sideb">tle</span>
        </Link>

        <nav className="hidden sm:flex gap-6 text-sm font-medium text-muted ml-2">
          <Link href="/battles" className="no-underline hover:text-text">Battles</Link>
          {user?.roles?.includes("Admin") && (
            <Link href="/create-battle" className="no-underline hover:text-text">Create</Link>
          )}
          <Link href="/credits" className="no-underline hover:text-text">Get Credits</Link>
        </nav>

        <div className="flex-1" />

        {!loading && user && (
          <>
            <Link
              href="/credits"
              className="chip !text-gold-ink !bg-gold-soft no-underline"
              title="Your Vote Credits"
            >
              ⚡ {formatNumber(user.voteCredits)} Credits
            </Link>
            {user.roles?.includes("Admin") && (
              <Link href="/admin" className="hidden sm:inline btn btn-ghost">Admin</Link>
            )}
            <Link href="/profile" className="btn btn-ghost">{user.username}</Link>
            <button className="btn btn-ink" onClick={() => logout()}>Sign out</button>
          </>
        )}

        {!loading && !user && (
          <>
            <Link href="/login" className="btn btn-ghost">Sign in</Link>
            <Link href="/register" className="btn btn-ink">Join</Link>
          </>
        )}
      </div>
    </header>
  );
}
