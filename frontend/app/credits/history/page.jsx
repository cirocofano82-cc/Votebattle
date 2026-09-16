"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";

export default function CreditHistoryPage() {
  const { user, loading: authLoading } = useAuth();
  const [page, setPage] = useState(1);

  const { data } = useQuery({
    queryKey: ["credit-history", page],
    queryFn: async () => (await apiFetch(`/users/me/credits/history?page=${page}&pageSize=20`)).data,
    enabled: !!user,
    placeholderData: (prev) => prev,
  });

  if (!authLoading && !user) {
    return (
      <div className="container-page py-12 text-center">
        <p className="text-muted mb-4">Sign in to see your credit history.</p>
        <Link href="/login" className="btn btn-ink">Sign in</Link>
      </div>
    );
  }

  const items = data?.items || [];
  const totalPages = data?.totalPages || 1;

  return (
    <div className="container-page py-8 max-w-[760px] mx-auto">
      <Link href="/credits" className="text-muted text-sm no-underline hover:text-text">← Back to credits</Link>
      <h1 className="font-display text-4xl mt-2 mb-6">Credit History</h1>

      <div className="card overflow-hidden">
        <div className="grid grid-cols-[1fr_auto_auto] gap-3 px-5 py-3 text-xs font-bold uppercase tracking-wide text-muted border-b border-line">
          <span>Description</span>
          <span className="text-right">Credits</span>
          <span className="text-right">Balance</span>
        </div>
        {items.length === 0 ? (
          <p className="text-muted p-5">No credit activity yet.</p>
        ) : (
          <div className="divide-y divide-line">
            {items.map((t) => (
              <div key={t.id} className="grid grid-cols-[1fr_auto_auto] gap-3 px-5 py-3 items-center">
                <div>
                  <div className="font-medium">{t.description}</div>
                  <div className="text-muted text-xs">{new Date(t.createdAt).toLocaleString()}</div>
                </div>
                <span className={`text-right font-semibold tnum ${t.amount >= 0 ? "text-sidea" : "text-sideb"}`}>
                  {t.amount >= 0 ? "+" : ""}{t.amount}
                </span>
                <span className="text-right tnum text-muted">{t.balanceAfter}</span>
              </div>
            ))}
          </div>
        )}
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3 mt-6">
          <button className="btn btn-ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>← Prev</button>
          <span className="text-muted text-sm">Page {page} of {totalPages}</span>
          <button className="btn btn-ghost" disabled={page >= totalPages} onClick={() => setPage((p) => p + 1)}>Next →</button>
        </div>
      )}
    </div>
  );
}
