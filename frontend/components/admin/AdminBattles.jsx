"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";

const STATUSES = ["PendingModeration", "Active", "Rejected", "Suspended", "Ended"];

export default function AdminBattles() {
  const [status, setStatus] = useState("PendingModeration");
  const [busy, setBusy] = useState(null);

  const { data, refetch, isLoading } = useQuery({
    queryKey: ["admin-battles", status],
    queryFn: async () => (await apiFetch(`/admin/battles?status=${status}&pageSize=50`)).data,
  });

  async function moderate(id, action) {
    setBusy(id + action);
    try {
      await apiFetch(`/admin/battles/${id}/moderate`, { method: "POST", body: { action } });
      await refetch();
    } finally {
      setBusy(null);
    }
  }

  const items = data?.items || [];

  return (
    <div>
      <div className="flex gap-2 flex-wrap mb-4">
        {STATUSES.map((s) => (
          <button key={s} className={`chip ${status === s ? "!bg-ink !text-onink" : ""}`} onClick={() => setStatus(s)}>{s}</button>
        ))}
      </div>

      {isLoading ? <p className="text-muted">Loading…</p> : items.length === 0 ? (
        <p className="text-muted">No battles with this status.</p>
      ) : (
        <div className="card divide-y divide-line">
          {items.map((b) => (
            <div key={b.id} className="flex items-center justify-between gap-3 px-4 py-3 flex-wrap">
              <div className="min-w-0">
                <div className="font-semibold truncate">{b.title}</div>
                <div className="text-muted text-sm">{b.categoryName} · by {b.createdByUsername} · {b.status}</div>
              </div>
              <div className="flex gap-2">
                {b.status !== "Active" && (
                  <button className="btn btn-a" disabled={busy === b.id + "Approve"} onClick={() => moderate(b.id, "Approve")}>Approve</button>
                )}
                {b.status !== "Rejected" && (
                  <button className="btn btn-ghost" disabled={busy === b.id + "Reject"} onClick={() => moderate(b.id, "Reject")}>Reject</button>
                )}
                {b.status === "Active" && (
                  <button className="btn btn-b" disabled={busy === b.id + "Suspend"} onClick={() => moderate(b.id, "Suspend")}>Suspend</button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
