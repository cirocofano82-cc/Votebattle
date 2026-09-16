"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";
import { timeAgo } from "@/lib/format";

const STATUSES = ["Pending", "Reviewed", "Actioned", "Dismissed"];

export default function AdminReports() {
  const [status, setStatus] = useState("Pending");
  const [busy, setBusy] = useState(null);

  const { data, refetch, isLoading } = useQuery({
    queryKey: ["admin-reports", status],
    queryFn: async () => (await apiFetch(`/admin/reports?status=${status}&pageSize=50`)).data,
  });

  async function resolve(id, newStatus) {
    setBusy(id + newStatus);
    try {
      await apiFetch(`/admin/reports/${id}/resolve`, { method: "POST", body: { status: newStatus } });
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
        <p className="text-muted">No reports with this status.</p>
      ) : (
        <div className="card divide-y divide-line">
          {items.map((r) => (
            <div key={r.id} className="flex items-center justify-between gap-3 px-4 py-3 flex-wrap">
              <div className="min-w-0">
                <div className="font-semibold">{r.targetType} · {r.reason}</div>
                <div className="text-muted text-sm">by {r.reporterUsername} · {timeAgo(r.createdAt)}{r.details ? ` · ${r.details}` : ""}</div>
              </div>
              {status === "Pending" && (
                <div className="flex gap-2">
                  <button className="btn btn-a" disabled={busy === r.id + "Actioned"} onClick={() => resolve(r.id, "Actioned")}>Action</button>
                  <button className="btn btn-ghost" disabled={busy === r.id + "Dismissed"} onClick={() => resolve(r.id, "Dismissed")}>Dismiss</button>
                </div>
              )}
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
