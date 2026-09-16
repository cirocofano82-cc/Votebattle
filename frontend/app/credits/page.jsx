"use client";

import { Suspense, useEffect, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { formatNumber } from "@/lib/format";

function CreditsInner() {
  const { user, loading: authLoading, refresh } = useAuth();
  const params = useSearchParams();
  const checkout = params.get("checkout");
  const [buyingId, setBuyingId] = useState(null);
  const [error, setError] = useState(null);

  useEffect(() => {
    // Returning from Stripe: refresh the balance (the webhook may have granted credits).
    if (checkout === "success") refresh();
  }, [checkout, refresh]);

  const { data: packages } = useQuery({
    queryKey: ["vote-packages"],
    queryFn: async () => (await apiFetch("/vote-packages")).data,
  });

  const { data: payments } = useQuery({
    queryKey: ["my-payments"],
    queryFn: async () => (await apiFetch("/users/me/payments?pageSize=5")).data,
    enabled: !!user,
  });

  async function buy(pkg) {
    setError(null);
    setBuyingId(pkg.id);
    try {
      const res = await apiFetch("/payments/checkout", {
        method: "POST",
        body: { votePackageId: pkg.id },
      });
      window.location.href = res.data.url;
    } catch (e) {
      setError(e.message || "Could not start checkout. Please try again.");
      setBuyingId(null);
    }
  }

  if (!authLoading && !user) {
    return (
      <div className="container-page py-12 text-center">
        <h1 className="font-display text-4xl mb-2">Vote Credits</h1>
        <p className="text-muted mb-5">Sign in to view your balance and buy credits.</p>
        <Link href="/login" className="btn btn-ink">Sign in</Link>
      </div>
    );
  }

  return (
    <div className="container-page py-8">
      {checkout === "success" && (
        <div className="card p-4 mb-6 bg-sidea-soft text-sidea">
          Payment received — your Vote Credits have been added. Thanks for backing your side!
        </div>
      )}
      {checkout === "cancel" && (
        <div className="card p-4 mb-6 text-muted">Checkout cancelled. No charge was made.</div>
      )}

      {/* Balance */}
      <div className="card p-6 mb-8 flex flex-wrap items-center justify-between gap-4"
           style={{ background: "linear-gradient(135deg, color-mix(in srgb, var(--gold-soft) 70%, var(--surface)), var(--surface))", borderColor: "color-mix(in srgb, var(--gold) 30%, transparent)" }}>
        <div>
          <div className="text-xs font-bold tracking-[0.1em] uppercase text-muted">Your Vote Credits</div>
          <div className="font-display text-5xl text-gold-ink leading-none mt-1">⚡ {formatNumber(user?.voteCredits)}</div>
        </div>
        <p className="text-muted max-w-[36ch] text-sm">
          Credits never expire. Use them across any active battle to back your side.
        </p>
      </div>

      {/* Packages */}
      <div className="text-xs font-bold tracking-[0.16em] uppercase text-muted">Top up</div>
      <h2 className="font-display text-3xl mb-4">Get More Credits</h2>
      {error && <p className="text-sideb mb-3">{error}</p>}
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-5">
        {(packages || []).map((p) => (
          <div key={p.id} className={`card p-5 flex flex-col gap-1 relative ${p.isPopular ? "!border-gold" : ""}`}
               style={p.isPopular ? { boxShadow: "0 0 0 1px var(--gold)" } : undefined}>
            {p.isPopular && (
              <span className="absolute -top-3 left-1/2 -translate-x-1/2 bg-gold text-[#1a1200] text-[10px] font-extrabold uppercase tracking-wide px-2.5 py-1 rounded-full whitespace-nowrap">
                Most Popular
              </span>
            )}
            <div className="font-semibold text-sm">{p.name}</div>
            <div className="font-display text-3xl leading-none">${p.price}</div>
            <div className="text-muted text-sm">{p.credits} Vote Credits</div>
            <button
              className={`btn mt-2 justify-center ${p.isPopular ? "btn-gold" : "btn-ghost"}`}
              disabled={buyingId === p.id}
              onClick={() => buy(p)}
            >
              {buyingId === p.id ? "Starting…" : "Buy"}
            </button>
          </div>
        ))}
      </div>

      {/* Purchase history */}
      <div className="flex items-end justify-between mt-10 mb-3">
        <h2 className="font-display text-2xl">Purchase History</h2>
        <Link href="/credits/history" className="btn btn-ghost">Credit history →</Link>
      </div>
      <div className="card divide-y divide-line">
        {(payments?.items || []).length === 0 ? (
          <p className="text-muted p-5">No purchases yet.</p>
        ) : (
          payments.items.map((p) => (
            <div key={p.id} className="flex items-center justify-between px-5 py-3">
              <div>
                <div className="font-semibold">{p.packageName}</div>
                <div className="text-muted text-sm">{new Date(p.createdAt).toLocaleDateString()} · {p.status}</div>
              </div>
              <div className="text-right">
                <div className="font-semibold tnum">${p.amount}</div>
                <div className="text-muted text-sm tnum">+{p.voteCreditsPurchased} credits</div>
              </div>
            </div>
          ))
        )}
      </div>
    </div>
  );
}

export default function CreditsPage() {
  return (
    <Suspense fallback={<div className="container-page py-12 text-muted">Loading…</div>}>
      <CreditsInner />
    </Suspense>
  );
}
